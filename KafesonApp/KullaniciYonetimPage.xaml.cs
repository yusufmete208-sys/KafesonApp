#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using Microsoft.Maui.Storage; // Eklendi
using System.Collections.ObjectModel;

namespace KafesonApp;

public partial class KullaniciYonetimPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();
    public ObservableCollection<Kullanici> PersonelListesi { get; set; } = new();

    // Düzenlenen kullanıcıyı aklında tutar
    private Kullanici _duzenlenenKullanici = null;

    public KullaniciYonetimPage()
    {
        InitializeComponent();
        BindingContext = this;

        ListeyiYenile();
        ListeView.ItemsSource = PersonelListesi;
    }

    private void ListeyiYenile()
    {
        PersonelListesi.Clear();

        // Admin kullanıcısını cihaz hafızasından al ve her zaman en başa ekle
        string adminKadi = Preferences.Default.Get("AdminKadi", "admin");
        string adminSifre = Preferences.Default.Get("AdminSifre", "1234");

        PersonelListesi.Add(new Kullanici
        {
            Id = 0, // Admin'in ID'si 0 olarak kabul edilir ki silinemesin
            KullaniciAdi = adminKadi,
            Sifre = adminSifre,
            MasaYetkisi = true,
            RaporYetkisi = true,
            AyarlarYetkisi = true
        });

        // Veritabanındaki diğer personelleri ekle
        if (App.Kullanicilar != null)
        {
            foreach (var k in App.Kullanicilar)
                PersonelListesi.Add(k);
        }
    }

    private void FormuTemizle()
    {
        _duzenlenenKullanici = null;
        KullaniciAdiEntry.Text = "";
        SifreEntry.Text = "";
        MasaYetkisiSwitch.IsToggled = true;
        RaporYetkisiSwitch.IsToggled = false;
        AyarlarYetkisiSwitch.IsToggled = false;

        BtnEkle.Text = "＋  PERSONELİ SİSTEME EKLE";
        BtnEkle.BackgroundColor = Colors.Transparent;
    }

    private async void Ekle_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KullaniciAdiEntry.Text) || string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await DisplayAlert("Hata", "Lütfen tüm alanları doldurun!", "Tamam");
            return;
        }

        BtnEkle.IsEnabled = false;

        // --- GÜNCELLEME MODU ---
        if (_duzenlenenKullanici != null)
        {
            BtnEkle.Text = "GÜNCELLENİYOR...";

            if (_duzenlenenKullanici.Id == 0)
            {
                // Admin güncelleniyorsa veritabanına değil, güvenli şekilde cihaz hafızasına kaydedilir
                Preferences.Default.Set("AdminKadi", KullaniciAdiEntry.Text);
                Preferences.Default.Set("AdminSifre", SifreEntry.Text);

                await DisplayAlert("Başarılı", "Kurucu Yöneticinin (Admin) bilgileri başarıyla güncellendi.", "Tamam");
                FormuTemizle();
                ListeyiYenile();
            }
            else
            {
                // Normal personel güncelleniyorsa veritabanına gidip güncellenir
                _duzenlenenKullanici.KullaniciAdi = KullaniciAdiEntry.Text;
                _duzenlenenKullanici.Sifre = SifreEntry.Text;
                _duzenlenenKullanici.MasaYetkisi = MasaYetkisiSwitch.IsToggled;
                _duzenlenenKullanici.RaporYetkisi = RaporYetkisiSwitch.IsToggled;
                _duzenlenenKullanici.AyarlarYetkisi = AyarlarYetkisiSwitch.IsToggled;

                if (await _servis.KullaniciGuncelle(_duzenlenenKullanici.Id, _duzenlenenKullanici))
                {
                    await App.ApiVerileriniCek();
                    ListeyiYenile();
                    FormuTemizle();
                    await DisplayAlert("Başarılı", "Personel başarıyla güncellendi.", "Tamam");
                }
                else
                {
                    await DisplayAlert("Hata", "Güncelleme başarısız oldu, API bağlantınızı kontrol edin.", "Tamam");
                }
            }
        }
        // --- YENİ PERSONEL EKLEME MODU ---
        else
        {
            BtnEkle.Text = "KAYDEDİLİYOR...";

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = KullaniciAdiEntry.Text,
                Sifre = SifreEntry.Text,
                MasaYetkisi = MasaYetkisiSwitch.IsToggled,
                RaporYetkisi = RaporYetkisiSwitch.IsToggled,
                AyarlarYetkisi = AyarlarYetkisiSwitch.IsToggled
            };

            if (await _servis.KullaniciEkle(yeniKullanici))
            {
                await App.ApiVerileriniCek();
                ListeyiYenile();
                FormuTemizle();
                await DisplayAlert("Başarılı", "Yeni personel sisteme tanımlandı.", "Tamam");
            }
            else
            {
                await DisplayAlert("Hata", "Personel eklenemedi, bağlantıyı kontrol edin.", "Tamam");
            }
        }

        BtnEkle.IsEnabled = true;
    }

    private void Duzenle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Kullanici k)
        {
            _duzenlenenKullanici = k;

            // Seçilen kullanıcının bilgilerini yukarıdaki forma doldurur
            KullaniciAdiEntry.Text = k.KullaniciAdi;
            SifreEntry.Text = k.Sifre;
            MasaYetkisiSwitch.IsToggled = k.MasaYetkisi;
            RaporYetkisiSwitch.IsToggled = k.RaporYetkisi;
            AyarlarYetkisiSwitch.IsToggled = k.AyarlarYetkisi;

            BtnEkle.Text = "📝  PERSONELİ GÜNCELLE";
            BtnEkle.BackgroundColor = Color.FromArgb("#3B82F6"); // Güncelleme modunu belli etmek için mavi renk
        }
    }

    private async void Sil_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Kullanici k)
        {
            // Adminin silinmesini kesin olarak engeller
            if (k.Id == 0)
            {
                await DisplayAlert("Uyarı", "Kurucu Yönetici (Admin) hesabı sistemden silinemez!", "Tamam");
                return;
            }

            bool onayla = await DisplayAlert("Personel Sil",
                $"{k.KullaniciAdi} isimli personeli silmek istediğinize emin misiniz?",
                "Evet, Sil", "Vazgeç");

            if (onayla)
            {
                if (await _servis.KullaniciSil(k.Id))
                {
                    App.Kullanicilar.Remove(k);
                    ListeyiYenile();

                    // Eğer silinen kişiyi o an yukarıda düzenliyorsa, formu da sıfırla
                    if (_duzenlenenKullanici?.Id == k.Id) FormuTemizle();
                }
                else
                {
                    await DisplayAlert("Hata", "Silme işlemi başarısız oldu.", "Tamam");
                }
            }
        }
    }
}