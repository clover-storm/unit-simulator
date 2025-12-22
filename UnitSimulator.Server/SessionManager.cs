using System.Collections.Concurrent;
using System.Timers;
using Timer = System.Timers.Timer;

namespace UnitSimulator;

/// <summary>
/// Manages multiple simulation sessions with lifecycle control.
/// </summary>
public class SessionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, SimulationSession> _sessions = new();
    private readonly Timer _cleanupTimer;
    private readonly object _cleanupLock = new();
    private bool _disposed;

    /// <summary>
    /// Configuration options for the session manager.
    /// </summary>
    public SessionManagerOptions Options { get; }

    /// <summary>
    /// Current number of active sessions.
    /// </summary>
    public int SessionCount => _sessions.Count;

    public SessionManager(SessionManagerOptions? options = null)
    {
        Options = options ?? new SessionManagerOptions();

        // Setup cleanup timer
        _cleanupTimer = new Timer(Options.CleanupIntervalMs);
        _cleanupTimer.Elapsed += OnCleanupTimer;
        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Start();

        Console.WriteLine($"[SessionManager] Initialized. Max sessions: {Options.MaxSessions}, Idle timeout: {Options.IdleTimeout.TotalMinutes} min");
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="sessionId">Optional specific session ID. If null, a UUID will be generated.</param>
    /// <returns>The created session, or null if max sessions reached.</returns>
    public SimulationSession? CreateSession(string? sessionId = null)
    {
        if (_sessions.Count >= Options.MaxSessions)
        {
            Console.WriteLine($"[SessionManager] Cannot create session: max sessions ({Options.MaxSessions}) reached");
            return null;
        }

        var session = new SimulationSession(sessionId);

        if (_sessions.TryAdd(session.SessionId, session))
        {
            Console.WriteLine($"[SessionManager] Session created: {session.SessionId[..8]}. Total: {_sessions.Count}");
            return session;
        }

        // Session ID already exists
        session.Dispose();
        return null;
    }

    /// <summary>
    /// Gets an existing session by ID.
    /// </summary>
    public SimulationSession? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    /// <summary>
    /// Gets or creates a session.
    /// </summary>
    public SimulationSession? GetOrCreateSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var existing))
        {
            return existing;
        }

        return CreateSession(sessionId);
    }

    /// <summary>
    /// Lists all active sessions.
    /// </summary>
    public IEnumerable<SessionInfo> ListSessions()
    {
        return _sessions.Values.Select(s => s.GetInfo());
    }

    /// <summary>
    /// Removes and disposes a session.
    /// </summary>
    public bool RemoveSession(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            session.Dispose();
            Console.WriteLine($"[SessionManager] Session removed: {sessionId[..8]}. Total: {_sessions.Count}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Cleans up idle sessions that have exceeded the timeout.
    /// </summary>
    public int CleanupIdleSessions()
    {
        lock (_cleanupLock)
        {
            var now = DateTime.UtcNow;
            var expiredSessions = _sessions
                .Where(kvp =>
                    kvp.Value.ClientCount == 0 &&
                    (now - kvp.Value.LastActivityAt) > Options.IdleTimeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                RemoveSession(sessionId);
            }

            if (expiredSessions.Count > 0)
            {
                Console.WriteLine($"[SessionManager] Cleaned up {expiredSessions.Count} idle sessions");
            }

            return expiredSessions.Count;
        }
    }

    private void OnCleanupTimer(object? sender, ElapsedEventArgs e)
    {
        try
        {
            CleanupIdleSessions();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionManager] Cleanup error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cleanupTimer.Stop();
        _cleanupTimer.Dispose();

        foreach (var session in _sessions.Values)
        {
            try
            {
                session.Dispose();
            }
            catch { }
        }

        _sessions.Clear();
        Console.WriteLine("[SessionManager] Disposed");
    }
}

/// <summary>
/// Configuration options for SessionManager.
/// </summary>
public class SessionManagerOptions
{
    /// <summary>
    /// Maximum number of concurrent sessions allowed.
    /// Default: 100 (data-driven, can be adjusted)
    /// </summary>
    public int MaxSessions { get; set; } = 100;

    /// <summary>
    /// Time after which an idle session (no clients) will be automatically removed.
    /// Default: 30 minutes
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Interval for running the cleanup timer.
    /// Default: 1 minute
    /// </summary>
    public double CleanupIntervalMs { get; set; } = 60_000;
}
