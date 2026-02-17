import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Logo: React.FC = () => (
    <svg width="36" height="36" viewBox="0 0 36 36" fill="none">
        <circle cx="6" cy="6" r="5" fill="#F59E0B" />
        <circle cx="18" cy="6" r="5" fill="#FFFFFF" />
        <circle cx="30" cy="6" r="5" fill="#FFFFFF" />
        <circle cx="6" cy="18" r="5" fill="#FFFFFF" />
        <circle cx="18" cy="18" r="5" fill="#FFFFFF" />
        <circle cx="30" cy="18" r="5" fill="#F59E0B" />
        <circle cx="6" cy="30" r="5" fill="#FFFFFF" />
        <circle cx="18" cy="30" r="5" fill="#FFFFFF" />
        <circle cx="30" cy="30" r="5" fill="#F59E0B" />
    </svg>
);

const HomeIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1" /></svg>
);
const RoomIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5" /></svg>
);
const CalendarIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
);
const AdminIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
);
const UsersIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" /></svg>
);
const BookingIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" /></svg>
);
const TrashIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
);
const LogoutIcon = () => (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" /></svg>
);

const Sidebar: React.FC = () => {
    const { user, isAuthenticated, logout } = useAuth();
    const location = useLocation();
    const isAdmin = user?.role === 'Admin';

    const isActive = (path: string) => location.pathname === path;

    if (!isAuthenticated) return null;

    return (
        <aside className="sidebar">
            <div className="sidebar-logo">
                <Logo />
                <div>
                    <div className="sidebar-logo-text">Formulatrix</div>
                    <div className="sidebar-logo-sub">Meeting Rooms</div>
                </div>
            </div>

            <nav className="sidebar-nav">
                <div className="sidebar-section">
                    <div className="sidebar-section-title">Menu</div>
                    <Link to="/" className={`sidebar-link ${isActive('/') ? 'active' : ''}`}>
                        <HomeIcon /> Dashboard
                    </Link>
                    <Link to="/rooms" className={`sidebar-link ${isActive('/rooms') ? 'active' : ''}`}>
                        <RoomIcon /> Meeting Rooms
                    </Link>
                    <Link to="/my-bookings" className={`sidebar-link ${isActive('/my-bookings') ? 'active' : ''}`}>
                        <CalendarIcon /> My Bookings
                    </Link>
                </div>

                {isAdmin && (
                    <div className="sidebar-section">
                        <div className="sidebar-section-title">Administration</div>
                        <Link to="/admin" className={`sidebar-link ${isActive('/admin') ? 'active' : ''}`}>
                            <AdminIcon /> Dashboard
                        </Link>
                        <Link to="/admin/rooms" className={`sidebar-link ${isActive('/admin/rooms') ? 'active' : ''}`}>
                            <RoomIcon /> Manage Rooms
                        </Link>
                        <Link to="/admin/bookings" className={`sidebar-link ${isActive('/admin/bookings') ? 'active' : ''}`}>
                            <BookingIcon /> Manage Bookings
                        </Link>
                        <Link to="/admin/users" className={`sidebar-link ${isActive('/admin/users') ? 'active' : ''}`}>
                            <UsersIcon /> Users
                        </Link>
                        <Link to="/admin/recycle-bin" className={`sidebar-link ${isActive('/admin/recycle-bin') ? 'active' : ''}`}>
                            <TrashIcon /> Recycle Bin
                        </Link>
                    </div>
                )}
            </nav>

            <div className="sidebar-footer">
                <div className="sidebar-user">
                    <Link to="/profile" className="sidebar-user-link" style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', flex: 1, textDecoration: 'none', color: 'inherit' }}>
                        <div className="sidebar-avatar">
                            {(user?.fullName || user?.email || 'U').charAt(0).toUpperCase()}
                        </div>
                        <div className="sidebar-user-info">
                            <div className="sidebar-user-name">{user?.fullName || user?.email}</div>
                            <div className="sidebar-user-role">{user?.role || 'User'}</div>
                        </div>
                    </Link>
                    <button className="btn-ghost btn-icon" onClick={logout} title="Logout">
                        <LogoutIcon />
                    </button>
                </div>
            </div>
        </aside>
    );
};

export default Sidebar;
