# General Document Module

Infrastruktur penyimpanan & lampiran dokumen terpusat yang bisa dipasang ke transaksi apapun di SINARA ERP, tanpa setiap modul perlu bikin tabel upload dan logic penyimpanan file sendiri-sendiri. Modul asal cukup mendaftarkan `reference_type` + `reference_id` ke tabel `doc_documents`. **Integrasi pertama & satu-satunya sejauh ini: HR Leave Requests** (lihat §5).

Rencana desain awal (`SINARA_ERP_GeneralDocument_Panduan_Detail.docx`) ditulis untuk arsitektur berbasis kategori yang **sudah tidak dipakai lagi** — anggap docx sebagai arsip sejarah, dokumen ini yang jadi acuan arsitektur final. Perubahan penting dari desain awal: (1) Document Categories dihapus total, diganti `doc_reference_type_configs` (aturan validasi per modul, bukan pengelompokan dokumen); (2) upload digabung dengan create/update record induk dalam SATU request multipart ("Opsi B" combined-submit, lihat §4), bukan create-dulu-baru-upload.

## 1. Entitas Domain (`ERP.Domain/Entities/Document`)

- **`DocDocument`** — reference_type + reference_id (generik, bukan FK per modul), nama file asli (metadata), nama file di disk (GUID + ekstensi), ukuran, Description (note per dokumen), UploadedBy/At. **Tidak punya kolom penunjuk slot/detail spesifik** — lihat §7 untuk implikasinya.
- **`DocReferenceTypeConfig`** (master) — satu baris per reference_type: ReferenceType (kode unik, mis. `hr_leave_requests`), DisplayName, IsMultiple (single slot vs multi slot), MaxFileCount (jumlah slot, cuma berlaku kalau IsMultiple=true — kalau false dipaksa 1), IsActive.
- **`DocReferenceTypeConfigDetail`** (detail, child dari master via `config_id`, cascade delete) — satu baris per SLOT lampiran: Name (label slot, mis. "KTP"/"Slip Gaji"), MaxFileSizeBytes (nullable, fallback ke `DocumentSettings` global), IsRequired (wajib per-slot), IsActive, AllowedExtensions (nullable, fallback ke default global). Jumlah baris detail selalu harus sama dengan MaxFileCount efektif — ditegakkan di `ValidateConfigRequest` (server), bukan cuma UI.

## 2. API Endpoints (`ERP.API/Controllers/v1/Document`)

| Controller | Endpoint utama |
|---|---|
| `DocumentReferenceTypeConfigsController` | CRUD master+detail config; tidak bisa dihapus kalau reference_type sudah punya dokumen terupload |
| `DocumentsController` | `GET ?referenceType=&referenceId=` (daftar dokumen satu transaksi), `GET config?referenceType=` (ambil aturan validasi, 404 kalau reference_type belum terdaftar/nonaktif), `GET {id}/download` (streaming, ber-otorisasi, bukan URL statis), `DELETE {id}` (hard delete row + file fisik), `POST` (single-file standalone — ada untuk modul yang tidak butuh combined-submit, tapi Leave Requests tidak memakainya) |

Endpoint `config` dipakai frontend (Web & mobile) untuk render form upload sesuai aturan (required/max size/max count/ekstensi) sebelum submit — client-side hint saja, validasi sebenarnya tetap di server.

## 3. Halaman Web (`ERP.Web/Controllers/Document/DocumentReferenceTypeConfigsController.cs`)

- **Document Settings** (`/document/reference-type-configs`) — satu-satunya menu modul ini: CRUD master+detail config dengan search. Form Create/Edit generate baris detail secara LIVE via JavaScript begitu Max File Count diketik (pola `<template>` + renumber(), sama seperti Journal Lines) — checkbox Multiple mati membuat Max File Count readonly (dipaksa 1) dan tabel detail otomatis diciutkan ke 1 baris.
- **Tidak ada menu untuk browse semua dokumen lintas modul** — upload/list/download/delete dokumen selalu diakses dari halaman modul asalnya (mis. Create/Edit/Details Leave Request di HR), bukan dari modul ini.
- **`GeneralDocumentUpload`** (`ERP.Web/Views/Shared/Components/GeneralDocumentUpload/Default.cshtml`) — satu-satunya ViewComponent tampilan dokumen (komponen lama `GeneralDocumentList` sudah dihapus total). Slot-aware: render satu blok per slot aktif (nama slot + tag Wajib/Opsional, file control + note sendiri-sendiri per slot). Slot yang sudah punya dokumen menampilkan info file + tombol Download/Delete (bukan file picker lagi — untuk ganti file, hapus dulu). `ReadOnly=true` mematikan file picker & tombol Delete, dipakai di halaman Details.

