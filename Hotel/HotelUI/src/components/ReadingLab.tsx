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
  tokens,
  makeStyles
} from '@fluentui/react-components';
import { Timer24Regular, ArrowRight24Regular, Flash24Regular } from '@fluentui/react-icons';
import { labService } from '../services/api';
import { TaskEditModal } from './TaskEditModal';

const useStyles = makeStyles({
  grid: {
    display: 'grid',
    gridTemplateColumns: '1fr',
    gap: '20px',
    [`@media (min-width: 900px)`]: {
      gridTemplateColumns: '1fr 1fr',
    },
  },
  card: {
    width: '100%',
    overflowX: 'auto'
  }
});

interface Task {
  id: number;
  title: string;
  status: string;
  description?: string;
  dueDate?: string;
}

export const ReadingLab: React.FC = () => {
  const styles = useStyles();
  // Estados para o Lab Clássico
  const [classicItems, setClassicItems] = useState<Task[]>([]);
  const [classicTime, setClassicTime] = useState<number | null>(null);
  const [nextPos, setNextPos] = useState<string | null>(null);
  const [isClassicLoading, setIsClassicLoading] = useState(false);

  // Estados para o Lab Stream
  const [streamItems, setStreamItems] = useState<Task[]>([]);
  const [streamTime, setStreamTime] = useState<number | null>(null);
  const [isStreamLoading, setIsStreamLoading] = useState(false);

  // Estados para o Modal
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Função para buscar dados via CSOM Clássico
  const fetchClassic = async (isNext = false) => {
    setIsClassicLoading(true);
    try {
      const pos = isNext ? nextPos || undefined : undefined;
      const response = await labService.getPaged(10, pos);
      const { items, nextPosition, elapsedMs } = response.data;
      
      setClassicItems(items);
      setNextPos(nextPosition);
      setClassicTime(elapsedMs);
    } catch (error) {
      console.error(error);
    } finally {
      setIsClassicLoading(false);
    }
  };

  const fetchStream = async () => {
    setIsStreamLoading(true);
    try {
      const response = await labService.getStream(10);
      const { items, elapsedMs } = response.data;
      setStreamItems(items);
      setStreamTime(elapsedMs);
    } catch (error) {
      console.error(error);
    } finally {
      setIsStreamLoading(false);
    }
  };

  const handleRowClick = (task: Task) => {
    setSelectedTask(task);
    setIsModalOpen(true);
  };

  const handleModalClose = (updated: boolean) => {
    setIsModalOpen(false);
    if (updated) {
      if (classicItems.length > 0) fetchClassic(false);
      if (streamItems.length > 0) fetchStream();
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
      <div>
        <Title2>The Reading Lab: Large Data Volumes</Title2>
        <Text block>Explore diferentes técnicas para ler dados do SharePoint, focando em performance e contorno do limite de 5.000 itens.</Text>
      </div>

      <div className={styles.grid}>
        
        {/* Lado Esquerdo: Clássico Paged */}
        <Card className={styles.card}>
          <CardHeader 
            header={<Subtitle1>A: ListItemCollectionPosition</Subtitle1>}
            description="Paginação clássica baseada em tokens de posição."
          />
          <div style={{ marginBottom: '10px', display: 'flex', flexWrap: 'wrap', gap: '10px' }}>
            <Button appearance="primary" onClick={() => fetchClassic(false)} disabled={isClassicLoading}>
              Carregar Início
            </Button>
            <Button 
              icon={<ArrowRight24Regular />} 
              disabled={!nextPos || isClassicLoading} 
              onClick={() => fetchClassic(true)}
            >
              Próxima
            </Button>
          </div>

          {classicTime && (
            <Badge appearance="filled" color="informative" icon={<Timer24Regular />}>
              Tempo: {classicTime}ms
            </Badge>
          )}

          <Divider style={{ margin: '15px 0' }} />

          {isClassicLoading ? <Spinner label="Consultando via CSOM..." /> : (
            <div style={{ overflowX: 'auto' }}>
              <Table size="extra-small">
                <TableHeader>
                  <TableRow>
                    <TableHeaderCell>ID</TableHeaderCell>
                    <TableHeaderCell>Título</TableHeaderCell>
                    <TableHeaderCell>Status</TableHeaderCell>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {classicItems.map(item => (
                    <TableRow 
                      key={item.id} 
                      onClick={() => handleRowClick(item)}
                      style={{ cursor: 'pointer' }}
                    >
                      <TableCell>{item.id}</TableCell>
                      <TableCell>{item.title}</TableCell>
                      <TableCell>
                        <Badge appearance="outline" color={item.status === 'Done' ? 'success' : 'informative'}>
                          {item.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </Card>

        {/* Lado Direito: Modern Stream */}
        <Card className={styles.card}>
          <CardHeader 
            header={<Subtitle1>B: RenderListDataAsStream</Subtitle1>}
            description="API moderna e performática."
          />
          <div style={{ marginBottom: '10px' }}>
            <Button appearance="primary" icon={<Flash24Regular />} onClick={fetchStream} disabled={isStreamLoading} style={{ backgroundColor: tokens.colorPaletteGreenBackground3 }}>
              Executar Stream
            </Button>
          </div>

          {streamTime && (
            <Badge appearance="filled" color="success" icon={<Timer24Regular />}>
              Tempo: {streamTime}ms
            </Badge>
          )}

          <Divider style={{ margin: '15px 0' }} />

          {isStreamLoading ? <Spinner label="Consultando via Stream API..." /> : (
            <div style={{ overflowX: 'auto' }}>
              <Table size="extra-small">
                <TableHeader>
                  <TableRow>
                    <TableHeaderCell>ID</TableHeaderCell>
                    <TableHeaderCell>Título</TableHeaderCell>
                    <TableHeaderCell>Status</TableHeaderCell>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {streamItems.map(item => (
                    <TableRow 
                      key={item.id}
                      onClick={() => handleRowClick(item)}
                      style={{ cursor: 'pointer' }}
                    >
                      <TableCell>{item.id}</TableCell>
                      <TableCell>{item.title}</TableCell>
                      <TableCell>
                        <Badge appearance="outline" color={item.status === 'Done' ? 'success' : 'informative'}>
                          {item.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </Card>

      </div>
      
      <TaskEditModal 
        isOpen={isModalOpen} 
        task={selectedTask} 
        onClose={handleModalClose} 
      />
    </div>
  );
};
