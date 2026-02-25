using KafesonApp.Models;

namespace KafesonApp;

public partial class AyarlarPage : ContentPage
{
    public AyarlarPage()
    {
        InitializeComponent();

        // --- GÜVENLİK KONTROLÜ ---
        // Eğer giriş yapan yoksa veya 'AyarlarYetkisi' (Yönetim Paneli) izni yoksa içeri alma.
        if (App.AktifKullanici == null || !App.AktifKullanici.AyarlarYetkisi)
        {
            DisplayAlert("Yetkisiz Giriş", "Bu alana giriş yetkiniz bulunmamaktadır.", "Tamam");
            Navigation.PopAsync(); // Ana menüye geri at
        }
    }

    private async void MenuSec_Clicked(object sender, EventArgs e)
    {
        // Tıklanan butonu ve parametresini alıyoruz
        if (sender is not Button btn || btn.CommandParameter == null) return;

        string secim = btn.CommandParameter.ToString();

        try
        {
            switch (secim)
            {
                case "MenuListesi":
                    // Sağ taraftaki alana (ContentArea) Menü Listesini getirir
                    ContentArea.Content = new MenuGosterimView();
                    break;

                case "YeniKayit":
                    // Ürün Ekleme ekranını getirir
                    ContentArea.Content = new YeniUrunKayitView();
                    break;

                case "MasaDuzenle":
                    // Masa Ekle/Çıkar ekranını getirir
                    ContentArea.Content = new MasaYonetimView();
                    break;

                case "FiyatGuncelle":
                    // Fiyat Güncelleme ekranını getirir
                    ContentArea.Content = new FiyatRevizeView();
                    break;

                case "PersonelKontrol":
                    // Personel Yönetimi sayfası tam ekran açılır
                    await Navigation.PushAsync(new KullaniciYonetimPage());
                    break;

                case "KapananMasalar":
                    // Kapanan Masalar (Geçmiş) sayfası tam ekran açılır
                    await Navigation.PushAsync(new KapananMasalar1View());
                    break;

                case "SistemLoglari":
                    // 🟢 YENİ EKLENEN: Sistem Logları sayfası tam ekran açılır
                    await Navigation.PushAsync(new SistemLogsPage());
                    break;

                case "Geri":
                    // Ana Sayfaya (MainPage) geri döner
                    await Navigation.PopAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Sayfa yüklenirken bir sorun oluştu: " + ex.Message, "Tamam");
        }
    }
}