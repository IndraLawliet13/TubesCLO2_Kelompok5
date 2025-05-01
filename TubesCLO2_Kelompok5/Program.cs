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

            _apiClient = new MahasiswaApiClient(_configService);
            _currentState = AppState.MainMenu;
        }
        while (_currentState != AppState.Exiting)
        {
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
                    Debug.Fail($"Unexpected state reached: {_currentState}");
                    Console.WriteLine("Error: Unknown application state.");
                    _currentState = AppState.MainMenu;
                    break;
            }
        }
    }

    private static void DisplayMainMenu()
    {
        Console.WriteLine(_configService.GetMessage("MainMenuHeader"));
        // Menggunakan Table-Driven data untuk menampilkan menu
        foreach (var option in _mainMenuOptions)
        {
            Console.WriteLine($"{option.Key}. {_configService.GetMessage(option.Value.MessageKey)}");
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
        var mahasiswaBaru = new Mahasiswa { NIM = nim!, Nama = nama!, Jurusan = jurusan, IPK = ipk };
        var (success, conflict, createdMhs) = await _apiClient.AddMahasiswaAsync(mahasiswaBaru);
        while (true)
        {
            if (InputValidator.IsValidNIM(nim)) break;
        }
        while (true)
        {
            if (InputValidator.IsNotEmpty(nama)) break;
        }
        while (true)
        {
            // ... (input ipkString)
            if (InputValidator.IsValidIPK(ipkString, out ipk)) break;
        }
    }

    private static async Task ViewAllStudentsAsync()
    {
        // ...
        var mahasiswaList = await _apiClient.GetAllMahasiswaAsync();
        // ... 
    }

    private static async Task SearchStudentAsync()
    {
        // ... 
        var result = await _apiClient.GetAllMahasiswaAsync(nim, nama);

    }
    private static async Task EditStudentAsync()
    {
        Console.WriteLine($"\n--- {_configService.GetMessage("EditOption")} ---");
        Console.Write(_configService.GetMessage("InputNIM", " (yang akan diedit)")); // Tambahan teks
        string? nimToEdit = Console.ReadLine();

        if (!InputValidator.IsValidNIM(nimToEdit))
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "NIM tidak valid."));
            return;
        }

        Console.WriteLine(_configService.GetMessage("Searching"));
        var mhsLama = await _apiClient.GetMahasiswaByNIMAsync(nimToEdit!); // Ambil data lama

        if (mhsLama == null)
        {
            Console.WriteLine(_configService.GetMessage("ErrorNotFound"));
            return;
        }

        Console.WriteLine("\nData Lama:");
        Console.WriteLine(mhsLama);
        Console.WriteLine("\nMasukkan Data Baru (kosongi jika tidak ingin ubah):");

        // Input Nama Baru
        Console.Write($"{_configService.GetMessage("InputName")} (Lama: {mhsLama.Nama}): ");
        string? namaBaru = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(namaBaru)) namaBaru = mhsLama.Nama; // Pakai lama jika kosong
        else if (!InputValidator.IsNotEmpty(namaBaru)) // Pastikan tidak cuma spasi
        {
            Console.WriteLine(_configService.GetMessage("ErrorInvalidInput", "Nama tidak boleh hanya spasi."));
            return;
        }


        // Input Jurusan Baru
        Console.Write($"{_configService.GetMessage("InputMajor")} (Lama: {mhsLama.Jurusan ?? "-"}): ");
        string? jurusanBaru = Console.ReadLine();
        // Pakai lama jika user hanya input spasi, set null jika user input string kosong (menghapus jurusan)
        if (string.IsNullOrWhiteSpace(jurusanBaru) && jurusanBaru != "") jurusanBaru = mhsLama.Jurusan;
        else if (jurusanBaru == "") jurusanBaru = null;


        // Input IPK Baru
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

            if (statusCode == System.Net.HttpStatusCode.NoContent) // 204
            {
                Console.WriteLine(_configService.GetMessage("SuccessUpdate"));
            }
            else if (statusCode == System.Net.HttpStatusCode.NotFound) // 404
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

            if (statusCode == System.Net.HttpStatusCode.NoContent) // 204
            {
                Console.WriteLine(_configService.GetMessage("SuccessDelete"));
            }
            else if (statusCode == System.Net.HttpStatusCode.NotFound) // 404
            {
                Console.WriteLine(_configService.GetMessage("ErrorNotFound", "(Mungkin sudah dihapus?)"));
            }
            else 
            {
                Console.WriteLine(_configService.GetMessage("ErrorApi", $"Gagal menghapus data (Status: {statusCode})."));
            }
        }
    }
}
