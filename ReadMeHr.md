# HR Module (Human Resources)

Modul kepegawaian: employee master, departemen & jabatan, attendance (termasuk self-attendance mobile via GPS), payroll run, dan leave request (cuti) — yang terakhir ini terintegrasi penuh dengan modul General Approval dan General Document.

## 1. Entitas Domain (`ERP.Domain/Entities/HR`)

- **`HrEmployee`** — code, name, email, phone, foto (dengan crop), `DepartmentId`, `PositionId`, tanggal hire/termination, status kepegawaian, `UserId` (opsional link ke `SysUser` — wajib ada untuk bisa submit leave request sendiri).
- **`HrDepartment`** — code, name, `ManagerId` (→ HrEmployee), `ParentDepartmentId` (self-reference, tidak boleh jadi parent untuk dirinya sendiri), status aktif. **`ManagerId` sekarang load-bearing untuk approval** — General Approval me-resolve approver cuti karyawan lewat field ini.
- **`HrPosition`** — name, code, `DepartmentId`, level jabatan (dipakai sebagai basis perhitungan gaji pokok di Payroll), status aktif.
- **`HrAttendanceRecord`** — per karyawan per tanggal: CheckIn/CheckOut (+ koordinat GPS), Status (Present/Late/Absent/HalfDay/Sick/Cuti), Notes. Status Sick/Cuti sekarang read-only kalau berasal dari sync leave request.
- **`HrAttendanceSetting`** — jam kerja global (masuk/pulang/istirahat), toleransi keterlambatan, minimum menit lembur, periode payroll (Start Day, End Day dihitung otomatis), lokasi kantor (Latitude/Longitude/Radius) untuk validasi self-attendance.
- **`HrHoliday`** — nama, tanggal, tipe (mis. National), deskripsi, cakupan (AppliesTo, default "all"), status aktif.
- **`HrLeaveRequest`** — EmployeeId, LeaveTypeId, StartDate/EndDate, Reason, Status (Pending/Approved/Rejected), attachment (via General Document).
- **`HrLeaveType`** — nama, kode, `MaxDaysPerYear`, `IsCarryOver`, status aktif. Default seed: Cuti Tahunan (ANNUAL, 12 hari, carry-over), Cuti Sakit (SICK, 12 hari, tidak carry-over), Cuti Tanpa Bayar (UNPAID, 30 hari).
- **`HrPayrollRun`** / detail per karyawan — bulan/tahun, status (Draft/Processing/Completed), basic salary, allowances, deductions, gross/net salary, tax; link opsional ke `FinJournalEntry` (Source=Payroll) setelah posting.

## 2. API Endpoints (`ERP.API/Controllers/v1/HR`, semua `[Authorize]`)

| Controller | Endpoint utama |
|---|---|
| `EmployeesController` | CRUD dengan filter kode/nama/email/telepon/departemen/jabatan/status/rentang tanggal; validasi posisi harus sesuai departemen, tanggal termination >= tanggal hire |
| `DepartmentsController` | CRUD dengan filter code/name/manager/parent/status; blokir self-parenting |
| `PositionsController` | CRUD dengan filter code/name/departemen/level/status |
| `AttendanceController` | CRUD attendance record; menolak set Status Sick/Cuti manual (error "Sick and Cuti status can only be set through an approved leave request") |
| `AttendanceSettingsController` | Get/update pengaturan jam kerja, toleransi, periode payroll, lokasi kantor |
| `HolidaysController` | CRUD hari libur, filter per tahun |
| `LeaveRequestsController` | CRUD (hanya Pending yang bisa diedit/dihapus), `GET options` (employee dropdown, scoped per user login), `GET self`/`POST self`/`GET self/leave-types` (self-service mobile) |
| `LeaveTypesController` | CRUD jenis cuti |
| `LeaveBalanceController` | `GET` — laporan saldo cuti (MaxDays/UsedDays/RemainingDays) per karyawan/jenis/tahun |
| `PayrollController` | Index (histori run), `POST run` (proses payroll, idempotent per bulan/tahun), Details, Payslip per karyawan |
| `SelfAttendanceController` | `GetToday`, `GetHistory`, `CheckIn`, `CheckOut`, `Mark` — endpoint khusus mobile (AbsenKu), employeeId di-resolve dari token, bukan dari body |
| `DiagnosticsController` | `GET server-time` (bukan khusus HR) — dipakai mobile app untuk sinkronisasi jam |

