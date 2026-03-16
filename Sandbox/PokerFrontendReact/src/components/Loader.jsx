import React from 'react';

const Loader = ({ text = "Loading..." }) => {
    return (
        <div className="flex flex-col items-center justify-center p-4">
            <div className="w-10 h-10 border-4 border-poker-gold border-t-transparent rounded-full animate-spin mb-2"></div>
            {text && <p className="text-poker-gold text-sm font-semibold">{text}</p>}
        </div>
    );
};

export default Loader;
