using KafesonApp.Models;
using KafesonApp.Data;

namespace KafesonApp;

public partial class MainPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();

    public MainPage()
    {
        InitializeComponent();

        // --- AKILLI GÜNCELLEME SİSTEMİ ---
#if ANDROID
        // Her 5 saniyede bir Windows'tan güncel verileri kontrol eder
        Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            if (App.AktifKullanici == null) return true;

            Task.Run(async () =>
            {
                await KasaVerileriniGuncelle();
            });
            return true;
        });
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (App.AktifKullanici == null) return;

        KullaniciLabel.Text = $"Hoş Geldin, {App.AktifKullanici.KullaniciAdi.ToUpper()}";

        BtnMasalar.IsVisible = App.AktifKullanici.MasaYetkisi;
        BtnRaporlar.IsVisible = App.AktifKullanici.RaporYetkisi;
        BtnAyarlar.IsVisible = App.AktifKullanici.AyarlarYetkisi;
        BtnMutfak.IsVisible = true;

#if ANDROID
        await KasaVerileriniGuncelle();
#endif
    }

    // --- GÜVENLİ GÜNCELLEME METODU (GÜNCELLENEN KISIM) ---
    private async Task KasaVerileriniGuncelle()
    {
        try
        {
            // ARTIK SADECE MASA DEĞİL, TÜM PAKETİ (Ürünler + Masalar) ÇEKİYORUZ
            var paket = await _servis.KasadanHerSeyiGetir();

            if (paket != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // 1. ÜRÜNLERİ (MENÜYÜ) DOLDUR
                    // Eğer telefonun menüsü boşsa veya Windows'ta menü değiştiyse burası günceller.
                    // Bu kısım SiparisPage'deki kategorilerin ve butonların çıkması için ŞART!
                    if (paket.Urunler != null && paket.Urunler.Count > 0)
                    {
                        App.Urunler.Clear();
                        foreach (var u in paket.Urunler) App.Urunler.Add(u);
                    }

                    // 2. MASALARI GÜNCELLE
                    if (paket.Masalar != null && paket.Masalar.Count > 0)
                    {
                        App.Masalar.Clear();
                        foreach (var m in paket.Masalar) App.Masalar.Add(m);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Bağlantı Tazeleme Hatası: {ex.Message}");
        }
    }

    // --- BUTON YÖNLENDİRMELERİ ---
    private async void Masalar_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new MasalarPage());
    private async void Raporlar_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new RaporPage());
    private async void Mutfak_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new MutfakPage());
    private async void Ayarlar_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new AyarlarPage());

    private async void Cikis_Clicked(object sender, EventArgs e)
    {
        bool cevap = await DisplayAlert("Çıkış", "Oturumu kapatmak istediğinize emin misiniz?", "Evet", "Hayır");
        if (cevap)
        {
            App.AktifKullanici = null;
            await Navigation.PopToRootAsync();
        }
    }
}