## 3. Halaman Web (`ERP.Web/Controllers/HR/*.cs`, Views di `HR/`)

| Menu | Route |
|---|---|
| All Employees | `/hr/employees` (+ `/create`, Details/Edit/Delete dari grid) |
| Departments | `/hr/departments` |
| Positions | `/hr/positions` |
| Daily Attendance | `/hr/attendance` |
| Holiday Master | `/hr/attendance/holiday` |
| Attendance Report | `/hr/attendance/report` |
| Attendance Setting | `/hr/attendance/setting` |
| Payroll Run | `/hr/payroll` (+ Details, Payslip per karyawan dari grid) |
| Salary Setup | `/hr/payroll/setup` — **belum diimplementasi**, lihat §6 |
| Payslips (list) | `/hr/payroll/payslips` — **belum diimplementasi**, lihat §6 |
| Leave Requests | `/hr/leave/requests` |
| Leave Balance | `/hr/leave/balance` |
| Leave Types | `/hr/leave/types` |
| Headcount Report | `/hr/reports/headcount` — **belum diimplementasi**, lihat §6 |
| Turnover Report | `/hr/reports/turnover` — **belum diimplementasi**, lihat §6 |

Form Create/Edit/Details Departments & Positions memakai satu partial `_Form.cshtml` yang sama (Details = mode disabled/read-only). Field Manager/Position/UserId pakai `SearchableSelectViewComponent`; checkbox aktif pakai `FormCheckboxViewComponent`.

## 4. Business Rules / Logic Penting

**Departemen & Approval**
- Manager departemen aktif **wajib ter-set** — General Approval me-resolve approver cuti staf lewat `HrDepartment.ManagerId`; departemen aktif tanpa manager membuat pengajuan cuti stafnya **gagal submit** (bukan cuma gagal tampil di UI).
- Halaman Details Department menampilkan manager/parent yang sudah tidak aktif apa adanya (tidak di-null-kan), lewat helper `PopulateReadOnlyOptionsAsync` — beda dari Create/Edit yang menyaring opsi tidak aktif.

**Attendance**
- Status **Sick** dan **Cuti** tidak bisa di-set manual dari form Create/Edit Attendance — murni dikelola otomatis lewat approval Leave Request. Record hasil sync ditampilkan read-only pada field Status (field lain seperti Notes tetap bisa diedit).
- **Attendance Setting**: End Day periode payroll dihitung otomatis (Start Day − 1, dibungkus ke 31 kalau Start Day = 1). Lokasi kantor dipilih lewat peta interaktif (Leaflet + OpenStreetMap, tanpa API key) dengan klik-pada-peta/drag marker/search alamat (Nominatim)/geolocation browser; validasi rentang Lat -90..90, Long -180..180.

**Payroll**
- Formula: `Basic Salary = 3.000.000 + (level jabatan × 1.000.000)`; `Allowances = 20% Basic Salary`; `Deductions = (hari Absent × 50.000) + (hari Late × 20.000)`; `Gross = Basic + Allowances`; `Tax = 5% flat dari Gross`; `Net = Gross − Deductions − Tax` (minimum 0).
- Hanya memproses karyawan status Active; run bersifat idempotent per bulan/tahun (re-run = update, bukan duplikat).
- Payroll yang sudah diposting terhubung ke Finance lewat jurnal otomatis (`Finance > Journals?source=Payroll`).

**Leave Request — dropdown employee di-scope per user**
- Super Admin/HR Manager/HR Staff (atau akun tanpa profil `HrEmployee` — dianggap back-office) melihat SEMUA karyawan aktif di dropdown. User lain hanya melihat dirinya sendiri + karyawan di departemen yang dia jadi manager-nya.
- Diatur lewat parameter opsional `ILeaveService.GetEmployeeOptionsAsync` + query string `scopeEmployeesToCurrentUser=true`. Dropdown filter di halaman Leave Balance **sengaja tidak di-scope** (tetap lihat semua).
- Kalau leave request yang sedang di-Edit employee-nya di luar scope user (mis. dibuat HR untuk karyawan lintas departemen), namanya tetap disuntik manual ke dropdown (`EnsureCurrentEmployeeOption`) supaya tidak kelihatan kosong/salah.

