import React, { useEffect, useState } from 'react';
import { 
  makeStyles, 
  Title1, 
  Subtitle2, 
  Spinner,
  tokens
} from '@fluentui/react-components';
import { hotelService } from '../services/api';
import { HotelCard } from '../components/HotelCard';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  grid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
    gap: '32px',
    marginTop: '20px',
  },
  header: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  }
});

interface Hotel {
  id: number;
  name: string;
  location: string;
  stars: number;
  description: string;
  imageUrl: string;
}

export const HotelPortfolio: React.FC = () => {
  const styles = useStyles();
  const [hotels, setHotels] = useState<Hotel[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchHotels = async () => {
      try {
        const response = await hotelService.getHotels();
        setHotels(response.data);
      } catch (error) {
        console.error("Erro ao buscar hotéis:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchHotels();
  }, []);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: '100px' }}>
        <Spinner label="Carregando portfólio de luxo..." />
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <Title1>Nossos Hotéis</Title1>
        <Subtitle2 style={{ color: tokens.colorNeutralForeground4 }}>
          Explore e gerencie as propriedades exclusivas do grupo Hospitality Excellence.
        </Subtitle2>
      </div>

      <div className={styles.grid}>
        {hotels.map(hotel => (
          <HotelCard
            key={hotel.id}
            name={hotel.name}
            location={hotel.location}
            stars={hotel.stars}
            description={hotel.description}
            imageUrl={hotel.imageUrl}
            onViewRooms={() => console.log(`Ver quartos do hotel ${hotel.id}`)}
          />
        ))}
      </div>
    </div>
  );
};
