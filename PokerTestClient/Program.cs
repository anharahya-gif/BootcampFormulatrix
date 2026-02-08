using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var baseUrl = "http://localhost:5292"; // sesuaikan server
        var http = new HttpClient();

        // =========================
        // 1️⃣ Login
        // =========================
        Console.Write("Username: ");
        var username = Console.ReadLine() ?? "";
        Console.Write("Password: ");
        var password = Console.ReadLine() ?? "";

        var loginResponse = await http.PostAsJsonAsync($"{baseUrl}/api/auth/login",
            new { Username = username, Password = password });

        if (!loginResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Login failed!");
            return;
        }

        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        var jwtToken = loginData?.Token;

        if (jwtToken == null)
        {
            Console.WriteLine("No token received.");
            return;
        }

        Console.WriteLine("Login successful!");

        // =========================
        // 2️⃣ Get Tables (Lobby)
        // =========================
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var tables = await http.GetFromJsonAsync<List<TableInfo>>($"{baseUrl}/api/table");

        if (tables == null || tables.Count == 0)
        {
            Console.WriteLine("No tables available.");
            return;
        }

        Console.WriteLine("\nAvailable tables:");
        for (int i = 0; i < tables.Count; i++)
        {
            Console.WriteLine($"{i}: {tables[i].Name} (TableId: {tables[i].TableId})");
        }

        Console.Write("Select table index to join: ");
        int tableIndex = int.Parse(Console.ReadLine() ?? "0");
        var tableId = tables[tableIndex].TableId;

        // =========================
        // 3️⃣ Connect to PokerHub
        // =========================
        var connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/pokerhub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
            })
            .Build();

        // Event handler untuk update state
        connection.On<object>("InitialState", state =>
        {
            Console.WriteLine("\n--- Initial Table State ---");
            Console.WriteLine(state);
        });

        connection.On<object>("SeatsUpdated", seats =>
        {
            Console.WriteLine("\n--- Seats Updated ---");
            Console.WriteLine(seats);
        });

        await connection.StartAsync();
        Console.WriteLine("\nConnected to PokerHub!");

        // =========================
        // 4️⃣ Join Table (Spectator)
        // =========================
        try
        {
            await connection.InvokeAsync("JoinTable", tableId);
            Console.WriteLine($"Joined table {tableId} as spectator.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Join table failed: {ex.Message}");
            return;
        }

        // =========================
        // 5️⃣ Sit Down
        // =========================
        Console.Write("\nEnter seat index to sit: ");
        int seatIndex = int.Parse(Console.ReadLine() ?? "0");
        Console.Write("Enter chip amount to buy-in: ");
        int chips = int.Parse(Console.ReadLine() ?? "0");

        try
        {
            var sitResult = await connection.InvokeAsync<ServiceResult>(
                "SitDown", tableId, seatIndex, chips);

            if (!sitResult.IsSuccess)
                Console.WriteLine($"Sit down failed: {sitResult.Message}");
            else
                Console.WriteLine("Successfully sat down!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sit down failed: {ex.Message}");
        }

        // =========================
        // 6️⃣ Stand Up
        // =========================
        Console.WriteLine("\nPress Enter to stand up...");
        Console.ReadLine();

        try
        {
            var standResult = await connection.InvokeAsync<ServiceResult>(
                "StandUp", tableId);

            if (standResult.IsSuccess)
                Console.WriteLine("Successfully stood up!");
            else
                Console.WriteLine($"Stand up failed: {standResult.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stand up failed: {ex.Message}");
        }

        // =========================
        // 7️⃣ Leave Table (Back to Lobby)
        // =========================
        Console.WriteLine("\nPress Enter to leave table...");
        Console.ReadLine();

        try
        {
            var leaveResult = await connection.InvokeAsync<ServiceResult>(
                "LeaveTable", tableId);

            if (leaveResult.IsSuccess)
                Console.WriteLine("Left table, back to lobby!");
            else
                Console.WriteLine($"Leave table failed: {leaveResult.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Leave table failed: {ex.Message}");
        }

        await connection.StopAsync();
        Console.WriteLine("\nDisconnected.");
    }

    // =========================
    // Helper Classes
    // =========================
    class LoginResult
    {
        public string? Token { get; set; }
    }

    class TableInfo
    {
        public Guid TableId { get; set; }
        public string Name { get; set; } = "";
    }

    class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
