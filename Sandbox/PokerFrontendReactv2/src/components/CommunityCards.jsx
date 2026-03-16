import React from 'react';
import Card from './Card';

const CommunityCards = ({ cards }) => {
    // cards is an array of strings like ["Ace of Spades", "10 of Hearts"]

    const slots = [0, 1, 2, 3, 4];

    return (
        <div className="flex justify-center items-center space-x-3 p-4">
            {slots.map((index) => {
                const cardString = cards && cards[index] ? cards[index] : null;
                return (
                    <div
                        key={index}
                        className="w-14 h-20 border-2 border-white/10 rounded-lg flex items-center justify-center bg-black/20 shadow-inner overflow-hidden"
                    >
                        {cardString && (
                            <Card
                                cardString={cardString}
                                className="w-full h-full animate-community-deal"
                                style={{ animationDelay: `${index * 0.2}s` }}
                            />
                        )}
                    </div>
                );
            })}

            <style>{`
                @keyframes communityDeal {
                    0% { transform: translateY(-50px) rotateY(90deg); opacity: 0; }
                    100% { transform: translateY(0) rotateY(0); opacity: 1; }
                }
                .animate-community-deal {
                    animation: communityDeal 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) both;
                }
            `}</style>
        </div>
    );
};

export default CommunityCards;
