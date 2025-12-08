interface SimulationControlsProps {
  onPlayPause: () => void;
  onStep: () => void;
  onStepBack: () => void;
  onSeek: () => void;
  seekValue: string;
  onSeekValueChange: (value: string) => void;
  onReset: () => void;
  isConnected: boolean;
  isPlaying: boolean;
}

function SimulationControls({
  onPlayPause,
  onStep,
  onStepBack,
  onSeek,
  seekValue,
  onSeekValueChange,
  onReset,
  isConnected,
  isPlaying,
}: SimulationControlsProps) {
  return (
    <div className="panel">
      <h2>Simulation Controls</h2>
      <div className="controls">
        <button
          className="btn-primary"
          onClick={onPlayPause}
          disabled={!isConnected}
        >
          {isPlaying ? '⏸ Pause' : '▶ Play'}
        </button>
        <button
          className="btn-secondary"
          onClick={onStep}
          disabled={!isConnected}
        >
          ⏭ Step
        </button>
        <button
          className="btn-secondary"
          onClick={onStepBack}
          disabled={!isConnected}
        >
          ⏮ Step Back
        </button>
        <button
          className="btn-secondary"
          onClick={onReset}
          disabled={!isConnected}
        >
          ↻ Reset
        </button>
      </div>
      <div className="controls" style={{ marginTop: '0.5rem' }}>
        <input
          type="number"
          value={seekValue}
          onChange={(e) => onSeekValueChange(e.target.value)}
          min={0}
          placeholder="Frame #"
          style={{ width: '120px' }}
          disabled={!isConnected}
        />
        <button
          className="btn-secondary"
          onClick={onSeek}
          disabled={!isConnected}
        >
          ⏩ Seek
        </button>
      </div>
    </div>
  );
}

export default SimulationControls;
