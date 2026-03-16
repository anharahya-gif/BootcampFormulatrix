import React, { useState } from 'react';
import { clsx } from 'clsx';

const GameControls = ({ onAction, currentBet, playerChips }) => {
    const [raiseAmount, setRaiseAmount] = useState(currentBet > 0 ? currentBet * 2 : 100);

    // Actions: Bet, Call, Raise, Check, Fold, All-in
    // Simplification: logic to show/hide buttons based on game state should be passed down or handled here?
    // For now, render all valid options. The parent usually controls usually what's available or we can just try and let server reject.

    return (
        <div className="fixed bottom-0 left-0 right-0 bg-black/80 p-4 flex flex-col items-center justify-center space-y-2 backdrop-blur-sm border-t border-poker-gold/30">
            <div className="flex space-x-4">
                <button
                    onClick={() => onAction('fold')}
                    className="px-6 py-2 bg-red-600 hover:bg-red-700 text-white font-bold rounded shadow border border-red-800"
                >
                    FOLD
                </button>

                <button
                    onClick={() => {
                        console.log("Action: check", { currentBet });
                        onAction('check');
                    }}
                    disabled={currentBet > 0}
                    className={clsx(
                        "px-6 py-2 font-bold rounded shadow border transition-all duration-200",
                        currentBet > 0
                            ? "bg-gray-800 text-gray-500 border-gray-900 cursor-not-allowed opacity-50"
                            : "bg-gray-600 hover:bg-gray-700 text-white border-gray-800"
                    )}
                >
                    CHECK
                </button>

                <button
                    onClick={() => onAction('call')}
                    className="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded shadow border border-blue-800"
                >
                    CALL
                </button>

                <button
                    onClick={() => onAction(currentBet > 0 ? 'raise' : 'bet', raiseAmount)}
                    className="px-6 py-2 bg-poker-gold hover:bg-yellow-400 text-black font-bold rounded shadow border border-yellow-600"
                >
                    {currentBet > 0 ? 'RAISE' : 'BET'}
                </button>
                <button
                    onClick={() => onAction('allin')}
                    className="px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white font-bold rounded shadow border border-purple-800"
                >
                    ALL-IN
                </button>
            </div>

            <div className="flex items-center space-x-2">
                <span className="text-white text-sm">Amount:</span>
                <input
                    type="number"
                    value={raiseAmount}
                    onChange={(e) => setRaiseAmount(parseInt(e.target.value))}
                    className="w-24 px-2 py-1 rounded bg-gray-800 text-white border border-gray-600"
                />
            </div>
        </div>
    );
};

export default GameControls;
