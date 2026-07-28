# Manufacturing Module

Modul produksi: BOM & routing, work center, work order eksekusi, MRP, quality control, scrap, dan rework, plus laporan produksi (output, OEE, cost variance, scrap analysis, capacity).

## 1. Entitas & Enum Domain (`ERP.Domain/Entities/Manufacturing`, `ERP.Domain/Enums/Manufacturing`)

- **`MfgBom`** (Bill of Materials) — Code, ItemId, RoutingId, EffectiveDate, Version, `Status` (`BomStatus`: Draft/Active/Obsolete), QtyProduced, StandardCost, IsActive; hanya boleh satu BOM Active per Item.
- **`MfgRouting`** — Code, Name, Version, `Status` (`RoutingStatus`: Draft/Active/Obsolete), ItemId, WorkCenterId, TotalLeadTimeHours, IsActive.
- **`MfgWorkCenter`** — Code, Name, CapacityHoursPerDay, LaborCostPerHour, OverheadCostPerHour, WipAccountId (→ Finance), IsActive.
- **`MfgWorkOrder`** — Code, ItemId, BomId, RoutingId, WorkCenterId, MrpRunId, `Status` (`WorkOrderStatus`: Draft/Planned/Released/InProgress/Completed/Closed/Cancelled), `ProductionType` (MakeToStock/MakeToOrder/BatchProduction/Rework), QtyPlanned/QtyGood/QtyScrap, PlannedStart/EndDate, ActualStart/EndAt, StandardCostTotal/ActualCostTotal, IsActive.
- **`MfgMrpRun`** — Code, RunDate, `Status` (`MrpStatus`: Draft/Running/Completed/Cancelled), HorizonDays, TotalDemandItems, RecommendedWoCount/RecommendedPrCount, Started/CompletedAt.
- **`MfgQcParameter`** — Code, Name, ItemId, `ParameterType` (Numeric/Boolean), MinValue/MaxValue, IsCritical, IsActive.
- **`MfgQcInspection`** — Code, WorkOrderId, ItemId, InspectorEmployeeId, InspectedAt, `Status` (`QcStatus`: Pending/InProgress/Passed/Failed/ConditionalPass/Cancelled), `Result` (`QcResult`: Pass/Fail/ConditionalPass).
- **`MfgScrapRecord`** — Code, WorkOrderId, ItemId, WorkCenterId, `Reason` (`ScrapReason`: Defective/MachineFault/OperatorError/MaterialDefect/DesignChange/Other), QtyScrap, UnitCost, RecordedAt.
- **`MfgReworkOrder`** — Code, SourceWorkOrderId, WorkOrderId (target), ItemId, QtyRework, `Status` (`WorkOrderStatus`), OpenedAt/ClosedAt.
- **`MfgOeeSnapshot`** — per work center per tanggal: availability/performance/quality/OEE total (lihat §6 — tidak ada alur untuk generate baris baru selain seed).

## 2. API Endpoints (`ERP.API/Controllers/v1/Manufacturing`, semua `[Authorize]` via `ManufacturingControllerBase`)

| Controller | Endpoint utama |
|---|---|
| `BomsController` | CRUD; hanya satu BOM Active per Item (`ValidateBomAsync`); delete diblokir jika direferensikan Work Order |
| `RoutingsController` | CRUD; delete diblokir jika direferensikan Work Order/BOM |
| `WorkCentersController` | CRUD; delete diblokir jika direferensikan Routing/WorkOrder/Scrap/OeeSnapshot |
| `WorkOrdersController` | CRUD (ItemId di BOM harus sama dengan ItemId WO), `/release`, `/start`, `/complete`, `/close`, `/cancel`; WO Closed/Cancelled tidak bisa diedit; delete hanya untuk Draft/Planned tanpa QC/Scrap/Rework terkait |
| `MrpController` | CRUD (hanya Draft yang editable/deletable), `/run` (Draft→Running), `/complete` (hanya dari Running), `/cancel` |
| `QcController` | CRUD, `POST {id}/complete` (menerima `Result`: Pass/Fail/ConditionalPass — **tidak ada endpoint Pass/Fail terpisah**, Web hanya wrapper tipis ke endpoint ini), `/cancel`; Completed/Cancelled tidak bisa diedit, hanya Pending yang bisa dihapus |
| `QcParametersController` | CRUD untuk parameter quality check per item |
| `ScrapController` | CRUD; **efek samping**: create/update/delete meng-adjust `QtyScrap` di Work Order terkait (`AdjustWorkOrderScrapAsync`) |
| `ReworkController` | CRUD, `/start`, `/complete`, `/close`, `/cancel`; Item auto-derive dari Target WO, fallback ke Source WO kalau target kosong (ditegakkan di server) |
| `ReportsController` | `production-output`, `oee`, `cost-variance`, `scrap-analysis`, `capacity` — semua query EF Core riil, bukan placeholder |
| `DashboardController` | `GET dashboard` — 5 KPI riil: ActiveWorkOrderCount, OpenMrpRunCount, PendingQcCount, TotalScrapCost, AverageOeePct |

