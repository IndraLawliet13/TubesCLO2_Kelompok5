
using TubesCLO2_Kelompok5.Models;
using TubesCLO2_Kelompok5.Services;
using TubesCLO2_Kelompok5.Utils;
using System.Diagnostics;
public enum AppState
{
    Initializing,
    MainMenu,
    AddingStudent,
    ViewingAllStudents,
    SearchingStudent,
    EditingStudent,
    DeletingStudent,
    Exiting
}

public class Program
{
    private static ConfigurationService _configService = null!;
    private static MahasiswaApiClient _apiClient = null!;

    private static AppState _currentState = AppState.Initializing;


    private static readonly Dictionary<string, (string MessageKey, AppState TargetState)> _mainMenuOptions =
        new Dictionary<string, (string, AppState)>
    {
        {"1", ("AddOption", AppState.AddingStudent)},
        {"2", ("ViewAllOption", AppState.ViewingAllStudents)},
        {"3", ("SearchOption", AppState.SearchingStudent)},
        {"4", ("EditOption", AppState.EditingStudent)},
        {"5", ("DeleteOption", AppState.DeletingStudent)},
        {"0", ("ExitOption", AppState.Exiting)}
    };

    public static async Task Main(string[] args)
    {
        try
        {
            _configService = new ConfigurationService();
            _apiClient = new MahasiswaApiClient(_configService);
            _currentState = AppState.MainMenu;
            Debug.Assert(_configService != null, "ConfigService failed to initialize.");
            Debug.Assert(_apiClient != null, "ApiClient failed to initialize.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR during initialization: {ex.Message}");
            // DbC: Assert bahwa inisialisasi gagal jika exception terjadi
            Debug.Assert(_currentState == AppState.Initializing, "State should remain Initializing on fatal error.");
            return; // Tidak bisa lanjut
        }

        Console.WriteLine(_configService.GetMessage("Welcome"));
        while (_currentState != AppState.Exiting)
        {
            // DbC: Invariant - State tidak boleh Initializing di dalam loop utama
            Debug.Assert(_currentState != AppState.Initializing, "State should not be Initializing in main loop.");
            switch (_currentState)
            {
                case AppState.MainMenu:
                    DisplayMainMenu();
                    break;
                case AppState.AddingStudent:
                    await AddStudentAsync();
                    _currentState = AppState.MainMenu;
                    break;
                case AppState.ViewingAllStudents:
                    await ViewAllStudentsAsync();
                    WaitForEnter();
                    _currentState = AppState.MainMenu;
                    break;
                case AppState.SearchingStudent:
                    await SearchStudentAsync();
                    WaitForEnter();
                    _currentState = AppState.MainMenu;
                    break;
                case AppState.EditingStudent:
                    await EditStudentAsync();
                    WaitForEnter();
                    _currentState = AppState.MainMenu;
                    break;
                case AppState.DeletingStudent:
                    await DeleteStudentAsync();
                    WaitForEnter();
                    _currentState = AppState.MainMenu;
                    break;
                default:
                    // DbC: Assert bahwa state tidak terduga tidak tercapai
                    Debug.Fail($"Unexpected state reached: {_currentState}");
                    Console.WriteLine("Error: Unknown application state.");
                    _currentState = AppState.MainMenu;
                    break;
            }
        }
        Console.WriteLine(_configService.GetMessage("Exiting"));
    }
    private static void DisplayMainMenu()
    {
        Console.WriteLine(_configService.GetMessage("MainMenuHeader"));
        foreach (var option in _mainMenuOptions)
        {
            Console.WriteLine($"{_configService.GetMessage(option.Value.MessageKey)}");
        }
        Console.Write(_configService.GetMessage("ChooseOption"));
        string? choice = Console.ReadLine();
        if (choice != null && _mainMenuOptions.TryGetValue(choice, out var selectedOption))
        {
            _currentState = selectedOption.TargetState;
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("InvalidOption"));
        }
    }
    private static async Task AddStudentAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("AddOption")} ---");
        string? nim, nama, jurusan, ipkString;
        double ipk;

        while (true)
        {
            Console.Write(_configService.GetMessage("InputNIM"));
            nim = Console.ReadLine();
            if (InputValidator.IsValidNIM(nim)) break;
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "NIM tidak valid (harus 10 digit angka)."));
        }
        while (true)
        {
            Console.Write(_configService.GetMessage("InputName"));
            nama = Console.ReadLine();
            if (InputValidator.IsNotEmpty(nama)) break;
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "Nama tidak boleh kosong."));
        }
        Console.Write(_configService.GetMessage("InputMajor"));
        jurusan = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(jurusan)) jurusan = null;
        while (true)
        {
            Console.Write(_configService.GetMessage("InputGPA"));
            ipkString = Console.ReadLine();
            if (InputValidator.IsValidIPK(ipkString, out ipk)) break;
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "IPK tidak valid (harus angka antara 0-4, gunakan koma ',')."));
        }
        var mahasiswaBaru = new Mahasiswa { NIM = nim!, Nama = nama!, Jurusan = jurusan, IPK = ipk };
        Console.WriteLine(_configService.GetMessage("Adding"));

        var (success, conflict, createdMhs) = await _apiClient.AddMahasiswaAsync(mahasiswaBaru);
        if (success)
        {
            Console.WriteLine(_configService.GetMessage("SuccessAdd"));
            Debug.Assert(createdMhs != null, "Created Mahasiswa object should not be null on success.");
            Console.WriteLine(createdMhs);
        }
        else if (conflict)
        {
            Console.WriteLine(_configService.GetMessage("ErrorAlreadyExists", nim!));
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal menambahkan mahasiswa."));
        }
        WaitForEnter();
    }
    private static async Task ViewAllStudentsAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("ViewAllOption")} ---");
        Console.WriteLine(_configService.GetMessage("Searching"));
        var mahasiswaList = await _apiClient.GetAllMahasiswaAsync();
        if (mahasiswaList != null && mahasiswaList.Any())
        {
            // DbC: Postcondition
            Debug.Assert(mahasiswaList.All(m => m != null), "All Mahasiswa objects in the list should be non-null.");
            Console.WriteLine("Daftar Mahasiswa:");
            int count = 1;
            foreach (var mhs in mahasiswaList)
            {
                Console.WriteLine($"{count++}. {mhs}");
            }
        }
        else if (mahasiswaList != null)
        {
            Console.WriteLine("Belum ada data mahasiswa.");
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mengambil data."));
        }
    }
    private static async Task SearchStudentAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("SearchOption")} ---");
        Console.Write("Cari berdasarkan NIM atau Nama? (n/m): ");
        string? searchBy = Console.ReadLine()?.Trim().ToLower();
        string? nim = null;
        string? nama = null;
        if (searchBy == "n")
        {
            Console.Write(_configService.GetMessage("InputNIM"));
            nim = Console.ReadLine();
        }
        else if (searchBy == "m")
        {
            Console.Write(_configService.GetMessage("InputName"));
            nama = Console.ReadLine();
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("InvalidOption"));
            return;
        }
        Console.WriteLine(_configService.GetMessage("Searching"));
        var result = await _apiClient.GetAllMahasiswaAsync(nim, nama);
        if (result != null && result.Any())
        {
            Console.WriteLine("Hasil Pencarian:");
            int count = 1;
            foreach (var mhs in result)
            {
                Console.WriteLine($"{count++}. {mhs}");
            }
        }
        else if (result != null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mencari data."));
        }
    }
    private static async Task EditStudentAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("EditOption")} ---");
        Console.Write(_configService.GetMessage("InputNIM", " (yang akan diedit)"));
        string? nimToEdit = Console.ReadLine();
        if (!InputValidator.IsValidNIM(nimToEdit))
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "NIM tidak valid."));
            return;
        }
        Console.WriteLine(_configService.GetMessage("Searching"));
        var mhsLama = await _apiClient.GetMahasiswaByNIMAsync(nimToEdit!);
        if (mhsLama == null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
            return;
        }
        Console.WriteLine("\nData Lama:");
        Console.WriteLine(mhsLama);
        Console.WriteLine("\nMasukkan Data Baru (kosongi jika tidak ingin ubah):");
        Console.Write($"{_configService.GetMessage("InputName")} (Lama: {mhsLama.Nama}): ");
        string? namaBaru = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(namaBaru)) namaBaru = mhsLama.Nama; // Pakai lama jika kosong
        else if (!InputValidator.IsNotEmpty(namaBaru)) // Pastikan tidak cuma spasi
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "Nama tidak boleh hanya spasi."));
            return;
        }
        Console.Write($"{_configService.GetMessage("InputMajor")} (Lama: {mhsLama.Jurusan ?? "-"}): ");
        string? jurusanBaru = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(jurusanBaru) && jurusanBaru != "") jurusanBaru = mhsLama.Jurusan;
        else if (jurusanBaru == "") jurusanBaru = null;
        double ipkBaruDouble;
        Console.Write($"{_configService.GetMessage("InputGPA")} (Lama: {mhsLama.IPK:N2}): ");
        string? ipkBaruString = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ipkBaruString)) ipkBaruDouble = mhsLama.IPK; // Pakai lama jika kosong
        else if (!InputValidator.IsValidIPK(ipkBaruString, out ipkBaruDouble))
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "Format IPK tidak valid."));
            return;
        }
        var mhsUpdate = new Mahasiswa
        {
            NIM = mhsLama.NIM, // NIM tidak boleh diubah
            Nama = namaBaru,
            Jurusan = jurusanBaru,
            IPK = ipkBaruDouble
        };
        Console.WriteLine("\nData Baru:");
        Console.WriteLine(mhsUpdate);
        if (InputValidator.GetYesNoInput(_configService.GetMessage("ConfirmEdit", mhsLama.NIM)))
        {
            Console.WriteLine(_configService.GetMessage("Updating"));
            var statusCode = await _apiClient.UpdateMahasiswaAsync(mhsLama.NIM, mhsUpdate);

            if (statusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine(_configService.GetMessage("SuccessUpdate"));
            }
            else if (statusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine(_configService.GetMessage("ErrorNotFound", "(Mungkin data sudah dihapus?)"));
            }
            else // Error lain
            {
                Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal mengupdate data (Status: {statusCode})."));
            }
        }
    }
    private static async Task DeleteStudentAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("DeleteOption")} ---");
        Console.Write(_configService.GetMessage("InputNIM", " (yang akan dihapus)"));
        string? nimToDelete = Console.ReadLine();
        if (!InputValidator.IsValidNIM(nimToDelete))
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "NIM tidak valid."));
            return;
        }
        Console.WriteLine(_configService.GetMessage("Searching"));
        var mhs = await _apiClient.GetMahasiswaByNIMAsync(nimToDelete!);
        if (mhs == null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
            return;
        }
        Console.WriteLine($"Data ditemukan: {mhs}");
        if (InputValidator.GetYesNoInput(_configService.GetMessage("ConfirmDelete", mhs.Nama, mhs.NIM)))
        {
            Console.WriteLine(_configService.GetMessage("Deleting"));
            var statusCode = await _apiClient.DeleteMahasiswaAsync(nimToDelete!);
            if (statusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine(_configService.GetMessage("SuccessDelete"));
            }
            else if (statusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine(_configService.GetMessage("ErrorNotFound", "(Mungkin sudah dihapus?)"));
            }
            else
            {
                Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal menghapus data (Status: {statusCode})."));
            }
        }
    }
    private static void WaitForEnter()
    {
        Console.Write(_configService.GetMessage("PressEnter"));
        Console.ReadLine();
    }
}