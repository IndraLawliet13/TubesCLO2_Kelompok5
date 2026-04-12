using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var mahasiswaDb = new ConcurrentDictionary<string, Mahasiswa>(StringComparer.OrdinalIgnoreCase);
mahasiswaDb.TryAdd("1302229999", new Mahasiswa("1302229999", "Seeder Mahasiswa", "RPL", 3.50));
mahasiswaDb.TryAdd("1302223002", new Mahasiswa("1302223002", "Dummy Praktikum", "Teknik Informatika", 3.72));

app.MapGet("/", () => Results.Ok(new ApiResponse<object>(200, "TubesCLO2_Kelompok5 demo API is running", new
{
    service = "TubesCLO2_Kelompok5.Api",
    mode = "in-memory-demo",
    totalMahasiswa = mahasiswaDb.Count
})));

app.MapGet("/api/mahasiswa", (string? nim, string? nama) =>
{
    IEnumerable<Mahasiswa> rows = mahasiswaDb.Values.OrderBy(x => x.Nim);

    if (!string.IsNullOrWhiteSpace(nim))
        rows = rows.Where(x => x.Nim.Equals(nim, StringComparison.OrdinalIgnoreCase));

    if (!string.IsNullOrWhiteSpace(nama))
        rows = rows.Where(x => x.Nama.Contains(nama, StringComparison.OrdinalIgnoreCase));

    var result = rows
        .Select(x => x.ToApi())
        .ToList();

    return Results.Ok(new ApiResponse<object>(200, "OK", result));
});

app.MapGet("/api/mahasiswa/{nim}", (string nim) =>
{
    if (!IsValidNim(nim))
        return Results.BadRequest(new ApiResponse<object>(400, "Format NIM tidak valid.", null));

    if (!mahasiswaDb.TryGetValue(nim, out var mahasiswa))
        return Results.NotFound(new ApiResponse<object>(404, "Not found", null));

    return Results.Ok(new ApiResponse<object>(200, "OK", mahasiswa.ToApi()));
});

app.MapPost("/api/mahasiswa", (MahasiswaRequest request) =>
{
    if (!IsValidMahasiswaRequest(request, out var message))
        return Results.BadRequest(new ApiResponse<object>(400, message, null));

    if (mahasiswaDb.ContainsKey(request.Nim))
        return Results.Conflict(new ApiResponse<object>(409, "already exists", null));

    var mahasiswa = request.ToModel();
    mahasiswaDb[mahasiswa.Nim] = mahasiswa;
    return Results.Created($"/api/mahasiswa/{mahasiswa.Nim}", new ApiResponse<object>(201, "created", mahasiswa.ToApi()));
});

app.MapPut("/api/mahasiswa/{nim}", (string nim, MahasiswaRequest request) =>
{
    if (!IsValidNim(nim))
        return Results.BadRequest(new ApiResponse<object>(400, "Format NIM tidak valid.", null));

    if (!mahasiswaDb.ContainsKey(nim))
        return Results.NotFound(new ApiResponse<object>(404, "Not found", null));

    if (!IsValidMahasiswaRequest(request, out var message))
        return Results.BadRequest(new ApiResponse<object>(400, message, null));

    if (!nim.Equals(request.Nim, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new ApiResponse<object>(400, "NIM pada URL dan body harus sama.", null));

    var mahasiswa = request.ToModel();
    mahasiswaDb[nim] = mahasiswa;
    return Results.Ok(new ApiResponse<object>(200, "updated", mahasiswa.ToApi()));
});

app.MapDelete("/api/mahasiswa/{nim}", (string nim) =>
{
    if (!IsValidNim(nim))
        return Results.BadRequest(new ApiResponse<object>(400, "Format NIM tidak valid.", null));

    if (!mahasiswaDb.TryRemove(nim, out _))
        return Results.NotFound(new ApiResponse<object>(404, "Not found", null));

    return Results.Ok(new ApiResponse<object>(200, "deleted", null));
});

app.Run();

static bool IsValidNim(string? nim) =>
    !string.IsNullOrWhiteSpace(nim) && nim.Length >= 10 && nim.All(char.IsDigit);

static bool IsValidMahasiswaRequest(MahasiswaRequest request, out string message)
{
    if (!IsValidNim(request.Nim))
    {
        message = "nim required or invalid";
        return false;
    }

    if (string.IsNullOrWhiteSpace(request.Nama))
    {
        message = "nama required";
        return false;
    }

    if (request.Ipk < 0 || request.Ipk > 4)
    {
        message = "ipk must be between 0 and 4";
        return false;
    }

    message = "OK";
    return true;
}

record ApiResponse<T>(int Status, string Message, T? Data);

record Mahasiswa(string Nim, string Nama, string? Jurusan, double Ipk)
{
    public object ToApi() => new
    {
        nim = Nim,
        nama = Nama,
        jurusan = Jurusan,
        ipk = Ipk
    };
}

record MahasiswaRequest(string Nim, string Nama, string? Jurusan, double Ipk)
{
    public Mahasiswa ToModel() => new(Nim, Nama, string.IsNullOrWhiteSpace(Jurusan) ? null : Jurusan, Ipk);
}
