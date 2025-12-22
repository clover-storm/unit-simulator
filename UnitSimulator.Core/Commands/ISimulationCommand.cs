namespace UnitSimulator;

/// <summary>
/// Base interface for all simulation commands.
/// Commands are serializable and support deterministic replay.
/// </summary>
public interface ISimulationCommand
{
    /// <summary>
    /// The frame number when this command should be executed.
    /// Commands are processed at the start of the specified frame.
    /// </summary>
    int FrameNumber { get; }
}
