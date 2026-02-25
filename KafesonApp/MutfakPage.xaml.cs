using KafesonApp.Models;

namespace KafesonApp;

public partial class MutfakPage : ContentPage
{
    public MutfakPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ListeyiGuncelle();
    }

    private void ListeyiGuncelle()
    {
        // App.xaml.cs'deki mutfak listesini eskiye göre (ilk sipariş en üstte) sıralayarak ekrana basıyoruz
        var siraliListe = App.MutfakSiparisleri.OrderBy(x => x.KayitSaati).ToList();
        MutfakListesiView.ItemsSource = siraliListe;
    }

    private void Yenile_Clicked(object sender, EventArgs e)
    {
        ListeyiGuncelle();
    }

    private void Hazir_Clicked(object sender, EventArgs e)
    {
        // Tıklanan butonu ve siparişi al
        if (sender is Button btn && btn.CommandParameter is MutfakSiparisi siparis)
        {
            // Listeden çıkar
            App.MutfakSiparisleri.Remove(siparis);
            App.VerileriKaydet(); // Kalıcı kaydet

            // Ekranı tazele
            ListeyiGuncelle();
        }
    }
}