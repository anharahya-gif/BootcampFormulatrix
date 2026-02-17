import React, { useEffect, useState } from 'react';
import { usersService } from '../api/users';
import type { UserReadDto, UserCreateDto } from '../types';
import Button from '../components/Button';
import Input from '../components/Input';
import ConfirmModal from '../components/ConfirmModal';
import DataTable, { type Column } from '../components/DataTable';
import { toast } from 'react-toastify';

const AdminUsers: React.FC = () => {
    const [users, setUsers] = useState<UserReadDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [isAdding, setIsAdding] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [newUser, setNewUser] = useState<UserCreateDto>({
        email: '',
        password: '',
        fullName: '',
        department: '',
        phoneNumber: '',
        role: 'User'
    });

    const [confirmAction, setConfirmAction] = useState<{
        isOpen: boolean;
        title: string;
        message: string;
        onConfirm: () => void;
        variant: 'danger' | 'warning' | 'primary';
    }>({
        isOpen: false,
        title: '',
        message: '',
        onConfirm: () => { },
        variant: 'primary'
    });

    const loadUsers = async () => {
        try {
            const data = await usersService.getAll();
            setUsers(Array.isArray(data) ? data : []);
        } catch (err) {
            toast.error('Failed to load users');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { loadUsers(); }, []);

    const handleCreateUser = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            await usersService.create(newUser);
            toast.success('User created successfully');
            setIsAdding(false);
            setNewUser({ email: '', password: '', fullName: '', department: '', phoneNumber: '', role: 'User' });
            loadUsers();
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Failed to create user');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleRoleChange = (userId: string, currentRole: string) => {
        const newRole = currentRole === 'Admin' ? 'User' : 'Admin';
        setConfirmAction({
            isOpen: true,
            title: 'Change User Role',
            message: `Are you sure you want to change this user's role from ${currentRole} to ${newRole}?`,
            variant: 'primary',
            onConfirm: async () => {
                try {
                    await usersService.updateRole(userId, newRole);
                    toast.success(`User role updated to ${newRole}`);
                    setConfirmAction(prev => ({ ...prev, isOpen: false }));
                    loadUsers();
                } catch (err: any) {
                    toast.error(err.response?.data?.message || 'Failed to update role');
                }
            }
        });
    };

    const handleDeleteUser = (userId: string) => {
        setConfirmAction({
            isOpen: true,
            title: 'Delete User',
            message: 'Are you sure you want to delete this user? They will be moved to the Recycle Bin.',
            variant: 'danger',
            onConfirm: async () => {
                try {
                    await usersService.delete(userId);
                    toast.success('User moved to Recycle Bin');
                    setConfirmAction(prev => ({ ...prev, isOpen: false }));
                    loadUsers();
                } catch (err: any) {
                    toast.error(err.response?.data?.message || 'Failed to delete user');
                }
            }
        });
    };

    const columns: Column<UserReadDto>[] = [
        {
            header: 'User',
            accessor: (u) => (
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
                    <div className="sidebar-avatar" style={{ width: '32px', height: '32px', fontSize: '0.75rem' }}>
                        {(u.fullName || u.email || 'U').charAt(0).toUpperCase()}
                    </div>
                    <span style={{ fontWeight: 600 }}>{u.fullName || u.email}</span>
                </div>
            ),
            sortable: true,
            sortKey: 'fullName'
        },
        { header: 'Email', accessor: 'email', sortable: true },
        { header: 'Department', accessor: (u) => u.department || '—', sortable: true, sortKey: 'department' },
        {
            header: 'Role',
            accessor: (u) => (
                <span className={`badge ${u.role === 'Admin' ? 'badge-primary' : 'badge-neutral'}`}>
                    {u.role}
                </span>
            ),
            sortable: true,
            sortKey: 'role'
        },
        {
            header: 'Actions',
            accessor: (u) => (
                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)' }}>
                    <Button
                        size="sm"
                        variant="secondary"
                        onClick={() => handleRoleChange(u.id, u.role || 'User')}
                    >
                        Switch Role
                    </Button>
                    <Button
                        size="sm"
                        variant="danger"
                        onClick={() => handleDeleteUser(u.id)}
                    >
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
                        <h1 className="page-title">User Management</h1>
                        <p className="page-subtitle">Manage system users and their access levels</p>
                    </div>
                    <Button onClick={() => setIsAdding(true)}>Add New User</Button>
                </div>
            </div>

            <DataTable
                data={users}
                columns={columns}
                searchPlaceholder="Search by name, email or department..."
            />

            {isAdding && (
                <div className="modal-overlay" onClick={() => setIsAdding(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Add New User</h2>
                            <button className="modal-close" onClick={() => setIsAdding(false)}>&times;</button>
                        </div>
                        <form onSubmit={handleCreateUser}>
                            <div className="modal-body">
                                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
                                    <Input
                                        label="Full Name"
                                        required
                                        value={newUser.fullName}
                                        onChange={e => setNewUser({ ...newUser, fullName: e.target.value })}
                                        placeholder="Enter full name"
                                    />
                                    <Input
                                        label="Email Address"
                                        type="email"
                                        required
                                        value={newUser.email}
                                        onChange={e => setNewUser({ ...newUser, email: e.target.value })}
                                        placeholder="user@example.com"
                                    />
                                </div>
                                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)', marginTop: 'var(--space-4)' }}>
                                    <Input
                                        label="Password"
                                        type="password"
                                        required
                                        value={newUser.password}
                                        onChange={e => setNewUser({ ...newUser, password: e.target.value })}
                                        placeholder="••••••••"
                                    />
                                    <Input
                                        label="Department"
                                        value={newUser.department}
                                        onChange={e => setNewUser({ ...newUser, department: e.target.value })}
                                        placeholder="e.g. Engineering"
                                    />
                                </div>
                                <div style={{ marginTop: 'var(--space-4)' }}>
                                    <label className="form-label">System Role</label>
                                    <select
                                        className="form-input"
                                        value={newUser.role}
                                        onChange={e => setNewUser({ ...newUser, role: e.target.value })}
                                    >
                                        <option value="User">User</option>
                                        <option value="Admin">Admin</option>
                                    </select>
                                </div>
                            </div>
                            <div className="modal-footer">
                                <Button type="button" variant="secondary" onClick={() => setIsAdding(false)}>Cancel</Button>
                                <Button type="submit" isLoading={isSubmitting}>Create User</Button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            <ConfirmModal
                isOpen={confirmAction.isOpen}
                title={confirmAction.title}
                message={confirmAction.message}
                variant={confirmAction.variant}
                onConfirm={confirmAction.onConfirm}
                onClose={() => setConfirmAction(prev => ({ ...prev, isOpen: false }))}
            />
        </div>
    );
};

export default AdminUsers;
