import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { bookingsService } from '../api/bookings';
import { useAuth } from '../context/AuthContext';
import type { BookingDto } from '../types';
import { toast } from 'react-toastify';

const BookingDetails: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { user } = useAuth();
    const [booking, setBooking] = useState<BookingDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadBooking = async () => {
            if (!id) return;
            try {
                const data = await bookingsService.getById(id);
                setBooking(data);
            } catch (err: any) {
                toast.error('Failed to load booking details');
                navigate('/my-bookings');
            } finally {
                setLoading(false);
            }
        };
        loadBooking();
    }, [id, navigate]);

    if (loading) return <div className="loading-container"><div className="spinner" /></div>;
    if (!booking) return <div className="page-content">Booking not found</div>;

    const isOrganizer = booking.createdByUserId === user?.id;

    const getStatusBadge = (status: number) => {
        switch (status) {
            case 0: return <span className="badge badge-warning">Pending</span>;
            case 1: return <span className="badge badge-success">Approved</span>;
            case 2: return <span className="badge badge-danger">Rejected</span>;
            case 3: return <span className="badge badge-neutral">Cancelled</span>;
            default: return <span className="badge badge-neutral">Unknown</span>;
        }
    };

    const formatDateTime = (dateStr: string) => {
        const d = new Date(dateStr);
        return d.toLocaleDateString([], { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) + ' at ' +
            d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    return (
        <div className="page-content">
            <div className="page-header">
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)', marginBottom: 'var(--space-2)' }}>
                    <button onClick={() => navigate(-1)} className="btn-icon" style={{ padding: 'var(--space-2)', borderRadius: '50%', background: 'var(--bg-elevated)', color: 'var(--text-main)' }}>
                        <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                        </svg>
                    </button>
                    <h1 className="page-title">{booking.title}</h1>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', marginLeft: 'var(--space-12)' }}>
                    {getStatusBadge(booking.status)}
                    <span className={`badge ${isOrganizer ? 'badge-primary' : 'badge-neutral'}`}>
                        {isOrganizer ? 'Organizer' : 'Participant'}
                    </span>
                </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '40px', marginTop: '24px' }}>
                {/* Left Column: Meeting Details & Schedule */}
                <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
                    <div className="card">
                        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-4)', color: 'var(--primary)' }}>
                            <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h7" /></svg>
                            <h3 className="card-title" style={{ marginBottom: 0 }}>Meeting Description</h3>
                        </div>
                        <p style={{ color: 'var(--text-muted)', lineHeight: 1.7, fontSize: '0.95rem' }}>
                            {booking.description || 'No description provided for this meeting.'}
                        </p>
                    </div>

                    <div className="card">
                        <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--primary)', marginBottom: 'var(--space-4)' }}>
                            <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                            <h3 className="card-title" style={{ marginBottom: 0 }}>Time & Schedule</h3>
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
                            <div style={{ display: 'flex', alignItems: 'flex-start', gap: 'var(--space-4)' }}>
                                <div style={{ width: '42px', height: '42px', borderRadius: '12px', background: 'var(--primary-subtle)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--primary)', flexShrink: 0 }}>
                                    <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                                </div>
                                <div>
                                    <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '2px' }}>Meeting Starts</div>
                                    <div style={{ fontWeight: 600, fontSize: '1rem', color: 'var(--text-main)' }}>{formatDateTime(booking.startTime)}</div>
                                </div>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'flex-start', gap: 'var(--space-4)' }}>
                                <div style={{ width: '42px', height: '42px', borderRadius: '12px', background: 'var(--danger-subtle)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--danger)', flexShrink: 0 }}>
                                    <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                                </div>
                                <div>
                                    <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '2px' }}>Meeting Ends</div>
                                    <div style={{ fontWeight: 600, fontSize: '1rem', color: 'var(--text-main)' }}>{formatDateTime(booking.endTime)}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Right Column: Organizer, Location & Participants */}
                <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
                    <div className="card">
                        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-4)', color: 'var(--primary)' }}>
                            <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                            <h3 className="card-title" style={{ marginBottom: 0 }}>Organizer Info</h3>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)', padding: 'var(--space-1)' }}>
                            <div style={{ width: '56px', height: '56px', borderRadius: '50%', background: 'linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%)', color: 'white', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontSize: '1.6rem', boxShadow: '0 4px 12px rgba(var(--primary-rgb), 0.3)' }}>
                                {booking.createdByUserName?.[0]?.toUpperCase() || 'U'}
                            </div>
                            <div>
                                <div style={{ fontWeight: 600, fontSize: '1.1rem', color: 'var(--text-main)' }}>{booking.createdByUserName}</div>
                                <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Meeting Creator & Organizer</div>
                            </div>
                        </div>
                    </div>

                    <div className="card">
                        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-4)', color: 'var(--primary)' }}>
                            <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5" /></svg>
                            <h3 className="card-title" style={{ marginBottom: 0 }}>Location</h3>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', padding: 'var(--space-3)', background: 'var(--bg-elevated)', borderRadius: '12px', border: '1px solid var(--border)' }}>
                            <div style={{ width: '40px', height: '40px', borderRadius: '10px', background: 'var(--primary-subtle)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--primary)' }}>
                                <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5" /></svg>
                            </div>
                            <div>
                                <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Room Assigned</div>
                                <div style={{ fontWeight: 600, fontSize: '1.15rem', color: 'var(--text-main)' }}>{booking.roomName || 'Universal Meeting Room'}</div>
                            </div>
                        </div>
                    </div>

                    {booking.participants && booking.participants.length > 0 && (
                        <div className="card">
                            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-4)', color: 'var(--primary)' }}>
                                <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2m16-10a4 4 0 11-8 0 4 4 0 018 0z" /></svg>
                                <h3 className="card-title" style={{ marginBottom: 0 }}>Participants ({booking.participants.length})</h3>
                            </div>
                            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(130px, 1fr))', gap: '12px' }}>
                                {booking.participants.map(p => (
                                    <div key={p.userId} style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '8px', background: 'var(--bg-elevated)', borderRadius: 'var(--radius-md)', border: '1px solid var(--border)' }}>
                                        <div style={{ width: '28px', height: '28px', borderRadius: '50%', background: 'var(--bg-main)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600, fontSize: '0.75rem', border: '1px solid var(--border)', flexShrink: 0, color: 'var(--primary)' }}>
                                            {p.fullName[0].toUpperCase()}
                                        </div>
                                        <div style={{ fontSize: '0.85rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', color: 'var(--text-main)' }} title={p.fullName}>
                                            {p.fullName}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default BookingDetails;
