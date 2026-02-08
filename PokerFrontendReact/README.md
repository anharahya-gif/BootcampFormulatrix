# Poker API Frontend (React)

This is the React frontend for the PokerAPI, built with Vite and Tailwind CSS.

## Prerequisites

- [Node.js](https://nodejs.org/) (Latest LTS recommended)
- The backend `PokerAPI` running on `http://localhost:5175`

## Setup

1.  Navigate to the project directory:
    ```bash
    cd BootcampFormulatrix/BootcampFormulatrix/PokerFrontendReact
    ```

2.  Install dependencies:
    ```bash
    npm install
    ```

3.  Run the development server:
    ```bash
    npm run dev
    ```

4.  Open your browser and navigate to the URL shown (usually `http://localhost:3000` or `http://localhost:5173`).

## Project Structure

-   `src/components`: Reusable UI components (Card, Seat, etc.)
-   `src/pages`: Application pages (StartPage, TablePage, WinnerPage)
-   `src/services`: API and SignalR services
-   `vite.config.js`: Configuration for Vite proxying to backend

## Features

-   **Real-time Gameplay**: Connects to the PokerAPI via SignalR.
-   **Responsive Design**: Built with Tailwind CSS.
-   **Game Flow**: seamless transition from Lobby -> Table -> Showdown.
