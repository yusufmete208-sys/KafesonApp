using KafesonApp.Models;

namespace KafesonApp;

public partial class SatisDetayPage : ContentPage
{
    public SatisDetayPage(Satis secilenSatis)
    {
        InitializeComponent();

        MasaBaslikLabel.Text = $"MASA {secilenSatis.MasaNo} DETAYI";
        ZamanLabel.Text = $"{secilenSatis.KapanisZamani:dd/MM/yyyy HH:mm}";

        UrunlerListesi.ItemsSource = secilenSatis.Urunler;

        NakitLabel.Text = secilenSatis.NakitTutari.ToString("N2") + " TL";
        KartLabel.Text = secilenSatis.KartTutari.ToString("N2") + " TL";
        ToplamLabel.Text = secilenSatis.ToplamTutar.ToString("N2") + " TL";
    }

    private async void Kapat_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Sayfayý kapatýr
    }
}