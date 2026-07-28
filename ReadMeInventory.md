# Inventory Module

Modul manajemen persediaan: item master, warehouse & lokasi, stock balance/valuasi, serta transaksi stok (goods receipt, goods issue, transfer, adjustment, stock opname/physical count).

## 1. Entitas & Data (`ERP.Domain/Entities/Inventory`)

**Master data**
- `InvItem` — code/SKU/name, `CategoryId`, `BrandId`, `Type` (enum), `BaseUomId`/`PurchaseUomId`, `Status`, `ValuationMethod`, `LastPurchasePrice`, `AvgCost`, `MinStock`/`MaxStock`/`ReorderPoint`, `LeadTimeDays`, link ke akun Finance (`InventoryAccountId`, `CogsAccountId`, `AdjustmentAccountId`).
- `InvItemCategory` — self-reference (`ParentCategoryId` → hierarki tree).
- `InvBrand` — nama/deskripsi sederhana.
- `InvUnitOfMeasure` — code/name; direferensikan sebagai BaseUom/PurchaseUom di item.
- `InvItemUnitConversion` — per item, `FromUomId` → `ToUomId` dengan `ConversionFactor`.

**Warehousing**
- `InvWarehouse` — code/name/address, opsional Manager (HR employee) dan CostCenter (Finance), flag `IsTransit`.
- `InvWarehouseLocation` — bin/lokasi per warehouse, flag `IsDefault`.

**Status stok**
- `InvStockBalance` — per Item+Warehouse+Location: `QtyOnHand`, `QtyReserved`, `QtyAvailable`, `AvgCost`, `TotalValue`, `LastMovementAt`.
- `InvStockMovement` — baris ledger immutable per pergerakan: tanggal, item/warehouse/location, `MovementType`, QtyIn/QtyOut/QtyBalance, UnitCost/TotalCost, `SourceTable`/`SourceId`/`SourceLineId` (jejak audit balik ke dokumen asal).

**Transaksi (header + lines)**
- `InvGoodsReceipt`/`InvGoodsReceiptLine` — tipe receipt, nama supplier, status, received/confirmed by, opsional link JournalEntry.
- `InvGoodsIssue`/`InvGoodsIssueLine` — tipe issue, department/cost center, requested/issued/confirmed by.
- `InvStockTransfer`/`InvStockTransferLine` — From/To warehouse & location.
- `InvStockAdjustment`/`InvStockAdjustmentLine` — reason, requested/approved/confirmed by.
- `InvStockOpname`/`InvStockOpnameLine` — physical count: QtySystem/QtyCounted/QtyVariance/TotalVarianceValue, counted/approved by, link ke `AdjustmentId` yang di-generate.

**Enum** (`ERP.Domain/Enums/Inventory`)
- `ItemType`: Product, RawMaterial, Asset, Consumable, Service.
- `ItemStatus`: Active, Discontinued, Draft.
- `ValuationMethod`: WeightedAverageCost, FIFO (catatan: hanya logic weighted-average yang benar-benar diimplementasi — lihat §4).
- `GoodsReceiptType`: PurchaseReceipt, InternalReceipt, OpeningBalance, ReturnFromIssue.
- `GoodsIssueType`: DepartmentalUse, ProductionUse, MaintenanceUse, Disposal, ReturnToVendor.
- `AdjustmentReason`: DamageOrExpiry, DataCorrection, Theft, ProductionWaste, FoundItem, Other.
- `TransactionStatus`: Draft, Confirmed, Cancelled (dipakai receipt/issue/transfer/adjustment).
- `OpnameStatus`: Draft, InProgress, Completed, Cancelled, Approved.
- `StockMovementType`: GoodsReceipt, GoodsIssue, TransferIn/Out, AdjustmentIn/Out, OpnameIn/Out, OpeningBalance.

## 2. API Endpoints (`ERP.API/Controllers/v1/Inventory`)

