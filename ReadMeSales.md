# Sales Module

Modul ini saat ini berisi **data master pendukung penjualan** (kategori customer, price list, tim sales, approval config) plus view read-only ke Customer (master customer sesungguhnya ada di modul Finance). **Transaksi Sales (Quotation, Sales Order, Delivery, Return) belum diimplementasi** — lihat §6.

## 1. Entitas Domain (`ERP.Domain/Entities/Sales`, `ERP.Domain/Enums/Sales`)

- **`SalCustomerCategory`** — Code (unik), Name, `DefaultPriceListId`, `DefaultPaymentTerms` (int, jumlah termin — bukan hari, beda dengan `PaymentTermsDays` di `FinCustomer`), `DefaultCreditLimit`, Description, IsActive.
- **`SalPriceList`** — Code (unik), Name, `Type` (`PriceListType`: Standard, Promotional, Contract, Wholesale, Retail), `CurrencyCode`, `ValidFrom`/`ValidTo`, IsActive, Notes.
- **`SalPriceListItem`** — child dari PriceList: ItemId, UomId, MinQty, UnitPrice, DiscountPct — kombinasi (Item, Uom, MinQty) unik per price list (tier pricing).
- **`SalApprovalConfig`** — `DocumentType` (`SalesDocumentType`: SalesQuotation, SalesOrder), Level, MinAmount/MaxAmount, MaxDiscountPct, ApproverRoleId/ApproverEmployeeId (wajib salah satu), TimeoutHours, AutoApproveIfTimeout, IsActive.
- **`SalSalesTeam`** — Code, Name, TeamLeaderId (→ HrEmployee), daftar member (many-to-many ke HrEmployee), IsActive.
- **Customer master** ada di namespace Finance, bukan Sales: `FinCustomer` (lihat `ReadMeFinance.md`) — Sales hanya membaca field-field Sales-nya (CustomerCategoryId, PriceListId, SalesEmployeeId, SalesTeamId, CreditUsed, LastOrderDate, TotalYtdSales).

**Enum yang sudah ada tapi belum dipakai** (`ERP.Domain/Enums/Sales`) — sinyal fitur transaksional yang direncanakan tapi belum digarap: `QuotationStatus`, `SoStatus`, `DeliveryStatus`, `ReturnReason`, `SalesReturnStatus`, `SalesType`, `SalesApprovalStatus`.

## 2. API Endpoints (`ERP.API/Controllers/v1/Sales`, semua `[Authorize]` via `SalesControllerBase`)

| Controller | Endpoint utama |
|---|---|
| `CustomerCategoriesController` | Full CRUD + `GET options`; Code unik; blokir delete jika masih dipakai Customer |
| `PriceListsController` | Full CRUD untuk header; nested `{priceListId}/items` CRUD untuk detail; Code unik; blokir delete jika dipakai Customer Category/Customer; validasi MinQty>0, UnitPrice>=0, DiscountPct 0-100, kombinasi tier unik |
| `ApprovalConfigsController` | Full CRUD; validasi Level>0, MaxAmount>=MinAmount, MaxDiscountPct 0-100, wajib isi ApproverRole atau ApproverEmployee, tidak boleh kombinasi (DocumentType+Level) duplikat |
| `TeamsController` | Full CRUD + `GET options`; blokir delete jika dipakai Customer |
| `CustomersController` | **Hanya `Get` (paged) dan `GetById`** — tidak ada Create/Update/Delete di sisi Sales |
| `DashboardController` | `GET dashboard` — 4 KPI riil: ActiveCustomerCategoryCount, ActivePriceListCount, ActiveSalesTeamCount, OverCreditLimitCustomerCount (dari `FinCustomer` yang `CreditUsed > CreditLimit`) |

## 3. Halaman Web (`ERP.Web/Controllers/SalesController*.cs`, route `/sales`)

