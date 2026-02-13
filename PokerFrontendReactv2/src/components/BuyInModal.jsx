import React, { useState } from 'react';

const BuyInModal = ({ isOpen, onClose, onConfirm, balance, minBuyIn = 200, maxBuyIn = 2000 }) => {
    const [amount, setAmount] = useState(Math.max(minBuyIn, Math.min(maxBuyIn, balance)));

    if (!isOpen) return null;

    const actualMax = Math.min(maxBuyIn, balance);
    const isValid = amount >= minBuyIn && amount <= actualMax;

    return (
        <div className="fixed inset-0 z-[300] flex items-center justify-center bg-black/80 backdrop-blur-md animate-fade-in">
            <div className="w-full max-w-md bg-gradient-to-br from-gray-900 via-black to-gray-900 border-2 border-poker-gold/50 rounded-[40px] p-10 shadow-[0_0_50px_rgba(250,204,21,0.2)] transform animate-scale-up text-center">
                <div className="w-20 h-20 bg-poker-gold/10 rounded-full flex items-center justify-center mx-auto mb-6 border border-poker-gold/20">
                    <span className="text-4xl text-poker-gold">💰</span>
                </div>
                <h2 className="text-3xl font-black text-white mb-2 tracking-tighter uppercase italic">Table Buy-In</h2>
                <p className="text-gray-400 mb-8 font-medium">Choose your stack size</p>

                <div className="space-y-6">
                    <div className="bg-white/5 p-6 rounded-2xl border border-white/10">
                        <div className="flex justify-between text-[10px] text-gray-500 mb-2 uppercase font-black tracking-widest">
                            <span>Min: {minBuyIn}</span>
                            <span>Max: {actualMax}</span>
                        </div>
                        <input
                            type="number"
                            className="w-full bg-transparent text-white text-4xl font-black text-center outline-none border-b-2 border-poker-gold/30 focus:border-poker-gold transition-all pb-2 font-mono"
                            value={amount}
                            onChange={(e) => setAmount(parseInt(e.target.value) || 0)}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                        <button
                            onClick={() => setAmount(minBuyIn)}
                            className="py-3 bg-white/5 hover:bg-white/10 text-white rounded-xl border border-white/10 transition-all text-xs font-bold"
                        >
                            MIN ({minBuyIn})
                        </button>
                        <button
                            onClick={() => setAmount(actualMax)}
                            className="py-3 bg-poker-gold/10 hover:bg-poker-gold/20 text-poker-gold rounded-xl border border-poker-gold/20 transition-all text-xs font-black"
                        >
                            MAX ({actualMax})
                        </button>
                    </div>

                    <div className="flex flex-col gap-3 pt-4">
                        <button
                            onClick={() => onConfirm(amount)}
                            disabled={!isValid}
                            className="w-full py-4 bg-poker-gold text-black font-black rounded-2xl hover:bg-yellow-400 transition-all duration-300 disabled:opacity-30 disabled:grayscale shadow-lg shadow-poker-gold/10"
                        >
                            JOIN GAME
                        </button>
                        <button
                            onClick={onClose}
                            className="w-full py-4 bg-white/5 text-white font-black rounded-2xl hover:bg-white/10 transition-all duration-300 border border-white/10"
                        >
                            CANCEL
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BuyInModal;
