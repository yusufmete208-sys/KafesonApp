#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using System.Collections.ObjectModel;

namespace KafesonApp;

public partial class SiparisPage : ContentPage
{
    public Masa SecilenMasa { get; set; }
    private readonly VeriServisi _servis = new VeriServisi();

    // Ödeme alındıktan sonra masayı kapatmak için bekleme durumu
    private bool _odemeAlindi = false;

    public SiparisPage(Masa masa)
    {
        InitializeComponent();
        SecilenMasa = masa;
        BindingContext = this;

        if (SecilenMasa.Sepet == null) SecilenMasa.Sepet = new ObservableCollection<Urun>();
        if (SecilenMasa.Siparisler == null) SecilenMasa.Siparisler = new ObservableCollection<Urun>();

        KategorileriYukle();
        UrunleriGoster("Tümü");
        DurumuGuncelle();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // OdemePage'den geri dönüldüğünde durumu yenile
        // Eğer ödeme alındıysa buton "Masayı Kapat" olarak güncellenir
        DurumuGuncelle();
    }

    private void KategorileriYukle()
    {
        KategoriContainer.Children.Clear();
        var kategoriler = new List<string> { "Tümü" };
        if (App.Urunler != null)
            kategoriler.AddRange(App.Urunler.Select(x => x.Kategori).Distinct().ToList());

        foreach (var kat in kategoriler)
        {
            // Kategori butonu — yeni beyaz tema stiline uygun
            var border = new Border
            {
                BackgroundColor = Color.FromArgb("#F1F5F9"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var btn = new Button
            {
                Text = kat,
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#64748B"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                HeightRequest = 44,
                HorizontalOptions = LayoutOptions.Fill,
                Padding = new Thickness(10, 0)
            };

            btn.Clicked += (s, e) =>
            {
                // Tüm butonları sıfırla
                foreach (var child in KategoriContainer.Children.OfType<Border>())
                {
                    child.BackgroundColor = Color.FromArgb("#F1F5F9");
                    child.Stroke = new SolidColorBrush(Color.FromArgb("#E2E8F0"));
                    if (child.Content is Button b)
                    {
                        b.TextColor = Color.FromArgb("#64748B");
                    }
                }
                // Seçili olanı vurgula
                border.BackgroundColor = Color.FromArgb("#FFF7ED");
                border.Stroke = new SolidColorBrush(Color.FromArgb("#FED7AA"));
                btn.TextColor = Color.FromArgb("#E67E22");

                UrunleriGoster(kat);
            };

            border.Content = btn;
            KategoriContainer.Children.Add(border);
        }

        // İlk kategoriyi (Tümü) seçili göster
        if (KategoriContainer.Children.FirstOrDefault() is Border ilk)
        {
            ilk.BackgroundColor = Color.FromArgb("#FFF7ED");
            ilk.Stroke = new SolidColorBrush(Color.FromArgb("#FED7AA"));
            if (ilk.Content is Button b) b.TextColor = Color.FromArgb("#E67E22");
        }
    }

    private void UrunleriGoster(string kategori)
    {
        UrunlerContainer.Children.Clear();
        if (App.Urunler == null) return;

        var liste = App.Urunler
            .Where(x => x.Fiyat > 0 && (kategori == "Tümü" || x.Kategori == kategori))
            .ToList();

        foreach (var urun in liste)
        {
            // Ürün kartı — beyaz tema
            var cardBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
                WidthRequest = 100,
                HeightRequest = 100,
                Margin = new Thickness(5),
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb("#CBD5E1")),
                    Offset = new Point(0, 2),
                    Radius = 8,
                    Opacity = 0.15f
                }
            };

            var inner = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 2,
                Padding = new Thickness(6)
            };

            inner.Children.Add(new Label
            {
                Text = urun.Ad,
                TextColor = Color.FromArgb("#0F172A"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2
            });

            inner.Children.Add(new Label
            {
                Text = $"{urun.Fiyat:N2}₺",
                TextColor = Color.FromArgb("#E67E22"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center
            });

            // Tıklanabilir buton üst üste
            var btn = new Button
            {
                BackgroundColor = Colors.Transparent,
                WidthRequest = 100,
                HeightRequest = 100,
                Padding = 0,
                Opacity = 0   // Görünmez, sadece tıklama algılar
            };

            var overlay = new Grid();
            overlay.Children.Add(inner);
            overlay.Children.Add(btn);
            cardBorder.Content = overlay;

            btn.Clicked += (s, e) =>
            {
                var sepetteki = SecilenMasa.Sepet.FirstOrDefault(x => x.Ad == urun.Ad);
                if (sepetteki != null)
                {
                    int index = SecilenMasa.Sepet.IndexOf(sepetteki);
                    SecilenMasa.Sepet.RemoveAt(index);
                    sepetteki.Miktar++;
                    SecilenMasa.Sepet.Insert(index, sepetteki);
                }
                else
                {
                    SecilenMasa.Sepet.Add(new Urun
                    {
                        Ad = urun.Ad,
                        Fiyat = urun.Fiyat,
                        Miktar = 1,
                        Kategori = urun.Kategori,
                        MasaNo = SecilenMasa.No
                    });
                }
                DurumuGuncelle();
            };

            UrunlerContainer.Children.Add(cardBorder);
        }
    }

