using KafesonApp.Models;
using KafesonApp.Data; // VeriServisi için eklendi
using System.Collections.ObjectModel;
using System.Linq;

namespace KafesonApp;

public partial class SiparisPage : ContentPage
{
    public Masa SecilenMasa { get; set; }
    private readonly VeriServisi _servis = new VeriServisi(); // Veri servisi tanımlandı

    bool isMobile = DeviceInfo.Idiom == DeviceIdiom.Phone;

    public SiparisPage(Masa masa)
    {
        InitializeComponent();
        SecilenMasa = masa;
        BindingContext = this;

        KategorileriYukle();
        UrunleriGoster("TÜMÜ");
        DurumuGuncelle();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DurumuGuncelle();
    }

    private void DurumuGuncelle()
    {
        ToplamLabel.Text = $"{SecilenMasa.KalanTutar:N2} ₺";

        if (SecilenMasa.Sepet.Count > 0)
        {
            AnaButon.Text = $"Siparişi Onayla ({SecilenMasa.Sepet.Sum(x => x.ToplamFiyat):N2} ₺)";
            AnaButon.BackgroundColor = Color.FromArgb("#E67E22");
        }
        else
        {
            AnaButon.Text = SecilenMasa.KalanTutar > 0.01 ? $"Ödeme Al ({SecilenMasa.KalanTutar:N2} ₺)" : "Hesabı Kapat";
            AnaButon.BackgroundColor = Color.FromArgb("#27AE60");
        }
    }

    private void KategorileriYukle()
    {
        KategoriContainer.Children.Clear();
        var btnTum = new Button
        {
            Text = "TÜMÜ",
            HeightRequest = isMobile ? 40 : 60,
            WidthRequest = isMobile ? 100 : -1,
            Padding = new Thickness(15, 0),
            BackgroundColor = Color.FromArgb("#3B82F6"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = isMobile ? 13 : 15,
            CornerRadius = isMobile ? 20 : 12
        };

        btnTum.Clicked += (s, e) => {
            KategoriRenkleriniSifirla();
            btnTum.BackgroundColor = Color.FromArgb("#3B82F6");
            UrunleriGoster("TÜMÜ");
        };

        KategoriContainer.Children.Add(btnTum);
        var kategoriler = App.Urunler.Select(x => x.Kategori).Distinct().ToList();

        foreach (var kat in kategoriler)
        {
            var btn = new Button
            {
                Text = kat,
                HeightRequest = isMobile ? 40 : 60,
                BackgroundColor = Color.FromArgb("#334155"),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = isMobile ? 13 : 15,
                CornerRadius = isMobile ? 20 : 12
            };
            btn.Clicked += (s, e) => {
                KategoriRenkleriniSifirla();
                btn.BackgroundColor = Color.FromArgb("#3B82F6");
                UrunleriGoster(kat);
            };
            KategoriContainer.Children.Add(btn);
        }
    }

    private void KategoriRenkleriniSifirla()
    {
        foreach (var child in KategoriContainer.Children)
            if (child is Button b) b.BackgroundColor = Color.FromArgb("#334155");
    }

    private void UrunleriGoster(string kategori)
    {
        UrunlerContainer.Children.Clear();
        var urunler = kategori == "TÜMÜ" ? App.Urunler.ToList() : App.Urunler.Where(x => x.Kategori == kategori).ToList();

        foreach (var urun in urunler)
        {
            var btn = new Button
            {
                Text = $"{urun.Ad}\n\n{urun.Fiyat:N2} ₺",
                WidthRequest = isMobile ? 165 : 140,
                HeightRequest = isMobile ? 100 : 110,
                Margin = new Thickness(0, 0, 0, 10),
                BackgroundColor = Colors.White,
                TextColor = Color.FromArgb("#2C3E50"),
                FontAttributes = FontAttributes.Bold,
                FontSize = isMobile ? 13 : 14,
                CornerRadius = 15,
                BorderColor = Color.FromArgb("#E2E8F0"),
                BorderWidth = 2
            };

            btn.Clicked += (s, e) => {
                var sepettekiUrun = SecilenMasa.Sepet.FirstOrDefault(x => x.Ad == urun.Ad);
                if (sepettekiUrun != null) sepettekiUrun.Miktar++;
                else SecilenMasa.Sepet.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = 1 });
                DurumuGuncelle();
            };
            UrunlerContainer.Children.Add(btn);
        }
    }

    // --- KASA SENKRONİZASYONLU ANA İŞLEMLER ---

    private async void AnaButon_Clicked(object sender, EventArgs e)
    {
        if (SecilenMasa.Sepet.Count > 0)
        {
            if (SecilenMasa.AcilisZamani == null) SecilenMasa.AcilisZamani = DateTime.Now;

            foreach (var urun in SecilenMasa.Sepet.ToList())
            {
                var mevcutUrun = SecilenMasa.Siparisler.FirstOrDefault(x => x.Ad == urun.Ad);
                if (mevcutUrun != null) mevcutUrun.Miktar += urun.Miktar;
                else SecilenMasa.Siparisler.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = urun.Miktar });

                App.MutfakSiparisleri.Add(new MutfakSiparisi { MasaNo = SecilenMasa.No, UrunAd = urun.Ad, Miktar = urun.Miktar, KayitSaati = DateTime.Now });
            }

            SecilenMasa.Sepet.Clear();
            SecilenMasa.IsDolu = true;

            // KASAYA GÖNDER
