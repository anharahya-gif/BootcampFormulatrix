import React from 'react';

const ShuffleDeck = () => {
    return (
        <div className="fixed inset-0 z-[300] flex items-center justify-center bg-black/60 backdrop-blur-sm">
            {/* Animated Deck Container */}
            <div className="relative flex items-center justify-center">
                {/* Left Stack */}
                <div className="relative animate-shuffle-left">
                    {[...Array(8)].map((_, i) => (
                        <div
                            key={`left-${i}`}
                            className="absolute w-20 h-28 rounded-xl shadow-2xl overflow-hidden"
                            style={{
                                transform: `translate(${-i * 2}px, ${-i * 2}px)`,
                                zIndex: 8 - i
                            }}
                        >
                            <img
                                src="/cards/back_dark.png"
                                alt="Card Back"
                                className="w-full h-full object-cover"
                            />
                        </div>
                    ))}
                </div>

                {/* Right Stack */}
                <div className="relative animate-shuffle-right ml-16">
                    {[...Array(8)].map((_, i) => (
                        <div
                            key={`right-${i}`}
                            className="absolute w-20 h-28 rounded-xl shadow-2xl overflow-hidden"
                            style={{
                                transform: `translate(${i * 2}px, ${-i * 2}px)`,
                                zIndex: 8 - i
                            }}
                        >
                            <img
                                src="/cards/back_dark.png"
                                alt="Card Back"
                                className="w-full h-full object-cover"
                            />
                        </div>
                    ))}
                </div>
            </div>

            {/* Shuffling Text */}
            <div className="absolute bottom-1/3 text-white text-2xl font-bold tracking-widest animate-pulse">
                SHUFFLING...
            </div>
        </div>
    );
};

export default ShuffleDeck;
