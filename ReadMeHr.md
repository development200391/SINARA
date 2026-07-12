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
- Ada aksi Approve dan Reject yang mengubah status cuti dan mencatat approver.
- Submit (baik dari web maupun self-service) sekarang **menolak** kalau total hari
  (Approved + Pending yang sudah ada + hari yang baru diajukan) melebihi
  MaxDaysPerYear jenis cuti tersebut, untuk employee/leaveType/tahun yang sama.
- Saat di-**Approve**, sistem otomatis membuat/mengupdate HrAttendanceRecord
  untuk setiap tanggal dalam rentang StartDate..EndDate dengan Status = Cuti,
  kecuali kalau tanggal itu sudah punya CheckIn/CheckOut (data absensi asli
  tidak akan ditimpa). Ini membuat Attendance Report & Payroll otomatis
  konsisten dengan cuti yang sudah disetujui, tanpa langkah manual tambahan.
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
  ERP.Application/Services/HR/LeaveService.cs.

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
- Tidak ada dukungan lampiran (mis. surat dokter untuk Sakit) di manapun —
  baik di self-service Leave Requests maupun di form Leave Requests web.

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
