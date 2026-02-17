import React from 'react';
import type { RoomDto } from '../types';
import Button from './Button';

interface RoomCardProps {
    room: RoomDto;
    onBook: (room: RoomDto) => void;
}

const RoomCard: React.FC<RoomCardProps> = ({ room, onBook }) => {
    return (
        <div className="room-card">
            <div className="room-card-image">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
                {room.hasProjector && (
                    <span className="room-card-badge">
                        <span className="badge badge-info">Projector</span>
                    </span>
                )}
            </div>
            <div className="room-card-body">
                <div className="room-card-name">{room.name}</div>
                <div className="room-card-details">
                    <div className="room-card-detail">
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                        </svg>
                        {room.location}
                    </div>
                    <div className="room-card-detail">
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                        </svg>
                        Capacity: {room.capacity} people
                    </div>
                </div>
                <Button onClick={() => onBook(room)} className="w-full">
                    Book Room
                </Button>
            </div>
        </div>
    );
};

export default RoomCard;
