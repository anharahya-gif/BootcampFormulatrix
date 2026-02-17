import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Button from './Button';
import { useAuth } from '../context/AuthContext';

const Navbar: React.FC = () => {
    const { user, logout, isAuthenticated } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <nav className="bg-white border-b border-gray-200 sticky top-0 z-50">
            <div className="container flex justify-between items-center h-16">
                <Link to="/" className="text-xl font-bold text-indigo-600">
                    RoomBooking
                </Link>

                <div className="flex items-center gap-md">
                    {isAuthenticated ? (
                        <>
                            <Link to="/" className="text-slate-600 hover:text-indigo-600 font-medium">Rooms</Link>
                            <Link to="/my-bookings" className="text-slate-600 hover:text-indigo-600 font-medium">My Bookings</Link>
                            {user?.role === 'Admin' && (
                                <Link to="/admin" className="text-slate-600 hover:text-indigo-600 font-medium">Admin</Link>
                            )}
                            <div className="flex items-center gap-sm ml-4 border-l pl-4 border-gray-200">
                                <span className="text-sm text-slate-500 hidden md:block">Hi, {user?.fullName || user?.email}</span>
                                <Button variant="secondary" onClick={handleLogout} className="text-sm py-1 px-3">
                                    Logout
                                </Button>
                            </div>
                        </>
                    ) : (
                        <>
                            <Link to="/login">
                                <Button variant="secondary" className="mr-2">Login</Button>
                            </Link>
                            <Link to="/register">
                                <Button variant="primary">Register</Button>
                            </Link>
                        </>
                    )}
                </div>
            </div>
        </nav>
    );
};

export default Navbar;
