# Deploy SINARA ERP ke VPS Ubuntu 24.04

Arsitektur deploy ini:
- **PostgreSQL & Redis** → jalan di **Docker** (`docker-compose.prod.yml`)
- **ERP.API & ERP.Web** → jalan **native** langsung di VPS via .NET runtime, dikelola sebagai **systemd service** (auto-restart, auto-start saat boot)

Prasyarat dari sisi kamu:
- VPS Ubuntu 24.04 sudah aktif (IP address dan username sudah ada dari provider)
- SSH key sudah dibuat di komputer lokal: `C:\Users\kokos\.ssh\id_ed25519_sinara`
- Public key sudah didaftarkan/diimpor saat pembuatan VPS

---

## 1. Login ke VPS

```
ssh -i ~/.ssh/id_ed25519_sinara USERNAME@IP_VPS
```

Ganti `USERNAME` dan `IP_VPS` sesuai data dari provider.

---

## 2. Update sistem

```bash
sudo apt update && sudo apt upgrade -y
```

---

## 3. Install Docker & Docker Compose (untuk DB + Redis)

```bash
sudo apt install -y ca-certificates curl gnupg

sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

Cek instalasi:

```bash
docker --version
docker compose version
```

Supaya tidak perlu `sudo` tiap jalankan docker (opsional):

```bash
sudo usermod -aG docker $USER
```

Logout dan login SSH ulang setelah ini supaya grup baru berlaku.

---

## 4. Install .NET 8 Runtime (untuk jalankan ERP.API & ERP.Web)

```bash
sudo apt install -y wget
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt update
sudo apt install -y aspnetcore-runtime-8.0
```

Cek instalasi:

```bash
dotnet --list-runtimes
```

Harus muncul `Microsoft.AspNetCore.App 8.0.x` dan `Microsoft.NETCore.App 8.0.x`.

> Kalau nanti butuh build dari source langsung di server (bukan publish dari lokal), install juga `dotnet-sdk-8.0` sebagai gantinya/tambahan.

---

## 5. Install Git

```bash
sudo apt install -y git
```

---

## 6. Clone project

```bash
cd ~
git clone <URL_REPO_GIT_KAMU> SINARA
cd SINARA
```

> Kalau repo private, siapkan akses (SSH key GitHub atau personal access token) sebelum clone.

---

## 7. Setup file `.env` (untuk Postgres & Redis)

```bash
cp .env.example .env
nano .env
```

Isi dengan nilai **production yang aman** (jangan pakai nilai default `ChangeMe_*`):

```env
POSTGRES_DB=erp_db
POSTGRES_USER=erp_user
POSTGRES_PASSWORD=<password_kuat_kamu>

REDIS_PASSWORD=<password_redis_kamu>

JWT_ISSUER=ERPSystem
JWT_AUDIENCE=ERPClients
JWT_EXPIRY_MINUTES=60
REFRESH_TOKEN_EXPIRY_DAYS=7
JWT_SIGNING_KEY=<random_string_minimal_32_karakter>

ASPNETCORE_ENVIRONMENT=Production
```

Simpan (Ctrl+O, Enter, Ctrl+X di nano).

---

## 8. Jalankan PostgreSQL & Redis via Docker

File yang dipakai: **[docker-compose.prod.yml](docker-compose.prod.yml)** — isinya cuma `erp-db` dan `erp-redis`, port di-bind ke `127.0.0.1` saja (tidak terekspos ke publik, hanya bisa diakses dari VPS itu sendiri, tempat ERP.API nanti jalan).

```bash
docker compose -f docker-compose.prod.yml up -d
```

Cek status:

```bash
docker compose -f docker-compose.prod.yml ps
```

Cek log kalau ada masalah:

```bash
docker compose -f docker-compose.prod.yml logs -f erp-db
docker compose -f docker-compose.prod.yml logs -f erp-redis
```

---

## 9. Publish ERP.API & ERP.Web

Ada dua cara: publish di komputer lokal lalu upload hasilnya, atau build langsung di server. Cara paling praktis untuk server dengan RAM terbatas (2GB) adalah **publish di lokal**, supaya server tidak perlu proses compile yang berat.

### Opsi A — Publish dari komputer lokal (disarankan untuk VPS RAM kecil)

Di komputer lokal (PowerShell, folder project):

```powershell
dotnet publish ERP.API -c Release -o publish/api
dotnet publish ERP.Web -c Release -o publish/web
```

Upload ke server (dari lokal):

```powershell
scp -i ~/.ssh/id_ed25519_sinara -r publish/api USERNAME@IP_VPS:~/SINARA/publish-api
scp -i ~/.ssh/id_ed25519_sinara -r publish/web USERNAME@IP_VPS:~/SINARA/publish-web
```

### Opsi B — Build langsung di server (butuh `dotnet-sdk-8.0`, lebih berat ke RAM)

```bash
cd ~/SINARA
dotnet publish ERP.API -c Release -o ~/SINARA/publish-api
dotnet publish ERP.Web -c Release -o ~/SINARA/publish-web
```

---

## 10. Setup Connection String Production

Karena Postgres & Redis jalan di Docker dengan port di-bind ke `127.0.0.1`, dan ERP.API/ERP.Web jalan native di host yang sama, connection string cukup pakai `localhost`.

Buat file `~/SINARA/publish-api/appsettings.Production.json` (kalau belum ada) atau pastikan environment variable berikut di-set saat service jalan (diatur di langkah 11 lewat systemd):

```
ConnectionStrings__DefaultConnection = Host=localhost;Port=5432;Database=erp_db;Username=erp_user;Password=<sama_dengan_.env>
Redis__ConnectionString = localhost:6379,password=<sama_dengan_.env>
Redis__InstanceName = ERP_
JwtSettings__Issuer = ERPSystem
JwtSettings__Audience = ERPClients
JwtSettings__ExpiryMinutes = 60
JwtSettings__RefreshTokenExpiryDays = 7
JwtSettings__SigningKey = <sama_dengan_.env>
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://localhost:5000   (untuk API)
ASPNETCORE_URLS = http://localhost:5001   (untuk Web)
```

> Web juga butuh setting `ApiSettings__BaseUrl = http://localhost:5000` supaya bisa memanggil API.