## 4. Business Rules / Logic Penting

**Pola Combined-Submit ("Opsi B")** — cara memasang dokumen ke modul baru:
1. Endpoint create/update modul (bukan `DocumentsController`) menerima `[FromForm]` gabungan: field record induk + `List<IFormFile>? Files` + `List<string?>? Notes` (index sejajar dengan Files), `[Consumes("multipart/form-data")]`.
2. Validasi required/max-count terhadap config reference_type SEBELUM record induk dibuat — record tidak pernah tersimpan kalau lampiran wajib tidak ada.
3. CREATE: loop upload tiap file lewat `DocumentService.UploadAsync` (Description = Notes[i] sejajar) — validasi per-file (ekstensi/ukuran/corruption) di sini.
4. UPDATE: per slot index i — Files[i] ada isi → upload sebagai dokumen baru; Files[i] kosong tapi ada dokumen existing di slot itu → update Description-nya saja lewat `UpdateDescriptionAsync` (edit note tanpa upload ulang).
5. Kegagalan per-file tidak membatalkan keseluruhan request — record induk tetap sukses, file gagal dilaporkan lewat `AttachmentWarnings` di response; klien wajib menampilkan warning ini, bukan diam-diam mengabaikan.
6. Tambahkan reference_type baru ke pengecekan server (`GetActiveConfigEntityAsync` + switch di `EnsureAuthorizationAsync` — lihat §7 untuk klarifikasi istilah "whitelist").
7. Tambahkan rule otorisasi untuk reference_type itu di `EnsureAuthorizationAsync`.
8. Insert 1 baris `doc_reference_type_configs` + detail-nya (lewat menu Document Settings atau seed).
9. Tidak perlu tabel/migration baru untuk dokumennya sendiri — `doc_documents` dipakai bersama semua modul.

**Kenapa combined-submit, bukan staged-upload?** Desain awal (upload file duluan sebelum record induk ada, dikaitkan belakangan) ditolak karena butuh cleanup file "yatim" kalau user batal submit. Combined-submit lebih sederhana: tidak ada file tersimpan sebelum record induknya pasti tersimpan.

**Validasi file saat upload** (`DocumentService.ValidateFileAsync`)
- Ekstensi & ukuran: **sementara** dicek terhadap baris detail PERTAMA (SortOrder terkecil) saja, bukan per-slot yang sebenarnya disubmit — server belum tahu file yang masuk itu untuk slot mana (`DocDocument` belum punya kolom penunjuk slot). Keputusan sadar sambil integrasi per-slot dikerjakan bertahap (lihat §7).
- Jumlah file: ditolak kalau dokumen existing + baru melebihi MaxFileCount (level master, bukan per-slot).
- Integrity/corruption check (`ValidateFileIntegrityAsync`): gambar via `ImageSharp.DetectFormatAsync`; PDF dicek 5 byte pertama = `%PDF-`; DOCX dibuka sebagai `ZipArchive` dan dicek ada entry `[Content_Types].xml`; stream yang tidak bisa di-seek → validasi integrity di-skip (bukan gagal).

**Penyimpanan file**
- File fisik di `ERP.API/App_Data/uploads/documents/{referenceType}/{referenceId}/` (bukan `wwwroot`) — tidak bisa diakses lewat URL statis, cuma lewat endpoint download ber-otorisasi.
- Nama file di disk selalu GUID + ekstensi asli (cegah path traversal/tebak nama/collision); nama asli cuma metadata (`OriginalFileName`). Folder `App_Data/uploads` di-gitignore.
- Default global (`appsettings` `DocumentSettings`, fallback kalau field config null): MaxFileSizeBytes 5 MB, AllowedExtensions `.pdf/.jpg/.jpeg/.png/.docx`, StorageDirectory `App_Data/uploads/documents`.

