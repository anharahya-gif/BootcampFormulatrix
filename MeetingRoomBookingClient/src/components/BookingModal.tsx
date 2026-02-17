import React, { useState, useEffect } from 'react';
import type { RoomDto, BookingCreateDto, UserReadDto } from '../types';
import { bookingsService } from '../api/bookings';
import { usersService } from '../api/users';
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
    const [showParticipants, setShowParticipants] = useState(false);
    const [allUsers, setAllUsers] = useState<UserReadDto[]>([]);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        const fetchUsers = async () => {
            try {
                const users = await usersService.getAll();
                setAllUsers(users);
            } catch (err) {
                console.error('Failed to fetch users', err);
            }
        };
        fetchUsers();
    }, []);

    const filteredUsers = allUsers.filter((user: UserReadDto) =>
        (user.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
            user.email?.toLowerCase().includes(searchTerm.toLowerCase())) &&
        !formData.participantUserIds?.includes(user.id)
    );

    const toggleParticipant = (userId: string) => {
        const current = formData.participantUserIds || [];
        const updated = current.includes(userId)
            ? current.filter(id => id !== userId)
            : [...current, userId];
        setFormData({ ...formData, participantUserIds: updated });
    };

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
                                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setFormData({ ...formData, description: e.target.value })}
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

                        <div style={{ marginTop: 'var(--space-6)', borderTop: '1px solid var(--border)', paddingTop: 'var(--space-4)' }}>
                            <label className="form-checkbox">
                                <input
                                    type="checkbox"
                                    checked={showParticipants}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                                        setShowParticipants(e.target.checked);
                                        if (!e.target.checked) {
                                            setFormData({ ...formData, participantUserIds: [] });
                                        }
                                    }}
                                />
                                <span>Invite participants?</span>
                            </label>

                            {showParticipants && (
                                <div style={{ marginTop: 'var(--space-4)' }}>
                                    <Input
                                        label="Search Users"
                                        placeholder="Type name or email..."
                                        value={searchTerm}
                                        onChange={e => setSearchTerm(e.target.value)}
                                    />

                                    {/* Selected Participants Tags */}
                                    {formData.participantUserIds && formData.participantUserIds.length > 0 && (
                                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-2)', marginBottom: 'var(--space-3)' }}>
                                            {formData.participantUserIds.map((id: string) => {
                                                const u = allUsers.find((user: UserReadDto) => user.id === id);
                                                if (!u) return null;
                                                return (
                                                    <span key={id} className="badge badge-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--space-1)', textTransform: 'none' }}>
                                                        {u.fullName || u.email}
                                                        <button
                                                            type="button"
                                                            onClick={() => toggleParticipant(id)}
                                                            style={{ color: 'inherit', display: 'flex', alignItems: 'center' }}
                                                        >
                                                            &times;
                                                        </button>
                                                    </span>
                                                );
                                            })}
                                        </div>
                                    )}

                                    {/* User Search Results */}
                                    {searchTerm && (
                                        <div style={{
                                            maxHeight: '150px',
                                            overflowY: 'auto',
                                            background: 'var(--bg-elevated)',
                                            border: '1px solid var(--border)',
                                            borderRadius: 'var(--radius-lg)'
                                        }}>
                                            {filteredUsers.length > 0 ? (
                                                filteredUsers.map((user: UserReadDto) => (
                                                    <div
                                                        key={user.id}
                                                        onClick={() => {
                                                            toggleParticipant(user.id);
                                                            setSearchTerm('');
                                                        }}
                                                        style={{
                                                            padding: 'var(--space-2) var(--space-3)',
                                                            cursor: 'pointer',
                                                            borderBottom: '1px solid var(--border)',
                                                            fontSize: '0.875rem'
                                                        }}
                                                        className="user-search-item"
                                                    >
                                                        <div style={{ fontWeight: 600 }}>{user.fullName}</div>
                                                        <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{user.email}</div>
                                                    </div>
                                                ))
                                            ) : (
                                                <div style={{ padding: 'var(--space-3)', textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                                                    No users found
                                                </div>
                                            )}
                                        </div>
                                    )}
                                </div>
                            )}
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
