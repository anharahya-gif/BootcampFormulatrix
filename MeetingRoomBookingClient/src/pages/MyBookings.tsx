import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { bookingsService } from '../api/bookings';
import { useAuth } from '../context/AuthContext';
import type { BookingDto } from '../types';
import Button from '../components/Button';
import ConfirmModal from '../components/ConfirmModal';
import { toast } from 'react-toastify';

const MyBookings: React.FC = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [bookings, setBookings] = useState<BookingDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [confirmCancel, setConfirmCancel] = useState<{ isOpen: boolean, bookingId: string | null }>({
        isOpen: false,
        bookingId: null
    });
    const [isCancelling, setIsCancelling] = useState(false);

    const loadBookings = async () => {
        try {
            const data = await bookingsService.getMyBookings();
            setBookings(Array.isArray(data) ? data : []);
        } catch { /* ignore */ }
        setLoading(false);
    };

    useEffect(() => { loadBookings(); }, []);

    const handleCancel = async () => {
        if (!confirmCancel.bookingId) return;

        setIsCancelling(true);
        try {
            await bookingsService.cancel(confirmCancel.bookingId);
            toast.success('Booking cancelled');
            setConfirmCancel({ isOpen: false, bookingId: null });
            loadBookings();
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Failed to cancel');
        } finally {
            setIsCancelling(false);
        }
    };

    const getStatusBadge = (status: number) => {
        switch (status) {
            case 0: return <span className="badge badge-warning badge-dot">Pending</span>;
            case 1: return <span className="badge badge-success badge-dot">Approved</span>;
            case 2: return <span className="badge badge-danger badge-dot">Rejected</span>;
            case 3: return <span className="badge badge-neutral badge-dot">Cancelled</span>;
            default: return <span className="badge badge-neutral">Unknown</span>;
        }
    };

    const formatDateTime = (dateStr: string) => {
        const d = new Date(dateStr);
        return d.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' }) + ' · ' +
            d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header">
                <h1 className="page-title">My Bookings</h1>
                <p className="page-subtitle">Manage your meeting room reservations</p>
            </div>

            {bookings.length === 0 ? (
                <div className="card">
                    <div className="empty-state">
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                        <div className="empty-state-title">No bookings yet</div>
                        <div className="empty-state-desc">Your meeting room reservations will appear here</div>
                    </div>
                </div>
            ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
                    {bookings.map(booking => {
                        const isOrganizer = booking.createdByUserId === user?.id;
                        return (
                            <div
                                key={booking.id}
                                className="booking-item"
                                onClick={() => navigate(`/bookings/${booking.id}`)}
                                style={{ cursor: 'pointer' }}
                            >
                                <div className="booking-info">
                                    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
                                        <div className="booking-title">{booking.title}</div>
                                        <span className={`badge ${isOrganizer ? 'badge-primary' : 'badge-neutral'}`} style={{ textTransform: 'none', fontSize: '0.65rem' }}>
                                            {isOrganizer ? 'Organizer' : 'Participant'}
                                        </span>
                                    </div>
                                    <div className="booking-meta">
                                        <span className="booking-meta-item">
                                            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                                            {formatDateTime(booking.startTime)}
                                        </span>
                                        <span className="booking-meta-item">
                                            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                                            {new Date(booking.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} – {new Date(booking.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                        </span>
                                        {booking.roomName && (
                                            <span className="booking-meta-item">
                                                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5" /></svg>
                                                {booking.roomName}
                                            </span>
                                        )}
                                        {!isOrganizer && booking.createdByUserName && (
                                            <span className="booking-meta-item">
                                                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                                                By {booking.createdByUserName}
                                            </span>
                                        )}
                                    </div>
                                </div>
                                <div className="booking-actions">
                                    {getStatusBadge(booking.status)}
                                    {(booking.status === 0 || booking.status === 1) && isOrganizer && (
                                        <Button
                                            variant="danger"
                                            size="sm"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                setConfirmCancel({ isOpen: true, bookingId: booking.id });
                                            }}
                                        >
                                            Cancel
                                        </Button>
                                    )}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}

            <ConfirmModal
                isOpen={confirmCancel.isOpen}
                title="Cancel Booking"
                message="Are you sure you want to cancel this booking?"
                variant="warning"
                confirmText="Yes, Cancel"
                onConfirm={handleCancel}
                onClose={() => setConfirmCancel({ isOpen: false, bookingId: null })}
                isLoading={isCancelling}
            />
        </div>
    );
};

export default MyBookings;
