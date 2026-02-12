using KafesonApp.Models;
using System.Collections.ObjectModel;

namespace KafesonApp;

public partial class UrunSilmePage : ContentPage
{
    // Ekranda görünecek olan filtreli liste
    public ObservableCollection<Urun> FiltreliUrunler { get; set; }

    public UrunSilmePage()
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
        // Ana listedeki ürünleri ekranda görünen listeye aktar
        FiltreliUrunler = new ObservableCollection<Urun>(App.Urunler);
        UrunCollectionView.ItemsSource = FiltreliUrunler;
    }
    private void UrunArama_TextChanged(object sender, TextChangedEventArgs e)
    {
        string aranan = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(aranan))
        {
            // Arama kutusu boþsa tüm listeyi göster
            UrunCollectionView.ItemsSource = App.Urunler;
        }
        else
        {
            // Yazýlan harflere göre filtrele
            var sonuc = App.Urunler.Where(x =>
                x.Ad.ToLower().Contains(aranan) ||
                x.Kategori.ToLower().Contains(aranan)).ToList();

            UrunCollectionView.ItemsSource = sonuc;
        }
    }

    // Sil Butonuna Basýldýðýnda
    private async void Sil_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;

        bool onay = await DisplayAlert("Dikkat", $"{urun.Ad} kalýcý olarak silinsin mi?", "Evet", "Hayýr");

        if (onay)
        {
            // 1. Ana listeden sil
            App.Urunler.Remove(urun);

            // 2. Dosyaya kaydet (Uygulama kapanýnca geri gelmesin)
            App.VerileriKaydet();

            // 3. Ekraný tazele
            ListeyiGuncelle();
            UrunAramaCubugu.Text = ""; // Aramayý temizle
        }
    }

    private async void GeriDon_Clicked(object sender, EventArgs e)
    {
        // Kalýcý kayýt yap ve bir önceki sayfaya dön
        App.VerileriKaydet();
        await Navigation.PopAsync();
    }
}