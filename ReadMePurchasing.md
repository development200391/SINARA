# Purchasing Module

Modul pengadaan — saat ini berisi data master pendukung pembelian (vendor category, buyer group, approval config) dan approve/reject vendor. **Purchase Requisition, Purchase Order, dan RFQ belum diimplementasi** — baru sebatas scaffolding (lihat §1).

## 1. Entitas & Data (`ERP.Domain`)

**Entitas yang sudah diimplementasi** (`ERP.Domain/Entities/Purchasing`):
- `PurVendorCategory` — Code, Name, Description, IsActive; punya koleksi `FinVendor`.
- `PurBuyerGroup` — Code, Name, `BuyerEmployeeId` (→ HrEmployee), Description, IsActive; terhubung ke Vendor dan mapping `PurBuyerGroupCategory`.
- `PurBuyerGroupCategory` — join entity: BuyerGroupId + ItemCategoryId (→ InvItemCategory).
- `PurApprovalConfig` — DocumentType (enum), Level, MinAmount, MaxAmount, `ApproverEmployeeId` (→ HrEmployee), IsActive, Notes.

**Vendor master ada di namespace Finance, bukan Purchasing**: `FinVendor` (`ERP.Domain/Entities/Finance/FinVendor.cs`) — Code, Name, TaxId, Address, Phone, Email, ContactPerson, PaymentTermsDays, DefaultAccountId/DefaultTaxCodeId (Finance), VendorCategoryId, BuyerGroupId, IsApprovedVendor, ApprovedDate, LeadTimeDays, PerformanceScore, IsActive. Terhubung ke AP invoice/payment.

**Enum** (`ERP.Domain/Enums/Purchasing`): `PurchasingDocumentType` (PurchaseRequisition, PurchaseOrder), `ApprovalStatus` (Pending/Approved/Rejected/Delegated), plus yang sudah di-scaffold tapi belum dipakai: `PoStatus`, `PrStatus`, `PurchaseType`, `ReceiptStatus`, `ReturnReason`, `RfqStatus`, `PaymentTerms`.

**Belum diimplementasi (scaffolding saja)**: Tidak ada class entity `PurchaseRequisition`, `PurchaseOrder`, atau `Rfq`. Yang ada baru interface/read model (`IPurchaseOrderRepository`, `IPurchaseRequisitionRepository`, `IRfqRepository`, `PurchasingReadModels.cs` berisi `PurchaseRequisitionReadModel`/`PurchaseOrderReadModel`/`RfqReadModel`) dan `IPurchasingIntegrationService` (stub untuk pembuatan AP invoice/goods-receipt dari penerimaan PO). Ini menegaskan PR/PO/RFQ direncanakan untuk fase berikutnya, belum dibangun.

## 2. API Endpoints (`ERP.API/Controllers/v1/Purchasing`)

Semua controller mewarisi `PurchasingControllerBase` (`[Authorize]`, menyediakan `GetCurrentUserId()`).

| Controller | Endpoint utama |
|---|---|
| `VendorsController` | `GET` (paged, filter search/code/name/category/buyer group/approved/perf score/payment terms/active, sortable), `GET {id}`, `PUT {id}/set-approved` (toggle IsApprovedVendor + ApprovedDate). Tidak ada create/update/delete (CRUD vendor kemungkinan ada di sisi Finance) |
| `VendorCategoriesController` | Full CRUD + `GET options` (daftar aktif untuk dropdown). Enforce Code unik; blokir delete jika masih direferensikan vendor |
| `BuyerGroupsController` | Full CRUD + `GET options`. Validasi buyer employee ada, code unik, item category ID valid; mengelola mapping `PurBuyerGroupCategory` saat create/update; blokir delete jika masih direferensikan vendor |
| `ApprovalConfigsController` | Full CRUD. Validasi Level>0, MinAmount≥0, MaxAmount≥MinAmount, approver ada, dan tidak boleh ada kombinasi duplikat (DocumentType+Level+MinAmount+MaxAmount) |
| `DashboardController` | Return `PurchasingDashboardDto`; hanya `ApprovedVendorCount` yang riil (hitung `IsApprovedVendor`); `PendingPrApprovalCount`, `OverduePoCount`, `CurrentMonthPoAmount` masih hardcode 0 (placeholder menunggu implementasi PR/PO) |

