import React, { useEffect, useState } from 'react';
import { recycleBinService } from '../api/recycleBin';
import type { DeletedItem } from '../types';
import Button from '../components/Button';
import ConfirmModal from '../components/ConfirmModal';
import DataTable, { type Column } from '../components/DataTable';
import { toast } from 'react-toastify';

const AdminRecycleBin: React.FC = () => {
    const [items, setItems] = useState<DeletedItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean, item: DeletedItem | null }>({
        isOpen: false,
        item: null
    });
    const [isDeleting, setIsDeleting] = useState(false);

    const loadItems = async () => {
        setLoading(true);
        try {
            const data = await recycleBinService.getDeletedItems();
            setItems(Array.isArray(data) ? data : []);
        } catch (err) {
            toast.error('Failed to load recycle bin items');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { loadItems(); }, []);

    const handleRestore = async (id: string, type: DeletedItem['type']) => {
        try {
            await recycleBinService.restoreItem(id, type);
            toast.success(`${type} restored successfully`);
            loadItems();
        } catch (err) {
            toast.error('Failed to restore item');
        }
    };

    const triggerHardDelete = (item: DeletedItem) => {
        setConfirmDelete({ isOpen: true, item });
    };

    const handleHardDelete = async () => {
        const item = confirmDelete.item;
        if (!item) return;

        setIsDeleting(true);
        try {
            await recycleBinService.hardDelete(item.id, item.type);
            toast.success(`${item.type} permanently deleted`);
            setConfirmDelete({ isOpen: false, item: null });
            loadItems();
        } catch (err) {
            toast.error('Failed to delete item permanently');
        } finally {
            setIsDeleting(false);
        }
    };

    const columns: Column<DeletedItem>[] = [
        { header: 'Item Name', accessor: (item) => <span style={{ fontWeight: 600 }}>{item.name}</span>, sortable: true, sortKey: 'name' },
        {
            header: 'Type',
            accessor: (item) => (
                <span className={`badge ${item.type === 'Room' ? 'badge-primary' : item.type === 'Booking' ? 'badge-neutral' : 'badge-danger'}`}>
                    {item.type}
                </span>
            ),
            sortable: true,
            sortKey: 'type'
        },
        {
            header: 'Deleted Date',
            accessor: (item) => (
                <span style={{ color: 'var(--text-secondary)' }}>
                    {new Date(item.deletedAt).toLocaleString()}
                </span>
            ),
            sortable: true,
            sortKey: 'deletedAt'
        },
        {
            header: 'Actions',
            accessor: (item) => (
                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)' }}>
                    <Button size="sm" variant="secondary" onClick={() => handleRestore(item.id, item.type)}>Restore</Button>
                    <Button size="sm" variant="danger" onClick={() => triggerHardDelete(item)}>Delete Permanent</Button>
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
                <h1 className="page-title">Recycle Bin</h1>
                <p className="page-subtitle">Restore or permanently delete system items</p>
            </div>

            <DataTable
                data={items}
                columns={columns}
                searchPlaceholder="Search in recycle bin..."
                emptyMessage="Your recycle bin is empty"
            />

            <ConfirmModal
                isOpen={confirmDelete.isOpen}
                title="Permanent Delete"
                message={`Are you sure you want to permanently delete this ${confirmDelete.item?.type}? This action cannot be undone.`}
                variant="danger"
                onConfirm={handleHardDelete}
                onClose={() => setConfirmDelete({ isOpen: false, item: null })}
                isLoading={isDeleting}
            />
        </div>
    );
};

export default AdminRecycleBin;
