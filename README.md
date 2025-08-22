# Inventory Stock Alert Monitoring

Sistem **Inventory Stock Alert Monitoring** adalah aplikasi berbasis **ASP.NET MVC (C#)** yang membantu dalam pemantauan stok barang secara real-time.  
Fitur utama meliputi notifikasi stok menipis pada dashboard dan tampilan riwayat barang masuk (QtyIn) terbaru berdasarkan transaksi.  
Proyek ini dikembangkan untuk mendukung pengelolaan gudang/penyimpanan barang secara efisien.

---

## ✨ Fitur Utama
- 🚨 **Peringatan Stok Menipis** – Dashboard otomatis menampilkan daftar barang dengan stok rendah.  
- 📊 **Riwayat Barang Masuk** – Menyimpan dan menampilkan transaksi masuk terbaru.  
- 🗂 **Manajemen Data** – CRUD untuk barang, kategori, dan transaksi.  
- 📱 **Tampilan Dashboard Interaktif** – Mudah dipahami dengan tabel terorganisir.  

---

## 🛠️ Teknologi yang Digunakan
- **C# ASP.NET MVC**  
- **Entity Framework Core**  
- **SQL Server**  
- **Bootstrap 5** (untuk tampilan UI)  

---

## 📂 Struktur Proyek
Project/
┣ Controllers/ # Logika pengendali (C# Controllers)
┣ Models/ # Representasi data & entity
┣ Views/ # Tampilan (Razor Pages)
┣ Migrations/ # Konfigurasi database (EF Core)
┣ wwwroot/ # Asset statis (CSS, JS, gambar)
┣ appsettings.json # Konfigurasi aplikasi
┗ Project.sln # File solution ASP.NET

---

## 🚀 Cara Menjalankan
1. Clone repository:
   ```bash
   git clone https://github.com/Jimmy-Tan/inventory-stockalert-monitoring.git
2. Buka file Project.sln dengan Visual Studio.

3. Jalankan perintah migrasi (opsional, jika database belum ada):
   ```bash
   Update-Database

5. Tekan F5 atau Ctrl+F5 untuk menjalankan aplikasi.
