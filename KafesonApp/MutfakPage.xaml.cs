using Kafeson.Shared.Models;
using KafesonApp.Data;

namespace KafesonApp.Views; // XAML ile aynı olmalı!

public partial class MutfakPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();
    private bool _sayfaAktif = false;

    public MutfakPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _sayfaAktif = true;
        await ListeyiGuncelle();
        OtomatikYenile();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _sayfaAktif = false;
    }

    private async void OtomatikYenile()
    {
        while (_sayfaAktif)
        {
            await Task.Delay(5000);
            if (_sayfaAktif) await ListeyiGuncelle();
        }
    }

    private async Task ListeyiGuncelle()
    {
        var siparisler = await _servis.MutfakSiparisleriGetir();

        // ÇİFTE GÜVENLİK: API'den sızsa bile, ekrana "Hazır" olanları yansıtma
        var siraliListe = siparisler.Where(x => x.Durum != "Hazır")
                                    .OrderBy(x => x.SiparisZamani)
                                    .ToList();

        MutfakListesiView.ItemsSource = siraliListe;
    }

    private async void Yenile_Clicked(object sender, EventArgs e)
    {
        await ListeyiGuncelle();
    }

    private async void Hazir_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is MutfakSiparisi siparis)
        {
            // İşlem başlarken butonu kilitle
            btn.IsEnabled = false;
            btn.Text = "Bekleyin...";

            string sonuc = await _servis.MutfakDurumGuncelle(siparis.Id, "Hazır");

            if (sonuc == "OK")
            {
                // İşlem başarılıysa listeyi yenile, sipariş anında ekrandan uçacak
                await ListeyiGuncelle();
            }
            else
            {
                // İşlem başarısız olursa butonu kilitli bırakma, eski haline getir
                btn.IsEnabled = true;
                btn.Text = "HAZIR ✓";

                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlert("Hata", "İşlem başarısız", "Tamam");
            }
        }
    }

    // 🚨 HATA VERMESİNE SEBEP OLAN EKSİK METOT BURASIYDI 🚨
    private async void Geri_Clicked(object sender, EventArgs e)
    {
        // Önceki sayfaya (Ayarlar veya Menü) güvenli bir şekilde geri dönmeyi sağlar
        await Navigation.PopAsync();
    }
}