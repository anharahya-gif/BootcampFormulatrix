import React, { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import Seat from '../components/Seat';
import CommunityCards from '../components/CommunityCards';
import GameControls from '../components/GameControls';
import Loader from '../components/Loader';
import apiService from '../services/apiService';
import signalrService from '../services/signalrService';

const TablePage = () => {
    const navigate = useNavigate();
    const [gameState, setGameState] = useState(null);
    const [loading, setLoading] = useState(true);
    const [messages, setMessages] = useState([]);

    // Sound effects
    const cardAudio = useRef(new Audio('/bgm/card-bgm.mp3'));
    const chipAudio = useRef(new Audio('/bgm/chip-bgm.mp3'));
    const prevCommunityCount = useRef(0);
    const prevPot = useRef(0);

    // Session Data
    const playerName = sessionStorage.getItem('poker_player_name');
    const playerChips = parseInt(sessionStorage.getItem('poker_player_chips') || '0');

    useEffect(() => {
        cardAudio.current.volume = 1.0;
        chipAudio.current.volume = 1.0; // High volume as requested
    }, []);

    useEffect(() => {
        if (!playerName) {
            navigate('/');
            return;
        }

        const playCardSound = () => {
            cardAudio.current.currentTime = 0;
            cardAudio.current.play().catch(e => console.log("Audio play deferred:", e));
        };

        const playChipSound = () => {
            chipAudio.current.currentTime = 0;
            chipAudio.current.play().catch(e => console.log("Audio play deferred:", e));
        };

        const init = async () => {
            try {
                // 1. Start SignalR
                await signalrService.startConnection();

                // 2. Subscribe to events
                signalrService.on('ReceiveGameState', (state) => {
                    console.log("Game State Received:", state);

                    // Detect if community cards increased
                    const newCount = state?.communityCards?.length || 0;
                    if (newCount > prevCommunityCount.current) {
                        playCardSound();
                    }
                    prevCommunityCount.current = newCount;

                    // Detect if pot increased
                    const newPot = state?.pot || 0;
                    if (newPot > prevPot.current) {
                        playChipSound();
                    }
                    prevPot.current = newPot;

                    setGameState(state);
                    setLoading(false);
                });

                signalrService.on('CommunityCardsUpdated', (d) => {
                    console.log("Community Cards Updated:", d.communityCards);
                    // Just play sound if we get this explicit event too
                    playCardSound();
                });

                signalrService.on('ShowdownCompleted', (details) => {
                    console.log("Showdown!", details);
                    // Reset community count ref so next round starts fresh
                    prevCommunityCount.current = 0;

                    // Navigate to winner page after a short delay
                    setTimeout(() => {
                        const winnerNames = details.winners || details.Winners || [];
                        const rawPlayers = details.players || details.Players || [];
                        const totalPot = details.pot || details.Pot || 0;

                        navigate('/winner', {
                            state: {
                                winners: rawPlayers.filter(p => {
                                    const pName = p.name || p.Name;
                                    return winnerNames.includes(pName);
                                }) || [],
                                allPlayers: rawPlayers,
                                communityCards: details.communityCards || details.CommunityCards || [],
                                handRank: details.handRank || details.rank || details.Rank || "Winner",
                                pot: totalPot,
                                message: details.message || details.Message || ""
                            }
                        });
                    }, 1000);
                });

                signalrService.on('ReceiveMessage', (msg) => {
                    setMessages(prev => [...prev.slice(-4), msg]);
                });

                // 3. Join Seat automatically (Find first empty or just join)
                // We'll try to join. If we are already there, it might fail or be idempotent.
                // We need to know if we are already in the game state.

                // Get initial state
                try {
                    const response = await apiService.getGameState();
                    const initialState = response.data;
                    setGameState(initialState);
                    // Looking at Controller: BuildGameStateDto returns object. 
                    // Let's assume response.data IS the state object.

                    // Logic to join if not present
                    const amIInGame = initialState.players?.some(p => p.name === playerName);
                    if (!amIInGame) {
                        try {
                            // Try to join at a random seat or just use joinSeat endpoint
                            // We need to find an empty seat from the state?
                            // Backend `addPlayer` adds to next available if seatIndex is -1 in some implementations, 
                            // but Controller `addPlayer` takes seatIndex defaults -1.
                            // Controller JoinSeat is for registered players. RegisterPlayer is what we probably did?
                            // Let's try `joinSeat` at first available index if we registered, OR `addPlayer` if we didn't.

                            // Try addPlayer (which registers + seats?)
                            // Controller `addPlayer` calls `_game.AddPlayer`.
                            // Controller `registerPlayer` calls `_game.RegisterPlayer` (no seat).
                            // Let's use `addPlayer` for simplicity as it puts us in a seat.

                            await apiService.joinSeat(playerName, -1).catch(async () => {
                                // If joinSeat fails, maybe we aren't registered? Try adding.
                                await apiService.registerPlayer(playerName, playerChips);
                                await apiService.joinSeat(playerName, -1);
                            });
                        } catch (e) {
                            console.error("Auto-join failed:", e);
                        }
                    }
                } catch (e) {
                    console.error("Failed to fetch initial state:", e);
                }

                setLoading(false);

            } catch (err) {
                console.error("Init failed:", err);
                setLoading(false);
            }
        };

        init();

        return () => {
            signalrService.off('ReceiveGameState');
            signalrService.off('ReceiveMessage');
            signalrService.off('ShowdownCompleted');
            signalrService.off('CommunityCardsUpdated');
        };
    }, []);

    const handleAction = async (action, amount) => {
        try {
            switch (action) {
                case 'bet': await apiService.bet(playerName, amount); break;
                case 'call': await apiService.call(playerName); break;
                case 'check': await apiService.check(playerName); break;
                case 'fold': await apiService.fold(playerName); break;
                case 'raise': await apiService.raise(playerName, amount); break;
                case 'allin': await apiService.allIn(playerName); break;
                default: break;
            }
        } catch (err) {
            console.error("Action failed:", err);
            alert("Action failed: " + (err.response?.data?.message || err.message));
        }
    };

    const handleStandUp = async () => {
        if (!window.confirm("Are you sure you want to stand up?")) return;
        try {
            await apiService.removePlayer(playerName);
            setGameState(prev => ({
                ...prev,
                players: prev.players.filter(p => p.name !== playerName)
            }));
            navigate('/'); // or stay and watch? User request implies just stand up.
            // Requirement: "jika current player standup maka current player akan otomatis ke player selanjutnya"
            // This is backend logic, but we trigger it here.
        } catch (err) {
            alert("Stand up failed: " + (err.response?.data?.message || err.message));
        }
    };

    const handleAddChips = async (amount) => {
        try {
            await apiService.addChips(playerName, amount);
        } catch (err) {
            alert("Add chips failed: " + (err.response?.data?.message || err.message));
        }
    };

    const handleStartGame = async () => {
        try {
            await apiService.startRound();
        } catch (err) {
            alert("Start failed: " + (err.response?.data?.message || err.message));
        }
    };

    const handleJoinSeat = async (seatIndex) => {
        try {
            const result = await apiService.joinSeat(playerName, seatIndex);
            if (result.data && !result.data.isSuccess) {
                alert(result.data.message);
            }
        } catch (err) {
            alert("Join seat failed: " + (err.response?.data?.message || err.message));
        }
    };

    if (loading) return <div className="min-h-screen bg-poker-dark flex items-center justify-center"><Loader text="Connecting to Table..." /></div>;

    // Fixed 8 seats layout
    const seatPositions = [
        "bottom-10 right-1/3 translate-x-12", // Seat 0 (Bottom Right)
        "bottom-10 left-1/3 -translate-x-12", // Seat 1 (Bottom Left)
        "left-4 top-1/2 translate-y-20",      // Seat 2 (Left Bottom)
        "left-4 top-1/2 -translate-y-20",     // Seat 3 (Left Top)
        "top-10 left-1/3 -translate-x-12",    // Seat 4 (Top Left)
        "top-10 right-1/3 translate-x-12",    // Seat 5 (Top Right)
        "right-4 top-1/2 -translate-y-20",    // Seat 6 (Right Top)
        "right-4 top-1/2 translate-y-20",     // Seat 7 (Right Bottom)
    ];

    const seatCardPositions = [
        "top", "top",       // 0, 1 (Bottom) -> Cards Top
        "right", "right",   // 2, 3 (Left) -> Cards Right
        "bottom", "bottom", // 4, 5 (Top) -> Cards Bottom
        "left", "left"      // 6, 7 (Right) -> Cards Left
    ];
    // This position list expects up to 10 players. 
    // Just mapping index to style.

    // helper to find player in state
    const getPlayerAtSeat = (idx) => gameState?.players?.find(p => p.seatIndex === idx);
    const currentPlayer = gameState?.players?.find(p => p.name === playerName);
    const isMyTurn = gameState?.currentPlayer === playerName;

    // Show controls if my turn (and not just waiting)
    const showControls = isMyTurn && gameState?.gameState === 'InProgress';

    // Can start?
    const playersWithChips = gameState?.players?.filter(p => p.seatIndex >= 0);
    const hasZeroChips = playersWithChips?.some(p => p.chipStack <= 0);
    const canStart = playersWithChips?.length >= 2 && gameState?.gameState !== 'InProgress' && !hasZeroChips;
    // Logic: "Only first player...". We can check if we are seat 0 or the "host".

    return (
        <div className="min-h-screen bg-poker-dark overflow-hidden relative">
            {/* Table Felt */}
            <div className="absolute inset-0 flex items-center justify-center">
                <div className="w-[80vw] h-[60vh] bg-poker-felt rounded-[200px] border-[20px] border-poker-dark shadow-2xl relative">
                    {/* Logo/Text in center */}
                    <div className="absolute inset-0 flex flex-col items-center justify-center opacity-30 pointer-events-none">
                        <h1 className="text-6xl font-black text-black tracking-widest">POKER</h1>
                    </div>

                    {/* Community Cards */}
                    <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 z-10 w-full max-w-lg">
                        <CommunityCards cards={gameState?.communityCards} />

                        {/* Pot Display */}
                        <div className="mt-4 text-center">
                            <span className="bg-black/40 text-poker-gold px-4 py-1 rounded-full text-lg font-mono border border-poker-gold/30">
                                POT: {gameState?.pot || 0}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Seats */}
            {Array.from({ length: 8 }).map((_, i) => {
                const isSeated = currentPlayer && currentPlayer.seatIndex >= 0;
                return (
                    <Seat
                        key={i}
                        seatIndex={i}
                        player={getPlayerAtSeat(i)}
                        isCurrentUser={getPlayerAtSeat(i)?.name === playerName}
                        isActiveTurn={gameState?.currentPlayer === getPlayerAtSeat(i)?.name}
                        positionClasses={seatPositions[i] || "hidden"}
                        cardPlacement={seatCardPositions[i] || "top"}
                        gameStatus={gameState?.gameState || gameState?.GameState}
                        onJoinSeat={!isSeated ? handleJoinSeat : undefined}
                    />
                );
            })}

            {/* Game Status / Phase */}
            <div className="absolute top-4 left-4 bg-black/50 p-2 rounded text-xs text-white z-50">
                <div>Phase: <span className="text-poker-gold">{gameState?.phase}</span></div>
                <div>State: {gameState?.gameState}</div>
            </div>

            {/* Identity Badge */}
            <div className="absolute top-4 left-1/2 -translate-x-1/2 bg-cyan-600/80 border border-cyan-400 text-white px-4 py-1 rounded-full text-xs font-bold shadow-lg z-50">
                YOU ARE: <span className="uppercase text-cyan-100 italic">{playerName}</span> 👤
            </div>

            {/* Messages Log */}
            <div className="absolute top-32 right-4 w-64 space-y-1 pointer-events-none">
                {messages.map((msg, i) => (
                    <div key={i} className="bg-black/60 text-white text-xs p-1 rounded animate-fade-in">
                        {msg}
                    </div>
                ))}
            </div>

            {/* Start Game Button (Overlay) */}
            {canStart && (
                <div className="absolute top-20 left-1/2 transform -translate-x-1/2 z-50">
                    <button
                        onClick={handleStartGame}
                        className="bg-poker-gold text-black font-bold px-8 py-3 rounded-full shadow-lg hover:bg-yellow-400 animate-pulse-slow border-2 border-white"
                    >
                        START GAME
                    </button>
                    {gameState.players.filter(p => p.seatIndex >= 0).length < 2 && <div className="text-white text-xs text-center mt-1 bg-black/50 px-2 rounded">Waiting for players...</div>}
                </div>
            )}

            {!canStart && hasZeroChips && gameState?.gameState !== 'InProgress' && (
                <div className="absolute top-20 left-1/2 transform -translate-x-1/2 z-50">
                    <div className="bg-red-600 text-white font-bold px-4 py-2 rounded text-sm shadow-lg border-2 border-red-800">
                        Cannot Start: Some players have 0 chips
                    </div>
                </div>
            )}

            {/* Player System Menu (Stand Up / Add Chips) - Only when NOT InProgress */}
            {currentPlayer && currentPlayer.seatIndex >= 0 && gameState?.gameState !== 'InProgress' && (
                <div className="absolute top-4 right-4 flex flex-col items-end space-y-2 z-50">
                    <div className="bg-black/80 p-2 rounded border border-gray-700 backdrop-blur-sm flex items-center space-x-2">
                        <input
                            type="number"
                            placeholder="Amount"
                            className="w-20 px-2 py-1 bg-gray-900 border border-gray-600 rounded text-white text-xs"
                            id="addChipsInput"
                        />
                        <button
                            onClick={() => {
                                const input = document.getElementById('addChipsInput');
                                if (input.value) handleAddChips(parseInt(input.value));
                            }}
                            className="bg-green-600 hover:bg-green-700 text-white text-xs px-2 py-1 rounded"
                        >
                            + Chips
                        </button>
                    </div>
                    <button
                        onClick={handleStandUp}
                        className="bg-red-600 hover:bg-red-700 text-white text-xs px-3 py-1 rounded shadow border border-red-800"
                    >
                        Stand Up
                    </button>
                </div>
            )}

            {/* Controls */}
            {showControls && (
                <GameControls
                    onAction={handleAction}
                    currentBet={gameState?.currentBet || 0}
                    playerChips={currentPlayer?.chipStack || 0}
                />
            )}

            {/* Turn Indicator Overlay */}
            {showControls && <div className="absolute inset-0 border-8 border-poker-gold pointer-events-none opacity-30 animate-pulse"></div>}
        </div>
    );
};

export default TablePage;
