using KafesonApp.Models;

namespace KafesonApp;

public partial class KayitPage : ContentPage
{
    public KayitPage()
    {
        InitializeComponent();
    }

    private async void Kaydet_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KadiEntry.Text) || string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await DisplayAlert("Hata", "Kullanıcı adı ve şifre boş olamaz!", "Tamam");
            return;
        }

        // Aynı isimde kullanıcı var mı?
        if (App.Kullanicilar.Any(x => x.KullaniciAdi == KadiEntry.Text))
        {
            await DisplayAlert("Hata", "Bu kullanıcı adı zaten alınmış.", "Tamam");
            return;
        }

        // Yeni kullanıcı oluştur
        var yeniKullanici = new Kullanici
        {
            KullaniciAdi = KadiEntry.Text,
            Sifre = SifreEntry.Text,
            MasaYetkisi = CheckMasalar.IsChecked,
            RaporYetkisi = CheckRaporlar.IsChecked,
            AyarlarYetkisi = CheckAyarlar.IsChecked
        };

        App.Kullanicilar.Add(yeniKullanici);
        App.VerileriKaydet(); // Kalıcı kaydet

        await DisplayAlert("Başarılı", "Kullanıcı oluşturuldu!", "Tamam");
        await Navigation.PopAsync(); // Giriş ekranına dön
    }

    private async void Vazgec_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}