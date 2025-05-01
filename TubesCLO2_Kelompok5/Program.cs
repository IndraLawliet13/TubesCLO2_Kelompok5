// Program.cs
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
}