import React, { useState } from 'react';
import { 
  Title2, 
  Subtitle1, 
  Button, 
  Card, 
  CardHeader, 
  Divider,
  Spinner,
  Badge,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Text,
  tokens
} from '@fluentui/react-components';
import { Timer24Regular, ArrowRight24Regular, Flash24Regular } from '@fluentui/react-icons';
import { sharePointApi } from '../services/api';

interface Task {
  id: number;
  title: string;
  status: string;
}

export const ReadingLab: React.FC = () => {
  // Estados para o Lab Clássico
  const [classicItems, setClassicItems] = useState<Task[]>([]);
  const [classicTime, setClassicTime] = useState<number | null>(null);
  const [nextPos, setNextPos] = useState<string | null>(null);
  const [isClassicLoading, setIsClassicLoading] = useState(false);

  // Estados para o Lab Stream
  const [streamItems, setStreamItems] = useState<Task[]>([]);
  const [streamTime, setStreamTime] = useState<number | null>(null);
  const [isStreamLoading, setIsStreamLoading] = useState(false);

  // Função para buscar dados via CSOM Clássico
  const fetchClassic = async (isNext = false) => {
    setIsClassicLoading(true);
    try {
      // Passamos o nextPos se estivermos indo para a "próxima página"
      const pos = isNext ? nextPos || undefined : undefined;
      const response = await sharePointApi.getPaged(10, pos);
      
      const { items, nextPosition, elapsedMs } = response.data;
      
      setClassicItems(items);
      setNextPos(nextPosition);
      setClassicTime(elapsedMs);
    } catch (error) {
      console.error("Erro ao buscar dados clássicos", error);
    } finally {
      setIsClassicLoading(false);
    }
  };

  // Função para buscar dados via Stream API
  const fetchStream = async () => {
    setIsStreamLoading(true);
    try {
      const response = await sharePointApi.getStream(10);
      const { items, elapsedMs } = response.data;
      setStreamItems(items);
      setStreamTime(elapsedMs);
    } catch (error) {
      console.error("Erro ao buscar dados via stream", error);
    } finally {
      setIsStreamLoading(false);
    }
  };

  return (
    <div style={{ padding: '20px', display: 'flex', flexDirection: 'column', gap: '20px' }}>
      <Title2>Reading Lab: Large Data Volumes</Title2>
      <Text>Compare a eficiência entre a paginação clássica do CSOM e a API moderna de Stream.</Text>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px' }}>
        
        {/* Lado Esquerdo: Classic Paging */}
        <Card>
          <CardHeader 
            header={<Subtitle1>Classic Paging (CSOM)</Subtitle1>}
            description="Usa ListItemCollectionPosition para navegar entre páginas."
          />
          <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
            <Button appearance="primary" icon={<Timer24Regular />} onClick={() => fetchClassic(false)} disabled={isClassicLoading}>
              Primeira Página
            </Button>
            <Button icon={<ArrowRight24Regular />} onClick={() => fetchClassic(true)} disabled={isClassicLoading || !nextPos}>
              Próxima Página
            </Button>
          </div>

          {classicTime !== null && (
            <Badge appearance="filled" color="informative" icon={<Timer24Regular />} style={{ marginBottom: '10px' }}>
              Tempo no Servidor: {classicTime}ms
            </Badge>
          )}

          {isClassicLoading ? <Spinner label="Lendo do SharePoint..." /> : (
            <Table size="extra-small">
              <TableHeader>
                <TableRow>
                  <TableHeaderCell>ID</TableHeaderCell>
                  <TableHeaderCell>Título</TableHeaderCell>
                </TableRow>
              </TableHeader>
              <TableBody>
                {classicItems.map(item => (
                  <TableRow key={item.id}>
                    <TableCell>{item.id}</TableCell>
                    <TableCell>{item.title}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </Card>

        {/* Lado Direito: Stream API */}
        <Card>
          <CardHeader 
            header={<Subtitle1>RenderListDataAsStream</Subtitle1>}
            description="A API moderna otimizada para listas grandes."
          />
          <div style={{ marginBottom: '10px' }}>
            <Button 
              appearance="primary" 
              icon={<Flash24Regular />} 
              onClick={fetchStream} 
              disabled={isStreamLoading} 
              style={{ backgroundColor: '#107c10', color: 'white' }} // Verde "Office/SharePoint" oficial
            >
              Executar Leitura Stream
            </Button>
          </div>

          {streamTime !== null && (
            <Badge appearance="filled" color="success" icon={<Flash24Regular />} style={{ marginBottom: '10px' }}>
              Tempo no Servidor: {streamTime}ms
            </Badge>
          )}

          {isStreamLoading ? <Spinner label="Processando Stream..." /> : (
            <Table size="extra-small">
              <TableHeader>
                <TableRow>
                  <TableHeaderCell>ID</TableHeaderCell>
                  <TableHeaderCell>Título</TableHeaderCell>
                </TableRow>
              </TableHeader>
              <TableBody>
                {streamItems.map(item => (
                  <TableRow key={item.id}>
                    <TableCell>{item.id}</TableCell>
                    <TableCell>{item.title}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          <div style={{ marginTop: '20px', padding: '15px', backgroundColor: tokens.colorNeutralBackground2, borderRadius: '4px' }}>
            <Text italic>
              Dica: O RenderListDataAsStream evita a sobrecarga de objetos CSOM no servidor.
            </Text>
          </div>
        </Card>

      </div>
    </div>
  );
};
