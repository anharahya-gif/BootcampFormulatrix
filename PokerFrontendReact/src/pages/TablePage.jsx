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

    // UI State for Modal
    const [showStandUpModal, setShowStandUpModal] = useState(false);
    const [showResetModal, setShowResetModal] = useState(false);
    const [isProcessing, setIsProcessing] = useState(false);

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

                    // Auto-redirect if server has been reset (no players left)
                    if (state && state.players && state.players.length === 0 && !loading) {
                        const amIInList = state.players.some(p => (p.name || p.Name) === playerName);
                        if (!amIInList && playerName) {
                            console.log("Server reset detected or player removed. Redirecting...");
                            navigate('/');
                        }
                    }
                });

                signalrService.on('CommunityCardsUpdated', (d) => {
                    console.log("Community Cards Updated:", d.communityCards);
                    // Just play sound if we get this explicit event too
                    playCardSound();
                });

                signalrService.on('ShowdownCompleted', (details) => {
                    console.log("Showdown!", details);
                    prevCommunityCount.current = 0; // Reset for next round

                    // Navigate after a delay to ensure players see the final community cards
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
                    }, 1);
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
                    const amIInGame = initialState.players?.some(p => (p.name || p.Name) === playerName);
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

    /**
     * handleAction: Dispatcher for all player actions (Bet, Call, Fold, etc.)
     * @param {string} action - The type of action to perform
     * @param {number} amount - Optional amount for bets/raises
     */
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
            // We could use a custom alert here too in the future
            alert("Action failed: " + (err.response?.data?.message || err.message));
        }
    };

    /**
     * executeStandUp: Triggers the actual API call to remove the player from the table
     */
    const executeStandUp = async () => {
        setIsProcessing(true);
        try {
            await apiService.removePlayer(playerName);

            // Optimistic update
            setGameState(prev => ({
                ...prev,
                players: prev.players.filter(p => (p.name || p.Name) !== playerName)
            }));

            // Navigate back to the start page
            navigate('/');
        } catch (err) {
            console.error("Stand up failed:", err);
            setShowStandUpModal(false);
            alert("Stand up failed. Please try again.");
        } finally {
            setIsProcessing(false);
        }
    };

    /**
     * handleStandUp: Opens the premium confirmation modal
     */
    const handleStandUp = () => {
        setShowStandUpModal(true);
    };

    /**
     * handleAddChips: Request additional chips from the backend
     * @param {number} amount - Amount to add
     */
    const handleAddChips = async (amount) => {
        try {
            await apiService.addChips(playerName, amount);
        } catch (err) {
            alert("Add chips failed: " + (err.response?.data?.message || err.message));
        }
    };

    /**
     * handleStartGame: Triggers the round start from the first player
     */
    const handleStartGame = async () => {
        try {
            await apiService.startRound();
        } catch (err) {
            alert("Start failed: " + (err.response?.data?.message || err.message));
        }
    };

    /**
     * executeResetServer: Completely wipes the backend state and kicks all players
     */
    const executeResetServer = async () => {
        setIsProcessing(true);
        try {
            await apiService.resetGame();
            setShowResetModal(false);
            // After reset, we navigate away since our player object is gone
            navigate('/');
        } catch (err) {
            console.error("Reset failed:", err);
            setShowResetModal(false);
            alert("Reset failed. Please check server logs.");
        } finally {
            setIsProcessing(false);
        }
    };

    /**
     * handleJoinSeat: Join a specific seat at the table
     * @param {number} seatIndex - Index of the seat (0-7)
     */
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

    // Floating seats layout - Pushed to Absolute margins and centered top/bottom
    const seatPositions = [
        "bottom-[12%] right-[25%] translate-x-0",  // Seat 0 (Bottom Right)
        "bottom-[12%] left-[25%] -translate-x-0",   // Seat 1 (Bottom Left)
        "left-[2%] top-1/2 translate-y-40",        // Seat 2 (Far Left Bottom)
        "left-[2%] top-1/2 -translate-y-40",       // Seat 3 (Far Left Top)
        "top-[12%] left-[25%] -translate-x-0",      // Seat 4 (Top Left)
        "top-[12%] right-[25%] translate-x-0",     // Seat 5 (Top Right)
        "right-[2%] top-1/2 -translate-y-40",      // Seat 6 (Far Right Top)
        "right-[2%] top-1/2 translate-y-40",       // Seat 7 (Far Right Bottom)
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
    const getPlayerAtSeat = (idx) => gameState?.players?.find(p => (p.seatIndex !== undefined ? p.seatIndex : p.SeatIndex) === idx);
    const currentPlayer = gameState?.players?.find(p => (p.name || p.Name) === playerName);
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
            {/* Modal Overlay for Stand Up */}
            {showStandUpModal && (
                <div className="fixed inset-0 z-[200] flex items-center justify-center bg-black/80 backdrop-blur-sm animate-fade-in">
                    <div className="w-full max-w-md bg-gradient-to-br from-poker-dark via-gray-900 to-black border-2 border-poker-gold/50 rounded-[40px] p-10 shadow-[0_0_50px_rgba(250,204,21,0.2)] transform animate-scale-up text-center">
                        <div className="w-20 h-20 bg-poker-gold/10 rounded-full flex items-center justify-center mx-auto mb-6 border border-poker-gold/20">
                            <span className="text-4xl text-poker-gold">👋</span>
                        </div>
                        <h2 className="text-3xl font-black text-white mb-4 tracking-tighter">LEAVE TABLE?</h2>
                        <p className="text-gray-400 mb-8 leading-relaxed font-medium">
                            Are you sure you want to stand up? Your current hand will be folded and you will leave the table.
                        </p>

                        <div className="flex flex-col gap-3">
                            <button
                                onClick={executeStandUp}
                                disabled={isProcessing}
                                className="w-full py-4 bg-poker-gold text-black font-black rounded-2xl hover:bg-yellow-400 transition-all duration-300 disabled:opacity-50 shadow-lg"
                            >
                                {isProcessing ? "PROCESSING..." : "YES, STAND UP"}
                            </button>
                            <button
                                onClick={() => setShowStandUpModal(false)}
                                disabled={isProcessing}
                                className="w-full py-4 bg-white/5 text-white font-black rounded-2xl hover:bg-white/10 transition-all duration-300 border border-white/10"
                            >
                                NO, STAY
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Modal Overlay for Server Reset */}
            {showResetModal && (
                <div className="fixed inset-0 z-[200] flex items-center justify-center bg-black/85 backdrop-blur-md animate-fade-in">
                    <div className="w-full max-w-md bg-gradient-to-br from-[#1a1a1a] via-[#111] to-black border-2 border-red-500/50 rounded-[40px] p-10 shadow-[0_0_60px_rgba(239,68,68,0.2)] transform animate-scale-up text-center">
                        <div className="w-24 h-24 bg-red-500/10 rounded-full flex items-center justify-center mx-auto mb-8 border border-red-500/20">
                            <span className="text-5xl">⚠️</span>
                        </div>
                        <h2 className="text-3xl font-black text-white mb-4 tracking-tighter">RESET SERVER?</h2>
                        <p className="text-gray-400 mb-10 leading-relaxed font-medium">
                            This will <span className="text-red-400 font-bold">WIPE ALL DATA</span>, remove all players, and reset the table to its initial state. This cannot be undone.
                        </p>

                        <div className="flex flex-col gap-3">
                            <button
                                onClick={executeResetServer}
                                disabled={isProcessing}
                                className="w-full py-4 bg-red-600 text-white font-black rounded-2xl hover:bg-red-500 transition-all duration-300 disabled:opacity-50 shadow-lg shadow-red-900/20"
                            >
                                {isProcessing ? "RESETTING..." : "YES, RESET EVERYTHING"}
                            </button>
                            <button
                                onClick={() => setShowResetModal(false)}
                                disabled={isProcessing}
                                className="w-full py-4 bg-white/5 text-white font-black rounded-2xl hover:bg-white/10 transition-all duration-300 border border-white/10"
                            >
                                CANCEL
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Table Felt (Centered and Separated) */}
            <div className="absolute inset-0 flex items-center justify-center p-20 pointer-events-none">
                <div className="w-[75vw] h-[48vh] bg-poker-felt rounded-[180px] border-[16px] border-poker-dark shadow-[0_0_100px_rgba(0,0,0,0.8)] relative pointer-events-auto">
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
                const playerAtSeat = getPlayerAtSeat(i);
                const playerNameAtSeat = playerAtSeat?.name || playerAtSeat?.Name;
                const isLastWinner = gameState?.showdown?.winners?.includes(playerNameAtSeat);

                return (
                    <Seat
                        key={i}
                        seatIndex={i}
                        player={playerAtSeat}
                        isCurrentUser={playerNameAtSeat === playerName}
                        isActiveTurn={gameState?.currentPlayer === playerNameAtSeat}
                        isLastWinner={isLastWinner}
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

            {/* Reset Server Button - Pojok Kanan Bawah */}
            <div className="absolute bottom-6 right-6 z-50">
                <button
                    onClick={() => setShowResetModal(true)}
                    className="flex items-center space-x-2 bg-black/40 hover:bg-red-900/30 text-white/40 hover:text-red-400 px-4 py-2 rounded-xl border border-white/5 hover:border-red-500/30 transition-all duration-500 backdrop-blur-md group"
                    title="Danger Zone: Hard Reset Server"
                >
                    <span className="text-sm font-black tracking-widest uppercase opacity-0 group-hover:opacity-100 transition-opacity duration-500">Reset Server</span>
                    <span className="text-xl">⚙️</span>
                </button>
            </div>

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

            <style>{`
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                @keyframes scaleUp {
                    from { transform: scale(0.85); opacity: 0; }
                    to { transform: scale(1); opacity: 1; }
                }
                .animate-fade-in {
                    animation: fadeIn 0.25s ease-out forwards;
                }
                .animate-scale-up {
                    animation: scaleUp 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
                }
            `}</style>
        </div>
    );
};

export default TablePage;
