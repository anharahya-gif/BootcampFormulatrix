import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Card from '../components/Card';
import { clsx } from 'clsx';

const Confetti = () => {
    const [particles, setParticles] = useState([]);

    useEffect(() => {
        const colors = ['#FFD700', '#FF4D4D', '#00FFCC', '#FF00FF', '#0099FF'];
        const newParticles = Array.from({ length: 60 }).map((_, i) => ({
            id: i,
            x: Math.random() * 100,
            y: -10,
            rotation: Math.random() * 360,
            color: colors[Math.floor(Math.random() * colors.length)],
            size: Math.random() * 10 + 5,
            delay: Math.random() * 3,
            duration: Math.random() * 2 + 2,
        }));
        setParticles(newParticles);
    }, []);

    return (
        <div className="fixed inset-0 pointer-events-none overflow-hidden z-[100]">
            {particles.map((p) => (
                <div
                    key={p.id}
                    className="absolute rounded-sm animate-fall"
                    style={{
                        left: `${p.x}%`,
                        backgroundColor: p.color,
                        width: `${p.size}px`,
                        height: `${p.size / 2}px`,
                        transform: `rotate(${p.rotation}deg)`,
                        animationDelay: `${p.delay}s`,
                        animationDuration: `${p.duration}s`,
                    }}
                />
            ))}
            <style>{`
                @keyframes fall {
                    0% { transform: translateY(-10vh) rotate(0deg); opacity: 1; }
                    100% { transform: translateY(110vh) rotate(720deg); opacity: 0; }
                }
                .animate-fall {
                    animation: fall linear infinite;
                }
            `}</style>
        </div>
    );
};

