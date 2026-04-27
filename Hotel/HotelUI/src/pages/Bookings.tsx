import React, { useEffect, useState } from 'react';
import { 
  makeStyles, 
  shorthands, 
  Title1, 
  Subtitle2, 
  Spinner,
  tokens,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
  Text
} from '@fluentui/react-components';
import { hotelService } from '../services/api';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  tableContainer: {
    backgroundColor: tokens.colorNeutralBackground1,
    boxShadow: tokens.shadow16,
    ...shorthands.borderRadius(tokens.borderRadiusLarge),
    overflow: 'hidden',
    ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
  },
  headerCell: {
    fontWeight: 'bold',
    backgroundColor: tokens.colorNeutralBackground2,
  }
});

interface Booking {
  id: number;
  bookingCode: string;
  guestName: string;
  checkIn: string;
  checkOut: string;
  totalAmount: number;
  status: string;
}

export const Bookings: React.FC = () => {
  const styles = useStyles();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBookings = async () => {
      try {
        const response = await hotelService.getBookings();
        setBookings(response.data);
      } catch (error) {
        console.error("Erro ao buscar reservas:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchBookings();
  }, []);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Confirmed': return 'informative';
      case 'CheckedIn': return 'brand';
      case 'Cancelled': return 'danger';
      case 'CheckedOut': return 'success';
      default: return 'subtle';
    }
  };

  return (
    <div className={styles.root}>
      <div>
        <Title1>Livro de Reservas</Title1>
        <Subtitle2 block style={{ color: tokens.colorNeutralForeground4 }}>
          Histórico completo e agendamentos futuros.
        </Subtitle2>
      </div>

      <div className={styles.tableContainer}>
        {loading ? <Spinner label="Carregando reservas..." style={{ padding: '40px' }} /> : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell className={styles.headerCell}>Código</TableHeaderCell>
                <TableHeaderCell className={styles.headerCell}>Hóspede</TableHeaderCell>
                <TableHeaderCell className={styles.headerCell}>Check-In</TableHeaderCell>
                <TableHeaderCell className={styles.headerCell}>Check-Out</TableHeaderCell>
                <TableHeaderCell className={styles.headerCell}>Valor</TableHeaderCell>
                <TableHeaderCell className={styles.headerCell}>Status</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {bookings.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} style={{ textAlign: 'center', padding: '20px' }}>
                    Nenhuma reserva encontrada.
                  </TableCell>
                </TableRow>
              ) : bookings.map((booking) => (
                <TableRow key={booking.id}>
                  <TableCell><Text weight="semibold">{booking.bookingCode}</Text></TableCell>
                  <TableCell>{booking.guestName}</TableCell>
                  <TableCell>{new Date(booking.checkIn).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(booking.checkOut).toLocaleDateString()}</TableCell>
                  <TableCell>R$ {booking.totalAmount.toLocaleString()}</TableCell>
                  <TableCell>
                    <Badge appearance="filled" color={getStatusColor(booking.status)}>
                      {booking.status}
                    </Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>
    </div>
  );
};
