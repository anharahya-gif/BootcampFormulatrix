import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Input from '../components/Input';
import Button from '../components/Button';
import type { RegisterDto } from '../types';

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

const Register: React.FC = () => {
    const { register } = useAuth();
    const navigate = useNavigate();
    const [formData, setFormData] = useState<RegisterDto>({
        email: '',
        password: '',
        fullName: '',
        department: '',
        phoneNumber: ''
    });
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError(null);
        try {
            await register(formData);
            navigate('/');
        } catch (err) {
            setError('Registration failed. Please try again.');
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
                        Join the workspace of tomorrow.
                    </h1>
                    <p className="auth-branding-description">
                        Create an account to unlock seamless scheduling and resource management.
                        Experience productivity like never before.
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
                        <h2 className="auth-title">Create account</h2>
                        <p className="auth-subtitle">Get started with your room bookings</p>

                        {error && (
                            <div className="form-error-banner">
                                {error}
                            </div>
                        )}

                        <form onSubmit={handleSubmit}>
                            <Input
                                label="Full Name"
                                placeholder="John Doe"
                                value={formData.fullName}
                                onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                                required
                            />
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
                                placeholder="Min. 6 characters"
                                value={formData.password}
                                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                required
                            />
                            <div className="form-row">
                                <Input
                                    label="Department"
                                    placeholder="Engineering"
                                    value={formData.department}
                                    onChange={(e) => setFormData({ ...formData, department: e.target.value })}
                                />
                                <Input
                                    label="Phone"
                                    placeholder="+62..."
                                    value={formData.phoneNumber}
                                    onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                                />
                            </div>
                            <Button type="submit" className="w-full mt-4" isLoading={isLoading}>
                                Create Account
                            </Button>
                        </form>

                        <div className="auth-footer">
                            Already have an account? <Link to="/login">Sign in</Link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Register;
