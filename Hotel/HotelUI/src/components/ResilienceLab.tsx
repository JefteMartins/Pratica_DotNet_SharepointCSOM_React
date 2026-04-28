import React, { useState } from 'react';
import { 
  Title2, 
  Subtitle1, 
  Button, 
  Card, 
  CardHeader, 
  Spinner,
  Badge,
  Text,
  tokens,
  Switch,
  Divider,
  makeStyles
} from '@fluentui/react-components';
import { 
  ShieldCheckmark24Regular 
} from '@fluentui/react-icons';
import { labService } from '../services/api';

const useStyles = makeStyles({
  grid: {
    display: 'grid',
    gridTemplateColumns: '1fr',
    gap: '20px',
    [`@media (min-width: 900px)`]: {
      gridTemplateColumns: '1fr 1fr',
    },
  },
  terminal: {
    height: '250px', 
    overflowY: 'auto', 
    display: 'flex', 
    flexDirection: 'column', 
    gap: '5px',
    fontFamily: 'monospace',
    padding: '10px',
    backgroundColor: tokens.colorNeutralBackgroundInverted
  }
});

interface LogEntry {
  id: number;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: string;
}

export const ResilienceLab: React.FC = () => {
  const styles = useStyles();
  const [stressEnabled, setStressEnabled] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [metrics, setMetrics] = useState<{ lastRetries: number; lastTime: number } | null>(null);

  const addLog = (message: string, type: LogEntry['type']) => {
    setLogs(prev => [{
      id: Date.now(),
      message,
      type,
      timestamp: new Date().toLocaleTimeString()
    }, ...prev].slice(0, 10));
  };

  const handleToggleStress = async (enabled: boolean) => {
    try {
      await labService.toggleStress(enabled);
      setStressEnabled(enabled);
      addLog(`Stress Mode ${enabled ? 'ENABLED' : 'DISABLED'}`, enabled ? 'warning' : 'info');
    } catch (error) {
      console.error(error);
      addLog("Error changing stress mode", 'error');
    }
  };

  const runTest = async () => {
    setIsLoading(true);
    addLog("Starting resilient operation...", 'info');
    
    try {
      const response = await labService.createResilient("Resilience Test");
      const { success, retries, elapsedMs, message } = response.data;

      if (success) {
        setMetrics({ lastRetries: retries, lastTime: elapsedMs });
        addLog(`${message} (Retries: ${retries})`, retries > 0 ? 'warning' : 'success');
      } else {
        addLog(`Failure: ${message}`, 'error');
      }
    } catch (error) {
      addLog("Request error", 'error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
      <div>
        <Title2>Resilience Lab: Throttling & Backoff</Title2>
        <Text block>Handle SharePoint Online limits using <strong>Polly</strong>.</Text>
      </div>

      <div className={styles.grid}>
        
        {/* Painel de Controle */}
        <Card>
          <CardHeader 
            header={<Subtitle1>Control</Subtitle1>}
          />
          
          <div style={{ display: 'flex', flexDirection: 'column', gap: '15px', padding: '10px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', flexWrap: 'wrap' }}>
              <Switch 
                label={stressEnabled ? "Stress: ON" : "Stress: OFF"} 
                checked={stressEnabled}
                onChange={(_, data) => handleToggleStress(data.checked)}
              />
              {stressEnabled && <Badge appearance="filled" color="danger">SIMULATED 429</Badge>}
            </div>

            <Divider />

            <Button 
              appearance="primary" 
              icon={<ShieldCheckmark24Regular />} 
              onClick={runTest} 
              disabled={isLoading}
            >
              Execute with Polly
            </Button>
          </div>

          {metrics && (
            <div style={{ marginTop: '20px', display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
              <Badge appearance="outline" color={metrics.lastRetries > 0 ? "warning" : "success"}>
                Retries: {metrics.lastRetries}
              </Badge>
              <Badge appearance="outline" color="informative">
                Time: {metrics.lastTime}ms
              </Badge>
            </div>
          )}
        </Card>

        {/* Terminal de Logs */}
        <Card>
          <CardHeader header={<Subtitle1>Console</Subtitle1>} />
          <div className={styles.terminal}>
            {isLoading && <Spinner size="tiny" label="Backoff..." />}
            {logs.length === 0 && <Text size={200} italic>No activities.</Text>}
            {logs.map(log => (
              <div key={log.id} style={{ fontSize: '11px' }}>
                <span style={{ color: tokens.colorNeutralForeground4 }}>[{log.timestamp}]</span>{' '}
                <span style={{ 
                  color: log.type === 'error' ? tokens.colorPaletteRedForeground1 : 
                         log.type === 'warning' ? tokens.colorPaletteYellowForeground1 :
                         log.type === 'success' ? tokens.colorPaletteGreenForeground1 : 
                         tokens.colorBrandForeground1
                }}>
                  {log.message}
                </span>
              </div>
            ))}
          </div>
        </Card>

      </div>
    </div>
  );
};
