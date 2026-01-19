using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReferenceModels.Models;

/// <summary>
/// 웨이브 레퍼런스 데이터.
/// waves.json의 각 웨이브 정의를 나타냅니다.
/// </summary>
public class WaveReference
{
    /// <summary>웨이브 번호 (1-based)</summary>
    [JsonPropertyName("waveNumber")]
    public int WaveNumber { get; set; }

    /// <summary>스폰 정의 목록</summary>
    [JsonPropertyName("spawns")]
    public List<WaveSpawnReference> Spawns { get; set; } = new();

    /// <summary>웨이브 시작 전 딜레이 (프레임)</summary>
    [JsonPropertyName("delayFrames")]
    public int DelayFrames { get; set; } = 0;
}

/// <summary>
/// 웨이브 내 스폰 정의.
/// </summary>
public class WaveSpawnReference
{
    /// <summary>유닛 ID (units.json 참조)</summary>
    [JsonPropertyName("unitId")]
    public string UnitId { get; set; } = string.Empty;

    /// <summary>스폰 위치</summary>
    [JsonPropertyName("position")]
    public WavePosition Position { get; set; } = new();

    /// <summary>커스텀 HP (선택)</summary>
    [JsonPropertyName("customHP")]
    public int? CustomHP { get; set; }

    /// <summary>커스텀 스피드 (선택)</summary>
    [JsonPropertyName("customSpeed")]
    public float? CustomSpeed { get; set; }
}

/// <summary>
/// 2D 위치 (x, y).
/// </summary>
public class WavePosition
{
    /// <summary>X 좌표</summary>
    [JsonPropertyName("x")]
    public float X { get; set; }

    /// <summary>Y 좌표</summary>
    [JsonPropertyName("y")]
    public float Y { get; set; }
}
