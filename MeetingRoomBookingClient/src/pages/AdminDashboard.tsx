import React, { useEffect, useState } from 'react';
import { roomsService } from '../api/rooms';
import { bookingsService } from '../api/bookings';
import type { RoomDto } from '../types';

const AdminDashboard: React.FC = () => {
    const [rooms, setRooms] = useState<RoomDto[]>([]);
    const [totalBookings, setTotalBookings] = useState(0);
    const [pending, setPending] = useState(0);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const load = async () => {
            try {
                const r = await roomsService.getAll();
                setRooms(Array.isArray(r) ? r : []);
                try {
                    const b = await bookingsService.getMyBookings();
                    const bookingsArr = Array.isArray(b) ? b : [];
                    setTotalBookings(bookingsArr.length);
                    setPending(bookingsArr.filter((x: any) => x.status === 0).length);
                } catch { /* */ }
            } catch { /* */ }
            setLoading(false);
        };
        load();
    }, []);

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header">
                <h1 className="page-title">Admin Dashboard</h1>
                <p className="page-subtitle">Overview of rooms and bookings</p>
            </div>

            <div className="stats-grid">
                <div className="stat-card">
                    <div className="stat-card-icon orange">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5" /></svg>
                    </div>
                    <div className="stat-card-value">{rooms.length}</div>
                    <div className="stat-card-label">Total Rooms</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-icon blue">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                    </div>
                    <div className="stat-card-value">{totalBookings}</div>
                    <div className="stat-card-label">Total Bookings</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-icon green">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <div className="stat-card-value">{rooms.filter(r => r.hasProjector).length}</div>
                    <div className="stat-card-label">Rooms with Projector</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-icon red">
                        <svg width="22" height="22" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <div className="stat-card-value">{pending}</div>
                    <div className="stat-card-label">Pending Approvals</div>
                </div>
            </div>
        </div>
    );
};

export default AdminDashboard;
