import { useState } from 'react';

export default function Lobby({ onJoin }) {
    const [name, setName] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!name) return;

        try {
            const res = await fetch(`/api/GameControllerAPI/addPlayer?name=${name}&chips=1000`, {
                method: 'POST'
            });

            const data = await res.json();

            // If success or if error is "Player name already exists" (we can rejoin as that player for now for simplicity, or handle error)
            if (res.ok || (data.message && data.message.includes('exists'))) {
                onJoin(name);
            } else {
                setError(data.message || 'Failed to join');
            }
        } catch (err) {
            console.error(err);
            setError('Connection error');
        }
    };

    return (
        <div className="lobby-container">
            <h1>Poker Night</h1>
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem', maxWidth: '300px', margin: '0 auto' }}>
                <input
                    type="text"
                    placeholder="Enter your name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid #444', background: '#222', color: 'white' }}
                />
                <button type="submit">Join Table</button>
                {error && <p style={{ color: 'red' }}>{error}</p>}
            </form>
        </div>
    );
}