- **Dashboard** (`/sales`) — 4 KPI card.
- **Customer Categories** (`/sales/customer-categories`) — Index/Create/Edit/Delete.
- **Price Lists** (`/sales/price-lists`) — Index/Create/Edit/Delete + halaman detail item (`/sales/price-lists/{id}`, Create/Edit item di `Views/Sales/PriceLists/Items/`).
- **Approval Configs** (`/sales/approval-configs`) — Index/Create/Edit/Delete.
- **Sales Teams** (`/sales/teams`) — Index/Create/Edit/Delete.
- **Customers** (`/sales/customers`) — **read-only**: Index (list + filter) dan Detail (`/sales/customers/{id}`) saja, tidak ada Create/Edit/Delete di sini.

## 4. Business Rules / Logic Penting

- **Customer Sales itu view read-only** — master maintenance customer (CRUD) sepenuhnya ada di modul Finance (`FinCustomer`); Sales hanya menampilkan atribut Sales-nya untuk monitoring (over credit limit, exposure per kategori/tim).
- **Delete guard** konsisten di semua master data: Customer Category, Price List, dan Sales Team tidak bisa dihapus kalau masih direferensikan Customer (mencegah data orphan).
- **Approval Config** memvalidasi rentang amount (Max>=Min) dan mewajibkan minimal satu approver (role atau employee) — tapi karena Sales Order/Quotation belum ada sebagai dokumen (lihat §6), config ini belum benar-benar diterapkan ke alur approval manapun.
- **Tier pricing** pada Price List Item mencegah duplikasi kombinasi (Item, UOM, MinQty) — supaya tidak ada dua baris harga yang saling tumpang tindih untuk kombinasi yang sama.

## 5. Relasi Kunci

- `SalPriceListItem` → `SalPriceList` (header), → `InvItem` + `InvUnitOfMeasure` (Inventory).
- `SalCustomerCategory` → `SalPriceList` (opsional, DefaultPriceListId); `FinCustomer` → `SalCustomerCategory`, `SalPriceList`, `SalSalesTeam` (semua opsional, di sisi entity Finance).
- `SalSalesTeam` → `HrEmployee` (TeamLeader + member, many-to-many).
- `SalApprovalConfig` → `HrEmployee`/`CfgRole` (approver), di-key oleh `SalesDocumentType` + `Level`.

## 6. Known Gaps / Belum Lengkap

- **Transaksi Sales (Quotation, Sales Order, Delivery, Return) sepenuhnya belum diimplementasi** — baru scaffolding di Domain layer:
  - Enum sudah ada tapi menganggur (lihat §1: `QuotationStatus`, `SoStatus`, `DeliveryStatus`, `ReturnReason`, `SalesReturnStatus`, `SalesType`, `SalesApprovalStatus`).
  - Interface repository tanpa implementasi: `ISalesOrderRepository`, `ISalesQuotationRepository`, `ISalesDeliveryRepository` (`ERP.Domain/Interfaces/Sales/`).
  - `ISalesIntegrationService` (untuk auto-create AR Invoice dari Delivery, auto-create Delivery dari SO, reverse AR Invoice) tidak punya satupun class implementasi di seluruh solusi.
  - Tidak ada entity `SalSalesOrder`/`SalSalesQuotation`/`SalSalesDelivery` — hanya read-model kosong (`SalesReadModels.cs`) yang mereferensikan tabel yang belum ada.
  - Akibatnya, `SalApprovalConfig.DocumentType = SalesOrder` sudah bisa dikonfigurasi (termasuk data seed), tapi tidak ada dokumen Sales Order sungguhan yang menerapkan approval config tersebut.
- Modul Sales saat ini = master data + view read-only ke Customer (Finance) + Dashboard KPI. Bagian transaksional (quotation → order → delivery → invoice) adalah pekerjaan lanjutan yang belum digarap.
- Tidak ditemukan TODO/NotImplementedException/stub di controller yang sudah ada — bagian yang memang diimplementasikan (master data) sudah lengkap, bukan setengah jadi.