    private async void AnaButon_Clicked(object sender, EventArgs e)
    {
        // ── DURUM 1: Sepette ürün var → Siparişi Onayla ──
        if (SecilenMasa.Sepet.Count > 0)
        {
            if (SecilenMasa.AcilisZamani == null)
                SecilenMasa.AcilisZamani = DateTime.Now;

            foreach (var u in SecilenMasa.Sepet.ToList())
            {
                var varolan = SecilenMasa.Siparisler.FirstOrDefault(x => x.Ad == u.Ad);
                if (varolan != null) varolan.Miktar += u.Miktar;
                else SecilenMasa.Siparisler.Add(u);

                _ = _servis.MutfagaGonder(SecilenMasa.No, u.Ad, u.Miktar);
            }

            SecilenMasa.Sepet.Clear();
            SecilenMasa.IsDolu = true;
            _odemeAlindi = false; // Yeni sipariş eklendi, ödeme sıfırla

            await _servis.MasaGuncelle(SecilenMasa.No, SecilenMasa);
            await DisplayAlert("Başarılı", "Sipariş mutfağa iletildi.", "Tamam");
        }

        // ── DURUM 2: Ödeme alındı, masa kapatılmayı bekliyor → Masayı Kapat ──
        else if (_odemeAlindi)
        {
            if (await DisplayAlert("Masayı Kapat", "Masa tamamen kapatılsın mı?", "Evet, Kapat", "Hayır"))
            {
                SecilenMasa.IsDolu = false;
                SecilenMasa.Siparisler.Clear();
                SecilenMasa.OdenmisTutar = 0;
                SecilenMasa.AcilisZamani = null;
                _odemeAlindi = false;

                await _servis.MasaGuncelle(SecilenMasa.No, SecilenMasa);
                await Navigation.PopAsync();
            }
        }

        // ── DURUM 3: Kalan borç var → Ödeme Al (OdemePage'e git) ──
        else if (SecilenMasa.KalanTutar > 0.01)
        {
            await Navigation.PushAsync(new OdemePage(SecilenMasa));
            // OdemePage'den döndüğünde OnAppearing → DurumuGuncelle çalışır
            // Eğer borç sıfırlandıysa _odemeAlindi = true yapılır
        }

        // ── DURUM 4: Sepet boş, borç yok → Masayı Kapat ──
        else
        {
            if (await DisplayAlert("Kapat", "Masa kapatılsın mı?", "Evet", "Hayır"))
            {
                SecilenMasa.IsDolu = false;
                SecilenMasa.Siparisler.Clear();
                SecilenMasa.OdenmisTutar = 0;
                SecilenMasa.AcilisZamani = null;

                await _servis.MasaGuncelle(SecilenMasa.No, SecilenMasa);
                await Navigation.PopAsync();
            }
        }

        DurumuGuncelle();
    }

