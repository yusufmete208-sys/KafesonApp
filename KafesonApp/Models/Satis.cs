namespace KafesonApp.Models
{
    public class Satis
    {
        public DateTime Tarih { get; set; }
        public double Tutar { get; set; }
        public string OdemeTuru { get; set; }
        public int MasaNo { get; set; } // Hangi masadan satıldı?
        public string UrunDetaylari { get; set; } // "2 Çay, 1 Tost" gibi özet metin
    }
}