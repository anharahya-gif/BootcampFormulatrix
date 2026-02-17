import React, { useEffect, useState } from 'react';
import { bookingsService } from '../api/bookings';
import type { BookingDto } from '../types';
import Button from '../components/Button';
import ConfirmModal from '../components/ConfirmModal';
import DataTable, { type Column } from '../components/DataTable';
import { toast } from 'react-toastify';

const AdminBookings: React.FC = () => {
    const [bookings, setBookings] = useState<BookingDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [confirmAction, setConfirmAction] = useState<{
        isOpen: boolean;
        title: string;
        message: string;
        onConfirm: () => void;
        variant: 'primary' | 'danger' | 'warning';
    }>({
        isOpen: false,
        title: '',
        message: '',
        onConfirm: () => { },
        variant: 'primary'
    });

    const loadBookings = async () => {
        try {
            const data = await bookingsService.getAll();
            setBookings(Array.isArray(data) ? data : []);
        } catch { /* */ }
        setLoading(false);
    };

    useEffect(() => { loadBookings(); }, []);

    const handleApprove = (id: string) => {
        setConfirmAction({
            isOpen: true,
            title: 'Approve Booking',
            message: 'Are you sure you want to approve this booking request?',
            variant: 'primary',
            onConfirm: async () => {
                try {
                    await bookingsService.updateStatus(id, 1);
                    toast.success('Booking approved');
                    setConfirmAction(prev => ({ ...prev, isOpen: false }));
                    loadBookings();
                } catch (err: any) {
                    toast.error(err.response?.data?.message || 'Failed to approve');
                }
            }
        });
    };

    const handleReject = (id: string) => {
        setConfirmAction({
            isOpen: true,
            title: 'Reject Booking',
            message: 'Are you sure you want to reject this booking request?',
            variant: 'danger',
            onConfirm: async () => {
                try {
                    await bookingsService.updateStatus(id, 2);
                    toast.success('Booking rejected');
                    setConfirmAction(prev => ({ ...prev, isOpen: false }));
                    loadBookings();
                } catch (err: any) {
                    toast.error(err.response?.data?.message || 'Failed to reject');
                }
            }
        });
    };

    const formatDT = (s: string) => {
        const d = new Date(s);
        return d.toLocaleDateString([], { month: 'short', day: 'numeric' }) + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
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

    const columns: Column<BookingDto>[] = [
        { header: 'Title', accessor: 'title', sortable: true },
        { header: 'Room', accessor: (b) => b.roomName || '—', sortable: true, sortKey: 'roomName' },
        {
            header: 'Time',
            accessor: (b) => (
                <span style={{ fontSize: '0.8125rem', color: 'var(--text-secondary)' }}>
                    {formatDT(b.startTime)} – {formatDT(b.endTime)}
                </span>
            ),
            sortable: true,
            sortKey: 'startTime'
        },
        { header: 'Requested By', accessor: (b) => b.createdByUserName || '—', sortable: true, sortKey: 'createdByUserName' },
        {
            header: 'Status',
            accessor: (b) => getStatusBadge(b.status),
            sortable: true,
            sortKey: 'status',
            align: 'center',
        },
        {
            header: 'Actions',
            accessor: (b) => (
                <div style={{ display: 'flex', justifyContent: 'center', gap: 'var(--space-2)' }}>
                    {b.status === 0 && (
                        <>
                            <Button size="sm" onClick={() => handleApprove(b.id)}>Approve</Button>
                            <Button variant="danger" size="sm" onClick={() => handleReject(b.id)}>Reject</Button>
                        </>
                    )}
                </div>
            ),
            align: 'center',
        }
    ];

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header">
                <h1 className="page-title">Manage Bookings</h1>
                <p className="page-subtitle">Approve or reject booking requests</p>
            </div>

            <DataTable
                data={bookings}
                columns={columns}
                searchPlaceholder="Search bookings by title, room or user..."
            />

            <ConfirmModal
                isOpen={confirmAction.isOpen}
                title={confirmAction.title}
                message={confirmAction.message}
                variant={confirmAction.variant}
                confirmText="Proceed"
                onConfirm={confirmAction.onConfirm}
                onClose={() => setConfirmAction(prev => ({ ...prev, isOpen: false }))}
            />
        </div>
    );
};

export default AdminBookings;
