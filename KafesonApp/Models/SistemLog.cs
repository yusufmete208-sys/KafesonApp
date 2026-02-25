using System;

namespace KafesonApp.Models
{
    public class SistemLog
    {
        public DateTime Tarih { get; set; }
        public string PersonelAdi { get; set; } = "Sistem";
        public string IslemTuru { get; set; } = "";
        public string Detay { get; set; } = "";
    }
}