ReadMe General Approval Module SINARA

Dokumen ini merangkum modul **General Approval (APV)**: mesin approval
generik lintas modul, yang dirancang supaya modul lain (Fixed Assets,
Purchasing, Payroll, Finance, dst.) tidak perlu bikin tabel & alur approval
sendiri-sendiri — cukup daftarkan satu baris ke `apv_approval_requests`
(pola `reference_type` + `reference_id`, sama seperti General Document).

Rencana lengkap ada di
`SINARA_ERP_GeneralApproval_Panduan_Detail.docx` (5 fase: APV-1 Fondasi,
APV-2 Request & Workflow, APV-3 Notifikasi & Eskalasi, APV-4 Integrasi &
Audit, APV-5 Laporan & Hak Akses). README ini merangkum isi docx tsb DAN
membandingkannya dengan apa yang sungguhan sudah ada di kode saat ini.

STATUS IMPLEMENTASI SAAT INI — PENTING, BACA DULU
==================================================
**Modul ini SUDAH DIIMPLEMENTASI dan fungsional end-to-end** (Domain →
Infrastructure → Application → API → Web). Menu "General Approval" di
ERP.Web sekarang bisa dipakai sungguhan: bikin request lewat API, masuk ke
Inbox approver yang tepat, di-approve/reject/cancel, notifikasi
in-app + email terkirim, dan job eskalasi/reminder jalan otomatis tiap 30
menit lewat Hangfire.

Yang SUDAH ada dan jalan:
- Domain entities — 7 tabel `apv_*` lengkap dengan relasi
  (`ERP.Domain/Entities/Approval/*.cs`).
- Migration `20260713100000_AddGeneralApproval` — sudah diterapkan ke
  database (`dotnet ef database update`, diverifikasi lewat
  `dotnet ef migrations script`).
- Enum (`ERP.Domain/Enums/Approval/*.cs`) — 6 file, tidak berubah dari
  rencana awal.
- DTO (`ERP.Application/DTOs/Approval/ApprovalDtos.cs`) — tidak berubah.
- Application services (`ERP.Application/Services/Approval/`):
  `ApprovalTemplateService` (CRUD Template + Level, options),
  `ApprovalRequestService` (routing engine inti: submit, resolve approver
  per ApproverType, Sequential/Parallel/AnyOne, approve/reject/cancel,
  delegasi otomatis & manual, job eskalasi/reminder),
  `ApprovalDelegationService` (CRUD + revoke + approver options),
  `ApprovalReportService` (dashboard, SLA report, by-template report,
  audit log). `IApprovalCallbackService` adalah registry pattern
  (`IEnumerable<IApprovalCallbackService>` di-inject ke
  `ApprovalRequestService`, dicari lewat `ReferenceType`) — implementasi
  konkret pertama sudah terpasang untuk `hr_leave_requests`
  (`LeaveRequestApprovalCallbackService`); Fixed Assets/Payroll/
  Purchasing/Finance belum. Lihat bagian "Integrasi Modul HR Leave
  Request" di bawah.
- `IApprovalNotificationService` (`ERP.API/Services/ApprovalNotificationService.cs`)
  — tulis baris `apv_notifications`, push in-app lewat SignalR
  (`ERP.API/Hubs/ApprovalHub.cs`, endpoint `/hubs/approval`), kirim email
  lewat MailKit memakai konfigurasi `Smtp` yang sudah ada di
  `appsettings.json` (dipakai bersama fitur lupa-password, sudah aktif
  dengan kredensial Gmail asli — lihat catatan keamanan di bawah).
- Hangfire (`ERP.API/Program.cs`) — storage PostgreSQL (pakai
  `ConnectionStrings:DefaultConnection` yang sama dengan EF Core),
  recurring job `approval-escalation-reminders` jalan tiap 30 menit
  memanggil `IApprovalRequestService.ProcessEscalationsAndRemindersAsync`.
  Dashboard Hangfire (`/hangfire`) hanya aktif di environment Development.
- API controllers (`ERP.API/Controllers/v1/Approval/`) — 7 controller
  (`ApprovalDashboardController`, `ApprovalInboxController`,
  `ApprovalRequestsController`, `ApprovalTemplatesController` (+ nested
  Levels), `ApprovalDelegationsController`, `ApprovalLookupsController`,
  `ApprovalReportsController`), route persis sesuai kontrak
  `ApprovalApiClient`/`IApprovalApiClient` — Web layer yang sudah dibangun
  sebelumnya JALAN TANPA PERUBAHAN.
- Web layer penuh: controller (`ApprovalController` + 7 partial class),
  ViewModel, Razor view, `ApprovalApiClient`/`IApprovalApiClient` — tidak
  berubah dari sebelumnya, sekarang benar-benar terhubung ke API asli.
- Approval Inbox (`/approval/inbox`) punya tombol "Detail" per baris yang
  langsung buka halaman detail record sumbernya (bukan cuma kolom generik
  Template/Amount/Requester) — lewat `ApprovalReferenceLinkResolver`
  (`ERP.Web/Services/ApprovalReferenceLinkResolver.cs`), registry kecil
  `ReferenceType` → URL template. Baru ada satu entry (`hr_leave_requests`
  → `/hr/leave/requests/details/{id}`); tinggal tambah satu baris per
  modul baru yang connect ke APV. Belum dipasang di `/approval/my-requests`
  (gampang ditambah kalau perlu, reuse resolver yang sama).
