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

2. Daily Attendance (/hr/attendance)
- Rekam absensi harian per karyawan per tanggal: check-in, check-out, status
  (Present/Late/Absent/HalfDay, dll), catatan.
- CRUD lengkap dengan filter karyawan, departemen, rentang tanggal, status.

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
- Pengaturan global jam kerja: jam masuk/pulang, jam istirahat, toleransi
  keterlambatan (menit), minimum menit lembur (overtime).
- Pengaturan periode absensi/payroll (tanggal mulai s/d akhir periode, default 26-25).
- Dipakai sebagai basis perhitungan status "Late" pada laporan absensi dan
  periode perhitungan payroll.

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
