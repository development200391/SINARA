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
- Master aturan validasi dokumen per modul (per reference_type). Satu baris =
  satu reference_type, isinya:
  * ReferenceType — kode unik yang dipakai modul asal (contoh:
    hr_leave_requests), harus persis sama dengan string yang dikirim modul
    saat panggil endpoint dokumen.
  * DisplayName — label yang ditampilkan di UI (contoh: "Leave Request").
  * IsRequired — apakah minimal 1 file wajib diupload sebelum record induk
    boleh disimpan.
  * MaxFileSizeBytes — batas ukuran per file (nullable — null berarti pakai
    default global DocumentSettings.MaxFileSizeBytes, saat ini 5 MB).
  * MaxFileCount — jumlah maksimal file per upload. 1 = single file,
    lebih dari 1 = multi-file (UI otomatis kasih multi-picker & multi-slot).
  * AllowedExtensions — daftar ekstensi diizinkan, comma-separated (nullable
    — null berarti pakai default global DocumentSettings.AllowedExtensions:
    .pdf, .jpg, .jpeg, .png, .docx).
  * IsActive — kalau nonaktif, upload untuk reference_type itu ditolak.
- CRUD lengkap (Index/Create/Edit/Delete) dengan search, lewat
  DocumentReferenceTypeConfigsController (Web) yang manggil
  DocumentReferenceTypeConfigsController (API) di
  /api/v1/document-reference-type-configs.
- Tidak bisa dihapus kalau reference_type itu sudah punya dokumen terupload
  (dicek lewat tabel doc_documents).
- Seed default (dibuat otomatis oleh DataSeeder, idempotent):
  * hr_leave_requests — "Leave Request", IsRequired=false, MaxFileSizeBytes=
    null (pakai default 5 MB), MaxFileCount=3 (multi-file), AllowedExtensions
    =null (pakai default global).

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
- Ekstensi & ukuran: dicek terhadap config reference_type (atau fallback ke
  DocumentSettings global kalau config-nya null di field tsb).
- Jumlah file: ditolak kalau jumlah dokumen existing + yang baru diupload
  melebihi MaxFileCount config.
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
  * GeneralDocumentUpload — widget upload (file input + note), dipasang DI
    DALAM form utama Create/Edit modul (karena field-nya ikut ter-submit
    bareng field form induk). Tampil kalau config untuk reference_type-nya
    ada & aktif. Otomatis single/multi-file sesuai MaxFileCount, dan kasih
    tanda "wajib" kalau IsRequired.
  * GeneralDocumentList — widget daftar dokumen yang sudah terupload (nama
    file, ukuran, siapa & kapan upload) + tombol Download opsional + tombol
    Delete opsional (per-dokumen, form kecil terpisah). Dirender DI LUAR form
    utama karena HTML tidak boleh nested <form>.
- Halaman Leave Request:
  * Create & Edit: GeneralDocumentUpload di dalam form utama (upload lampiran
    baru ikut ter-submit bareng field leave request), GeneralDocumentList di
    bawahnya dengan Delete aktif (Edit only — Create belum ada dokumen
    existing).
  * Details: cuma GeneralDocumentList tanpa Delete — halaman ini view-only,
    edit lampiran harus lewat halaman Edit.

Integrasi Mobile (AbsenKu, Flutter)
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
  ERP.Domain/Entities/Document/DocReferenceTypeConfig.cs
- Konfigurasi:
  ERP.Application/Options/DocumentSettings.cs,
  ERP.API/appsettings.json (section DocumentSettings)
- Seed menu & config default:
  ERP.Infrastructure/Data/DataSeeder.cs
  (SeedDocumentReferenceTypeConfigsAsync, EnsureMenuAsync untuk
  "Document Settings")
- Mobile (Flutter, repo terpisah):
  D:\Flutter\AbsenKu\lib\features\leave\