**Leave Request — integrasi General Approval**
- Approve/Reject **bukan lagi flip status langsung** — submit otomatis membuat `ApprovalRequest` (template `HR_LEAVE`, 1 level: `DirectSuperior`), approver-nya HANYA manager departemen karyawan bersangkutan.
- Karyawan pemohon **wajib** punya akun user (`HrEmployee.UserId`) ter-link — kalau tidak, submit gagal dengan pesan jelas.
- Tombol Approve/Reject di grid/Details hanya muncul kalau user login benar-benar approver aktif untuk request itu (`LeaveRequestDto.CanApprove`). Approver juga bisa bertindak lewat Approval Inbox (`/approval/inbox`).
- Leave request Pending yang dibuat sebelum integrasi ini di-backfill otomatis saat startup API (`DataSeeder.BackfillLeaveRequestApprovalsAsync`).
- Submit menolak kalau total hari (Approved + Pending existing + hari baru) melebihi `MaxDaysPerYear` untuk employee/leaveType/tahun yang sama.
- Saat **Approve**, sistem otomatis membuat/update `HrAttendanceRecord` untuk tiap tanggal dalam rentang dengan Status=Cuti (kecuali tanggal itu sudah punya CheckIn/CheckOut asli — tidak ditimpa). Logic ini di `LeaveAttendanceSyncHelper`, dipakai bersama jalur approval baru maupun fallback lama.
- Reject/Delete hanya bisa selama status Pending — begitu Approved, tidak ada jalur API untuk membatalkan (revisi = request baru).

**Leave Request — self-service mobile (AbsenKu)**
- `GET/POST self`, `GET self/leave-types` — employeeId di-resolve dari token login, tidak dipercaya dari body request. Endpoint leave-types self hanya menampilkan jenis cuti aktif (tanpa daftar karyawan, beda dari `/options` yang dipakai admin web).

**Leave Request — lampiran (General Document)**
- Upload lampiran (mis. surat dokter) digabung dalam SATU request bersama field leave request ("combined-submit"), bukan langkah terpisah. Delete lampiran hanya bisa selama status Pending. Halaman Details hanya menampilkan daftar lampiran (view-only).
- **Bug fix otorisasi**: user yang kebetulan punya profil `HrEmployee` sendiri (mis. admin yang juga manager departemen) sebelumnya bisa 500 error saat edit lampiran leave request milik karyawan lain — sudah diperbaiki di `DocumentService.EnsureLeaveRequestAccessAsync` (role Super Admin/HR Manager/HR Staff selalu boleh akses lampiran siapapun).
- Detail arsitektur lengkap ada di `ReadMeDocumentGeneral.md`.

**Self-Attendance (mobile, GPS)**
- CheckIn/CheckOut: karyawan kirim koordinat GPS, server menghitung jarak ke titik kantor (rumus Haversine) dan menolak kalau di luar radius. Jam dicatat pakai `DateTimeOffset.UtcNow` di server, bukan jam dari HP.
- CheckOut **boleh disubmit berkali-kali** dalam satu hari — submit ulang menimpa CheckOut/koordinat dengan yang terbaru (disengaja, supaya karyawan yang salah check-out kecepetan bisa revisi sendiri).
- **Mark** (self-report satu tanggal tanpa approval) **dibatasi hanya untuk Absent dan HalfDay** — Sick/Cuti wajib lewat alur Leave Requests self-service supaya tetap kena kuota & approval. Percobaan Mark dengan Status Sick/Cuti ditolak ("Status is not self-reportable.").

## 5. Relasi Kunci

