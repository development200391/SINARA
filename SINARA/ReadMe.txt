1. Start semua service (DB + Redis + API + Web)

PowerShell:
cd D:\Projek\SINARA
Copy-Item .env.example .env -Force
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d --build
docker compose ps

admin
Admin@123!