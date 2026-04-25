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
