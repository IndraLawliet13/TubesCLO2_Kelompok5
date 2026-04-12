# TubesCLO2_Kelompok5

C# coursework project for CLO2 group 5.

## Ringkasan

Repo ini sekarang terdiri dari dua bagian utama:
- `TubesCLO2_Kelompok5` -> aplikasi CLI untuk manajemen mahasiswa
- `TubesCLO2_Kelompok5.Api` -> backend demo sederhana berbasis in-memory data

Default aplikasi CLI sudah diarahkan ke endpoint publik berikut:
- `https://rawon-tubesclo2-api.indrayuda.my.id`

Jadi untuk kebutuhan demo/praktikum, alur dasarnya adalah:
1. clone repo
2. restore dependency
3. run aplikasi CLI
4. langsung pakai menu CRUD tanpa harus menyalakan backend lokal sendiri

## Fitur CLI

- Add Student
- View All Students
- Search Student
- Edit Student
- Delete Student

## Cara Menjalankan CLI

```bash
git clone https://github.com/IndraLawliet13/TubesCLO2_Kelompok5.git
cd TubesCLO2_Kelompok5
dotnet restore TubesCLO2_Kelompok5.sln
dotnet run --project TubesCLO2_Kelompok5
```

## Cara Menjalankan Backend Lokal Sendiri

Kalau ingin backend lokal tanpa endpoint publik, jalankan project API ini sendiri:

```bash
dotnet run --project TubesCLO2_Kelompok5.Api --urls http://127.0.0.1:5046
```

Lalu ubah `TubesCLO2_Kelompok5/appsettings.json` menjadi:

```json
{
  "ApiConfig": {
    "BaseUrl": "http://localhost:5046"
  }
}
```

## Catatan

- Backend demo memakai data dummy in-memory, jadi data akan reset saat service backend restart.
- Cocok untuk demo fitur CRUD dan kebutuhan praktikum.
- Unit test tetap tersedia pada project `TubesCLO2_Kelompok5.Test`.
