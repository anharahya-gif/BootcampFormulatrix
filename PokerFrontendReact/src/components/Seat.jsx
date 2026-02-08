import React from 'react';
import Card from './Card';
import { clsx } from 'clsx';

const Seat = ({ player, seatIndex, isCurrentUser, positionClasses, onJoinSeat, isActiveTurn, cardPlacement = "top", gameStatus = "WaitingForPlayers" }) => {
    if (!player) {
        return (
            <div className={clsx("absolute flex flex-col items-center justify-center w-24 h-24 rounded-full border-2 border-dashed border-gray-600 bg-black/40 text-gray-400 text-xs transition-all", positionClasses)}>
                <span className="text-[10px] mb-1">Seat {seatIndex}</span>
                {onJoinSeat ? (
                    <button
                        onClick={() => onJoinSeat(seatIndex)}
                        className="bg-poker-gold text-black font-bold px-3 py-1 rounded-full text-xs shadow-lg hover:bg-yellow-400 hover:scale-105 transition-all"
                    >
                        SIT
                    </button>
                ) : (
                    <span className="text-[10px]">Empty</span>
                )}
            </div>
        );
    }

    const { name, chipStack, currentBet, isFolded, state, hand } = player;

    const getCardPositionClass = () => {
        if (isCurrentUser) {
            switch (cardPlacement) {
                case 'top': return "-top-24 left-1/2 -translate-x-1/2";
                case 'bottom': return "-bottom-28 left-1/2 -translate-x-1/2";
                case 'left': return "-left-20 top-1/2 -translate-y-1/2";
                case 'right': return "-right-20 top-1/2 -translate-y-1/2";
                default: return "-top-24 left-1/2 -translate-x-1/2";
            }
        } else {
            switch (cardPlacement) {
                case 'top': return "-top-10 left-1/2 -translate-x-1/2";
                case 'bottom': return "-bottom-16 left-1/2 -translate-x-1/2";
                case 'left': return "-left-12 top-1/2 -translate-y-1/2";
                case 'right': return "-right-12 top-1/2 -translate-y-1/2";
                default: return "-top-10 left-1/2 -translate-x-1/2";
            }
        }
    };

    // Hand is list of strings if visible, or empty/null if hidden
    // If isCurrentUser, we might see cards. If showdown, everyone sees.

    return (
        <div className={clsx("absolute flex flex-col items-center", positionClasses)}>

            {/* Bet Amount Bubble */}
            {currentBet > 0 && (
                <div className="mb-2 bg-poker-gold text-black px-2 py-1 rounded-full text-xs font-bold shadow-lg border border-yellow-600 z-10 transition-all">
                    {currentBet}
                </div>
            )}

            {/* Player Avatar/Info */}
            <div className={clsx(
                "relative w-24 h-24 rounded-full border-4 flex flex-col items-center justify-center shadow-xl transition-all duration-300",
                isCurrentUser && !isFolded ? "border-cyan-400 bg-gray-900 shadow-[0_0_20px_rgba(34,211,238,0.5)] animate-pulse-gentle scale-105 ring-4 ring-cyan-400/20" :
                    isFolded ? "opacity-50 border-gray-500 bg-gray-800" :
                        state === 'AllIn' ? "border-purple-500 bg-purple-900 shadow-[0_0_15px_rgba(168,85,247,0.5)]" :
                            isActiveTurn ? "border-yellow-400 bg-gray-800 scale-110 shadow-[0_0_20px_rgba(250,204,21,0.6)] ring-4 ring-yellow-400/30" :
                                state === 'Active' ? "border-green-500 bg-gray-900" : "border-gray-600 bg-gray-800"
            )}>
                {isCurrentUser && (
                    <div className="absolute -top-3 bg-cyan-500 text-white text-[10px] font-black px-2 py-0.5 rounded-full shadow-lg border border-white/20 animate-bounce">
                        YOU
                    </div>
                )}
                <div className="font-bold text-white text-sm truncate w-20 text-center">{name}</div>
                <div className="text-poker-gold text-xs">🪙 {chipStack}</div>
                {state === 'AllIn' && <div className="text-purple-300 text-[10px] font-bold">ALL-IN</div>}
                {isFolded && <div className="text-gray-400 text-[10px]">FOLDED</div>}
            </div>

            {/* Cards */}
            {(gameStatus === 'InProgress' || gameStatus === 'Showdown') && (
                <div className={clsx("absolute flex space-x-1 transition-all cursor-default z-50 animate-pop-in", getCardPositionClass())}>
                    {isCurrentUser ? (
                        // Logic for Current User: Only show if hand exists and is valid array
                        Array.isArray(hand) && hand.length > 0 && (
                            hand.map((card, idx) => (
                                <div key={idx} className={clsx("transform transition-transform hover:-translate-y-4", idx === 1 ? "rotate-6 translate-x-2" : "-rotate-6 -translate-x-2")}>
                                    <Card cardString={card} className="w-16 h-24 text-[0.8rem] shadow-2xl ring-2 ring-white/20" />
                                </div>
                            ))
                        )
                    ) : (
                        // Logic for Others: Show face-down if not folded
                        !isFolded && (
                            <>
                                <Card faceDown className="w-10 h-14 -rotate-6 transform shadow-lg border-white/10" />
                                <Card faceDown className="w-10 h-14 rotate-6 transform -ml-4 shadow-lg border-white/10" />
                            </>
                        )
                    )}
                </div>
            )}

            <style>{`
                @keyframes popIn {
                    0% { transform: scale(0) translateY(20px); opacity: 0; }
                    100% { transform: scale(1) translateY(0); opacity: 1; }
                }
                @keyframes pulseGentle {
                    0%, 100% { opacity: 1; transform: scale(1.05); }
                    50% { opacity: 0.8; transform: scale(1.02); }
                }
                .animate-pop-in {
                    animation: popIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards;
                }
                .animate-pulse-gentle {
                    animation: pulseGentle 2s ease-in-out infinite;
                }
            `}</style>
        </div>
    );
};

export default Seat;
