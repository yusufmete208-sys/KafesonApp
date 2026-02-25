using System;
using System.Text.Json.Serialization;

namespace KafesonApp.Models
{
    public class MutfakSiparisi
    {
        public int MasaNo { get; set; }
        public string UrunAd { get; set; } = "";
        public int Miktar { get; set; }
        public DateTime KayitSaati { get; set; }

        // Mutfakta sadece saat ve dakika gözüksün
        [JsonIgnore]
        public string SaatMetni => KayitSaati.ToString("HH:mm");
    }
}