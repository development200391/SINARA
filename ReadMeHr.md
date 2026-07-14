ReadMe HR Module SINARA

Dokumen ini menjelaskan kegunaan setiap menu Human Resources (HR) yang sudah dibuat di aplikasi ERP.WEB.

Struktur Menu Human Resources (HR)

1. Employees
   1.1 All Employees
       Route: /hr/employees
   1.2 Add Employee
       Route: /hr/employees/create
   1.3 Departments
       Route: /hr/departments
   1.4 Positions
       Route: /hr/positions

2. Attendance
   2.1 Daily Attendance
       Route: /hr/attendance
   2.2 Holiday Master
       Route: /hr/attendance/holiday
   2.3 Attendance Report
       Route: /hr/attendance/report
   2.4 Attendance Setting
       Route: /hr/attendance/setting

3. Payroll
   3.1 Payroll Run
       Route: /hr/payroll
   3.2 Salary Setup
       Route: /hr/payroll/setup (belum diimplementasi)
   3.3 Payslips
       Route: /hr/payroll/payslips (belum diimplementasi, yang ada baru route detail per karyawan)

4. Leave (Cuti)
   4.1 Leave Requests
       Route: /hr/leave/requests
   4.2 Leave Balance
       Route: /hr/leave/balance
   4.3 Leave Types
       Route: /hr/leave/types

5. Reports
   5.1 Headcount Report
       Route: /hr/reports/headcount (belum diimplementasi)
   5.2 Turnover Report
       Route: /hr/reports/turnover (belum diimplementasi)

Kegunaan Tiap Menu

1. All Employees (/hr/employees)
- Daftar seluruh karyawan dengan pencarian dan filter: kode, nama, email, telepon,
  departemen, jabatan, status kepegawaian, rentang tanggal hire/termination.
- Mendukung sorting multi-kolom dan paginasi.

1.1 Add Employee (/hr/employees/create)
- Form tambah karyawan baru: kode, nama, email, telepon, foto (dengan crop),
  departemen, jabatan, tanggal hire, status kerja.
- Validasi: posisi harus sesuai departemen yang dipilih, tanggal termination
  harus >= tanggal hire.
- Ada juga halaman Details/Edit/Delete per karyawan (tidak tampil di menu, diakses dari grid).

1.2 Departments (/hr/departments)
- Master departemen: code, name, manager (dari daftar karyawan), parent department
  (struktur hierarkis antar departemen, tidak boleh menjadi parent untuk dirinya sendiri).
- CRUD lengkap dengan filter code/name/manager/parent/status aktif.
- Satu-satunya controller HR yang sudah menerapkan permission granular
  (View/Create/Edit/Delete) per menu.
- Form Create/Edit/Details memakai satu partial form (_Form.cshtml) yang sama
  (pola sama seperti Positions di 1.3); halaman Details menampilkan form yang
  sama dalam mode disabled (read-only), tapi kalau manager/parent department-
  nya sudah tidak aktif tetap ditampilkan apa adanya (tidak di-null-kan
  seperti di Create/Edit, lewat helper terpisah `PopulateReadOnlyOptionsAsync`
  di `HrDepartmentsController.cs`).
- Field Manager pakai `SearchableSelectViewComponent` (dropdown dengan
  pencarian), sama seperti field Position/UserId di form Employee.
- **Manager departemen sekarang load-bearing untuk approval** — General
  Approval (lihat poin 4 di bawah & `ReadMeGeneralApproval.md`) resolve
  approver cuti karyawan lewat `HrDepartment.ManagerId`. Departemen aktif
  tanpa manager akan bikin pengajuan cuti staf-nya GAGAL submit, bukan cuma
  gagal ditampilkan di UI — pastikan tiap departemen aktif punya manager
  ter-set.

1.3 Positions / Jabatan (/hr/positions)
- Master jabatan: name, code, departemen, level jabatan, status aktif.
- Level jabatan dipakai sebagai basis perhitungan gaji pokok di modul Payroll.
- CRUD lengkap dengan filter code/name/departemen/level/status aktif.
- Form Create/Edit/Details memakai satu partial form (_Form.cshtml) yang sama;
  halaman Details menampilkan form yang sama dalam mode disabled (read-only).
- Checkbox "Aktif" (IsActive) pakai komponen FormCheckboxViewComponent.

