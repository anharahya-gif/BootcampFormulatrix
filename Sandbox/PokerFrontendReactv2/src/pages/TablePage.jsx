import React, { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import Seat from '../components/Seat';
import CommunityCards from '../components/CommunityCards';
import GameControls from '../components/GameControls';
import Loader from '../components/Loader';
import apiService from '../services/apiService';
import signalrService from '../services/signalrService';
import CustomAlert from '../components/CustomAlert';
import BuyInModal from '../components/BuyInModal';

const TablePage = () => {
    const navigate = useNavigate();
    const [gameState, setGameState] = useState(null);
    const [loading, setLoading] = useState(true);
    const [messages, setMessages] = useState([]);
    const [lastWinners, setLastWinners] = useState([]);

    const prevPhase = useRef(null);

    // UI State for Modals
    const [showStandUpModal, setShowStandUpModal] = useState(false);
    const [showResetModal, setShowResetModal] = useState(false);
    const [showBuyInModal, setShowBuyInModal] = useState(false);
    const [selectedSeatIndex, setSelectedSeatIndex] = useState(null);
    const [isProcessing, setIsProcessing] = useState(false);

    // Alert State
    const [alertConfig, setAlertConfig] = useState({ isOpen: false, message: '', type: 'error' });

    const showAlert = (message, type = 'error') => {
        setAlertConfig({ isOpen: true, message, type });
    };

    // Sound effects
    const cardAudio = useRef(new Audio('/bgm/card-bgm.mp3'));
    const chipAudio = useRef(new Audio('/bgm/chip-bgm.mp3'));
    const prevCommunityCount = useRef(0);
    const prevPot = useRef(0);

    // Session Data
    const playerName = sessionStorage.getItem('poker_player_name');
    const tableId = sessionStorage.getItem('poker_table_id');
    const tableName = sessionStorage.getItem('poker_table_name');
    const [userBalance, setUserBalance] = useState(0);

    useEffect(() => {
        cardAudio.current.volume = 1.0;
        chipAudio.current.volume = 1.0;
    }, []);

    const fetchBalance = async () => {
        try {
            const response = await apiService.getProfile();
            const data = response.data;
            setUserBalance(data.Balance ?? data.balance ?? 0);
        } catch (err) {
            console.error("Failed to fetch balance:", err);
        }
    };

    useEffect(() => {
        if (!playerName || !tableId) {
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
                // Fetch balance
                fetchBalance();

                // 1. Join Table via API
                await apiService.joinTable(tableId);

                // 2. Start SignalR
                await signalrService.startConnection();

                // 3. Join Table Group (SignalR)
                await signalrService.invoke('JoinTable', tableId);

                // 4. Subscribe to events
                signalrService.on('ReceiveGameState', (state) => {
                    console.log("Game State Received:", state);

                    const communityCards = state?.CommunityCards || state?.communityCards || [];
                    const newCount = communityCards.length;
                    const phase = state?.Phase || state?.phase;

                    prevPhase.current = phase;

                    if (newCount > prevCommunityCount.current) {
                        playCardSound();
                    }
                    prevCommunityCount.current = newCount;

                    const newPot = state?.Pot || state?.pot || 0;
                    if (newPot > prevPot.current) {
                        playChipSound();
                    }
                    prevPot.current = newPot;

                    setGameState(state);
                    setLoading(false);
                });

                signalrService.on('SeatsUpdated', (seats) => {
                    console.log("Seats Updated:", seats);
                    setGameState(prev => ({ ...prev, Seats: seats, seats: seats }));
                });

                signalrService.on('RoundStarted', (data) => {
                    console.log("Round Started:", data);
                    setGameState(prev => ({ ...prev, ...data }));
                    playCardSound();
                });

                signalrService.on('PhaseAdvanced', (data) => {
                    console.log("Phase Advanced:", data);
                    setGameState(prev => ({ ...prev, ...data }));
                    playCardSound();
                });

                signalrService.on('CommunityCardsUpdated', (d) => {
                    playCardSound();
                });

                signalrService.on('ShowdownCompleted', (details) => {
                    console.log("Showdown!", details);
                    prevCommunityCount.current = 0;

                    const data = details?.Data || details?.data || details;
                    const winnersArr = data.winners || data.Winners || [];
                    setLastWinners(winnersArr);

                    setTimeout(() => {
                        const rawPlayers = data.players || data.Players || [];
                        const totalPot = data.pot || data.Pot || 0;

                        navigate('/winner', {
                            state: {
                                winners: rawPlayers.filter(p => {
                                    const pName = p.name || p.Name;
                                    return winnersArr.includes(pName);
                                }) || [],
                                allPlayers: rawPlayers,
                                communityCards: data.communityCards || data.CommunityCards || [],
                                handRank: data.handRank || data.rank || data.Rank || "Winner",
                                pot: totalPot,
                                message: data.message || data.Message || ""
                            }
                        });
                    }, 4000);
                });

                signalrService.on('ReceiveMessage', (msg) => {
                    setMessages(prev => [...prev.slice(-4), msg]);
                });

                // Fetch initial Game State
                const response = await apiService.getGameState(tableId);
                setGameState(response.data);
                setLoading(false);

            } catch (err) {
                console.error("Init failed:", err);
                showAlert("Failed to connect to table: " + (err.response?.data?.message || err.message));
                setLoading(false);
            }
        };

        init();

        return () => {
            signalrService.off('ReceiveGameState');
            signalrService.off('SeatsUpdated');
            signalrService.off('RoundStarted');
            signalrService.off('PhaseAdvanced');
            signalrService.off('ReceiveMessage');
            signalrService.off('ShowdownCompleted');
            signalrService.off('CommunityCardsUpdated');
        };
    }, []);

    const handleAction = async (action, amount) => {
        const actionSounds = {
            bet: '/bgm/bet.mp3',
            call: '/bgm/call.mp3',
            check: '/bgm/check.mp3',
            fold: '/bgm/fold.mp3',
            raise: '/bgm/raise.mp3',
            allin: '/bgm/allin.mp3'
        };

        if (actionSounds[action]) {
            const audio = new Audio(actionSounds[action]);
            audio.play().catch(e => console.log("Action audio blocked:", e));
        }

        try {
            switch (action) {
                case 'bet':
                    const currentTableBet = gameState?.CurrentBet || gameState?.currentBet || 0;
                    if (currentTableBet > 0) {
                        await apiService.raise(tableId, amount);
                    } else {
                        await apiService.bet(tableId, amount);
                    }
                    break;
                case 'call': await apiService.call(tableId); break;
                case 'check': await apiService.check(tableId); break;
                case 'fold': await apiService.fold(tableId); break;
                case 'raise': await apiService.raise(tableId, amount); break;
                case 'allin': await apiService.allIn(tableId); break;
                default: break;
            }
        } catch (err) {
            console.error("Action failed:", err);
            showAlert("Action failed: " + (err.response?.data?.message || err.message));
        }
    };

    const executeStandUp = async () => {
        setIsProcessing(true);
        try {
            await apiService.standUp(tableId);
            setShowStandUpModal(false);
            showAlert("Stood up from table.", "success");

            // Refresh balance after standing up
            await fetchBalance();
        } catch (err) {
            console.error("Stand up failed:", err);
            setShowStandUpModal(false);
            showAlert("Stand up failed. Please try again.");
        } finally {
            setIsProcessing(false);
        }
    };

    const executeLeaveTable = async () => {
        if (isSeated) {
            showAlert("You must stand up before leaving the table.");
            return;
        }

        setIsProcessing(true);
        try {
            await apiService.leaveTable(tableId);
            navigate('/');
        } catch (err) {
            console.error("Leave failed:", err);
            showAlert("Failed to leave table.");
        } finally {
            setIsProcessing(false);
        }
    };

    const handleJoinSeat = async (seatIndex) => {
        setSelectedSeatIndex(seatIndex);
        // Refresh balance before showing modal
        await fetchBalance();
        setShowBuyInModal(true);
    };

    const handleBuyInConfirm = async (buyInAmount) => {
        setIsProcessing(true);
        try {
            // Sit player (combines choose seat and buy in backend)
            await apiService.chooseSeat(tableId, selectedSeatIndex, buyInAmount);

            setShowBuyInModal(false);
            showAlert("Joined seat and bought in!", "success");

            // Refresh balance after buy-in
            await fetchBalance();
        } catch (err) {
            console.error("Sit/Buy-in failed:", err);
            showAlert("Buy-in failed: " + (err.response?.data?.message || err.message));
        } finally {
            setIsProcessing(false);
        }
    };

    const handleStartGame = async () => {
        try {
            await apiService.startRound(tableId);
        } catch (err) {
            showAlert("Start failed: " + (err.response?.data?.message || err.message));
        }
    };

    if (loading) return <div className="min-h-screen bg-poker-dark flex items-center justify-center"><Loader text="Connecting to Table..." /></div>;

    const seatPositions = [
        "top-[5%] left-[34%] -translate-x-0",
        "top-[5%] right-[34%] translate-x-0",
        "right-[2%] top-[32%] -translate-y-0",
        "right-[2%] bottom-[36%] translate-y-0",
        "bottom-[18%] right-[34%] translate-x-0",
        "bottom-[18%] left-[34%] -translate-x-0",
        "left-[2%] bottom-[36%] translate-y-0",
        "left-[2%] top-[32%] -translate-y-0",
    ];

    const seatCardPositions = ["bottom", "bottom", "left", "left", "top", "top", "right", "right"];
    const seatRotations = [0, 0, 90, 90, 180, 180, -90, -90];

    const players = gameState?.Players || gameState?.players || [];
    const getPlayerAtSeat = (idx) => {
        // 1. Look in Seats list first to check occupancy
        const seatInfo = gameState?.Seats?.find(s => (s.seatIndex ?? s.SeatIndex) === idx) ||
            gameState?.seats?.find(s => (s.seatIndex ?? s.SeatIndex) === idx);

        if (!seatInfo || (!seatInfo.IsOccupied && !seatInfo.isOccupied)) return null;

        // 2. Look in Players list for actual data
        const pData = players.find(p => (p.seatIndex ?? p.SeatIndex) === idx);
        if (pData) return pData;

        // 3. Fallback to seatInfo data (for truncated SignalR/API responses)
        return {
            name: seatInfo.PlayerName || seatInfo.playerName || "Player",
            chipStack: seatInfo.Chips || seatInfo.chips || 0,
            seatIndex: idx,
            state: 'Active'
        };
    };

    // Robust player detection (case-insensitive)
    const normalizedPlayerName = String(playerName || '').trim().toLowerCase();
    const seatedPlayer = players.find(p => String(p.Name || p.name || '').trim().toLowerCase() === normalizedPlayerName) ||
        Array.from({ length: 12 }).map((_, i) => getPlayerAtSeat(i)).find(p => p && String(p.name || p.Name || '').trim().toLowerCase() === normalizedPlayerName);

    const isSeated = !!seatedPlayer;

    const apiCurrentPlayer = gameState?.CurrentPlayer || gameState?.currentPlayer;
    const isMyTurn = String(apiCurrentPlayer || '').trim().toLowerCase() === normalizedPlayerName;
    const currentTableState = gameState?.GameState || gameState?.gameState || gameState?.Phase || gameState?.phase;
    // Robust phase check (handles "WaitingForPlayer" string or index 0)
    const isWaiting = currentTableState === 'WaitingForPlayer' || currentTableState === 0;

    const showControls = isMyTurn && (currentTableState === 'InProgress' || !isWaiting);

    // Count players from either Players list or Seats list (whichever is more populated)
    const playersInSeats = players.length > 0 ? players.filter(p => (p.seatIndex ?? p.SeatIndex) >= 0) :
        (gameState?.Seats || gameState?.seats || []).filter(s => s.IsOccupied || s.isOccupied);

    const canStart = playersInSeats.length >= 2 && isWaiting;

    return (
        <div className="min-h-screen bg-poker-dark overflow-hidden relative">
            {/* Table Name Label */}
            <div className="absolute top-4 left-4 flex flex-col z-50">
                <div className="flex items-center gap-2">
                    <button onClick={executeLeaveTable} className="text-gray-500 hover:text-white transition-colors">←</button>
                    <span className="text-poker-gold font-black tracking-widest text-lg uppercase italic">{tableName}</span>
                </div>
                <span className="text-[10px] text-gray-500 uppercase font-bold tracking-tighter">Powered by PokerAPI v2</span>
            </div>

            {/* Buy-In Modal */}
            <BuyInModal
                isOpen={showBuyInModal}
                onClose={() => setShowBuyInModal(false)}
                onConfirm={handleBuyInConfirm}
                balance={userBalance || 0}
                minBuyIn={gameState?.MinBuyIn || gameState?.minBuyIn || 200}
                maxBuyIn={gameState?.MaxBuyIn || gameState?.maxBuyIn || 2000}
            />

            {/* Cash Out Modal */}
            {showStandUpModal && (
                <div className="fixed inset-0 z-[200] flex items-center justify-center bg-black/80 backdrop-blur-sm animate-fade-in">
                    <div className="w-full max-w-md bg-gradient-to-br from-poker-dark via-gray-900 to-black border-2 border-poker-gold/50 rounded-[40px] p-10 shadow-[0_0_50px_rgba(250,204,21,0.2)] transform animate-scale-up text-center">
                        <div className="w-20 h-20 bg-poker-gold/10 rounded-full flex items-center justify-center mx-auto mb-6 border border-poker-gold/20">
                            <span className="text-4xl text-poker-gold">👋</span>
                        </div>
                        <h2 className="text-3xl font-black text-white mb-4 tracking-tighter italic">STAND UP?</h2>
                        <p className="text-gray-400 mb-8 leading-relaxed font-medium">
                            Go to the rail? Your chips will be returned to your account.
                        </p>
                        <div className="flex flex-col gap-3">
                            <button onClick={executeStandUp} disabled={isProcessing} className="w-full py-4 bg-poker-gold text-black font-black rounded-2xl hover:bg-yellow-400 transition-all shadow-lg">
                                {isProcessing ? "PROCESSING..." : "CONFIRM STAND UP"}
                            </button>
                            <button onClick={() => setShowStandUpModal(false)} disabled={isProcessing} className="w-full py-4 bg-white/5 text-white font-black rounded-2xl border border-white/10 transition-all">
                                CANCEL
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Table Felt */}
            <div className="absolute inset-0 flex items-center justify-center p-10 pointer-events-none -translate-y-6">
                <div className="w-[55vw] h-[32vh] bg-poker-felt rounded-[140px] border-[16px] border-[#3e2723] shadow-[0_40px_80px_rgba(0,0,0,0.9),_inset_0_0_60px_rgba(0,0,0,0.6)] relative pointer-events-auto overflow-hidden">
                    <div className="absolute inset-0 rounded-[124px] border-[6px] border-[#4e342e] pointer-events-none z-20 shadow-[inset_0_0_30px_rgba(0,0,0,0.9)]"></div>
                    <div className="absolute inset-0 flex flex-col items-center justify-center opacity-30 pointer-events-none">
                        <h1 className="text-6xl font-black text-black tracking-widest">{tableName}</h1>
                    </div>
                    <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 z-10 w-full max-w-lg flex items-center justify-center">
                        <CommunityCards cards={gameState?.CommunityCards || gameState?.communityCards} />
                        <div className="absolute -bottom-10 left-1/2 -translate-x-1/2 mt-4 text-center">
                            <span className="bg-black/60 text-poker-gold px-6 py-2 rounded-full text-xl font-black font-mono border-2 border-poker-gold/40 shadow-2xl backdrop-blur-sm">
                                POT: {gameState?.Pot || gameState?.pot || 0}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Seats */}
            {Array.from({ length: gameState?.Seats?.length || gameState?.seats?.length || 6 }).map((_, i) => {
                const playerAtSeat = getPlayerAtSeat(i);
                // isSeated is already calculated above
                return (
                    <Seat
                        key={i}
                        seatIndex={i}
                        player={playerAtSeat}
                        isCurrentUser={playerAtSeat && String(playerAtSeat.name || playerAtSeat.Name || '').trim().toLowerCase() === normalizedPlayerName}
                        isActiveTurn={playerAtSeat && String(playerAtSeat.name || playerAtSeat.Name || '').trim().toLowerCase() === String(apiCurrentPlayer || '').trim().toLowerCase()}
                        isLastWinner={playerAtSeat && lastWinners.some(w => String(w || '').trim().toLowerCase() === String(playerAtSeat.name || playerAtSeat.Name || '').trim().toLowerCase())}
                        positionClasses={seatPositions[i]}
                        cardPlacement={seatCardPositions[i]}
                        rotation={seatRotations[i]}
                        gameStatus={currentTableState}
                        onJoinSeat={!isSeated ? handleJoinSeat : undefined}
                        onStandUp={isSeated ? () => setShowStandUpModal(true) : undefined}
                    />
                );
            })}

            {/* Game Info Overlay */}
            <div className="absolute top-4 right-4 text-right z-50">
                <div className="bg-black/60 backdrop-blur-md border border-white/10 p-4 rounded-2xl">
                    <p className="text-gray-500 text-[10px] uppercase font-bold tracking-widest mb-1">Authenticated As</p>
                    <p className="text-white font-black text-lg leading-none">{playerName}</p>
                    <p className="text-poker-gold text-xs mt-2">Account Balance: ${userBalance || "?"}</p>
                </div>
            </div>

            {/* Start Game Button */}
            {canStart && (
                <div className="absolute top-20 left-1/2 transform -translate-x-1/2 z-50">
                    <button onClick={handleStartGame} className="bg-poker-gold text-black font-black px-10 py-4 rounded-full shadow-2xl hover:scale-105 active:scale-95 transition-all animate-pulse-slow">
                        START ROUND
                    </button>
                </div>
            )}

            {/* Controls */}
            {showControls && (
                <GameControls
                    onAction={handleAction}
                    currentBet={gameState?.CurrentBet || gameState?.currentBet || 0}
                    playerChips={seatedPlayer?.ChipStack || seatedPlayer?.chipStack || seatedPlayer?.chips || 0}
                />
            )}

            <CustomAlert
                isOpen={alertConfig.isOpen}
                message={alertConfig.message}
                type={alertConfig.type}
                onClose={() => setAlertConfig({ ...alertConfig, isOpen: false })}
            />
        </div>
    );
};

export default TablePage;