- Menu & modul sudah di-seed di database (module "APV", 8 item menu), dan
  Super Admin otomatis dapat izin penuh ke menu-menu itu lewat
  `SeedSuperAdminPermissionsAsync` (yang men-generate izin untuk SEMUA
  menu, termasuk yang baru) — tidak perlu seed permission `apv.*` terpisah
  karena modul ini memakai mekanisme permission yang sama dengan modul
  lain (`CfgRoleMenuPermission` + `[RequireMenuPermission]`), bukan
  string permission `apv.*` seperti yang tadinya direncanakan di docx.
- Template & level default — di-seed lewat `SeedApprovalTemplatesAsync`
  di `DataSeeder.cs` (lihat tabel di bagian 7 di bawah untuk detail per
  template, termasuk penyesuaian dari rencana awal docx).

Yang BELUM ada / catatan penting:
- **Integrasi ke modul lain** — **HR Leave Request SUDAH terhubung**
  (integrasi pertama, lihat bagian "Integrasi Modul HR Leave Request" di
  bawah). Fixed Assets, Purchasing, Payroll, Finance MASIH belum — tidak
  ada satupun dari modul-modul itu yang memanggil
  `IApprovalRequestService.SubmitAsync` atau mengimplementasikan
  `IApprovalCallbackService`. Engine-nya sudah lengkap dan bisa dipanggil
  langsung secara in-process (`SubmitAsync` sengaja tidak diekspos sebagai
  endpoint API publik — hanya dipanggil modul sumber lewat Application
  layer, persis seperti `LeaveService.SubmitAsync` memanggilnya), tapi
  belum ada "tombol Submit for Approval" di modul-modul itu. Sebagian juga
  karena entity transaksional sumbernya sendiri (PR/PO, Payroll Run yang
  bisa "diajukan", dst.) belum lengkap dibangun di modul-modul itu — lihat
  detail per template di bagian 7.
- **SignalR client di Razor view belum dipasang** — hub `/hubs/approval`
  sudah jalan di server dan `IApprovalNotificationService` sudah push ke
  grup `approval-user-{userId}`, tapi belum ada JS client
  (`@microsoft/signalr`) di `ERP.Web` yang connect & tampilkan toast
  notifikasi real-time. Notifikasi tetap tersimpan di `apv_notifications`
  dan bisa dibaca lewat polling/reload halaman.
- **Kredensial SMTP di `appsettings.json` adalah kredensial Gmail asli**
  (dipakai bersama fitur reset password) — commit ini TIDAK menambah
  risiko baru (sudah ada sebelumnya), tapi kalau repo ini pernah/akan
  di-push ke remote publik, rotate App Password Gmail tsb segera.
- **Split nilai PRC_PO_LOW/PRC_PO_HIGH (Rp5.000.000)** adalah asumsi
  penulis seeder, bukan dari docx (docx cuma sebut ambang auto-approve
  Rp500.000 untuk PO rendah) — sesuaikan lewat halaman Templates kalau
  nilainya tidak cocok kebutuhan nyata.
- **`ReferenceType` di beberapa template default menunjuk tabel yang
  belum ada** (`pur_purchase_orders` untuk PRC_PO_LOW/PRC_PO_HIGH — modul
  Purchasing baru punya master data, belum ada entity Purchase
  Order/Requisition). Baris template-nya tetap di-seed sebagai config
  siap-pakai; begitu modul Purchasing punya entity PO, tinggal panggil
  `SubmitAsync("Purchasing", "pur_purchase_orders", poId, subject, amount,
  requestedByUserId, notes)`.
- Kalau mau sambungkan modul sumber, urutan kerja yang masuk akal: (1)
  pastikan entity transaksionalnya (mis. Fixed Assets Transfer) punya
  status "PendingApproval" dan field untuk menyimpan hasil approval, (2)
  panggil `IApprovalRequestService.SubmitAsync(...)` saat user klik
  "Submit for Approval", (3) buat kelas yang implement
  `IApprovalCallbackService` dengan `ReferenceType` yang cocok, daftarkan
  di `ERP.Application/DependencyInjection.cs` sebagai
  `services.AddScoped<IApprovalCallbackService, XxxCallbackService>()`
  (bisa daftar lebih dari satu — routing engine resolve otomatis lewat
  `ReferenceType`), lalu implementasikan efek samping approve/reject/
  cancel-nya di sana (README bagian 6 masih relevan untuk pola ini).

Integrasi Modul HR Leave Request (SUDAH JALAN — integrasi pertama)
======================================================================
`hr/leave/requests` (menu HR → Leave Requests) sekarang memakai General
Approval sebagai mesin approval-nya, menggantikan flip status
langsung yang dipakai sebelumnya. Tombol Approve/Reject & konfirmasi
`confirm()` polos di halaman itu TIDAK BERUBAH BENTUKNYA, tapi sekarang
CUMA MUNCUL kalau user yang login benar-benar boleh bertindak (lihat
"CanApprove" di bawah) — sebelumnya tombolnya selalu tampil untuk semua
Pending request tanpa peduli siapa yang login.

- **Submit** — `LeaveService.SubmitAsync`
  (`ERP.Application/Services/HR/LeaveService.cs`) sekarang, setelah
  `HrLeaveRequest` tersimpan, memanggil
  `IApprovalRequestService.SubmitAsync("HR", "hr_leave_requests",
  entity.Id, subject, amount: null, requestedByUserId, notes)`.
  `requestedByUserId` diambil dari `HrEmployee.UserId` milik karyawan yang
  mengajukan cuti — **kalau karyawan itu tidak punya akun user (SysUser)
  yang ter-link, pengajuan cuti akan GAGAL** dengan pesan
  "Cannot submit for approval: '{nama}' has no linked user account."
  (baris `HrLeaveRequest` yang sudah sempat tersimpan otomatis di-rollback
  lewat soft-delete supaya tidak jadi baris hantu yang mengganggu
  pengecekan cuti overlap/kuota di pengajuan berikutnya). Ini perubahan
  perilaku baru dibanding sebelumnya (dulu karyawan tanpa akun user tetap
  bisa diajukan cuti-nya oleh HR admin) — kalau ada karyawan begitu, link
  dulu akun user-nya sebelum mengajukan cuti.
