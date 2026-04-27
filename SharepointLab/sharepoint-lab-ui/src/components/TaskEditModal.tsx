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
  Select, 
  Textarea,
  Spinner,
  tokens
} from '@fluentui/react-components';
import { sharePointApi } from '../services/api';

interface Task {
  id: number;
  title: string;
  status: string;
  description?: string;
  dueDate?: string;
}

interface TaskEditModalProps {
  task: Task | null;
  isOpen: boolean;
  onClose: (updated: boolean) => void;
}

export const TaskEditModal: React.FC<TaskEditModalProps> = ({ task, isOpen, onClose }) => {
  const [formData, setFormData] = useState<Task | null>(null);
  const [isSaving, setIsLoading] = useState(false);

  useEffect(() => {
    if (task) {
      setFormData({ ...task });
    }
  }, [task]);

  if (!formData) return null;

  const handleSave = async () => {
    setIsLoading(true);
    try {
      await sharePointApi.updateTask({
        id: formData.id,
        title: formData.title,
        status: formData.status,
        description: formData.description,
        dueDate: formData.dueDate ? new Date(formData.dueDate).toISOString() : null
      });
      onClose(true);
    } catch (error) {
      console.error("Erro ao atualizar tarefa", error);
      alert("Erro ao salvar alterações no SharePoint.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={() => onClose(false)}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>Editar Tarefa (ID: {formData.id})</DialogTitle>
          <DialogContent style={{ display: 'flex', flexDirection: 'column', gap: '15px', paddingTop: '20px' }}>
            
            <div>
              <Label htmlFor="edit-title" required>Título:</Label>
              <Input 
                id="edit-title" 
                style={{ width: '100%' }}
                value={formData.title} 
                onChange={(e, d) => setFormData({...formData, title: d.value})}
              />
            </div>

            <div>
              <Label htmlFor="edit-status">Status:</Label>
              <Select 
                id="edit-status" 
                style={{ width: '100%' }}
                value={formData.status} 
                onChange={(e, d) => setFormData({...formData, status: d.value})}
              >
                <option value="Pending">Pending</option>
                <option value="In Progress">In Progress</option>
                <option value="Done">Done</option>
                <option value="Cancelled">Cancelled</option>
              </Select>
            </div>

            <div>
              <Label htmlFor="edit-desc">Descrição:</Label>
              <Textarea 
                id="edit-desc" 
                style={{ width: '100%' }}
                value={formData.description || ''} 
                onChange={(e, d) => setFormData({...formData, description: d.value})}
              />
            </div>

            <div>
              <Label htmlFor="edit-date">Data de Vencimento:</Label>
              <Input 
                id="edit-date" 
                type="date"
                style={{ width: '100%' }}
                value={formData.dueDate ? formData.dueDate.split('T')[0] : ''} 
                onChange={(e, d) => setFormData({...formData, dueDate: d.value})}
              />
            </div>

          </DialogContent>
          <DialogActions>
            <Button appearance="secondary" onClick={() => onClose(false)} disabled={isSaving}>
              Cancelar
            </Button>
            <Button appearance="primary" onClick={handleSave} disabled={isSaving}>
              {isSaving ? <Spinner size="tiny" label="Salvando..." /> : "Salvar Alterações"}
            </Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};