| Controller | Endpoint utama |
|---|---|
| `ItemsController` | Paged list dengan filter kaya, `low-stock` (QtyAvailable ≤ MinStock), `options`, CRUD; auto-generate kode `ITEM-yyyyMM-####`; blokir delete jika item punya conversion/stock balance |
| `CategoriesController`, `BrandsController`, `UnitsController` | CRUD standar + endpoint `options` masing-masing |
| `ItemConversionsController` | CRUD faktor konversi UOM per item |
| `WarehousesController` | CRUD warehouse; nested `{warehouseId}/locations` CRUD + `options`; `{warehouseId}/stock` view |
| `StockController` | `balance` (paged on-hand/available/value), `available` (lookup tunggal), `movements`/`card` (ledger pergerakan), `valuation` |
| `StockBalancesController` | Listing sederhana stock balance |
| `GoodsReceiptsController` | CRUD (edit/delete hanya saat Draft), `confirm` (posting stok IN via mutation helper, tipe OpeningBalance → `StockMovementType.OpeningBalance`), `cancel`, `print` (stub, belum diimplementasi). Auto-generate `GR-yyyyMM-####` |
| `GoodsIssuesController` | CRUD, `confirm` (cek qty available lalu posting stok OUT), `cancel`, `print` (stub). Auto-generate `GI-yyyyMM-####`. QtyIssued tidak boleh melebihi QtyRequested |
| `TransfersController` | CRUD, `confirm` (cek available di sumber, posting TransferOut di sumber lalu TransferIn di tujuan dengan cost dari movement keluar), `cancel` |
| `AdjustmentsController` | CRUD, `approve` (wajib hanya jika ada baris qty negatif), `confirm` (blokir adjustment negatif sebelum di-approve; cek stok available untuk baris negatif; posting AdjustmentIn/Out), `cancel` |
| `OpnamesController` | CRUD, `start` (auto-populate baris count dari snapshot `InvStockBalance` saat ini jika belum ada baris, set QtyCounted=QtyOnHand default), `lines`/`lines/{lineId}` (update qty count saat InProgress), `complete`, `approve` (auto-create `InvStockAdjustment` berstatus Draft dari baris variance, hanya jika `QtyVariance != 0`; link balik via `AdjustmentId`), `cancel` |
| `ReportsController` | `stock-summary`, `stock-by-warehouse`, `stock-by-category`, `stock-card`, `low-stock`, `inventory-valuation`, `inventory-aging`, `movement-history`, `receipt-summary`, `issue-summary`, `transfer-summary`, `adjustment-summary`, + varian export untuk stock-card/valuation/low-stock |
| `InventoryStockMutationHelper` | (internal, dipakai bersama semua action posting) `GetAvailableAsync` (baca `InvStockBalance.QtyAvailable`), `ApplyMovementAsync` (lihat §4) |

## 3. Halaman Web (`ERP.Web/Views` & `Controllers/Inventory`)

`InventoryController` partial class dipecah per fitur: Items, Categories, Brands, Units, ItemConversions, Warehouses (+WarehouseDetails untuk locations/stock), GoodsReceipts, GoodsIssues, Transfers, Adjustments, Opnames, Reports.

Views yang ada: Items, Categories, Brands, ItemConversions (Index/Create/Edit masing-masing); Warehouses (Index/Create/Edit/Locations/CreateLocation/EditLocation/Stock); GoodsReceipts/GoodsIssues/Transfers/Adjustments/Opnames (Index/Create/Edit); Reports (StockSummary, LowStock, Valuation, Aging, MovementHistory, ReceiptSummary, IssueSummary, TransferSummary, AdjustmentSummary).

Web controller murni jadi client MVC yang memanggil ERP.API via `IInventoryApiClient` (plus `IHrApiClient` untuk opsi employee/department dan `IFinanceApiClient` untuk cost center/akun GL) — tidak ada akses DB/business logic langsung di ERP.Web.

## 4. Business Rules / Logic Penting

