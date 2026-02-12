namespace KafesonApp.Models
{
    public class MutfakSiparisi
    {
        public int MasaNo { get; set; }
        public string UrunAd { get; set; } = string.Empty;
        public int Miktar { get; set; }
        public DateTime KayitSaati { get; set; } = DateTime.Now;
    }
}