ReadMe General Document Module SINARA

Dokumen ini menjelaskan modul General Document: infrastruktur penyimpanan dan
lampiran dokumen terpusat yang bisa dipasang ke transaksi apapun di SINARA
ERP, tanpa setiap modul perlu bikin tabel upload dan logic penyimpanan file
sendiri-sendiri. Modul asal cukup mendaftarkan reference_type + reference_id
ke tabel doc_documents.

Rencana lengkap (fase pengerjaan, skema database, dsb.) ada di
SINARA_ERP_GeneralDocument_Panduan_Detail.docx. Dokumen ini fokus ke apa yang
sudah benar-benar berjalan dan cara pakainya.

Struktur Menu General Document

1. Document Categories
   Route: /document/categories

(Modul ini tidak punya menu untuk browse semua dokumen lintas modul — upload/
list/download/delete dokumen selalu diakses dari halaman modul asalnya,
misalnya dari Details Leave Request di HR. Lihat "Kegunaan Tiap Menu" di
bawah untuk detail integrasinya.)

Kegunaan Tiap Menu

1. Document Categories (/document/categories)
- Master kategori dokumen: code, name, module (opsional, penanda modul
  pemilik kategori), status aktif.
- CRUD lengkap (Index/Create/Edit/Delete) dengan search, lewat
  DocumentCategoriesController (Web) yang manggil DocumentCategoriesController
  (API) di /api/v1/document-categories.
- Kategori dipakai opsional saat upload dokumen (dropdown), murni buat
  pengelompokan — dokumen tetap bisa diupload tanpa pilih kategori.
- Seed default (dibuat otomatis oleh DataSeeder, idempotent):
  * LEAVE_EVIDENCE — Bukti/Lampiran Pengajuan Cuti (module: HR)
  * SICK_NOTE — Surat Keterangan Sakit (module: HR)
  * GENERAL — Dokumen Umum (module: -)
- Kategori tidak bisa dihapus kalau masih dipakai oleh dokumen yang sudah
  diupload (dicek lewat DocDocument.CategoryId).

2. Upload/List/Download/Delete Dokumen (tidak ada menu sendiri)
- Ini bukan halaman mandiri — selalu diakses dari halaman modul yang
  "menempelkan" dokumen ke recordnya. Integrasi pertama & satu-satunya saat
  ini: HR Leave Requests (lihat ReadMeHr.md bagian "Leave Requests" dan
  "General Document").
- Endpoint generik yang dipakai modul manapun untuk integrasi baru:
  * GET  /api/v1/documents?referenceType=&referenceId= — daftar dokumen
    untuk satu transaksi.
  * POST /api/v1/documents (multipart/form-data: file, referenceType,
    referenceId, categoryId?, description?) — upload.
  * GET  /api/v1/documents/{id}/download — download (streaming, ber-otorisasi,
    bukan URL statis).
  * DELETE /api/v1/documents/{id} — hapus (hard delete row + file fisik).
  * GET  /api/v1/documents/categories — daftar kategori aktif (buat dropdown
    upload, beda dari CRUD admin di /api/v1/document-categories).

Cara Pasang Dokumen ke Modul Baru
- Tambahkan reference_type (nama tabel snake_case, contoh: fa_asset_transfers)
  ke whitelist AllowedReferenceTypes di DocumentService.cs (server-side, tidak
  boleh terima string bebas dari client demi keamanan).
- Tambahkan rule otorisasi untuk reference_type itu di
  EnsureReferenceAccessAsync (siapa yang boleh lihat/upload/hapus dokumen
  untuk record tsb — untuk Leave Requests aturannya: pemilik leave request
  sendiri, atau user tanpa profil karyawan yang dianggap staff HR/back-office).
- Panggil endpoint generik di atas dari halaman modul tsb (Web/mobile) sambil
  kirim reference_type yang baru didaftarkan + reference_id record terkait.
- Tidak perlu bikin tabel/migration baru — doc_documents dipakai bersama oleh
  semua modul.

Penyimpanan File
- File fisik disimpan di server API (ERP.API/App_Data/uploads/documents/
  {referenceType}/{referenceId}/), BUKAN di ERP.Web/wwwroot — jadi tidak bisa
  diakses lewat URL statis langsung, cuma lewat endpoint download yang
  mengecek otorisasi setiap kali diakses.
- Nama file di disk selalu digenerate GUID + ekstensi asli (mencegah path
  traversal / tebak nama file / collision). Nama file asli yang diupload user
  cuma disimpan sebagai metadata (OriginalFileName) untuk ditampilkan &
  dipakai sebagai nama file saat di-download.
- Folder App_Data/uploads di-gitignore (runtime files, sama seperti pola
  ERP.Web/wwwroot/uploads/employees yang sudah ada untuk foto karyawan).
- Batas & validasi (appsettings DocumentSettings, ERP.API):
  * MaxFileSizeBytes: 5 MB (default)
  * AllowedExtensions: .pdf, .jpg, .jpeg, .png, .docx
  * StorageDirectory: App_Data/uploads/documents

