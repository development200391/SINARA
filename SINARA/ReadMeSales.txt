ReadMe Sales Module SINARA

Dokumen ini menjelaskan kegunaan setiap menu Sales yang sudah dibuat di aplikasi.

Struktur Menu Sales

1. Sales Dashboard
   Route: /sales

2. Sales Master (menu parent, tidak punya halaman sendiri)
   2.1 Customer Categories
       Route: /sales/customer-categories
   2.2 Price Lists
       Route: /sales/price-lists
   2.3 Approval Configs
       Route: /sales/approval-configs
   2.4 Sales Teams
       Route: /sales/teams
   2.5 Customers
       Route: /sales/customers

Kegunaan Tiap Menu

1. Sales Dashboard (/sales)
- Menampilkan ringkasan KPI/summary Sales dari backend.
- Dipakai untuk monitoring cepat kondisi Sales tanpa masuk ke menu detail.

2. Customer Categories (/sales/customer-categories)
- Master kategori pelanggan Sales (segmentasi customer).
- Menyimpan default pengaturan per kategori:
  code, name, default price list, default payment terms, default credit limit, status active.
- Dipakai sebagai acuan standar saat customer di-assign ke kategori tertentu.

3. Price Lists (/sales/price-lists)
- Master daftar harga jual per periode, tipe price list, dan currency.
- Digunakan untuk mengatur harga berlaku (valid from - valid to) dan status aktif.
- Mendukung CRUD price list.

3.1 Price List Detail Items (/sales/price-lists/{id})
- Halaman detail per price list untuk mengelola item harga.
- Mengatur kombinasi item + UOM + minimum qty + unit price + discount pct.
- Dipakai untuk tier pricing dan variasi harga per kuantitas.

4. Approval Configs (/sales/approval-configs)
- Konfigurasi approval dokumen Sales (quotation/order) berdasarkan level.
- Mengatur min/max amount, max discount, approver role/employee, timeout, auto approve.
- Dipakai agar approval Sales berjalan konsisten sesuai limit dan hirarki.

5. Sales Teams (/sales/teams)
- Master tim Sales:
  code, name, team leader, daftar member, status aktif.
- Dipakai untuk pembagian tanggung jawab dan kepemilikan customer per tim.

6. Customers (/sales/customers)
- Daftar customer dengan atribut Sales:
  customer category, price list, sales employee, sales team,
  credit used, last order date, total ytd sales.
- Dipakai untuk monitoring exposure customer, analisis over credit limit,
  dan melihat detail customer Sales.

6.1 Customer Detail (/sales/customers/{id})
- Menampilkan detail data Sales customer secara lengkap.

Catatan Permission

- Akses menu mengikuti permission matrix per menu (view/create/edit/delete).
- Tombol aksi di grid (create/edit/delete/detail) mengikuti permission menu terkait.
- Jika menu belum ter-mapping di permission matrix, akses diperlakukan fail-closed.

Acuan Implementasi

- Web controller Sales:
  ERP.Web/Controllers/SalesController*.cs
- Seed menu Sales:
  ERP.Infrastructure/Data/DataSeeder.cs
