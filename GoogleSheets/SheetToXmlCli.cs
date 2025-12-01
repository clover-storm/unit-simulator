namespace UnitSimulator.GoogleSheets;

/// <summary>
/// Google Sheets to XML 변환 CLI 인터페이스
/// </summary>
public static class SheetToXmlCli
{
    private const string DefaultOutputDir = "xml_output";
    private const string DefaultCredentialsFile = "credentials.json";

    /// <summary>
    /// CLI 명령을 실행합니다.
    /// </summary>
    /// <param name="args">명령줄 인수</param>
    /// <returns>종료 코드 (0: 성공, 1: 실패)</returns>
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        string? spreadsheetId = null;
        string? outputDirectory = null;
        string? credentialsPath = null;

        // 인수 파싱
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--sheet-id" or "-s":
                    if (i + 1 < args.Length)
                        spreadsheetId = args[++i];
                    break;
                case "--output" or "-o":
                    if (i + 1 < args.Length)
                        outputDirectory = args[++i];
                    break;
                case "--credentials" or "-c":
                    if (i + 1 < args.Length)
                        credentialsPath = args[++i];
                    break;
            }
        }

        // 필수 인수 확인
        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            Console.Error.WriteLine("Error: Spreadsheet ID is required. Use --sheet-id or -s option.");
            PrintUsage();
            return 1;
        }

        // 기본값 설정
        outputDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), DefaultOutputDir);
        credentialsPath ??= Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
                           ?? Path.Combine(Directory.GetCurrentDirectory(), DefaultCredentialsFile);

        try
        {
            await ConvertSheetToXmlAsync(spreadsheetId, outputDirectory, credentialsPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Google Sheets를 XML로 변환하고 저장합니다.
    /// </summary>
    private static async Task ConvertSheetToXmlAsync(string spreadsheetId, string outputDirectory, string credentialsPath)
    {
        Console.WriteLine("=== Google Sheets to XML Converter ===");
        Console.WriteLine($"Spreadsheet ID: {spreadsheetId}");
        Console.WriteLine($"Output Directory: {outputDirectory}");
        Console.WriteLine($"Credentials Path: {credentialsPath}");
        Console.WriteLine();

        // 자격 증명 파일 확인
        if (!File.Exists(credentialsPath))
        {
            throw new FileNotFoundException($"Credentials file not found: {credentialsPath}\n" +
                "Please provide a valid service account JSON file or set GOOGLE_APPLICATION_CREDENTIALS environment variable.");
        }

        Console.WriteLine("Connecting to Google Sheets API...");
        using var sheetsService = new GoogleSheetsService(credentialsPath);

        Console.WriteLine("Fetching spreadsheet information...");
        var spreadsheet = await sheetsService.GetSpreadsheetAsync(spreadsheetId);
        Console.WriteLine($"Spreadsheet Title: {spreadsheet.Properties.Title}");
        Console.WriteLine($"Total Sheets: {spreadsheet.Sheets.Count}");
        Console.WriteLine();

        Console.WriteLine("Fetching all sheets data...");
        var allSheetsData = await sheetsService.GetAllSheetsDataAsync(spreadsheetId);

        Console.WriteLine("Converting to XML and saving files...");
        var savedPaths = XmlConverter.SaveAllToXmlFiles(allSheetsData, outputDirectory);

        Console.WriteLine();
        Console.WriteLine("=== Conversion Complete ===");
        Console.WriteLine($"Total files saved: {savedPaths.Count}");
        foreach (var path in savedPaths)
        {
            Console.WriteLine($"  - {path}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine(@"
Google Sheets to XML Converter
==============================

Usage:
  UnitMove sheet-to-xml --sheet-id <SPREADSHEET_ID> [options]

Required:
  --sheet-id, -s <ID>       Google Spreadsheet ID (found in the URL)

Options:
  --output, -o <DIR>        Output directory for XML files (default: ./xml_output)
  --credentials, -c <PATH>  Path to Google service account credentials JSON file
                            (default: GOOGLE_APPLICATION_CREDENTIALS env var or ./credentials.json)
  --help, -h                Show this help message

Examples:
  UnitMove sheet-to-xml -s 1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms -o ./output
  UnitMove sheet-to-xml --sheet-id 1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms --credentials ./my-credentials.json

Environment Variables:
  GOOGLE_APPLICATION_CREDENTIALS  Path to the service account credentials file

Note:
  To use this tool, you need:
  1. A Google Cloud project with Sheets API enabled
  2. A service account with access to the spreadsheet
  3. The service account's JSON key file
");
    }
}
