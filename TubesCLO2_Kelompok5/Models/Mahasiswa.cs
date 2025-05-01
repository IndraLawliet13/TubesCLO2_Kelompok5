using System.Text.Json.Serialization; 

namespace MahasiswaCLI.Models
{
    public class Mahasiswa
    {
        [JsonPropertyName("nim")]
        public required string NIM { get; set; }
        [JsonPropertyName("nama")]
        public required string Nama { get; set; }
        [JsonPropertyName("jurusan")]
        public string? Jurusan { get; set; }
        [JsonPropertyName("ipk")]
        public double IPK { get; set; }
        public override string ToString()
        {
            return $"NIM: {NIM}, Nama: {Nama}, Jurusan: {Jurusan ?? "-"}, IPK: {IPK:N2}";
        }
    }
}
