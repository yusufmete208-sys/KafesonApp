#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using KafesonApp.Views; // Kendi Views klasörün
using Microsoft.Maui.Devices;
using System.Reflection;

namespace KafesonApp;

public partial class AyarlarPage : ContentPage
{
    public AyarlarPage()
    {
        InitializeComponent();

        if (App.AktifKullanici == null || !App.AktifKullanici.AyarlarYetkisi)
        {
            DisplayAlert("Yetkisiz Giriş", "Bu alana giriş yetkiniz bulunmamaktadır.", "Tamam");
            Navigation.PopAsync();
            return;
        }

        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            ContentArea.IsVisible = false;
        }
    }

    private async void MenuSec_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter == null) return;
        string secim = btn.CommandParameter.ToString();

        if (secim == "Geri")
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Phone && ContentArea.IsVisible)
            {
                ContentArea.IsVisible = false;
                MobilKartlarAlani.IsVisible = true;
                MobilBaslikLabel.Text = "YÖNETİM PANELİ";
                MobilAltBaslikLabel.Text = "Sistem ve Personel Kontrol Merkezi";
                ContentArea.Content = VarsayilanIcerik;
                return;
            }
            await Navigation.PopAsync();
            return;
        }

        try
        {
            object sayfaNesnesi = null;
            string baslik = "AYARLAR";

            switch (secim)
            {
                case "MenuListesi": baslik = "MENÜ LİSTESİ"; sayfaNesnesi = new MenuGosterimView(); break;
                case "YeniKayit": baslik = "YENİ ÜRÜN EKLE"; sayfaNesnesi = new YeniUrunKayitView(); break;
                case "FiyatGuncelle": baslik = "FİYAT GÜNCELLE"; sayfaNesnesi = new FiyatRevizeView(); break;
                case "MasaDuzenle": baslik = "MASA DÜZENİ"; sayfaNesnesi = new MasaYonetimView(); break;
                case "PersonelKontrol": baslik = "PERSONEL KONTROL"; sayfaNesnesi = new KullaniciYonetimPage(); break;
                case "BaglantiAyar": baslik = "BAĞLANTI VE QR"; sayfaNesnesi = new BaglantiAyarView(); break;

                // 🚨 KAPANAN MASALAR İÇİN ZORLA VERİ ÇEKME
                case "KapananMasalar":
                    baslik = "GEÇMİŞ KAYITLAR";
                    var kapananMasalar = new KapananMasalar1View();
                    await kapananMasalar.VerileriYukle();
                    sayfaNesnesi = kapananMasalar;
                    break;

                // 🚨 SİSTEM LOGLARI İÇİN ZORLA VERİ ÇEKME 🚨
                case "SistemLoglari":
                    baslik = "SİSTEM LOGLARI";
                    var loglarSayfasi = new LoglarPage();
                    await loglarSayfasi.LoglariYukle(); // LoglarPage.xaml.cs içinde public yaptığımız metod
                    sayfaNesnesi = loglarSayfasi;
                    break;
            }

            if (sayfaNesnesi == null) return;

            View gosterilecekIcerik = null;

            if (sayfaNesnesi is ContentPage cp)
            {
                gosterilecekIcerik = cp.Content;

                // Diğer sayfalar için eski tetikleyici (Reflection) çalışmaya devam etsin
                if (secim != "KapananMasalar" && secim != "SistemLoglari")
                {
                    try
                    {
                        var onAppearingMetodu = cp.GetType().GetMethod("OnAppearing", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        onAppearingMetodu?.Invoke(cp, null);
                    }
                    catch { }
                }
            }
            else if (sayfaNesnesi is View v)
            {
                gosterilecekIcerik = v;
            }

            if (gosterilecekIcerik != null)
            {
                ContentArea.Content = gosterilecekIcerik;

                if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                {
                    MobilKartlarAlani.IsVisible = false;
                    ContentArea.IsVisible = true;
                    MobilBaslikLabel.Text = baslik;
                    MobilAltBaslikLabel.Text = "Çıkmak için geri okuna basınız";
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Modül Hatası", "Sayfa yüklenirken hata oluştu: " + ex.Message, "Tamam");
        }
    }
}