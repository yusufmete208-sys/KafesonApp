using KafesonApp.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace KafesonApp;

public partial class App : Application
{
    public static Kullanici? AktifKullanici { get; set; }

    public static ObservableCollection<Kullanici> Kullanicilar { get; set; } = new();
    public static ObservableCollection<Masa> Masalar { get; set; } = new();
    public static ObservableCollection<Urun> Urunler { get; set; } = new();
    public static ObservableCollection<SatisRaporu> SatisRaporlari { get; set; } = new();
    public static ObservableCollection<MutfakSiparisi> MutfakSiparisleri { get; set; } = new();

    // --- YENİ EKLENEN: LOG LİSTESİ ---
    public static ObservableCollection<SistemLog> SistemLoglari { get; set; } = new();

    public static ObservableCollection<MutfakSiparisi> MutfakListesi => MutfakSiparisleri;
    public static ObservableCollection<Urun> TumUrunler => Urunler;

    private static string dosyaYolu = Path.Combine(FileSystem.AppDataDirectory, "kafeson_final_data.json");

    public App()
    {
        InitializeComponent();
        VerileriYukle();

        if (Kullanicilar.Count == 0)
        {
            Kullanicilar.Add(new Kullanici
            {
                KullaniciAdi = "admin",
                Sifre = "1234",
                MasaYetkisi = true,
                RaporYetkisi = true,
                AyarlarYetkisi = true
            });
            VerileriKaydet();
        }

        if (Masalar.Count == 0)
        {
            for (int i = 1; i <= 20; i++) Masalar.Add(new Masa { No = i, IsDolu = false });
        }

        // =================================================================
        // --- YENİ EKLENEN: SADECE WINDOWS'TA KASAYI (SUNUCUYU) BAŞLAT ---
        // =================================================================
#if WINDOWS
        var sunucu = new KafesonApp.Data.YerelSunucu();
        sunucu.Baslat();
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new LoginPage()));
    }

    // --- YENİ EKLENEN: HER YERDEN ÇAĞIRILABİLECEK LOG EKLEME KOMUTU ---
    public static void LogEkle(string islemTuru, string detay)
    {
        string personel = AktifKullanici != null ? AktifKullanici.KullaniciAdi : "Admin";

        var yeniLog = new SistemLog
        {
            Tarih = DateTime.Now,
            PersonelAdi = personel,
            IslemTuru = islemTuru,
            Detay = detay
        };

        SistemLoglari.Insert(0, yeniLog);
        VerileriKaydet(); // Log eklendiğinde JSON dosyasına anında kaydet
    }

    public static void VerileriKaydet()
    {
        try
        {
            var veriPaketi = new
            {
                Kullanicilar = Kullanicilar.ToList(),
                KayitliMasalar = Masalar.ToList(),
                KayitliUrunler = Urunler.ToList(),
                KayitliRaporlar = SatisRaporlari.ToList(),
                KayitliLoglar = SistemLoglari.ToList() // LOGLARI DA KAYDEDİYORUZ
            };
            string json = JsonSerializer.Serialize(veriPaketi);
            File.WriteAllText(dosyaYolu, json);
        }
        catch { }
    }

    private static void VerileriYukle()
    {
        // EĞER ANDROID (TELEFON) İSE: Kendi içindeki dosyayı yükleme! 
        // Çünkü o dosya eski veya boştur. Her şeyi kasadan çekeceğiz.
#if ANDROID
        return;
#endif

        // PC (Windows) ise normal yüklemeye devam et
        if (!File.Exists(dosyaYolu)) return;

        try
        {
            string jsonString = File.ReadAllText(dosyaYolu);
            var veri = JsonSerializer.Deserialize<VeriDeposu>(jsonString);
            if (veri != null)
            {
                Kullanicilar.Clear(); foreach (var k in veri.Kullanicilar) Kullanicilar.Add(k);
                Masalar.Clear(); foreach (var m in veri.KayitliMasalar) Masalar.Add(m);
                Urunler.Clear(); foreach (var u in veri.KayitliUrunler) Urunler.Add(u);
                SatisRaporlari.Clear(); foreach (var r in veri.KayitliRaporlar) SatisRaporlari.Add(r);
                SistemLoglari.Clear(); foreach (var l in veri.KayitliLoglar) SistemLoglari.Add(l);
            }
        }
        catch { }
    }

    private class VeriDeposu
    {
        public List<Kullanici> Kullanicilar { get; set; } = new();
        public List<Masa> KayitliMasalar { get; set; } = new();
        public List<Urun> KayitliUrunler { get; set; } = new();
        public List<SatisRaporu> KayitliRaporlar { get; set; } = new();
        public List<SistemLog> KayitliLoglar { get; set; } = new(); // DEPODA LOGLAR İÇİN YER AÇTIK
    }
}