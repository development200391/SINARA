# Fixed Assets Module

Modul manajemen aset tetap: asset register, kategori & lokasi aset, depresiasi (config, run, schedule), transfer antar lokasi/departemen, disposal, revaluasi, dan maintenance order.

## 1. Entitas & Enum (`ERP.Domain/Entities/FixedAssets`, prefix `Fa`)

- **`FaAsset`** — record register aset: code, name, category, location, department, tanggal acquisition/in-service, acquisition cost, salvage value, useful life (bulan), metode/rate depresiasi, accumulated depreciation, book value, status, serial number, vendor.
- **`FaAssetCategory`** — code/name, default metode depresiasi, useful life, rate depresiasi, dan 3 akun GL terkait (asset, accumulated depreciation, depreciation expense).
- **`FaLocation`** — code/name/address, opsional department dan manager (HR employee).
- **`FaDepreciationConfig`** — konfigurasi per fiscal-year: run day, flag `IsAutoPostJournal`, flag aktif (setting per-fiscal-year, tidak 1:1 dengan run).
- **`FaDepreciationRun`** — nomor run, period year/month, tanggal run, total jumlah/nilai aset, status, approver, `JournalEntryId` (FK nullable, ada di skema tapi tidak pernah diisi di kode).
- **`FaDepreciationSchedule`** — baris depresiasi per-aset per-periode (jumlah, accumulated, book value, status Pending/Processed), terhubung ke run setelah diproses.
- **`FaAssetTransfer`** — memindahkan aset antar lokasi/departemen, dengan alur approval.
- **`FaDisposal`** — record disposal (tipe, jumlah jual, biaya disposal, gain/loss).
- **`FaRevaluation`** — book value lama/baru, jumlah impairment.
- **`FaMaintenanceOrder`** — work order (preventive/corrective), biaya, flag capitalizable.
- **`FaAssetHistory`** — jejak audit event per aset (registrasi, depresiasi, transfer, maintenance, revaluasi, disposal, perubahan status).
- **`FaAssetDocument`** — file terlampir per aset.

**Enum**: `AssetStatus` (Draft/Active/InMaintenance/FullyDepreciated/Disposed), `DepreciationMethod` (StraightLine/DecliningBalance), `DepreciationRunStatus` (Draft/Processed/Approved), `DepreciationScheduleStatus` (Pending/Processed), `AssetTransferStatus` (Draft/Approved/Rejected), `DisposalType` (Sale/WriteOff/Scrap), `DisposalStatus` (Draft/Approved/Posted/Cancelled), `RevaluationStatus` (Draft/Approved/Posted), `MaintenanceType` (Preventive/Corrective), `MaintenanceStatus` (Open/InProgress/Completed/Cancelled), `AssetHistoryType`.

## 2. API Endpoints (`ERP.API/Controllers/v1/FixedAssets`, route `/api/v1/fixed-assets/...`, semua `[Authorize]`)

| Controller | Endpoint utama |
|---|---|
| `AssetsController` (`/assets`) | Paged list dengan filter (category, location, department, status, rentang book value/tanggal), `/options`, get-by-id (dengan schedule/transfer/maintenance/disposal/revaluasi/history), create/update/delete (blokir edit parameter depresiasi setelah schedule diproses; blokir delete jika ada transaksi). Saat create, auto-generate kode `FA-{categoryCode}-{seq}` dan membangun seluruh schedule depresiasi |
| `AssetCategoriesController` (`/asset-categories`) | CRUD + `/options`; validasi akun GL terkait ada; blokir delete jika masih dipakai |
| `LocationsController` (`/locations`) | CRUD + `/options`; blokir delete jika dipakai aset/transfer |
| `DepreciationConfigsController` (`/depreciation-configs`) | CRUD setting per-fiscal-year (run day, flag auto-post) |
| `DepreciationRunsController` (`/depreciation-runs`) | List/get; `POST /run` — memproses semua schedule Pending untuk satu periode jadi satu run (create/update run, tandai schedule Processed, update accumulated depreciation/book value/status aset, tulis history); `PUT /{id}/approve` |
| `DisposalsController` (`/disposals`) | CRUD (draft only); `/approve`, `/post` (hitung gain/loss = jual − biaya − book value, tandai aset Disposed/inactive/book value 0), `/cancel` |
| `RevaluationsController` (`/revaluations`) | CRUD (draft only); `/approve`, `/post` (set book value aset ke nilai baru, reaktivasi status), rekam history |
| `TransfersController` (`/transfers`) | CRUD (draft only); `/approve` (pindahkan aset ke lokasi/departemen baru, clear InMaintenance), `/reject` |
| `MaintenanceOrdersController` (`/maintenance-orders`) | CRUD; `/start`, `/complete` (jika `IsCapitalized`, tambahkan biaya ke acquisition cost & book value), `/cancel`; set status aset InMaintenance/Active sesuai alur |
| `DashboardController` (`/dashboard`) | KPI ringkasan — jumlah aset aktif, total acquisition cost, total book value, total depresiasi bulan berjalan, jumlah aset dalam maintenance, jumlah disposed |
| `FixedAssetsControllerBase` | Base bersama, menyediakan `GetCurrentUserId()` dari claims; semua controller `[Authorize]` |

