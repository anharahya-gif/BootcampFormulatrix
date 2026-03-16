import * as signalR from "@microsoft/signalr";

class SignalRService {
    constructor() {
        this.connection = null;
        this.events = {};
    }

    async startConnection() {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/pokerHub") // Proxy in vite.config.js handles the redirection
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Register handlers that were added before connection started
        Object.keys(this.events).forEach(eventName => {
            this.connection.on(eventName, this.events[eventName]);
        });

        try {
            await this.connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            setTimeout(() => this.startConnection(), 5000);
        }
    }

    on(eventName, callback) {
        if (this.connection) {
            this.connection.on(eventName, callback);
        }
        // Store callback to re-register if connection restarts or hasn't started yet
        this.events[eventName] = callback;
    }

    off(eventName) {
        if (this.connection) {
            this.connection.off(eventName);
        }
        delete this.events[eventName];
    }
}

const signalrService = new SignalRService();
export default signalrService;
