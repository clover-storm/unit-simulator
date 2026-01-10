using System.Text.Json.Serialization;

namespace ReferenceModels.Models;

public class TowerSkillReference
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("effectType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TowerSkillEffectType EffectType { get; init; } = TowerSkillEffectType.TargetedDamage;

    [JsonPropertyName("targetType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TowerSkillTargetType TargetType { get; init; } = TowerSkillTargetType.None;

    [JsonPropertyName("cooldownMs")]
    public int CooldownMs { get; init; } = 10000;

    [JsonPropertyName("damage")]
    public int Damage { get; init; } = 0;

    [JsonPropertyName("range")]
    public float Range { get; init; } = 0f;

    [JsonPropertyName("durationMs")]
    public int DurationMs { get; init; } = 0;

    [JsonPropertyName("effectValue")]
    public int EffectValue { get; init; } = 0;
}

public enum TowerSkillEffectType
{
    TargetedDamage,
    AreaOfEffect,
    Buff,
    Debuff,
    Utility
}

public enum TowerSkillTargetType
{
    None,
    SingleUnit,
    Position
}