## 3. Halaman Web (`ERP.Web/Controllers/ManufacturingController*.cs`, Views di `Manufacturing/`)

- **Dashboard** (`/manufacturing`) — 5 KPI card.
- **Production Execution**: Work Orders (`/manufacturing/work-orders`, + create/edit/release/start/complete/close/cancel/delete), MRP (`/manufacturing/mrp`, + create/edit/run/complete/cancel/delete), Quality Control (`/manufacturing/qc`, + create/edit/start/pass/fail/cancel/delete), Scrap (`/manufacturing/scrap`, + create/edit/delete), Rework (`/manufacturing/rework`, + create/edit/start/complete/close/cancel/delete).
- **Manufacturing Reports**: Production Output, OEE Report, Cost Variance, Scrap Analysis, Capacity (semua di `/manufacturing/reports/*`).
- **Manufacturing Master**: BOMs, Routings, Work Centers, QC Parameters (`/manufacturing/qc/parameters`).
- Semua halaman pakai `PagedGrid` ViewComponent; filter mengikuti query parameter URL dan bertahan saat paging/sorting.
- Form master & transaksi memakai dropdown lookup (`SearchableSelect`) untuk Item, BOM, Routing, Work Center, Work Order, MRP Run, Inspector — bukan input ID manual.

## 4. Business Rules / Logic Penting

- **BOM**: hanya boleh ada satu BOM Active per Item pada satu waktu.
- **Work Order**: ItemId di BOM yang dipilih harus sama dengan ItemId WO; WO Closed/Cancelled tidak bisa diedit; delete hanya untuk status Draft/Planned dan tidak ada QC/Scrap/Rework terkait.
- **MRP Run**: hanya status Draft yang bisa diedit/dihapus; transisi Run hanya dari Draft→Running, Complete hanya dari Running.
- **QC**: "Pass"/"Fail" di Web cuma wrapper tipis yang memanggil API `POST qc/{id}/complete` dengan `Result` berbeda — tidak ada endpoint Pass/Fail terpisah di level API. Inspeksi Completed/Cancelled tidak bisa diedit; hanya status Pending yang bisa dihapus.
- **Scrap**: create/update/delete otomatis meng-adjust field `QtyScrap` di Work Order terkait — bukan catatan berdiri sendiri, memengaruhi angka Work Order secara langsung.
- **Rework**: Item mengikuti Target WO; kalau Target WO kosong, fallback ke Source WO — fallback ditegakkan di server/controller, bukan cuma client-side.
- **Delete guard cross-reference**: Work Center/Routing/BOM tidak bisa dihapus kalau masih direferensikan Routing, Work Order, Scrap, atau OeeSnapshot.
- **Dropdown dependent**: form Work Order pakai mekanisme `DependsOnElementId`/`GroupValue` bawaan komponen `SearchableSelect` untuk filter BOM/Routing berdasarkan Item; form QC/Scrap/Rework pakai sedikit JS tambahan untuk auto-pilih Item dari Work Order (pola beda — auto-pilih satu nilai, bukan filter list berdasar grup).

## 5. Relasi Kunci

- `MfgWorkOrder` → `InvItem`, `MfgBom`, `MfgRouting`, `MfgWorkCenter`, `MfgMrpRun` (semua via FK, sebagian opsional).
- `MfgBom` → `InvItem`, `MfgRouting`; `MfgRouting` → `InvItem`, `MfgWorkCenter`.
- `MfgQcInspection`/`MfgScrapRecord`/`MfgReworkOrder` → `MfgWorkOrder` + `InvItem`; `MfgQcInspection` → `HrEmployee` (Inspector).
- `MfgOeeSnapshot` → `MfgWorkCenter` per tanggal.
- `MfgWorkCenter` → akun GL Finance (`WipAccountId`).

## 6. Known Gaps / Belum Lengkap

- **Tidak ada endpoint untuk membuat/update `MfgOeeSnapshot`** (tidak ada OeeController atau action snapshot di WorkCenters/WorkOrders) — satu-satunya sumber data OEE adalah seed data di `DataSeeder.cs`. Akibatnya, halaman OEE Report dan KPI `AverageOeePct` di Dashboard cuma menampilkan angka seed/basi — tidak ada alur kerja untuk generate snapshot baru dari aktivitas produksi sungguhan.
- **Formula `UtilizationPct` di laporan Capacity patut dicurigai**: `ReportsController.Capacity()` menghitung `UtilizationPct = GoodQtyTotal * 100 / CapacityHoursPerDay` — membagi jumlah qty dengan angka jam/hari, bukan perhitungan utilisasi berbasis jam (jam terpakai / jam tersedia). Ini terlihat seperti formula placeholder/keliru, bukan utilisasi kapasitas yang sesungguhnya.
- Selain dua hal di atas, tidak ditemukan TODO/NotImplementedException/stub di controller API maupun Web Manufacturing — kelima halaman Reports semuanya query EF Core riil terhadap `MfgWorkOrders`/`MfgOeeSnapshots`/`MfgScrapRecords`/`MfgWorkCenters`, bukan data dummy.
