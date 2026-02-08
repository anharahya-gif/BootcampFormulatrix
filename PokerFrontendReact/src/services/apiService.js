import axios from 'axios';

const API_BASE_URL = '/api/GameControllerAPI';

const apiService = {
    registerPlayer: async (name, chips) => {
        // Note: The backend uses query parameters for POST requests in some cases based on the controller code.
        // Ensure we match the backend expectation (FromQuery vs FromBody).
        // Based on GameControllerAPI.cs: [HttpPost("registerPlayer")] public ... ([FromQuery] string playerName, [FromQuery] int chipStack)
        return axios.post(`${API_BASE_URL}/registerPlayer?playerName=${name}&chipStack=${chips}`);
    },

    joinSeat: async (playerName, seatIndex) => {
        // [HttpPost("joinSeat")] ... ([FromQuery] string playerName, [FromQuery] int seatIndex)
        return axios.post(`${API_BASE_URL}/joinSeat?playerName=${playerName}&seatIndex=${seatIndex}`);
    },

    startRound: async () => {
        return axios.post(`${API_BASE_URL}/startRound`);
    },

    bet: async (playerName, amount) => {
        return axios.post(`${API_BASE_URL}/bet?name=${playerName}&amount=${amount}`);
    },

    call: async (playerName) => {
        return axios.post(`${API_BASE_URL}/call?name=${playerName}`);
    },

    check: async (playerName) => {
        return axios.post(`${API_BASE_URL}/check?name=${playerName}`);
    },

    fold: async (playerName) => {
        return axios.post(`${API_BASE_URL}/fold?name=${playerName}`);
    },

    raise: async (playerName, amount) => {
        return axios.post(`${API_BASE_URL}/raise?name=${playerName}&amount=${amount}`);
    },

    allIn: async (playerName) => {
        return axios.post(`${API_BASE_URL}/allin?name=${playerName}`);
    },

    getGameState: async () => {
        return axios.get(`${API_BASE_URL}/state`);
    },

    removePlayer: async (name) => {
        return axios.post(`${API_BASE_URL}/removePlayer?name=${name}`);
    },

    addChips: async (playerName, amount) => {
        return axios.post(`${API_BASE_URL}/addchips`, {
            PlayerName: playerName,
            Amount: amount
        });
    },

    nextPhase: async () => {
        return axios.post(`${API_BASE_URL}/nextPhase`);
    },

    showdown: async () => {
        return axios.post(`${API_BASE_URL}/showdown`);
    }
};

export default apiService;
