# **Elden Ring Companion**

**Full-stack web application for tracking your Elden Ring playthroughs** — bosses defeated, weapons collected, fight attempts, and more.

---

## **Features**

### **Character Management**
- Create and manage multiple characters  
- Track progress across different playthroughs  
- Persistent character selection  
- Starting level customization  

### **Boss Tracking**
- Browse all Elden Ring bosses by region  
- Mark bosses as defeated with automatic detection on victory  
- Remove bosses from defeated list  
- Search and filter your defeated bosses  
- Boss detail pages with stats and weaknesses  

### **Weapon Collection**
- Full weapon catalog with categories  
- Track collected weapons per character  
- Add/remove weapons from your arsenal  
- Search and filter your collection  
- Weapon detail pages with stats and requirements  
- Dashboard recommendations (random 3 weapons)  

### **Fight Tracker**
- Timer for boss attempts  
- Record victory/defeat with notes  
- Track weapon used per attempt  
- Fight session management  
- Automatic boss defeat marking on victory  

### **Additional Features**
- Tips and strategies page  
- Victory modal on boss defeat  
- Auth system (register/login)  
- Responsive design  

---

## **Tech Stack**

### **Frontend**
- **Framework:** SvelteKit  
- **Language:** TypeScript  
- **Styling:** TailwindCSS  
- **Deployment:** Vercel  
- **Features:** SSR, form actions, reactive stores  

### **Backend**
- **Framework:** ASP.NET Core 9  
- **Language:** C#  
- **Database:** PostgreSQL (Neon.tech)  
- **Caching:** Redis (Upstash)  
- **ORM:** Entity Framework Core  
- **Auth:** JWT tokens with ASP.NET Identity  
- **Deployment:** Render (Docker)  

---

## **Architecture**
- RESTful API with controllers  
- Repository pattern for data access  
- Service layer for business logic  
- Redis caching for boss/weapon lookups  
- CSV data seeding on startup  
- CORS configured for cross-origin requests  

---

## **Live App**
- **Frontend:** https://elden-ring-companion-alpha.vercel.app  
- **Backend API:** https://elden-ring-companion.onrender.com  

> **Note:** Backend is hosted on Render's free tier, which spins down after 15 minutes of inactivity.  
> The first request after idle takes ~30–60 seconds to wake up, then remains fast for 15 minutes.

---

## **Local Development Setup**

### **Prerequisites**
- Node.js 18+  
- .NET 9 SDK  
- PostgreSQL  
- Redis  

### **Frontend Setup**
```bash
cd frontend
npm install
npm run dev
