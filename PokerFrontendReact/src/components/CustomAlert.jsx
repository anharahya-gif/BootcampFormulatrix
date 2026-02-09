import React from 'react';
import { clsx } from 'clsx';

const CustomAlert = ({ isOpen, message, onClose, type = 'error' }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-[1000] flex items-center justify-center p-4 bg-black/85 backdrop-blur-sm animate-fade-in font-inter">
            <div className="w-full max-w-sm bg-gradient-to-br from-[#1a1a1a] via-[#111] to-black border-2 border-poker-gold/30 rounded-[32px] p-8 shadow-[0_0_60px_rgba(250,204,21,0.15)] transform animate-scale-up text-center relative overflow-hidden">
                {/* Decorative background shine */}
                <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-poker-gold to-transparent opacity-50"></div>

                {/* Icon Container */}
                <div className={clsx(
                    "w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6 border transition-all duration-500",
                    type === 'error'
                        ? "bg-red-500/10 border-red-500/30 text-red-500 shadow-[0_0_20px_rgba(239,68,68,0.2)]"
                        : "bg-poker-gold/10 border-poker-gold/30 text-poker-gold shadow-[0_0_20px_rgba(250,204,21,0.2)]"
                )}>
                    {type === 'error' ? (
                        <svg className="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                        </svg>
                    ) : (
                        <svg className="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                    )}
                </div>

                {/* Content */}
                <h3 className="text-xl font-black text-white mb-3 tracking-tight uppercase">
                    {type === 'error' ? 'Notice' : 'Information'}
                </h3>
                <p className="text-gray-400 mb-8 leading-relaxed font-medium">
                    {message}
                </p>

                {/* Button */}
                <button
                    onClick={onClose}
                    className="w-full py-4 bg-gradient-to-r from-poker-gold to-yellow-600 text-black font-black rounded-2xl hover:from-yellow-400 hover:to-yellow-500 transition-all duration-300 shadow-lg active:scale-95 uppercase tracking-widest text-sm"
                >
                    Understood
                </button>
            </div>

            <style>{`
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                @keyframes scaleUp {
                    from { transform: scale(0.9); opacity: 0; }
                    to { transform: scale(1); opacity: 1; }
                }
                .animate-fade-in {
                    animation: fadeIn 0.3s ease-out forwards;
                }
                .animate-scale-up {
                    animation: scaleUp 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
                }
                .font-inter {
                    font-family: 'Inter', sans-serif;
                }
            `}</style>
        </div>
    );
};

export default CustomAlert;
