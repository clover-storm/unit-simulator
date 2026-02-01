import { GameResult } from '../types';

interface GameResultOverlayProps {
  result: GameResult;
  onRestart: () => void;
  onDismiss: () => void;
}

function GameResultOverlay({ result, onRestart, onDismiss }: GameResultOverlayProps) {
  const isWin = result.winner === 'Friendly';
  const isDraw = result.winner === 'Draw';

  const titleText = isDraw ? 'DRAW' : isWin ? 'VICTORY' : 'DEFEAT';
  const titleColor = isDraw ? '#fbbf24' : isWin ? '#4ade80' : '#f87171';

  // 크라운 표시 (최대 3개)
  const renderCrowns = (count: number, color: string) => {
    return (
      <div className="crown-display">
        {[0, 1, 2].map(i => (
          <span
            key={i}
            className={`crown ${i < count ? 'active' : 'inactive'}`}
            style={{ color: i < count ? color : '#374151' }}
          >
            ♛
          </span>
        ))}
      </div>
    );
  };

  return (
    <div className="game-result-overlay" onClick={onDismiss}>
      <div className="game-result-content" onClick={(e) => e.stopPropagation()}>
        {/* 결과 타이틀 */}
        <h1 className="result-title" style={{ color: titleColor }}>
          {titleText}
        </h1>

        {/* 오버타임 표시 */}
        {result.isOvertime && (
          <div className="overtime-badge">⏱ OVERTIME</div>
        )}

        {/* 크라운 비교 */}
        <div className="crowns-comparison">
          <div className="crown-side friendly">
            <span className="crown-label">Friendly</span>
            {renderCrowns(result.friendlyCrowns, '#4ade80')}
            <span className="crown-count">{result.friendlyCrowns}</span>
          </div>
          <div className="crown-vs">VS</div>
          <div className="crown-side enemy">
            <span className="crown-label">Enemy</span>
            {renderCrowns(result.enemyCrowns, '#f87171')}
            <span className="crown-count">{result.enemyCrowns}</span>
          </div>
        </div>

        {/* 추가 정보 */}
        <div className="result-info">
          <span>Frame: {result.finalFrame}</span>
          <span>Reason: {result.reason}</span>
        </div>

        {/* 버튼 */}
        <div className="result-actions">
          <button className="btn-primary" onClick={onRestart}>
            Restart
          </button>
          <button className="btn-secondary" onClick={onDismiss}>
            Dismiss
          </button>
        </div>
      </div>

      <style>{`
        .game-result-overlay {
          position: absolute;
          inset: 0;
          display: flex;
          align-items: center;
          justify-content: center;
          background-color: rgba(0, 0, 0, 0.75);
          z-index: 100;
          animation: fadeIn 0.3s ease-out;
        }

        @keyframes fadeIn {
          from { opacity: 0; }
          to { opacity: 1; }
        }

        @keyframes slideUp {
          from { transform: translateY(20px); opacity: 0; }
          to { transform: translateY(0); opacity: 1; }
        }

        .game-result-content {
          background-color: #16213e;
          border: 2px solid #0f3460;
          border-radius: 12px;
          padding: 2rem;
          text-align: center;
          min-width: 360px;
          max-width: 480px;
          animation: slideUp 0.4s ease-out;
        }

        .result-title {
          font-size: 2.5rem;
          font-weight: 800;
          margin-bottom: 0.5rem;
          text-shadow: 0 2px 8px rgba(0, 0, 0, 0.5);
          letter-spacing: 0.1em;
        }

        .overtime-badge {
          display: inline-block;
          padding: 0.25rem 0.75rem;
          background-color: rgba(251, 191, 36, 0.15);
          color: #fbbf24;
          border: 1px solid rgba(251, 191, 36, 0.3);
          border-radius: 12px;
          font-size: 0.8rem;
          font-weight: 600;
          margin-bottom: 1rem;
        }

        .crowns-comparison {
          display: flex;
          align-items: center;
          justify-content: center;
          gap: 1.5rem;
          margin: 1.5rem 0;
        }

        .crown-side {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 0.25rem;
        }

        .crown-label {
          font-size: 0.75rem;
          color: #9ca3af;
          text-transform: uppercase;
          letter-spacing: 0.05em;
        }

        .crown-display {
          display: flex;
          gap: 0.25rem;
        }

        .crown {
          font-size: 1.5rem;
          transition: color 0.3s;
        }

        .crown.inactive {
          opacity: 0.3;
        }

        .crown-count {
          font-size: 1.25rem;
          font-weight: 700;
          color: #eaeaea;
        }

        .crown-vs {
          font-size: 1rem;
          color: #6b7280;
          font-weight: 700;
        }

        .result-info {
          display: flex;
          justify-content: center;
          gap: 1.5rem;
          font-size: 0.8rem;
          color: #6b7280;
          margin-bottom: 1.5rem;
        }

        .result-actions {
          display: flex;
          gap: 0.75rem;
          justify-content: center;
        }

        .result-actions button {
          min-width: 100px;
        }
      `}</style>
    </div>
  );
}

export default GameResultOverlay;
