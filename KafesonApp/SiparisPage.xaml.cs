using KafesonApp.Models;
using System.Collections.ObjectModel;

namespace KafesonApp;

public partial class SiparisPage : ContentPage
{
    public Masa SecilenMasa { get; set; }

    public SiparisPage(Masa masa)
    {
        InitializeComponent();
        SecilenMasa = masa;
        BindingContext = this;

        // Ayarlar sayfasýndan gelen verileri yükle
        KategorileriYukle();

        // Varsa ilk kategoriyi otomatik göster
        var ilkKat = App.Urunler.FirstOrDefault()?.Kategori;
        if (!string.IsNullOrEmpty(ilkKat)) UrunleriGoster(ilkKat);

        DurumuGuncelle();
    }

    // SiparisPage.xaml.cs içine bu metodu ekleyin veya güncelleyin
    // SiparisPage.xaml.cs içinde OnAppearing metodunu güncelle
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Ödeme sayfasýndan dönüldüðünde rakamlarý ve listeyi tazeler
        DurumuGuncelle();
    }

    private void DurumuGuncelle()
    {
        // Kalan borcu hesapla ve yaz
        ToplamLabel.Text = $"{SecilenMasa.KalanTutar:N2} TL";

        // Ödeme Al / Hesabý Kapat butonu kontrolü
        if (SecilenMasa.Sepet.Count > 0)
        {
            AnaButon.Text = $"Sipariþi Onayla ({SecilenMasa.Sepet.Sum(x => x.ToplamFiyat):N2} TL)";
            AnaButon.BackgroundColor = Colors.Green;
        }
        else
        {
            AnaButon.Text = SecilenMasa.KalanTutar > 0 ? $"Ödeme Al ({SecilenMasa.KalanTutar:N2} TL)" : "Hesabý Kapat";
            AnaButon.BackgroundColor = Color.FromArgb("#2980B9");
        }
    }

    // --- DÝNAMÝK ÜRÜN YÜKLEME (Ayarlar sayfasýndan gelen veriler) ---

    private void KategorileriYukle()
    {
        KategoriContainer.Children.Clear();
        var kategoriler = App.Urunler.Select(x => x.Kategori).Distinct().ToList();

        foreach (var kat in kategoriler)
        {
            var btn = new Button { Text = kat, Margin = 2, BackgroundColor = Color.FromArgb("#34495E"), TextColor = Colors.White };
            btn.Clicked += (s, e) => UrunleriGoster(kat);
            KategoriContainer.Children.Add(btn);
        }
    }

    private void UrunleriGoster(string kategori)
    {
        UrunlerContainer.Children.Clear();
        var urunler = App.Urunler.Where(x => x.Kategori == kategori).ToList();

        foreach (var urun in urunler)
        {
            var btn = new Button
            {
                Text = $"{urun.Ad}\n{urun.Fiyat} TL",
                WidthRequest = 120,
                HeightRequest = 120,
                Margin = 5,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold
            };

            btn.Clicked += (s, e) => {
                // 1. AYNI ÜRÜN SEPETTE VAR MI KONTROL ET
                var mevcutUrun = SecilenMasa.Sepet.FirstOrDefault(x => x.Ad == urun.Ad);

                if (mevcutUrun != null)
                {
                    // Varsa miktarýný artýr (Yeni satýr açmaz)
                    mevcutUrun.Miktar++;
                }
                else
                {
                    // Yoksa yeni bir satýr olarak ekle
                    SecilenMasa.Sepet.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = 1 });
                }
                DurumuGuncelle();
            };
            UrunlerContainer.Children.Add(btn);
        }
    }

    // --- BUTON OLAYLARI (Event Handlers) ---

    private async void AnaButon_Clicked(object sender, EventArgs e)
    {
        // 1. DURUM: SEPETÝ ONAYLA
        if (SecilenMasa.Sepet.Count > 0)
        {
            foreach (var urun in SecilenMasa.Sepet)
            {
                SecilenMasa.Siparisler.Add(urun);
                App.MutfakSiparisleri.Add(new MutfakSiparisi
                {
                    MasaNo = SecilenMasa.No,
                    UrunAd = urun.Ad,
                    Miktar = urun.Miktar
                });
            }
            SecilenMasa.Sepet.Clear();
            SecilenMasa.IsDolu = true;
            App.VerileriKaydet();
        }
        // 2. DURUM: ÖDEME AL
        else if (SecilenMasa.KalanTutar > 0)
        {
            await Navigation.PushModalAsync(new OdemePage(SecilenMasa));
        }
        // 3. DURUM: HESABI KAPAT
        else
        {
            SecilenMasa.IsDolu = false;
            SecilenMasa.Siparisler.Clear();
            SecilenMasa.OdenmisTutar = 0;
            App.VerileriKaydet();
            await Navigation.PopAsync(); // Çökmeyi önleyen tekil navigasyon
        }
        DurumuGuncelle();
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