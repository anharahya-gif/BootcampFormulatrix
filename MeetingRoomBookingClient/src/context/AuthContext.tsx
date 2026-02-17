import React, { createContext, useState, useEffect, useContext } from 'react';
import { authService } from '../api/auth';
import type { LoginDto, RegisterDto, User } from '../types';
import { toast } from 'react-toastify';

interface AuthContextType {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (data: LoginDto) => Promise<User | null>;
    register: (data: RegisterDto) => Promise<User | null>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
    const [isLoading, setIsLoading] = useState<boolean>(true);

    useEffect(() => {
        // Check local storage for existing session
        const storedToken = localStorage.getItem('token');
        const storedUser = localStorage.getItem('user');

        if (storedToken && storedUser) {
            setToken(storedToken);
            setUser(JSON.parse(storedUser));
        }
        setIsLoading(false);
    }, []);

    const login = async (data: LoginDto): Promise<User | null> => {
        try {
            const response = await authService.login(data);
            if (response.success && response.token) {
                setToken(response.token);

                const userObj: User = {
                    id: '',
                    email: response.email || '',
                    userName: response.userName || '',
                    fullName: response.userName || '',
                    role: response.role || 'User'
                };

                setUser(userObj);
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(userObj));
                toast.success('Login successful');
                return userObj;
            } else {
                toast.error(response.errorMessage || 'Login failed');
                return null;
            }
        } catch (error: any) {
            toast.error(error.response?.data?.message || error.response?.data?.errorMessage || 'An error occurred during login');
            throw error;
        }
    };

    const register = async (data: RegisterDto): Promise<User | null> => {
        try {
            const response = await authService.register(data);
            if (response.success && response.token) {
                setToken(response.token);
                const userObj: User = {
                    id: '',
                    email: response.email || '',
                    userName: response.userName || '',
                    fullName: response.userName || '',
                    role: response.role || 'User'
                };
                setUser(userObj);
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(userObj));
                toast.success('Registration successful');
                return userObj;
            } else {
                toast.error(response.errorMessage || 'Registration failed');
                return null;
            }
        } catch (error: any) {
            toast.error(error.response?.data?.message || error.response?.data?.errorMessage || 'An error occurred during registration');
            throw error;
        }
    };

    const logout = () => {
        setUser(null);
        setToken(null);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        toast.info('Logged out');
    };

    return (
        <AuthContext.Provider value={{ user, token, isAuthenticated: !!token, isLoading, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