2. Daily Attendance (/hr/attendance)
- Rekam absensi harian per karyawan per tanggal: check-in, check-out, status
  (Present/Late/Absent/HalfDay, dll), catatan.
- CRUD lengkap dengan filter karyawan, departemen, rentang tanggal, status.
- Status **Sick** dan **Cuti** tidak lagi bisa dipilih manual dari form
  Create/Edit di sini — dua status itu sekarang murni dikelola otomatis lewat
  approval Leave Requests (poin 4). Kalau sebuah record hasil sync leave
  dibuka untuk diedit, field Status ditampilkan read-only dengan keterangan
  bahwa statusnya dikelola lewat leave request; field lain (mis. Notes) tetap
  bisa diedit seperti biasa. Percobaan set Status ke Sick/Cuti lewat API
  (Create atau ganti ke status baru saat Update) ditolak dengan error
  "Sick and Cuti status can only be set through an approved leave request."

2.1 Holiday Master (/hr/attendance/holiday)
- Master hari libur: nama, tanggal, tipe libur (contoh: National), deskripsi,
  cakupan berlaku (AppliesTo, default "all"), status aktif.
- Bisa difilter per tahun.

2.2 Attendance Report (/hr/attendance/report)
- Laporan rekap kehadiran per periode (mengikuti setting periode payroll),
  dikelompokkan per karyawan.
- Menghitung total Present/Late/Absent/Cuti, status dominan, dan persentase kehadiran.
- Bisa difilter per departemen/karyawan/status dan disortir.

2.3 Attendance Setting (/hr/attendance/setting)
- Pengaturan global jam kerja: jam masuk/pulang, jam istirahat (pakai komponen
  FormTimeViewComponent), toleransi keterlambatan (menit), minimum menit
  lembur (overtime).
- Pengaturan periode absensi/payroll: hanya "Start Day" yang diisi manual;
  "End Day" dihitung otomatis (Start Day - 1, dan dibungkus ke 31 kalau
  Start Day = 1), baik saat load maupun saat submit di server.
- Dipakai sebagai basis perhitungan status "Late" pada laporan absensi dan
  periode perhitungan payroll.
- Lokasi Kantor (untuk absen via HP): Latitude/Longitude/Radius sekarang
  dipilih lewat peta interaktif (Leaflet + OpenStreetMap, tanpa API key),
  dengan fitur klik-pada-peta, drag marker, search alamat (geocoding via
  Nominatim), dan tombol "Lokasi Saya Sekarang" (geolocation browser).
  Latitude/Longitude tetap bisa diisi manual dan otomatis sinkron ke peta,
  dengan validasi rentang (-90..90 / -180..180) dan indikator error kalau
  formatnya salah.

3. Payroll Run (/hr/payroll)
- Index: daftar histori payroll run per bulan/tahun beserta status
  (Draft/Processing/Completed), total karyawan, dan total net salary.
- Run: form untuk memproses payroll bulan/tahun terpilih, redirect ke halaman
  Details setelah selesai.
- Ada juga halaman Details (rincian payroll per karyawan dalam satu run) dan
  Payslip per karyawan (tidak tampil di menu, diakses dari grid).
- Logika perhitungan:
  * Basic Salary = 3.000.000 + (level jabatan x 1.000.000)
  * Allowances (tunjangan) = 20% dari Basic Salary
  * Deductions (potongan) = (jumlah hari Absent x 50.000) + (jumlah hari Late x 20.000)
  * Gross Salary = Basic Salary + Allowances
  * Tax = 5% dari Gross Salary (flat)
  * Net Salary = Gross Salary - Deductions - Tax (minimum 0)
  * Hanya memproses karyawan dengan status kepegawaian Active.
  * Payroll run bersifat idempotent per bulan/tahun (dijalankan ulang akan
    meng-update, bukan duplikat).
- Payroll yang sudah diposting terhubung ke modul Finance lewat menu
  "Payroll Journals" (Finance > Journals?source=Payroll) untuk melihat jurnal
  akuntansi otomatis hasil payroll.

3.1 Salary Setup (/hr/payroll/setup)
- Menu sudah terdaftar di navigasi tapi belum ada implementasi controller/view.
- Rencana: pengaturan komponen gaji (belum dikembangkan).

