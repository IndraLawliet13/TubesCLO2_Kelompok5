# TubesCLO2_Kelompok5

C# coursework project for CLO2 group 5.

## Ringkasan

Repo ini terdiri dari dua bagian utama:

- `TubesCLO2_Kelompok5` -> aplikasi CLI untuk manajemen mahasiswa
- `TubesCLO2_Kelompok5.Api` -> backend demo sederhana berbasis in-memory data

Default aplikasi CLI diarahkan ke endpoint publik berikut:

- `https://rawon-tubesclo2-api.indrayuda.my.id`

Kalau endpoint publik sedang tidak aktif atau ingin demo/praktikum secara mandiri, jalankan backend API secara lokal. Cara lokal ada di bagian quickstart di bawah.

## Fitur CLI

- Add Student
- View All Students
- Search Student
- Edit Student
- Delete Student

## Quickstart: Menjalankan dengan API Lokal

Gunakan cara ini kalau ingin menjalankan aplikasi tanpa bergantung ke API server publik.

### 1. Clone repo dan restore dependency

```bash
git clone https://github.com/IndraYuda13/TubesCLO2_Kelompok5.git
cd TubesCLO2_Kelompok5
dotnet restore TubesCLO2_Kelompok5.sln
```

### 2. Jalankan backend API lokal

Buka terminal pertama dari folder root repo, lalu jalankan:

```bash
dotnet run --project TubesCLO2_Kelompok5.Api --urls http://127.0.0.1:5046
```

Kalau berhasil, terminal akan menampilkan kira-kira seperti ini:

```text
Now listening on: http://127.0.0.1:5046
Application started. Press Ctrl+C to shut down.
```

Backend ini harus tetap menyala selama CLI dipakai.

### 3. Ubah CLI agar memakai API lokal

Buka file:

```text
TubesCLO2_Kelompok5/appsettings.json
```

Ubah `BaseUrl` menjadi:

```json
{
  "ApiConfig": {
    "BaseUrl": "http://127.0.0.1:5046"
  },
  "AppConfig": {
    "DefaultLanguage": "en"
  }
}
```

Catatan: bagian `AppConfig:Messages` yang sudah ada di file jangan dihapus. Cukup ubah nilai `ApiConfig:BaseUrl` saja.

### 4. Jalankan aplikasi CLI

Buka terminal kedua dari folder root repo, lalu jalankan:

```bash
dotnet run --project TubesCLO2_Kelompok5
```

Setelah itu menu CLI bisa digunakan untuk tambah, lihat, cari, edit, dan hapus data mahasiswa dengan backend lokal.

### 5. Cek cepat API lokal

Opsional, untuk memastikan backend lokal benar-benar hidup:

```bash
curl http://127.0.0.1:5046/
curl http://127.0.0.1:5046/api/mahasiswa
```

Jika API hidup, request akan mengembalikan response JSON.

## Quickstart: Menjalankan CLI dengan Endpoint Publik

Jika endpoint publik aktif, CLI bisa langsung dijalankan tanpa menyalakan backend lokal:

```bash
git clone https://github.com/IndraYuda13/TubesCLO2_Kelompok5.git
cd TubesCLO2_Kelompok5
dotnet restore TubesCLO2_Kelompok5.sln
dotnet run --project TubesCLO2_Kelompok5
```

Pastikan `TubesCLO2_Kelompok5/appsettings.json` masih mengarah ke:

```json
{
  "ApiConfig": {
    "BaseUrl": "https://rawon-tubesclo2-api.indrayuda.my.id"
  }
}
```

## Menjalankan Test

```bash
dotnet test TubesCLO2_Kelompok5.sln
```

## Catatan Praktikum / Demo

- Backend lokal memakai data in-memory, jadi data akan reset saat service backend dihentikan atau restart.
- Untuk demo di laptop sendiri, disarankan memakai mode API lokal agar tidak bergantung ke server publik.
- Jika CLI gagal menghubungi API, cek kembali:
  - backend lokal masih menyala atau tidak
  - `BaseUrl` di `TubesCLO2_Kelompok5/appsettings.json` sudah benar
  - port yang dipakai sama, misalnya `http://127.0.0.1:5046`
- Unit test tersedia pada project `TubesCLO2_Kelompok5.Test`.
