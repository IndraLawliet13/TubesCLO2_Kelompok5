
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
        var apiResponse = await _apiClient.AddMahasiswaAsync(mahasiswaBaru);
        if (apiResponse != null)
        {
            if (apiResponse.Status == 201 && apiResponse.Data != null)
            {
                Console.WriteLine(_configService.GetMessage("SuccessAdd"));
                Debug.Assert(apiResponse.Data != null, "Created Mahasiswa object should not be null on success.");
                Console.WriteLine(apiResponse.Data);
            }
            else if (apiResponse.Status == 409)
            {
                Console.WriteLine(_configService.GetMessage("ErrorAlreadyExists", nim!));
            }
            else
            {
                Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal menambahkan mahasiswa (Status API: {apiResponse.Status})."));
            }
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal menambahkan mahasiswa, respons API tidak diterima/valid."));
        }
        WaitForEnter();
    }
    private static async Task ViewAllStudentsAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("ViewAllOption")} ---");
        Console.WriteLine(_configService.GetMessage("Searching"));
        var apiResponse = await _apiClient.GetAllMahasiswaAsync();
        if (apiResponse != null && apiResponse.Status == 200 && apiResponse.Data != null)
        {
            List<Mahasiswa> mahasiswaList = apiResponse.Data;
            if (mahasiswaList.Any())
            {
                Debug.Assert(mahasiswaList.All(m => m != null), "All Mahasiswa objects in the list should be non-null.");
                Console.WriteLine("Daftar Mahasiswa:");
                int count = 1;
                foreach (var mhs in mahasiswaList)
                {
                    Console.WriteLine($"{count++}. {mhs}");
                }
            }
            else
            {
                Console.WriteLine("Belum ada data mahasiswa atau data kosong sesuai pencarian.");
            }
        }
        else if (apiResponse != null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal mengambil data (Status API: {apiResponse.Status})."));
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mengambil data, respons API tidak diterima/valid."));
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
        var apiResponse = await _apiClient.GetAllMahasiswaAsync(nim, nama);
        if (apiResponse != null && apiResponse.Status == 200 && apiResponse.Data != null)
        {
            List<Mahasiswa> result = apiResponse.Data;
            if (result.Any())
            {
                Console.WriteLine("Hasil Pencarian:");
                int count = 1;
                foreach (var mhs in result)
                {
                    Console.WriteLine($"{count++}. {mhs}");
                }
            }
            else
            {
                Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
            }
        }
        else if (apiResponse != null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal mencari data (Status API: {apiResponse.Status})."));
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mencari data, respons API tidak diterima/valid."));
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
        var apiGetResponse = await _apiClient.GetMahasiswaByNIMAsync(nimToEdit!);
        Mahasiswa? mhsLama = null;
        if (apiGetResponse != null && apiGetResponse.Status == 200 && apiGetResponse.Data != null) // 200 OK
        {
            mhsLama = apiGetResponse.Data;
        }
        else if (apiGetResponse != null)
        {
            return;
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mengambil data mahasiswa untuk diedit."));
            return;
        }
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
        if (string.IsNullOrWhiteSpace(namaBaru)) namaBaru = mhsLama.Nama;
        else if (!InputValidator.IsNotEmpty(namaBaru))
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
        if (string.IsNullOrWhiteSpace(ipkBaruString)) ipkBaruDouble = mhsLama.IPK;
        else if (!InputValidator.IsValidIPK(ipkBaruString, out ipkBaruDouble))
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "Format IPK tidak valid."));
            return;
        }
        var mhsUpdate = new Mahasiswa
        {
            NIM = mhsLama.NIM,
            Nama = namaBaru!,
            Jurusan = jurusanBaru,
            IPK = ipkBaruDouble
        };
        Console.WriteLine("\nData Baru:");
        Console.WriteLine(mhsUpdate);
        if (InputValidator.GetYesNoInput(_configService.GetMessage("ConfirmEdit", mhsLama.NIM)))
        {
            Console.WriteLine(_configService.GetMessage("Updating"));
            var apiUpdateResponse = await _apiClient.UpdateMahasiswaAsync(mhsLama.NIM, mhsUpdate);
            if (apiUpdateResponse != null)
            {
                if (apiUpdateResponse.Status == 200)
                {
                    Console.WriteLine(_configService.GetMessage("SuccessUpdate"));
                }
                else if (apiUpdateResponse.Status == 404)
                {
                    Console.WriteLine(_configService.GetMessage("ErrorNotFound", "(Mungkin data sudah dihapus saat proses edit?)"));
                }
                else
                {
                    Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal mengupdate data (Status API: {apiUpdateResponse.Status})."));
                }
            }
            else
            {
                Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mengupdate data, respons API tidak diterima/valid."));
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
        var apiGetResponse = await _apiClient.GetMahasiswaByNIMAsync(nimToDelete!);
        Mahasiswa? mhs = null;

        if (apiGetResponse != null && apiGetResponse.Status == 200 && apiGetResponse.Data != null) // 200 OK
        {
            mhs = apiGetResponse.Data;
        }
        else if (apiGetResponse != null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
            return;
        }
        else
        {
            Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal mengambil data mahasiswa untuk dihapus."));
            return;
        }
        if (mhs == null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
            return;
        }
        Console.WriteLine($"Data ditemukan: {mhs}");
        if (InputValidator.GetYesNoInput(_configService.GetMessage("ConfirmDelete", mhs.Nama, mhs.NIM)))
        {
            Console.WriteLine(_configService.GetMessage("Deleting"));
            var apiDeleteResponse = await _apiClient.DeleteMahasiswaAsync(nimToDelete!);
            if (apiDeleteResponse != null)
            {
                if (apiDeleteResponse.Status == 200)
                {
                    Console.WriteLine(_configService.GetMessage("SuccessDelete"));
                }
                else if (apiDeleteResponse.Status == 404)
                {
                    Console.WriteLine(_configService.GetMessage("ErrorNotFound", "(Mungkin sudah dihapus?)"));
                }
                else
                {
                    Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal menghapus data (Status API: {apiDeleteResponse.Status})."));
                }
            }
            else
            {
                Console.WriteLine(_configService.GetMessage("ErrorApi", "Gagal menghapus data, respons API tidak diterima/valid."));
            }
        }
    }
    private static void WaitForEnter()
    {
        Console.Write(_configService.GetMessage("PressEnter"));
        Console.ReadLine();
    }
}