# Finance Module

Modul General Ledger, AP/AR, dan pelaporan keuangan. Ini modul terbesar dalam solusi — mencakup chart of accounts, jurnal double-entry multi-currency, siklus AP (vendor/invoice/payment) dan AR (customer/invoice/receipt), budget, serta laporan keuangan.

## 1. Entitas & Enum Domain (`ERP.Domain/Entities/Finance`, `ERP.Domain/Enums`)

**Chart of Accounts & dimensi organisasi**
- `FinAccountGroup` — hierarkis, `Type` + `NormalBalance`, `SortOrder`.
- `FinAccount` — Code/Name, `Type`, `NormalBalance`, `IsHeader`, `ParentAccountId` (hierarki), `IsBankAccount` + BankName/BankAccountNo, `CurrencyCode`.
- `FinCostCenter` — terhubung ke `HrDepartment`/manager `HrEmployee`, opsional `BudgetAccount`.

**Currency & kalender**
- `FinCurrency` — Code/Symbol/IsBaseCurrency.
- `FinExchangeRate` — From/To currency, Rate, EffectiveDate, Source.
- `FinFiscalYear` — Start/EndDate, Status.
- `FinPeriod` — PeriodNumber, Start/EndDate, Status.

**Journal & Ledger**
- `FinJournalEntry` — JournalNo, PeriodId, `Source` enum, SourceRefId/Type (link ke AP/AR/Payroll/Closing/Inventory), Status, PostedBy/At, ReversedJournalId, CurrencyCode/ExchangeRate.
- `FinJournalEntryLine` — AccountId, CostCenterId, Debit/Credit (mata uang transaksi) + DebitBase/CreditBase (mata uang dasar).

**Tax & Budget**
- `FinTaxCode` — Type (Ppn/Pph21/Pph23/Pph4Ayat2), Rate, IsInclusive, AccountId terkait.
- `FinBudget` — scoped FiscalYear/Period/CostCenter/Account.
- `FinBudgetLine` — jumlah per period/account/cost center.

**AP (Accounts Payable)**
- `FinVendor` — kategori, buyer group, flag approval, performance score.
- `FinApInvoice`/`FinApInvoiceLine` — Status: Draft/Approved/PartiallyPaid/Paid/Cancelled.
- `FinApPayment`/`FinApPaymentApplication` — pembayaran yang diterapkan ke satu/lebih invoice.

**AR (Accounts Receivable)**
- `FinCustomer` — credit limit/used, sales team/employee, price list.
- `FinArInvoice`/`FinArInvoiceLine` — Status: Draft/Sent/PartiallyPaid/Paid/Cancelled.
- `FinArReceipt`/`FinArReceiptApplication`.

**Enum penting**: `FinanceAccountType` (Asset/Liability/Equity/Revenue/Expense), `FinanceNormalBalance`, `FinancePeriodStatus` (Open/Closed/Locked), `FinanceJournalStatus` (Draft/Posted/Reversed), `FinanceApPaymentMethod`/`FinanceArReceiptMethod` (Transfer/Cash/Check/Giro).

## 2. API Endpoints (`ERP.API/Controllers/v1/Finance`, semua `[Authorize]` via `FinanceControllerBase`)

| Area | Endpoint utama |
|---|---|
| Master data | CRUD paged untuk AccountGroups, Accounts, CostCenters, Currencies, TaxCodes, Customers, Vendors, Budgets, FiscalYears (+ `PUT {id}/close`) |
| ExchangeRates | Get, GetById, Create — append-only (tanpa update/delete), riwayat rate |
| Periods | Get, GetById, `PUT {id}/close` (blokir jika Locked atau masih ada journal Draft) |
| Journals | CRUD (hanya Draft yang editable), `PUT {id}/post` (validasi baris ada & Debit=Credit base), `PUT {id}/reverse` (hanya Posted & belum pernah di-reverse; buat journal reversal cermin, tandai asal jadi Reversed) |
| Ledger | `GET` — query general ledger paged |
| ApInvoices | CRUD (Draft only), `PUT {id}/approve` — auto-generate & post journal seimbang (akun AP default dari vendor atau kode akun fallback "2101", 1 baris per invoice line + baris pajak) |
| ApPayments | Get/GetById/Create — validasi vendor, akun bank, total aplikasi = jumlah payment, status invoice payable, applied ≤ outstanding, resolve periode Open yang mencakup tanggal bayar |
| ArInvoices | CRUD, `PUT {id}/send` (analog approve AP, post journal AR) |
| ArReceipts | Get/GetById/Create (mirror logic ApPayments untuk customer) |
| ApAging / ArAging | `GET` — laporan aging |
| Reports | `trial-balance`, `balance-sheet`, `profit-loss`, `cash-flow`, `budget-vs-actual` |
| Finalization | `GET period-closing` (kesiapan per periode: jumlah journal draft/posted, AP/AR pending, net income/loss, flag CanClose), `GET smoke-tests` (10 pengecekan integritas data otomatis — CoA seeded, mapping cost center, fiscal year terbuka, jumlah period, journal draft = 0, seed AP/AR/exchange rate/budget, trial balance balance) |

## 3. Halaman Web (`ERP.Web/Controllers/Finance/FinanceSetupController.*.cs`, Views di `FinanceSetup/`)

