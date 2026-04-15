using Kafeson.Shared.Models;
using KafesonApp.Data;
using System.Linq;

namespace KafesonApp;

public partial class KapananMasalar1View : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();

    public KapananMasalar1View()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await VerileriYukle();
    }

    public async Task VerileriYukle()
    {
        try
        {
            var raporlar = await _servis.RaporlariGetir();
            if (raporlar != null)
            {
                var liste = raporlar.OrderByDescending(x => x.Tarih).ToList();
                GecmisListesi.ItemsSource = liste;

                double toplam = liste.Sum(x => x.Tutar);
                int adet = liste.Count;

                if (IslemSayisiLabel != null) IslemSayisiLabel.Text = $"{adet} İşlem";
                if (ToplamTutarLabel != null) ToplamTutarLabel.Text = $"{toplam:N2} ₺";
            }
        }
        catch (Exception ex)
        {
            // 🚨 EĞER EKRAN BOŞ KALIRSA ARTIK SEBEBİNİ SANA SÖYLEYECEK:
            await DisplayAlert("Gizli Hata Yakalandı", $"Veriler yüklenirken arka planda şu sorun çıktı:\n{ex.Message}", "Tamam");
        }
    }
}