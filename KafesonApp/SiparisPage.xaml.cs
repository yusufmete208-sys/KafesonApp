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
                // 1. ADIM: SEPETTE ÜRÜN VAR MI KONTROL ET
                var sepettekiUrun = SecilenMasa.Sepet.FirstOrDefault(x => x.Ad == urun.Ad);

                if (sepettekiUrun != null)
                {
                    sepettekiUrun.Miktar++; // Varsa miktar artýr
                }
                else
                {
                    // Yoksa yeni ekle
                    SecilenMasa.Sepet.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = 1 });
                }
                DurumuGuncelle();
            };
            UrunlerContainer.Children.Add(btn);
        }
    }

    // SiparisPage.xaml.cs içindeki AnaButon_Clicked metodunu güncelle
    private async void AnaButon_Clicked(object sender, EventArgs e)
    {
        // 1. DURUM: SEPETTE SÝPARÝÞ VARSA (MUTFAÐA GÖNDER)
        if (SecilenMasa.Sepet.Count > 0)
        {
            // Masa yeni açýlýyorsa saatini baþlat
            if (SecilenMasa.AcilisZamani == null)
            {
                SecilenMasa.AcilisZamani = DateTime.Now;
            }

            foreach (var urun in SecilenMasa.Sepet.ToList())
            {
                // Ayný ürün masada varsa üstüne ekle (Satýr birleþtirme)
                var mevcutUrun = SecilenMasa.Siparisler.FirstOrDefault(x => x.Ad == urun.Ad);
                if (mevcutUrun != null)
                {
                    mevcutUrun.Miktar += urun.Miktar;
                }
                else
                {
                    // Yoksa yeni olarak ekle
                    SecilenMasa.Siparisler.Add(new Urun
                    {
                        Ad = urun.Ad,
                        Fiyat = urun.Fiyat,
                        Miktar = urun.Miktar
                    });
                }

                // Mutfak listesine ekle
                App.MutfakSiparisleri.Add(new MutfakSiparisi
                {
                    MasaNo = SecilenMasa.No,
                    UrunAd = urun.Ad,
                    Miktar = urun.Miktar
                });
            }

            // Sepeti temizle ve masayý dolu yap
            SecilenMasa.Sepet.Clear();
            SecilenMasa.IsDolu = true;

            App.VerileriKaydet();
            await DisplayAlert("Baþarýlý", "Sipariþler mutfaða iletildi.", "Tamam");
        }

        // 2. DURUM: SEPET BOÞ AMA BORÇ VAR (ÖDEME AL)
        else if (SecilenMasa.KalanTutar > 0.01)
        {
            await Navigation.PushModalAsync(new OdemePage(SecilenMasa));
        }

        // 3. DURUM: BORÇ BÝTTÝ (HESABI KAPAT VE ARÞÝVLE)
        else
        {
            // --- KRÝTÝK NOKTA: ÜRÜNLERÝN BAÐIMSIZ KOPYASINI AL ---
            // Bu iþlem yapýlmazsa, masa sýfýrlandýðýnda arþivdeki ürünler de silinir!
            var urunKopyalari = SecilenMasa.Siparisler.Select(u => new Urun
            {
                Ad = u.Ad,
                Fiyat = u.Fiyat,
                Miktar = u.Miktar
            }).ToList();

            // Toplam tutarý kopyalanan ürünlerden hesapla (Hata payý sýfýr)
            double hesaplananToplam = urunKopyalari.Sum(x => x.Fiyat * x.Miktar);

            // Yeni satýþ kaydý oluþtur
            var yeniSatis = new Satis
            {
                MasaNo = SecilenMasa.No,
                AcilisZamani = SecilenMasa.AcilisZamani,
                KapanisZamani = DateTime.Now,
                ToplamTutar = hesaplananToplam,
                Urunler = urunKopyalari // Kopyalanmýþ listeyi veriyoruz
            };

            // Kapanan masalar listesinin en baþýna ekle
            App.KapananMasalar.Insert(0, yeniSatis);

            // ÞÝMDÝ MASAYI GÜVENLE SIFIRLA
            SecilenMasa.AcilisZamani = null;
            SecilenMasa.IsDolu = false;
            SecilenMasa.Siparisler.Clear();
            SecilenMasa.OdenmisTutar = 0;

            // Verileri telefona kaydet
            App.VerileriKaydet();

            await DisplayAlert("Hesap Kapandý", "Masa arþive eklendi.", "Tamam");
            await Navigation.PopAsync();
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
    // MASA AKTAR BUTONU
    private async void MasaAktar_Clicked(object sender, EventArgs e)
    {
        // _masa yerine SecilenMasa kullanýyoruz
        if (SecilenMasa == null || !SecilenMasa.IsDolu)
        {
            await DisplayAlert("Uyarý", "Aktarýlacak sipariþ bulunamadý.", "Tamam");
            return;
        }

        // Boþ masalarý bul (App.Masalar üzerinden)
        var bosMasalar = App.Masalar.Where(m => !m.IsDolu).Select(m => $"Masa {m.No}").ToArray();

        if (bosMasalar.Length == 0)
        {
            await DisplayAlert("Hata", "Aktarýlacak boþ masa bulunamadý.", "Tamam");
            return;
        }

        string secim = await DisplayActionSheet("Hedef Masayý Seçin", "Ýptal", null, bosMasalar);

        if (secim != "Ýptal" && !string.IsNullOrEmpty(secim))
        {
            int hedefNo = int.Parse(secim.Replace("Masa ", ""));
            var hedefMasa = App.Masalar.First(m => m.No == hedefNo);

            // Sipariþleri taþý
            foreach (var urun in SecilenMasa.Siparisler.ToList())
            {
                hedefMasa.Siparisler.Add(urun);
            }

            hedefMasa.IsDolu = true;
            SecilenMasa.Siparisler.Clear();
            SecilenMasa.IsDolu = false;

            App.VerileriKaydet(); // Deðiþiklikleri kalýcý hale getir
            await DisplayAlert("Baþarýlý", $"Sipariþler Masa {hedefNo} konumuna aktarýldý.", "Tamam");
            await Navigation.PopAsync();
        }
    }

    // MASA BÝRLEÞTÝR BUTONU
    private async void MasaBirlestir_Clicked(object sender, EventArgs e)
    {
        if (SecilenMasa == null || !SecilenMasa.IsDolu) return;

        // Diðer dolu masalarý bul
        var doluMasalar = App.Masalar
            .Where(m => m.IsDolu && m.No != SecilenMasa.No)
            .Select(m => $"Masa {m.No}")
            .ToArray();

        if (doluMasalar.Length == 0)
        {
            await DisplayAlert("Bilgi", "Birleþtirilecek baþka dolu masa yok.", "Tamam");
            return;
        }

        string secim = await DisplayActionSheet("Hangi Masa ile Birleþtirilsin?", "Ýptal", null, doluMasalar);

        if (secim != "Ýptal" && !string.IsNullOrEmpty(secim))
        {
            int hedefNo = int.Parse(secim.Replace("Masa ", ""));
            var hedefMasa = App.Masalar.First(m => m.No == hedefNo);

            foreach (var urun in SecilenMasa.Siparisler.ToList())
            {
                var mevcut = hedefMasa.Siparisler.FirstOrDefault(x => x.Ad == urun.Ad);
                if (mevcut != null)
                    mevcut.Miktar += urun.Miktar;
                else
                    hedefMasa.Siparisler.Add(urun);
            }

            SecilenMasa.Siparisler.Clear();
            SecilenMasa.IsDolu = false;

            App.VerileriKaydet(); // Deðiþiklikleri kalýcý hale getir
            await DisplayAlert("Baþarýlý", $"Masalar Masa {hedefNo} altýnda birleþtirildi.", "Tamam");
            await Navigation.PopAsync();
        }
    }


    private async void GeriDonTiklandi(object sender, EventArgs e) => await Navigation.PopAsync();
}