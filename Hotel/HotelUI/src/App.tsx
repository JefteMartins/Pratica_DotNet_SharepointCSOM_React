import { 
  FluentProvider, 
  makeStyles, 
  shorthands, 
  tokens,
  Title2,
  Caption1
} from '@fluentui/react-components';
import { 
  Home24Regular, 
  Building24Regular, 
  CalendarMonth24Regular, 
  Flash24Regular
} from '@fluentui/react-icons';
import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import { hotelTheme } from './theme';
import { HotelPortfolio } from './pages/HotelPortfolio';
import { Dashboard } from './pages/Dashboard';
import { RoomManagement } from './pages/RoomManagement';
import { Bookings } from './pages/Bookings';
import { LabDashboard } from './pages/LabDashboard';
import './App.css';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    height: '100vh',
    width: '100vw',
    backgroundColor: '#faf9fc', // Grey10 do design
  },
  sidebar: {
    width: '280px',
    minWidth: '280px',
    flexShrink: 0,
    backgroundColor: '#001E42', // Navy Primário
    color: '#ffffff',
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.padding('40px', '20px'),
  },
  logoArea: {
    marginBottom: '40px',
    ...shorthands.padding('0', '10px'),
  },
  nav: {
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
  },
  navItem: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    ...shorthands.padding('12px', '16px'),
    textDecorationLine: 'none',
    color: 'rgba(255, 255, 255, 0.7)',
    borderRadius: tokens.borderRadiusMedium,
    transition: 'all 0.2s',
    ':hover': {
      backgroundColor: 'rgba(255, 255, 255, 0.1)',
      color: '#ffffff',
    },
  },
  navItemActive: {
    backgroundColor: '#C5A059', // Gold secundário
    color: '#001E42',
    fontWeight: 'bold',
    ':hover': {
      backgroundColor: '#d4b47a',
      color: '#001E42',
    }
  },
  content: {
    flexGrow: 1,
    minWidth: 0,
    overflowX: 'auto',
    overflowY: 'auto',
    ...shorthands.padding('32px', '40px'),
  },
});

const NavItem = ({ to, icon: Icon, label }: { to: string, icon: any, label: string }) => {
  const styles = useStyles();
  const location = useLocation();
  const isActive = location.pathname === to || (to !== '/' && location.pathname.startsWith(to));

  return (
    <Link to={to} className={`${styles.navItem} ${isActive ? styles.navItemActive : ''}`}>
      <Icon />
      <span>{label}</span>
    </Link>
  );
};

const Sidebar = () => {
  const styles = useStyles();
  return (
    <aside className={styles.sidebar}>
      <div className={styles.logoArea}>
        <Title2 style={{ color: '#ffffff', letterSpacing: '-0.02em' }}>EXCELLENCE</Title2>
        <Caption1 style={{ color: '#C5A059', display: 'block', fontWeight: 'bold' }}>HOTEL MANAGEMENT</Caption1>
      </div>
      <nav className={styles.nav}>
        <NavItem to="/" icon={Home24Regular} label="Dashboard" />
        <NavItem to="/hotels" icon={Building24Regular} label="Hotels" />
        <NavItem to="/rooms" icon={Building24Regular} label="Rooms" />
        <NavItem to="/bookings" icon={CalendarMonth24Regular} label="Bookings" />
        <NavItem to="/lab" icon={Flash24Regular} label="Technical Lab" />
      </nav>
    </aside>
  );
};

function App() {
  const styles = useStyles();

  return (
    <FluentProvider theme={hotelTheme}>
      <Router>
        <div className={styles.container}>
          <Sidebar />
          <main className={styles.content}>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/hotels" element={<HotelPortfolio />} />
              <Route path="/rooms" element={<RoomManagement />} />
              <Route path="/bookings" element={<Bookings />} />
              <Route path="/lab/*" element={<LabDashboard />} />
              <Route path="/settings" element={<div style={{ padding: '20px' }}><Title2>Settings</Title2><Caption1>Coming soon...</Caption1></div>} />
            </Routes>
          </main>
        </div>
      </Router>
    </FluentProvider>
  );
}

export default App;
