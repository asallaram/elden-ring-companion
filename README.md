# Elden Ring Companion

Full-stack web app for tracking your Elden Ring playthroughs - bosses defeated, weapons collected, fight attempts, and more.

## Features

### Character Management

- Create and manage multiple characters
- Track progress across different playthroughs
- Persistent character selection
- Starting level customization

### Boss Tracking

- Browse all Elden Ring bosses by region
- Mark bosses as defeated with automatic detection on victory
- Remove bosses from defeated list
- Search and filter your defeated bosses
- Boss detail pages with stats and weaknesses

### Weapon Collection

- Full weapon catalog with categories
- Track collected weapons per character
- Add/remove weapons from your arsenal
- Search and filter your collection
- Weapon detail pages with stats and requirements
- Dashboard recommendations (random 3 weapons)

### Fight Tracker

- Timer for boss attempts
- Record victory/defeat with notes
- Track weapon used per attempt
- Fight session management
- Automatic boss defeat marking on victory

### Additional Features

- Tips and strategies page
- Victory modal on boss defeat
- Auth system (register/login)
- Responsive design

## Tech Stack

### Frontend

- **Framework:** SvelteKit
- **Language:** TypeScript
- **Styling:** TailwindCSS
- **Deployment:** Vercel
- **Features:** SSR, form actions, reactive stores

### Backend

- **Framework:** ASP.NET Core 9
- **Language:** C#
- **Database:** PostgreSQL (Neon.tech)
- **Caching:** Redis (Upstash)
- **ORM:** Entity Framework Core
- **Auth:** JWT tokens with ASP.NET Identity
- **Deployment:** Render (Docker)

### Architecture

- RESTful API with controllers
- Repository pattern for data access
- Service layer for business logic
- Redis caching for boss/weapon lookups
- CSV data seeding on startup
- CORS configured for cross-origin requests

## Live App

- **Frontend:** https://elden-ring-companion-alpha.vercel.app
- **Backend API:** https://elden-ring-companion.onrender.com

**Note:** Backend is hosted on Render's free tier which spins down after 15 minutes of inactivity. First request after idle takes 30-60 seconds to wake up, then it's fast for 15 minutes.

## Local Development Setup

### Prerequisites

- Node.js 18+
- .NET 9 SDK
- PostgreSQL
- Redis

### Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

Frontend runs on `http://localhost:5173`

### Backend Setup

```bash
cd EldenRingSim
dotnet restore
dotnet run
```

Backend runs on `http://localhost:5019`

### Database Configuration

Update `appsettings.json` with your local database connections:
- PostgreSQL connection string
- Redis connection string

CSV data files are automatically seeded on first run from `elden-ring-data/` folder.

## Project Structure

```
├── frontend/                 # SvelteKit frontend
│   ├── src/
│   │   ├── routes/          # Pages and API routes
│   │   ├── lib/
│   │   │   ├── api/         # API client functions
│   │   │   ├── components/  # Reusable UI components
│   │   │   ├── stores/      # Svelte stores for state
│   │   │   └── types/       # TypeScript types
│
├── EldenRingSim/            # ASP.NET backend
│   ├── Controllers/         # API endpoints
│   ├── Models/             # Data models
│   ├── Repositories/       # Database access layer
│   ├── Services/           # Business logic
│   ├── DB/                 # EF Core context
│   ├── CSVParsing/         # Data seeding
│   └── elden-ring-data/    # CSV data files
```

## API Endpoints

### Auth

- `POST /api/auth/register` - Create account
- `POST /api/auth/login` - Login
- `GET /api/auth/profile` - Get user profile

### Player Progress

- `GET /api/playerprogress/my-characters` - Get all characters
- `POST /api/playerprogress` - Create character
- `GET /api/playerprogress/{id}/detailed` - Get character details
- `POST /api/playerprogress/{id}/defeat-boss` - Mark boss defeated
- `POST /api/playerprogress/{id}/obtain-weapon` - Add weapon
- `DELETE /api/playerprogress/{id}/bosses/{bossId}` - Remove boss
- `DELETE /api/playerprogress/{id}/weapons/{weaponId}` - Remove weapon

### Bosses

- `GET /api/bosses` - Get all bosses
- `GET /api/bosses/{id}` - Get boss by ID
- `GET /api/bosses/{id}/stats` - Get boss stats
- `GET /api/bosses/region/{region}` - Filter by region
- `GET /api/bosses/{id}/weapon-recommendations` - Recommended weapons

### Weapons

- `GET /api/weapons` - Get all weapons
- `GET /api/weapons/{id}` - Get weapon by ID
- `GET /api/weapons/{id}/matchups` - Boss matchups
- `GET /api/weapons/category/{category}` - Filter by category

### Boss Fights

- `POST /api/bossfight/start` - Start fight session
- `POST /api/bossfight/attempt` - Record attempt
- `POST /api/bossfight/{id}/end` - End session
- `GET /api/bossfight/active` - Get active session

## Future Plans

### WIP

- **Activity Feed:** Real-time feed showing recent boss defeats, attempts, and weapon collections across all your characters
- **Build Creator:** Full build planning tool with stat allocation, equipment selection, and optimization
- **Weapon Comparison:** Side-by-side weapon comparison with damage calculations
- **Boss Card Stats:** Show "last fought", "best time", "attempts" on boss cards
- **Advanced Analytics:** Charts and graphs for playtime, death rates, boss difficulty
- **AI Recommendations:** Weapon and build suggestions based on playstyle and progress
- **Social Features:** Share builds, compare progress with friends
- **Mobile App:** React Native version

## Notes

Made this mostly for myself to track my own runs but figured I'd deploy it in case anyone else wants to use it. Still lots of work to do, but I've been using it in my recent twin blades run.
