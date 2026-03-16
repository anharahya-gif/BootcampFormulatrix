# Meeting Room Booking System 🏢

An enterprise-ready Meeting Room Booking Full-Stack System built gracefully with proper separation of frontend and backend. 

The backend boasts **.NET 8** under **Clean Architecture** principles, while the UI is built dynamically using modern Javascript ecosystems.

## 🚀 Key Features & Highlights

- **Full-Stack Presentation**: Contains both an organized **React.js/HTML** frontend interface and a heavily structured backend API directly on the root workspace.
- **Backend Clean Architecture & N-Tier Separation**: C# logic is elegantly segregated into Domain, Application, Infrastructure, and Web API sub-layers.
- **ServiceResult Pattern**: Methods in the Application Service layer return standardized `ServiceResult<T>` instead of relying on `throw new Exception()` indiscriminately.
- **Global Exception Handling Middleware**: Integrated error-catching pipeline to uniformly respond to system unhandled exceptions with readable JSON structures (HTTP 500) without leaking sensitive stack traces.
- **Rich Tech Stack**: EF Core SQLite, ASP.NET Core Identity (Bearer JWT), AutoMapper, FluentValidation, and Swagger UI integration.
- **Unit Tested**: Core domain logic and application services (e.g., `HasOverlapAsync` booking collision logic) are strictly tested with **NUnit** and **Moq**.

## 📁 Repository Structure

```text
├── MeetingRoomBookingAPI/      # Backend: ASP.NET Core API (.NET 8)
├── MeetingRoomBookingClient/   # Frontend: React/Vite UI
├── MeetingRoomBooking.Tests/   # Quality: Unit Testing Module (NUnit & Moq)
├── Sandbox/                    # Playground: Various basic C# exercises & experiments
├── MeetingRoomBooking.sln      # Main Solution File (.sln) for Visual Studio / Rider
├── README.md                   # You are here
└── .gitignore                  
```

*(Note: Random experiments and basic C# drills from the bootcamp days have been explicitly moved to the `/Sandbox` folder to keep the root directory focused and professional).*

## 🛠️ Quick Setup

**1. Backend (API)**
1. Ensure **.NET 8 SDK** is installed on your machine.
2. Open terminal in the `MeetingRoomBookingAPI` folder and execute:
   ```bash
   dotnet restore
   dotnet run
   ```
3. Test the endpoints locally at: `http://localhost:5163/swagger` (Port may vary, verify in terminal output).

**2. Frontend (Client)**
1. Ensure **Node.js** is installed.
2. Open terminal in the `MeetingRoomBookingClient` folder and run:
   ```bash
   npm install
   npm run dev
   ```

**3. Unit Tests**
1. Run `dotnet test` from the root directory or inside `MeetingRoomBooking.Tests` to execute all API business validations.

---
*Built to showcase scalable backend craftsmanship and full-stack integration capability.*