    private async void MasaAktar_Clicked(object sender, EventArgs e)
    {
        var digerMasalar = App.Masalar?.Where(m => m.No != SecilenMasa.No).ToList();

        if (digerMasalar == null || !digerMasalar.Any())
        {
            await DisplayAlert("Bilgi", "Aktarım yapılabilecek başka masa bulunamadı.", "Tamam");
            return;
        }

        string[] secenekler = digerMasalar
            .Select(m => $"Masa {m.No} {(m.IsDolu ? "(Dolu)" : "(Boş)")}")
            .ToArray();

        string secilen = await DisplayActionSheet("Hangi masaya aktarılacak?", "İptal", null, secenekler);

        if (secilen != "İptal" && !string.IsNullOrEmpty(secilen))
        {
            string numaraStr = secilen.Split(' ')[1];
            if (int.TryParse(numaraStr, out int hedefNo))
            {
                bool basarili = await _servis.MasaAktar(SecilenMasa.No, hedefNo);
                if (basarili)
                {
                    await DisplayAlert("Başarılı", $"Masa {SecilenMasa.No}, Masa {hedefNo} numaraya aktarıldı.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                    await DisplayAlert("Hata", "Masa aktarılamadı. Hedef masa zaten dolu olabilir.", "Tamam");
            }
        }
    }

    private async void MasaBirlestir_Clicked(object sender, EventArgs e)
    {
        var acikMasalar = App.Masalar?.Where(m => m.No != SecilenMasa.No && m.IsDolu).ToList();

        if (acikMasalar == null || !acikMasalar.Any())
        {
            await DisplayAlert("Bilgi", "Birleştirilecek başka açık (dolu) masa yok.", "Tamam");
            return;
        }

        string[] secenekler = acikMasalar
            .Select(m => $"Masa {m.No} (Açık - {m.KalanTutar}₺)")
            .ToArray();

        string secilen = await DisplayActionSheet("Hangi masa ile birleştirilecek?", "İptal", null, secenekler);

        if (secilen != "İptal" && !string.IsNullOrEmpty(secilen))
        {
            string numaraStr = secilen.Split(' ')[1];
            if (int.TryParse(numaraStr, out int hedefNo))
            {
                bool basarili = await _servis.MasaBirlestir(SecilenMasa.No, hedefNo);
                if (basarili)
                {
                    await DisplayAlert("Başarılı", $"Hesaplar Masa {hedefNo} üzerinde birleştirildi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                    await DisplayAlert("Hata", "Masalar birleştirilirken bir hata oluştu.", "Tamam");
            }
        }
    }

    private void DurumuGuncelle()
    {
        // Anlık toplam hesapla
        double sepetToplami = SecilenMasa.Sepet.Sum(x => x.Fiyat * x.Miktar);
        double genelToplam = SecilenMasa.KalanTutar + sepetToplami;
        ToplamLabel.Text = $"{genelToplam:N2} ₺";

        // OdemePage'den döndükten sonra: borç sıfırlandıysa ve sepet boşsa ödeme alındı
        if (SecilenMasa.Sepet.Count == 0 && SecilenMasa.KalanTutar <= 0.01 && SecilenMasa.IsDolu)
        {
            _odemeAlindi = true;
        }

        // ── Buton durumu ──
        if (SecilenMasa.Sepet.Count > 0)
        {
            // Sepette ürün var → Onayla
            AnaButon.Text = "✓  Siparişi Onayla";
            AnaButonBorder.BackgroundColor = Color.FromArgb("#E67E22");
            AnaButonBorder.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#E67E22")),
                Offset = new Point(0, 4),
                Radius = 14,
                Opacity = 0.35f
            };
        }
        else if (_odemeAlindi)
        {
            // Ödeme alındı, masa kapanmayı bekliyor
            AnaButon.Text = "✕  Masayı Kapat";
            AnaButonBorder.BackgroundColor = Color.FromArgb("#DC2626");
            AnaButonBorder.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#DC2626")),
                Offset = new Point(0, 4),
                Radius = 14,
                Opacity = 0.3f
            };
        }
        else if (SecilenMasa.KalanTutar > 0.01)
        {
            // Borç var → Ödeme Al
            AnaButon.Text = "💳  Ödeme Al";
            AnaButonBorder.BackgroundColor = Color.FromArgb("#16A34A");
            AnaButonBorder.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#16A34A")),
                Offset = new Point(0, 4),
                Radius = 14,
                Opacity = 0.3f
            };
        }
        else
        {
            // Her şey tamam → Masayı Kapat
            AnaButon.Text = "✕  Masayı Kapat";
            AnaButonBorder.BackgroundColor = Color.FromArgb("#DC2626");
            AnaButonBorder.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#DC2626")),
                Offset = new Point(0, 4),
                Radius = 14,
                Opacity = 0.3f
            };
        }
    }

    private void MiktarArtir_Clicked(object sender, EventArgs e)
    {
        if (((Button)sender).CommandParameter is Urun urun)
        {
            int index = SecilenMasa.Sepet.IndexOf(urun);
            SecilenMasa.Sepet.RemoveAt(index);
            urun.Miktar++;
            SecilenMasa.Sepet.Insert(index, urun);
            DurumuGuncelle();
        }
    }

    private void SilTiklandi(object sender, EventArgs e)
    {
        if (((Button)sender).CommandParameter is Urun urun)
        {
            if (urun.Miktar > 1)
            {
                int index = SecilenMasa.Sepet.IndexOf(urun);
                SecilenMasa.Sepet.RemoveAt(index);
                urun.Miktar--;
                SecilenMasa.Sepet.Insert(index, urun);
            }
            else
            {
                SecilenMasa.Sepet.Remove(urun);
            }
            DurumuGuncelle();
        }
    }

    private async void GeriDonTiklandi(object sender, EventArgs e) => await Navigation.PopAsync();
}
