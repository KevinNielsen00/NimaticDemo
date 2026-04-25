# NimaticDemo

Et demo-projekt bygget med **ASP.NET Core Web API** og en **Razor frontend**.
Projektet demonstrerer en simpel fullstack-opsætning med backend API og frontend UI.

---

## 🚀 Teknologier

* ASP.NET Core (Web API)
* Razor Pages / MVC (Frontend)
* C#
* .NET

---

## 📂 Projektstruktur

```
NimaticDemo/
│── NimaticDemo/        # Backend (Web API)
│── Frontend/           # Frontend (Razor)
│── NimaticDemo.slnx    # Solution fil
```

---

## ⚙️ Setup & Installation

### 1. Clone repository

```bash
git clone https://github.com/KevinNielsen00/NimaticDemo.git
cd NimaticDemo
```

---

### 2. Kør backend

```bash
cd NimaticDemo
dotnet run
```

Backend kører typisk på:

```
https://localhost:xxxx
```

---

### 3. Kør frontend

```bash
cd Frontend
dotnet run
```

---

## 🔗 API Endpoints

Eksempel (tilpas hvis nødvendigt):

```
GET    /api/...
POST   /api/...
PUT    /api/...
DELETE /api/...
```

---

## 🔐 Konfiguration

Følsomme data skal **ikke** ligge i Git.

Brug:

* `appsettings.json` → generel config
* `appsettings.Development.json` → lokal (ignoreret)
* User Secrets (anbefalet)

---

## 🧹 Git

Projektet bruger `.gitignore` til at udelukke:

* `bin/`, `obj/`
* `.vs/`
* `appsettings.Development.json`

---

## 📌 Formål

Projektet er lavet som:

* Demo / læringsprojekt
* Fullstack eksempel med ASP.NET
* Grundlag for videre udvikling

---

## 👤 Author

Kevin Nielsen
GitHub: https://github.com/KevinNielsen00

---

## Server Setup

Projektet bruger en selv-hostet PostgreSQL database på en Oracle Linux server.

### Server

- Hosting: Oracle Cloud
- OS: Oracle Linux
- Database: PostgreSQL 17
- Database UI: pgAdmin 4
- Backend: ASP.NET Core
- ORM: Entity Framework Core
- Database provider: Npgsql.EntityFrameworkCore.PostgreSQL

---

## Database

Databasen kører på PostgreSQL og tilgås af backend via EF Core.

### Connection string

Backend bruger connection string fra `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=<SERVER_IP>;Port=5432;Database=nimaticdb;Username=<DB_USER>;Password=<DB_PASSWORD>"
}

## MQTT Broker

Projektet bruger en lokal MQTT broker på serveren til at modtage data fra IoT-enheder.

### Broker

- Software: :contentReference[oaicite:0]{index=0}
- Kører på: Oracle Linux server
- Port: 1883 (standard MQTT)

---

## MQTT Configuration

Backend forbinder til MQTT brokeren via konfiguration:

```json
"Mqtt": {
  "Broker": "<SERVER_IP>",
  "Topic": "iot/data",
  "Username": "<MQTT_USER>",
  "Password": "<MQTT_PASSWORD>"
}
