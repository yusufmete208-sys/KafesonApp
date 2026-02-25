using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

public partial class KapananMasalar1View : ContentPage
{
    public KapananMasalar1View()
    {
        InitializeComponent();

        // 1. ADIM: Sayfa açýldýðýnda tarih seçiciyi bugüne ayarla
        TarihSecici.Date = DateTime.Today;

        // 2. ADIM: Sadece bugünkü verileri filtreleyip göster
        VerileriFiltrele(DateTime.Today);
    }

    private async void GeriDon_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnMasaTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var tapGesture = frame.GestureRecognizers[0] as TapGestureRecognizer;
        var secilenSatis = tapGesture.CommandParameter as Satis;

        if (secilenSatis != null)
        {
            await Navigation.PushModalAsync(new SatisDetayPage(secilenSatis));
        }
    }

    // 3. ADIM: Tarih deðiþtiðinde çalýþacak filtreleme mantýðý
    private void TarihDegisti(object sender, DateChangedEventArgs e)
    {
        VerileriFiltrele(e.NewDate);
    }

    // Ortak filtreleme metodu
    // Ortak filtreleme metodu - HATALAR DÜZELTÝLDÝ
    private void VerileriFiltrele(DateTime hedefTarih)
    {
        // KapanisZamani zaten DateTime olduðu için .HasValue ve .Value kullanmaya gerek yoktur.
        var filtrelenmis = App.KapananMasalar
            .Where(x => x.KapanisZamani.Date == hedefTarih.Date)
            .OrderByDescending(x => x.KapanisZamani)
            .ToList();

        KapananMasalarListesi.ItemsSource = filtrelenmis;
    }

    private void TumunuGoster_Clicked(object sender, EventArgs e)
    {
        // Ýsterseniz bu butonu kaldýrabilir veya tüm geçmiþi görmek için tutabilirsiniz
        KapananMasalarListesi.ItemsSource = App.KapananMasalar;
    }
}