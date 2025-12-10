import { useState, useEffect, useCallback, useRef } from 'react';
import { FrameData, Command, WebSocketMessage } from '../types';

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected';

export interface UseWebSocketResult {
  frameData: FrameData | null;
  frameLog: FrameData[];
  connectionStatus: ConnectionStatus;
  sendCommand: (command: Command) => void;
  error: string | null;
  lastMessageType: WebSocketMessage['type'] | null;
}

export function useWebSocket(url: string): UseWebSocketResult {
  const [frameData, setFrameData] = useState<FrameData | null>(null);
  const [frameLogMap, setFrameLogMap] = useState<Map<number, FrameData>>(new Map());
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('disconnected');
  const [error, setError] = useState<string | null>(null);
  const [lastMessageType, setLastMessageType] = useState<WebSocketMessage['type'] | null>(null);
  const wsRef = useRef<WebSocket | null>(null);
  const reconnectTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const reconnectAttempts = useRef(0);

  const connect = useCallback(() => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      return;
    }

    setConnectionStatus('connecting');
    setError(null);

    try {
      const ws = new WebSocket(url);
      wsRef.current = ws;

      ws.onopen = () => {
        setConnectionStatus('connected');
        setError(null);
        reconnectAttempts.current = 0;
      };

      ws.onmessage = (event) => {
        try {
          const message: WebSocketMessage = JSON.parse(event.data);
          setLastMessageType(message.type);
          
          switch (message.type) {
            case 'frame':
              const newFrameData = message.data as FrameData;
              setFrameData(newFrameData);
              // Add frame to log, replacing if it already exists
              setFrameLogMap(prev => {
                const updated = new Map(prev);
                updated.set(newFrameData.frameNumber, newFrameData);
                return updated;
              });
              break;
            case 'state_change':
              // Could show a notification here
              break;
            case 'simulation_complete':
              // Could show completion UI
              break;
            case 'error':
              setError(message.data as string);
              break;
            default:
              break;
          }
        } catch {
          console.error('Failed to parse WebSocket message:', event.data);
        }
      };

      ws.onclose = () => {
        setConnectionStatus('disconnected');
        wsRef.current = null;

        // Attempt to reconnect with exponential backoff
        if (reconnectAttempts.current < 5) {
          const delay = Math.min(1000 * Math.pow(2, reconnectAttempts.current), 30000);
          reconnectAttempts.current++;
          
          reconnectTimeoutRef.current = setTimeout(() => {
            connect();
          }, delay);
        } else {
          setError('Failed to connect after multiple attempts');
        }
      };

      ws.onerror = () => {
        setError('WebSocket connection error');
      };
    } catch (err) {
      setError('Failed to create WebSocket connection');
      setConnectionStatus('disconnected');
    }
  }, [url]);

  const sendCommand = useCallback((command: Command) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify({
        type: 'command',
        data: command,
        timestamp: Date.now(),
      }));
      
      // When seeking, clean up frames after the target frame
      if (command.type === 'seek' && command.frameNumber !== undefined) {
        const targetFrame = command.frameNumber;
        setFrameLogMap(prev => {
          const updated = new Map(prev);
          // Remove all frames with frameNumber > targetFrame
          for (const frameNumber of updated.keys()) {
            if (frameNumber > targetFrame) {
              updated.delete(frameNumber);
            }
          }
          return updated;
        });
      }
    } else {
      setError('Not connected to server');
    }
  }, []);

  useEffect(() => {
    connect();

    return () => {
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
      }
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, [connect]);

  // Convert frameLogMap to sorted array
  const frameLog = Array.from(frameLogMap.values()).sort(
    (a, b) => a.frameNumber - b.frameNumber
  );

  return {
    frameData,
    frameLog,
    connectionStatus,
    sendCommand,
    error,
    lastMessageType,
  };
}
