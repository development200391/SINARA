SINARA (Sistem Niaga Ramdan)
=========================================

Deskripsi Singkat
-----------------------------------------
SINARA adalah aplikasi ERP modular berbasis ASP.NET Core (.NET 8) dengan arsitektur terpisah antara Web MVC, REST API, Application, Domain, dan Infrastructure.

Teknologi Utama
-----------------------------------------
- Backend API: ASP.NET Core Web API
- Frontend: ASP.NET Core MVC + Razor Views
- Database: PostgreSQL 16
- Caching: Redis 7
- ORM: Entity Framework Core 8 (Code First)
- Otentikasi: JWT Bearer + ASP.NET Core Identity (akan diimplementasikan bertahap)
- UI: Bootstrap 5.3
- Container: Docker + Docker Compose

Struktur Solusi
-----------------------------------------
- ERP.sln
- ERP.API/             -> REST API
- ERP.Web/             -> MVC + Razor Views
- ERP.Application/     -> Service layer, DTO, validator, mapping
- ERP.Domain/          -> Entity, enum, interface domain
- ERP.Infrastructure/  -> DbContext, repository, cache Redis
- docker/api/Dockerfile
- docker/web/Dockerfile
- docker-compose.yml
- docker-compose.override.yml
- .env.example

Modul Fase 1
-----------------------------------------
1. Human Resources (HR)
2. System Configuration

Konvensi Inti
-----------------------------------------
- Primary key: int (identity PostgreSQL)
- Nama tabel/kolom: snake_case
- Soft delete + audit columns
- RBAC berbasis role/menu permission
- Cache-aside pattern di service layer

Catatan Status Saat Ini
-----------------------------------------
Dokumen dan struktur Step 1 (Solution & Docker Setup) sudah disiapkan.
Implementasi domain, migration, service, API endpoint, dan UI modul akan dilanjutkan pada step berikutnya.
