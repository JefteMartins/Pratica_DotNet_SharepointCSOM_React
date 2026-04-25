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
  Input,
  Label
} from '@fluentui/react-components';
import { Timer24Regular, Flash24Regular, Warning24Regular, CheckmarkCircle24Regular } from '@fluentui/react-icons';
import { sharePointApi } from '../services/api';

export const WritingLab: React.FC = () => {
  const [itemCount, setItemCount] = useState<number>(10);
  const [sequentialResult, setSequentialResult] = useState<{ time: number, avg: number } | null>(null);
  const [batchedResult, setBatchedResult] = useState<{ time: number, avg: number } | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [currentMode, setCurrentMode] = useState<string | null>(null);

  const runSequential = async () => {
    setIsLoading(true);
    setCurrentMode('Sequential');
    try {
      const response = await sharePointApi.createSequential(itemCount);
      const { elapsedMs } = response.data;
      setSequentialResult({ 
        time: elapsedMs, 
        avg: Math.round(elapsedMs / itemCount) 
      });
    } catch (error) {
      console.error("Erro no teste sequencial", error);
    } finally {
      setIsLoading(false);
      setCurrentMode(null);
    }
  };

  const runBatched = async () => {
    setIsLoading(true);
    setCurrentMode('Batched');
    try {
      const response = await sharePointApi.createBatched(itemCount, 50);
      const { elapsedMs } = response.data;
      setBatchedResult({ 
        time: elapsedMs, 
        avg: Math.round(elapsedMs / itemCount) 
      });
    } catch (error) {
      console.error("Erro no teste batched", error);
    } finally {
      setIsLoading(false);
      setCurrentMode(null);
    }
  };

  return (
    <div style={{ padding: '20px', display: 'flex', flexDirection: 'column', gap: '20px' }}>
      <Title2>Writing Lab: Batching & Optimization</Title2>
      <Text>Compare o impacto de realizar várias chamadas de rede (round-trips) contra o agrupamento de operações em um único lote.</Text>

      <Card style={{ maxWidth: '400px' }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', padding: '10px' }}>
          <Label htmlFor="item-count">Quantidade de itens para criar:</Label>
          <Input 
            id="item-count" 
            type="number" 
            value={itemCount.toString()} 
            onChange={(e, data) => setItemCount(parseInt(data.value) || 0)} 
          />
          <Text size={200} italic color="neutralTertiary">
            Recomendado: 10 a 50 itens para notar a diferença sem travar o navegador.
          </Text>
        </div>
      </Card>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px' }}>
        
        {/* Lado Esquerdo: Sequential */}
        <Card>
          <CardHeader 
            header={<Subtitle1>Naive Loop (One-by-One)</Subtitle1>}
            description="Executa context.ExecuteQuery() para cada item adicionado."
          />
          <div style={{ marginBottom: '10px' }}>
            <Button 
              appearance="outline" 
              icon={<Warning24Regular />} 
              onClick={runSequential} 
              disabled={isLoading}
              style={{ color: tokens.colorPaletteRedForeground1, borderColor: tokens.colorPaletteRedBorder1 }}
            >
              Executar Sequencial
            </Button>
          </div>

          {sequentialResult && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
              <Badge appearance="filled" color="danger" icon={<Timer24Regular />}>
                Tempo Total: {sequentialResult.time}ms
              </Badge>
              <Badge appearance="outline" color="danger">
                Média: {sequentialResult.avg}ms / item
              </Badge>
            </div>
          )}

          {isLoading && currentMode === 'Sequential' && <Spinner label={`Criando ${itemCount} itens sequencialmente...`} />}
          
          <div style={{ marginTop: '20px', padding: '15px', backgroundColor: tokens.colorNeutralBackground2, borderRadius: '4px' }}>
            <Text size={200}>
              <strong>Problema:</strong> Cada item gera um round-trip de rede completo (Request/Response). A latência da rede mata a performance.
            </Text>
          </div>
        </Card>

        {/* Lado Direito: Batched */}
        <Card>
          <CardHeader 
            header={<Subtitle1>CSOM Batching</Subtitle1>}
            description="Acumula as atualizações e executa uma única query para o lote."
          />
          <div style={{ marginBottom: '10px' }}>
            <Button 
              appearance="primary" 
              icon={<CheckmarkCircle24Regular />} 
              onClick={runBatched} 
              disabled={isLoading} 
              style={{ backgroundColor: tokens.colorPaletteGreenBackground3 }}
            >
              Executar Batched
            </Button>
          </div>

          {batchedResult && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
              <Badge appearance="filled" color="success" icon={<Flash24Regular />}>
                Tempo Total: {batchedResult.time}ms
              </Badge>
              <Badge appearance="outline" color="success">
                Média: {batchedResult.avg}ms / item
              </Badge>
            </div>
          )}

          {isLoading && currentMode === 'Batched' && <Spinner label={`Criando ${itemCount} itens em lote...`} />}

          <div style={{ marginTop: '20px', padding: '15px', backgroundColor: tokens.colorNeutralBackground2, borderRadius: '4px' }}>
            <Text size={200}>
              <strong>Vantagem:</strong> O SharePoint recebe uma lista de instruções de uma vez. O custo da latência de rede é pago apenas uma vez.
            </Text>
          </div>
        </Card>

      </div>

      {sequentialResult && batchedResult && (
        <Card appearance="subtle" style={{ backgroundColor: tokens.colorBrandBackground2 }}>
          <Text weight="bold" size={500}>
            Resultado: A operação Batched foi {Math.round(sequentialResult.time / batchedResult.time)}x mais rápida!
          </Text>
        </Card>
      )}
    </div>
  );
};
