# Stylo.Backend

Backend service for Stylo built with **ASP.NET Core (.NET 10)** and structured according to **Clean Architecture** principles.

---

## 🏗 Project Structure

The solution is organized into four core layers:

```text
Stylo.Backend/
│
├── Stylo.API/               # Presentation Layer (Controllers, Startup, Configurations)
│   ├── Controllers/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Stylo.Backend.http
│
├── Stylo.Application/       # Application Layer (Business rules, Use Cases, Interfaces)
│
├── Stylo.Domain/            # Domain Layer (Core Entities, Value Objects)
│
└── Stylo.Infrastructure/    # Infrastructure Layer (Data access, External Services)
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```
