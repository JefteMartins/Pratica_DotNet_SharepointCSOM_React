import { FluentProvider, webLightWithCustomTokens, teamsLightTheme, tokens } from '@fluentui/react-components';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';

// Customizando o tema conforme o plano
const hotelTheme = {
  ...teamsLightTheme,
  colorBrandBackground: '#0078d4', // Exemplo de cor corporativa
  borderRadiusLarge: '12px',
};

function App() {
  return (
    <FluentProvider theme={hotelTheme}>
      <Router>
        <div className="app-container">
          <aside className="sidebar">
            <nav>
              <h2>Hotel Admin</h2>
              <ul>
                <li>Dashboard</li>
                <li>Hotéis</li>
                <li>Reservas</li>
              </ul>
            </nav>
          </aside>
          <main className="content">
            <header>
              <h1>Bem-vindo ao Hotel Management System</h1>
            </header>
            <Routes>
              <Route path="/" element={<div>Dashboard Page (Em breve)</div>} />
            </Routes>
          </main>
        </div>
      </Router>
    </FluentProvider>
  );
}

export default App;
