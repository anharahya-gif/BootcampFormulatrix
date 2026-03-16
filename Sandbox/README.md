# Sandbox & Bootcamp Exercises 🛠️

Welcome to the Sandbox! This directory contains various projects, technical exercises, and mini-applications built during the **Bootcamp Formulatrix**. 

This folder serves as a history of my learning progression—from mastering basic C# concepts to building functional, multiplayer web environments. The original solution file (`BootcampFormulatrix.sln`) rests here to safely group these legacy exercises together.

> **Note:** For the primary, enterprise-grade application demonstrating my current architectural proficiency, please refer to the `MeetingRoomBookingAPI` and `MeetingRoomBookingClient` in the root directory.

---

## 📂 Project Categories

### 1. Fundamental C# & .NET Core Learning
These projects laid the groundwork for my understanding of Object-Oriented Programming (OOP) and early structure concepts.
*   `AdvanceC#` & `ExploreBasic` : Drills exploring language capabilities, generics, delegates, and LINQ.
*   `ConsoleApp1`, `Test1`, `Exercise1`: Initial console-based application assignments.
*   `CobaWebApp` & `CobaWebAPI`: My earliest steps into ASP.NET Core MVC and creating basic RESTful endpoints.
*   `WebAPITest`: Foundational experiments with API testing mechanisms.

### 2. Boardgame Engines (OOP Architecture)
Developing these games involved designing robust domain models and simulating complex state management through raw OOP mechanisms.
*   `MiniBoardgame`: Simplified architecture for generic turn-based rules.
*   `Uno`, `UnoGame`, `UnoBoardGame`: Full implementations of the Uno card game rules engine handling deck shuffling, reverse mechanics, and varying card effects.

### 3. Multiplayer Poker Application Ecosystem 🃏
The most extensive project in the sandbox! This is a complete, real-time multiplayer implementation of Poker. Over time, it underwent several architectural evolutions and UI refactors, tracing my progression from simple APIs to full database-backed SignalR solutions.

**Backend Services:**
*   `PokerAPI` & `PokerMultiplayerAPI`: The raw game engine handling logic (dealing algorithms, hand evaluation, betting limits).
*   `PokerAPIMPwDB`, `PokerAPIMPwDBv2`, `PokerAPIMultiplayerWithDB`: The upgraded API stack integrating Database Persistence (managing user chips and session persistence) alongside real-time SignalR hubs.
*   `PokerAPI.Tests`: NUnit tests enforcing the reliability of the poker hand evaluation logic.

**Frontend Clients:**
*   `PokerFrontend`: Initial, raw frontend experiments.
*   `PokerUIClient`, `PokerBetterUI`, `PokerBetterUI_Redesign`: Continuous iterations improving the User Interface using standard vanilla HTML/CSS frameworks.
*   `PokerFrontendRazor`: Porting the frontend utilizing ASP.NET Core Razor Pages for server-side rendering.
*   `PokerFrontendReact` & `PokerFrontendReactv2`: Upgrading the visual layer by leveraging React.js to provide a highly interactive, state-driven real-time gambling experience.

**Testing Clients:**
*   `PokerTestClient`: Headless/command-line clients built specifically to spam and stress-test the real-time SignalR server endpoints.

---

### 🚀 Running the Legacy Poker App
If you wish to demo the final version of the poker app, detailed instructions are available in the local [`QUICK_START.md`](./QUICK_START.md) file included in this folder.
