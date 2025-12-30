using Spectre.Console;
using System.Globalization;
using System.Text;
using System.Text.Json;
using UnitSimulator;
using UnitSimulator.GoogleSheets;
using UnitSimulator.Core.Pathfinding;

namespace UnitDevTool;
internal static class Program
{
    private sealed class GoogleSheetsConfig
    {
        public string CredentialsPath { get; set; } = string.Empty;
        public List<SheetDocumentConfig> Documents { get; set; } = new();
    }

    private sealed class SheetDocumentConfig
    {
        public string Name { get; set; } = string.Empty;
        public string SpreadsheetId { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = "./DataSheets";
    }

    private sealed class AppConfig
    {
        public GoogleSheetsConfig GoogleSheets { get; set; } = new();
    }

    private static void Main(string[] args)
    {
        AnsiConsole.MarkupLine("[green]Unit Dev Tool[/] - 기본 TUI 스켈레톤");
        AnsiConsole.MarkupLine("[grey]데이터 시트 다운로드 기능부터 구현합니다.[/]\n");

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]메뉴를 선택하세요[/]")
                    .AddChoices("1. Data Sheet Download", "2. Pathfinding Report", "3. Pathfinding SVG", "0. 종료"));

            switch (choice)
            {
                case "1. Data Sheet Download":
                    RunDataSheetDownload();
                    break;
                case "2. Pathfinding Report":
                    RunPathfindingReport();
                    break;
                case "3. Pathfinding SVG":
                    RunPathfindingSvg();
                    break;
                case "0. 종료":
                    return;
            }
        }
    }

    private static void RunPathfindingReport()
    {
        try
        {
            var settings = new PathfindingTestSettings
            {
                Seed = 1234,
                ObstacleDensity = 0.15f,
                ScenarioCount = 25
            };

            var runner = new PathfindingTestRunner();
            var report = runner.Run(settings);

            const string outputDir = "output";
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, "pathfinding-report.json");
            report.SaveToJson(outputPath);

            AnsiConsole.MarkupLine("[green]Pathfinding report saved.[/]");
            AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(outputPath)}");
            AnsiConsole.MarkupLine($"[grey]Success rate:[/] {report.Summary.SuccessRate:P1}");
            AnsiConsole.MarkupLine($"[grey]Average path length:[/] {report.Summary.AveragePathLength:F1}");
            AnsiConsole.MarkupLine($"[grey]Average node count:[/] {report.Summary.AverageNodeCount:F1}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]오류 발생:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private static void RunPathfindingSvg()
    {
        try
        {
            var reportPath = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Report path[/]")
                    .DefaultValue(Path.Combine("output", "pathfinding-report.json")));

            if (!File.Exists(reportPath))
            {
                AnsiConsole.MarkupLine($"[red]리포트 파일을 찾을 수 없습니다:[/] {Markup.Escape(reportPath)}");
                return;
            }

            var report = LoadPathfindingReport(reportPath);
            if (report == null || report.Results.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]리포트를 읽을 수 없거나 시나리오가 없습니다.[/]");
                return;
            }

            int scenarioIndex = AnsiConsole.Prompt(
                new TextPrompt<int>("[yellow]Scenario index[/]")
                    .DefaultValue(0)
                    .ValidationErrorMessage("유효한 인덱스를 입력하세요.")
                    .Validate(i => i >= 0 && i < report.Results.Count));

            var result = report.Results[scenarioIndex];
            var svg = BuildSvg(report, result);

            var outputDir = Path.GetDirectoryName(reportPath);
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = ".";
            }

            var outputPath = Path.Combine(outputDir, $"pathfinding-scenario-{scenarioIndex}.svg");
            File.WriteAllText(outputPath, svg);

            AnsiConsole.MarkupLine("[green]SVG generated.[/]");
            AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(outputPath)}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]오류 발생:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private static PathfindingTestReport? LoadPathfindingReport(string reportPath)
    {
        var json = File.ReadAllText(reportPath);
        return JsonSerializer.Deserialize<PathfindingTestReport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private static string BuildSvg(PathfindingTestReport report, PathfindingTestResult result)
    {
        float nodeSize = report.Settings.NodeSize;
        int gridWidth = (int)MathF.Ceiling(report.Settings.MapWidth / nodeSize);
        int gridHeight = (int)MathF.Ceiling(report.Settings.MapHeight / nodeSize);
        const int cellSize = 8;

        int widthPx = gridWidth * cellSize;
        int heightPx = gridHeight * cellSize;

        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{widthPx}\" height=\"{heightPx}\" viewBox=\"0 0 {widthPx} {heightPx}\">");
        sb.AppendLine("<rect width=\"100%\" height=\"100%\" fill=\"#0b1220\"/>");

        foreach (var obstacle in report.Obstacles)
        {
            int rectWidth = (obstacle.MaxX - obstacle.MinX + 1) * cellSize;
            int rectHeight = (obstacle.MaxY - obstacle.MinY + 1) * cellSize;
            int x = obstacle.MinX * cellSize;
            int y = (gridHeight - (obstacle.MaxY + 1)) * cellSize;
            sb.AppendLine($"<rect x=\"{x}\" y=\"{y}\" width=\"{rectWidth}\" height=\"{rectHeight}\" fill=\"#1f2937\"/>");
        }

        var pathPoints = result.Path;
        if (pathPoints != null && pathPoints.Count > 0)
        {
            var points = new StringBuilder();
            foreach (var point in pathPoints)
            {
                float gridX = point.X / nodeSize;
                float gridY = point.Y / nodeSize;
                float svgX = gridX * cellSize;
                float svgY = (gridHeight - gridY) * cellSize;
                points.Append($"{F(svgX)},{F(svgY)} ");
            }
            sb.AppendLine($"<polyline points=\"{points.ToString().Trim()}\" fill=\"none\" stroke=\"#22d3ee\" stroke-width=\"2\"/>");
        }

        DrawMarker(sb, result.Start, nodeSize, gridHeight, cellSize, "#22c55e", 0.6f);
        DrawMarker(sb, result.End, nodeSize, gridHeight, cellSize, "#f97316", 0.6f);

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static void DrawMarker(StringBuilder sb, SerializableVector2 point, float nodeSize, int gridHeight, int cellSize, string color, float radiusScale)
    {
        float gridX = point.X / nodeSize;
        float gridY = point.Y / nodeSize;
        float svgX = gridX * cellSize;
        float svgY = (gridHeight - gridY) * cellSize;
        float radius = cellSize * radiusScale;
        sb.AppendLine($"<circle cx=\"{F(svgX)}\" cy=\"{F(svgY)}\" r=\"{F(radius)}\" fill=\"{color}\"/>");
    }

    private static string F(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void RunDataSheetDownload()
    {
        try
        {
            var config = LoadConfig();
            if (config.GoogleSheets.Documents.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]다운로드할 Google 문서가 appsettings.json에 정의되어 있지 않습니다.[/]");
                return;
            }

            var doc = AnsiConsole.Prompt(
                new SelectionPrompt<SheetDocumentConfig>()
                    .Title("[yellow]다운로드할 Google Sheet 문서를 선택하세요[/]")
                    .UseConverter(d => string.IsNullOrWhiteSpace(d.Name) ? d.SpreadsheetId : d.Name)
                    .AddChoices(config.GoogleSheets.Documents));

            var outputDir = doc.OutputDirectory;
            var credentialsPath = Path.GetFullPath(config.GoogleSheets.CredentialsPath);

            AnsiConsole.Status()
                .Spinner(Spinner.Known.BouncingBar)
                .SpinnerStyle(new Style(Color.Green))
                .Start("Google Sheets에서 데이터 시트를 다운로드 중...", ctx =>
                {
                    return DownloadSheetsAsync(doc.SpreadsheetId, outputDir, credentialsPath, status => ctx.Status = status);
                })
                .GetAwaiter().GetResult();

            AnsiConsole.MarkupLine("[green]데이터 시트 다운로드 및 XML 저장이 완료되었습니다.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]오류 발생:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private static async Task DownloadSheetsAsync(string spreadsheetId, string outputDirectory, string credentialsPath, Action<string>? statusReporter)
    {
        statusReporter?.Invoke("크레덴셜 검증 중...");
        if (!File.Exists(credentialsPath))
        {
            throw new FileNotFoundException($"Credentials file not found: {credentialsPath}");
        }

        statusReporter?.Invoke("Google Sheets API에 연결 중...");
        using var service = new GoogleSheetsService(credentialsPath);

        statusReporter?.Invoke("시트 메타데이터 조회 중...");
        var spreadsheet = await service.GetSpreadsheetAsync(spreadsheetId);
        var title = spreadsheet.Properties?.Title ?? spreadsheetId;

        statusReporter?.Invoke($"'{title}' 문서의 시트 데이터 다운로드 중...");
        var sheets = await service.GetAllSheetsDataAsync(spreadsheetId);

        statusReporter?.Invoke("XML 파일로 저장 중...");
        var savedPaths = XmlConverter.SaveAllToXmlFiles(sheets, outputDirectory);

        statusReporter?.Invoke($"총 {savedPaths.Count}개의 시트를 XML로 저장했습니다.");
    }

    private static AppConfig LoadConfig()
    {
        const string configFile = "appsettings.json";
        if (!File.Exists(configFile))
        {
            throw new FileNotFoundException($"환경 설정 파일을 찾을 수 없습니다: {configFile}");
        }

        var json = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null)
        {
            throw new InvalidOperationException("환경 설정을 로드할 수 없습니다.");
        }

        return config;
    }
}
