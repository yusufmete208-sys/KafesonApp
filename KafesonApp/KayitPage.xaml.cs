using Kafeson.Shared.Models; // DÜZELTİLDİ
using KafesonApp.Data;

namespace KafesonApp;

public partial class KayitPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();

    public KayitPage()
    {
        InitializeComponent();
    }

    private async void KayitOl_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KullaniciAdiEntry.Text) || string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await DisplayAlert("Hata", "Bilgileri doldurunuz.", "Tamam");
            return;
        }

        var yeniKullanici = new Kullanici
        {
            KullaniciAdi = KullaniciAdiEntry.Text,
            Sifre = SifreEntry.Text,
            MasaYetkisi = true,
            RaporYetkisi = false,
            AyarlarYetkisi = false
        };

        if (await _servis.KullaniciEkle(yeniKullanici))
        {
            await DisplayAlert("Başarılı", "Kayıt tamamlandı.", "Tamam");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Hata", "Kullanıcı adı alınmış olabilir.", "Tamam");
        }
    }
}