- **Approve/Reject** — endpoint API `PUT .../approve` dan `.../reject`
  (`ERP.API/Controllers/v1/HR/LeaveRequestsController.cs`) sekarang cek
  dulu lewat `IApprovalRequestService.FindActiveRequestIdAsync
  ("hr_leave_requests", id)`: kalau ada `ApprovalRequest` yang masih
  Pending/InProgress untuk leave request itu, aksi didelegasikan ke
  `IApprovalRequestService.ApproveAsync`/`RejectAsync` (lewat mesin APV,
  lengkap dengan audit log, notifikasi, dan enforcement siapa yang boleh
  bertindak). Kalau TIDAK ada (mis. data lama dari sebelum fitur ini ada),
  fallback ke `LeaveService.ApproveAsync`/`RejectAsync` yang lama (flip
  status langsung) — jadi data lama tetap bisa diproses tanpa error.
- **Efek samping approve/reject** — `LeaveRequestApprovalCallbackService`
  (`ERP.Application/Services/HR/LeaveRequestApprovalCallbackService.cs`,
  `ReferenceType = "hr_leave_requests"`) dipanggil routing engine saat
  keputusan final: set `HrLeaveRequest.Status`/`ApprovedBy`/`ApprovedAt`,
  dan kalau Approved, sinkronkan ke `HrAttendanceRecord` (persis logika
  `SyncApprovedLeaveToAttendanceAsync` yang lama, sekarang diekstrak ke
  `LeaveAttendanceSyncHelper` supaya dipakai bersama oleh alur baru DAN
  alur fallback lama tanpa duplikasi kode). Cancel dari APV (lewat
  `/approval/my-requests`) diperlakukan sama seperti Reject (`LeaveStatus`
  tidak punya nilai `Cancelled` tersendiri) — pilihan pragmatis supaya
  tidak perlu ubah skema/enum.
- **Siapa yang bisa approve sekarang** — template `HR_LEAVE` (lihat tabel
  di bagian 7) cuma punya 1 level: `ApproverType = DirectSuperior`, jadi
  **HANYA manajer department karyawan yang bersangkutan** (`HrDepartment.
  ManagerId`) yang punya step aktif untuk di-approve/reject. Ini LEBIH
  KETAT dari sebelumnya (dulu siapapun dengan izin menu Leave Requests
  bisa approve/reject siapapun, tanpa cek hubungan manajerial) — perilaku
  baru ini konsekuensi wajar dari pindah ke mesin approval generik; kalau
  departemen karyawan belum punya `ManagerId` ter-set, submit-nya akan
  gagal dengan pesan "Cannot resolve direct superior: the requester's
  department has no manager assigned." — pastikan semua department yang
  aktif sudah punya manajer sebelum staf-nya mengajukan cuti.
- **RequireCommentOnReject = false** khusus untuk `HR_LEAVE` (beda dari
  template lain yang defaultnya `true`) — supaya perilaku reject-tanpa-
  alasan yang sudah ada sebelumnya di halaman Leave Requests tetap sama.
  Kalau nanti mau mewajibkan alasan reject, ubah field ini lewat halaman
  Approval → Templates (tidak perlu ubah kode).
- Approver JUGA bisa bertindak lewat Approval Inbox (`/approval/inbox`)
  selain lewat tombol di halaman HR Leave Requests — dua-duanya memanggil
  engine yang sama, jadi hasilnya konsisten dari sisi manapun diambil.
- **`LeaveRequestDto.CanApprove`** (bool) — dihitung server-side lewat
  `IApprovalRequestService.GetActionablePermissionsAsync(referenceType,
  referenceIds, userId)`: true kalau belum ada `ApprovalRequest` aktif
  utk record itu (fallback legacy, siapapun boleh) ATAU user yang login
  punya step aktif di request itu. Dipakai `LeaveService.GetRequestsAsync`/
  `GetByIdAsync` (parameter `currentUserId` opsional — kalau tidak diisi,
  default `false`/tersembunyi) untuk menyembunyikan tombol Approve/Reject
  di `Index.cshtml` & `Details.cshtml` SEBELUM user klik, bukan menunggu
  gagal 403 setelah klik.
- **Bug ditemukan & diperbaiki**: leave request Pending yang dibuat
  SEBELUM integrasi ini ada tidak punya `ApprovalRequest` terkait, jadi
  approve/reject-nya selalu jatuh ke jalur fallback lama (unrestricted —
  siapapun bisa approve, bukan cuma manajer). Fix: `DataSeeder` sekarang
  punya `BackfillLeaveRequestApprovalsAsync` (jalan tiap startup, no-op
  kalau sudah lengkap) yang bikinkan `ApprovalRequest` utk leave request
  Pending lama yang belum punya, dengan logika resolve manajer yang sama
  seperti submit normal. Employee tanpa akun user ter-link dilewati
  (di-log sebagai warning, tidak menghentikan startup).
- **Bug terkait ditemukan & diperbaiki (di modul General Document, bukan
  APV, tapi ditemukan lewat alur ini)**: `DocumentService.
  EnsureLeaveRequestAccessAsync` sebelumnya cuma izinkan akses lampiran
  kalau user adalah pemilik record ATAU user itu SAMA SEKALI tidak punya
  profil `HrEmployee` (dianggap "back-office"). Akun `admin` yang
  kebetulan juga terhubung ke `HrEmployee` (jadi manajer semua departemen
  di data seed) gagal lolos dua-duanya saat coba edit lampiran leave
  request milik karyawan lain → 500 `UnauthorizedAccessException`. Fix:
  tambah fallback cek role — Super Admin/HR Manager/HR Staff tetap boleh
  akses lampiran leave request siapapun. Detail lengkap & sisa gap-nya ada
  di `ReadMeDocumentGeneral.md`.
