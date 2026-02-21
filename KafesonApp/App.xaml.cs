using KafesonApp.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO; // Path ve File işlemleri için ŞART
using System.Linq; // .ToList() ve LINQ işlemleri için ŞART

namespace KafesonApp;

public partial class App : Application
{
    // 1. STATİK LİSTELER
    public static ObservableCollection<Masa> Masalar { get; set; } = new();
    public static ObservableCollection<Satis> KapananMasalar { get; set; } = new();
    public static ObservableCollection<Urun> Urunler { get; set; } = new();
    public static ObservableCollection<SatisRaporu> SatisRaporlari { get; set; } = new();
    public static ObservableCollection<LogKaydi> Loglar { get; set; } = new();

    // Yardımcı tanımlamalar
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
            for (int i = 1; i <= 10; i++)
                Masalar.Add(new Masa { No = i, Mekan = "İç Mekan" });

            for (int i = 11; i <= 20; i++)
                Masalar.Add(new Masa { No = i, Mekan = "Bahçe" });
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new MainPage()));
    }

    // --- LOG EKLEME METODU ---
    public static void LogEkle(string mesaj, string tip = "Genel")
    {
        // Listenin en başına ekle
        Loglar.Insert(0, new LogKaydi
        {
            Mesaj = mesaj,
            IslemTipi = tip,
            Tarih = DateTime.Now
        });

        VerileriKaydet();
    }

    public static void VerileriKaydet()
    {
        try
        {
            var veriPaketi = new VeriDeposu
            {
                KayitliMasalar = Masalar.ToList(),
                KayitliUrunler = Urunler.ToList(),
                KayitliRaporlar = SatisRaporlari.ToList(),
                KayitliKapananMasalar = KapananMasalar.ToList(),
                KayitliLoglar = Loglar.ToList() // Logları kayda ekle
            };

            string json = JsonSerializer.Serialize(veriPaketi);
            File.WriteAllText(dosyaYolu, json);
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
                Masalar.Clear();
                foreach (var m in veri.KayitliMasalar) Masalar.Add(m);

                Urunler.Clear();
                foreach (var u in veri.KayitliUrunler) Urunler.Add(u);

                SatisRaporlari.Clear();
                foreach (var r in veri.KayitliRaporlar) SatisRaporlari.Add(r);

                KapananMasalar.Clear();
                if (veri.KayitliKapananMasalar != null)
                    foreach (var k in veri.KayitliKapananMasalar) KapananMasalar.Add(k);

                Loglar.Clear();
                if (veri.KayitliLoglar != null)
                    foreach (var l in veri.KayitliLoglar) Loglar.Add(l);
            }
        }
        catch { }
    }

    // --- VERİ SAKLAMA SINIFI (Hata almamak için burayı kontrol et) ---
    private class VeriDeposu
    {
        public List<Masa> KayitliMasalar { get; set; } = new();
        public List<Urun> KayitliUrunler { get; set; } = new();
        public List<SatisRaporu> KayitliRaporlar { get; set; } = new();
        public List<Satis> KayitliKapananMasalar { get; set; } = new();
        public List<LogKaydi> KayitliLoglar { get; set; } = new(); // Bu satır eksikse hata verir
    }
}