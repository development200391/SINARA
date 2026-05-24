1. Start semua service (DB + Redis + API + Web)

PowerShell:
cd D:\Projek\SINARA
Copy-Item .env.example .env -Force
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d --build
docker compose ps


2. Test Database (PostgreSQL)
PowerShell:
docker exec -it erp_postgres psql -U erp_user -d erp_db -c "\dt"
docker exec -it erp_postgres psql -U erp_user -d erp_db -c "select username,email from sys_users;"
docker exec -it erp_postgres psql -U erp_user -d erp_db -c "select name from cfg_roles order by id;"
docker exec -it erp_postgres psql -U erp_user -d erp_db -c "select code,name from cfg_modules order by sort_order;"

3. Test API
 
Cari log migration + seeding sukses saat startup.

PowerShell:
docker compose logs -f erp-api

Lalu cek endpoint dasar:

PowerShell:
curl http://localhost:8081/swagger/index.html

 
Ekspektasi: Swagger terbuka (HTTP 200). Catatan: endpoint bisnis penuh (auth/hr/config) baru lengkap di fase berikutnya.
4. Test Web

PowerShell:
curl http://localhost:8080
 
Ekspektasi: service web merespons (bisa 200/redirect/404 tergantung page yang sudah dibuat). Yang penting container   jalan tanpa crash.
5. Test Redis

PowerShell:
docker exec -it erp_redis redis-cli -1 ChangeMe_Redis! ping
 
Ekspektasi:  PONG.
6. Stop setelah test

PowerShell:
docker compose down