- **Bug terkait #2 (ditemukan lewat fitur Approval Inbox di AbsenKu,
  mobile)**: fix di atas masih belum menutup kasus paling umum — approver
  SESUNGGUHNYA (manajer departemen biasa via `ApprovalApproverType.
  DirectSuperior`) BUKAN Super Admin/HR Manager/HR Staff, jadi tetap
  ditolak (403) saat coba lihat lampiran (mis. surat dokter) sebelum
  approve/reject dari mobile. Fix: `EnsureLeaveRequestAccessAsync`
  sekarang juga cek langsung ke `apv_approval_steps`/`apv_approval_requests`
  — siapapun yang punya step aktif (`IsActive && Action == null`) untuk
  `ApprovalRequest` yang ter-link ke leave request itu, diizinkan akses
  lampirannya. Lihat `D:\Flutter\AbsenKu\README.md` bagian "Approval Inbox"
  untuk sisi mobile-nya.

Struktur Menu General Approval (sudah ter-seed, tapi isinya belum jalan)
=========================================================================
1. Approval Dashboard — `/approval`
2. Worklist
   - Approval Inbox — `/approval/inbox`
   - My Approval Requests — `/approval/my-requests`
   - Delegations — `/approval/delegations`
3. Configuration
   - Approval Templates — `/approval/templates`
4. Reports
   - SLA Report — `/approval/reports/sla`
   - By Template Report — `/approval/reports/by-template`
   - Audit Trail — `/approval/reports/audit`

(Level approval per template, mis. `/approval/templates/{id}/levels`,
diakses dari dalam halaman Templates, bukan menu sidebar tersendiri.)

Konsep Desain (dari docx — belum tentu sama persis dengan kode final nanti)
=============================================================================

1. Entitas Inti (rencana skema `apv_*`)
- **apv_approval_templates** — satu baris = satu aturan approval untuk satu
  jenis dokumen: Code, Name, Module, ReferenceType (kode reference_type
  yang dipakai modul sumber, mis. `fa_asset_transfers`), ApprovalType
  (Sequential/Parallel/AnyOne), MinAmount/MaxAmount (rentang nilai transaksi
  yang dicakup template ini), AutoApproveBelow (nilai di bawah ini approve
  otomatis tanpa lewat approver), SlaHours (default 24), AllowDelegation,
  RequireCommentOnReject, IsActive.
- **apv_approval_levels** — level-level approval dalam satu template,
  urut lewat LevelOrder (harus unik & berurutan per template): LevelName,
  ApproverType (Role/Position/SpecificUser/DirectSuperior) + FK sesuai
  tipe (ApproverRoleId/ApproverPositionId/ApproverUserId),
  MinApproversRequired (kuorum untuk Parallel/AnyOne), EscalationHours +
  EscalateToLevelId (self-FK, ke level mana dieskalasi kalau lewat SLA).
- **apv_delegations** — pendelegasian approval dari satu user ke user lain:
  DelegatorUserId → DelegateUserId, TemplateId (nullable = berlaku untuk
  SEMUA template), StartDate/EndDate, Reason, IsActive.
- **apv_approval_requests** — satu baris = satu pengajuan approval:
  RequestNo (auto-generate format `APV-{YYYY}-{00001}`, reset tiap tahun),
  TemplateId, ReferenceType + ReferenceId (menunjuk record sumber),
  Subject, Amount, RequestedBy/At, CurrentLevelId, Status (Pending/
  InProgress/Approved/Rejected/Cancelled/Expired), FinalActionAt/By, Notes.
  **Aturan penting**: unique constraint `(reference_type, reference_id)`
  selama status masih Pending/InProgress — satu record sumber cuma boleh
  punya SATU approval request yang masih berjalan di satu waktu.
- **apv_approval_steps** — satu baris per approver per level dalam satu
  request: RequestId, LevelId (+ salinan LevelOrder), ApproverUserId,
  IsDelegated + DelegatedFromUserId (kalau step ini hasil delegasi),
  Action (Approved/Rejected/Delegated/Returned, nullable selama belum
  diambil tindakan), ActionAt, Comment (WAJIB diisi kalau Action=Rejected),
  DueAt (= RequestedAt + SlaHours template), NotifiedAt, ReminderCount,
  IsActive (step yang belum giliran/sudah dilewati = false).
- **apv_notifications** — notifikasi per step: RecipientUserId,
  NotificationType (NewRequest/Approved/Rejected/Reminder/Escalated/
  Cancelled/Delegated), Channel (InApp/Email/Both), Subject/Body, IsRead,
  SentAt/FailedAt/RetryCount.
- **apv_approval_audit_logs** — log APPEND-ONLY (tidak ada soft-delete,
  tidak bisa diubah/dihapus): RequestId, StepId (nullable), ActorUserId,
  Action (string bebas: CREATED/APPROVED/REJECTED/DELEGATED/ESCALATED/
  CANCELLED/REMINDED), OldStatus/NewStatus, IpAddress, UserAgent, Comment,
  CreatedAt. Docx menyarankan partisi per tahun kalau sudah di atas 1 juta
  baris/tahun.

2. Enum (SUDAH ada di kode, `ERP.Domain/Enums/Approval/`)
- `ApprovalType`: Sequential(0) / Parallel(1) / AnyOne(2).
- `ApprovalApproverType`: Role(0) / Position(1) / SpecificUser(2) /
  DirectSuperior(3).
