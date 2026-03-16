// ========================
// SESSION PLAYER NAME
// ========================
const playerName = sessionStorage.getItem("playerName");
if (!playerName) {
    alert("Player belum login! Silakan kembali ke start page.");
    window.location.href = "/index"; // redirect ke start page
}

// ========================
// SIGNALR SETUP
// ========================
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5175/pokerHub", { withCredentials: true })
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start()
    .then(() => {
        console.log("✅ SignalR connected");

        // Contoh: minta update game state setelah connected
        connection.invoke("SendGameState")
            .catch(err => console.error("❌ invoke SendGameState:", err));
    })
    .catch(err => console.error("❌ SignalR connection error:", err));

// ========================
// RECEIVE DATA
// ========================
connection.on("ReceiveGameState", (gameState) => {
    console.log("🃏 GameState updated:", gameState);
    updateUI(gameState);
});

connection.on("ShowdownStateUpdated", (showdownState) => {
    console.log("🏆 ShowdownState updated:", showdownState);
    showWinnerOverlay(showdownState);
});

// ========================
// JOIN / LEAVE SEAT
// ========================
async function joinSeat(seatIndex) {
    if (!playerName) { alert("Player belum login!"); return; }

    try {
        const url = `http://localhost:5175/api/GameControllerAPI/joinSeat?playerName=${encodeURIComponent(playerName)}&seatIndex=${seatIndex}`;
        const res = await fetch(url, { method: "POST" });
        if (!res.ok) { alert("Gagal join seat"); return; }
        const data = await res.json();
        if (data.success) {
            console.log(`${playerName} berhasil join seat ${seatIndex}`);
            window.location.reload();
        } else alert(data.message ?? "Gagal join seat");
    } catch (err) {
        console.error(err); alert("Error saat join seat");
    }
}

async function leaveSeat(playerName) {
    if (!playerName) return;
    if (!confirm(`Yakin ingin leave seat ${playerName}?`)) return;

    try {
        const url = `http://localhost:5175/api/GameControllerAPI/removePlayer?playerName=${encodeURIComponent(playerName)}`;
        const res = await fetch(url, { method: "POST" });
        const data = await res.json();
        if (data.success) window.location.reload();
        else alert(data.message ?? "Gagal leave seat");
    } catch (err) {
        console.error(err); alert("Error saat leave seat");
    }
}

// ========================
// ALL-IN
// ========================
async function allIn(playerName, btn) {
    try {
        btn.disabled = true;
        const originalText = btn.textContent;
        const res = await fetch(`http://localhost:5175/api/GameControllerAPI/allin?name=${encodeURIComponent(playerName)}`, { method: "POST" });
        if (res.ok) window.location.reload();
        else { btn.disabled = false; btn.textContent = originalText; alert("Gagal All-In!"); }
    } catch (err) {
        console.error(err);
        alert("Terjadi error!"); btn.disabled = false; btn.textContent = originalText;
    }
}

// ========================
// ADD CHIPS
// ========================
document.querySelectorAll(".btn-add-chips").forEach(btn => {
    btn.addEventListener("click", async () => {
        const playerName = btn.getAttribute("data-player-name");
        const input = btn.parentElement.querySelector(".add-chips-amount");
        const amount = parseInt(input?.value);
        if (isNaN(amount) || amount <= 0) { alert("Jumlah chip harus >0"); return; }

        try {
            const res = await fetch("http://localhost:5175/api/GameControllerAPI/addchips", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ playerName, amount })
            });
            const data = await res.json();
            if (!res.ok) { alert(data.message || "Gagal menambahkan chip"); return; }
            window.location.reload();
        } catch (err) { console.error(err); alert("Error saat add chips"); }
    });
});

// ========================
// WINNER OVERLAY
// ========================
function showWinnerOverlay(showdownState) {
    if (!showdownState) return;
    const overlay = document.createElement("div");
    overlay.className = "winner-overlay";
    overlay.innerHTML = `
        <div class="winner-box" onclick="event.stopPropagation()">
            <div class="winner-title">🏆 WINNER 🏆</div>
            <div class="winner-names">${showdownState.Winners.map(w => `<span>${w}</span>`).join('')}</div>
            <div class="winner-rank">Hand: <strong>${showdownState.HandRank}</strong></div>
            <div class="winner-message">${showdownState.Message}</div>
        </div>`;
    overlay.onclick = () => overlay.remove();
    document.body.appendChild(overlay);
    setTimeout(() => overlay.remove(), 3000);
}

// ========================
// CHECK PLAYER CHIPS -> Start Round
// ========================
function checkPlayersChips() {
    const startBtn = document.querySelector(".start-round-center button");
    if (!startBtn) return;

    let canStart = true;
    document.querySelectorAll(".player-container .chip-count").forEach(span => {
        if (parseInt(span.textContent) <= 0) canStart = false;
    });
    startBtn.disabled = !canStart;
    startBtn.title = canStart ? "" : "Tidak bisa mulai: ada player chip 0!";
}

// ========================
// INIT
// ========================
document.addEventListener("DOMContentLoaded", () => {
    checkPlayersChips();

    // Bind seat buttons otomatis
    document.querySelectorAll(".seat-btn").forEach((btn, index) => {
        btn.addEventListener("click", () => joinSeat(index));
    });
});

function joinSeat(seatIndex) {
    const playerName = window.currentPlayerName;
    if (!playerName) {
        alert("Player belum login!");
        return;
    }

    fetch(`http://localhost:5175/api/GameControllerAPI/joinSeat?playerName=${encodeURIComponent(playerName)}&seatIndex=${seatIndex}`, {
        method: "POST"
    })
    .then(resp => resp.json())
    .then(data => {
        console.log("Join seat response:", data);
        location.reload(); // reload halaman supaya state update
    })
    .catch(err => {
        console.error(err);
        alert("Gagal join seat");
    });
}
