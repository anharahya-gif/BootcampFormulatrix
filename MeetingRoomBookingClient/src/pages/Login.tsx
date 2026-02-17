import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Input from '../components/Input';
import Button from '../components/Button';
import type { LoginDto } from '../types';

const Logo: React.FC = () => (
    <svg width="48" height="48" viewBox="0 0 36 36" fill="none">
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

const Login: React.FC = () => {
    const { login } = useAuth();
    const navigate = useNavigate();
    const [formData, setFormData] = useState<LoginDto>({ email: '', password: '' });
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError(null);
        try {
            const user = await login(formData);
            if (user?.role === 'Admin') {
                navigate('/admin');
            } else {
                navigate('/');
            }
        } catch (err) {
            setError('Invalid email or password');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="auth-container">
            {/* Left: Branding Section */}
            <div className="auth-branding-section">
                <div className="auth-branding-content">
                    <div className="auth-branding-logo">
                        <Logo />
                        <div className="auth-branding-logo-text">Formulatrix</div>
                    </div>
                    <h1 className="auth-branding-title">
                        Efficient spaces for brilliant ideas.
                    </h1>
                    <p className="auth-branding-description">
                        Streamline your office experience with our intelligent room booking system.
                        Manage meetings, collaborate with ease, and focus on what matters.
                    </p>
                </div>
            </div>

            {/* Right: Form Section */}
            <div className="auth-form-section">
                <div className="auth-form-container">
                    {/* Mobile Logo Only */}
                    <div className="auth-mobile-logo">
                        <div className="flex flex-col items-center">
                            <Logo />
                            <div className="auth-logo-text">Formulatrix</div>
                        </div>
                    </div>

                    <div className="auth-form-card">
                        <h2 className="auth-title">Welcome back</h2>
                        <p className="auth-subtitle">Sign in to manage your bookings</p>

                        <form onSubmit={handleSubmit}>
                            <Input
                                label="Email"
                                type="email"
                                placeholder="you@company.com"
                                value={formData.email}
                                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                required
                            />
                            <Input
                                label="Password"
                                type="password"
                                placeholder="••••••••"
                                value={formData.password}
                                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                required
                                error={error || undefined}
                            />
                            <Button type="submit" className="w-full mt-4" isLoading={isLoading}>
                                Sign In
                            </Button>
                        </form>

                        <div className="auth-footer">
                            Don't have an account? <Link to="/register">Create one</Link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Login;
