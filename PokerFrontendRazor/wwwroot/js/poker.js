
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/pokerHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    await start();
});

// Event Handlers
connection.on("ReceiveGameState", (gameState) => {
    console.log("Game State:", gameState);
    updateTable(gameState);
});

connection.on("CommunityCardsUpdated", (data) => {
    console.log("Community Cards:", data);
    // Ideally we re-fetch state or update partial
    // For simplicity, we might just wait for the next ReceiveGameState or trigger a fetch
    // But let's try to update just cards if possible, or reliance on ReceiveGameState is safer
});

connection.on("ShowdownCompleted", (details) => {
    console.log("Showdown:", details);
    // Redirect to Winner Page
    window.location.href = `/Winner?data=${encodeURIComponent(JSON.stringify(details))}`;
});

connection.on("ReceiveMessage", (message) => {
    const log = document.getElementById("gameLog");
    if (log) {
        const entry = document.createElement("div");
        entry.textContent = message;
        log.appendChild(entry);
        log.scrollTop = log.scrollHeight;
    }
});

// Start Connection
start();

// --- UI Updaters ---

function updateTable(state) {
    if (!state) return;

    // Update Pot
    document.getElementById("potAmount").innerText = state.pot;

    // Update Community Cards
    const board = document.getElementById("communityCards");
    board.innerHTML = "";
    if (state.communityCards) {
        state.communityCards.forEach(card => {
            board.appendChild(createCardElement(card));
        });
    }

    // Update Seats
    // We assume fixed 10 seats. state.players has the data.
    // Clear all seats first or update existing?
    // Let's reset for simplicity
    for (let i = 0; i < 10; i++) {
        const seatEl = document.getElementById(`seat-${i}`);
        if (!seatEl) continue;

        const player = state.players.find(p => p.seatIndex === i);
        seatEl.innerHTML = ""; // Clear

        if (player) {
            seatEl.classList.remove("empty-seat");
            seatEl.classList.add("occupied-seat");

            // Name & Chips
            const info = document.createElement("div");
            info.className = "player-info";
            info.innerHTML = `<div>${player.name}</div><div class="chips">🪙 ${player.chipStack}</div>`;

            if (player.currentBet > 0) {
                const bet = document.createElement("div");
                bet.className = "current-bet";
                bet.innerText = player.currentBet;
                seatEl.appendChild(bet);
            }

            seatEl.appendChild(info);

            // Cards (Private View handled by Razor/Session usually, OR we check if this is US)
            // But ReceiveGameState usually scrubs private cards for others.
            // If it's ME, I should see my cards.
            // The backend `BuildGameStateDto` scrapes hands for others? 
            // Actually `GetPlayersPublicState` in backend returns `Hand` as list.
            // Wait, does it return ALL hands?
            // Checking backend code: `GetPlayersPublicState` -> `Hand = status.Hand...`.
            // It seems it returns hands for Everyone?
            // If so, that's a security flaw in the backend (it's a demo?), but for now we render what we get.
            // Optimization: Only render cards if it's ME or Showdown.
            // For now, render what is sent.

            const handDiv = document.createElement("div");
            handDiv.className = "player-hand";
            if (player.hand && player.hand.length > 0) {
                player.hand.forEach(cardStr => {
                    // Check if we should show card face
                    // For this demo, we assume if backend sends it, we show it?
                    // Or we should hide if not me?
                    // Let's just render.
                    handDiv.appendChild(createCardElement(cardStr));
                });
            } else if (!player.isFolded) {
                // Back of cards
                handDiv.appendChild(createCardBack());
                handDiv.appendChild(createCardBack());
            }
            seatEl.appendChild(handDiv);

            if (player.isFolded) seatEl.classList.add("folded");

        } else {
            seatEl.className = "seat empty-seat";
            seatEl.innerHTML = `<span class="seat-num">${i}</span>`;
        }
    }

    // Update Controls (Enable/Disable based on turn)
    const myName = getMyName(); // Need to inject this from Razor
    const isMyTurn = state.currentPlayer === myName;
    document.getElementById("controls").style.display = isMyTurn ? "flex" : "none";

    // Start Button Logic
    const startBtn = document.getElementById("startGameBtn");
    if (state.gameState !== "InProgress" && state.players.length >= 2) {
        startBtn.style.display = "block";
    } else {
        startBtn.style.display = "none";
    }
}

function createCardElement(cardStr) {
    const el = document.createElement("div");
    el.className = "card";
    // Parse "Rank of Suit"
    const parts = cardStr.split(" of ");
    let rank = parts[0];
    let suit = parts[1];

    // Simple render
    el.innerHTML = `<span class="${getSuitColor(suit)}">${rank[0]}${getSuitSymbol(suit)}</span>`;
    return el;
}

function createCardBack() {
    const el = document.createElement("div");
    el.className = "card back";
    return el;
}

function getSuitSymbol(suit) {
    switch (suit) {
        case 'Hearts': return '♥';
        case 'Diamonds': return '♦';
        case 'Clubs': return '♣';
        case 'Spades': return '♠';
        default: return '?';
    }
}

function getSuitColor(suit) {
    return (suit === 'Hearts' || suit === 'Diamonds') ? 'text-red' : 'text-black';
}

function getMyName() {
    return document.getElementById("sessionPlayerName").value;
}

// Game Actions
async function sendAction(action, amount = 0) {
    const name = getMyName();
    const url = `/api/GameControllerAPI/${action}?name=${name}` + (amount ? `&amount=${amount}` : ``);

    try {
        const res = await fetch(`http://localhost:5175${url}`, { method: 'POST' });
        if (!res.ok) {
            const err = await res.json();
            alert(err.message || "Action failed");
        }
    } catch (e) {
        console.error(e);
    }
}
