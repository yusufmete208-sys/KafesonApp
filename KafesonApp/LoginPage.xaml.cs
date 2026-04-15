#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using Microsoft.Maui.Storage;
using System;

namespace KafesonApp;

public partial class LoginPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var dinleyici = new UdpDinleyici();
        string kasaIp = await dinleyici.OtomatikKasaBulAsync();

        if (!string.IsNullOrEmpty(kasaIp))
        {
            Preferences.Default.Set("SunucuIP", kasaIp);
        }
        else
        {
            string mevcutIp = Preferences.Default.Get("SunucuIP", "");
            if (string.IsNullOrEmpty(mevcutIp) || mevcutIp == "192.168.1.108")
            {
                string girilenIp = await DisplayPromptAsync(
                    "Otomatik Bağlantı Başarısız",
                    "Kasa ağda bulunamadı. Lütfen Kasa IP adresini manuel girin:",
                    "Bağlan", "İptal", "Örn: 192.168.1.107", maxLength: 15,
                    keyboard: Keyboard.Numeric, initialValue: "192.168.1.107");

                if (!string.IsNullOrWhiteSpace(girilenIp))
                {
                    Preferences.Default.Set("SunucuIP", girilenIp);
                }
            }
        }
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

        // 🚨 HATA DÜZELTMESİ: Klavyeden Enter'a basınca sender "Button" gelmez.
        // Bu yüzden "as Button" null dönerse hata vermemesi için güvenli kontrol ekledik.
        var btn = sender as Button;
        string orijinalYazi = btn?.Text ?? "GİRİŞ YAP";

        if (btn != null)
        {
            btn.Text = "⏳ Giriş Yapılıyor...";
            btn.IsEnabled = false;
        }

        try
        {
            string adminKadi = Preferences.Default.Get("AdminKadi", "admin");
            string adminSifre = Preferences.Default.Get("AdminSifre", "1234");

            bool girisBasarili = false;

            if (kadi == adminKadi && sifre == adminSifre)
            {
                App.AktifKullanici = new Kullanici { KullaniciAdi = adminKadi, MasaYetkisi = true, RaporYetkisi = true, AyarlarYetkisi = true };
                girisBasarili = true;
            }

            await App.ApiVerileriniCek();

            if (!girisBasarili)
            {
                var bulunanKullanici = App.Kullanicilar?.FirstOrDefault(x => x.KullaniciAdi == kadi && x.Sifre == sifre);
                if (bulunanKullanici != null)
                {
                    App.AktifKullanici = bulunanKullanici;
                    girisBasarili = true;
                }
            }

            if (girisBasarili)
            {
                KadiEntry.Text = ""; SifreEntry.Text = "";
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await DisplayAlert("Hata", "Kullanıcı adı veya şifre hatalı!", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bağlantı Hatası", $"Kasaya ulaşılamadı.\n\nHata: {ex.Message}", "Tamam");
        }
        finally
        {
            if (btn != null)
            {
                btn.Text = orijinalYazi;
                btn.IsEnabled = true;
            }
        }
    }

    // 🚨 ŞİFREMİ UNUTTUM & ADMIN KURTARMA MEKANİZMASI 🚨
    private async void SifremiUnuttum_Tapped(object sender, TappedEventArgs e)
    {
        string secim = await DisplayActionSheet("Şifre Yardımı", "Vazgeç", null, "Garson Şifremi Unuttum", "Admin Şifremi Unuttum");

        if (secim == "Garson Şifremi Unuttum")
        {
            await DisplayAlert("Bilgi", "Garson şifreleri sadece yönetici paneli üzerinden değiştirilebilir.", "Tamam");
        }
        else if (secim == "Admin Şifremi Unuttum")
        {
            string kurtarmaKodu = await DisplayPromptAsync("Admin Kurtarma",
                "Admin şifresini sıfırlamak için kurtarma kodunu giriniz:",
                "Sıfırla", "İptal", "Kurtarma Kodu");

            // Bu kod dükkan sahibinin fiziksel olarak cihazın başında olduğunun kanıtıdır
            if (kurtarmaKodu == "Kafeson2026")
            {
                Preferences.Default.Set("AdminSifre", "1234");
                await DisplayAlert("Başarılı", "Admin şifresi '1234' olarak sıfırlandı. Lütfen giriş yapıp Ayarlar'dan güncelleyin.", "Tamam");
            }
            else if (!string.IsNullOrEmpty(kurtarmaKodu))
            {
                await DisplayAlert("Hata", "Yanlış kurtarma kodu!", "Tamam");
            }
        }
    }
}