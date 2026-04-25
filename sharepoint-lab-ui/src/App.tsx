import { useState } from 'react';
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
  Spinner
} from '@fluentui/react-components';
import { Database24Regular, ReadingList24Regular, ArrowSync24Regular, Edit24Regular } from '@fluentui/react-icons';
import { ReadingLab } from './components/ReadingLab';
import { WritingLab } from './components/WritingLab';
import { sharePointApi } from './services/api';

function App() {
  const [selectedTab, setSelectedTab] = useState<'reading' | 'writing' | 'data'>('reading');
  const [seeding, setSeeding] = useState(false);
  const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' | 'info' } | null>(null);

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
      <div style={{ 
        minHeight: '100vh', 
        backgroundColor: tokens.colorNeutralBackground3, // Fundo cinza claro global
        padding: '40px 20px' 
      }}>
        <div style={{ 
          maxWidth: '1200px', 
          margin: '0 auto', 
          backgroundColor: tokens.colorNeutralBackground1, // Fundo branco para o conteúdo
          padding: '40px',
          borderRadius: tokens.borderRadiusXLarge,
          boxShadow: tokens.shadow16,
          minHeight: '80vh'
        }}>
          
          <header style={{ marginBottom: '40px' }}>
            <LargeTitle block style={{ color: tokens.colorBrandForeground1 }}>
              SharePoint CSOM <span style={{ fontWeight: tokens.fontWeightRegular }}>Performance Lab</span>
            </LargeTitle>
            <Text size={400} style={{ color: tokens.colorNeutralForeground3 }}>
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

        <TabList 
          selectedValue={selectedTab} 
          onTabSelect={(_, data) => setSelectedTab(data.value as any)}
          style={{ marginBottom: '30px' }}
        >
          <Tab value="reading" icon={<ReadingList24Regular />}>The Reading Lab</Tab>
          <Tab value="writing" icon={<Edit24Regular />}>The Writing Lab</Tab>
          <Tab value="data" icon={<Database24Regular />}>Data Management (Seed)</Tab>
        </TabList>

        <main>
          {selectedTab === 'reading' && <ReadingLab />}
          
          {selectedTab === 'writing' && <WritingLab />}
          
          {selectedTab === 'data' && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              <Title1>Gerenciamento de Dados</Title1>
              <Text>Para testar o Threshold e a performance de paginação, você precisa de uma massa de dados significativa.</Text>
              
              <div style={{ display: 'flex', gap: '10px', marginTop: '10px' }}>
                <Button disabled={seeding} onClick={() => handleSeed(100)}>Seed 100 Itens</Button>
                <Button disabled={seeding} onClick={() => handleSeed(1000)}>Seed 1.000 Itens</Button>
                <Button 
                  disabled={seeding} 
                  appearance="primary" 
                  icon={seeding ? <Spinner size="tiny" /> : <ArrowSync24Regular />} 
                  onClick={() => handleSeed(5000)}
                >
                  Seed 5.000 Itens (Threshold)
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