- `HrEmployee` → `HrDepartment`, `HrPosition`, opsional `SysUser` (via `UserId`).
- `HrDepartment` → `HrEmployee` (Manager) + self-reference (ParentDepartment); `ManagerId` dipakai General Approval sebagai approver cuti.
- `HrLeaveRequest` → `HrEmployee`, `HrLeaveType`, dan (setelah integrasi Approval) → `ApprovalRequest` (`apv_approval_requests`, reference_type=`hr_leave_requests`) → menghasilkan sync ke `HrAttendanceRecord` saat Approved.
- `HrLeaveRequest` → dokumen lampiran via `doc_documents` (reference_type=`hr_leave_requests`).
- `HrPayrollRun`/detail → `HrEmployee`, `HrPosition` (level untuk basic salary), opsional `FinJournalEntry` (Source=Payroll).
- `HrAttendanceRecord` → `HrEmployee`; status Sick/Cuti-nya di-drive oleh `HrLeaveRequest` yang Approved, bukan input langsung.

## 6. Known Gaps / Belum Lengkap

- **Salary Setup** (`/hr/payroll/setup`) dan **Payslips list** (`/hr/payroll/payslips`) — sudah terdaftar di seed menu database tapi **belum ada controller/view**, akan error/404 kalau diklik dari sidebar. Payslip yang ada baru per-karyawan dari halaman Details payroll run.
- **Headcount Report** (`/hr/reports/headcount`) dan **Turnover Report** (`/hr/reports/turnover`) — sama, terdaftar di menu tapi belum ada implementasi.
- **Permission granular** baru diterapkan penuh di Departments (`[RequireMenuPermission]` View/Create/Edit/Delete); menu HR lainnya (Employees, Positions, Attendance, Leave, Payroll) masih mengandalkan `[Authorize]` umum, belum granular per aksi.
- **Lampiran dokumen** (General Document) baru terpasang di Leave Requests — belum ada halaman admin untuk browse semua dokumen lintas modul; beberapa edge-case otorisasi masih tercatat sebagai gap di `ReadMeDocumentGeneral.md`.
- **Utang teknis migration**: `AppDbContextModelSnapshot.cs` sudah lama tidak sinkron (cuma mencakup entity Config/HR/System; Finance/Inventory/Purchasing/Sales/Manufacturing/FixedAssets tidak pernah masuk snapshot) — akibatnya `dotnet ef migrations add` crash (NullReferenceException) untuk migration baru apapun, sehingga migration terbaru (`AddGeneralDocument`, `AddAttendanceGpsSelfService`) ditulis manual dan diverifikasi lewat `dotnet ef migrations script`. Merekonstruksi snapshot adalah pekerjaan terpisah yang belum digarap.

## 7. Catatan Riwayat & Verifikasi

- **Verifikasi ulang 2026-07-28**: re-scan penuh terhadap seluruh path HR (Domain/Application/API/Web/DataSeeder) — tidak ada perubahan kode sejak dokumen ini terakhir ditulis, semua klaim di atas (termasuk keempat gap) masih akurat 1:1 dengan kode saat ini.
- **Riwayat perbaikan (awal Juli 2026)**: sebelumnya Self-Attendance "Mark" dan Leave Requests berjalan sepenuhnya terpisah — karyawan bisa menandai Sick/Cuti lewat mobile tanpa approval/potong kuota, dan form admin Attendance juga bisa set Sick/Cuti manual lewat pintu yang sama. Sudah diperbaiki: Mark sekarang hanya menerima Absent/HalfDay, form admin Attendance tidak bisa set Sick/Cuti manual, kuota MaxDaysPerYear divalidasi saat submit, dan approval Leave Request otomatis sync ke Attendance (lihat §4). Mobile app AbsenKu sudah disesuaikan — lihat `D:\Flutter\AbsenKu\README.md`.

## Acuan Implementasi

- Web: `ERP.Web/Controllers/HR/*.cs`, `ERP.Web/Views/HR/*`
- API: `ERP.API/Controllers/v1/HR/*.cs`
- Application services: `ERP.Application/Services/HR/*.cs`
- Domain: `ERP.Domain/Entities/HR/*.cs`
- Seed menu & role: `ERP.Infrastructure/Data/DataSeeder.cs`
- Integrasi terkait: `ReadMeGeneralApproval.md` (approval cuti), `ReadMeDocumentGeneral.md` (lampiran leave request)
