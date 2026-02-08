import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiService from '../services/apiService';

const StartPage = () => {
    const [name, setName] = useState('');
    const [chips, setChips] = useState(1000);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    /**
     * handleSubmit: Registers the player and initializes their chip stack before entering the table.
     */
    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            // 1. Register Player via API
            // Note: In a real app we might check if player exists or just register.
            // The requirement says "Save to session storage".

            // We will try to register. If it fails (e.g. name taken), we might handle it.
            // But for simplicity/requirement flow:

            sessionStorage.setItem('poker_player_name', name);
            sessionStorage.setItem('poker_player_chips', chips);

            // Optional: Call register API here or do it when joining table. 
            // Requirement 2 says "Join table using session data".
            // Use RegisterPlayer API to reserve the name if possible, or just proceed.

            await apiService.registerPlayer(name, chips).catch(err => {
                // Ignore if already registered or handle specifically
                console.warn("Registration check:", err);
            });

            navigate('/table');

        } catch (err) {
            setError('Failed to start game. Please try again.');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-poker-dark bg-[url('https://www.transparenttextures.com/patterns/cubes.png')]">
            <div className="bg-black/60 p-8 rounded-2xl shadow-2xl backdrop-blur-md border border-poker-gold/20 w-full max-w-md">
                <h1 className="text-4xl font-bold text-center text-white mb-2">♠ Poker API ♥</h1>
                <p className="text-center text-gray-400 mb-8">Enter your details to join the table</p>

                <form onSubmit={handleSubmit} className="space-y-6">
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-1">Player Name</label>
                        <input
                            type="text"
                            required
                            maxLength={15}
                            className="w-full px-4 py-3 bg-gray-800 border border-gray-700 rounded-lg focus:ring-2 focus:ring-poker-gold focus:border-transparent text-white placeholder-gray-500 outline-none transition-all"
                            placeholder="e.g. Maverick"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-1">Initial Chips</label>
                        <input
                            type="number"
                            min="100"
                            max="100000"
                            className="w-full px-4 py-3 bg-gray-800 border border-gray-700 rounded-lg focus:ring-2 focus:ring-poker-gold focus:border-transparent text-white outline-none transition-all"
                            value={chips}
                            onChange={(e) => setChips(parseInt(e.target.value))}
                        />
                    </div>

                    {error && <div className="text-red-500 text-sm text-center bg-red-900/20 p-2 rounded">{error}</div>}

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full py-3 px-4 bg-gradient-to-r from-poker-gold to-yellow-600 hover:from-yellow-400 hover:to-yellow-500 text-black font-bold rounded-lg shadow-lg transform transition-all hover:scale-[1.02] active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {loading ? 'Joining...' : 'Start Game'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default StartPage;
