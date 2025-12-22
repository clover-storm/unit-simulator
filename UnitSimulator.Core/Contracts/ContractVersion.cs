namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Versioning for public contracts to track breaking changes over time.
/// </summary>
public static class ContractVersion
{
    /// <summary>Major version for breaking changes.</summary>
    public const int Major = 1;

    /// <summary>Minor version for backward-compatible changes.</summary>
    public const int Minor = 0;
}
