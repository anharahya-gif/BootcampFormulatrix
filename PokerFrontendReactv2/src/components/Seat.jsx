import React from 'react';
import Card from './Card';
import { clsx } from 'clsx';

const Seat = ({ player, seatIndex, isCurrentUser, positionClasses, onJoinSeat, onStandUp, isActiveTurn, isLastWinner, cardPlacement = "top", gameStatus = "WaitingForPlayers", rotation = 0 }) => {
    // 1. Handle Empty Seat
    if (!player) {
        return (
            <div className={clsx("absolute flex flex-col items-center justify-center w-24 h-24 rounded-full border-2 border-dashed border-gray-600 bg-black/10 text-gray-500 text-[10px] transition-all", positionClasses)}>
                {/* Chair Icon for Empty Seat - Centered and Enlarged */}
                <div className="absolute inset-0 flex items-center justify-center pointer-events-none z-[-1]">
                    <img
                        src="/icon/chair-png.png"
                        alt="Chair"
                        className="w-52 h-52 max-w-none object-contain opacity-40 transition-transform duration-500"
                        style={{ transform: `rotate(${rotation}deg)` }}
                    />
                </div>
                <span className="mb-1">Seat {seatIndex}</span>
                {onJoinSeat ? (
                    <button
                        onClick={() => onJoinSeat(seatIndex)}
                        className="bg-poker-gold text-black font-black px-4 py-1.5 rounded-full text-[11px] shadow-lg hover:bg-yellow-400 hover:scale-110 active:scale-95 transition-all outline-none ring-2 ring-black/20"
                    >
                        SIT
                    </button>
                ) : (
                    <span className="opacity-50">Locked</span>
                )}
            </div>
        );
    }

    // 2. Data Extraction (Prefer PascalCase from DTO)
    const name = player.Name || player.name || "Player";
    const chipStack = player.ChipStack !== undefined ? player.ChipStack : (player.chipStack !== undefined ? player.chipStack : 0);
    const currentBet = player.CurrentBet !== undefined ? player.CurrentBet : (player.currentBet !== undefined ? player.currentBet : 0);
    const isFolded = player.IsFolded !== undefined ? player.IsFolded : (player.isFolded !== undefined ? player.isFolded : false);
    const state = player.State || player.state || "Active";
    const hand = player.Hand || player.hand || [];
    const possibleRank = player.PossibleHandRank || player.possibleHandRank;

    // Card Positioning Style
    const getCardContainerStyle = () => {
        const dist = isCurrentUser ? "150px" : "110px";
        switch (cardPlacement) {
            case 'top': return { bottom: dist, left: '50%', transform: 'translateX(-50%)' };
            case 'bottom': return { top: dist, left: '50%', transform: 'translateX(-50%)' };
            case 'left': return { right: dist, top: '50%', transform: 'translateY(-50%)' };
            case 'right': return { left: dist, top: '50%', transform: 'translateY(-50%)' };
            default: return { bottom: dist, left: '50%', transform: 'translateX(-50%)' };
        }
    };

    const canShowRank = isCurrentUser || gameStatus === 'Showdown' || gameStatus === 'Completed';
    const hasRank = possibleRank && !isFolded && canShowRank;

    // Clean Rank Text (Removing "You Have")
    const displayRank = hasRank
        ? (possibleRank.replace(/([A-Z])/g, ' $1').trim().toUpperCase())
        : (isCurrentUser ? "YOU" : "");

    return (
        <div className={clsx("absolute flex flex-col items-center z-50", positionClasses)}>

            {/* 👑 WINNER CROWN (At very top) */}
            {isLastWinner && (
                <div className="absolute -top-12 text-3xl filter drop-shadow-[0_0_15px_rgba(250,204,21,0.9)] animate-bounce z-[80] pointer-events-none">
                    👑
                </div>
            )}

            {/* Avatar Container */}
            <div className="relative group">
                {/* Player Avatar Circle */}
                <div className={clsx(
                    "relative w-24 h-24 rounded-full border-4 flex flex-col items-center justify-center shadow-2xl transition-all duration-500 bg-gray-900",
                    isCurrentUser && !isFolded ? "border-cyan-400 ring-4 ring-cyan-400/20 shadow-[0_0_30px_rgba(34,211,238,0.3)]" :
                        isFolded ? "opacity-30 border-gray-600 grayscale" :
                            state === 'AllIn' ? "border-[#a855f7] shadow-[0_0_25px_rgba(168,85,247,0.7)]" :
                                isActiveTurn ? "border-yellow-400 scale-110 shadow-[0_0_35_px_rgba(250,204,21,0.6)] ring-4 ring-yellow-400/30" :
                                    "border-gray-700",
                    isLastWinner && "ring-[6px] ring-yellow-500/40 border-yellow-400 shadow-[0_0_50px_rgba(250,204,21,0.5)] bg-gradient-to-b from-yellow-900/40 to-black"
                )}>
                    {/* Chair Icon - Centered, Enlarged, and Rotated */}
                    <div className="absolute inset-0 flex items-center justify-center pointer-events-none z-[-1]">
                        <img
                            src="/icon/chair-png.png"
                            alt="Chair"
                            className={clsx(
                                "w-48 h-48 max-w-none object-contain transition-transform duration-500",
                                isFolded && "grayscale opacity-50"
                            )}
                            style={{ transform: `rotate(${rotation}deg)` }}
                        />
                    </div>

                    {/* Glossy Overlay */}
                    <div className="absolute inset-0 bg-gradient-to-tr from-white/5 to-transparent pointer-events-none rounded-full" />

                    {isActiveTurn && <div className="absolute inset-0 bg-yellow-400/10 animate-pulse rounded-full" />}

                    {/* 2.1 Player State Badge (Left Side) - Increased Offset */}
                    {(state === 'AllIn' || isFolded) && (
                        <div className="absolute -left-10 top-1/2 -translate-y-1/2 z-[70] animate-fade-in">
                            <span className={clsx(
                                "text-[8px] px-2 py-0.5 rounded-full border shadow-lg font-black tracking-widest block whitespace-nowrap",
                                state === 'AllIn' ? "bg-purple-600 text-white border-purple-400" : "bg-gray-700 text-gray-300 border-gray-500"
                            )}>
                                {state === 'AllIn' ? 'ALLIN' : 'FOLD'}
                            </span>
                        </div>
                    )}

                    {/* 2.2 Current Bet Badge (Right Side) - Increased Offset */}
                    {currentBet > 0 && (
                        <div className="absolute -right-10 top-1/2 -translate-y-1/2 z-[70] animate-fade-in">
                            <div className="bg-yellow-400 text-black px-2 py-0.5 rounded-full text-[9px] font-black border-2 border-yellow-600 shadow-xl flex items-center whitespace-nowrap">
                                <img src="/icon/chip.png" className="w-4 h-4 object-contain mr-1 drop-shadow-sm" />{currentBet}
                            </div>
                        </div>
                    )}

                    {/* 2.3 Centered Player Info - Adjusted for balance */}
                    <div className="flex flex-col items-center justify-center z-10 px-1 -translate-y-3">
                        <div className="font-black text-white text-[11px] truncate w-20 text-center leading-tight mb-0.5 shadow-black/80 drop-shadow-md">{name}</div>
                        <div className="text-poker-gold text-[10px] font-black font-mono leading-none flex items-center shadow-black/80 drop-shadow-md">
                            <img src="/icon/chip.png" className="w-3 h-3 object-contain mr-1 drop-shadow-sm" />{chipStack}
                        </div>
                    </div>
                </div>

                {/* PREMIUM RIBBON BADGE (At bottom - lowered to prevent chips obstruction) */}
                {(isCurrentUser || (isLastWinner && (gameStatus === 'Showdown' || gameStatus === 'Completed' || gameStatus === 'WaitingForStartRound'))) && (
                    <div className="absolute -bottom-10 left-1/2 -translate-x-1/2 z-[70] pointer-events-none w-[130px] flex flex-col items-center">
                        <div
                            className={clsx(
                                "relative px-3 py-1.5 text-white text-[10px] font-black italic tracking-[0.1em] transition-all duration-500 flex items-center justify-center min-w-[80px]",
                                isLastWinner
                                    ? "bg-gradient-to-r from-yellow-700 via-yellow-500 to-yellow-700 shadow-[0_8px_20px_rgba(250,204,21,0.5)] border-y-2 border-yellow-300/60"
                                    : hasRank
                                        ? "bg-gradient-to-r from-blue-800 via-cyan-500 to-blue-800 shadow-[0_8px_20px_rgba(34,211,238,0.4)] border-y-2 border-cyan-300/50"
                                        : (isCurrentUser ? "bg-gray-800 border-y-2 border-gray-500" : "hidden")
                            )}
                            style={{
                                clipPath: "polygon(15% 0%, 85% 0%, 100% 50%, 85% 100%, 15% 100%, 0% 50%)",
                                textShadow: "0 2px 4px rgba(0,0,0,0.8)"
                            }}
                        >
                            {/* Inner Shine */}
                            <div className="absolute inset-0 bg-gradient-to-b from-white/20 to-transparent pointer-events-none" />

                            <span className="relative z-10 whitespace-nowrap">
                                {isLastWinner && !hasRank ? "CHAMPION" : displayRank}
                            </span>
                        </div>

                        {/* Stand Up Button (Compact version under YOU badge - with extra clearance) */}
                        {isCurrentUser && onStandUp && (
                            <button
                                onClick={onStandUp}
                                className="pointer-events-auto mt-2 px-3 py-1 bg-red-600/10 hover:bg-red-600 text-red-500 hover:text-white border border-red-500/20 hover:border-red-400 rounded-full transition-all font-black text-[8px] uppercase tracking-wider shadow-lg backdrop-blur-sm"
                            >
                                STAND UP
                            </button>
                        )}
                    </div>
                )}
            </div>


            {/* Hole Cards - Forced onto Table Felt */}
            {(gameStatus === 'InProgress' || gameStatus === 'Showdown' || gameStatus === 'Completed') && (
                <div
                    className="absolute flex space-x-1 transition-all z-[40] pointer-events-none"
                    style={getCardContainerStyle()}
                >
                    {isCurrentUser ? (
                        Array.isArray(hand) && hand.length > 0 ? (
                            <div className="flex space-x-2">
                                {hand.map((card, idx) => (
                                    <div
                                        key={idx}
                                        className={clsx(
                                            "transform transition-all duration-500",
                                            idx === 1 ? "rotate-6" : "-rotate-6"
                                        )}
                                    >
                                        <Card cardString={card} className="w-16 h-24 text-[0.85rem] shadow-[0_15px_40px_rgba(0,0,0,0.8)] ring-4 ring-white/10 rounded-xl" />
                                    </div>
                                ))}
                            </div>
                        ) : null
                    ) : (
                        // 2.4 Action/Phase Badge (Removed ALL-IN from here)
                        !isFolded && (
                            <div className="flex -space-x-4 scale-90">
                                <Card faceDown className="w-12 h-16 -rotate-6 transform shadow-xl border-white/20" />
                                <Card faceDown className="w-12 h-16 rotate-6 transform shadow-xl border-white/20" />
                            </div>
                        )
                    )}
                </div>
            )}

            <style>{`
                @keyframes popIn {
                    0% { transform: scale(0.5); opacity: 0; }
                    100% { transform: scale(1); opacity: 1; }
                }
                @keyframes fadeIn {
                    from { opacity: 0; transform: translateY(10px); }
                    to { opacity: 1; transform: translateY(0); }
                }
                .animate-pop-in { animation: popIn 0.8s cubic-bezier(0.19, 1, 0.22, 1) forwards; }
                .animate-fade-in { animation: fadeIn 0.4s ease-out forwards; }
            `}</style>
        </div>
    );
};

export default Seat;
