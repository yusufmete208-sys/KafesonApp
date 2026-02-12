using KafesonApp.Models; // BU SATIR ŞART: Modelleri tanıması için
using System.Collections.ObjectModel;
using System.Text.Json;

namespace KafesonApp;

public partial class App : Application
{
    // 1. STATİK LİSTELER: Tüm sayfalardan erişilebilen ortak hafıza
    public static ObservableCollection<Masa> Masalar { get; set; } = new();
    public static ObservableCollection<Urun> Urunler { get; set; } = new();
    public static ObservableCollection<SatisRaporu> SatisRaporlari { get; set; } = new();

    // Hataları bitiren kritik tanımlar
    public static ObservableCollection<MutfakSiparisi> MutfakSiparisleri { get; set; } = new();
    public static ObservableCollection<MutfakSiparisi> MutfakListesi => MutfakSiparisleri;
    public static ObservableCollection<Urun> TumUrunler => Urunler;

    private static string dosyaYolu = Path.Combine(FileSystem.AppDataDirectory, "kafeson_pro_data.json");

    public App()
    {
        InitializeComponent();
        VerileriYukle();

        if (Masalar.Count == 0)
        {
            for (int i = 1; i <= 20; i++)
                Masalar.Add(new Masa { No = i, IsDolu = false });
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new MainPage()));
    }

    public static void VerileriKaydet()
    {
        try
        {
            var veriPaketi = new { KayitliMasalar = Masalar.ToList(), KayitliUrunler = Urunler.ToList(), KayitliRaporlar = SatisRaporlari.ToList() };
            File.WriteAllText(dosyaYolu, JsonSerializer.Serialize(veriPaketi));
        }
        catch { }
    }

    private static void VerileriYukle()
    {
        if (!File.Exists(dosyaYolu)) return;
        try
        {
            string jsonString = File.ReadAllText(dosyaYolu);
            var veri = JsonSerializer.Deserialize<VeriDeposu>(jsonString);
            if (veri != null)
            {
                Masalar.Clear(); foreach (var m in veri.KayitliMasalar) Masalar.Add(m);
                Urunler.Clear(); foreach (var u in veri.KayitliUrunler) Urunler.Add(u);
                SatisRaporlari.Clear(); foreach (var r in veri.KayitliRaporlar) SatisRaporlari.Add(r);
            }
        }
        catch { }
    }

    private class VeriDeposu
    {
        public List<Masa> KayitliMasalar { get; set; } = new();
        public List<Urun> KayitliUrunler { get; set; } = new();
        public List<SatisRaporu> KayitliRaporlar { get; set; } = new();
    }
}