---

## 11. Jalankan ERP.API & ERP.Web sebagai systemd service

Supaya otomatis jalan lagi kalau VPS restart/crash, buat service systemd.

### Service untuk API

```bash
sudo nano /etc/systemd/system/erp-api.service
```

Isi:

```ini
[Unit]
Description=SINARA ERP API
After=network.target docker.service

[Service]
WorkingDirectory=/home/USERNAME/SINARA/publish-api
ExecStart=/usr/bin/dotnet /home/USERNAME/SINARA/publish-api/ERP.API.dll
Restart=always
RestartSec=10
User=USERNAME
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=erp_db;Username=erp_user;Password=<password_kamu>
Environment=Redis__ConnectionString=localhost:6379,password=<password_redis_kamu>
Environment=Redis__InstanceName=ERP_
Environment=JwtSettings__Issuer=ERPSystem
Environment=JwtSettings__Audience=ERPClients
Environment=JwtSettings__ExpiryMinutes=60
Environment=JwtSettings__RefreshTokenExpiryDays=7
Environment=JwtSettings__SigningKey=<jwt_signing_key_kamu>

[Install]
WantedBy=multi-user.target
```

Ganti `USERNAME` dan semua `<...>` dengan nilai sebenarnya (samakan dengan `.env`).

### Service untuk Web

```bash
sudo nano /etc/systemd/system/erp-web.service
```

Isi:

```ini
[Unit]
Description=SINARA ERP Web
After=network.target erp-api.service

[Service]
WorkingDirectory=/home/USERNAME/SINARA/publish-web
ExecStart=/usr/bin/dotnet /home/USERNAME/SINARA/publish-web/ERP.Web.dll
Restart=always
RestartSec=10
User=USERNAME
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5001
Environment=ApiSettings__BaseUrl=http://localhost:5000
Environment=Redis__ConnectionString=localhost:6379,password=<password_redis_kamu>
Environment=Redis__InstanceName=ERP_

[Install]
WantedBy=multi-user.target
```

### Aktifkan kedua service

```bash
sudo systemctl daemon-reload
sudo systemctl enable erp-api erp-web
sudo systemctl start erp-api erp-web
```

Cek status:

```bash
sudo systemctl status erp-api
sudo systemctl status erp-web
```

Cek log:

```bash
journalctl -u erp-api -f
journalctl -u erp-web -f
```

---

## 12. Jalankan migration database (kalau perlu)

Kalau EF Core migration tidak otomatis jalan saat startup:

```bash
cd ~/SINARA
dotnet ef database update --project ERP.Infrastructure --startup-project ERP.API
```

(Sesuaikan connection string yang dipakai `dotnet ef` mengarah ke `localhost:5432`, bukan container network.)

---

## 13. Pasang Nginx sebagai reverse proxy + HTTPS

Karena API & Web sekarang jalan di `localhost:5000`/`localhost:5001` (tidak diekspos langsung ke publik), pakai Nginx di depan untuk expose ke internet dengan domain + SSL.

```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

Buat config di `/etc/nginx/sites-available/sinara`:

```nginx
server {
    listen 80;
    server_name erp.namadomainkamu.com;

    location / {
        proxy_pass http://localhost:5001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name api.namadomainkamu.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Aktifkan:

```bash
sudo ln -s /etc/nginx/sites-available/sinara /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

Pasang SSL gratis (butuh domain sudah di-pointing ke IP VPS):

```bash
sudo certbot --nginx -d erp.namadomainkamu.com -d api.namadomainkamu.com
```

---

## 14. Setup Firewall (UFW)

```bash
sudo apt install -y ufw
sudo ufw allow OpenSSH
sudo ufw allow "Nginx Full"
sudo ufw enable
sudo ufw status
```

> Port 5432, 6379, 5000, 5001 **tidak perlu** dibuka di firewall — semuanya hanya diakses via `localhost` (Postgres/Redis dari Docker ke `127.0.0.1`, API/Web diakses lewat Nginx).

---

## Perintah harian yang berguna

```bash
# Restart DB & Redis (Docker)
docker compose -f docker-compose.prod.yml restart

# Restart API & Web (systemd)
sudo systemctl restart erp-api erp-web

# Cek log real-time
journalctl -u erp-api -f
journalctl -u erp-web -f

# Update kode terbaru
cd ~/SINARA
git pull

# Publish ulang & restart (setelah update kode)
dotnet publish ERP.API -c Release -o ~/SINARA/publish-api
dotnet publish ERP.Web -c Release -o ~/SINARA/publish-web
sudo systemctl restart erp-api erp-web
```
