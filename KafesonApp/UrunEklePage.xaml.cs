using KafesonApp.Models; // Model klasöründeki Urun sýnýfýný tanýmasý için
using System.Linq;

namespace KafesonApp;

public partial class UrunEklePage : ContentPage
{
    public UrunEklePage()
    {
        InitializeComponent();

        // Mevcut kategorileri Picker'a yükle (Örn: Çay, Kahve, Yemek)
        KategoriListesiniGuncelle();
    }

    private void KategoriListesiniGuncelle()
    {
        // Sistemdeki mevcut ürünlerin kategorilerini benzersiz olarak al
        var kategoriler = App.TumUrunler.Select(u => u.Kategori).Distinct().ToList();
        KategoriPicker.ItemsSource = kategoriler;
    }

    private async void UrunKaydet_Clicked(object sender, EventArgs e)
    {
        // 1. Kategori belirleme
        string secilenKategori = KategoriPicker.SelectedItem?.ToString();
        string yeniKategori = YeniKategoriEntry.Text?.Trim();
        string finalKategori = !string.IsNullOrEmpty(yeniKategori) ? yeniKategori : secilenKategori;

        // 2. Boþluk kontrolü
        if (string.IsNullOrEmpty(finalKategori) || string.IsNullOrEmpty(UrunAdiEntry.Text) || string.IsNullOrEmpty(FiyatEntry.Text))
        {
            await DisplayAlert("Hata", "Lütfen tüm alanlarý doldurun!", "Tamam");
            return;
        }

        // 3. Ürünü listeye ekle (App.Urunler ismini kullandýk)
        App.Urunler.Add(new Urun
        {
            Ad = UrunAdiEntry.Text,
            Fiyat = double.Parse(FiyatEntry.Text),
            Kategori = finalKategori
        });

        // 4. KALICI KAYDET (App.xaml.cs içindeki metodu çaðýrýr)
        App.VerileriKaydet();

        await DisplayAlert("Baþarýlý", $"{UrunAdiEntry.Text} ürünü kaydedildi.", "Tamam");

        // 5. Picker'ý güncelle ve alanlarý temizle
        KategoriListesiniGuncelle();
        UrunAdiEntry.Text = "";
        FiyatEntry.Text = "";
        YeniKategoriEntry.Text = "";
    }




    private async void Geri_Clicked(object sender, EventArgs e) { await Navigation.PopAsync(); }
    

    private async void Ayarlar_Clicked(object sender, EventArgs e)
    {
        // Eðer Ayarlar sayfasýný açmak istiyorsan:
        await Navigation.PushAsync(new AyarlarPage());
    }

    // Ürünleri Görüntüle butonu için gereken metod
    private async void UrunleriGoruntule_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UrunListesiPage());
    }

    // XAML tarafýndaki Clicked="FiyatGuncelle_Clicked" ile birebir ayný olmalý
    private async void FiyatGuncelle_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Bilgi", "Fiyat güncelleme ekraný yakýnda eklenecek.", "Tamam");
    }


    private async void AyarlaraDon_Clicked(object sender, EventArgs e)
    {
        // Bir önceki sayfaya (Ayarlar'a) geri döner
        await Navigation.PopAsync();
    }

    private async void UrunlerListesiContainer_Clicked(object sender, EventArgs e)
    {
        // Yeni oluþturduðumuz sayfaya yönlendiriyoruz
        await Navigation.PushAsync(new UrunListesiPage());
    }

    private async void UrunSil_Clicked(object sender, EventArgs e)
    {
        // Yeni oluþturduðun sayfaya yönlendirir
        await Navigation.PushAsync(new UrunSilmePage());
    }

}