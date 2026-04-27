import React, { useEffect, useState } from 'react';
import { 
  makeStyles, 
  shorthands, 
  Title1, 
  Subtitle2, 
  Spinner,
  tokens,
  Text,
  Badge,
  Select,
  Label,
  Slider,
  Button,
  Input
} from '@fluentui/react-components';
import { 
  ArrowRight24Regular, 
  Star24Filled
} from '@fluentui/react-icons';
import { hotelService } from '../services/api';
import { BookingModal } from '../components/BookingModal';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  filterBar: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
    gap: '20px',
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.padding('24px'),
    borderRadius: tokens.borderRadiusLarge,
    boxShadow: tokens.shadow4,
  },
  filterItem: {
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
  },
  roomGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
    gap: '24px',
  },
  roomCard: {
    display: 'flex',
    flexDirection: 'row',
    ...shorthands.padding(0),
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    overflow: 'hidden',
    backgroundColor: tokens.colorNeutralBackground1,
    transition: 'all 0.2s',
    ':hover': {
      boxShadow: tokens.shadow16,
      ...shorthands.borderColor(tokens.colorBrandStroke1),
    }
  },
  statusBar: {
    width: '8px',
    height: '100%',
  },
  cardContent: {
    ...shorthands.padding('16px'),
    flexGrow: 1,
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
  },
  cardHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  statusAvailable: { backgroundColor: tokens.colorPaletteGreenBackground3 },
  statusOccupied: { backgroundColor: tokens.colorPaletteBlueBackground2 },
  statusMaintenance: { backgroundColor: tokens.colorPaletteDarkOrangeBackground3 },
  statusCleaning: { backgroundColor: tokens.colorPaletteYellowBackground3 },
  goldStar: { color: '#C5A059' }
});

interface Room {
  id: number;
  title: string;
  roomType: string;
  pricePerNight: number;
  status: string;
  hotelId: number;
  hotelStars: number;
}

interface Booking {
  roomId: number;
  checkIn: string;
  checkOut: string;
  status: string;
}

interface Hotel {
  id: number;
  name: string;
}

