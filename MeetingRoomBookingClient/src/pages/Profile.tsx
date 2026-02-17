import React, { useEffect, useState } from 'react';
import { profileService } from '../api/profile';
import type { UserReadDto, UserUpdateDto } from '../types';
import Button from '../components/Button';
import Input from '../components/Input';
import { toast } from 'react-toastify';

const Profile: React.FC = () => {
    const [profile, setProfile] = useState<UserReadDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState<UserUpdateDto>({
        fullName: '',
        department: '',
        phoneNumber: '',
        avatarUrl: ''
    });

    const loadProfile = async () => {
        try {
            const data = await profileService.getProfile();
            setProfile(data);
            setFormData({
                fullName: data.fullName || '',
                department: data.department || '',
                phoneNumber: data.phoneNumber || '',
                avatarUrl: data.avatarUrl || ''
            });
        } catch (err) {
            toast.error('Failed to load profile');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadProfile();
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            await profileService.updateProfile(formData);
            toast.success('Profile updated successfully');
            // We should ideally update the AuthContext user object too if fullName changed
            // But for now, just reloading the profile data is fine
            loadProfile();
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Failed to update profile');
        } finally {
            setIsSubmitting(false);
        }
    };

    if (loading) {
        return <div className="loading-container"><div className="spinner" /></div>;
    }

    return (
        <div className="page-content">
            <div className="page-header" style={{ maxWidth: '800px', margin: '0 auto var(--space-8)' }}>
                <h1 className="page-title">My Profile</h1>
                <p className="page-subtitle">View and update your personal information</p>
            </div>

            <div className="profile-container" style={{ maxWidth: '800px', margin: '0 auto' }}>
                <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr) 2fr', gap: 'var(--space-8)' }}>
                    {/* Left: Info Card */}
                    <div className="card" style={{ height: 'fit-content' }}>
                        <div style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
                            <div className="sidebar-avatar" style={{ width: '80px', height: '80px', fontSize: '2rem', margin: '0 auto var(--space-4)' }}>
                                {(profile?.fullName || profile?.email || 'U').charAt(0).toUpperCase()}
                            </div>
                            <h2 style={{ fontSize: '1.25rem', fontWeight: 600, marginBottom: 'var(--space-1)' }}>{profile?.fullName}</h2>
                            <p style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>{profile?.email}</p>

                            <div style={{ marginTop: 'var(--space-6)', display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
                                <div className={`badge ${profile?.role === 'Admin' ? 'badge-primary' : 'badge-neutral'}`} style={{ alignSelf: 'center' }}>
                                    {profile?.role}
                                </div>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                                    Member since {new Date(profile?.createdAt || '').toLocaleDateString()}
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Right: Edit Form */}
                    <div className="card">
                        <form onSubmit={handleSubmit}>
                            <div style={{ display: 'grid', gap: 'var(--space-6)' }}>
                                <Input
                                    label="Full Name"
                                    required
                                    value={formData.fullName || ''}
                                    onChange={e => setFormData({ ...formData, fullName: e.target.value })}
                                    placeholder="Enter your full name"
                                />
                                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
                                    <Input
                                        label="Department"
                                        value={formData.department || ''}
                                        onChange={e => setFormData({ ...formData, department: e.target.value })}
                                        placeholder="e.g. Engineering"
                                    />
                                    <Input
                                        label="Phone Number"
                                        value={formData.phoneNumber || ''}
                                        onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })}
                                        placeholder="+62..."
                                    />
                                </div>

                                <div style={{ paddingTop: 'var(--space-4)', borderTop: '1px solid var(--border-color)', display: 'flex', justifyContent: 'flex-end' }}>
                                    <Button type="submit" isLoading={isSubmitting}>
                                        Save Changes
                                    </Button>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Profile;