## 3. Halaman Web (`ERP.Web/Controllers/Purchasing/PurchasingController.cs`, route `/purchasing`)

- Dashboard (`/purchasing`)
- Vendor Categories: list/create/edit/delete
- Buyer Groups: list/create/edit/delete (dengan multi-select item category)
- Approval Configs: list/create/edit/delete (dengan dropdown approver dari HR)
- Vendors: list (filter kaya), detail view, action Approve/Reject (memanggil `set-approved`) — tidak ada UI create/edit di sini (read-mostly)
- Menggunakan `IPurchasingApiClient` untuk memanggil API; ambil opsi employee via `IHrApiClient` dan opsi item category via `IInventoryApiClient`.

## 4. Business Rules / Logic Penting

- **Approval workflow (lokal-modul, sederhana)**: `PurApprovalConfig` mendefinisikan matriks approval multi-level berbasis threshold — per `PurchasingDocumentType`, per `Level`, band jumlah (Min/Max) dipetakan ke satu approver employee, divalidasi agar tidak overlap/duplikat. Tidak ditemukan integrasi ke modul "Approval" generik; enum `ApprovalStatus` ada tapi belum terhubung ke workflow engine manapun (karena PR/PO belum diimplementasi).
- **Buyer group ↔ item category**: many-to-many via `PurBuyerGroupCategory`; dipakai untuk filter vendor/buyer group berdasarkan category, dan kemungkinan untuk auto-routing PO berdasarkan item category di masa depan.
- **Approval vendor**: vendor punya action approve/reject eksplisit (`IsApprovedVendor` + `ApprovedDate`), terpisah dari matriks threshold approval-config.
- Guard delete mencegah penghapusan Vendor Category atau Buyer Group yang masih direferensikan vendor.

## 5. Relasi Kunci

- `FinVendor` → `PurVendorCategory` (opsional, many-to-one) dan → `PurBuyerGroup` (opsional, many-to-one).
- `PurBuyerGroup` → `HrEmployee` (BuyerEmployee, wajib) dan ↔ `InvItemCategory` (via `PurBuyerGroupCategory`).
- `PurApprovalConfig` → `HrEmployee` (ApproverEmployee), di-key oleh `PurchasingDocumentType` + `Level` + rentang jumlah.

## 6. Known Gaps / Belum Lengkap

Ini modul yang paling belum lengkap dibanding Config/Finance/Inventory/FixedAssets:

- **Purchase Requisition, Purchase Order, dan RFQ belum diimplementasi sama sekali** — hanya ada interface repository (`IPurchaseOrderRepository`, `IPurchaseRequisitionRepository`, `IRfqRepository`) dan read model kosong (`PurchasingReadModels.cs`); tidak ada entity, tidak ada controller API, tidak ada halaman Web untuk PR/PO/RFQ.
- **`IPurchasingIntegrationService`** (untuk membuat AP invoice / goods-receipt otomatis dari penerimaan PO) baru berupa stub interface — belum ada implementasi konkret, karena PO-nya sendiri belum ada.
- **Dashboard sebagian besar placeholder** — `PendingPrApprovalCount`, `OverduePoCount`, `CurrentMonthPoAmount` di `PurchasingDashboardDto` hardcode 0; hanya `ApprovedVendorCount` yang riil.
- **Vendor CRUD tidak ada di Purchasing API** — `VendorsController` hanya punya list/get/`set-approved`; create/update/delete vendor kemungkinan besar harus lewat modul Finance (`FinVendor`), perlu dicek konsistensi UX-nya (user Purchasing tidak bisa bikin vendor baru dari halaman Purchasing).
- **`ApprovalStatus` enum menganggur** — ada (Pending/Approved/Rejected/Delegated) tapi tidak dipakai workflow manapun karena belum ada dokumen (PR/PO) yang butuh status approval bertingkat; `PurApprovalConfig` baru sebatas data matriks, belum ada mesin approval yang jalan di atasnya.
- Enum sudah di-scaffold tapi menganggur: `PoStatus`, `PrStatus`, `PurchaseType`, `ReceiptStatus`, `ReturnReason`, `RfqStatus`, `PaymentTerms` — sinyal kuat bahwa PR/PO/RFQ memang direncanakan tapi belum digarap.
