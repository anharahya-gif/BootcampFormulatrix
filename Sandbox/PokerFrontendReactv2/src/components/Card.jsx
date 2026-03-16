import React from 'react';
import { clsx } from 'clsx';

const Card = ({ cardString, faceDown = false, className }) => {
    // cardString expected format: "Rank of Suit" e.g., "Ace of Spades", "10 of Hearts"

    const getCardImage = () => {
        if (faceDown) return "/cards/back_dark.png";
        if (!cardString) return null;

        const parts = cardString.split(' of ');
        if (parts.length !== 2) return null;

        const rank = parts[0].trim();
        const suit = parts[1].trim().toLowerCase(); // "Spades" -> "spades"

        // Robust Rank Mapping
        let rankCode = rank;
        const rankMap = {
            'Ace': 'A',
            'King': 'K',
            'Queen': 'Q',
            'Jack': 'J',
            '10': '10',
            'Ten': '10',
            '9': '9',
            'Nine': '9',
            '8': '8',
            'Eight': '8',
            '7': '7',
            'Seven': '7',
            '6': '6',
            'Six': '6',
            '5': '5',
            'Five': '5',
            '4': '4',
            'Four': '4',
            '3': '3',
            'Three': '3',
            '2': '2',
            'Two': '2'
        };

        if (rankMap[rank]) {
            rankCode = rankMap[rank];
        }

        return `/cards/${suit}_${rankCode}.png`;
    };

    const imgSrc = getCardImage();

    if (!imgSrc) return null;

    return (
        <div className={clsx("relative rounded-lg shadow-xl overflow-hidden select-none bg-white", className)}>
            <img
                src={imgSrc}
                alt={faceDown ? "Card Back" : cardString}
                className="w-full h-full object-contain"
            />
        </div>
    );
};

export default Card;
