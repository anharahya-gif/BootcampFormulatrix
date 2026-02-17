import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { bookingsService } from '../api/bookings';
import { roomsService } from '../api/rooms';
import type { BookingDto, RoomDto } from '../types';
import Button from '../components/Button';

const Dashboard: React.FC = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [bookings, setBookings] = useState<BookingDto[]>([]);
    const [rooms, setRooms] = useState<RoomDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadData = async () => {
            try {
                const [b, r] = await Promise.all([
                    bookingsService.getMyBookings(),
                    roomsService.getAll()
                ]);
                setBookings(Array.isArray(b) ? b : []);
                setRooms(Array.isArray(r) ? r : []);
            } catch { /* ignore */ }
            setLoading(false);
        };
        loadData();
    }, []);

    const todayBookings = bookings.filter(b => {
        const d = new Date(b.startTime);
        const now = new Date();
        return d.toDateString() === now.toDateString();
    });

    const upcomingBookings = bookings.filter(b => new Date(b.startTime) > new Date())
        .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
        .slice(0, 5);

    const getStatusBadge = (status: number) => {
        switch (status) {
            case 0: return <span className="badge badge-warning badge-dot">Pending</span>;
            case 1: return <span className="badge badge-success badge-dot">Approved</span>;
            case 2: return <span className="badge badge-danger badge-dot">Rejected</span>;
            case 3: return <span className="badge badge-neutral badge-dot">Cancelled</span>;
            default: return <span className="badge badge-neutral">Unknown</span>;
        }
    };

    const formatTime = (dateStr: string) => {
        return new Date(dateStr).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    const formatDate = (dateStr: string) => {
        return new Date(dateStr).toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
    };

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header">
                <h1 className="page-title">Good {new Date().getHours() < 12 ? 'morning' : new Date().getHours() < 18 ? 'afternoon' : 'evening'}, {user?.userName || 'User'}</h1>
                <p className="page-subtitle">Here's an overview of your bookings today</p>
            </div>

            {/* Stats */}
            <div className="stats-grid" style={{ marginBottom: 'var(--space-8)' }}>
                <div className="stat-card">
                    <div className="stat-card-icon orange">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                    </div>
                    <div className="stat-card-value">{todayBookings.length}</div>
                    <div className="stat-card-label">Today's Bookings</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-icon blue">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1" /></svg>
                    </div>
                    <div className="stat-card-value">{rooms.length}</div>
                    <div className="stat-card-label">Available Rooms</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-icon green">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <div className="stat-card-value">{bookings.filter(b => b.status === 1).length}</div>
                    <div className="stat-card-label">Approved</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-icon red">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <div className="stat-card-value">{bookings.filter(b => b.status === 0).length}</div>
                    <div className="stat-card-label">Pending</div>
                </div>
            </div>

            {/* Upcoming Bookings */}
            <div className="section-header">
                <div>
                    <h2 className="section-title">Upcoming Bookings</h2>
                    <p className="section-desc">Your next scheduled meetings</p>
                </div>
                <Button variant="secondary" size="sm" onClick={() => navigate('/my-bookings')}>View All</Button>
            </div>

            {upcomingBookings.length === 0 ? (
                <div className="card">
                    <div className="empty-state">
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                        <div className="empty-state-title">No upcoming bookings</div>
                        <div className="empty-state-desc">Book a meeting room to get started</div>
                        <Button className="mt-4" onClick={() => navigate('/rooms')}>Browse Rooms</Button>
                    </div>
                </div>
            ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
                    {upcomingBookings.map(booking => (
                        <div key={booking.id} className="booking-item">
                            <div className="booking-info">
                                <div className="booking-title">{booking.title}</div>
                                <div className="booking-meta">
                                    <span className="booking-meta-item">
                                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                                        {formatDate(booking.startTime)}
                                    </span>
                                    <span className="booking-meta-item">
                                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                                        {formatTime(booking.startTime)} – {formatTime(booking.endTime)}
                                    </span>
                                    {booking.roomName && (
                                        <span className="booking-meta-item">
                                            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5" /></svg>
                                            {booking.roomName}
                                        </span>
                                    )}
                                </div>
                            </div>
                            {getStatusBadge(booking.status)}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default Dashboard;
