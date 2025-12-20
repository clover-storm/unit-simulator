using System.Net.WebSockets;

namespace UnitSimulator;

/// <summary>
/// Represents a client connected to a simulation session.
/// </summary>
public class SessionClient
{
    /// <summary>
    /// The WebSocket connection for this client.
    /// </summary>
    public WebSocket Socket { get; }

    /// <summary>
    /// Unique identifier for this client, persisted in browser localStorage.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// The role of this client in the session (Owner or Viewer).
    /// </summary>
    public SessionRole Role { get; private set; }

    /// <summary>
    /// When this client joined the session.
    /// </summary>
    public DateTime JoinedAt { get; }

    /// <summary>
    /// Reference to the session this client belongs to.
    /// </summary>
    public SimulationSession? Session { get; set; }

    public SessionClient(WebSocket socket, string clientId, SessionRole role)
    {
        Socket = socket;
        ClientId = clientId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the role of this client.
    /// </summary>
    public void SetRole(SessionRole role)
    {
        Role = role;
    }

    /// <summary>
    /// Checks if the client connection is still open.
    /// </summary>
    public bool IsConnected => Socket.State == WebSocketState.Open;
}

/// <summary>
/// Defines the role of a client within a session.
/// </summary>
public enum SessionRole
{
    /// <summary>
    /// Session creator with full control permissions.
    /// Can execute all commands (start, stop, move, etc.)
    /// </summary>
    Owner,

    /// <summary>
    /// Observer with read-only access.
    /// Can only view frames and seek through history locally.
    /// </summary>
    Viewer
}
