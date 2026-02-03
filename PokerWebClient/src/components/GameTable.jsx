import { useEffect, useState } from 'react';

export default function GameTable({ playerName }) {
    const [gameState, setGameState] = useState(null);
    const [loading, setLoading] = useState(true);

    const fetchState = async () => {
        try {
            const res = await fetch('/api/GameControllerAPI/state');
            if (res.ok) {
                const data = await res.json();
                setGameState(data);
            }
        } catch (err) {
            console.error("Error fetching state:", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchState();
        const interval = setInterval(fetchState, 1000); // Poll every second
        return () => clearInterval(interval);
    }, []);

    const handleAction = async (action, params = '') => {
        try {
            await fetch(`/api/GameControllerAPI/${action}?name=${playerName}${params}`, { method: 'POST' });
            fetchState();
        } catch (err) {
            console.error("Action failed:", err);
        }
    };

    if (loading) return <div>Loading table...</div>;
    if (!gameState) return <div>Error loading game state</div>;

    const myPlayer = gameState.players.find(p => p.name === playerName);
    const isMyTurn = gameState.currentPlayer === playerName;

    return (
        <div className="game-container">
            <div className="status-bar" style={{ marginBottom: '1rem' }}>
                <span>Phase: {gameState.phase}</span> |
                <span> Pot: ${gameState.pot}</span> |
                <span> Current Bet: ${gameState.currentBet}</span>
            </div>

            <div className="poker-table">
                {/* Community Cards */}
                <div className="community-cards" style={{ position: 'absolute', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', display: 'flex', gap: '0.5rem' }}>
                    {gameState.communityCards.map((card, idx) => (
                        <div key={idx} className="card">{parseCard(card)}</div>
                    ))}
                </div>

                {/* Players */}
                {gameState.players.map((player, idx) => {
                    const angle = (360 / gameState.players.length) * idx;
                    const radius = 220; // Distance from center
                    const x = radius * Math.cos(angle * Math.PI / 180);
                    const y = radius * Math.sin(angle * Math.PI / 180);

                    return (
                        <div key={player.name} style={{
                            position: 'absolute',
                            top: `calc(50% + ${y}px)`,
                            left: `calc(50% + ${x}px)`,
                            transform: 'translate(-50%, -50%)',
                            background: player.name === playerName ? '#444' : '#222',
                            padding: '0.5rem',
                            borderRadius: '8px',
                            border: gameState.currentPlayer === player.name ? '2px solid gold' : '1px solid #555',
                            textAlign: 'center',
                            minWidth: '80px'
                        }}>
                            <div style={{ fontWeight: 'bold' }}>{player.name}</div>
                            <div>${player.chipStack}</div>
                            {player.currentBet > 0 && <div style={{ color: 'yellow' }}>Bet: {player.currentBet}</div>}
                            <div style={{ fontSize: '0.8em', color: '#aaa' }}>{player.state}</div>

                            {/* Hand */}
                            {player.hand && player.hand.length > 0 && (
                                <div style={{ display: 'flex', gap: '2px', justifyContent: 'center', marginTop: '4px' }}>
                                    {player.hand.map((c, i) => (
                                        <div key={i} className="card" style={{ width: '30px', height: '45px', fontSize: '0.8em' }}>{parseCard(c)}</div>
                                    ))}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Controls */}
            <div className="controls" style={{ marginTop: '2rem', display: 'flex', gap: '1rem', justifyContent: 'center' }}>
                {gameState.gameState === 'WaitingForPlayers' && (
                    <button onClick={() => fetch('/api/GameControllerAPI/startRound', { method: 'POST' })}>Start Round</button>
                )}

                {isMyTurn && (
                    <>
                        <button onClick={() => handleAction('check')}>Check</button>
                        <button onClick={() => handleAction('call')}>Call</button>
                        <button onClick={() => handleAction('fold')}>Fold</button>
                        <button onClick={() => {
                            const amount = prompt("Raise amount?");
                            if (amount) handleAction('raise', `&amount=${amount}`);
                        }}>Raise</button>
                        <button onClick={() => handleAction('allin')}>All In</button>
                    </>
                )}
            </div>
            {gameState.showdown && (
                <div className="showdown-result" style={{ marginTop: '1rem', padding: '1rem', background: '#333', borderRadius: '8px' }}>
                    <h3>Round Over</h3>
                    <p>{gameState.showdown.message}</p>
                    <p>Winner: {gameState.showdown.winners.join(', ')} ({gameState.showdown.handRank})</p>
                </div>
            )}
        </div>
    );
}

// Helper to parse "Ace of Spades" -> symbol
function parseCard(cardStr) {
    if (!cardStr) return "?";
    const parts = cardStr.split(' of ');
    const rank = parts[0]; // Ace, Two, etc.
    const suit = parts[1]; // Spades, Hearts, etc.

    const suitIcons = { 'Spades': '♠', 'Hearts': '♥', 'Diamonds': '♦', 'Clubs': '♣' };
    const rankMap = { 'Two': '2', 'Three': '3', 'Four': '4', 'Five': '5', 'Six': '6', 'Seven': '7', 'Eight': '8', 'Nine': '9', 'Ten': '10', 'Jack': 'J', 'Queen': 'Q', 'King': 'K', 'Ace': 'A' };

    return `${rankMap[rank] || rank}${suitIcons[suit] || suit}`;
}
