namespace KafesonApp.Models
{
    public class SatisRaporu
    {
        public string UrunAd { get; set; }
        public int Adet { get; set; }
        public double Fiyat { get; set; }
        public DateTime Tarih { get; set; }
        public string OdemeTuru { get; set; }
        public int MasaNo { get; set; }

        // XAML tarafı artık direkt birleştirilmiş metni gösterecek
        public string UrunDetaylari => UrunAd;

        // Fiyat alanında toplam tutarı tuttuğumuz için direkt onu döndürüyoruz
        public double Tutar => Fiyat;
    }
}