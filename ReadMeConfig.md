da# Config Module (System Configuration)

Modul ini mengatur konfigurasi inti aplikasi: modul & menu navigasi, role & permission, user management, pengaturan aplikasi, bahasa, dan audit log. Arsitektur mengikuti pola solusi: `ERP.API` (REST API), `ERP.Web` (MVC frontend yang memanggil API), `ERP.Application/Services/Config` (business logic), `ERP.Domain` (entities).

## 1. Entitas Domain (`ERP.Domain/Entities/Config` & `System`)

- **`CfgModule`** — grup navigasi tingkat atas: `Name`, `Code`, `Icon`, `SortOrder`, `IsActive`; punya banyak `CfgMenu`.
- **`CfgMenu`** — item menu: `ModuleId`, `ParentId` (self-reference, mendukung menu bertingkat), `Name`, `Url`, `Icon`, `SortOrder`, `IsActive`.
- **`CfgRole`** — `Name`, `Description`, `IsSystem`, `IsActive`; terhubung ke `CfgRoleMenuPermission` dan `SysUserRole`.
- **`CfgRoleMenuPermission`** — join entity role↔menu, menyimpan flag `CanView`, `CanCreate`, `CanEdit`, `CanDelete` per kombinasi.
- **`SysUser`** — `Username`, `PasswordHash`, `FullName`, `Email`, `Phone`, `AvatarUrl`, `LanguagePreference`, `IsActive`; terhubung ke role, dan lintas-modul (karyawan HR, approver leave request, payroll run, journal finance, dokumen).
- **`SysUserRole`** — join many-to-many `SysUser` ↔ `CfgRole`.
- **`SysAuditLog`** — `UserId`, `Username`, `Action`, `EntityName`, `EntityId`, `OldValues`/`NewValues` (JSON), `IpAddress`, `CreatedAt`.

> Catatan: Languages dan App Settings **tidak** disimpan sebagai entity/tabel — daftar bahasa (en/id) di-hardcode di controller, dan app settings hanya hidup di cache (lihat §4).

## 2. API Endpoints (`ERP.API/Controllers/v1/Config`)

Semua controller `[Authorize]`, mewarisi `ConfigControllerBase` (menyediakan `GetCurrentUserId()` dari JWT claims).

| Controller | Endpoint utama |
|---|---|
| `UsersController` | Paged list (search/sort by username/fullName/email/isActive), `options` (lookup), get by id, create (validasi email aktif, username/email unik, auto-generate password kuat 12 karakter + kirim email kredensial), update, delete |
| `RolesController` | List semua role, get permission matrix per role, update permission matrix (upsert/delete diff) |
| `MenusController` | Get by module, get by id, create/update (validasi parent harus 1 module, blokir circular parent), delete (blokir jika masih punya child), reorder (bulk update sort-order dalam 1 module) |
| `ModulesController` | List semua, update (name/icon/sortOrder/isActive) |
| `NavigationController` | Return navigation tree user yang login (module → menu, difilter permission `CanView` role-nya) |
| `SettingsController` | Get/update app settings (nama app, logo URL, default language) |
| `LanguagesController` | Daftar statis: English (`en`, default) dan Indonesian (`id`) |
| `AuditLogsController` | Paged & filterable query audit log |

## 3. Halaman Web (`ERP.Web/Controllers/Config`, `Views/Config`)

- **Users** (`ConfigUsersController`) — index (paged/sortable/filterable), Create/Edit (multi-select role), Details, Delete.
- **Menu Config** (`ConfigMenusController`) — index per-module dengan render baris hierarkis (indentasi tree), update pengaturan module inline, Create/Edit menu (dengan Icon Picker via `FormIconPickerViewComponent`), reorder (drag/drop-style), dropdown parent-menu yang mengecualikan diri sendiri/descendant, Delete.
- **Roles** (`ConfigRolesController`) — index + halaman Permissions matrix (per-role, menu × view/create/edit/delete checkbox).
- **Settings** (`ConfigSettingsController`) — halaman System Settings (nama app, logo, default language).
- **Languages** (`ConfigLanguagesController`) — halaman read-only daftar bahasa.
- **Audit Log** (`ConfigAuditController`) — filter (search, rentang tanggal, status/action, multi-select entity name, "has IP only"), paging/sorting, dan **export Excel** (stream semua halaman via `IAuditLogExcelExportService`, tulis ke temp file, streamed download lalu dihapus).
- Komponen **Icon Picker** (`Views/Shared/Components/FormIconPicker`) dipakai ulang di form menu.

