import { useState, useEffect } from 'react';
import { 
  FluentProvider, 
  webLightTheme, 
  TabList, 
  Tab, 
  Title1, 
  Text,
  LargeTitle,
  tokens,
  MessageBar,
  MessageBarTitle,
  MessageBarBody,
  Button,
  Spinner,
  makeStyles,
  shorthands
} from '@fluentui/react-components';
import { Database24Regular, ReadingList24Regular, ArrowSync24Regular, Edit24Regular, ShieldCheckmark24Regular, Search24Regular, Delete24Regular } from '@fluentui/react-icons';
import { ReadingLab } from './components/ReadingLab';
import { WritingLab } from './components/WritingLab';
import { ResilienceLab } from './components/ResilienceLab';
import { CustomSearchLab } from './components/CustomSearchLab';
import { DeletionLab } from './components/DeletionLab';
import { sharePointApi } from './services/api';

const useStyles = makeStyles({
  container: {
    minHeight: '100vh',
    backgroundColor: tokens.colorNeutralBackground3,
    ...shorthands.padding('20px', '10px'),
    [`@media (min-width: 600px)`]: {
      ...shorthands.padding('40px', '20px'),
    },
  },
  contentCard: {
    maxWidth: '1200px',
    marginRight: 'auto',
    marginLeft: 'auto',
    backgroundColor: tokens.colorNeutralBackground1,
    ...shorthands.padding('20px'),
    ...shorthands.borderRadius(tokens.borderRadiusXLarge),
    boxShadow: tokens.shadow16,
    minHeight: '80vh',
    [`@media (min-width: 600px)`]: {
      ...shorthands.padding('40px'),
    },
  },
  header: {
    marginBottom: '30px',
    [`@media (min-width: 600px)`]: {
      marginBottom: '40px',
    },
  },
  tabList: {
    marginBottom: '30px',
    // Permite scroll horizontal se as abas não couberem
    overflowX: 'auto',
    whiteSpace: 'nowrap',
    '-webkit-overflow-scrolling': 'touch',
    '&::-webkit-scrollbar': {
      display: 'none',
    },
    ...shorthands.margin('0', '0', '30px', '0'),
  }
});

function App() {
  const styles = useStyles();
  const [selectedTab, setSelectedTab] = useState<'reading' | 'writing' | 'resilience' | 'search' | 'deletion' | 'data'>('reading');
  const [seeding, setSeeding] = useState(false);
  const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' | 'info' } | null>(null);
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const handleSeed = async (count: number) => {
    setSeeding(true);
    setStatus({ message: `Gerando ${count} tarefas no SharePoint...`, type: 'info' });
    try {
      await sharePointApi.seedData(count);
      setStatus({ message: `Sucesso! ${count} tarefas foram criadas.`, type: 'success' });
    } catch (error) {
      console.error(error);
      setStatus({ message: 'Falha ao injetar dados.', type: 'error' });
    } finally {
      setSeeding(false);
    }
  };

  return (
    <FluentProvider theme={webLightTheme}>
      <div className={styles.container}>
        <div className={styles.contentCard}>
          
          <header className={styles.header}>
            <LargeTitle block style={{ color: tokens.colorBrandForeground1, fontSize: isMobile ? '24px' : undefined }}>
              SharePoint CSOM <span style={{ fontWeight: tokens.fontWeightRegular }}>Performance Lab</span>
            </LargeTitle>
            <Text size={isMobile ? 300 : 400} style={{ color: tokens.colorNeutralForeground3 }}>
              Ambiente de testes para técnicas avançadas de leitura e escrita em listas de grande volume.
            </Text>
          </header>

        {status && (
          <MessageBar intent={status.type} style={{ marginBottom: '20px' }}>
            <MessageBarBody>
              <MessageBarTitle>{status.message}</MessageBarTitle>
            </MessageBarBody>
          </MessageBar>
        )}

        <div className={styles.tabList}>
          <TabList 
            selectedValue={selectedTab} 
            onTabSelect={(_, data) => setSelectedTab(data.value as any)}
            vertical={false} // Mantém horizontal mas com scroll se necessário
          >
            <Tab value="reading" icon={<ReadingList24Regular />}>{isMobile ? "Read" : "The Reading Lab"}</Tab>
            <Tab value="writing" icon={<Edit24Regular />}>{isMobile ? "Write" : "The Writing Lab"}</Tab>
            <Tab value="deletion" icon={<Delete24Regular />}>{isMobile ? "Delete" : "The Deletion Lab"}</Tab>
            <Tab value="resilience" icon={<ShieldCheckmark24Regular />}>{isMobile ? "Resilience" : "The Resilience Lab"}</Tab>
            <Tab value="search" icon={<Search24Regular />}>{isMobile ? "Search" : "The Search Lab"}</Tab>
            <Tab value="data" icon={<Database24Regular />}>{isMobile ? "Data" : "Data Management"}</Tab>
          </TabList>
        </div>

        <main>
          {selectedTab === 'reading' && <ReadingLab />}
          
          {selectedTab === 'writing' && <WritingLab />}

          {selectedTab === 'deletion' && <DeletionLab />}

          {selectedTab === 'resilience' && <ResilienceLab />}

          {selectedTab === 'search' && <CustomSearchLab />}
          
          {selectedTab === 'data' && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              <Title1>Gerenciamento de Dados</Title1>
              <Text>Para testar o Threshold e a performance de paginação, você precisa de uma massa de dados significativa.</Text>
              
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '10px', marginTop: '10px' }}>
                <Button disabled={seeding} onClick={() => handleSeed(100)}>Seed 100</Button>
                <Button disabled={seeding} onClick={() => handleSeed(1000)}>Seed 1.000</Button>
                <Button 
                  disabled={seeding} 
                  appearance="primary" 
                  icon={seeding ? <Spinner size="tiny" /> : <ArrowSync24Regular />} 
                  onClick={() => handleSeed(5000)}
                >
                  Seed 5.000 (Threshold)
                </Button>
              </div>
              
              {seeding && <Spinner label="Gerando dados em lotes de 100 para evitar timeout..." />}
            </div>
          )}
        </main>
      </div>
     </div>
    </FluentProvider>
  );
}

export default App;
