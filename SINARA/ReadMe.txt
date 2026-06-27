1. Start semua service (DB + Redis + API + Web)

PowerShell:
cd D:\Projek\SINARA
Copy-Item .env.example .env -Force
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d --build
docker compose ps

admin
Admin@123!

buat buid aja
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d --build erp-api erp-web```



Fase 8 udh di finance

setiap buat grid, pakai pagedgrid dan jangan lupa semua multi language


FLUSH REDIS
docker exec -it erp_redis redis-cli -a "ChangeMe_Redis!"
FLUSHALL ASYNC