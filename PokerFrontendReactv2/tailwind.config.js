/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                poker: {
                    green: '#35654d',
                    dark: '#1a3c2f',
                    felt: '#2d5a45',
                    gold: '#ffd700',
                    red: '#e11d48'
                }
            },
            animation: {
                'deal': 'deal 0.5s ease-out forwards',
                'fade-in': 'fadeIn 0.3s ease-out forwards',
                'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
            },
            keyframes: {
                deal: {
                    '0%': { transform: 'translateY(-100px) scale(0.5)', opacity: '0' },
                    '100%': { transform: 'translateY(0) scale(1)', opacity: '1' },
                },
                fadeIn: {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                }
            }
        },
    },
    plugins: [],
}
