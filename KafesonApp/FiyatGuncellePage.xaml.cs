using KafesonApp.Models;

namespace KafesonApp;

public partial class FiyatGuncellePage : ContentPage
{
    public FiyatGuncellePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UrunCollectionView.ItemsSource = App.Urunler.ToList();
    }

    // 1. Arama Fonksiyonu
    private void UrunAraCubugu_TextChanged(object sender, TextChangedEventArgs e)
    {
        string aranan = e.NewTextValue?.ToLower() ?? "";
        UrunCollectionView.ItemsSource = App.Urunler
            .Where(x => x.Ad.ToLower().Contains(aranan) || x.Kategori.ToLower().Contains(aranan))
            .ToList();
    }

    // 2. Toplu Zam Uygulama
    private async void TopluZamUygula_Clicked(object sender, EventArgs e)
    {
        if (double.TryParse(TopluZamEntry.Text, out double oran))
        {
            foreach (var urun in App.Urunler)
            {
                // Mevcut fiyatý oran kadar artýr
                urun.Fiyat = Math.Round(urun.Fiyat * (1 + (oran / 100)), 2);
            }

            UrunCollectionView.ItemsSource = null;
            UrunCollectionView.ItemsSource = App.Urunler.ToList();
            TopluZamEntry.Text = "";
            await DisplayAlert("Baþarýlý", "Tüm menüye zam uygulandý. Kaydetmeyi unutmayýn!", "Tamam");
        }
    }

    // 3. Deðiþiklikleri Kaydet
    private async void Kaydet_Clicked(object sender, EventArgs e)
    {
        // Merkezi listeyi dosyaya mühürle
        App.VerileriKaydet();
        await DisplayAlert("Bilgi", "Fiyatlar kalýcý olarak güncellendi.", "Tamam");
    }

    private async void GeriDon_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();
}