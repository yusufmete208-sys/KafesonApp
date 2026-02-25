using KafesonApp.Models;

namespace KafesonApp;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void GirisYap_Clicked(object sender, EventArgs e)
    {
        string kadi = KadiEntry.Text;
        string sifre = SifreEntry.Text;

        if (string.IsNullOrWhiteSpace(kadi) || string.IsNullOrWhiteSpace(sifre))
        {
            await DisplayAlert("Uyarı", "Lütfen kullanıcı adı ve şifrenizi giriniz.", "Tamam");
            return;
        }

        // Listeden kullanıcıyı bul (Büyük/küçük harf duyarsız yapabilirsin istersen)
        var bulunanKullanici = App.Kullanicilar.FirstOrDefault(x => x.KullaniciAdi == kadi && x.Sifre == sifre);

        if (bulunanKullanici != null)
        {
            App.AktifKullanici = bulunanKullanici;

            // Kutuları bir sonraki giriş için temizle
            KadiEntry.Text = "";
            SifreEntry.Text = "";

            // Ana menüye git
            await Navigation.PushAsync(new MainPage());
        }
        else
        {
            await DisplayAlert("Hata", "Kullanıcı adı veya şifre hatalı!", "Tamam");
        }
    }
}