const WinnerPage = () => {
    const { state } = useLocation();
    const navigate = useNavigate();

    // Support both camelCase and PascalCase from state
    const winnersRaw = state?.Winners || state?.winners || [];
    const allPlayers = state?.Players || state?.allPlayers || [];
    const communityCards = state?.CommunityCards || state?.communityCards || [];
    const handRank = state?.HandRank || state?.handRank || state?.Rank || state?.rank || "Winning Hand";
    const pot = state?.Pot || state?.pot || 0;

    const winnerNames = winnersRaw.map(w => w.name || w.Name);

    /**
     * Lifecycle: Plays the victory BGM on mount and manages cleanup on unmount.
     */
    useEffect(() => {
        const audio = new Audio('/bgm/win-bgm.mp3');
        audio.volume = 1.0;
        audio.play().catch(e => console.log("Audio playback error:", e));

        return () => {
            audio.pause();
            audio.currentTime = 0;
        };
    }, []);

    return (
        <div className="min-h-screen flex flex-col items-center justify-center bg-poker-dark p-4 relative overflow-hidden font-inter">
            {/* Celebration Background Effects */}
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_center,_rgba(250,204,21,0.1)_0%,_transparent_70%)] pointer-events-none" />
            <div className="absolute inset-0 opacity-20 pointer-events-none overflow-hidden">
                <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[200%] h-[200%] bg-[conic-gradient(from_0deg,_transparent_0deg,_rgba(255,215,0,0.1)_15deg,_transparent_30deg)] animate-spin-slow" />
            </div>

            <Confetti />

            {/* Header Section */}
            <div className="relative z-10 text-center mb-12">
                <div className="inline-block animate-bounce-slow mb-4">
                    <span className="text-8xl">🏆</span>
                </div>
                <h1 className="text-7xl font-black text-transparent bg-clip-text bg-gradient-to-b from-yellow-200 via-poker-gold to-yellow-600 drop-shadow-2xl uppercase tracking-tighter">
                    {winnersRaw.length > 1 ? "Split Pot!" : "Winner!"}
                </h1>
                <div className="flex flex-col items-center mt-4 gap-2">
                    <div className="bg-black/40 backdrop-blur-md px-6 py-2 rounded-full border border-poker-gold/30 shadow-xl">
                        <span className="text-2xl text-white font-medium italic tracking-wide">{handRank}</span>
                    </div>
                    {pot > 0 && (
                        <div className="bg-poker-gold text-black px-8 py-2 rounded-full font-black text-xl shadow-2xl animate-pulse">
                            TOTAL POT: {pot}
                        </div>
                    )}
                </div>
            </div>

            {/* Community Cards Section */}
            <div className="relative z-10 mb-12 transform hover:scale-105 transition-transform duration-500">
                <div className="absolute inset-0 bg-poker-gold/10 blur-3xl rounded-full" />
                <div className="relative border-t border-b border-poker-gold/20 py-8 px-12 bg-black/30 backdrop-blur-sm rounded-3xl shadow-2xl">
                    <h3 className="text-poker-gold/50 text-center mb-4 text-xs font-black uppercase tracking-[0.3em]">Table Cards</h3>
                    <div className="flex space-x-3 justify-center">
                        {communityCards && communityCards.length > 0 ? (
                            communityCards.map((card, i) => (
                                <Card key={i} cardString={card} className="w-20 h-28 transform hover:-translate-y-4 hover:rotate-3 transition-all duration-300 ring-2 ring-white/10" />
                            ))
                        ) : (
                            <span className="text-gray-600 italic">No community cards shown</span>
                        )}
                    </div>
                </div>
            </div>

            {/* Players Grid */}
            <div className="relative z-10 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 w-full max-w-6xl mb-12 px-4">
                {allPlayers.map((player, idx) => {
                    const name = player.Name || player.name;
                    const hand = player.Hand || player.hand;
                    const isWinner = winnerNames.includes(name);
                    const chipStack = player.ChipStack || player.chipStack;
                    const winnings = player.Winnings || player.winnings;

                    return (
                        <div
                            key={idx}
                            className={clsx(
                                "group p-6 rounded-3xl flex flex-col items-center relative transition-all duration-500",
                                isWinner
                                    ? "bg-gradient-to-br from-yellow-600/40 via-yellow-900/60 to-black border-2 border-poker-gold shadow-[0_0_40px_rgba(250,204,21,0.3)] scale-110 z-20"
                                    : "bg-black/60 border border-white/5 opacity-80 backdrop-blur-sm hover:opacity-100 hover:scale-105 hover:bg-white/5"
                            )}
                        >
                            {isWinner && (
                                <div className="absolute -top-3 left-1/2 -translate-x-1/2 bg-gradient-to-r from-yellow-400 to-yellow-600 text-black text-[10px] font-black px-4 py-1 rounded-full shadow-lg ring-2 ring-white animate-pulse">
                                    🏆 CHAMPION
                                </div>
                            )}

                            <h3 className={clsx(
                                "text-2xl font-black mb-1 tracking-tight",
                                isWinner ? "text-white" : "text-gray-400"
                            )}>
                                {name}
                            </h3>

                            {isWinner && (
                                <div className="bg-green-500/20 px-3 py-1 rounded-full mb-4">
                                    <span className="text-green-400 font-black text-sm">+{winnings || 'Pot share'} Chips</span>
                                </div>
                            )}

                            <div className="text-white/40 text-[10px] mb-4 uppercase font-bold tracking-widest">
                                Total: {chipStack}
                            </div>

                            {/* Player's Hole Cards */}
                            <div className="flex space-x-2 mt-auto">
                                {Array.isArray(hand) && hand.length > 0 ? (
                                    hand.map((card, i) => (
                                        <div
                                            key={i}
                                            className={clsx(
                                                "transition-all duration-700",
                                                isWinner ? "ring-4 ring-poker-gold/50 shadow-2xl translate-y-[-5px]" : "grayscale-[50%]"
                                            )}
                                        >
                                            <Card cardString={card} className="w-16 h-24" />
                                        </div>
                                    ))
                                ) : (
                                    <div className="h-24 flex items-center justify-center border-2 border-dashed border-white/10 rounded-xl px-4 text-white/20 text-xs italic">
                                        No cards shown
                                    </div>
                                )}
                            </div>

                            {player.isFolded && (
                                <div className="absolute inset-0 bg-black/60 backdrop-grayscale rounded-3xl flex items-center justify-center">
                                    <span className="text-red-500 font-black text-2xl rotate-12 tracking-widest opacity-80">FOLDED</span>
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

            <button
                onClick={() => navigate('/table')}
                className="relative z-10 group px-12 py-4 bg-white text-poker-dark font-black rounded-2xl hover:bg-poker-gold hover:text-white transition-all duration-300 shadow-[0_10px_30px_rgba(0,0,0,0.5)] active:scale-95"
            >
                <div className="flex items-center space-x-3">
                    <span className="text-xl">Return to Table</span>
                    <span className="group-hover:translate-x-2 transition-transform duration-300">→</span>
                </div>
            </button>

            <style>{`
                @keyframes bounce-slow {
                    0%, 100% { transform: translateY(0); }
                    50% { transform: translateY(-20px); }
                }
                @keyframes spin-slow {
                    from { transform: translate(-50%, -50%) rotate(0deg); }
                    to { transform: translate(-50%, -50%) rotate(360deg); }
                }
                .animate-bounce-slow {
                    animation: bounce-slow 4s ease-in-out infinite;
                }
                .animate-spin-slow {
                    animation: spin-slow 20s linear infinite;
                }
                .font-inter {
                    font-family: 'Inter', sans-serif;
                }
            `}</style>
        </div>
    );
};

export default WinnerPage;
