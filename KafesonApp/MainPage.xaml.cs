using Kafeson.Shared.Models;
using KafesonApp.Data;
using KafesonApp.Views;

namespace KafesonApp;

public partial class MainPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();
    private bool _isRefreshing = false;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Kullanıcı adını ekranda göster
        KullaniciLabel.Text = $"Merhaba, {App.AktifKullanici?.KullaniciAdi ?? "Misafir"}";

        // YETKİ KONTROLÜ: Butonları personelin yetkisine göre gizle/göster
        if (App.AktifKullanici != null)
        {
            // Mutfak butonu herkese açık
            BtnMutfak.IsVisible = true;

            // Diğer butonlar yetki durumuna göre
            BtnMasalar.IsVisible = App.AktifKullanici.MasaYetkisi;
            BtnRaporlar.IsVisible = App.AktifKullanici.RaporYetkisi;
            BtnAyarlar.IsVisible = App.AktifKullanici.AyarlarYetkisi;
        }

        await VerileriYukle();

        // Her 5 saniyede bir otomatik yenileme
        Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            if (!_isRefreshing) VerileriYukle();
            return true;
        });
    }

    private async Task VerileriYukle()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            var paket = await _servis.KasadanHerSeyiGetir();

            if (paket != null)
            {
                App.Masalar.Clear();
                foreach (var m in paket.Masalar) App.Masalar.Add(m);

                App.Urunler.Clear();
                foreach (var u in paket.Urunler) App.Urunler.Add(u);

                App.MutfakSiparisleri.Clear();
                foreach (var ms in paket.MutfakSiparisleri) App.MutfakSiparisleri.Add(ms);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Veri yükleme hatası: " + ex.Message);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async void Masalar_Clicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new KafesonApp.Views.MasalarPage());

    private async void Mutfak_Clicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new MutfakPage());

    private async void Raporlar_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new RaporPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Kritik Hata", "Rapor sayfası açılamadı:\n" + ex.Message, "Tamam");
        }
    }

    private async void Ayarlar_Clicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new AyarlarPage());

    private void Cikis_Clicked(object sender, EventArgs e)
    {
        App.AktifKullanici = null;
        Application.Current.Windows[0].Page = new NavigationPage(new LoginPage());
    }
}