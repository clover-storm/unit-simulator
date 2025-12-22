namespace UnitSimulator;

/// <summary>
/// Server infrastructure constants.
/// These define rendering, output, and server-specific settings.
/// </summary>
public static class ServerConstants
{
    // Rendering settings (can differ from simulation space if needed)
    public const int IMAGE_WIDTH = GameConstants.SIMULATION_WIDTH;
    public const int IMAGE_HEIGHT = GameConstants.SIMULATION_HEIGHT;

    // Output settings
    public const string OUTPUT_DIRECTORY = "output";
    public const string DEBUG_SUBDIRECTORY = "debug";
}
