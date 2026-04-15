using Kafeson.Shared.Models; // DÜZELTİLDİ: Artık Shared modellerini kullanıyor
using KafesonApp.Data;

namespace KafesonApp;

public partial class MenuGosterimView : ContentView
{
    private readonly VeriServisi _servis = new VeriServisi();

    public MenuGosterimView()
    {
        InitializeComponent();
        UrunListesi.ItemsSource = App.Urunler;
    }

    private async void OnUrunSilClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Urun urun)
        {
            bool onay = await App.Current.Windows[0].Page.DisplayAlert("Onay", $"{urun.Ad} silinsin mi?", "Evet", "Hayır");
            if (onay)
            {
                if (await _servis.UrunSil(urun.Id))
                {
                    App.Urunler.Remove(urun);
                    // LogEkle metoduna 3 parametre gönderiyoruz: İşlem, Detay, MasaNo (Yönetim olduğu için 0)
                    await App.LogEkle("Ürün Silindi", urun.Ad, 0);
                }
            }
        }
    }
}