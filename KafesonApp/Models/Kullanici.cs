namespace KafesonApp.Models
{
    public class Kullanici
    {
        public string KullaniciAdi { get; set; } = "";
        public string Sifre { get; set; } = "";

        public bool MasaYetkisi { get; set; }
        public bool RaporYetkisi { get; set; }
        public bool AyarlarYetkisi { get; set; }
    }
}