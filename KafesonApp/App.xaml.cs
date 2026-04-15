
global using Kafeson.Shared;
global using Kafeson.Shared.Models;
global using KafesonApp.Data;#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using System.Collections.ObjectModel;
using System.Net.Sockets; // RADAR İÇİN EKLENDİ
using Microsoft.Maui.Storage; // BİLGİSAYARIN IP'SİNİ KAYDETMEK İÇİN EKLENDİ

namespace KafesonApp;

public partial class App : Application
{
    public static Kullanici AktifKullanici { get; set; }

    public static ObservableCollection<Kullanici> Kullanicilar { get; set; } = new();
    public static ObservableCollection<Masa> Masalar { get; set; } = new();
    public static ObservableCollection<Urun> Urunler { get; set; } = new();
    public static ObservableCollection<SatisRaporu> SatisRaporlari { get; set; } = new();
    public static ObservableCollection<MutfakSiparisi> MutfakSiparisleri { get; set; } = new();

    public static ObservableCollection<MutfakSiparisi> MutfakListesi => MutfakSiparisleri;
    public static ObservableCollection<Urun> TumUrunler => Urunler;

    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());

        // 🚨 SİHİRLİ DOKUNUŞ: Uygulama açıldığı an arka planda Kasa'yı aramaya başla
        OtomatikKasaBulVeBaglan();
    }

    // 🚨 RADAR MOTORUNUN KENDİSİ (Senin yazdığın UdpDinleyici sınıfını kullanır) 🚨
    private async void OtomatikKasaBulVeBaglan()
    {
        try
        {
            // Senin kendi yazdığın o harika sınıfı çağırıyoruz
            var dinleyici = new UdpDinleyici();
            string kasaIpAdresi = await dinleyici.OtomatikKasaBulAsync();

            // Eğer 3 saniye içinde Kasa'yı duyduysa ve IP'yi aldıysa
            if (!string.IsNullOrEmpty(kasaIpAdresi))
            {
                // 1. Yakalanan IP'yi telefona sessizce kaydet
                Preferences.Default.Set("SunucuIP", kasaIpAdresi);

                // 2. Yeni IP ile API'den güncel verileri hemen çekip masaları doldur!
                await ApiVerileriniCek();
            }
        }
        catch
        {
            // Bulamazsa kimseyi rahatsız etmeden eski kayıtlı IP ile devam et
        }
    }

    public static async Task ApiVerileriniCek()
    {
        try
        {
            var servis = new VeriServisi();
            var paket = await servis.KasadanHerSeyiGetir();

            if (paket != null)
            {
                // DÜZELTİLEN YER TAM OLARAK BURASI!
                // Artık "BeginInvoke..." yerine "await ... InvokeAsync" kullanıyoruz ki 
                // telefon listeyi doldurmadan sayfayı çizmeye çalışmasın.
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Kullanicilar.Clear(); foreach (var k in paket.Kullanicilar) Kullanicilar.Add(k);
                    Masalar.Clear(); foreach (var m in paket.Masalar) Masalar.Add(m);
                    Urunler.Clear(); foreach (var u in paket.Urunler) Urunler.Add(u);
                    MutfakSiparisleri.Clear(); foreach (var ms in paket.MutfakSiparisleri) MutfakSiparisleri.Add(ms);
                });
            }
        }
        catch { }
    }

    public static async Task LogEkle(string islem, string detay, int masaNo = 0)
    {
        try
        {
            var servis = new VeriServisi();
            await servis.LogGonder(masaNo, islem, detay);
        }
        catch { }
    }
}