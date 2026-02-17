import React, { useEffect, useState } from 'react';
import { roomsService } from '../api/rooms';
import type { RoomDto } from '../types';
import Button from '../components/Button';
import RoomModal from '../components/RoomModal';
import ConfirmModal from '../components/ConfirmModal';
import DataTable, { type Column } from '../components/DataTable';
import { toast } from 'react-toastify';

const AdminRooms: React.FC = () => {
    const [rooms, setRooms] = useState<RoomDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [editingRoom, setEditingRoom] = useState<RoomDto | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean, roomId: string | null }>({
        isOpen: false,
        roomId: null
    });
    const [isDeleting, setIsDeleting] = useState(false);

    const loadRooms = async () => {
        try {
            const data = await roomsService.getAll();
            setRooms(Array.isArray(data) ? data : []);
        } catch { /* */ }
        setLoading(false);
    };

    useEffect(() => { loadRooms(); }, []);

    const handleDelete = async () => {
        if (!confirmDelete.roomId) return;

        setIsDeleting(true);
        try {
            await roomsService.delete(confirmDelete.roomId);
            toast.success('Room moved to Recycle Bin');
            setConfirmDelete({ isOpen: false, roomId: null });
            loadRooms();
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Failed to delete');
        } finally {
            setIsDeleting(false);
        }
    };

    const columns: Column<RoomDto>[] = [
        { header: 'Room Name', accessor: 'name', sortable: true },
        { header: 'Location', accessor: 'location', sortable: true },
        { header: 'Capacity', accessor: (r) => `${r.capacity} people`, sortable: true, sortKey: 'capacity' },
        {
            header: 'Projector',
            accessor: (r) => (
                r.hasProjector ? (
                    <span className="badge badge-success">Yes</span>
                ) : (
                    <span className="badge badge-neutral">No</span>
                )
            ),
            sortable: true,
            sortKey: 'hasProjector',
        },
        {
            header: 'Actions',
            accessor: (room) => (
                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)' }}>
                    <Button variant="ghost" size="sm" onClick={() => { setEditingRoom(room); setShowModal(true); }}>
                        Edit
                    </Button>
                    <Button variant="danger" size="sm" onClick={() => setConfirmDelete({ isOpen: true, roomId: room.id })}>
                        Delete
                    </Button>
                </div>
            ),
            align: 'right',
        }
    ];

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', width: '100%' }}>
                    <div>
                        <h1 className="page-title">Manage Rooms</h1>
                        <p className="page-subtitle">Create and manage meeting rooms</p>
                    </div>
                    <Button onClick={() => { setEditingRoom(null); setShowModal(true); }}>
                        <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24" style={{ marginRight: '8px' }}>
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                        </svg>
                        Add Room
                    </Button>
                </div>
            </div>

            <DataTable
                data={rooms}
                columns={columns}
                searchPlaceholder="Search rooms by name or location..."
            />

            {showModal && (
                <RoomModal
                    isOpen={showModal}
                    onClose={() => setShowModal(false)}
                    room={editingRoom}
                    onSuccess={loadRooms}
                />
            )}

            <ConfirmModal
                isOpen={confirmDelete.isOpen}
                title="Delete Room"
                message="Are you sure you want to delete this room? It will be moved to the Recycle Bin."
                variant="danger"
                onConfirm={handleDelete}
                onClose={() => setConfirmDelete({ isOpen: false, roomId: null })}
                isLoading={isDeleting}
            />
        </div>
    );
};

export default AdminRooms;