- **Posting stok tersentralisasi** di `InventoryStockMutationHelper.ApplyMovementAsync`, dipanggil oleh semua action "confirm" (receipt, issue, transfer, adjustment). Fungsi ini melakukan upsert `InvStockBalance` dan menambahkan baris audit `InvStockMovement` dalam satu pemanggilan yang sama.
- **Valuasi selalu weighted-average di praktiknya**: saat stok bertambah, `AvgCost = (currentQty*AvgCost + qtyDelta*unitCost) / newQty`; saat berkurang, AvgCost tidak berubah dan movement di-cost pakai `balance.AvgCost` yang ada (kecuali unitCost positif eksplisit diberikan). Meski `ValuationMethod.FIFO` ada sebagai opsi enum di `InvItem`, tidak ada logic costing FIFO-layer di helper ini — FIFO secara efektif belum diimplementasi/hanya kosmetik.
- **Stok tidak boleh negatif** — throw jika hasil qty < 0, dan action confirm issue/transfer/adjustment-negatif melakukan pre-check `QtyAvailable` (on-hand dikurangi reserved) sebelum posting.
- **Alur Draft/Confirmed/Cancelled**: Receipt, Issue, Transfer, Adjustment hanya bisa diedit/dihapus saat Draft; `confirm` melakukan posting stock movement dalam satu DB transaction (rollback jika gagal) dan mengunci record; `cancel` hanya boleh dari Draft.
- **Adjustment butuh approval untuk baris negatif** (pengurangan) sebelum bisa di-confirm (`ApprovedBy` harus terisi).
- **Alur Opname (physical count)**: Draft → start (snapshot balance saat ini jadi baris count) → InProgress (counter mengedit QtyCounted) → Complete → Approve (auto-generate `InvStockAdjustment` berstatus Draft dari baris variance, reason=DataCorrection) → adjustment tsb tetap harus di-confirm terpisah supaya benar-benar posting perubahan stok.
- **Low stock** = `QtyAvailable <= MinStock` (dihitung via endpoint `low-stock` di ItemsController dan Reports).
- **Faktor konversi unit** disimpan per item (`InvItemUnitConversion.ConversionFactor`) tapi baris transaksi membawa qty transaksi (mis. `QtyReceived`) DAN `QtyBase` yang dinormalisasi (fallback ke qty transaksi jika tidak diberikan) — `QtyBase` inilah yang benar-benar mengalir ke stock movement/balance.
- **Konvensi penomoran**: `ITEM-yyyyMM-####`, `GR-yyyyMM-####`, `GI-yyyyMM-####` (transfer/adjustment/opname mengikuti pola sekuens per-bulan serupa).
- Endpoint print di Goods Receipts/Issues masih stub, return "not implemented yet."

## 5. Relasi Kunci

- `InvItem` → `InvItemCategory` (many-to-one, category self-hierarchical), → `InvBrand` (opsional), → `InvUnitOfMeasure` (Base + opsional Purchase UOM), → akun `FinAccount` (GL Inventory/COGS/Adjustment).
- `InvItemUnitConversion` → `InvItem` + dua `InvUnitOfMeasure` (From/To).
- `InvStockBalance` → `InvItem` + `InvWarehouse` + opsional `InvWarehouseLocation` (kombinasi unik yang dipakai di mana-mana untuk lookup stok).
- `InvWarehouseLocation` → `InvWarehouse`; `InvWarehouse` → opsional Manager HR dan CostCenter Finance.
- Semua header transaksi (Receipt/Issue/Transfer/Adjustment/Opname) → `InvWarehouse`(+Location), dan opsional → `FinJournalEntry` (link posting GL) dan `SysUser` (requested/issued/approved/confirmed by).
- Baris transaksi → `InvItem` (+ opsional `InvUnitOfMeasure`); `InvStockOpname` → `InvStockAdjustment` (link satu arah setelah di-approve).
- `InvStockMovement` mereferensikan balik ke dokumen sumber secara generik lewat `SourceTable`/`SourceId`/`SourceLineId`, bukan FK per tipe dokumen.

## 6. Known Gaps / Belum Lengkap

- **FIFO valuation belum diimplementasi** — `ValuationMethod.FIFO` ada sebagai opsi di `InvItem`, tapi `InventoryStockMutationHelper` hanya punya logic weighted-average; item yang diset FIFO tetap akan dihitung pakai weighted-average tanpa peringatan/validasi.
- **Print dokumen belum diimplementasi** — endpoint `print` di `GoodsReceiptsController` dan `GoodsIssuesController` masih stub, return "not implemented yet."
- **Export laporan belum diimplementasi** — endpoint `stock-card/export`, `inventory-valuation/export`, dan `low-stock/export` di `ReportsController` semuanya stub, return "Export is not implemented yet." (beda dengan modul Finance yang exportnya sudah jalan).
- **Reservasi stok (`QtyReserved`) ada di skema `InvStockBalance`** tapi tidak ditemukan alur yang benar-benar mengisi/mengurangi field ini dari sisi Inventory sendiri — kemungkinan diisi dari modul lain (Sales/Manufacturing) atau memang belum dipakai.
- Tidak ada integrasi eksplisit ke Purchasing (Goods Receipt tidak mereferensikan Purchase Order karena PO belum ada di modul Purchasing — lihat `ReadMePurchasing.md`).
