using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

public partial class KapananMasalar1View : ContentPage
{
    public KapananMasalar1View()
    {
        InitializeComponent();
        KapananMasalarListesi.ItemsSource = App.KapananMasalar;
    }

    private async void GeriDon_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();

    // YENÝ VE GARANTÝ TIKLAMA METODU
    private async void OnMasaTapped(object sender, EventArgs e)
    {
        // Týklanan Frame'in içindeki Satis objesini alýyoruz
        var frame = sender as Frame;
        var tapGesture = frame.GestureRecognizers[0] as TapGestureRecognizer;
        var secilenSatis = tapGesture.CommandParameter as Satis;

        if (secilenSatis != null)
        {
            // Detay sayfasýný aç
            await Navigation.PushModalAsync(new SatisDetayPage(secilenSatis));
        }
    }

    private void TarihDegisti(object sender, DateChangedEventArgs e)
    {
        var filtrelenmis = App.KapananMasalar
            .Where(x => x.KapanisZamani.Date == e.NewDate.Date)
            .OrderByDescending(x => x.KapanisZamani)
            .ToList();
        KapananMasalarListesi.ItemsSource = filtrelenmis;
    }

    private void TumunuGoster_Clicked(object sender, EventArgs e) => KapananMasalarListesi.ItemsSource = App.KapananMasalar;
}