Integrasi Web (ERP.Web)
- Halaman Details Leave Request (/hr/leave/requests/details/{id}) punya
  bagian "Attachments": list dokumen terlampir (nama file, ukuran, kategori,
  siapa & kapan upload) + tombol Download, form upload, tombol Delete.
- Upload & Delete cuma muncul/diizinkan selama leave request masih berstatus
  Pending (sama seperti aturan Edit leave request yang sudah ada) — sekali
  Approved/Rejected, lampiran tidak bisa diubah lagi lewat halaman ini.

Integrasi Mobile (AbsenKu, Flutter)
- Form "Ajukan Cuti / Sakit" punya field lampiran opsional (pakai package
  file_picker: pdf/jpg/jpeg/png/docx, maks 5 MB).
- Alur 2 langkah: submit leave request dulu (server balikin id-nya), baru
  upload lampiran pakai id tsb — karena reference_id baru ada setelah leave
  request berhasil dibuat.
- Kalau upload lampiran gagal padahal leave request-nya sudah tersimpan,
  layar tidak menganggap itu gagal total: form dikunci (mencegah submit
  dobel) dan tampil pesan bahwa cutinya sudah tersimpan, lampiran perlu
  diupload ulang nanti.
- Layar Riwayat Cuti punya ikon lampiran per pengajuan (bottom sheet,
  lazy-loaded saat diklik, tampilkan nama file + ukuran).
- Acuan: D:\Flutter\AbsenKu\lib\features\leave\ (models/leave_models.dart —
  class LeaveDocument, data/leave_repository.dart — getAttachments &
  uploadAttachment, presentation/leave_request_screen.dart,
  presentation/leave_history_screen.dart).

Catatan Permission
- Semua controller Document memakai [Authorize] umum (belum granular per
  menu seperti [RequireMenuPermission] yang cuma dipakai HR Departments).
- Module "General Document" (kode DOC) & menu "Document Categories" otomatis
  dapat izin penuh (view/create/edit/delete) untuk role Super Admin lewat
  SeedSuperAdminPermissionsAsync, sama seperti seluruh menu lain. Role selain
  Super Admin belum di-seed permission spesifik untuk modul ini.
- Otorisasi akses dokumen per-record (misalnya "cuma pemilik leave request
  atau HR staff yang boleh lihat/upload/hapus lampirannya") dicek di service
  layer (DocumentService.EnsureReferenceAccessAsync), bukan lewat
  [RequireMenuPermission] — karena aturannya spesifik per reference_type,
  bukan sekadar per-menu.

Catatan Gap Implementasi (untuk backlog)
- Belum ada halaman admin untuk melihat/mencari semua dokumen lintas modul
  (cuma bisa dilihat dari halaman modul asalnya).
- Baru satu reference_type yang terdaftar (hr_leave_requests). Rencana
  lanjutan: Fixed Assets (transfer/disposal), Purchasing (PO), General
  Approval — lihat SINARA_ERP_GeneralDocument_Panduan_Detail.docx untuk
  urutan prioritas.
- Tidak ada preview file di browser/app (PDF/gambar) — download langsung ke
  device, belum ada inline viewer.
- Tidak ada antivirus/malware scanning untuk file yang diupload — validasi
  cuma dari ekstensi & ukuran, bukan isi file.
- Migration 20260712180000_AddGeneralDocument ditulis manual (bukan hasil
  dotnet ef migrations add) karena tooling migration scaffolding di project
  ini sedang crash untuk SEMUA migration baru — root cause pra-existing
  (AppDbContextModelSnapshot.cs sudah lama tidak sinkron, jauh sebelum modul
  ini dibuat), lihat detail penjelasannya di ReadMeHr.md bagian "General
  Document" dan di Panduan Detail docx.

Acuan Implementasi
- Web controller:
  ERP.Web/Controllers/Document/DocumentCategoriesController.cs
  (CRUD kategori admin), ERP.Web/Controllers/HR/HrLeaveRequestsController.cs
  (integrasi upload/list/download/delete dokumen di Leave Requests).
- Web views:
  ERP.Web/Views/Document/DocumentCategories/*
- Web API client:
  ERP.Web/Services/DocumentApiClient.cs (kategori admin),
  ERP.Web/Services/HrApiClient.cs (upload/list/download/delete dokumen,
  dipanggil dari halaman Leave Requests).
- API controller:
  ERP.API/Controllers/v1/Document/DocumentsController.cs,
  ERP.API/Controllers/v1/Document/DocumentCategoriesController.cs
- Application services:
  ERP.Application/Services/Document/DocumentService.cs,
  ERP.Application/Services/Document/IDocumentStorageService.cs
  (implementasi konkretnya di ERP.API/Services/DocumentStorageService.cs,
  karena butuh IWebHostEnvironment dari project hosting).
- Domain entities:
  ERP.Domain/Entities/Document/DocDocument.cs,
  ERP.Domain/Entities/Document/DocDocumentCategory.cs
- Konfigurasi:
  ERP.Application/Options/DocumentSettings.cs,
  ERP.API/appsettings.json (section DocumentSettings)
- Seed menu & kategori default:
  ERP.Infrastructure/Data/DataSeeder.cs
- Mobile (Flutter, repo terpisah):
  D:\Flutter\AbsenKu\lib\features\leave\
