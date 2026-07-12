ReadMe General Document Module SINARA

Dokumen ini menjelaskan modul General Document: infrastruktur penyimpanan dan
lampiran dokumen terpusat yang bisa dipasang ke transaksi apapun di SINARA
ERP, tanpa setiap modul perlu bikin tabel upload dan logic penyimpanan file
sendiri-sendiri. Modul asal cukup mendaftarkan reference_type + reference_id
ke tabel doc_documents.

Rencana lengkap (fase pengerjaan awal, dsb.) ada di
SINARA_ERP_GeneralDocument_Panduan_Detail.docx, tapi dokumen itu ditulis untuk
desain awal berbasis kategori yang sudah tidak dipakai lagi — anggap docx
sebagai arsip sejarah, README ini yang jadi acuan arsitektur final.

Perubahan Arsitektur Penting (dari desain awal)
- Document Categories (tabel doc_document_categories) SUDAH DIHAPUS TOTAL —
  tidak ada lagi konsep kategori dokumen. Diganti dengan tabel
  doc_reference_type_configs yang mendefinisikan aturan validasi per modul
  (reference_type), bukan pengelompokan dokumen.
- Upload dokumen SEKARANG DIGABUNG dengan create/update record induknya dalam
  SATU request multipart ("Opsi B" combined-submit), bukan dua langkah
  terpisah (create dulu baru upload). Ini mencegah file "nyangkut" tanpa induk
  kalau salah satu langkah gagal, dan bikin validasi "wajib upload" bisa
  ditegakkan sebelum record induk disimpan sama sekali.

Struktur Menu General Document

1. Document Settings
   Route: /document/reference-type-configs

