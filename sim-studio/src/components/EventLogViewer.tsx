import { useState, useRef, useEffect, useCallback } from 'react';
import { UnitEvent, UnitEventType } from '../types';

interface EventLogViewerProps {
  events: UnitEvent[];
  onClear: () => void;
}

// 이벤트 타입별 아이콘 및 색상
const EVENT_CONFIG: Record<UnitEventType, { icon: string; color: string; label: string }> = {
  Spawned: { icon: '🟢', color: '#4ade80', label: 'Spawn' },
  Died: { icon: '💀', color: '#f87171', label: 'Death' },
  Attack: { icon: '⚔️', color: '#fbbf24', label: 'Attack' },
  Damaged: { icon: '💥', color: '#fb923c', label: 'Damage' },
  TargetAcquired: { icon: '🎯', color: '#60a5fa', label: 'Target' },
  TargetLost: { icon: '❌', color: '#9ca3af', label: 'Lost Target' },
  MovementStarted: { icon: '🏃', color: '#a78bfa', label: 'Move Start' },
  MovementStopped: { icon: '🛑', color: '#94a3b8', label: 'Move Stop' },
  EnteredCombat: { icon: '🔥', color: '#ef4444', label: 'Combat' },
  ExitedCombat: { icon: '🕊️', color: '#6b7280', label: 'Disengage' },
};

const ALL_EVENT_TYPES: UnitEventType[] = [
  'Spawned', 'Died', 'Attack', 'Damaged',
  'TargetAcquired', 'TargetLost',
  'MovementStarted', 'MovementStopped',
  'EnteredCombat', 'ExitedCombat',
];

