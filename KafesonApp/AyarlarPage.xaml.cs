using KafesonApp.Models; // Eðer Models klasörünüz yoksa bu satýrý da silebilirsiniz.
using System.Collections.ObjectModel;
using System.Linq;

namespace KafesonApp;

public partial class AyarlarPage : ContentPage
{
    public AyarlarPage()
    {
        InitializeComponent();
    }


    private async void MenuSec_Clicked(object sender, EventArgs e)
    {
        // Test amaçlý bu satýrý ekle. Eðer bu mesaj geliyorsa buton çalýþýyor demektir.
        // await DisplayAlert("Test", "Butona basýldý!", "Tamam");

        if (sender is not Button btn || btn.CommandParameter == null) return;

        string secim = btn.CommandParameter.ToString();

        try
        {
            switch (secim)
            {
                case "MenuListesi":
                    ContentArea.Content = new MenuGosterimView();
                    break;
                    
                case "YeniKayit":
                    ContentArea.Content = new YeniUrunKayitView();
                    break;
                case "MasaDuzenle":
                    ContentArea.Content = new MasaYonetimiView();
                    break;
                case "FiyatGuncelle": // XAML'daki CommandParameter ile ayný olmalý
                                      // Yeni oluþturduðumuz FiyatRevizeView'ý buraya baðlýyoruz
                    ContentArea.Content = new FiyatRevizeView();
                    break;
               

                case "Geri":
                    await Navigation.PopAsync();
                    break;

            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Görünüm yüklenemedi: " + ex.Message, "Tamam");
            
        }
    }

    private async void AnaSayfaDon_Clicked(object sender, EventArgs e)
    {
        // Bir önceki sayfaya (MainPage) geri döner
        await Navigation.PopAsync();
    }

   
}