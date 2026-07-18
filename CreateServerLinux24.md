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

Tidak perlu buat file `appsettings.Production.json` — semua nilai ini langsung di-set sebagai environment variable di file systemd (langkah 11), jadi cukup pastikan nilainya konsisten:

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
ASPNETCORE_URLS = http://0.0.0.0:5000   (untuk API — bisa diakses dari luar VPS lewat IP:Port)
ASPNETCORE_URLS = http://0.0.0.0:5001   (untuk Web — bisa diakses dari luar VPS lewat IP:Port)
```

> `Host=localhost` dan `Redis__ConnectionString=localhost:...` tetap dipakai walau `ASPNETCORE_URLS` pakai `0.0.0.0` — karena Postgres/Redis cuma dipanggil dari ERP.API/ERP.Web yang jalan di VPS yang sama (bukan dari luar).
>
> Kalau nanti pakai domain + Nginx (Opsi B di langkah 13), ganti `ASPNETCORE_URLS` jadi `http://localhost:5000`/`5001` lagi (tidak perlu expose ke `0.0.0.0`, karena Nginx yang jadi pintu masuk publik).
>
> Web juga butuh setting `ApiSettings__BaseUrl = http://localhost:5000` supaya bisa memanggil API — ini **tidak berubah** di kedua opsi, karena panggilan Web→API selalu lewat `localhost` (server-to-server).

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
WorkingDirectory=/home/sinaraadmin/SINARA/publish-api
ExecStart=/usr/bin/dotnet /home/sinaraadmin/SINARA/publish-api/ERP.API.dll
Restart=always
RestartSec=10
User=sinaraadmin
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
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
WorkingDirectory=/home/sinaraadmin/SINARA/publish-web
ExecStart=/usr/bin/dotnet /home/sinaraadmin/SINARA/publish-web/ERP.Web.dll
Restart=always
RestartSec=10
User=sinaraadmin
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5001
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

## 12. Migration database

Tidak perlu langkah manual — project ini sudah otomatis menjalankan migration EF Core saat ERP.API start (`ERP.API/Program.cs` memanggil `db.Database.MigrateAsync()`). Begitu service `erp-api` (langkah 11) berhasil start, database akan otomatis ter-update ke skema terbaru.

Untuk memastikan migration sukses, cek log:

```bash
journalctl -u erp-api -f
```

Kalau ada error koneksi database di log, cek lagi connection string di `/etc/systemd/system/erp-api.service` dan pastikan container `erp-db` sudah running (`docker compose -f docker-compose.prod.yml ps`).

---

## 13. Expose aplikasi ke publik

Pilih salah satu — tergantung apakah kamu sudah punya domain atau belum.

### Opsi A — Belum punya domain: akses langsung via IP:Port

Ini opsi default di panduan ini — file systemd di langkah 11 sudah langsung diset `ASPNETCORE_URLS=http://0.0.0.0:5000` (API) dan `http://0.0.0.0:5001` (Web), jadi tidak ada langkah tambahan di sini. Kalau service sudah running (langkah 11), langsung akses dari browser:
```
http://IP_VPS:5001   -> Web
http://IP_VPS:5000   -> API
```

> Catatan: ini tanpa HTTPS (koneksi tidak dienkripsi) — cukup untuk testing/internal, tapi kalau data sensitif (login, dsb) sebaiknya upgrade ke Opsi B begitu ada domain.

### Opsi B — Sudah/nanti punya domain: pakai Nginx + HTTPS

Kalau domain sudah diarahkan (A record) ke IP VPS, pakai Nginx sebagai reverse proxy supaya bisa akses via `https://` dengan URL rapi. Ubah dulu `ASPNETCORE_URLS` di kedua file systemd (langkah 11) dari `http://0.0.0.0:...` jadi `http://localhost:...` (tidak perlu expose langsung ke luar lagi, karena Nginx yang jadi pintu masuk publik), lalu `sudo systemctl daemon-reload && sudo systemctl restart erp-api erp-web`.

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
```

Lanjutkan sesuai opsi yang dipakai di langkah 13:

**Kalau pakai Opsi A (akses via IP:Port):**
```bash
sudo ufw allow 5000/tcp
sudo ufw allow 5001/tcp
```

**Kalau pakai Opsi B (Nginx + domain):**
```bash
sudo ufw allow "Nginx Full"
```
> Port 5000/5001 **tidak perlu** dibuka di firewall untuk Opsi B — akses hanya lewat Nginx (port 80/443), aplikasi tetap `localhost` saja.

Terakhir, aktifkan:
```bash
sudo ufw enable
sudo ufw status
```

> Port 5432 (Postgres) dan 6379 (Redis) **tidak perlu** dibuka sama sekali — sudah di-bind ke `127.0.0.1` di `docker-compose.prod.yml`, hanya bisa diakses dari VPS itu sendiri.

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

# Publish dari update project
1. Kerja seperti biasa di lokal — edit kode, test, lalu commit & push ke git (kamu commit sendiri sesuai kebiasaan).

2. Publish ulang project yang berubah (di lokal, PowerShell):


cd D:\NET\SINARA
dotnet publish ERP.API -c Release -o publish/api
dotnet publish ERP.Web -c Release -o publish/web
(Kalau cuma salah satu yang berubah, publish yang itu saja.)

3. Upload hasil publish ke VPS:


scp -i C:\Users\kokos\.ssh\id_ed25519_sinara -r publish/api/. sinaraadmin@103.127.137.63:~/SINARA/publish-api/
scp -i C:\Users\kokos\.ssh\id_ed25519_sinara -r publish/web/. sinaraadmin@103.127.137.63:~/SINARA/publish-web/
4. Restart service di VPS:


ssh -i C:\Users\kokos\.ssh\id_ed25519_sinara sinaraadmin@103.127.137.63 "sudo systemctl restart erp-api erp-web"


```