#if ANDROID
            await _servis.MasaGuncelle(SecilenMasa);
#endif
            App.VerileriKaydet();
            App.LogEkle("Sipariş Onayı", $"Masa {SecilenMasa.No} siparişleri mutfağa iletildi.");
            await DisplayAlert("Başarılı", "Siparişler iletildi.", "Tamam");
        }
        else if (SecilenMasa.KalanTutar > 0.01)
        {
            await Navigation.PushAsync(new OdemePage(SecilenMasa));
        }
        else
        {
            // HESAP KAPATMA
            foreach (var kalan in SecilenMasa.Siparisler)
            {
                var arsivUrun = SecilenMasa.KapanisUrunleri.FirstOrDefault(x => x.Ad == kalan.Ad);
                if (arsivUrun != null) arsivUrun.Miktar += kalan.Miktar;
                else SecilenMasa.KapanisUrunleri.Add(new Urun { Ad = kalan.Ad, Fiyat = kalan.Fiyat, Miktar = kalan.Miktar });
            }

            var yeniRapor = new SatisRaporu
            {
                MasaNo = SecilenMasa.No,
                Tutar = SecilenMasa.NakitBirikim + SecilenMasa.KartBirikim,
                Tarih = DateTime.Now,
                PersonelAdi = App.AktifKullanici?.KullaniciAdi ?? "Admin"
            };
            App.SatisRaporlari.Add(yeniRapor);

            SecilenMasa.IsDolu = false;
            SecilenMasa.Siparisler.Clear();
            SecilenMasa.NakitBirikim = 0;
            SecilenMasa.KartBirikim = 0;
            SecilenMasa.OdenmisTutar = 0;

            // KASAYA GÖNDER (Masa artık boşaldı)
#if ANDROID
            await _servis.MasaGuncelle(SecilenMasa);
#endif
            App.VerileriKaydet();
            await DisplayAlert("Hesap Kapandı", "Masa kapatıldı.", "Tamam");
            await Navigation.PopAsync();
        }
    }

    private async void MasaAktar_Clicked(object sender, EventArgs e)
    {
        if (SecilenMasa == null || !SecilenMasa.IsDolu) return;
        var bosMasalar = App.Masalar.Where(m => !m.IsDolu).Select(m => $"Masa {m.No}").ToArray();
        string secim = await DisplayActionSheet("Hedef Masa?", "İptal", null, bosMasalar);

        if (secim != "İptal" && !string.IsNullOrEmpty(secim))
        {
            int hedefNo = int.Parse(secim.Replace("Masa ", ""));
            var hedefMasa = App.Masalar.First(m => m.No == hedefNo);

            // Verileri aktar
            foreach (var urun in SecilenMasa.Siparisler.ToList()) hedefMasa.Siparisler.Add(urun);
            hedefMasa.IsDolu = true;
            SecilenMasa.IsDolu = false;
            SecilenMasa.Siparisler.Clear();

            // KASAYI GÜNCELLE (İki masa da değişti)
#if ANDROID
            await _servis.MasaGuncelle(SecilenMasa);
            await _servis.MasaGuncelle(hedefMasa);
#endif
            App.VerileriKaydet();
            await Navigation.PopAsync();
        }
    }

    private async void MasaBirlestir_Clicked(object sender, EventArgs e)
    {
        if (SecilenMasa == null || !SecilenMasa.IsDolu) return;
        var doluMasalar = App.Masalar.Where(m => m.IsDolu && m.No != SecilenMasa.No).Select(m => $"Masa {m.No}").ToArray();
        string secim = await DisplayActionSheet("Hangi Masa?", "İptal", null, doluMasalar);

        if (secim != "İptal" && !string.IsNullOrEmpty(secim))
        {
            int hedefNo = int.Parse(secim.Replace("Masa ", ""));
            var hedefMasa = App.Masalar.First(m => m.No == hedefNo);

            foreach (var urun in SecilenMasa.Siparisler.ToList())
            {
                var mevcut = hedefMasa.Siparisler.FirstOrDefault(x => x.Ad == urun.Ad);
                if (mevcut != null) mevcut.Miktar += urun.Miktar;
                else hedefMasa.Siparisler.Add(urun);
            }
            SecilenMasa.IsDolu = false;
            SecilenMasa.Siparisler.Clear();

            // KASAYI GÜNCELLE
#if ANDROID
            await _servis.MasaGuncelle(SecilenMasa);
            await _servis.MasaGuncelle(hedefMasa);
#endif
            App.VerileriKaydet();
            await Navigation.PopAsync();
        }
    }

    private void MiktarArtir_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null) { urun.Miktar++; DurumuGuncelle(); }
    }

    private void SilTiklandi(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null)
        {
            if (urun.Miktar > 1) urun.Miktar--;
            else SecilenMasa.Sepet.Remove(urun);
            DurumuGuncelle();
        }
    }

    private async void GeriDonTiklandi(object sender, EventArgs e) => await Navigation.PopAsync();
}