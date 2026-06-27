ReadMe Manufacturing Module SINARA

Dokumen ini menjelaskan kegunaan setiap menu Manufacturing yang sudah dibuat di aplikasi.

Struktur Menu Manufacturing

1. Manufacturing Dashboard
   Route: /manufacturing

2. Production Execution (menu parent, tidak punya halaman sendiri)
   2.1 Work Orders
       Route: /manufacturing/work-orders
   2.2 MRP
       Route: /manufacturing/mrp
   2.3 Quality Control
       Route: /manufacturing/qc
   2.4 Scrap
       Route: /manufacturing/scrap
   2.5 Rework
       Route: /manufacturing/rework

3. Manufacturing Reports (menu parent, tidak punya halaman sendiri)
   3.1 Production Output
       Route: /manufacturing/reports/production-output
   3.2 OEE Report
       Route: /manufacturing/reports/oee
   3.3 Cost Variance
       Route: /manufacturing/reports/cost-variance
   3.4 Scrap Analysis
       Route: /manufacturing/reports/scrap-analysis
   3.5 Capacity
       Route: /manufacturing/reports/capacity

4. Manufacturing Master (menu parent, tidak punya halaman sendiri)
   4.1 BOMs
       Route: /manufacturing/boms
   4.2 Routings
       Route: /manufacturing/routings
   4.3 Work Centers
       Route: /manufacturing/work-centers
   4.4 QC Parameters
       Route: /manufacturing/qc/parameters

Kegunaan Tiap Menu

1. Manufacturing Dashboard (/manufacturing)
- Menampilkan KPI cepat Manufacturing: work order aktif, MRP terbuka, QC pending, dan rata-rata OEE.
- Menjadi pintu masuk monitoring harian proses produksi.

2. Work Orders (/manufacturing/work-orders)
- Daftar order produksi per item/work center.
- Dipakai untuk memantau status eksekusi, qty planned/good/scrap, serta jadwal produksi.

3. MRP (/manufacturing/mrp)
- Daftar hasil run Material Requirement Planning.
- Dipakai untuk memantau demand item, rekomendasi WO/PR, dan status run MRP.

4. Quality Control (/manufacturing/qc)
- Daftar hasil inspeksi kualitas per work order/item.
- Dipakai untuk tracking status hasil QC dan waktu inspeksi.

5. Scrap (/manufacturing/scrap)
- Catatan scrap produksi berikut reason dan biaya scrap.
- Dipakai untuk analisis kerugian produksi dan penyebab utama defect.

6. Rework (/manufacturing/rework)
- Daftar pekerjaan rework dari WO sumber ke WO target.
- Dipakai untuk memantau progress dan penyelesaian aktivitas perbaikan.

7. Production Output Report (/manufacturing/reports/production-output)
- Laporan output produksi per work order (planned/good/scrap + completion rate).
- Dipakai untuk analisis performa output produksi.

8. OEE Report (/manufacturing/reports/oee)
- Laporan OEE per work center per tanggal snapshot.
- Dipakai untuk evaluasi availability, performance, quality, dan OEE total.

9. Cost Variance (/manufacturing/reports/cost-variance)
- Laporan perbandingan biaya standar vs aktual per work order.
- Dipakai untuk kontrol deviasi biaya produksi.

10. Scrap Analysis (/manufacturing/reports/scrap-analysis)
- Ringkasan scrap berdasarkan reason.
- Dipakai untuk prioritas perbaikan area penyebab scrap terbesar.

11. Capacity (/manufacturing/reports/capacity)
- Laporan kapasitas work center vs output produksi.
- Dipakai untuk melihat utilisasi kapasitas dan potensi bottleneck.

12. BOMs (/manufacturing/boms)
- Master bill of materials untuk item produksi.
- Dipakai sebagai acuan komposisi material dan standard cost.

13. Routings (/manufacturing/routings)
- Master urutan proses produksi per item/work center.
- Dipakai untuk standar alur proses dan lead time produksi.

14. Work Centers (/manufacturing/work-centers)
- Master pusat kerja produksi (kapasitas, labor, overhead, WIP account).
- Dipakai untuk struktur kapasitas dan costing manufaktur.

15. QC Parameters (/manufacturing/qc/parameters)
- Master parameter quality check per item.
- Dipakai sebagai standar inspeksi quality (numeric/boolean, critical/non-critical).

Catatan Permission

- Semua halaman Manufacturing memakai PagedGrid ViewComponent.
- Filtering grid mengikuti query parameter di URL, dan dipertahankan saat paging/sorting.
- Akses halaman dan tombol aksi grid mengikuti menu permission matrix (view/create/edit/delete).
- Path Manufacturing dibuat fail-closed pada filter permission: jika menu tidak ter-mapping, akses ditolak.

Acuan Implementasi

- API Manufacturing:
  ERP.API/Controllers/v1/Manufacturing/*.cs
- Web Manufacturing:
  ERP.Web/Controllers/ManufacturingController*.cs
  ERP.Web/Views/Manufacturing/*.cshtml
  ERP.Web/Views/Manufacturing/Reports/*.cshtml
- Seed menu + seed data Manufacturing:
  ERP.Infrastructure/Data/DataSeeder.cs

Tambahan Implementasi Eksekusi (CRUD + Process)

- Work Orders:
  - Create: /manufacturing/work-orders/create
  - Edit: /manufacturing/work-orders/edit/{id}
  - Process: Release, Start, Complete, Close, Cancel, Delete (status-based)

- MRP:
  - Create: /manufacturing/mrp/create
  - Edit: /manufacturing/mrp/edit/{id}
  - Process: Run, Complete, Cancel, Delete (status-based)

- Quality Control:
  - Create: /manufacturing/qc/create
  - Edit: /manufacturing/qc/edit/{id}
  - Process: Start, Pass, Fail, Cancel, Delete (status-based)

- Scrap:
  - Create: /manufacturing/scrap/create
  - Edit: /manufacturing/scrap/edit/{id}
  - Process: Delete

- Rework:
  - Create: /manufacturing/rework/create
  - Edit: /manufacturing/rework/edit/{id}
  - Process: Start, Complete, Close, Cancel, Delete (status-based)

Catatan:
- Tombol Add/Edit/Process/Delete di grid tetap mengikuti permission matrix via PagedGrid ViewComponent.
- Jika user tidak punya hak create/edit/delete, tombol otomatis tidak tampil.

Update Lanjut - Lookup Dropdown Form

- Form master dan transaksi Manufacturing sudah memakai dropdown lookup (bukan input ID manual) untuk relasi utama seperti Item, BOM, Routing, Work Center, Work Order, MRP Run, dan Inspector.
- Opsi dropdown dimuat dari API Inventory/Manufacturing/HR pada saat buka form create/edit, serta dimuat ulang saat validasi gagal agar pilihan tetap tampil.

Update Lanjut - Dependent Dropdown

- Form Work Order: pilihan BOM dan Routing otomatis terfilter berdasarkan Item yang dipilih.
- Form QC dan Scrap: saat Work Order dipilih, Item otomatis diarahkan ke item milik Work Order tersebut.
- Form Rework: Item mengikuti Target WO; jika Target WO kosong maka fallback ke Source WO.
- Mekanisme ini berjalan client-side dari data lookup preload, jadi tidak menambah call API saat user mengganti pilihan di form.

