import axios from 'axios';

const api = axios.create({
    baseURL: '/api',
});

// Axios Interceptor to inject JWT Token
api.interceptors.request.use((config) => {
    const token = sessionStorage.getItem('poker_token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, (error) => {
    return Promise.reject(error);
});

const apiService = {
    // [AUTH DOMAIN]
    register: async (username, password) => {
        // Backend expects: { username, password, balance }
        return api.post('/auth/register', {
            username: username,
            password: password,
            balance: 0
        });
    },
    login: async (username, password) => {
        // Backend expects: { username, password }
        return api.post('/auth/login', {
            username: username,
            password: password
        });
    },

    // [USER DOMAIN]
    deposit: async (amount) => {
        // Backend expects: { amount }, uses JWT for userId
        return api.post('/user/deposit', { amount: amount });
    },
    getProfile: async () => {
        return api.get('/user/profile');
    },
    getUserInfo: async (id) => {
        return api.get(`/user/${id}`);
    },

    // [LOBBY DOMAIN]
    getTables: async () => {
        return api.get('/table');
    },
    createTable: async (tableData) => {
        return api.post('/table', tableData);
    },

    // [POKER DOMAIN - GAME ACTIONS]
    // Identifies player via JWT, table via Query Param 'tableId' (Guid)
    joinTable: async (tableId) => {
        return api.post(`/poker/join?tableId=${tableId}`);
    },
    chooseSeat: async (tableId, seatIndex, chips) => {
        return api.post(`/poker/sit?tableId=${tableId}&seatIndex=${seatIndex}&chips=${chips}`);
    },
    standUp: async (tableId) => {
        return api.post(`/poker/stand?tableId=${tableId}`);
    },
    leaveTable: async (tableId) => {
        return api.post(`/poker/leave?tableId=${tableId}`);
    },
    getGameState: async (tableId) => {
        return api.get(`/poker/state?tableId=${tableId}`);
    },

    // Betting Actions
    check: async (tableId) => {
        return api.post(`/poker/check?tableId=${tableId}`);
    },
    call: async (tableId) => {
        return api.post(`/poker/call?tableId=${tableId}`);
    },
    bet: async (tableId, amount) => {
        return api.post(`/poker/bet?tableId=${tableId}&amount=${amount}`);
    },
    raise: async (tableId, amount) => {
        return api.post(`/poker/raise?tableId=${tableId}&raiseAmount=${amount}`);
    },
    fold: async (tableId) => {
        return api.post(`/poker/fold?tableId=${tableId}`);
    },
    allIn: async (tableId) => {
        return api.post(`/poker/allin?tableId=${tableId}`);
    },

    // Legacy/Utility (Cleanup if needed later)
    startRound: async () => {
        // Note: New backend might handle this automatically or via a specific action
        return api.post('/poker/start');
    }
};

export default apiService;