Controller partial class dipecah per fitur, masing-masing punya folder Views: AccountGroups, Accounts, AccountsPayable (ApInvoices/ApPayments/ApAging), AccountsReceivable (ArInvoices/ArReceipts/ArAging), Budgets (+ Budget vs Actual), CostCenters, Currencies, ExchangeRates, FiscalPeriods (FiscalYears/Periods), JournalsLedger (list Journal/Create/Edit/Post/Reverse + tampilan Ledger), Reports (TrialBalance + export, Statement.cshtml untuk balance sheet/P&L/cash flow, BudgetVsActual), Finalization (PeriodClosing, SmokeTests), TaxCodes, Vendors/Customers. Semua action Web memanggil API, tidak ada akses DB/business logic langsung di ERP.Web.

## 4. Business Rules / Logic Penting

- **Validasi double-entry**: tiap baris journal harus debit-XOR-credit (tidak boleh dua-duanya/tidak ada); total DebitBase harus sama dengan total CreditBase sebelum posting (`Post`, `Approve`, `Send` semua enforce ini).
- **Konversi mata uang dasar**: setiap baris menyimpan jumlah mata uang transaksi (Debit/Credit) DAN mata uang dasar (DebitBase/CreditBase), dihitung via `ExchangeRate` dibulatkan 4 desimal (AwayFromZero).
- **Penguncian periode**: journal hanya bisa dibuat/diedit/di-posting di periode `Open`; periode `Closed`/`Locked` menolak draft baru; penutupan periode diblokir jika masih ada journal Draft; penutupan fiscal year meng-cascade menutup semua periode Open miliknya, juga diblokir oleh journal draft atau status Locked.
- **Batasan edit**: hanya journal/invoice Draft yang bisa diedit/dihapus; journal posted hanya bisa di-reverse (tidak bisa diedit), dan hanya sekali (dicek via `ReversedJournalId`).
- **Reversal**: membuat journal baru dengan Debit/Credit (dan base) yang ditukar, referensi periode/tanggal sama, menandai originalnya Reversed.
- **Auto-posting AP/AR**: action Approve/Send men-generate journal sistem (`Source = Ap/Ar`) menggunakan akun default vendor/customer atau fallback kode akun hardcode ("2101"), plus 1 baris per invoice line dan per baris pajak (kode pajak wajib jika TaxAmount > 0), tetap enforce balance sebelum simpan.
- **Aturan aplikasi payment/receipt**: aplikasi harus mereferensikan invoice unik, jumlah applied > 0 dan ≤ OutstandingAmount invoice, invoice tidak boleh Draft/Cancelled, total applied harus sama dengan Amount payment, dan tanggal payment harus jatuh di periode Open.
- **Penomoran journal**: sekuensial per tahun (`JE-{year}-{seq:D6}`), dihitung dengan scan nomor yang sudah ada (bukan DB sequence).
- **Net income/loss** dihitung dinamis per periode dari baris journal Revenue/Expense (journal Draft dikecualikan).

## 5. Relasi Kunci

- `FinJournalEntryLine` → `FinAccount` (+ opsional `FinCostCenter`); `FinJournalEntry` → `FinPeriod`, `FinCurrency`, opsional self-reference `ReversedJournal`.
- `FinApInvoice`/`FinArInvoice` → `FinPeriod`, `FinCurrency`, `FinVendor`/`FinCustomer`, opsional link `JournalEntryId` setelah di-approve/send; baris invoice → `FinAccount`, opsional `FinTaxCode`, `FinCostCenter`.
- `FinApPayment`/`FinArReceipt` → akun bank (`FinAccount`), aplikasi → invoice (many-to-many via tabel aplikasi), opsional `JournalEntryId`.
- `FinBudget`/`FinBudgetLine` scoped ke `FinFiscalYear`/`FinPeriod`/`FinAccount`/`FinCostCenter` untuk perbandingan budget-vs-actual.
- `FinAccount.CurrencyCode` + `FinExchangeRate` (pasangan From/To currency by EffectiveDate) menggerakkan konversi multi-currency di seluruh journal/invoice.

## 6. Known Gaps / Belum Lengkap

- **Single company/single legal entity** — tidak ditemukan konsep multi-company/multi-entity/consolidation di manapun (tidak ada `CompanyId` pada `FinAccount`/`FinJournalEntry`/dsb). Jika ke depan butuh multi-entity, ini butuh perubahan skema signifikan.
- **Fallback akun hardcode "2101"** dipakai saat vendor/customer tidak punya default account saat approve AP invoice / send AR invoice — kalau akun ini tidak ada di chart of accounts pada instalasi baru, auto-posting journal akan gagal; sebaiknya dibuat konfigurasi eksplisit alih-alih hardcode.
- **Penomoran journal** (`JE-{year}-{seq:D6}`) dihitung dengan scan nomor existing tiap kali, bukan DB sequence/counter — berpotensi race condition pada concurrent posting volume tinggi (dua request bisa mendapat nomor sama sebelum salah satu commit).
- Tidak ditemukan fitur **recurring/template journal** (jurnal berulang otomatis, mis. depresiasi manual atau akrual bulanan) di luar yang di-generate otomatis oleh AP/AR/Inventory/FixedAssets.
- Export laporan keuangan (Trial Balance, Balance Sheet, P&L, Cash Flow) sudah terimplementasi penuh di Web (`FinanceSetupController.Reports.cs`) — bukan stub, jadi ini bukan gap, tapi perlu dicek konsistensi format export antar laporan bila ada penambahan laporan baru ke depan.
