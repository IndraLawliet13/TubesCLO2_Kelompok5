using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace TubesCLO2_Kelompok5.Models
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
