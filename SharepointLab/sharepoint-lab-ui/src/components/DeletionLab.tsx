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
  Label,
  Select,
  makeStyles
} from '@fluentui/react-components';
import { Timer24Regular, Delete24Regular, Filter24Regular, Warning24Regular } from '@fluentui/react-icons';
import { sharePointApi } from '../services/api';

const useStyles = makeStyles({
  grid: {
    display: 'grid',
    gridTemplateColumns: '1fr',
    gap: '20px',
    [`@media (min-width: 900px)`]: {
      gridTemplateColumns: '400px 1fr',
    },
  }
});

export const DeletionLab: React.FC = () => {
  const styles = useStyles();
  const [filters, setFilters] = useState({
    title: '',
    status: '',
    minDate: '',
    maxDate: ''
  });
  const [result, setResult] = useState<{ time: number, count: number } | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleDeleteByFilter = async () => {
    if (!window.confirm("Remover TODOS os itens correspondentes?")) return;

    setIsLoading(true);
    try {
      const formattedFilters = {
        title: filters.title || null,
        status: filters.status || null,
        minDate: filters.minDate ? new Date(filters.minDate).toISOString() : null,
        maxDate: filters.maxDate ? new Date(filters.maxDate).toISOString() : null
      };
      const response = await sharePointApi.deleteByFilter(formattedFilters);
      const { elapsedMs, count } = response.data;
      setResult({ time: elapsedMs, count });
    } catch (error) {
      console.error("Erro na exclusão", error);
    } finally {
      setIsLoading(false);
    }
  };

  const clearFilters = () => {
    setFilters({ title: '', status: '', minDate: '', maxDate: '' });
    setResult(null);
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
      <div>
        <Title2>Deletion Lab: Bulk Cleanup</Title2>
        <Text block>Remova itens em massa combinando <strong>CAML Queries</strong> com <strong>Batching</strong>.</Text>
      </div>

      <div className={styles.grid}>
        
        {/* Painel de Filtros */}
        <Card>
          <CardHeader 
            header={<Subtitle1>Critérios</Subtitle1>}
            image={<Filter24Regular />}
          />
          <div style={{ display: 'flex', flexDirection: 'column', gap: '15px', padding: '10px' }}>
            
            <div>
              <Label htmlFor="del-title">Título:</Label>
              <Input 
                id="del-title" 
                style={{ width: '100%' }}
                value={filters.title} 
                onChange={(e, d) => setFilters({...filters, title: d.value})}
              />
            </div>

            <div>
              <Label htmlFor="del-status">Status:</Label>
              <Select 
                id="del-status" 
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
              <Button 
                appearance="primary" 
                icon={<Delete24Regular />} 
                onClick={handleDeleteByFilter} 
                disabled={isLoading}
                style={{ backgroundColor: tokens.colorPaletteRedBackground3 }}
              >
                Excluir
              </Button>
              <Button onClick={clearFilters} disabled={isLoading}>Limpar</Button>
            </div>
          </div>
        </Card>

        {/* Status */}
        <Card>
          <CardHeader header={<Subtitle1>Métricas</Subtitle1>} />
          
          <div style={{ padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '20px', minHeight: '150px' }}>
            {isLoading ? (
              <Spinner label="Deletando..." />
            ) : result ? (
              <>
                <div style={{ display: 'flex', gap: '15px', flexWrap: 'wrap', justifyContent: 'center' }}>
                  <Badge appearance="filled" color="danger" size="extra-large" icon={<Delete24Regular />}>
                    {result.count} Removidos
                  </Badge>
                  <Badge appearance="filled" color="important" size="extra-large" icon={<Timer24Regular />}>
                    {result.time}ms
                  </Badge>
                </div>
              </>
            ) : (
              <div style={{ textAlign: 'center' }}>
                <Warning24Regular style={{ fontSize: '48px', color: tokens.colorNeutralForeground4 }} />
                <Text block italic>Aguardando critérios...</Text>
              </div>
            )}
          </div>
        </Card>

      </div>
    </div>
  );
};
