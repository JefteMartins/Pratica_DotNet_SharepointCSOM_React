import { useState, useEffect } from 'react'
import './App.css'
import { sharePointApi } from './services/api'

interface Task {
  id: number;
  title: string;
  description: string;
  status: string;
  dueDate: string;
}

function App() {
  const [tasks, setTasks] = useState<Task[]>([])
  const [loading, setLoading] = useState(false)
  const [seeding, setSeeding] = useState(false)
  const [message, setMessage] = useState('')

  const fetchTasks = async () => {
    setLoading(true)
    try {
      const response = await sharePointApi.getTasks()
      console.log('Dados recebidos do SharePoint:', response.data)
      
      setTasks(response.data)
      setMessage(`Carregadas ${response.data.length} tarefas.`)
    } catch (error: any) {
      console.error('Erro na requisição:', error)
      setMessage(`Erro: ${error.response?.data || 'Falha ao conectar na API'}`)
    } finally {
      setLoading(false)
    }
  }

  const handleSeed = async (count: number) => {
    setSeeding(true)
    setMessage(`Gerando ${count} tarefas... aguarde...`)
    try {
      await sharePointApi.seedData(count)
      setMessage(`Sucesso! ${count} tarefas geradas.`)
      fetchTasks()
    } catch (error) {
      console.error(error)
      setMessage('Erro ao gerar dados.')
    } finally {
      setSeeding(false)
    }
  }

  useEffect(() => {
    fetchTasks()
  }, [])

  return (
    <div className="lab-container">
      <header className="lab-header">
        <h1>SharePoint <span className="highlight">Performance Lab</span></h1>
        <div className="status-bar">
          <span className={`badge ${loading ? 'loading' : 'ready'}`}>
            {loading ? 'Sincronizando...' : 'Online'}
          </span>
          <p>{message}</p>
        </div>
      </header>

      <main className="lab-content">
        <section className="control-panel">
          <h2>Controles de Carga</h2>
          <p>Use os botões abaixo para injetar dados no SharePoint e testar a performance do CSOM.</p>
          <div className="button-group">
            <button 
              disabled={seeding || loading} 
              onClick={() => handleSeed(10)}
              className="btn-seed"
            >
              Seed 10 Itens
            </button>
            <button 
              disabled={seeding || loading} 
              onClick={() => handleSeed(100)}
              className="btn-seed primary"
            >
              Seed 100 Itens
            </button>
            <button 
              disabled={seeding || loading} 
              onClick={() => handleSeed(1000)}
              className="btn-seed danger"
            >
              Seed 1000 Itens
            </button>
            <button onClick={fetchTasks} className="btn-refresh">
              Atualizar Lista
            </button>
          </div>
        </section>

        <section className="data-section">
          <div className="section-header">
            <h2>Itens no SharePoint</h2>
            <span className="count-tag">{tasks.length} itens</span>
          </div>
          
          <div className="table-wrapper">
            {loading && tasks.length === 0 ? (
              <div className="loader">Carregando dados do SharePoint...</div>
            ) : (
              <table className="task-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Título</th>
                    <th>Status</th>
                    <th>Vencimento</th>
                  </tr>
                </thead>
                <tbody>
                  {tasks.length > 0 ? tasks.map(task => (
                    <tr key={task.id}>
                      <td>{task.id}</td>
                      <td className="task-title">{task.title}</td>
                      <td>
                        <span className={`status-pill ${task.status.toLowerCase()}`}>
                          {task.status}
                        </span>
                      </td>
                      <td>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-'}</td>
                    </tr>
                  )) : (
                    <tr>
                      <td colSpan={4} className="empty-state">Nenhum item encontrado. Use o Seed para começar.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            )}
          </div>
        </section>
      </main>
    </div>
  )
}

export default App
