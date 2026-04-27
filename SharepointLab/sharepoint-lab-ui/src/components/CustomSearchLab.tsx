import React, { useState } from 'react';
import { 
  Title2, 
  Subtitle1, 
  Button, 
  Card, 
  CardHeader, 
  Spinner,
  Text,
  tokens,
  Input,
  Label,
  Select,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
  makeStyles
} from '@fluentui/react-components';
import { Search24Regular, Filter24Regular, Delete24Regular } from '@fluentui/react-icons';
import { sharePointApi } from '../services/api';
import { TaskEditModal } from './TaskEditModal';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '20px'
  },
  grid: {
    display: 'grid',
    gridTemplateColumns: '1fr',
    gap: '20px',
    [`@media (min-width: 900px)`]: {
      gridTemplateColumns: '350px 1fr',
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
  dueDate: string;
  description?: string;
}

export const CustomSearchLab: React.FC = () => {
  const styles = useStyles();
  const [filters, setFilters] = useState({
    title: '',
    status: '',
    minDate: '',
    maxDate: ''
  });
  const [results, setResults] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Estados para o Modal
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleSearch = async () => {
    setIsLoading(true);
    try {
      const formattedFilters = {
        title: filters.title || null,
        status: filters.status || null,
        minDate: filters.minDate ? new Date(filters.minDate).toISOString() : null,
        maxDate: filters.maxDate ? new Date(filters.maxDate).toISOString() : null
      };
      const response = await sharePointApi.searchTasks(formattedFilters);
      setResults(response.data);
    } catch (error) {
      console.error("Erro na busca customizada", error);
    } finally {
      setIsLoading(false);
    }
  };

  const clearFilters = () => {
    setFilters({ title: '', status: '', minDate: '', maxDate: '' });
    setResults([]);
  };

  const handleRowClick = (task: Task) => {
    setSelectedTask(task);
    setIsModalOpen(true);
  };

  const handleModalClose = (updated: boolean) => {
    setIsModalOpen(false);
    if (updated) {
      handleSearch();
    }
  };

  return (
    <div className={styles.container}>
      <div>
        <Title2>Custom Search Lab: CAML Mastery</Title2>
        <Text block>Crie consultas complexas no servidor usando <strong>CAML</strong> dinâmico.</Text>
      </div>

      <div className={styles.grid}>
        
        {/* Painel de Filtros */}
        <Card className={styles.card}>
          <CardHeader 
            header={<Subtitle1>Filtros</Subtitle1>}
            icon={<Filter24Regular />}
          />
          <div style={{ display: 'flex', flexDirection: 'column', gap: '15px', padding: '10px' }}>
            
            <div>
              <Label htmlFor="search-title">Título:</Label>
              <Input 
                id="search-title" 
                style={{ width: '100%' }}
                value={filters.title} 
                onChange={(e, d) => setFilters({...filters, title: d.value})}
              />
            </div>

            <div>
              <Label htmlFor="search-status">Status:</Label>
              <Select 
                id="search-status" 
                style={{ width: '100%' }}
                value={filters.status} 
                onChange={(e, d) => setFilters({...filters, status: d.value})}
              >
                <option value="">Todos</option>
                <option value="Pending">Pending</option>
                <option value="In Progress">In Progress</option>
                <option value="Done">Done</option>
                <option value="Cancelled">Cancelled</option>
              </Select>
            </div>

            <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
              <Button appearance="primary" icon={<Search24Regular />} onClick={handleSearch} disabled={isLoading}>
                Pesquisar
              </Button>
              <Button icon={<Delete24Regular />} onClick={clearFilters} disabled={isLoading}>
                Limpar
              </Button>
            </div>
          </div>
        </Card>

        {/* Tabela de Resultados */}
        <Card className={styles.card}>
          <CardHeader 
            header={<Subtitle1>Resultados ({results.length})</Subtitle1>}
            description="Clique em uma linha para editar."
          />
          
          {isLoading ? <Spinner label="Buscando..." /> : (
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
                  {results.length > 0 ? results.map(item => (
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
                  )) : (
                    <TableRow>
                      <TableCell colSpan={3} style={{ textAlign: 'center', padding: '20px' }}>
                        <Text italic>Nenhum resultado.</Text>
                      </TableCell>
                    </TableRow>
                  )}
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
