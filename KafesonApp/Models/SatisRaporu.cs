using System;

namespace KafesonApp.Models
{
    public class SatisRaporu
    {
        public int MasaNo { get; set; }
        public string OdemeTuru { get; set; } = "";
        public string UrunDetaylari { get; set; } = "";
        public double Tutar { get; set; }
        public DateTime Tarih { get; set; }

        // YENİ EKLENEN KISIM: İşlemi Yapan Personel
        public string PersonelAdi { get; set; } = "Bilinmiyor";
    }
}