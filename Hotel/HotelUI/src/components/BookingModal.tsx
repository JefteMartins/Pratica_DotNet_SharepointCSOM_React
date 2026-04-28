import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogContent,
  DialogActions,
  Button,
  Input,
  Label,
  Spinner,
  makeStyles,
  shorthands,
  Text,
  tokens
} from '@fluentui/react-components';
import { hotelService } from '../services/api';

const useStyles = makeStyles({
  field: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    marginBottom: '16px',
  },
  summary: {
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.padding('12px'),
    borderRadius: tokens.borderRadiusMedium,
    marginTop: '10px',
  }
});

interface Room {
  id: number;
  title: string;
  pricePerNight: number;
}

interface BookingModalProps {
  room: Room | null;
  isOpen: boolean;
  initialDates: { checkIn: string; checkOut: string };
  onClose: (success: boolean) => void;
}

export const BookingModal: React.FC<BookingModalProps> = ({ room, isOpen, initialDates, onClose }) => {
  const styles = useStyles();
  const [guestName, setGuestName] = useState('');
  const [checkIn, setCheckIn] = useState(initialDates.checkIn);
  const [checkOut, setCheckOut] = useState(initialDates.checkOut);
  const [isSaving, setIsSaving] = useState(false);

  // Sincroniza datas se mudarem no pai enquanto aberto
  useEffect(() => {
    setCheckIn(initialDates.checkIn);
    setCheckOut(initialDates.checkOut);
  }, [initialDates, isOpen]);

  if (!room) return null;

  const calculateTotal = () => {
    if (!checkIn || !checkOut) return 0;
    const start = new Date(checkIn);
    const end = new Date(checkOut);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays * room.pricePerNight : room.pricePerNight;
  };

  const handleSave = async () => {
    if (!guestName || !checkIn || !checkOut) {
      alert("Please fill in all fields.");
      return;
    }

    setIsSaving(true);
    try {
      await hotelService.createBooking({
        roomId: room.id,
        guestName,
        checkIn: new Date(checkIn).toISOString(),
        checkOut: new Date(checkOut).toISOString(),
        totalAmount: calculateTotal(),
        status: 'Confirmed'
      });
      
      // Opcional: Atualizar o status físico do quarto no SharePoint 
      // se a reserva for para HOJE.
      const today = new Date().toISOString().split('T')[0];
      if (checkIn === today) {
        await hotelService.updateRoomStatus(room.id, 'Occupied');
      }
      
      onClose(true);
    } catch (error: any) {
      console.error("Erro ao criar reserva:", error);
      const message = error.response?.data?.message || "Failed to save booking in SharePoint.";
      alert(message);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={() => !isSaving && onClose(false)}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>Confirm Booking - {room.title}</DialogTitle>
          <DialogContent>
            <div className={styles.field}>
              <Label required>Guest Name</Label>
              <Input value={guestName} onChange={(_, d) => setGuestName(d.value)} placeholder="Full name" />
            </div>

            <div style={{ display: 'flex', gap: '16px' }}>
              <div className={styles.field} style={{ flex: 1 }}>
                <Label>Check-In</Label>
                <Input type="date" value={checkIn} disabled />
              </div>
              <div className={styles.field} style={{ flex: 1 }}>
                <Label>Check-Out</Label>
                <Input type="date" value={checkOut} disabled />
              </div>
            </div>

            <div className={styles.summary}>
              <Text size={200} block>Total for the stay:</Text>
              <Text weight="bold" size={500}>$ {calculateTotal().toLocaleString()}</Text>
              <Text size={100} block color={tokens.colorNeutralForeground4}>
                {Math.ceil(Math.abs(new Date(checkOut).getTime() - new Date(checkIn).getTime()) / (1000 * 3600 * 24))} nights confirmed.
              </Text>
            </div>
          </DialogContent>
          <DialogActions>
            <Button appearance="secondary" onClick={() => onClose(false)} disabled={isSaving}>
              Back
            </Button>
            <Button appearance="primary" onClick={handleSave} disabled={isSaving}>
              {isSaving ? <Spinner size="tiny" label="Syncing..." /> : "Complete Booking"}
            </Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};