3.2 Payslips (/hr/payroll/payslips)
- Menu sudah terdaftar di navigasi tapi belum ada implementasi controller/view
  yang sesuai; yang tersedia hanya slip gaji individual dari halaman Details
  payroll run. Perlu klarifikasi/perbaikan link menu.

4. Leave Requests (/hr/leave/requests)
- Pengajuan cuti karyawan: karyawan, jenis cuti, tanggal mulai/selesai, alasan.
- Hanya request berstatus Pending yang bisa diedit.
- **Approve/Reject sekarang lewat mesin General Approval** (lihat
  `ReadMeGeneralApproval.md` bagian "Integrasi Modul HR Leave Request" untuk
  detail lengkap) — bukan flip status langsung lagi:
  * Submit otomatis bikin `ApprovalRequest` (template `HR_LEAVE`,
    1 level: `DirectSuperior`) — approver-nya HANYA manager departemen
    karyawan yang bersangkutan (`HrDepartment.ManagerId`, lihat poin 1.2),
    bukan siapapun dengan akses menu ini seperti dulu.
  * Karyawan pemohon HARUS punya akun user (`HrEmployee.UserId`) ter-link,
    kalau tidak submit-nya GAGAL dengan pesan jelas — ini pembatasan baru.
  * Tombol Approve/Reject di grid (`Index.cshtml`) & halaman Details
    sekarang cuma muncul kalau user yang login benar-benar approver aktif
    utk request itu (`LeaveRequestDto.CanApprove`) — bukan lagi selalu
    tampil untuk semua Pending request.
  * Approver juga bisa bertindak lewat Approval Inbox (`/approval/inbox`),
    tidak harus dari halaman Leave Requests ini.
  * Leave request Pending yang dibuat SEBELUM integrasi ini di-backfill
    otomatis saat startup API (`DataSeeder.BackfillLeaveRequestApprovalsAsync`)
    supaya ikut aturan baru juga, bukan tetap longgar selamanya.
- Submit (baik dari web maupun self-service) sekarang **menolak** kalau total hari
  (Approved + Pending yang sudah ada + hari yang baru diajukan) melebihi
  MaxDaysPerYear jenis cuti tersebut, untuk employee/leaveType/tahun yang sama.
- Saat di-**Approve**, sistem otomatis membuat/mengupdate HrAttendanceRecord
  untuk setiap tanggal dalam rentang StartDate..EndDate dengan Status = Cuti,
  kecuali kalau tanggal itu sudah punya CheckIn/CheckOut (data absensi asli
  tidak akan ditimpa). Ini membuat Attendance Report & Payroll otomatis
  konsisten dengan cuti yang sudah disetujui, tanpa langkah manual tambahan.
  Logika ini sekarang di `LeaveAttendanceSyncHelper` (dipakai bersama oleh
  jalur approval baru maupun fallback lama, lihat `ReadMeGeneralApproval.md`).
- Reject/Delete hanya bisa dilakukan selama status masih Pending — begitu
  Approved, tidak ada jalur di API untuk membatalkannya (kalau perlu revisi,
  request baru yang perlu diajukan).
- **Self-service (mobile)**: ada endpoint terpisah khusus dipakai app AbsenKu
  (karyawan mengajukan & melihat cuti sendiri), employeeId di-resolve dari
  token login, tidak dipercaya dari body request:
  * `GET /api/v1/hr/leave-requests/self` — riwayat pengajuan milik sendiri.
  * `POST /api/v1/hr/leave-requests/self` — submit pengajuan baru
    (leaveTypeId, startDate, endDate, reason).
  * `GET /api/v1/hr/leave-requests/self/leave-types` — daftar jenis cuti aktif
    saja (tanpa daftar karyawan, beda dari endpoint /options yang dipakai
    admin web, supaya mobile app tidak perlu akses direktori staff).
- Acuan: ERP.API/Controllers/v1/HR/LeaveRequestsController.cs,
  ERP.Application/Services/HR/LeaveService.cs,
  ERP.Application/Services/HR/LeaveRequestApprovalCallbackService.cs,
  ERP.Application/Services/HR/LeaveAttendanceSyncHelper.cs.
