using KafesonApp.Models;
using System.Collections.ObjectModel;
using System.Linq; // LINQ iþlemleri için mutlaka olmalý

namespace KafesonApp;

public partial class SiparisPage : ContentPage
{
    public Masa SecilenMasa { get; set; }

    public SiparisPage(Masa masa)
    {
        InitializeComponent();
        SecilenMasa = masa;
        BindingContext = this;

        KategorileriYukle();

        var ilkKat = App.Urunler.FirstOrDefault()?.Kategori;
        if (!string.IsNullOrEmpty(ilkKat)) UrunleriGoster(ilkKat);

        DurumuGuncelle();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DurumuGuncelle();
    }

    private void DurumuGuncelle()
    {
        ToplamLabel.Text = $"{SecilenMasa.KalanTutar:N2} TL";

        if (SecilenMasa.Sepet.Count > 0)
        {
            AnaButon.Text = $"Sipariþi Onayla ({SecilenMasa.Sepet.Sum(x => x.ToplamFiyat):N2} TL)";
            AnaButon.BackgroundColor = Colors.Green;
        }
        else
        {
            AnaButon.Text = SecilenMasa.KalanTutar > 0.01 ? $"Ödeme Al ({SecilenMasa.KalanTutar:N2} TL)" : "Hesabý Kapat";
            AnaButon.BackgroundColor = Color.FromArgb("#2980B9");
        }
    }

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
                var sepettekiUrun = SecilenMasa.Sepet.FirstOrDefault(x => x.Ad == urun.Ad);
                if (sepettekiUrun != null) sepettekiUrun.Miktar++;
                else SecilenMasa.Sepet.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = 1 });
                DurumuGuncelle();
            };
            UrunlerContainer.Children.Add(btn);
        }
    }

    private async void AnaButon_Clicked(object sender, EventArgs e)
    {
        // 1. DURUM: SEPETTE ONAYLANMAMIÞ ÜRÜN VARSA (Sipariþi Onayla)
        if (SecilenMasa.Sepet.Count > 0)
        {
            if (SecilenMasa.AcilisZamani == null) SecilenMasa.AcilisZamani = DateTime.Now;

            // Log için sipariþ detaylarýný hazýrla
            string urunDetay = string.Join(", ", SecilenMasa.Sepet.Select(x => $"{x.Miktar}x {x.Ad}"));
            App.LogEkle($"Masa {SecilenMasa.No}: Sipariþ eklendi ({SecilenMasa.Sepet.Count} çeþit ürün: {urunDetay})", "Sipariþ");

            foreach (var urun in SecilenMasa.Sepet.ToList())
            {
                var mevcutUrun = SecilenMasa.Siparisler.FirstOrDefault(x => x.Ad == urun.Ad);
                if (mevcutUrun != null)
                    mevcutUrun.Miktar += urun.Miktar;
                else
                    SecilenMasa.Siparisler.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = urun.Miktar });

                // Mutfaða bilgi gönder
                App.MutfakSiparisleri.Add(new MutfakSiparisi { MasaNo = SecilenMasa.No, UrunAd = urun.Ad, Miktar = urun.Miktar });
            }

            SecilenMasa.Sepet.Clear();
            SecilenMasa.IsDolu = true;
            App.VerileriKaydet(); //
            await DisplayAlert("Baþarýlý", "Sipariþler mutfaða iletildi.", "Tamam");
        }
        // 2. DURUM: SEPET BOÞ AMA ÖDENMEMÝÞ TUTAR VARSA (Ödeme Al Sayfasýna Git)
        else if (SecilenMasa.KalanTutar > 0.01)
        {
            await Navigation.PushModalAsync(new OdemePage(SecilenMasa)); //
        }
        // 3. DURUM: BORÇ BÝTTÝ, MASAYI KAPAT (Arþivleme ve Günlük Sýra Sýfýrlama)
        else
        {
            // BUGÜNÜN SIRA NUMARASINI HESAPLA (Her gün 1'den baþlamasý için)
            int bugunkuSira = App.KapananMasalar.Count(x => x.KapanisZamani.Date == DateTime.Today) + 1;

            // Arþiv kaydýný oluþtur
            var yeniSatis = new Satis
            {
                SiraNo = bugunkuSira, // Günlük sýfýrlanan numara
                MasaNo = SecilenMasa.No,
                AcilisZamani = SecilenMasa.AcilisZamani,
                KapanisZamani = DateTime.Now,
                ToplamTutar = SecilenMasa.NakitBirikim + SecilenMasa.KartBirikim,
                NakitTutari = SecilenMasa.NakitBirikim,
                KartTutari = SecilenMasa.KartBirikim,
                Urunler = new List<Urun>(SecilenMasa.KapanisUrunleri)
            };

            // Kapatma iþlemini logla
            App.LogEkle($"Masa {SecilenMasa.No} kapatýldý. Günlük Sýra: {bugunkuSira}, Toplam: {yeniSatis.ToplamTutar:N2} TL", "Kapatma");

            // Arþive listenin en baþýna ekle
            App.KapananMasalar.Insert(0, yeniSatis);

            // Masayý tamamen sýfýrla
            SecilenMasa.AcilisZamani = null;
            SecilenMasa.IsDolu = false;
            SecilenMasa.Siparisler.Clear();
            SecilenMasa.KapanisUrunleri.Clear();
            SecilenMasa.NakitBirikim = 0;
            SecilenMasa.KartBirikim = 0;
            SecilenMasa.OdenmisTutar = 0;

            App.VerileriKaydet();
            await DisplayAlert("Hesap Kapandý", $"Masa arþive kaydedildi. Bugünün {bugunkuSira}. satýþý.", "Tamam");
            await Navigation.PopAsync();
        }

        DurumuGuncelle(); // Buton metnini ve renklerini tazele
    }

    private async void MasaAktar_Clicked(object sender, EventArgs e)
    {
        // 1. Masanýn dolu olup olmadýðýný kontrol et
        if (SecilenMasa == null || !SecilenMasa.IsDolu) return;

        // 2. Boþ masalarý listele
        var bosMasalar = App.Masalar.Where(m => !m.IsDolu).Select(m => $"Masa {m.No}").ToArray();
        string secim = await DisplayActionSheet("Hedef Masayý Seçin", "Ýptal", null, bosMasalar);

        // 3. Bir seçim yapýldýysa iþlemi baþlat
        if (secim != "Ýptal" && !string.IsNullOrEmpty(secim))
        {
            int hedefNo = int.Parse(secim.Replace("Masa ", ""));
            var hedefMasa = App.Masalar.First(m => m.No == hedefNo);

            // --- LOG KAYDI HAZIRLAMA VE EKLEME ---
            // Aktarýlan ürünlerin listesini oluþtur
            string aktarilanUrunler = string.Join(", ", SecilenMasa.Siparisler.Select(x => $"{x.Miktar}x {x.Ad}"));

            // Logu listeye ekle (Hata almamak için deðiþkenlerin olduðu bu blokta olmalý)
            App.LogEkle($"Masa {SecilenMasa.No} -> Masa {hedefNo} aktarýldý. Aktarýlan Ürünler: {aktarilanUrunler}", "Masa Ýþlemi");
            // -------------------------------------

            // 4. Ürünleri yeni masaya taþý
            foreach (var urun in SecilenMasa.Siparisler.ToList())
            {
                hedefMasa.Siparisler.Add(urun);
            }

            // 5. Hedef masayý güncelle
            hedefMasa.IsDolu = true;
            hedefMasa.AcilisZamani = SecilenMasa.AcilisZamani;

            // 6. Eski masayý sýfýrla
            SecilenMasa.Siparisler.Clear();
            SecilenMasa.IsDolu = false;
            SecilenMasa.AcilisZamani = null;

            // 7. Verileri kaydet ve geri dön
            App.VerileriKaydet();
            await Navigation.PopAsync();
        }
    }

    private async void MasaBirlestir_Clicked(object sender, EventArgs e)
    {
        if (SecilenMasa == null || !SecilenMasa.IsDolu) return;

        var doluMasalar = App.Masalar.Where(m => m.IsDolu && m.No != SecilenMasa.No).Select(m => $"Masa {m.No}").ToArray();
        string secim = await DisplayActionSheet("Hangi Masa ile Birleþtirilsin?", "Ýptal", null, doluMasalar);

        if (secim != "Ýptal" && !string.IsNullOrEmpty(secim))
        {
            int hedefNo = int.Parse(secim.Replace("Masa ", ""));
            var hedefMasa = App.Masalar.First(m => m.No == hedefNo);

            // Log için birleþen ürünleri al
            string birlestirilenUrunler = string.Join(", ", SecilenMasa.Siparisler.Select(x => $"{x.Miktar}x {x.Ad}"));
            App.LogEkle($"Masa {SecilenMasa.No} + Masa {hedefNo} birleþtirildi. Aktarýlanlar: {birlestirilenUrunler}", "Masa Ýþlemi");

            foreach (var urun in SecilenMasa.Siparisler.ToList())
            {
                var mevcut = hedefMasa.Siparisler.FirstOrDefault(x => x.Ad == urun.Ad);
                if (mevcut != null) mevcut.Miktar += urun.Miktar;
                else hedefMasa.Siparisler.Add(urun);
            }

            SecilenMasa.Siparisler.Clear();
            SecilenMasa.IsDolu = false;
            SecilenMasa.AcilisZamani = null;

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