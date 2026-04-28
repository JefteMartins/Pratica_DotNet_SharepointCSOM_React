import React, { useEffect, useState } from 'react';
import { 
  makeStyles, 
  shorthands, 
  Title1, 
  Subtitle2, 
  tokens,
  Card,
  Text,
  LargeTitle
} from '@fluentui/react-components';
import { 
  Building24Regular, 
  CalendarCheckmark24Regular, 
  Money24Regular, 
  PersonAvailable24Regular 
} from '@fluentui/react-icons';
import { hotelService } from '../services/api';
import { DashboardSkeleton } from '../components/PageSkeletons';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '32px',
  },
  statsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))',
    gap: '24px',
  },
  statCard: {
    ...shorthands.padding('24px'),
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
    backgroundColor: tokens.colorNeutralBackground1,
    boxShadow: tokens.shadow16,
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
  },
  iconArea: {
    width: '40px',
    height: '40px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: '8px',
    marginBottom: '8px',
  },
  navyIcon: {
    backgroundColor: 'rgba(0, 30, 66, 0.1)',
    color: '#001E42',
  },
  goldIcon: {
    backgroundColor: 'rgba(197, 160, 89, 0.1)',
    color: '#C5A059',
  }
});

interface DashboardStats {
  totalHotels: number;
  totalBookings: number;
  totalRevenue: number;
  activeBookings: number;
}

export const Dashboard: React.FC = () => {
  const styles = useStyles();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const response = await hotelService.getDashboardStats();
        setStats(response.data);
      } catch (error) {
        console.error("Error fetching statistics:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  return (
    <div className={styles.root}>
      <div>
        <Title1>Management Dashboard</Title1>
        <Subtitle2 block style={{ color: tokens.colorNeutralForeground4 }}>
          Overview of the hotel group performance in real time.
        </Subtitle2>
      </div>

      {loading ? (
        <DashboardSkeleton />
      ) : (
        <div className={styles.statsGrid}>
          <Card className={styles.statCard} appearance="subtle">
            <div className={`${styles.iconArea} ${styles.navyIcon}`}>
              <Building24Regular />
            </div>
            <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>Total Hotels</Text>
            <LargeTitle>{stats?.totalHotels || 0}</LargeTitle>
          </Card>

          <Card className={styles.statCard} appearance="subtle">
            <div className={`${styles.iconArea} ${styles.goldIcon}`}>
              <CalendarCheckmark24Regular />
            </div>
            <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>Bookings Made</Text>
            <LargeTitle>{stats?.totalBookings || 0}</LargeTitle>
          </Card>

          <Card className={styles.statCard} appearance="subtle">
            <div className={`${styles.iconArea} ${styles.navyIcon}`}>
              <PersonAvailable24Regular />
            </div>
            <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>Active Guests</Text>
            <LargeTitle>{stats?.activeBookings || 0}</LargeTitle>
          </Card>

          <Card className={styles.statCard} appearance="subtle">
            <div className={`${styles.iconArea} ${styles.goldIcon}`}>
              <Money24Regular />
            </div>
            <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>Total Revenue</Text>
            <LargeTitle>
              {stats?.totalRevenue ? `$ ${stats.totalRevenue.toLocaleString()}` : '$ 0'}
            </LargeTitle>
          </Card>
        </div>
      )}
    </div>
  );
};