**Integrasi Web — trik binding yang perlu diperhatikan kalau menambah modul baru**
- File input SEMUA slot **wajib** pakai `name` yang PERSIS SAMA (`AttachmentFiles`, tanpa index) — `FormFileModelBinder` untuk `List<IFormFile>` memanggil `Request.Form.Files.GetFiles(namaPersisIni)` langsung, bukan lewat index-discovery; nama field berbeda per slot = semua gagal ke-bind tanpa error. Slot yang sudah punya file tetap merender file input tersembunyi (kosong, `tabindex="-1"`) supaya posisi entry `Files[i]` di server tidak bergeser untuk slot sesudahnya.
- Note per-slot dikirim terindeks (`AttachmentNotes[0]`, `[1]`, dst. — `List<string?>` biasa, TIDAK kena aturan spesial file binder) dan **harus selalu dikirim untuk semua slot** supaya server tahu note mana dipakai untuk update dokumen existing.
- Tombol Delete per slot dirender di dalam form utama tapi `<form>` yang sesungguhnya di-submit dirender OUTSIDE form utama (setelah `</form>`), dihubungkan lewat atribut HTML5 `form="id-form-tsb"` — valid HTML5, menghindari nested `<form>` yang otomatis di-drop browser.
- Positional slot matching: karena `DocDocument` belum ada kolom slot, Web mencocokkan dokumen existing ke slot lewat URUTAN `UploadedAt` (dokumen pertama = slot pertama, dst.) — heuristik, bukan pengait sungguhan (lihat §7).

## 5. Integrasi Modul HR Leave Request (satu-satunya integrasi nyata sejauh ini)

**Web** (`ERP.Web`, lihat `ReadMeHr.md` untuk sisi bisnis)
- Halaman Create & Edit: satu `GeneralDocumentUpload` di dalam form utama, berisi semua slot (file + note per slot); slot yang sudah ada dokumennya (Edit) otomatis tampil dengan tombol Download/Delete.
- Halaman Details: `GeneralDocumentUpload` yang sama dipanggil dengan `ReadOnly=true` — tidak ada form dokumen sama sekali di halaman ini.