- `ApprovalRequestStatus`: Pending(0) / InProgress(1) / Approved(2) /
  Rejected(3) / Cancelled(4) / Expired(5).
- `ApprovalStepAction`: Approved(0) / Rejected(1) / Delegated(2) /
  Returned(3).
- `ApprovalNotificationChannel`: InApp(0) / Email(1) / Both(2).
- `ApprovalNotificationType`: NewRequest(0) / Approved(1) / Rejected(2) /
  Reminder(3) / Escalated(4) / Cancelled(5) / Delegated(6).

3. Aturan Routing Engine (SUDAH diimplementasi —
   `ERP.Application/Services/Approval/ApprovalRequestService.cs`)
- Request baru dibuat → resolve template berdasarkan ReferenceType +
  Amount (dicocokkan ke MinAmount/MaxAmount template) → kalau Amount di
  bawah AutoApproveBelow, langsung Status=Approved tanpa bikin step sama
  sekali → kalau tidak, buat step(s) untuk level 1, aktifkan, kirim
  notifikasi.
- **Sequential**: level diproses satu-satu berurutan. Level N selesai
  (MinApproversRequired terpenuhi) baru level N+1 diaktifkan. Level
  terakhir selesai → Status=Approved → panggil callback.
- **Parallel**: SEMUA step di level yang sama diaktifkan bersamaan; level
  dianggap selesai kalau jumlah approve ≥ `MinApproversRequired` level
  tsb.
- **AnyOne**: sama seperti Parallel (semua step di level itu aktif
  bersamaan), tapi step saudara-saudaranya di level yang sama otomatis
  di-skip (IsActive=false) begitu SATU approver approve, sehingga level
  langsung selesai.
  > **Catatan implementasi**: kuorum "selesai" memakai `MinApproversRequired`
  > yang di-set statis di konfigurasi Level, BUKAN dihitung ulang dari
  > jumlah approver yang benar-benar ter-resolve saat request itu dibuat.
  > Untuk approver type Role/Position, jumlah orang riil bisa berubah
  > (mis. staf baru masuk role tsb) — kalau mau "Parallel" berarti benar-
  > benar SEMUA anggota role saat itu, set `MinApproversRequired` manual
  > sesuai estimasi headcount, atau pakai ApproverType=SpecificUser untuk
  > level yang butuh kepastian penuh.
- Reject dari approver manapun (di level manapun) → langsung
  Status=Rejected, seluruh step yang masih aktif dinonaktifkan, callback
  dipanggil dengan hasil Rejected — TIDAK melanjutkan ke level berikutnya.
- Requester bisa Cancel — HANYA selama status masih Pending/InProgress DAN
  belum ada satupun step yang Approved (bukan cuma "level 1" — di
  Parallel/AnyOne, approval pertama yang masuk di level manapun langsung
  mengunci status supaya tidak membatalkan approval yang sudah diberikan).
- `ApprovalStepAction.Returned` ada di enum (dari rencana docx) tapi TIDAK
  dipakai oleh engine saat ini — tidak ada alur "kembalikan ke requester
  untuk revisi", hanya Approved/Rejected/Delegated.

4. SLA, Reminder, Eskalasi (SUDAH diimplementasi — job Hangfire
   `approval-escalation-reminders`, tiap 30 menit, dikonfigurasi di
   `ERP.API/Program.cs`, logikanya di
   `ApprovalRequestService.ProcessEscalationsAndRemindersAsync`)
- Tiap step punya DueAt = RequestedAt + SlaHours template.
- Sisa waktu ≤ 4 jam & belum pernah diingatkan → reminder pertama.
- Sisa waktu ≤ 1 jam & sudah 1x diingatkan → reminder kedua (mendesak).
- Lewat DueAt & level itu punya EscalateToLevelId → otomatis eskalasi ke
  level tsb (approver baru dapat step, level asal ditandai selesai/skip).
- Lewat DueAt & tidak ada target eskalasi → alert ke Super Admin +
  ditandai overdue di dashboard (tidak otomatis approve/reject).

5. Delegasi (SUDAH diimplementasi)
- User A (delegator) bisa set delegasi ke User B (delegate) untuk periode
  StartDate–EndDate, scope ke SATU template tertentu atau SEMUA template
  (TemplateId null) — CRUD lewat `ApprovalDelegationService`.
- Saat approval engine resolve approver suatu step dan approver aslinya
  sedang punya delegasi aktif yang cocok, step BARU dibuat untuk delegate
  (IsDelegated=true, DelegatedFromUserId=user asli), step lama ditandai
  Action=Delegated. Ini terjadi otomatis setiap level diaktifkan
  (`ActivateLevelAsync`), tidak perlu aksi manual dari approver.
- Selain delegasi terjadwal di atas, ada juga **delegasi ad-hoc per-aksi**:
  saat approve/reject, `TakeApprovalActionRequest.DelegateUserId` bisa
  diisi untuk meneruskan step itu ke user lain alih-alih approver
  bertindak sendiri (tidak butuh baris `apv_delegations` permanen) — ini
  interpretasi penulis atas field `DelegateUserId` di DTO yang sudah ada
  di kontrak Web sebelumnya, karena docx tidak menjelaskan detailnya.

6. Callback ke Modul Sumber (SUDAH diimplementasi kerangkanya —
   `IApprovalCallbackService` di
   `ERP.Application/Services/Approval/IApprovalCallbackService.cs`,
   **belum ada implementasi konkret terpasang**)
