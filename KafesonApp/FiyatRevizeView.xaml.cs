using Kafeson.Shared.Models; // DÜZELTİLDİ
using KafesonApp.Data;

namespace KafesonApp;

public partial class FiyatRevizeView : ContentView
{
    private readonly VeriServisi _servis = new VeriServisi();

    public FiyatRevizeView()
    {
        InitializeComponent();
        UrunListesi.ItemsSource = App.Urunler;
    }

    private async void Guncelle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Urun urun)
        {
            string sonuc = await Application.Current.MainPage.DisplayPromptAsync("Fiyat Güncelle", $"{urun.Ad} için yeni fiyat:", initialValue: urun.Fiyat.ToString());

            if (double.TryParse(sonuc, out double yeniFiyat))
            {
                if (await _servis.FiyatGuncelle(urun.Id, yeniFiyat))
                {
                    urun.Fiyat = yeniFiyat;
                    await Application.Current.MainPage.DisplayAlert("Başarılı", "Fiyat güncellendi.", "Tamam");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Bağlantı hatası.", "Tamam");
                }
            }
        }
    }
}