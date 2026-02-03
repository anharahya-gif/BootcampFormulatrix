import { useState } from 'react'
import Lobby from './components/Lobby'
import GameTable from './components/GameTable'

function App() {
  const [playerName, setPlayerName] = useState(null);

  return (
    <div className="app">
      {!playerName ? (
        <Lobby onJoin={setPlayerName} />
      ) : (
        <GameTable playerName={playerName} />
      )}
    </div>
  )
}

export default App