- Approval TIDAK tahu cara memproses efek samping tiap jenis dokumen
  (mis. memindahkan lokasi aset, posting jurnal, kirim PO ke vendor) — itu
  tanggung jawab modul sumbernya sendiri.
- Pola: `IApprovalCallbackService.OnApprovedAsync/OnRejectedAsync/
  OnCancelledAsync(referenceId, actorUserId, ...)`, di-resolve dari
  `IEnumerable<IApprovalCallbackService>` (dicari lewat properti
  `ReferenceType`) yang di-inject ke `ApprovalRequestService` — pola
  strategy/registry, BUKAN generic event bus. Kalau tidak ada
  implementasi yang cocok untuk `ReferenceType` sebuah request, callback
  dilewati (tidak error) — request tetap ganti status seperti biasa,
  cuma tidak ada efek samping ke modul sumber.
- Rencana pemetaan (BELUM ada satupun yang diimplementasikan — lihat
  bagian "Integrasi ke modul lain" di status implementasi di atas):
  * `fa_asset_transfers` → `FATransferCallbackService` (update status
    transfer + lokasi/departemen aset kalau approved).
  * `fa_disposals` → `FADisposalCallbackService` (set aset jadi Disposed +
    buat jurnal pelepasan).
  * `hr_payroll_runs` → `PayrollCallbackService` (proses pembayaran
    payslip) — nama tabel dikoreksi dari rencana awal docx
    (`prl_payroll_runs`) ke nama tabel asli di kode, `hr_payroll_runs`.
  * `pur_purchase_orders` → `ProcurementCallbackService` (kirim PO ke
    vendor) — tabel ini belum ada di modul Purchasing sama sekali.
  * `fin_journal_entries` → `FinanceCallbackService` (posting ke GL).
- Setiap transisi status (dibuat/diapprove/ditolak/dieskalasi/
  dibatalkan/diingatkan) dicatat ke apv_approval_audit_logs, terlepas dari
  callback berhasil atau tidak.

7. Template Default (SUDAH di-seed — `DataSeeder.SeedApprovalTemplatesAsync`,
   idempotent lewat cek `Code` yang sudah ada)
| Code | Module | ReferenceType | ApprovalType | SLA | Level | Catatan |
|---|---|---|---|---|---|---|
| FA_TRANSFER | Fixed Assets | `fa_asset_transfers` | Sequential | 24 jam | 1) DirectSuperior 2) Role "Finance Staff" | — |
| FA_DISPOSAL | Fixed Assets | `fa_disposals` | Sequential | 48 jam | 1) DirectSuperior 2) Role "Finance Staff" | — |
| FA_MAINTENANCE | Fixed Assets | `fa_maintenance_orders` | AnyOne | 24 jam | 1) Role "Inventory Manager" | Auto-approve di bawah Rp1.000.000 |
| PRL_PAYROLL | Payroll | `hr_payroll_runs` | Sequential | 24 jam | 1) Role "HR Manager" 2) Role "Finance Staff" | ReferenceType dikoreksi dari rencana docx (`prl_payroll_runs`) ke nama tabel asli |
| PRC_PO_LOW | Purchasing | `pur_purchase_orders` | AnyOne | 8 jam | 1) Role "Finance Staff" | Auto-approve di bawah Rp500.000; berlaku utk jumlah ≤ Rp5.000.000 (asumsi seeder); tabel sumber belum ada |
| PRC_PO_HIGH | Purchasing | `pur_purchase_orders` | Sequential | 24 jam | 1) DirectSuperior 2) Role "Finance Staff" | Berlaku utk jumlah > Rp5.000.000 (asumsi seeder); tabel sumber belum ada |
| FIN_JOURNAL | Finance | `fin_journal_entries` | Sequential | 24 jam | 1) Role "Finance Staff" | — |
| HR_LEAVE | HR | `hr_leave_requests` | Sequential | 24 jam | 1) DirectSuperior | RequireCommentOnReject=false; **satu-satunya template yang sudah benar-benar dipanggil** oleh modul sumbernya (`LeaveService.SubmitAsync`) — lihat bagian "Integrasi Modul HR Leave Request" |

Role yang dipakai di atas (`HR Manager`, `Finance Staff`, `Inventory
Manager`) adalah role yang SUDAH ada di `DataSeeder.SeedRolesAsync` —
bukan role baru. Kalau role-role itu belum ter-seed (database lama),
`SeedApprovalTemplatesAsync` akan skip seluruh seeding template (guard
di awal method) sampai `SeedRolesAsync` jalan duluan — urutannya sudah
benar di `SeedAsync` jadi ini seharusnya tidak pernah terjadi di deploy
normal.

8. Role & Permission — TIDAK memakai skema string permission `apv.*`
   seperti rencana awal docx. Modul ini memakai mekanisme yang SAMA
   dengan semua modul lain di codebase: `CfgRoleMenuPermission` per
   (Role, Menu) dengan flag CanView/CanCreate/CanEdit/CanDelete,
   ditegakkan lewat `[RequireMenuPermission]` di WEB controller (API
   controller cuma `[Authorize]`, tidak ada pengecekan per-menu — sama
   seperti modul General Document dan modul lain). Super Admin otomatis
   dapat izin penuh ke semua menu APV lewat `SeedSuperAdminPermissionsAsync`
   (tidak spesifik ke APV, berlaku untuk semua menu). Kalau mau role lain
   (mis. "Approval Admin" khusus Configuration saja, "Approver" yang cuma
   bisa lihat Inbox) juga bisa akses menu APV, atur lewat halaman
   Role Management → Menu Permission yang sudah ada (bukan seed baru).

Flowchart Alur Approval (mengikuti desain di docx)
====================================================

