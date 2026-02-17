import React, { useState } from 'react';
import type { RoomDto, RoomCreateDto, RoomUpdateDto } from '../types';
import Button from './Button';
import Input from './Input';
import { toast } from 'react-toastify';
import { roomsService } from '../api/rooms';

interface RoomModalProps {
    isOpen: boolean;
    room?: RoomDto | null;
    onClose: () => void;
    onSuccess: () => void;
}

const RoomModal: React.FC<RoomModalProps> = ({ isOpen, room, onClose, onSuccess }) => {
    if (!isOpen) return null;

    const isEdit = !!room;
    const [formData, setFormData] = useState({
        name: room?.name || '',
        capacity: room?.capacity || 10,
        location: room?.location || '',
        hasProjector: room?.hasProjector || false
    });
    const [isLoading, setIsLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            if (isEdit && room) {
                await roomsService.update(room.id, formData as RoomUpdateDto);
                toast.success('Room updated');
            } else {
                await roomsService.create(formData as RoomCreateDto);
                toast.success('Room created');
            }
            onSuccess();
            onClose();
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Failed to save room');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal" onClick={e => e.stopPropagation()}>
                <div className="modal-header">
                    <h3 className="modal-title">{isEdit ? 'Edit Room' : 'Add New Room'}</h3>
                    <button className="modal-close" onClick={onClose}>
                        <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>
                <form onSubmit={handleSubmit}>
                    <div className="modal-body">
                        <Input
                            label="Room Name"
                            placeholder="e.g. Meeting Room A"
                            value={formData.name}
                            onChange={e => setFormData({ ...formData, name: e.target.value })}
                            required
                        />
                        <Input
                            label="Location"
                            placeholder="e.g. Floor 3, Building A"
                            value={formData.location}
                            onChange={e => setFormData({ ...formData, location: e.target.value })}
                            required
                        />
                        <Input
                            label="Capacity"
                            type="number"
                            min={1}
                            value={formData.capacity}
                            onChange={e => setFormData({ ...formData, capacity: parseInt(e.target.value) || 1 })}
                            required
                        />
                        <label className="form-checkbox">
                            <input
                                type="checkbox"
                                checked={formData.hasProjector}
                                onChange={e => setFormData({ ...formData, hasProjector: e.target.checked })}
                            />
                            <span>Has Projector</span>
                        </label>
                    </div>
                    <div className="modal-footer">
                        <Button variant="secondary" type="button" onClick={onClose}>Cancel</Button>
                        <Button type="submit" isLoading={isLoading}>{isEdit ? 'Save Changes' : 'Create Room'}</Button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default RoomModal;