- **Lampiran (evidence)**: Leave Request sekarang bisa dilampiri file (mis. surat
  dokter) lewat modul General Document — lihat bagian tersendiri di bawah.
  Upload lampiran digabung dalam SATU request bareng field leave request-nya
  sendiri (create/update, "combined-submit"), bukan langkah terpisah; Delete
  lampiran cuma bisa dilakukan selama status masih Pending. Halaman
  Create/Edit (ERP.Web) menampilkan form upload, halaman Details cuma
  menampilkan daftar lampiran (view-only, tanpa upload/delete). Widget
  upload-nya (`GeneralDocumentUploadViewComponent`) sekarang update label
  jadi nama file begitu dipilih (dulu tidak ada feedback visual sama
  sekali setelah klik "Choose File", kelihatan seperti tidak ngapa-ngapain
  padahal file-nya sudah terpasang) — lihat `ReadMeDocumentGeneral.md`.
- **Akses lampiran (bug fix)**: sebelumnya user yang KEBETULAN punya profil
  `HrEmployee` sendiri (mis. akun admin yang juga terdaftar sebagai
  manager departemen) bisa 500 error saat coba edit lampiran leave request
  milik karyawan LAIN. Fix di `DocumentService.EnsureLeaveRequestAccessAsync`:
  role Super Admin/HR Manager/HR Staff sekarang selalu boleh akses lampiran
  leave request siapapun. Detail di `ReadMeDocumentGeneral.md`.

4.1 Leave Balance (/hr/leave/balance)
- Laporan saldo cuti per karyawan per jenis cuti per tahun.
- Menampilkan MaxDays (kuota), UsedDays (dihitung dari cuti yang sudah disetujui),
  dan RemainingDays (sisa, minimum 0).
- Bisa difilter per tahun/karyawan/jenis cuti dan disortir.

4.2 Leave Types (/hr/leave/types)
- Master jenis cuti: nama, kode, MaxDaysPerYear, IsCarryOver (bisa dibawa ke
  tahun berikutnya atau tidak), status aktif.
- Data default: Cuti Tahunan (ANNUAL, 12 hari, carry-over), Cuti Sakit
  (SICK, 12 hari, tidak carry-over), Cuti Tanpa Bayar (UNPAID, 30 hari).

5. Headcount Report (/hr/reports/headcount)
- Menu sudah terdaftar di navigasi tapi belum ada implementasi controller/view.

5.1 Turnover Report (/hr/reports/turnover)
- Menu sudah terdaftar di navigasi tapi belum ada implementasi controller/view.