(Modul ini tidak punya menu untuk browse semua dokumen lintas modul — upload/
list/download/delete dokumen selalu diakses dari halaman modul asalnya,
misalnya dari Create/Edit/Details Leave Request di HR. Lihat "Kegunaan Tiap
Menu" di bawah untuk detail integrasinya.)

Kegunaan Tiap Menu

1. Document Settings (/document/reference-type-configs)
- Struktur master-detail (direstrukturisasi dari desain flat sebelumnya):
  * doc_reference_type_configs (master) — satu baris per reference_type:
    ReferenceType (kode unik dipakai modul asal, contoh: hr_leave_requests),
    DisplayName (label UI), IsMultiple (single slot vs multi slot),
    MaxFileCount (jumlah slot, cuma berlaku kalau IsMultiple=true — kalau
    false dipaksa 1), IsActive (saklar hidup/mati seluruh reference_type).
  * doc_reference_type_config_details (detail, child dari master via
    config_id, cascade delete) — satu baris per SLOT lampiran: Name (label
    slot, contoh "KTP"/"Slip Gaji"), MaxFileSizeBytes (nullable, fallback ke
    DocumentSettings global), IsRequired (wajib per-slot), IsActive (slot ini
    ditawarkan atau tidak), AllowedExtensions (nullable, fallback ke default
    global .pdf/.jpg/.jpeg/.png/.docx).
  * Jumlah baris detail SELALU harus sama dengan MaxFileCount efektif
    (1 kalau IsMultiple=false, atau nilai MaxFileCount kalau true) —
    ditegakkan di ValidateConfigRequest (DocumentService), bukan cuma di UI.
- Form Create/Edit (_Form.cshtml) generate baris detail secara LIVE via
  JavaScript begitu field Max File Count diketik (nambah/kurangi row tanpa
  reload), pola `<template>` + renumber() yang sama dengan Journal Lines.
  Checkbox Multiple mati → Max File Count jadi readonly (dipaksa ke 1) dan
  tabel detail otomatis diciutkan ke 1 baris.
- CRUD lengkap (Index/Create/Edit/Delete) dengan search, lewat
  DocumentReferenceTypeConfigsController (Web) yang manggil
  DocumentReferenceTypeConfigsController (API) di
  /api/v1/document-reference-type-configs.
- Tidak bisa dihapus kalau reference_type itu sudah punya dokumen terupload
  (dicek lewat tabel doc_documents).
- Seed default (dibuat otomatis oleh DataSeeder, idempotent, TAPI cuma insert
  detail rows saat pertama kali dibuat — update berikutnya tidak menimpa
  detail yang sudah diubah admin lewat UI):
  * hr_leave_requests — "Leave Request", IsMultiple=true, MaxFileCount=3 →
    3 baris detail ("Leave Request 1/2/3"), masing-masing IsRequired=false,
    MaxFileSizeBytes=null, AllowedExtensions=null (semua fallback ke default
    global).
- Kompatibilitas mundur: DocumentReferenceTypeConfigDto (dipakai bersama oleh
  endpoint GET /api/v1/documents/config yang dikonsumsi Leave Requests &
  mobile) tetap punya IsRequired/MaxFileSizeBytes/AllowedExtensions sebagai
  COMPUTED PROPERTY yang diambil dari baris detail pertama (SortOrder
  terkecil) — supaya konsumen lama yang belum di-rework tetap jalan tanpa
  perubahan kode.

2. Upload/List/Download/Delete Dokumen (tidak ada menu sendiri)
- Ini bukan halaman mandiri — selalu diakses dari halaman modul yang
  "menempelkan" dokumen ke recordnya. Integrasi pertama & satu-satunya saat
  ini: HR Leave Requests (lihat ReadMeHr.md bagian "Leave Requests" dan
  "General Document").
- Endpoint generik yang dipakai modul manapun untuk integrasi baru:
  * GET  /api/v1/documents?referenceType=&referenceId= — daftar dokumen
    untuk satu transaksi.
  * GET  /api/v1/documents/config?referenceType= — ambil aturan validasi
    (DocumentReferenceTypeConfigDto) buat reference_type tsb; 404 kalau
    reference_type belum didaftarkan/nonaktif. Dipakai frontend (Web & mobile)
    buat render form upload yang sesuai aturan (required/max size/max count/
    ekstensi) sebelum submit, dan dipakai server buat validasi ulang saat
    submit (client-side hint saja, validasi sebenarnya tetap di server).
  * GET  /api/v1/documents/{id}/download — download (streaming, ber-otorisasi,
    bukan URL statis).
  * DELETE /api/v1/documents/{id} — hapus (hard delete row + file fisik).
- Endpoint POST /api/v1/documents (single-file, standalone) masih ada di
  DocumentsController buat kasus modul yang tidak butuh pola combined-submit,
  tapi integrasi Leave Requests TIDAK memakainya — lihat pola combined-submit
  di bawah.

Pola Combined-Submit ("Opsi B") — Cara Pasang Dokumen ke Modul Baru
Ini pola yang dipakai Leave Requests dan jadi acuan untuk modul berikutnya
yang butuh "field form + lampiran wajib/opsional" dalam satu form:
1. Controller endpoint create/update modul (bukan DocumentsController) yang
   menerima [FromForm] gabungan: field-field record induk (contoh
   LeaveTypeId, StartDate, EndDate, Reason) DITAMBAH `List<IFormFile>? Files`
   dan `string? Note`, dengan [Consumes("multipart/form-data")] dan
   [RequestSizeLimit(...)].
2. Sebelum record induk dibuat: validasi dulu required/max-count terhadap
   config reference_type-nya (ValidateAttachmentRequirementAsync di
   LeaveRequestsController jadi contoh pola ini) — supaya record TIDAK
   pernah tersimpan kalau lampiran wajib tidak ada.
3. Setelah record induk berhasil dibuat/disimpan (dan reference_id-nya sudah
   ada), baru loop upload tiap file lewat DocumentService.UploadAsync —
   validasi per-file (ekstensi, ukuran, dan corruption check) dilakukan di
   sini.
4. Kegagalan per-file (misal satu dari tiga file korup) TIDAK membatalkan
   keseluruhan request — record induk yang sudah tersimpan tetap dianggap
   sukses, file yang gagal cuma dilaporkan lewat AttachmentWarnings di
   response (lihat SubmitLeaveRequestResult di LeaveDto.cs sebagai contoh
   DTO-nya: { LeaveRequest, AttachmentWarnings[] }). Klien (Web/mobile) wajib
   menampilkan warning ini ke user, bukan diam-diam diabaikan.
5. Tambahkan reference_type baru (nama tabel snake_case, contoh:
   fa_asset_transfers) ke whitelist AllowedReferenceTypes di
   DocumentService.cs (server-side, tidak boleh terima string bebas dari
   client demi keamanan).
6. Tambahkan rule otorisasi untuk reference_type itu di
   EnsureAuthorizationAsync (siapa yang boleh lihat/upload/hapus dokumen
   untuk record tsb).
7. Insert 1 baris di doc_reference_type_configs (lewat menu Document
   Settings atau seed) buat set aturan validasinya.
8. Tidak perlu bikin tabel/migration baru untuk dokumennya sendiri —
   doc_documents dipakai bersama oleh semua modul.

Kenapa Combined-Submit, Bukan Staged-Upload?
Desain awal yang sempat dipertimbangkan adalah "staged upload": upload file
duluan ke server sebelum record induk ada (pakai reference_id sementara/
null), baru dikaitkan ke record induk setelah record-nya tersimpan. Ini
ditolak karena butuh mekanisme cleanup buat file "yatim" (kalau user upload
lalu batal submit form) dan menambah state sementara yang harus dikelola.
Pola combined-submit (satu request multipart berisi field + file sekaligus)
lebih sederhana: tidak ada file yang tersimpan sebelum record induknya pasti
tersimpan, jadi tidak ada orphan file yang perlu dibersihkan.

Validasi File Saat Upload (DocumentService.ValidateFileAsync)
- Ekstensi & ukuran: **sementara** dicek terhadap baris detail PERTAMA
  (SortOrder terkecil) config reference_type-nya saja (atau fallback ke
  DocumentSettings global kalau field itu null di baris tsb) — bukan
  per-slot yang sebenarnya di-submit, karena endpoint upload backend
  (DocumentService.UploadAsync, dipanggil dari LeaveRequestsController)
  belum tahu file yang masuk itu untuk slot yang mana (DocDocument belum
  punya kolom yang menunjuk ke detail/slot spesifik). Ini keputusan sadar
  ("dibiarkan dulu" per arahan user) sambil integrasi Web/mobile per-slot
  dikerjakan bertahap — lihat "Integrasi Web" di bawah untuk status
  per-modul.
- Jumlah file: ditolak kalau jumlah dokumen existing + yang baru diupload
  melebihi MaxFileCount config (di level master, bukan per-slot).
- Integrity/corruption check (ValidateFileIntegrityAsync), di luar sekadar
  cek ekstensi:
  * Gambar (.jpg/.jpeg/.png) — divalidasi lewat
    SixLabors.ImageSharp.Image.DetectFormatAsync (format file harus valid,
    bukan cuma ekstensinya yang cocok).
  * PDF (.pdf) — dicek 5 byte pertama harus "%PDF-" (magic header).
  * DOCX (.docx) — dibuka sebagai System.IO.Compression.ZipArchive dan dicek
    ada entry wajib [Content_Types].xml (DOCX adalah ZIP terstruktur).
  * Kalau stream tidak bisa di-seek, validasi integrity di-skip (bukan gagal)
    karena tidak bisa baca ulang dari awal.

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
- Default global (appsettings DocumentSettings, ERP.API) — dipakai kalau
  field terkait di doc_reference_type_configs null:
  * MaxFileSizeBytes: 5 MB
  * AllowedExtensions: .pdf, .jpg, .jpeg, .png, .docx
  * StorageDirectory: App_Data/uploads/documents

Integrasi Web (ERP.Web)
- Dua ViewComponent reusable di Views/Shared/Components/:
  * GeneralDocumentUpload — SEKARANG slot-aware (mengikuti restrukturisasi
    master-detail): menerima `Slots` (satu entry per baris detail aktif),
    lalu render SATU file input per slot — masing-masing dengan label sesuai
    Name slot-nya, tanda "wajib" (*) kalau slot itu IsRequired (dan belum ada
    dokumen existing di record-nya), hint ukuran maks & ekstensi izinnya
    sendiri-sendiri. Semua input WAJIB pakai `name` yang PERSIS SAMA
    (`AttachmentFiles`, semuanya, tanpa index) — sempat salah ditulis pakai
    nama terindeks (`AttachmentFiles[0]`, `[1]`, dst.) karena dikira ikut
    konvensi collection binding ASP.NET Core biasa, padahal
    `FormFileModelBinder` untuk `List<IFormFile>` itu SPESIAL: dia manggil
    `Request.Form.Files.GetFiles(namaPersisIni)` langsung, BUKAN lewat
    index-discovery seperti collection binder pada umumnya — jadi kalau
    nama field-nya beda-beda (`[0]`, `[1]`, ...), semuanya gagal ke-bind dan
    file yang dipilih user hilang tanpa error apapun (ini bug yang sempat
    kejadian & sudah diperbaiki). Validasi "wajib" per-slot berjalan di CLIENT
    (jQuery Unobtrusive Validation, `data-val-required` per input) — bukan di
    server, karena endpoint upload backend belum menyimpan slot mana yang
    dipakai tiap file (lihat catatan di "Validasi File Saat Upload" di atas).
    Field Note tetap satu untuk seluruh submission (belum per-slot).
  * GeneralDocumentList — widget daftar dokumen yang sudah terupload (nama
    file, ukuran, siapa & kapan upload) + tombol Download opsional + tombol
    Delete opsional (per-dokumen, form kecil terpisah). Dirender DI LUAR form
    utama karena HTML tidak boleh nested <form>. TIDAK diubah oleh
    restrukturisasi master-detail — DocDocument belum punya kolom slot,
    jadi daftar dokumen masih flat per reference_id, bukan dikelompokkan
    per-slot.
- Halaman Leave Request:
  * Create & Edit: GeneralDocumentUpload di dalam form utama (upload lampiran
    baru ikut ter-submit bareng field leave request), GeneralDocumentList di
    bawahnya dengan Delete aktif (Edit only — Create belum ada dokumen
    existing).
  * Details: cuma GeneralDocumentList tanpa Delete — halaman ini view-only,
    edit lampiran harus lewat halaman Edit.
- **Belum dikerjakan** (langkah lanjutan setelah Web): DocDocument belum
  dikaitkan ke baris detail spesifik, jadi daftar dokumen existing
  (GeneralDocumentList) tidak bisa menunjukkan "file ini untuk slot yang
  mana", dan enforcement required/size/extension di SERVER masih pakai proxy
  baris detail pertama (bukan per-slot yang sesungguhnya disubmit). Kalau mau
  ditutup penuh, perlu tambah kolom penunjuk slot di doc_documents plus ubah
  DocumentService.UploadAsync/ValidateFileAsync supaya slot-aware.

Integrasi Mobile (AbsenKu, Flutter) — BELUM di-rework ke master-detail
Bagian ini masih mendeskripsikan integrasi versi LAMA (flat, sebelum
doc_reference_type_configs dipecah jadi master+detail). Mobile masih jalan
karena DocumentReferenceTypeConfigDto tetap expose IsRequired/
MaxFileSizeBytes/MaxFileCount/AllowedExtensions secara flat (proxy dari baris
detail pertama, lihat "Kompatibilitas mundur" di atas), tapi belum
menampilkan slot-slot terpisah seperti Web. Rework mobile jadi langkah
selanjutnya setelah Web selesai diverifikasi.
- Form "Ajukan Cuti / Sakit" (leave_request_screen.dart) mengambil config
  validasi reference_type hr_leave_requests dari server saat layar dibuka
  (getAttachmentConfig), lalu render UI sesuai aturannya secara dinamis:
  label "wajib"/"opsional", multi-file picker kalau MaxFileCount > 1 (dengan
  slot "+ Add File" sampai batas tercapai), validasi ukuran/ekstensi dari
  config (fallback ke default kalau config null/belum ada).
- Submit pakai SATU request multipart (leave_repository.dart method submit())
  berisi field leave request + note + daftar file sekaligus ke
  POST /hr/leave-requests/self — sama persis dengan pola combined-submit di
  atas, bukan dua langkah terpisah lagi.
- Response submit (SubmitLeaveRequestResult) bisa membawa attachmentWarnings
  kalau ada file yang gagal diupload meski leave request-nya sendiri sukses
  tersimpan — ditampilkan lewat dialog peringatan sebelum layar ditutup,
  bukan disembunyikan.
- Layar Riwayat Cuti punya ikon lampiran per pengajuan (bottom sheet,
  lazy-loaded saat diklik, tampilkan nama file + ukuran) — tidak berubah dari
  desain sebelumnya, cuma field categoryName yang sudah tidak ada lagi di
  model LeaveDocument (karena kategori sudah dihapus).
- Acuan: D:\Flutter\AbsenKu\lib\features\leave\ (models/leave_models.dart —
  class LeaveDocument, SubmitLeaveRequestResult, DocumentReferenceTypeConfig;
  data/leave_repository.dart — getAttachmentConfig & submit gabungan;
  presentation/leave_request_screen.dart; presentation/leave_history_screen.dart).

Catatan Permission
- Semua controller Document memakai [Authorize] umum (belum granular per
  menu seperti [RequireMenuPermission] yang cuma dipakai HR Departments).
- Module "General Document" (kode DOC) & menu "Document Settings" otomatis
  dapat izin penuh (view/create/edit/delete) untuk role Super Admin lewat
  SeedSuperAdminPermissionsAsync, sama seperti seluruh menu lain. Role selain
  Super Admin belum di-seed permission spesifik untuk modul ini.
- Otorisasi akses dokumen per-record (misalnya "cuma pemilik leave request
  atau HR staff yang boleh lihat/upload/hapus lampirannya") dicek di service
  layer (DocumentService.EnsureAuthorizationAsync -> EnsureLeaveRequestAccessAsync
  untuk hr_leave_requests), bukan lewat [RequireMenuPermission] — karena
  aturannya spesifik per reference_type, bukan sekadar per-menu.

Catatan Gap Implementasi (untuk backlog)
- Belum ada halaman admin untuk melihat/mencari semua dokumen lintas modul
  (cuma bisa dilihat dari halaman modul asalnya).
- Baru satu reference_type yang terdaftar (hr_leave_requests). Rencana
  lanjutan: Fixed Assets (transfer/disposal), Purchasing (PO), General
  Approval.
- Tidak ada preview file di browser/app (PDF/gambar) — download langsung ke
  device, belum ada inline viewer.
- Tidak ada antivirus/malware scanning untuk file yang diupload — validasi
  cuma dari ekstensi, ukuran, dan format/magic-header, bukan pemindaian
  malware sesungguhnya.
- EnsureLeaveRequestAccessAsync cuma cek `isOwner || isBackOffice` (user
  tanpa profil karyawan dianggap back-office/HR admin). Ini berarti seorang
  HR admin yang KEBETULAN juga punya profil HrEmployee sendiri bisa salah
  ditolak (403) saat mengelola dokumen leave request milik karyawan LAIN
  lewat panel admin, karena dia bukan owner record tsb dan bukan
  "tanpa-profil". Belum diperbaiki di iterasi ini (diterima sebagai
  trade-off scope), perlu rule tambahan (misal cek role/permission admin
  eksplisit) di iterasi berikutnya.
- Migration 20260712180000_AddGeneralDocument ditulis manual (bukan hasil
  dotnet ef migrations add) karena tooling migration scaffolding di project
  ini crash untuk SEMUA migration baru — root cause pra-existing
  (AppDbContextModelSnapshot.cs sudah lama tidak sinkron, jauh sebelum modul
  ini dibuat), lihat detail penjelasannya di ReadMeHr.md bagian "General
  Document".

Acuan Implementasi
- Web controller:
  ERP.Web/Controllers/Document/DocumentReferenceTypeConfigsController.cs
  (CRUD aturan validasi admin), ERP.Web/Controllers/HR/HrLeaveRequestsController.cs
  (integrasi combined-submit + list/download/delete dokumen di Leave
  Requests).
- Web views:
  ERP.Web/Views/Document/DocumentReferenceTypeConfigs/*,
  ERP.Web/Views/Shared/Components/GeneralDocumentUpload/Default.cshtml,
  ERP.Web/Views/Shared/Components/GeneralDocumentList/Default.cshtml
- Web ViewComponents:
  ERP.Web/ViewComponents/GeneralDocumentUploadViewComponent.cs,
  ERP.Web/ViewComponents/GeneralDocumentListViewComponent.cs
- Web API client:
  ERP.Web/Services/DocumentApiClient.cs (CRUD config admin),
  ERP.Web/Services/HrApiClient.cs (combined-submit + list/download/delete
  dokumen, dipanggil dari halaman Leave Requests).
- API controller:
  ERP.API/Controllers/v1/Document/DocumentsController.cs,
  ERP.API/Controllers/v1/Document/DocumentReferenceTypeConfigsController.cs,
  ERP.API/Controllers/v1/HR/LeaveRequestsController.cs (contoh combined-submit)
- Application services:
  ERP.Application/Services/Document/DocumentService.cs,
  ERP.Application/Services/Document/IDocumentStorageService.cs
  (implementasi konkretnya di ERP.API/Services/DocumentStorageService.cs,
  karena butuh IWebHostEnvironment dari project hosting).
- Domain entities:
  ERP.Domain/Entities/Document/DocDocument.cs,
  ERP.Domain/Entities/Document/DocReferenceTypeConfig.cs (master),
  ERP.Domain/Entities/Document/DocReferenceTypeConfigDetail.cs (detail/slot)
- Konfigurasi:
  ERP.Application/Options/DocumentSettings.cs,
  ERP.API/appsettings.json (section DocumentSettings)
- Seed menu & config default:
  ERP.Infrastructure/Data/DataSeeder.cs
  (SeedDocumentReferenceTypeConfigsAsync, EnsureMenuAsync untuk
  "Document Settings")
- Mobile (Flutter, repo terpisah):
  D:\Flutter\AbsenKu\lib\features\leave\
