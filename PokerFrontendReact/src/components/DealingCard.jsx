import React from 'react';

const DealingCard = ({ startX, startY, deltaX, deltaY, targetRot, delay = 0 }) => {
    return (
        <div
            className="fixed z-[200] pointer-events-none"
            style={{
                left: `${startX}px`,
                top: `${startY}px`,
                width: '48px',
                height: '64px',
                animation: `dealCard 0.5s ease-out ${delay}s forwards`,
                '--delta-x': `${deltaX}px`,
                '--delta-y': `${deltaY}px`,
                '--target-rot': `${targetRot}deg`,
            }}
        >
            <img
                src="/cards/back_dark.png"
                alt="Card"
                className="w-full h-full object-cover rounded shadow-lg"
            />

            <style>{`
                @keyframes dealCard {
                    0% { 
                        transform: translate(0, 0) rotate(0deg) scale(0.8); 
                        opacity: 1;
                    }
                    100% { 
                        transform: translate(var(--delta-x), var(--delta-y)) rotate(var(--target-rot)) scale(1); 
                        opacity: 0;
                    }
                }
            `}</style>
        </div>
    );
};

export default DealingCard;
