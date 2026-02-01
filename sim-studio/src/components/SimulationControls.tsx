import { CameraFocusMode, SPEED_PRESETS, SpeedMultiplier } from '../types';

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
  focusMode: CameraFocusMode;
  onFocusModeChange: (mode: CameraFocusMode) => void;
  disabled?: boolean;
  // 속도 조절
  speed: SpeedMultiplier;
  onSpeedChange: (speed: SpeedMultiplier) => void;
  // 프레임 타임라인
  currentFrame: number;
  maxFrame: number;
  onFrameSeek: (frame: number) => void;
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
  focusMode,
  onFocusModeChange,
  disabled = false,
  speed,
  onSpeedChange,
  currentFrame,
  maxFrame,
  onFrameSeek,
}: SimulationControlsProps) {
  const isDisabled = !isConnected || disabled;

  return (
    <div className="panel">
      <h2>Simulation Controls</h2>
      <div className="controls">
        <button
          className="btn-primary"
          onClick={onPlayPause}
          disabled={isDisabled}
        >
          {isPlaying ? 'Pause' : 'Play'}
        </button>
        <button
          className="btn-secondary"
          onClick={onStep}
          disabled={isDisabled}
        >
          Step
        </button>
        <button
          className="btn-secondary"
          onClick={onStepBack}
          disabled={isDisabled}
        >
          Step Back
        </button>
        <button
          className="btn-secondary"
          onClick={onReset}
          disabled={isDisabled}
        >
          Reset
        </button>
      </div>

      {/* 속도 조절 컨트롤 */}
      <div className="controls" style={{ marginTop: '0.5rem' }}>
        <div className="speed-control">
          <span className="speed-label">Speed</span>
          <div className="speed-buttons">
            {SPEED_PRESETS.map((preset) => (
              <button
                key={preset}
                className={`speed-btn ${speed === preset ? 'active' : ''}`}
                onClick={() => onSpeedChange(preset)}
                disabled={!isConnected}
                title={`${preset}x speed`}
              >
                {preset}x
              </button>
            ))}
          </div>
          <span className="speed-shortcut-hint">[ / ] keys</span>
        </div>
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
          Seek
        </button>
      </div>

      {/* 프레임 타임라인 슬라이더 */}
      <div className="timeline-container" style={{ marginTop: '0.5rem' }}>
        <div className="timeline-info">
          <span className="timeline-frame">Frame: {currentFrame}</span>
          <span className="timeline-max">/ {maxFrame}</span>
        </div>
        <input
          type="range"
          className="timeline-slider"
          min={0}
          max={Math.max(maxFrame, 1)}
          value={currentFrame}
          onChange={(e) => onFrameSeek(parseInt(e.target.value, 10))}
          disabled={!isConnected}
        />
      </div>

      <div className="controls" style={{ marginTop: '0.75rem' }}>
        <label className="control-label">
          Camera Focus
          <select
            value={focusMode}
            onChange={(e) => onFocusModeChange(e.target.value as CameraFocusMode)}
            disabled={!isConnected}
          >
            <option value="auto">Auto (Selected &gt; Living)</option>
            <option value="selected">Selected Only</option>
            <option value="all-living">All Living</option>
            <option value="friendly">Friendly</option>
            <option value="enemy">Enemy</option>
            <option value="all">All Units</option>
          </select>
        </label>
      </div>

      <style>{`
        .speed-control {
          display: flex;
          align-items: center;
          gap: 0.5rem;
          flex: 1;
        }

        .speed-label {
          font-size: 0.75rem;
          color: #cbd5f5;
          white-space: nowrap;
        }

        .speed-buttons {
          display: flex;
          gap: 0.25rem;
        }

        .speed-btn {
          padding: 0.25rem 0.5rem;
          font-size: 0.75rem;
          background-color: #0f3460;
          color: #eaeaea;
          border: 1px solid #0f3460;
          border-radius: 4px;
          cursor: pointer;
          min-width: 44px;
          transition: background-color 0.15s, border-color 0.15s;
        }

        .speed-btn:hover:not(:disabled) {
          background-color: #164b8a;
        }

        .speed-btn.active {
          background-color: #e94560;
          border-color: #e94560;
          color: white;
        }

        .speed-btn:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }

        .speed-shortcut-hint {
          font-size: 0.65rem;
          color: #6b7280;
          white-space: nowrap;
        }

        .timeline-container {
          display: flex;
          flex-direction: column;
          gap: 0.25rem;
        }

        .timeline-info {
          display: flex;
          gap: 0.25rem;
          font-size: 0.8rem;
          color: #9ca3af;
        }

        .timeline-frame {
          color: #eaeaea;
          font-weight: 500;
        }

        .timeline-slider {
          width: 100%;
          height: 6px;
          -webkit-appearance: none;
          appearance: none;
          background: #1a1a2e;
          border-radius: 3px;
          outline: none;
          cursor: pointer;
        }

        .timeline-slider::-webkit-slider-thumb {
          -webkit-appearance: none;
          appearance: none;
          width: 14px;
          height: 14px;
          background: #e94560;
          border-radius: 50%;
          cursor: pointer;
          border: 2px solid #16213e;
        }

        .timeline-slider::-moz-range-thumb {
          width: 14px;
          height: 14px;
          background: #e94560;
          border-radius: 50%;
          cursor: pointer;
          border: 2px solid #16213e;
        }

        .timeline-slider::-webkit-slider-runnable-track {
          height: 6px;
          background: linear-gradient(to right, #e94560 0%, #e94560 var(--progress, 0%), #1a1a2e var(--progress, 0%), #1a1a2e 100%);
          border-radius: 3px;
        }

        .timeline-slider:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }
      `}</style>
    </div>
  );
}

export default SimulationControls;