```mermaid
flowchart TD
    A[Modul sumber ajukan approval\nreference_type + reference_id + amount] --> B{Resolve Template\nberdasar ReferenceType + Amount}
    B -->|Amount di bawah AutoApproveBelow| Z1([Status = Approved\notomatis, tanpa step])
    B -->|Amount dalam rentang template| C[Buat apv_approval_requests\nStatus = Pending/InProgress]
    C --> D[Buat step untuk Level 1\nresolve approver: Role/Position/\nSpecificUser/DirectSuperior]
    D --> E{Approver sedang\npunya delegasi aktif?}
    E -->|Ya| F[Step baru untuk Delegate\nstep asal ditandai Delegated]
    E -->|Tidak| G[Step tetap ke approver asli]
    F --> H{ApprovalType di level ini}
    G --> H

    H -->|Sequential| S1[Satu approver, satu giliran]
    H -->|Parallel| S2[Semua approver di level\naktif bersamaan]
    H -->|AnyOne| S3[Semua approver di level\naktif bersamaan]

    S1 --> ACT{Aksi approver}
    S2 --> ACT
    S3 --> ACT

    ACT -->|Reject| RJ([Status = Rejected\nstep sisanya dinonaktifkan])
    ACT -->|Approve| CHK{Kuorum level ini terpenuhi?\nSequential/Parallel: semua approve\nAnyOne: cukup satu}
    ACT -->|Lewat SLA / DueAt| ESC{Ada EscalateToLevelId?}

    ESC -->|Ya| D
    ESC -->|Tidak| ALERT[Tandai overdue\nalert Super Admin]
    ALERT --> ACT

    CHK -->|Belum - AnyOne: skip step saudara| WAIT[Tunggu approver lain\naktif di level sama]
    WAIT --> ACT
    CHK -->|Sudah, masih ada level berikutnya| D
    CHK -->|Sudah, ini level terakhir| AP([Status = Approved])

    RJ --> CB[Panggil IApprovalCallbackService\nOnRejectedAsync]
    AP --> CB2[Panggil IApprovalCallbackService\nOnApprovedAsync]
    Z1 --> CB2

    CB --> CBDISP{Dispatch berdasar\nReferenceType}
    CB2 --> CBDISP
    CBDISP --> FA[fa_asset_transfers /\nfa_disposals]
    CBDISP --> PRL[hr_payroll_runs]
    CBDISP --> PRC[pur_purchase_orders]
    CBDISP --> FIN[fin_journal_entries]

    FA --> AUD[Tulis apv_approval_audit_logs\nappend-only, tiap transisi]
    PRL --> AUD
    PRC --> AUD
    FIN --> AUD
    RJ -.-> AUD
    C -.->|created| AUD

    C -.->|requester bisa Cancel\nhanya saat masih level 1| CANCEL[Requester Cancel]
    CANCEL --> CX([Status = Cancelled])
    CX --> CB3[Panggil IApprovalCallbackService\nOnCancelledAsync]
    CB3 --> AUD
```

Jangan Tertukar dengan Modul Lain
===================================
Ada DUA entitas approval-matrix yang TIDAK ADA HUBUNGANNYA dengan modul
General Approval di atas — jangan disangka bagian dari APV:
- `PurApprovalConfig` (Purchasing) — `ERP.Domain/Entities/Purchasing/
  PurApprovalConfig.cs`: matriks approval per level (DocumentType, Level,
  MinAmount/MaxAmount, ApproverEmployeeId). Punya CRUD sendiri
  (`ERP.API/Controllers/v1/Purchasing/ApprovalConfigsController.cs`,
  menu `/purchasing/approval-configs`), langsung ke `AppDbContext` tanpa
  service layer. **Tidak dipakai oleh transaksi apapun** — entity
  PurchaseRequisition/PurchaseOrder yang seharusnya memicu approval ini
  belum ada di modul Purchasing (baru ada master data: BuyerGroup,
  VendorCategory, dll).
- `SalApprovalConfig` (Sales) — pola sama persis di modul Sales (menu
  `/sales/approval-configs`), juga belum ada transaksi Quotation/Sales
  Order yang memicunya.
- Enum `Purchasing.ApprovalStatus` dan `Sales.SalesApprovalStatus` — ada
  di kode tapi TIDAK DIPAKAI di manapun (dead code), sisa dari desain
  awal sebelum diputuskan pakai modul General Approval terpusat.

Catatan Gap Implementasi (Rencana vs Kenyataan)
==================================================
| Layer | Direncanakan | Kenyataan di Kode |
|---|---|---|
| Domain entities (7 tabel) | Lengkap | **Lengkap** (`ERP.Domain/Entities/Approval/*.cs`) |
| Enum | 6 enum | Ada semua, sudah final |
| DTO | ~9 file per-concern | 1 file gabungan, strukturnya sudah sesuai rencana |
| Migration / tabel Postgres | 7 tabel `apv_*` | **Lengkap**, sudah diterapkan (`20260713100000_AddGeneralApproval`) |
| Application services | Template/Request/RoutingEngine/Notification/Delegation/Report | **Lengkap** — 4 service (`ApprovalTemplateService`, `ApprovalRequestService`, `ApprovalDelegationService`, `ApprovalReportService`) menggantikan 8 interface yang direncanakan docx (beberapa digabung, mis. RoutingEngine+Action jadi satu `ApprovalRequestService`) |
| API controllers | Full REST, 8 controller | **Lengkap**, 7 controller (route sesuai kontrak `ApprovalApiClient` persis) |
| Web layer | Dashboard/Inbox/My Requests/Delegations/Templates/Levels/Reports | Lengkap dibangun sebelumnya, sekarang **benar-benar jalan** karena API-nya sudah ada |
| Hangfire (job terjadwal) | Eskalasi/reminder tiap 30 menit | **Lengkap**, recurring job `approval-escalation-reminders` |
| MailKit (email) | Kirim email approval | **Lengkap**, pakai config `Smtp` yang sudah ada (dipakai bersama fitur lupa-password) |
| SignalR (push real-time) | Notifikasi real-time | **Server lengkap** (`ApprovalHub` di `/hubs/approval`); **client JS di Razor belum dipasang** |
| DataSeeder | Module + menu + permission + seed template/level | **Lengkap** — permission ikut mekanisme `CfgRoleMenuPermission` yang sudah ada (bukan seed `apv.*` terpisah seperti rencana docx), 7 template + level default ter-seed |
| Integrasi ke modul lain | Callback pattern via `IApprovalCallbackService` per reference_type | **HR Leave Request sudah terintegrasi penuh** (`LeaveRequestApprovalCallbackService`) — integrasi pertama & satu-satunya sejauh ini. Fixed Assets/Payroll/Purchasing/Finance belum memanggil `SubmitAsync` atau mendaftarkan `IApprovalCallbackService` |