6. Self-Attendance (Mobile API only — tidak ada menu web)
- Bukan menu ERP.Web, tapi endpoint API terpisah yang dipakai aplikasi mobile
  karyawan (AbsenKu, Flutter) untuk absen sendiri lewat HP.
  Route: /api/v1/hr/attendance/self/* (GetToday, GetHistory, CheckIn,
  CheckOut, Mark).
- CheckIn/CheckOut: karyawan kirim koordinat GPS, server yang menghitung
  jarak ke titik kantor (rumus Haversine, lihat Attendance Setting di atas)
  dan menolak kalau di luar radius. Jam dicatat pakai DateTimeOffset.UtcNow
  di server, bukan jam yang dikirim dari HP.
- CheckOut sekarang **boleh disubmit berkali-kali** dalam satu hari — submit
  ulang akan menimpa CheckOut/CheckOutLatitude/CheckOutLongitude dengan yang
  terbaru (perubahan dari sebelumnya yang menolak dengan pesan "You have
  already checked out today."). Ini disengaja supaya karyawan yang salah
  check-out kecepetan bisa merevisi sendiri tanpa lewat admin.
- Mark (self-report untuk satu tanggal, dengan catatan opsional): langsung
  tersimpan ke HrAttendanceRecord tanpa approval — **sekarang dibatasi hanya
  untuk status Absent dan HalfDay** (dulu juga menerima Sick/Cuti, tapi itu
  memungkinkan karyawan menandai "Cuti" tanpa approval/kuota sama sekali).
  Sick dan Cuti sekarang wajib lewat alur Leave Requests self-service (poin 4
  di atas: `POST /api/v1/hr/leave-requests/self`), yang mendukung rentang
  tanggal, approval, dan validasi kuota. Percobaan Mark dengan Status
  Sick/Cuti ditolak dengan error "Status is not self-reportable."
- Endpoint tambahan `GET /api/v1/diagnostics/server-time` (di
  DiagnosticsController, bukan khusus HR) dipakai mobile app untuk
  menampilkan jam berjalan yang tersinkron ke waktu server.
- Acuan: ERP.API/Controllers/v1/HR/SelfAttendanceController.cs,
  ERP.Application/Services/HR/AttendanceService.cs,
  ERP.API/Controllers/v1/DiagnosticsController.cs.

7. General Document (modul lintas HR, bukan menu HR tersendiri)
- Infrastruktur lampiran file terpusat — dokumentasi lengkap & terbaru ada di
  ReadMeDocumentGeneral.md (docx Panduan Detail sudah jadi arsip desain awal
  yang tidak dipakai lagi, jangan dijadikan acuan arsitektur). Dipasang
  pertama kali di Leave Requests untuk lampiran evidence (mis. surat dokter).
  Modul lain bisa pakai tabel/endpoint yang sama nanti tinggal ditambah ke
  whitelist reference_type + didaftarkan satu baris di doc_reference_type_configs.
- Tidak ada lagi konsep "kategori dokumen" — diganti tabel
  doc_reference_type_configs yang isinya aturan validasi per modul
  (IsRequired, MaxFileSizeBytes, MaxFileCount, AllowedExtensions), bukan
  pengelompokan dokumen. Dikelola lewat menu Document Settings
  (/document/reference-type-configs).
- Endpoint: `GET/POST /api/v1/documents` (list per reference / upload single
  file standalone), `GET /api/v1/documents/config?referenceType=` (ambil
  aturan validasi), `GET /api/v1/documents/{id}/download`,
  `DELETE /api/v1/documents/{id}`. Untuk Leave Requests, upload TIDAK lewat
  POST /api/v1/documents — lampiran ikut digabung dalam
  POST/PUT /api/v1/hr/leave-requests/self (dan varian admin-nya) sebagai
  bagian dari satu request multipart yang sama dengan field leave request-nya
  ("combined-submit", lihat ReadMeDocumentGeneral.md untuk penjelasan
  lengkap kenapa pola ini dipilih ketimbang staged-upload).
- File fisik disimpan di ERP.API/App_Data/uploads/documents/{referenceType}/{referenceId}/
  (bukan wwwroot — tidak bisa diakses langsung lewat URL statis, cuma lewat
  endpoint download yang ber-otorisasi). Nama file di disk selalu GUID, nama
  asli cuma disimpan sebagai metadata. Selain cek ekstensi/ukuran, ada juga
  pengecekan format/corruption (magic header PDF, DetectFormat gambar, buka
  DOCX sebagai ZIP) — lihat ReadMeDocumentGeneral.md.
- Batas default (fallback kalau config reference_type-nya null di field
  tsb): 5 MB, ekstensi .pdf/.jpg/.jpeg/.png/.docx (appsettings
  DocumentSettings). Cuma reference_type yang di-whitelist server yang bisa
  dipakai — sekarang baru `hr_leave_requests` (IsRequired=false,
  MaxFileCount=3/multi-file).
- Web (ERP.Web): terpasang di halaman Create & Edit Leave Request (form
  upload + note jadi satu dengan form leave request, plus daftar lampiran
  existing dengan tombol Delete di Edit). Halaman Details cuma menampilkan
  daftar lampiran (view-only).
- Mobile (AbsenKu, Flutter): form "Ajukan Cuti / Sakit" mengambil aturan
  validasi dari server saat layar dibuka lalu render UI secara dinamis
  (label wajib/opsional, single/multi-file picker sesuai MaxFileCount).
  Submit memakai SATU request multipart berisi field leave request + note +
  semua file sekaligus (bukan dua langkah terpisah seperti desain awal).
  Kalau ada file yang gagal diupload meski leave request-nya sendiri sukses
  tersimpan, ditampilkan lewat dialog peringatan (attachmentWarnings) sebelum
  layar ditutup. Layar Riwayat Cuti tetap punya tombol lihat lampiran per
  pengajuan (bottom sheet, lazy-load saat diklik).
- Migration: 20260712180000_AddGeneralDocument. Ditulis manual (bukan hasil
  `dotnet ef migrations add`) karena tooling scaffolding di project ini
  sedang crash (NullReferenceException di EF Core migrations differ) untuk
  SEMUA migration baru, termasuk yang tidak menyentuh modul ini sama sekali
  — root cause-nya AppDbContextModelSnapshot.cs yang sudah lama tidak sinkron
  (cuma mencakup entity Config/HR/System, modul Finance/Inventory/Purchasing/
  Sales/Manufacturing/FixedAssets tidak pernah masuk snapshot). Ini bug/utang
  teknis pra-existing, bukan sesuatu yang ditimbulkan oleh modul Document —
  migration sebelumnya (AddAttendanceGpsSelfService) juga sudah ditulis
  manual dengan alasan yang sama. Migration AddGeneralDocument sudah
  diverifikasi lewat `dotnet ef migrations script` (jalur berbeda yang tidak
  kena bug differ) dan menghasilkan SQL yang bersih (tabel doc_documents +
  doc_reference_type_configs, tanpa kolom/tabel kategori). **Belum
  diselesaikan**: snapshot besar itu sendiri belum direkonstruksi — kalau mau
  `dotnet ef migrations add` bisa dipakai normal lagi ke depannya, snapshot
  perlu disinkronkan ulang (pekerjaan terpisah, besar, di luar scope modul
  ini).
- Acuan: ERP.API/Controllers/v1/Document/DocumentsController.cs,
  ERP.API/Controllers/v1/Document/DocumentReferenceTypeConfigsController.cs,
  ERP.API/Controllers/v1/HR/LeaveRequestsController.cs (contoh
  combined-submit), ERP.Application/Services/Document/DocumentService.cs,
  ERP.Domain/Entities/Document/*.cs, D:\Flutter\AbsenKu\lib\features\leave\,
  dan detail lengkapnya di ReadMeDocumentGeneral.md.

Catatan Permission
- Semua controller HR memakai [Authorize] dan meneruskan access_token user ke API.
- Hanya Departments yang sudah pakai permission granular per menu
  ([RequireMenuPermission] View/Create/Edit/Delete); menu HR lainnya
  (Employees, Positions, Attendance, Leave, Payroll) baru mengandalkan
  [Authorize] umum, belum granular per aksi.

Catatan Gap Implementasi (untuk backlog)
- Salary Setup, Payslips (sebagai halaman list terpisah), Headcount Report,
  dan Turnover Report sudah terdaftar di seed menu database tapi belum
  memiliki controller/view, sehingga akan error/404 jika diklik dari sidebar.
- Lampiran dokumen (modul General Document, lihat bagian tersendiri di atas)
  baru terpasang di Leave Requests (web & mobile AbsenKu). Belum ada halaman
  admin untuk browse semua dokumen lintas modul (cuma bisa dilihat dari
  halaman modul asalnya), dan otorisasi per-record punya edge-case yang
  belum diperbaiki (lihat "Catatan Gap Implementasi" di
  ReadMeDocumentGeneral.md).

Riwayat Perbaikan: Integrasi Self-Attendance Mark & Leave Requests
- Sampai dengan awal Juli 2026, Self-Attendance "Mark" (poin 6) dan Leave
  Requests (poin 4) berjalan sepenuhnya terpisah: karyawan bisa menandai
  "Cuti"/"Sakit" lewat mobile tanpa approval maupun potong kuota resmi sama
  sekali, dan form admin Create/Edit Attendance juga bisa set Status
  Sick/Cuti manual lewat pintu yang sama tanpa lewat Leave Requests.
- Sudah diperbaiki: Mark sekarang hanya menerima Absent/HalfDay, form admin
  Attendance juga tidak bisa set Sick/Cuti manual, kuota MaxDaysPerYear
  divalidasi saat submit Leave Request, dan approval Leave Request otomatis
  sync ke HrAttendanceRecord (lihat detail masing-masing di poin 2, 4, dan 6
  di atas). Mobile app (AbsenKu) sudah disesuaikan juga — lihat
  D:\Flutter\AbsenKu\README.md.

Acuan Implementasi
- Web controller HR:
  ERP.Web/Controllers/HR/*.cs
- Web views HR:
  ERP.Web/Views/HR/*
- API controller HR:
  ERP.API/Controllers/v1/HR/*.cs
- Application services HR:
  ERP.Application/Services/HR/*.cs
- Domain entities HR:
  ERP.Domain/Entities/HR/*.cs
- Seed menu &amp; role HR:
  ERP.Infrastructure/Data/DataSeeder.cs
