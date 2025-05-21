using TubesCLO2_Kelompok5.Utils;
namespace TubesCLO2_Kelompok5.Test
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void IsValidNim_InputNimValid_ReturnsTrue()
        {
            string nimValid = "1302220001";
            bool hasil = InputValidator.IsValidNIM(nimValid);
            Assert.IsTrue(hasil, "NIM seharusnya valid.");
        }

        [TestMethod]
        public void IsValidNIM_InputNimTidakValidPanjang_ReturnsFalse()
        {
            string nimTidakValid = "123";
            bool hasil = InputValidator.IsValidNIM(nimTidakValid);
            Assert.IsFalse(hasil, "NIM '123' seharusnya tidak valid karena terlalu pendek.");
        }

        [TestMethod]
        public void IsValidNIM_InputNimTidakValidAdaHuruf_ReturnsFalse()
        {
            string nimTidakValid = "130222000A";

            bool hasil = InputValidator.IsValidNIM(nimTidakValid);

            Assert.IsFalse(hasil, "NIM '130222000A' seharusnya tidak valid karena mengandung huruf.");
        }

        [TestMethod]
        public void IsValidNIM_InputNullAtauKosong_ReturnsFalse()
        {
            string? nimNull = null;
            string nimKosong = "";
            string nimSpasi = "   ";

            Assert.IsFalse(InputValidator.IsValidNIM(nimNull), "NIM null seharusnya tidak valid.");
            Assert.IsFalse(InputValidator.IsValidNIM(nimKosong), "NIM kosong seharusnya tidak valid.");
            Assert.IsFalse(InputValidator.IsValidNIM(nimSpasi), "NIM spasi seharusnya tidak valid.");
        }

        [TestMethod]
        public void IsNotEmpty_InputAdaIsi_ReturnsTrue()
        {
            string inputAdaIsi = "ada isinya";

            bool hasil = InputValidator.IsNotEmpty(inputAdaIsi);

            Assert.IsTrue(hasil, "Input 'ada isinya' seharusnya tidak kosong.");
        }

        [TestMethod]
        public void IsNotEmpty_InputNullAtauKosong_ReturnsFalse()
        {
            string? inputNull = null;
            string inputKosong = "";
            string inputSpasi = "   ";

            Assert.IsFalse(InputValidator.IsNotEmpty(inputNull), "Input null seharusnya dianggap kosong.");
            Assert.IsFalse(InputValidator.IsNotEmpty(inputKosong), "Input string kosong seharusnya dianggap kosong.");
            Assert.IsFalse(InputValidator.IsNotEmpty(inputSpasi), "Input spasi seharusnya dianggap kosong.");
        }

        [TestMethod]
        public void IsValidIPK_InputTidakValidFormat_ReturnsFalse()
        {
            string ipkStringTidakValid = "abc";

            bool hasil = InputValidator.IsValidIPK(ipkStringTidakValid, out double ipkValue);

            Assert.IsFalse(hasil, "IPK 'abc' seharusnya tidak valid.");
            Assert.AreEqual(-1, ipkValue, "Nilai IPK default jika tidak valid seharusnya -1."); // Sesuai implementasi
        }
    }
}
