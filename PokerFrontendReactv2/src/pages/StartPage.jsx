import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiService from '../services/apiService';

const StartPage = () => {
    // Auth State
    const [isLoggedIn, setIsLoggedIn] = useState(!!sessionStorage.getItem('poker_token'));
    const [authMode, setAuthMode] = useState('login'); // 'login' or 'register'
    const [username, setUsername] = useState(sessionStorage.getItem('poker_player_name') || '');
    const [password, setPassword] = useState('');
    const [balance, setBalance] = useState(0);

    // Lobby State
    const [tables, setTables] = useState([]);
    const [depositAmount, setDepositAmount] = useState(1000);

    // UX State
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        if (isLoggedIn) {
            fetchTables();
            fetchBalance();
        }
    }, [isLoggedIn]);

    const fetchBalance = async () => {
        try {
            const response = await apiService.getProfile();
            setBalance(response.data.balance);
        } catch (err) {
            console.error("Failed to fetch balance:", err);
        }
    };

    const fetchTables = async () => {
        try {
            const response = await apiService.getTables();
            setTables(response.data || []);
        } catch (err) {
            console.error("Failed to fetch tables:", err);
        }
    };

    const handleAuth = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            if (authMode === 'register') {
                await apiService.register(username, password);
                alert('Registration successful! Please login.');
                setAuthMode('login');
                setPassword('');
            } else {
                const response = await apiService.login(username, password);
                const { token } = response.data;

                sessionStorage.setItem('poker_token', token);
                sessionStorage.setItem('poker_player_name', username);

                setIsLoggedIn(true);
                // fetchBalance will be triggered by useEffect
            }
        } catch (err) {
            console.error("Auth failed:", err);
            setError(err.response?.data?.message || err.response?.data || `${authMode === 'register' ? 'Registration' : 'Login'} failed.`);
        } finally {
            setLoading(false);
        }
    };

    const handleDeposit = async () => {
        setLoading(true);
        try {
            await apiService.deposit(depositAmount);
            await fetchBalance();
            alert(`Deposited ${depositAmount}!`);
        } catch (err) {
            setError('Deposit failed.');
        } finally {
            setLoading(false);
        }
    };

    const handleJoinTable = async (tableId, tableName) => {
        sessionStorage.setItem('poker_table_id', tableId);
        sessionStorage.setItem('poker_table_name', tableName);
        navigate('/table');
    };

    const handleCreateTable = async () => {
        try {
            const name = prompt("Enter table name:", "New Table");
            if (!name) return;
            await apiService.createTable({ name, maxPlayers: 8 });
            await fetchTables();
        } catch (err) {
            console.error("Failed to create table:", err);
            alert("Failed to create table.");
        }
    };

    if (!isLoggedIn) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-poker-dark bg-[url('https://www.transparenttextures.com/patterns/cubes.png')]">
                <div className="bg-black/60 p-8 rounded-2xl shadow-2xl backdrop-blur-md border border-poker-gold/20 w-full max-w-md">
                    <h1 className="text-4xl font-bold text-center text-white mb-2 flex items-center justify-center gap-2 uppercase tracking-tighter italic">
                        <img src="/icon/icon-card.png" alt="icon" className="w-8 h-8" />
                        Poker API
                        <img src="/icon/icon-card.png" alt="icon" className="w-8 h-8" />
                    </h1>
                    <p className="text-center text-gray-400 mb-8 font-medium">
                        {authMode === 'login' ? 'Login to your account' : 'Create a new account'}
                    </p>

                    <form onSubmit={handleAuth} className="space-y-6">
                        <div>
                            <label className="block text-xs font-black text-gray-400 mb-2 uppercase tracking-widest">Username</label>
                            <input
                                type="text"
                                required
                                className="w-full px-4 py-3 bg-gray-800 border border-gray-700 rounded-lg focus:ring-2 focus:ring-poker-gold text-white outline-none transition-all"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-black text-gray-400 mb-2 uppercase tracking-widest">Password</label>
                            <input
                                type="password"
                                required
                                className="w-full px-4 py-3 bg-gray-800 border border-gray-700 rounded-lg focus:ring-2 focus:ring-poker-gold text-white outline-none transition-all"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                        </div>

                        {error && <div className="text-red-500 text-sm text-center bg-red-900/20 p-2 rounded border border-red-500/20">{error}</div>}

                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full py-4 bg-gradient-to-r from-poker-gold to-yellow-600 hover:from-yellow-400 hover:to-yellow-500 text-black font-black rounded-xl shadow-lg uppercase tracking-widest transition-all active:scale-95"
                        >
                            {loading ? 'Processing...' : authMode === 'login' ? 'Login' : 'Register'}
                        </button>
                    </form>

                    <div className="mt-8 text-center">
                        <p className="text-gray-500 text-sm">
                            {authMode === 'login' ? "Don't have an account?" : "Already have an account?"}
                            <button
                                onClick={() => setAuthMode(authMode === 'login' ? 'register' : 'login')}
                                className="ml-2 text-poker-gold font-bold hover:underline"
                            >
                                {authMode === 'login' ? 'Register here' : 'Login here'}
                            </button>
                        </p>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-poker-dark p-8">
            <div className="max-w-4xl mx-auto space-y-8">
                {/* Header / User Profile */}
                <div className="bg-black/40 p-6 rounded-2xl border border-poker-gold/20 flex justify-between items-center backdrop-blur-md">
                    <div>
                        <h2 className="text-2xl font-bold text-white">Welcome, {username}!</h2>
                        <p className="text-poker-gold font-mono text-xl">Balance: ${balance}</p>
                    </div>
                    <div className="flex gap-4 items-end">
                        <div className="flex flex-col">
                            <label className="text-xs text-gray-400 mb-1 uppercase font-bold">Deposit Amount</label>
                            <input
                                type="number"
                                className="w-32 px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-white font-mono"
                                value={depositAmount}
                                onChange={(e) => setDepositAmount(parseInt(e.target.value))}
                            />
                        </div>
                        <button
                            onClick={handleDeposit}
                            disabled={loading}
                            className="px-6 py-2 bg-green-600 hover:bg-green-500 text-white font-black rounded-lg h-[42px] transition-all"
                        >
                            Deposit
                        </button>
                    </div>
                </div>

                {/* Lobby / Table List */}
                <div className="space-y-4">
                    <div className="flex justify-between items-end border-b border-white/5 pb-4">
                        <h3 className="text-3xl font-black text-white italic tracking-tighter">LOBBY</h3>
                        <div className="flex gap-4">
                            <button onClick={handleCreateTable} className="bg-poker-gold/20 hover:bg-poker-gold text-poker-gold hover:text-black px-4 py-1 rounded-lg text-xs font-black uppercase transition-all">Create Table</button>
                            <button onClick={fetchTables} className="text-poker-gold hover:text-yellow-400 text-xs font-black uppercase tracking-widest mt-2">Refresh</button>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {tables.length > 0 ? tables.map(table => (
                            <div key={table.tableId} className="bg-gray-900/80 p-6 rounded-xl border border-white/5 hover:border-poker-gold/50 transition-all group flex justify-between items-center shadow-xl">
                                <div>
                                    <h4 className="text-xl font-bold text-white group-hover:text-poker-gold transition-colors">{table.name}</h4>
                                    <p className="text-gray-500 text-sm uppercase font-bold tracking-tighter">Players: {table.playerCount}/{table.maxPlayers}</p>
                                </div>
                                <button
                                    onClick={() => handleJoinTable(table.tableId, table.name)}
                                    className="px-6 py-2 bg-poker-gold text-black font-black rounded-lg hover:scale-110 active:scale-95 transition-all shadow-lg"
                                >
                                    Join
                                </button>
                            </div>
                        )) : (
                            <div className="col-span-full text-center py-20 bg-black/20 rounded-2xl border border-dashed border-gray-800">
                                <p className="text-gray-500 font-medium">No active tables found. Click refresh to check again.</p>
                            </div>
                        )}
                    </div>
                </div>

                <div className="pt-8 text-center border-t border-white/5">
                    <button
                        onClick={() => {
                            sessionStorage.clear();
                            setIsLoggedIn(false);
                            setUsername('');
                        }}
                        className="text-gray-600 hover:text-red-400 transition-all text-xs font-bold uppercase tracking-[0.2em]"
                    >
                        Sign Out
                    </button>
                </div>
            </div>
        </div>
    );
};

export default StartPage;
