import React, { useEffect, useState } from 'react';
import { roomsService } from '../api/rooms';
import type { RoomDto } from '../types';
import RoomCard from '../components/RoomCard';
import BookingModal from '../components/BookingModal';

const Rooms: React.FC = () => {
    const [rooms, setRooms] = useState<RoomDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [selectedRoom, setSelectedRoom] = useState<RoomDto | null>(null);

    const loadRooms = async () => {
        try {
            const data = await roomsService.getAll();
            setRooms(Array.isArray(data) ? data : []);
        } catch { /* ignore */ }
        setLoading(false);
    };

    useEffect(() => { loadRooms(); }, []);

    const filtered = rooms.filter(r =>
        r.name.toLowerCase().includes(search.toLowerCase()) ||
        r.location.toLowerCase().includes(search.toLowerCase())
    );

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header">
                <h1 className="page-title">Meeting Rooms</h1>
                <p className="page-subtitle">Browse and book available rooms</p>
            </div>

            <div className="toolbar">
                <div className="toolbar-left">
                    <div className="search-bar" style={{ maxWidth: 320 }}>
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input
                            placeholder="Search rooms..."
                            value={search}
                            onChange={e => setSearch(e.target.value)}
                        />
                    </div>
                </div>
                <div className="toolbar-right">
                    <span style={{ fontSize: '0.875rem', color: 'var(--text-muted)' }}>
                        {filtered.length} room{filtered.length !== 1 ? 's' : ''}
                    </span>
                </div>
            </div>

            {filtered.length === 0 ? (
                <div className="card">
                    <div className="empty-state">
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5" /></svg>
                        <div className="empty-state-title">No rooms found</div>
                        <div className="empty-state-desc">{search ? 'Try a different search term' : 'No meeting rooms available yet'}</div>
                    </div>
                </div>
            ) : (
                <div className="rooms-grid">
                    {filtered.map(room => (
                        <RoomCard key={room.id} room={room} onBook={setSelectedRoom} />
                    ))}
                </div>
            )}

            {selectedRoom && (
                <BookingModal
                    room={selectedRoom}
                    onClose={() => setSelectedRoom(null)}
                    onBooked={loadRooms}
                />
            )}
        </div>
    );
};

export default Rooms;
