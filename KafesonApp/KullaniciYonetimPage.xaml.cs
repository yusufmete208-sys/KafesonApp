using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

public partial class KullaniciYonetimPage : ContentPage
{
    public KullaniciYonetimPage()
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
        PersonelListesiView.ItemsSource = null;
        PersonelListesiView.ItemsSource = App.Kullanicilar;
    }

    // --- YENİ PERSONEL EKLEME ---
    private async void PersonelEkle_Clicked(object sender, EventArgs e)
    {
        string kadi = YeniKadiEntry.Text?.Trim();
        string sifre = YeniSifreEntry.Text?.Trim();

        if (string.IsNullOrEmpty(kadi) || string.IsNullOrEmpty(sifre))
        {
            await DisplayAlert("Hata", "Lütfen kullanıcı adı ve şifre belirleyin.", "Tamam");
            return;
        }

        if (App.Kullanicilar.Any(x => x.KullaniciAdi.ToLower() == kadi.ToLower()))
        {
            await DisplayAlert("Hata", "Bu kullanıcı adı zaten sistemde kayıtlı!", "Tamam");
            return;
        }

        var yeniPersonel = new Kullanici
        {
            KullaniciAdi = kadi,
            Sifre = sifre,
            MasaYetkisi = ChkMasa.IsChecked,
            RaporYetkisi = ChkRapor.IsChecked,
            AyarlarYetkisi = ChkAyarlar.IsChecked
        };

        App.Kullanicilar.Add(yeniPersonel);
        App.VerileriKaydet();

        YeniKadiEntry.Text = "";
        YeniSifreEntry.Text = "";
        ChkMasa.IsChecked = true;
        ChkRapor.IsChecked = false;
        ChkAyarlar.IsChecked = false;

        ListeyiGuncelle();
        await DisplayAlert("Başarılı", $"{kadi} sisteme eklendi.", "Tamam");
    }

    // --- PERSONEL SİLME ---
    private async void PersonelSil_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Kullanici seciliPersonel)
        {
            if (seciliPersonel.KullaniciAdi == "admin")
            {
                await DisplayAlert("Hata", "Ana yönetici hesabı (admin) silinemez!", "Tamam");
                return;
            }

            bool cevap = await DisplayAlert("Uyarı", $"{seciliPersonel.KullaniciAdi} adlı personeli silmek istediğinize emin misiniz?", "Evet", "Hayır");

            if (cevap)
            {
                App.Kullanicilar.Remove(seciliPersonel);
                App.VerileriKaydet();
                ListeyiGuncelle();
            }
        }
    }

    // --- DİNAMİK PERFORMANS EKRANINI AÇMA ---
    private void PersonelPerformans_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Kullanici seciliPersonel)
        {
            // İlgili personelin satışlarını Raporlar listesinden bul
            var personelinSatislari = App.SatisRaporlari?
                .Where(x => x.PersonelAdi != null && x.PersonelAdi.ToLower() == seciliPersonel.KullaniciAdi.ToLower())
                .OrderByDescending(x => x.Tarih)
                .ToList() ?? new List<SatisRaporu>();

            // İstatistikleri hesapla
            double ciro = personelinSatislari.Sum(x => x.Tutar);
            int islem = personelinSatislari.Count;

            // Ekrana yazdır
            PerfKullaniciLabel.Text = $"{seciliPersonel.KullaniciAdi.ToUpper()} PERFORMANSI";
            PerfCiroLabel.Text = $"{ciro:N2} ₺";
            PerfIslemLabel.Text = $"{islem}";
            PerfOrtalamaLabel.Text = islem > 0 ? $"{(ciro / islem):N2} ₺" : "0.00 ₺";

            // Son işlemleri listeye bağla
            PerfIslemlerListesi.ItemsSource = personelinSatislari;

            // Formu Gizle, Performans Panelini Göster (Şık animasyonsuz geçiş)
            YeniPersonelFormu.IsVisible = false;
            PerformansPaneli.IsVisible = true;
        }
    }

    // --- PERFORMANS EKRANINI KAPATMA (Tekrar Forma Dönme) ---
    private void PerformansKapat_Clicked(object sender, EventArgs e)
    {
        PerformansPaneli.IsVisible = false;
        YeniPersonelFormu.IsVisible = true;
    }
}