function EventLogViewer({ events, onClear }: EventLogViewerProps) {
  const [autoScroll, setAutoScroll] = useState(true);
  const [filterTypes, setFilterTypes] = useState<Set<UnitEventType>>(new Set(ALL_EVENT_TYPES));
  const [unitNameFilter, setUnitNameFilter] = useState('');
  const logContainerRef = useRef<HTMLDivElement>(null);

  // 자동 스크롤
  useEffect(() => {
    if (autoScroll && logContainerRef.current) {
      logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
    }
  }, [events, autoScroll]);

  const handleScroll = useCallback(() => {
    if (!logContainerRef.current) return;
    const { scrollTop, scrollHeight, clientHeight } = logContainerRef.current;
    // 바닥에서 20px 이내면 자동 스크롤 유지
    setAutoScroll(scrollHeight - scrollTop - clientHeight < 20);
  }, []);

  const toggleEventType = useCallback((type: UnitEventType) => {
    setFilterTypes(prev => {
      const next = new Set(prev);
      if (next.has(type)) {
        next.delete(type);
      } else {
        next.add(type);
      }
      return next;
    });
  }, []);

  const toggleAllTypes = useCallback(() => {
    setFilterTypes(prev => {
      if (prev.size === ALL_EVENT_TYPES.length) {
        return new Set<UnitEventType>();
      }
      return new Set(ALL_EVENT_TYPES);
    });
  }, []);

  // 필터링된 이벤트
  const filteredEvents = events.filter(event => {
    if (!filterTypes.has(event.eventType)) return false;
    if (unitNameFilter) {
      const search = unitNameFilter.toLowerCase();
      const unitStr = `${event.faction}-${event.unitId}`.toLowerCase();
      if (!unitStr.includes(search)) return false;
    }
    return true;
  });

  return (
    <div className="panel event-log-panel">
      <h2>Event Log ({filteredEvents.length})</h2>

      {/* 필터 컨트롤 */}
      <div className="event-log-filters">
        <div className="event-type-filters">
          <button
            className={`event-filter-btn ${filterTypes.size === ALL_EVENT_TYPES.length ? 'active' : ''}`}
            onClick={toggleAllTypes}
            title="Toggle all"
          >
            All
          </button>
          {ALL_EVENT_TYPES.map(type => {
            const config = EVENT_CONFIG[type];
            return (
              <button
                key={type}
                className={`event-filter-btn ${filterTypes.has(type) ? 'active' : ''}`}
                onClick={() => toggleEventType(type)}
                title={config.label}
                style={{
                  borderColor: filterTypes.has(type) ? config.color : undefined,
                }}
              >
                {config.icon}
              </button>
            );
          })}
        </div>
        <div className="event-log-search">
          <input
            type="text"
            placeholder="Filter unit (e.g. Friendly-1)"
            value={unitNameFilter}
            onChange={(e) => setUnitNameFilter(e.target.value)}
            style={{ width: '100%', fontSize: '0.8rem' }}
          />
        </div>
        <button
          className="btn-secondary"
          onClick={onClear}
          style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem' }}
        >
          Clear
        </button>
      </div>

      {/* 이벤트 목록 */}
      <div
        className="event-log-list"
        ref={logContainerRef}
        onScroll={handleScroll}
      >
        {filteredEvents.length === 0 ? (
          <div className="empty-state" style={{ padding: '1rem' }}>
            No events
          </div>
        ) : (
          filteredEvents.map(event => {
            const config = EVENT_CONFIG[event.eventType] ?? {
              icon: '❓', color: '#9ca3af', label: event.eventType,
            };
            return (
              <div
                key={event.id}
                className="event-log-item"
                style={{ borderLeftColor: config.color }}
              >
                <span className="event-icon">{config.icon}</span>
                <span className="event-frame">#{event.frameNumber}</span>
                <span className="event-faction" style={{
                  color: event.faction === 'Friendly' ? '#4ade80' : '#f87171',
                }}>
                  {event.faction}
                </span>
                <span className="event-unit">Unit {event.unitId}</span>
                <span className="event-type" style={{ color: config.color }}>
                  {config.label}
                </span>
                {event.targetUnitId != null && (
                  <span className="event-target">→ Unit {event.targetUnitId}</span>
                )}
                {event.value != null && (
                  <span className="event-value">({event.value})</span>
                )}
              </div>
            );
          })
        )}
      </div>

      {!autoScroll && filteredEvents.length > 0 && (
        <button
          className="event-log-scroll-btn"
          onClick={() => {
            setAutoScroll(true);
            if (logContainerRef.current) {
              logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
            }
          }}
        >
          ↓ Scroll to latest
        </button>
      )}

      <style>{`
        .event-log-panel {
          max-height: 400px;
          display: flex;
          flex-direction: column;
        }

        .event-log-filters {
          display: flex;
          flex-direction: column;
          gap: 0.5rem;
          margin-bottom: 0.5rem;
        }

        .event-type-filters {
          display: flex;
          flex-wrap: wrap;
          gap: 0.25rem;
        }

        .event-filter-btn {
          padding: 0.15rem 0.35rem;
          font-size: 0.7rem;
          background-color: #1a1a2e;
          color: #9ca3af;
          border: 1px solid #0f3460;
          border-radius: 4px;
          cursor: pointer;
          transition: background-color 0.15s, border-color 0.15s;
        }

        .event-filter-btn:hover {
          background-color: #0f3460;
        }

        .event-filter-btn.active {
          background-color: #0f3460;
          color: #eaeaea;
        }

        .event-log-search {
          display: flex;
          gap: 0.5rem;
        }

        .event-log-list {
          flex: 1;
          overflow-y: auto;
          display: flex;
          flex-direction: column;
          gap: 2px;
          min-height: 0;
        }

        .event-log-item {
          display: flex;
          align-items: center;
          gap: 0.4rem;
          padding: 0.25rem 0.4rem;
          font-size: 0.75rem;
          background-color: #1a1a2e;
          border-left: 3px solid #0f3460;
          border-radius: 2px;
          white-space: nowrap;
          overflow: hidden;
        }

        .event-icon {
          flex-shrink: 0;
        }

        .event-frame {
          color: #6b7280;
          font-family: monospace;
          font-size: 0.7rem;
          min-width: 50px;
        }

        .event-faction {
          font-size: 0.7rem;
          min-width: 55px;
        }

        .event-unit {
          color: #cbd5f5;
          min-width: 50px;
        }

        .event-type {
          font-weight: 500;
        }

        .event-target {
          color: #9ca3af;
        }

        .event-value {
          color: #6b7280;
        }

        .event-log-scroll-btn {
          margin-top: 0.25rem;
          padding: 0.25rem;
          font-size: 0.7rem;
          background-color: #0f3460;
          color: #eaeaea;
          border: none;
          border-radius: 4px;
          cursor: pointer;
          text-align: center;
        }

        .event-log-scroll-btn:hover {
          background-color: #164b8a;
        }
      `}</style>
    </div>
  );
}

export default EventLogViewer;