## 3. Halaman Web (`ERP.Web/Controllers/FixedAssets/FixedAssetsController.*` + `Views/FixedAssets/**`)

`FixedAssetsController` partial class dipecah jadi: `Master` (categories/locations/depreciation configs), `Assets` (asset register + depreciation runs), `Operations` (transfer, maintenance order), `DisposalsRevaluations` (disposal, revaluasi). Route di bawah `/fixed-assets`.

Halaman: Dashboard (`Index`), Asset Categories, Locations, Depreciation Configs (Index/Create/Edit/Delete masing-masing), Assets (Index/Create/Edit/Delete/Detail dengan tab schedule, transfer, maintenance, disposal, revaluasi, history), Depreciation Runs (list + Run + Approve), Transfers (Create/Edit/Delete/Approve/Reject), Maintenance Orders (Create/Edit/Delete/Process/Cancel), Disposals (Create/Edit/Delete/Process/Cancel), Revaluations (Create/Edit/Delete/Process).

Web controller memanggil ERP.API via `IFixedAssetsApiClient`, dan mereferensikan silang API HR (department/employee) dan Finance (akun GL) untuk dropdown.

## 4. Business Rules

- **Perhitungan depresiasi**: straight-line = `(cost − salvage)/usefulLifeMonths` per bulan (periode terakhir menyerap sisa pembulatan); declining balance = `(book value awal × rate/100)/12`, dibatasi supaya book value tidak pernah turun di bawah salvage (rate default 20% jika tidak diset). Schedule dibangun untuk seluruh useful life saat aset dibuat/diupdate.
- Metode declining-balance mewajibkan `DepreciationRate` — divalidasi baik di API maupun Web.
- Mengedit field finansial aset (cost, salvage, life, method, rate, tanggal in-service) diblokir begitu ada satu periode schedule yang sudah Processed.
- **Depreciation run**: mengagregasi semua schedule Pending untuk satu periode ke `FaDepreciationRun`, update accumulated depreciation/book value tiap aset, otomatis ubah status ke `FullyDepreciated` saat book value ≤ salvage. Run bisa langsung di-approve atau di-approve terpisah belakangan. **Catatan**: meski ada flag `IsAutoPostJournal` di `FaDepreciationConfig` dan field `JournalEntryId` di run, tidak ada code path yang benar-benar membuat/menghubungkan journal entry (berbeda dengan modul AP/AR/Inventory yang benar-benar posting journal) — ini tampaknya masih stub/belum diimplementasi.
- **Disposal**: gain/loss = `saleAmount − disposalExpense − bookValueAtDisposal`, dihitung hanya saat Post (Approved→Posted); posting menandai aset Disposed, inactive, book value 0. Alur Draft→Approved→Posted/Cancelled.
- **Revaluasi**: saat Post, book value aset diset ke `NewBookValue`; aset yang sudah disposed tidak bisa direvaluasi atau ditransfer.
- **Maintenance**: membuat/memulai order menandai aset InMaintenance; menyelesaikan dengan `IsCapitalized=true` menambahkan biaya ke acquisition cost dan book value (kapitalisasi), lalu reaktivasi aset.
- **Transfer**: approve memindahkan `LocationId`/`DepartmentId` pada aset dan reaktivasi dari InMaintenance jika berlaku; lokasi asal/tujuan harus berbeda.
- Semua action yang mengubah state menambahkan record `FaAssetHistory` (idempoten — cek referensi yang sudah ada sebelum insert).

## 5. Relasi Kunci

- `FaAsset` → `FaAssetCategory` (mendefinisikan default metode depresiasi/useful life/rate dan 3 akun GL: asset, accumulated depreciation, depreciation expense).
- `FaAsset` → `FaLocation` (→ opsional `HrDepartment`, `HrEmployee` manager) dan opsional `DepartmentId` langsung.
- `FaAsset` → `FaDepreciationSchedule` (1:many, bulanan) → `FaDepreciationRun` (banyak schedule dikelompokkan per run setelah diproses).
- `FaAsset` → `FaAssetTransfer`, `FaDisposal`, `FaRevaluation`, `FaMaintenanceOrder`, `FaAssetHistory`, `FaAssetDocument` (semua 1:many, mengubah state balik ke aset secara cascading).

## 6. Known Gaps / Belum Lengkap

- **Depreciation run tidak pernah posting journal ke Finance** — `FaDepreciationConfig.IsAutoPostJournal` dan field `FaDepreciationRun.JournalEntryId` sudah ada di skema, tapi tidak ada code path yang benar-benar membuat `FinJournalEntry` saat run diproses/di-approve. Ini berbeda dari modul AP/AR/Inventory yang benar-benar posting journal — akibatnya biaya depresiasi tidak otomatis masuk ke General Ledger, harus dijurnal manual atau butuh pengembangan lanjutan.
- **Disposal & Revaluasi juga tidak posting journal** — gain/loss disposal dan perubahan book value dari revaluasi dihitung dan disimpan di entity `FaDisposal`/`FaRevaluation`, tapi tidak ditemukan pembuatan journal entry otomatis ke Finance untuk mencatat gain/loss atau selisih revaluasi ke GL.
- Tidak ditemukan laporan/print khusus (mis. depreciation schedule report, asset register report cetak) selain data mentah yang ditampilkan di halaman Detail aset.
