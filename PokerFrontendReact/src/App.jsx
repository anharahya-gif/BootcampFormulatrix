import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import StartPage from './pages/StartPage';
import TablePage from './pages/TablePage';
import WinnerPage from './pages/WinnerPage';

function App() {
    return (
        <Router>
            <div className="min-h-screen bg-poker-dark text-white font-sans antialiased selection:bg-poker-gold selection:text-poker-dark">
                <Routes>
                    <Route path="/" element={<StartPage />} />
                    <Route path="/table" element={<TablePage />} />
                    <Route path="/winner" element={<WinnerPage />} />
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </div>
        </Router>
    );
}

export default App;