Acuan Implementasi
=====================
- Dokumen desain lengkap (rencana awal, sebagian sudah disesuaikan dengan
  kenyataan kode — lihat catatan koreksi di bagian 6-7 di atas):
  `SINARA_ERP_GeneralApproval_Panduan_Detail.docx` (root repo)
- Domain entities: `ERP.Domain/Entities/Approval/*.cs` (7 file)
- Enum: `ERP.Domain/Enums/Approval/ApprovalType.cs`,
  `ApprovalApproverType.cs`, `ApprovalRequestStatus.cs`,
  `ApprovalStepAction.cs`, `ApprovalNotificationChannel.cs`,
  `ApprovalNotificationType.cs`
- DTO: `ERP.Application/DTOs/Approval/ApprovalDtos.cs`
- Migration: `ERP.Infrastructure/Migrations/20260713100000_AddGeneralApproval.cs`,
  konfigurasi EF di `ERP.Infrastructure/Data/AppDbContext.cs` (7
  `Configure*` method + 7 `DbSet<>`)
- Application services: `ERP.Application/Services/Approval/*.cs`
  (`IApprovalTemplateService`/`ApprovalTemplateService`,
  `IApprovalRequestService`/`ApprovalRequestService` — routing engine +
  job eskalasi, `IApprovalDelegationService`/`ApprovalDelegationService`,
  `IApprovalReportService`/`ApprovalReportService`,
  `IApprovalCallbackService` — interface saja,
  `IApprovalNotificationService` — interface saja, implementasi di API)
- API: `ERP.API/Controllers/v1/Approval/*.cs` (7 controller),
  `ERP.API/Services/ApprovalNotificationService.cs`,
  `ERP.API/Hubs/ApprovalHub.cs`, wiring Hangfire/SignalR/JWT-for-hub di
  `ERP.API/Program.cs`
- Web layer (SUDAH ADA sebelumnya, sekarang fungsional):
  `ERP.Web/Controllers/Approval/ApprovalController*.cs` (8 file: induk +
  Dashboard/Inbox/Requests/Delegations/Templates/Levels/Reports),
  `ERP.Web/ViewModels/Approval/ApprovalViewModels.cs`,
  `ERP.Web/Views/Approval/**/*.cshtml`,
  `ERP.Web/Services/ApprovalApiClient.cs` + `IApprovalApiClient.cs`
- Seed module, menu, template, level, backfill:
  `ERP.Infrastructure/Data/DataSeeder.cs` — module "General
  Approval"/"APV", menu tree, `SeedApprovalTemplatesAsync` (template +
  level default), `BackfillLeaveRequestApprovalsAsync` (catch-up utk leave
  request Pending dari sebelum integrasi ini ada), permission ikut
  `SeedSuperAdminPermissionsAsync` yang generik untuk semua menu.
- Integrasi HR Leave Request (lihat bagian "Integrasi Modul HR Leave
  Request" di atas untuk detail):
  `ERP.Application/Services/HR/LeaveService.cs` (`SubmitAsync`,
  `GetRequestsAsync`/`GetByIdAsync` param `currentUserId` → `CanApprove`),
  `ERP.Application/Services/HR/LeaveRequestApprovalCallbackService.cs`,
  `ERP.Application/Services/HR/LeaveAttendanceSyncHelper.cs`,
  `ERP.Application/Services/Approval/ApprovalRequestService.cs`
  (`FindActiveRequestIdAsync`, `GetActionablePermissionsAsync`),
  `ERP.API/Controllers/v1/HR/LeaveRequestsController.cs` (`Approve`/`Reject`/
  `Get`/`GetSelf`/`GetById`),
  `ERP.Web/Views/HR/HrLeaveRequests/Index.cshtml` + `Details.cshtml`
  (gating tombol lewat `CanApprove`),
  `ERP.Web/Services/ApprovalReferenceLinkResolver.cs` (link Inbox → detail
  leave request),
  `ERP.Application/Services/Document/DocumentService.cs`
  (`EnsureLeaveRequestAccessAsync` — fix akses Super Admin/HR Manager/HR
  Staff, lihat `ReadMeDocumentGeneral.md` untuk detail).
- Entitas approval-matrix TIDAK TERKAIT (jangan disangka bagian APV):
  `ERP.Domain/Entities/Purchasing/PurApprovalConfig.cs`,
  `ERP.Domain/Entities/Sales/SalApprovalConfig.cs`,
  `ERP.API/Controllers/v1/Purchasing/ApprovalConfigsController.cs`,
  `ERP.API/Controllers/v1/Sales/ApprovalConfigsController.cs`.
