import React, { useState } from 'react';
import type { RoomDto } from '../types';
import type { BookingCreateDto } from '../types';
import { bookingsService } from '../api/bookings';
import Button from './Button';
import Input from './Input';
import { toast } from 'react-toastify';

interface BookingModalProps {
    room: RoomDto;
    onClose: () => void;
    onBooked: () => void;
}

const BookingModal: React.FC<BookingModalProps> = ({ room, onClose, onBooked }) => {
    const [formData, setFormData] = useState<BookingCreateDto>({
        title: '',
        description: '',
        roomId: room.id,
        startTime: '',
        endTime: '',
        participantUserIds: []
    });
    const [isLoading, setIsLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            await bookingsService.create(formData);
            toast.success('Booking created successfully!');
            onBooked();
            onClose();
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Failed to create booking');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal" onClick={e => e.stopPropagation()}>
                <div className="modal-header">
                    <h3 className="modal-title">Book {room.name}</h3>
                    <button className="modal-close" onClick={onClose}>
                        <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>
                <form onSubmit={handleSubmit}>
                    <div className="modal-body">
                        <Input
                            label="Meeting Title"
                            placeholder="Team standup, Sprint planning..."
                            value={formData.title}
                            onChange={e => setFormData({ ...formData, title: e.target.value })}
                            required
                        />
                        <div className="form-group">
                            <label className="form-label">Description (optional)</label>
                            <textarea
                                className="form-input"
                                rows={3}
                                placeholder="Add any notes..."
                                value={formData.description}
                                onChange={e => setFormData({ ...formData, description: e.target.value })}
                                style={{ resize: 'vertical' }}
                            />
                        </div>
                        <Input
                            label="Date"
                            type="date"
                            value={formData.startTime.split('T')[0] || ''}
                            onChange={e => {
                                const date = e.target.value;
                                const startTime = formData.startTime.split('T')[1] || '09:00';
                                const endTime = formData.endTime.split('T')[1] || '10:00';
                                setFormData({
                                    ...formData,
                                    startTime: `${date}T${startTime}`,
                                    endTime: `${date}T${endTime}`
                                });
                            }}
                            required
                        />
                        <div className="form-row">
                            <Input
                                label="Start Time"
                                type="time"
                                value={formData.startTime.split('T')[1] || ''}
                                onChange={e => {
                                    const date = formData.startTime.split('T')[0] || new Date().toISOString().split('T')[0];
                                    setFormData({ ...formData, startTime: `${date}T${e.target.value}` });
                                }}
                                required
                            />
                            <Input
                                label="End Time"
                                type="time"
                                value={formData.endTime.split('T')[1] || ''}
                                onChange={e => {
                                    const date = formData.endTime.split('T')[0] || formData.startTime.split('T')[0] || new Date().toISOString().split('T')[0];
                                    setFormData({ ...formData, endTime: `${date}T${e.target.value}` });
                                }}
                                required
                            />
                        </div>
                    </div>
                    <div className="modal-footer">
                        <Button variant="secondary" type="button" onClick={onClose}>Cancel</Button>
                        <Button type="submit" isLoading={isLoading}>Confirm Booking</Button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default BookingModal;
