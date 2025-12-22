using System.Numerics;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;

namespace UnitSimulator;

/// <summary>
/// Renders simulation frames to images.
/// Can work with either FrameData or direct unit lists.
/// </summary>
public class Renderer
{
    private Font? _font;
    private Font? _labelFont;
    private static readonly JsonSerializerOptions DebugJsonOptions = new() { WriteIndented = true };
    private readonly string _outputDirectory;

    public Renderer(string? outputDirectory = null)
    {
        _outputDirectory = outputDirectory ?? ServerConstants.OUTPUT_DIRECTORY;
        InitializeFont();
    }

    /// <summary>
    /// Generates a frame image from FrameData.
    /// </summary>
    public void GenerateFrame(FrameData frameData, Vector2 mainTarget)
    {
        try
        {
            using var image = new Image<Rgba32>(ServerConstants.IMAGE_WIDTH, ServerConstants.IMAGE_HEIGHT);
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.DarkSlateGray);
                DrawUI(ctx, frameData);
                DrawMainTarget(ctx, mainTarget);
                DrawUnitsFromFrameData(ctx, frameData);
            });

            string filePath = System.IO.Path.Combine(_outputDirectory, $"frame_{frameData.FrameNumber:D4}.png");
            image.Save(filePath);

            WriteFrameDebugInfo(frameData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating frame {frameData.FrameNumber}: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a frame image from direct unit lists (legacy support).
    /// </summary>
    public void GenerateFrame(int frameNumber, List<Unit> friendlies, List<Unit> enemies, Vector2 mainTarget, int currentWave)
    {
        try
        {
            using var image = new Image<Rgba32>(ServerConstants.IMAGE_WIDTH, ServerConstants.IMAGE_HEIGHT);
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.DarkSlateGray);
                DrawUILegacy(ctx, frameNumber, currentWave, enemies);
                DrawMainTarget(ctx, mainTarget);
                DrawUnits(ctx, friendlies, enemies);
            });

            string filePath = System.IO.Path.Combine(_outputDirectory, $"frame_{frameNumber:D4}.png");
            image.Save(filePath);

            WriteFrameDebugInfoLegacy(frameNumber, friendlies, enemies, currentWave);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating frame {frameNumber}: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensures the output directory exists.
    /// </summary>
    public void SetupOutputDirectory(bool clearExisting = true)
    {
        try
        {
            var debugDirPath = System.IO.Path.Combine(_outputDirectory, ServerConstants.DEBUG_SUBDIRECTORY);

            var dirInfo = new DirectoryInfo(_outputDirectory);
            if (clearExisting && dirInfo.Exists)
            {
                dirInfo.Delete(true);
            }
            dirInfo.Create();
            Directory.CreateDirectory(debugDirPath);

            Console.WriteLine($"Output directory setup: {_outputDirectory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up output directory: {ex.Message}");
            throw;
        }
    }

    private void DrawUI(IImageProcessingContext ctx, FrameData frameData)
    {
        if (_font != null)
        {
            var textOptions = new RichTextOptions(_font) { Origin = new PointF(10, 10) };
            ctx.DrawText(textOptions,
                $"Frame: {frameData.FrameNumber:D4} | Wave: {frameData.CurrentWave} | Enemies Remaining: {frameData.LivingEnemyCount}",
                Color.White);
        }
    }

    private void DrawUILegacy(IImageProcessingContext ctx, int frameNumber, int currentWave, List<Unit> enemies)
    {
        if (_font != null)
        {
            var textOptions = new RichTextOptions(_font) { Origin = new PointF(10, 10) };
            ctx.DrawText(textOptions,
                $"Frame: {frameNumber:D4} | Wave: {currentWave} | Enemies Remaining: {enemies.Count(e => !e.IsDead)}",
                Color.White);
        }
    }

    private void WriteFrameDebugInfo(FrameData frameData)
    {
        try
        {
            var debugDir = System.IO.Path.Combine(_outputDirectory, ServerConstants.DEBUG_SUBDIRECTORY);
            Directory.CreateDirectory(debugDir);
            var debugPath = System.IO.Path.Combine(debugDir, $"frame_{frameData.FrameNumber:D4}.json");
            var payload = frameData.ToJson();
            File.WriteAllText(debugPath, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write debug info for frame {frameData.FrameNumber}: {ex.Message}");
        }
    }

    private void WriteFrameDebugInfoLegacy(int frameNumber, List<Unit> friendlies, List<Unit> enemies, int currentWave)
    {
        try
        {
            var info = new FrameDebugInfo(
                frameNumber,
                currentWave,
                friendlies.Count(f => !f.IsDead),
                enemies.Count(e => !e.IsDead),
                friendlies.Select(CreateUnitDebug).ToList(),
                enemies.Select(CreateUnitDebug).ToList());

            var debugDir = System.IO.Path.Combine(_outputDirectory, ServerConstants.DEBUG_SUBDIRECTORY);
            Directory.CreateDirectory(debugDir);
            var debugPath = System.IO.Path.Combine(debugDir, $"frame_{frameNumber:D4}.json");
            var payload = JsonSerializer.Serialize(info, DebugJsonOptions);
            File.WriteAllText(debugPath, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write debug info for frame {frameNumber}: {ex.Message}");
        }
    }

    private static UnitDebugInfo CreateUnitDebug(Unit unit)
    {
        var targetLabel = unit.Target is { IsDead: false } target ? target.Label : null;
        bool isMoving = unit.Velocity.LengthSquared() > 0.01f;
        bool inRange = unit.Target != null && !unit.Target.IsDead && Vector2.Distance(unit.Position, unit.Target.Position) <= unit.AttackRange;
        Vec2? avoidance = unit.HasAvoidanceTarget && unit.AvoidanceTarget != Vector2.Zero
            ? new Vec2(unit.AvoidanceTarget.X, unit.AvoidanceTarget.Y)
            : null;

        return new UnitDebugInfo(
            unit.Label,
            unit.Id,
            unit.Role.ToString(),
            unit.Faction.ToString(),
            unit.IsDead,
            unit.HP,
            unit.AttackCooldown,
            unit.TakenSlotIndex,
            targetLabel,
            isMoving,
            inRange,
            new Vec2(unit.Position.X, unit.Position.Y),
            new Vec2(unit.CurrentDestination.X, unit.CurrentDestination.Y),
            avoidance);
    }

    private record FrameDebugInfo(int Frame, int Wave, int LivingFriendlies, int LivingEnemies, List<UnitDebugInfo> Friendlies, List<UnitDebugInfo> Enemies);

    private record UnitDebugInfo(
        string Label,
        int Id,
        string Role,
        string Faction,
        bool Dead,
        int HP,
        float AttackCooldown,
        int SlotIndex,
        string? TargetLabel,
        bool IsMoving,
        bool InAttackRange,
        Vec2 Position,
        Vec2 Destination,
        Vec2? AvoidanceTarget);

    private readonly record struct Vec2(float X, float Y);

    private void DrawMainTarget(IImageProcessingContext ctx, Vector2 mainTarget)
    {
        ctx.Fill(new SolidBrush(Color.Green.WithAlpha(0.5f)), new EllipsePolygon(mainTarget, 10f));
    }

    private void DrawUnitsFromFrameData(IImageProcessingContext ctx, FrameData frameData)
    {
        // Draw friendly units
        foreach (var unit in frameData.FriendlyUnits)
        {
            var pos = unit.Position.ToVector2();
            ctx.Fill(new SolidBrush(Color.Cyan), new EllipsePolygon(pos, unit.Radius));

            var forward = unit.Forward.ToVector2();
            ctx.DrawLine(new SolidPen(Color.White, 3f), pos, pos + forward * (unit.Radius + 5));

            // Draw destination line
            var dest = unit.CurrentDestination.ToVector2();
            if (Vector2.Distance(pos, dest) > 1f)
            {
                ctx.DrawLine(new SolidPen(Color.LightSkyBlue, 1.5f), pos, dest);
            }

            // Draw label
            if (_labelFont != null)
            {
                var textOptions = new RichTextOptions(_labelFont)
                {
                    Origin = new PointF(pos.X, pos.Y - unit.Radius - 18),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                ctx.DrawText(textOptions, unit.Label, Color.White);
            }
        }

        // Draw enemy units
        foreach (var unit in frameData.EnemyUnits)
        {
            var pos = unit.Position.ToVector2();
            var color = unit.IsDead ? Color.Gray : Color.Red;
            ctx.Fill(new SolidBrush(color), new EllipsePolygon(pos, unit.Radius));

            if (!unit.IsDead)
            {
                var forward = unit.Forward.ToVector2();
                ctx.DrawLine(new SolidPen(Color.OrangeRed, 2f), pos, pos + forward * (unit.Radius + 5));
            }

            // Draw destination line
            var dest = unit.CurrentDestination.ToVector2();
            if (Vector2.Distance(pos, dest) > 1f)
            {
                ctx.DrawLine(new SolidPen(Color.LightSalmon, 1.5f), pos, dest);
            }

            // Draw label
            if (_labelFont != null)
            {
                var textOptions = new RichTextOptions(_labelFont)
                {
                    Origin = new PointF(pos.X, pos.Y - unit.Radius - 18),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                ctx.DrawText(textOptions, unit.Label, Color.White);
            }
        }
    }

    private void DrawUnits(IImageProcessingContext ctx, List<Unit> friendlies, List<Unit> enemies)
    {
        var labelQueue = new List<(Unit unit, Color color)>();
        DrawFriendlyUnits(ctx, friendlies, labelQueue);
        DrawEnemyUnits(ctx, enemies, labelQueue);
        DrawUnitLabels(ctx, labelQueue);
    }

    private void DrawFriendlyUnits(IImageProcessingContext ctx, List<Unit> friendlies, List<(Unit unit, Color color)> labelQueue)
    {
        foreach (var friendly in friendlies)
        {
            ctx.Fill(new SolidBrush(Color.Cyan), new EllipsePolygon(friendly.Position, friendly.Radius));
            ctx.DrawLine(new SolidPen(Color.White, 3f), friendly.Position, friendly.Position + friendly.Forward * (friendly.Radius + 5));

            DrawMovementDebug(ctx, friendly);
            DrawAttackSlots(ctx, friendly);
            DrawRecentAttacks(ctx, friendly);
            labelQueue.Add((friendly, Color.White));
        }
    }

    private void DrawAttackSlots(IImageProcessingContext ctx, Unit unit)
    {
        for (int i = 0; i < unit.AttackSlots.Length; i++)
        {
            var slotTaker = unit.AttackSlots[i];
            var slotPos = unit.GetSlotPosition(i, 30f);
            var color = slotTaker != null ? Color.Red.WithAlpha(0.8f) : Color.Gray.WithAlpha(0.3f);
            float radius = slotTaker != null ? 6f : 3f;
            ctx.Fill(color, new EllipsePolygon(slotPos, radius));
        }
    }

    private void DrawRecentAttacks(IImageProcessingContext ctx, Unit unit)
    {
        for (int i = unit.RecentAttacks.Count - 1; i >= 0; i--)
        {
            var (target, timer) = unit.RecentAttacks[i];
            if (timer > 0)
            {
                ctx.DrawLine(new SolidPen(Color.White, 2f), unit.Position, target.Position);
                unit.RecentAttacks[i] = new Tuple<Unit, int>(target, timer - 1);
            }
            else unit.RecentAttacks.RemoveAt(i);
        }
    }

    private void DrawEnemyUnits(IImageProcessingContext ctx, List<Unit> enemies, List<(Unit unit, Color color)> labelQueue)
    {
        foreach (var enemy in enemies)
        {
            var color = enemy.IsDead ? Color.Gray : Color.Red;
            ctx.Fill(new SolidBrush(color), new EllipsePolygon(enemy.Position, enemy.Radius));
            if (!enemy.IsDead)
            {
                ctx.DrawLine(new SolidPen(Color.OrangeRed, 2f), enemy.Position, enemy.Position + enemy.Forward * (enemy.Radius + 5));
            }

            DrawMovementDebug(ctx, enemy);
            labelQueue.Add((enemy, Color.White));
        }
    }

    private void DrawMovementDebug(IImageProcessingContext ctx, Unit unit)
    {
        if (Vector2.Distance(unit.Position, unit.CurrentDestination) > 1f)
        {
            var pathColor = unit.Faction == UnitFaction.Friendly ? Color.LightSkyBlue : Color.LightSalmon;
            ctx.DrawLine(new SolidPen(pathColor, 1.5f), unit.Position, unit.CurrentDestination);
        }

        if (unit.HasAvoidanceTarget && unit.AvoidanceTarget != Vector2.Zero)
        {
            ctx.Draw(new SolidPen(Color.Gold, 2f), new EllipsePolygon(unit.AvoidanceTarget, 6f));
            ctx.DrawLine(new SolidPen(Color.Gold, 1.5f), unit.Position, unit.AvoidanceTarget);

            if (unit.AvoidanceThreat != null)
            {
                ctx.DrawLine(new SolidPen(Color.Goldenrod, 1f), unit.Position, unit.AvoidanceThreat.Position);
                if (_labelFont != null)
                {
                    var info = $"Avoid {unit.AvoidanceThreat.Label}";
                    var textOptions = new RichTextOptions(_labelFont)
                    {
                        Origin = new PointF(unit.AvoidanceTarget.X + 6, unit.AvoidanceTarget.Y + 6)
                    };
                    ctx.DrawText(textOptions, info, Color.Gold);
                }
            }
        }
    }

    private void DrawUnitLabels(IImageProcessingContext ctx, List<(Unit unit, Color color)> labelQueue)
    {
        foreach (var (unit, color) in labelQueue)
        {
            DrawUnitLabel(ctx, unit, color);
        }
    }

    private void DrawUnitLabel(IImageProcessingContext ctx, Unit unit, Color color)
    {
        if (_labelFont == null) return;

        var textOptions = new RichTextOptions(_labelFont)
        {
            Origin = new PointF(unit.Position.X, unit.Position.Y - unit.Radius - 18),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        ctx.DrawText(textOptions, unit.Label, color);
    }

    private void InitializeFont()
    {
        var fontCollection = new FontCollection();
        try
        {
            _font = fontCollection.Add("Arial").CreateFont(16, FontStyle.Regular);
        }
        catch
        {
            try
            {
                if (SystemFonts.TryGet("Verdana", out var f))
                    _font = f.CreateFont(16);
            }
            catch
            {
                try
                {
                    _font = SystemFonts.Collection.Families.First().CreateFont(16);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not find system font. {ex.Message}");
                }
            }
        }

        _labelFont = _font;
    }
}
