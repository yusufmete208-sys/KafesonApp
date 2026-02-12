using KafesonApp.Models; // Eðer Models klasörünüz yoksa bu satýrý da silebilirsiniz.
using System.Collections.ObjectModel;

namespace KafesonApp;

public partial class AyarlarPage : ContentPage
{
    public AyarlarPage()
    {
        InitializeComponent();
    }

    private async void MenuSec_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;

        // Týklanan butonun parametresi boþsa iþlem yapma
        if (btn.CommandParameter == null) return;

        string secim = btn.CommandParameter.ToString();

        // Sað taraftaki içeriði deðiþtiriyoruz
        // NOT: Eðer 'UrunEklePage' altý çizili kýrmýzýysa, o sayfanýn isminin doðru olduðundan emin olun.
        switch (secim)
        {
            case "UrunEkle":
                ContentArea.Content = new UrunEklePage().Content;
                break;

            case "UrunListesi":
                ContentArea.Content = new UrunListesiPage().Content;
                break;

            case "FiyatGuncelle":
                ContentArea.Content = new FiyatGuncellePage().Content;
                break;

            case "MasaDuzenle":
                ContentArea.Content = new MasaYonetimiPage().Content;
                break;

            case "Personel":
                await DisplayAlert("Bilgi", "Personel yetkilendirme ekraný yakýnda eklenecek.", "Tamam");
                break;
        }
    }

    private async void AnaSayfaDon_Clicked(object sender, EventArgs e)
    {
        // Bir önceki sayfaya (MainPage) geri döner
        await Navigation.PopAsync();
    }
}