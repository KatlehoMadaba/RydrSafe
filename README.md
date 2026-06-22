# 🛡️ RydrSafe

A community-driven e-hailing safety platform that helps passengers verify drivers before entering a vehicle.

Users upload screenshots from ride-hailing apps (Uber, Bolt, inDrive) and the platform extracts driver and vehicle information using OCR, matches it against a database of reported drivers, and warns users if a driver has been flagged.

---

## 📁 Project Structure

```
RydrSafe/
├── frontend/          # React + Vite + TypeScript
├── backend/           # ASP.NET Core 9 Web API
└── docs/              # Project specification
```

---

## 🧰 Tech Stack

### 🖥️ Frontend
- React 19, TypeScript, Vite
- Tailwind CSS, shadcn/ui
- TanStack Query, React Hook Form, Zod
- Sonner (notifications), Lucide React

### ⚙️ Backend
- ASP.NET Core 9 Web API (Clean Architecture)
- Entity Framework Core 9 + Npgsql
- MediatR (CQRS), FluentValidation
- JWT Authentication + Refresh Tokens
- SignalR (real-time notifications)
- Tesseract OCR

### 🗄️ Database
- PostgreSQL (Supabase)

---

## 🚀 Getting Started

### Prerequisites
- Node.js 18+
- .NET 9 SDK
- A Supabase account (free tier works)

---

### 🖥️ Frontend Setup

```bash
cd frontend
npm install
cp .env.example .env
# Set VITE_API_URL in .env to your backend URL
npm run dev
```

Runs on `http://localhost:5173`

---

### ⚙️ Backend Setup

#### 1. Configure appsettings.json

Create `backend/RydrSafe.API/appsettings.json` (not committed — contains secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_SUPABASE_POOLER_HOST;Database=postgres;Username=postgres.YOUR_PROJECT_REF;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Secret": "YOUR_RANDOM_64_CHAR_SECRET",
    "Issuer": "RydrSafe",
    "Audience": "RydrSafeUsers"
  },
  "Tesseract": {
    "DataPath": "./tessdata"
  },
  "AllowedOrigins": "http://localhost:5173"
}
```

🔑 Generate a JWT secret with PowerShell:
```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

#### 2. Add Tesseract language data

Download `eng.traineddata` from the [Tesseract GitHub](https://github.com/tesseract-ocr/tessdata) and place it in:
```
backend/RydrSafe.API/tessdata/eng.traineddata
```

#### 3. Run the backend

```bash
cd backend/RydrSafe.API
dotnet run
```

✅ On first startup, EF Core automatically creates all database tables in Supabase.

The API and Swagger UI will open at `https://localhost:7138`

---

## 📡 API Endpoints

### 🔐 Authentication
| Method | Endpoint | Access |
|--------|----------|--------|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/refresh-token` | Authenticated |

### 🔍 Verification
| Method | Endpoint | Access |
|--------|----------|--------|
| POST | `/api/verification/upload` | Passenger |
| GET | `/api/verification/history` | Passenger |

### 🚗 Drivers
| Method | Endpoint | Access |
|--------|----------|--------|
| GET | `/api/drivers` | Authenticated |
| GET | `/api/drivers/{id}` | Authenticated |
| GET | `/api/drivers/search?q=` | Authenticated |

### 📋 Reports
| Method | Endpoint | Access |
|--------|----------|--------|
| POST | `/api/reports` | Passenger |
| GET | `/api/reports` | Moderator, Admin |
| GET | `/api/reports/{id}` | Authenticated |
| PUT | `/api/reports/{id}/approve` | Moderator, Admin |
| PUT | `/api/reports/{id}/reject` | Moderator, Admin |

### 🔔 Notifications
| Method | Endpoint | Access |
|--------|----------|--------|
| GET | `/api/notifications` | Authenticated |
| PUT | `/api/notifications/{id}/read` | Authenticated |

### ⚡ Real-time
| Protocol | Endpoint | Description |
|----------|----------|-------------|
| SignalR | `/hubs/notifications` | Live alerts for moderators |

---

## 👥 User Roles

| Role | Capabilities |
|------|-------------|
| 🧑‍💼 **Passenger** | Register, login, upload screenshots, submit reports, view results |
| 🛡️ **Moderator** | Review reports, approve/reject, receive real-time alerts |
| 👑 **Admin** | Manage users, manage moderators, view analytics |

---

## 🔎 How Driver Verification Works

1. 📸 Passenger uploads 1–3 screenshots from a ride-hailing app
2. 🤖 Tesseract OCR extracts text from each image
3. 🔍 Regex patterns parse driver name, registration number, phone number, vehicle make/model
4. 🗄️ System searches the database in order: registration number → phone number → driver name (fuzzy, Levenshtein distance ≤ 2)
5. 📊 If a match is found, a risk score (0–100) is calculated and returned
6. 🚨 If the driver is Flagged or High Risk, moderators are notified via SignalR in real time

### 📊 Risk Score Levels
| Score | Status |
|-------|--------|
| 0–29 | ✅ Safe |
| 30–59 | 🟡 Under Review |
| 60–79 | 🟠 Flagged |
| 80–100 | 🔴 High Risk |

---

## 🏗️ Backend Architecture

```
RydrSafe.Domain          ← Entities, Enums (no dependencies)
RydrSafe.Application     ← CQRS, Interfaces, DTOs, Validators
RydrSafe.Infrastructure  ← EF Core, Repositories, JWT, OCR, SignalR
RydrSafe.API             ← Controllers, Middleware, Program.cs
```

---

## 🔮 Future Roadmap

**Phase 2**
- 📱 Mobile app
- 🆘 Panic button
- 📞 Emergency contacts
- 🤖 AI risk predictions

**Phase 3**
- 🔗 Ride-hailing integrations
- 🗺️ Community safety maps
- 📈 Predictive safety analytics
