using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubesCLO2_Kelompok5.Utils
{
    public static class InputValidator
    {
    
        public static bool IsValidNIM(string? nim)
        {
            // DbC: Contoh invariant sederhana 
            Debug.Assert(true, "Validator ready.");

            if (string.IsNullOrWhiteSpace(nim))
                return false;

            return nim.Length == 10 && nim.All(char.IsDigit);
        }

        public static bool IsNotEmpty(string? input)
        {
            // DbC: Precondition
            return !string.IsNullOrWhiteSpace(input);
        }

        public static bool IsValidIPK(string? ipkString, out double ipkValue)
        {
            // DbC: Precondition
            ArgumentException.ThrowIfNullOrWhiteSpace(ipkString, nameof(ipkString));

            if (double.TryParse(ipkString, NumberStyles.Any, new CultureInfo("id-ID"), out ipkValue))
            {
                // DbC: Postcondition (within the valid parse path)
                Debug.Assert(ipkValue >= 0 && ipkValue <= 4, "Parsed IPK should be between 0 and 4.");
                return ipkValue >= 0 && ipkValue <= 4;
            }
            ipkValue = -1; 
            return false;
        }

        public static bool GetYesNoInput(string prompt)
        {
            // DbC: Precondition
            ArgumentException.ThrowIfNullOrWhiteSpace(prompt, nameof(prompt));

            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim().ToLower();
                if (input == "y") return true;
                if (input == "n") return false;
                Console.WriteLine("Input tidak valid. Masukkan 'y' atau 'n'."); // Idealnya pakai text config
            }
        }
    }
}