export const RoomManagement: React.FC = () => {
  const styles = useStyles();
  const [allRooms, setAllRooms] = useState<Room[]>([]);
  const [allBookings, setAllBookings] = useState<Booking[]>([]);
  const [filteredRooms, setFilteredRooms] = useState<Room[]>([]);
  const [hotels, setHotels] = useState<Hotel[]>([]);
  const [loading, setLoading] = useState(true);

  // Controle do Modal
  const [selectedRoom, setSelectedRoom] = useState<Room | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Estados dos Filtros
  const [filterHotel, setFilterHotel] = useState<string>('0');
  const [filterStars, setFilterStars] = useState<string>('0');
  const [maxPrice, setMaxPrice] = useState<number>(3000);
  const [checkIn, setCheckIn] = useState<string>('');
  const [checkOut, setCheckOut] = useState<string>('');

  const handleCheckInChange = (date: string) => {
    setCheckIn(date);
    // Se a nova data de check-in for maior ou igual ao check-out atual, ajusta o check-out para o dia seguinte
    if (checkOut && new Date(date) >= new Date(checkOut)) {
      const nextDay = new Date(date);
      nextDay.setDate(nextDay.getDate() + 1);
      setCheckOut(nextDay.toISOString().split('T')[0]);
    }
  };

  const handleCheckOutChange = (date: string) => {
    if (checkIn && new Date(date) <= new Date(checkIn)) {
      alert("A data de saída deve ser posterior à data de entrada.");
      return;
    }
    setCheckOut(date);
  };

  const fetchData = async () => {
    setLoading(true);
    try {
      const [hotelsRes, roomsRes, bookingsRes] = await Promise.all([
        hotelService.getHotels(),
        hotelService.getAllRooms(),
        hotelService.getBookings()
      ]);
      setHotels(hotelsRes.data);
      setAllRooms(roomsRes.data);
      setAllBookings(bookingsRes.data);
    } catch (error) {
      console.error("Erro ao carregar dados:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Lógica de Filtragem Reativa (Incluindo Datas)
  useEffect(() => {
    let results = allRooms;

    // 1. Filtro de Hotel
    if (filterHotel !== '0') {
      results = results.filter(r => r.hotelId === Number(filterHotel));
    }

    // 2. Filtro de Estrelas
    if (filterStars !== '0') {
      results = results.filter(r => r.hotelStars === Number(filterStars));
    }

    // 3. Filtro de Preço
    results = results.filter(r => r.pricePerNight <= maxPrice);

    // 4. Filtro de Disponibilidade por Datas (O MAIS IMPORTANTE)
    if (checkIn && checkOut) {
      const requestedStart = new Date(checkIn);
      const requestedEnd = new Date(checkOut);

      results = results.filter(room => {
        // Primeiro, verifica se o quarto não está em manutenção/limpeza
        if (room.status === 'Maintenance' || room.status === 'Cleaning') return false;

        // Segundo, verifica se há conflito com reservas existentes
        const hasConflict = allBookings.some(booking => {
          if (booking.roomId !== room.id || booking.status === 'Cancelled') return false;
          
          const existingStart = new Date(booking.checkIn);
          const existingEnd = new Date(booking.checkOut);

          // Lógica de intersecção: (Início1 < Fim2) AND (Fim1 > Início2)
          return requestedStart < existingEnd && requestedEnd > existingStart;
        });

        return !hasConflict;
      });
    }

    setFilteredRooms(results);
  }, [filterHotel, filterStars, maxPrice, checkIn, checkOut, allRooms, allBookings]);

  const handleBookingClose = (success: boolean) => {
    setIsModalOpen(false);
    setSelectedRoom(null);
    if (success) fetchData(); // Recarregar tudo após reserva
  };

  const getStatusClass = (status: string) => {
    switch (status) {
      case 'Available': return styles.statusAvailable;
      case 'Occupied': return styles.statusOccupied;
      case 'Maintenance': return styles.statusMaintenance;
      case 'Cleaning': return styles.statusCleaning;
      default: return '';
    }
  };

  if (loading) return <Spinner label="Verificando disponibilidade em tempo real..." style={{ marginTop: '100px' }} />;

  return (
    <div className={styles.root}>
      <div>
        <Title1>Busca de Acomodações</Title1>
        <Subtitle2 block style={{ color: tokens.colorNeutralForeground4 }}>
          Selecione as datas para encontrar os quartos disponíveis em nossa rede.
        </Subtitle2>
      </div>

      {/* Barra de Filtros Reformulada */}
      <div className={styles.filterBar}>
        <div className={styles.filterItem}>
          <Label weight="semibold">Check-In</Label>
          <Input type="date" value={checkIn} onChange={(_, d) => handleCheckInChange(d.value)} />
        </div>

        <div className={styles.filterItem}>
          <Label weight="semibold">Check-Out</Label>
          <Input type="date" value={checkOut} onChange={(_, d) => handleCheckOutChange(d.value)} />
        </div>

        <div className={styles.filterItem}>
          <Label weight="semibold">Hotel</Label>
          <Select value={filterHotel} onChange={(_, d) => setFilterHotel(d.value)}>
            <option value="0">Todos os Hotéis</option>
            {hotels.map(h => <option key={h.id} value={h.id}>{h.name}</option>)}
          </Select>
        </div>

        <div className={styles.filterItem}>
          <Label weight="semibold">Classificação</Label>
          <Select value={filterStars} onChange={(_, d) => setFilterStars(d.value)}>
            <option value="0">Qualquer Estrela</option>
            <option value="4">4+ Estrelas</option>
            <option value="5">Apenas 5 Estrelas</option>
          </Select>
        </div>

        <div className={styles.filterItem}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <Label weight="semibold">Preço Máx</Label>
            <Text size={200} weight="bold">R$ {maxPrice}</Text>
          </div>
          <Slider min={200} max={5000} step={100} value={maxPrice} onChange={(_, d) => setMaxPrice(d.value)} />
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
        <Text weight="semibold" size={400}>{filteredRooms.length} opções disponíveis</Text>
        {(!checkIn || !checkOut) && (
          <Badge appearance="outline" color="warning">Selecione datas para filtrar disponibilidade real</Badge>
        )}
      </div>

      {/* Grid de Quartos */}
      <div className={styles.roomGrid}>
        {filteredRooms.map(room => (
          <div key={room.id} className={styles.roomCard}>
            <div className={`${styles.statusBar} ${getStatusClass(room.status)}`} />
            <div className={styles.cardContent}>
              <div className={styles.cardHeader}>
                <div>
                  <Text weight="bold" size={400}>{room.title}</Text>
                  <div style={{ display: 'flex', gap: '4px', marginTop: '2px' }}>
                    {[...Array(room.hotelStars)].map((_, i) => (
                      <Star24Filled key={i} className={styles.goldStar} style={{ fontSize: '12px' }} />
                    ))}
                  </div>
                </div>
                <Badge appearance="tint" color="success">Livre</Badge>
              </div>
              
              <Text size={200} color={tokens.colorNeutralForeground3}>{room.roomType}</Text>
              
              <div style={{ flexGrow: 1 }} />
              
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginTop: '12px' }}>
                <div>
                  <Text size={100} block color={tokens.colorNeutralForeground4}>Diária</Text>
                  <Text weight="bold" size={500}>R$ {room.pricePerNight.toLocaleString()}</Text>
                </div>
                <Button 
                  appearance="primary" 
                  icon={<ArrowRight24Regular />}
                  disabled={!checkIn || !checkOut}
                  onClick={() => {
                    setSelectedRoom(room);
                    setIsModalOpen(true);
                  }}
                >
                  Reservar
                </Button>
              </div>
            </div>
          </div>
        ))}
      </div>

      <BookingModal 
        isOpen={isModalOpen} 
        room={selectedRoom} 
        initialDates={{ checkIn, checkOut }}
        onClose={handleBookingClose} 
      />
    </div>
  );
};