## 4. Business Rules / Logic Penting (`ERP.Application/Services/Config`)

- **UserService** — pembuatan user mewajibkan email lolos `IUserCredentialEmailService.IsEmailActiveAsync`; enforce username/email unik (termasuk cek baris soft-deleted via `IgnoreQueryFilters`); generate password awal acak yang kuat (jamin ada huruf besar/kecil/angka/simbol) dan kirim kredensial via email — bukan menerima password dari client; assignment role dilakukan dengan diff `SysUserRole` (add/remove), bukan replace-all.
- **MenuService** — enforce parent menu harus 1 module yang sama; deteksi & blokir circular parent reference dan self-parenting; blokir delete menu yang masih punya child; membangun navigasi user dengan resolve semua menu `CanView` dari semua role user, lalu naik ke parent chain untuk menyertakan ancestor menu, lalu render tree per module; caching agresif dengan prefix invalidation eksplisit untuk menus, role permissions, dan navigasi per-user.
- **RoleService** — permission matrix mengembalikan semua menu aktif dengan flag default false jika belum ada baris permission; update permission melakukan diff row (add/update/delete) dan invalidasi cache role-level dan semua user-level (karena perubahan role memengaruhi semua user pemilik role tsb).
- **ModuleService** — CRUD sederhana + invalidasi cache daftar module dan cache permission user.
- **AppSettingsService** — settings **cache-only** (tidak ada tabel DB) — default `AppName="SINARA ERP"`, `DefaultLanguage="en"`; update menormalkan/fallback nilai invalid (nama app kosong → reset default, DefaultLanguage dibatasi ke "en"/"id").
- Caching (`ICacheService`) menyeluruh dengan key prefix (`ERP_cfg:permissions:role:`, `ERP_cfg:permissions:user:`, `ERP_cfg:menus:module:`, `ERP_cfg:modules:all`, `ERP_sys:settings:app`); semua panggilan cache dibungkus try/catch supaya cache down tidak pernah mematahkan fungsi inti.

## 5. Relasi Kunci

- `SysUser` ↔ `CfgRole` many-to-many via `SysUserRole`.
- `CfgRole` ↔ `CfgMenu` many-to-many via `CfgRoleMenuPermission`, membawa flag CRUD per relasi.
- `CfgMenu` self-reference (`ParentId`/`Children`) untuk tree menu bertingkat, dikelompokkan di bawah `CfgModule`.
- Navigasi efektif user = union semua menu `CanView` dari semua role-nya, ditambah ancestor menu lewat parent chain, dikelompokkan kembali di bawah module-nya.
- `SysAuditLog` mereferensikan `UserId`/`Username` secara longgar (tanpa FK navigation), merekam diff before/after JSON per entity.

## 6. Known Gaps / Belum Lengkap

- **Languages** murni hardcode 2 bahasa (en/id) di `LanguagesController` — tidak ada tabel/CRUD untuk menambah bahasa baru; halaman Web-nya juga read-only.
- **App Settings** cache-only, tidak ada tabel DB — jika cache di-flush/server restart tanpa persistensi, custom settings (nama app, logo, default language) yang pernah diubah bisa kembali ke default `SINARA ERP`/`en`.
- **Audit log** hanya menyimpan `UserId`/`Username` secara longgar tanpa FK navigation ke `SysUser` — tidak ada jaminan referential integrity (mis. user dihapus, log tetap ada tapi tanpa link balik yang solid).
- Tidak ditemukan endpoint/UI untuk reset password user secara mandiri (self-service "lupa password") — alur yang ada hanya generate password awal otomatis saat user dibuat oleh admin.