**Mobile (AbsenKu, Flutter)** — sudah di-rework ke master-detail menyusul Web (arsitektur disamakan sedapat mungkin; tidak ada Delete/existing-file karena layar CREATE-only):
- `DocumentReferenceTypeConfig` (`leave_models.dart`) punya field `details` (`List<DocumentReferenceTypeConfigDetail>`, cuma yang aktif) — persis mengikuti struktur server.
- Form "Ajukan Cuti/Sakit" ambil config saat layar dibuka, render satu baris per slot (nama, tag Wajib/Opsional, file picker + note TextField sendiri-sendiri). Validasi wajib dicek per slot sebelum submit.
- Submit pakai satu request multipart (`leave_repository.dart` method `submit()`) — parameter `slots` (bytes/fileName/note per slot). Sama seperti Web: setiap slot wajib mengirim bagian Files (kosong kalau tidak diisi) supaya posisi entry tidak bergeser; note dikirim terindeks sejajar Files.
- Response submit membawa `attachmentWarnings` kalau ada file gagal upload — ditampilkan lewat dialog sebelum layar ditutup.
- Layar Riwayat Cuti tidak berubah — masih menampilkan daftar dokumen flat per pengajuan (bottom sheet, lazy-load), karena cuma VIEW dokumen yang sudah ada.
- Acuan: `D:\Flutter\AbsenKu\lib\features\leave\` (models, `leave_repository.dart`, `leave_request_screen.dart`, `leave_history_screen.dart`).

## 6. Relasi Kunci

- `DocDocument` → record sumber via `ReferenceType` + `ReferenceId` (generik, bukan FK — pola sama dengan General Approval).
- `DocReferenceTypeConfigDetail` → `DocReferenceTypeConfig` (many-to-one, cascade delete).
- `DocDocument` ↔ `DocReferenceTypeConfigDetail`: **tidak ada FK langsung** — pencocokan slot murni heuristik urutan upload (lihat §7).
- `HrLeaveRequest` → `DocDocument` (via ReferenceType=`hr_leave_requests`) — satu-satunya integrasi aktif; otorisasi akses lampirannya juga ikut cek `apv_approval_steps` (lihat `ReadMeGeneralApproval.md` §5).

## 7. Known Gaps / Belum Lengkap

- **Positional slot matching belum benar-benar solid**: `DocDocument` belum punya kolom yang menunjuk ke baris detail/slot tertentu — pencocokan ke slot (baik saat tampil di Details maupun submit di Create/Edit) memakai urutan `UploadedAt` sebagai proxy. Konsisten dari kedua sisi, tapi tetap heuristik. Enforcement required/size/extension di **server** juga masih pakai proxy baris detail PERTAMA untuk semua file dalam satu submission, bukan aturan per-slot sesungguhnya — client-side sudah benar per-slot, server belum. Untuk menutup penuh: perlu kolom penunjuk slot eksplisit di `doc_documents` + `DocumentService.UploadAsync`/`ValidateFileAsync` dibuat slot-aware sungguhan.
- **Belum ada halaman admin untuk browse/cari semua dokumen lintas modul** — cuma bisa dilihat dari halaman modul asalnya.
- **Baru satu reference_type terdaftar** (`hr_leave_requests`). Rencana lanjutan: Fixed Assets (transfer/disposal), Purchasing (PO) — General Approval sendiri sudah terhubung ke `hr_leave_requests` juga, tapi APV bukan pemakai `doc_reference_type_configs`, cuma numpang string reference_type yang sama.
- **Tidak ada preview file** (PDF/gambar) di browser/app — download langsung ke device, belum ada inline viewer.
- **Tidak ada antivirus/malware scanning** — validasi cuma ekstensi, ukuran, dan format/magic-header, bukan pemindaian malware sesungguhnya.
- **Klarifikasi istilah** (verifikasi 2026-07-28): "whitelist AllowedReferenceTypes" di §4 poin 6 tidak merujuk ke satu named constant — gate-nya sebenarnya dua pengecekan terpisah (`GetActiveConfigEntityAsync` yang mewajibkan baris config aktif, plus switch hardcode di `EnsureAuthorizationAsync`). Perilakunya identik dengan yang dideskripsikan, cuma bukan literally satu daftar bernama.
- **Utang teknis migration**: `20260712180000_AddGeneralDocument` ditulis manual (bukan `dotnet ef migrations add`) karena `AppDbContextModelSnapshot.cs` sudah lama tidak sinkron (root cause pra-existing, bukan ditimbulkan modul ini) — lihat detail di `ReadMeHr.md` §6.
- **Verifikasi ulang 2026-07-28**: re-scan penuh + grep `GeneralDocumentUpload` di seluruh solusi — tidak ada drift, masih hanya `hr_leave_requests` yang terdaftar dan belum ada view Inventory/Finance/Manufacturing/FixedAssets/Purchasing/Sales yang memakai komponen upload ini.
- **Sudah diperbaiki (riwayat)**: (1) `EnsureLeaveRequestAccessAsync` dulu cuma cek `isOwner || isBackOffice`, gagal untuk admin yang kebetulan juga `HrEmployee` — ditambal dengan fallback role Super Admin/HR Manager/HR Staff, lalu ditambal lagi dengan cek langsung ke `apv_approval_steps` supaya approver sungguhan (manajer departemen biasa) juga bisa akses lampiran sebelum approve/reject dari mobile; (2) `GeneralDocumentUploadViewComponent` dulu tidak ada feedback visual setelah pilih file — sudah ditambah JS untuk update label nama file + toggle class border/bg hijau.

## Acuan Implementasi

- Web: `ERP.Web/Controllers/Document/DocumentReferenceTypeConfigsController.cs`, `ERP.Web/Controllers/HR/HrLeaveRequestsController.cs` (contoh combined-submit)
- Web views: `ERP.Web/Views/Document/DocumentReferenceTypeConfigs/*`, `ERP.Web/Views/Shared/Components/GeneralDocumentUpload/Default.cshtml`, `ERP.Web/wwwroot/css/site.css` (kelas `.sinara-doc-*`)
- Web ViewComponent: `ERP.Web/ViewComponents/GeneralDocumentUploadViewComponent.cs`
- Web API client: `ERP.Web/Services/DocumentApiClient.cs`, `ERP.Web/Services/HrApiClient.cs`
- API: `ERP.API/Controllers/v1/Document/DocumentsController.cs`, `DocumentReferenceTypeConfigsController.cs`, `ERP.API/Controllers/v1/HR/LeaveRequestsController.cs`
- Application services: `ERP.Application/Services/Document/DocumentService.cs`, `IDocumentStorageService.cs` (implementasi konkret di `ERP.API/Services/DocumentStorageService.cs`, butuh `IWebHostEnvironment`)
- Domain: `ERP.Domain/Entities/Document/DocDocument.cs`, `DocReferenceTypeConfig.cs`, `DocReferenceTypeConfigDetail.cs`
- Konfigurasi: `ERP.Application/Options/DocumentSettings.cs`, `ERP.API/appsettings.json` (section `DocumentSettings`)
- Seed: `ERP.Infrastructure/Data/DataSeeder.cs` (`SeedDocumentReferenceTypeConfigsAsync`)
- Mobile: `D:\Flutter\AbsenKu\lib\features\leave\`
