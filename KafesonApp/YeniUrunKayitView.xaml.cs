#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using System.Linq;

namespace KafesonApp;

public partial class YeniUrunKayitView : ContentView
{
    private string seciliKategori = "";
    private readonly VeriServisi _servis = new VeriServisi();

    public YeniUrunKayitView()
    {
        InitializeComponent();
        KategorileriOlustur();
        ListeyiGuncelle();
    }

    private void KategorileriOlustur()
    {
        var sabitButon = KategoriStack.Children.OfType<Button>().FirstOrDefault(b => b.Text.Contains("+"));
        KategoriStack.Children.Clear();
        if (App.Urunler == null) return;

        var kategoriler = App.Urunler.Select(x => x.Kategori).Distinct().ToList();
        foreach (var kat in kategoriler)
        {
            var btn = new Button { Text = kat, Margin = new Thickness(0, 0, 5, 0), BackgroundColor = Color.FromArgb("#3498DB"), TextColor = Colors.White, CornerRadius = 20 };
            btn.Clicked += (s, e) => {
                seciliKategori = kat;
                foreach (var b in KategoriStack.Children.OfType<Button>().Where(x => !x.Text.Contains("+"))) b.BackgroundColor = Color.FromArgb("#3498DB");
                btn.BackgroundColor = Color.FromArgb("#27AE60");

                // 🚨 Kategori seçildiğinde SİL butonunu gösteriyoruz
                BtnKategoriSil.IsVisible = true;

                ListeyiGuncelle();
            };
            KategoriStack.Children.Add(btn);
        }
        if (sabitButon != null) KategoriStack.Children.Add(sabitButon);
    }

    private async void OnUrunEkleClicked(object sender, EventArgs e)
    {
        var ekran = Application.Current?.Windows[0].Page;
        if (ekran == null) return;

        if (string.IsNullOrEmpty(seciliKategori))
        {
            await ekran.DisplayAlert("Hata", "Lütfen önce bir kategori seçin!", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(UrunAdEntry.Text) || !double.TryParse(UrunFiyatEntry.Text, out double fiyat))
        {
            await ekran.DisplayAlert("Hata", "Geçerli isim ve fiyat girin!", "Tamam");
            return;
        }

        var urun = new Urun { Id = 0, Ad = UrunAdEntry.Text, Fiyat = fiyat, Kategori = seciliKategori, Miktar = 1 };

        try
        {
            bool basarili = await _servis.UrunEkle(urun);
            if (basarili)
            {
                App.Urunler.Add(urun);
                UrunAdEntry.Text = ""; UrunFiyatEntry.Text = "";
                ListeyiGuncelle();
                await ekran.DisplayAlert("Başarılı", "Ürün menüye eklendi.", "Tamam");
            }
            else
            {
                await ekran.DisplayAlert("Sunucu Hatası", "API veritabanına kaydedemedi.", "Tamam");
            }
        }
        catch
        {
            await ekran.DisplayAlert("Bağlantı Hatası", "API'ye ulaşılamıyor.", "Tamam");
        }
    }

    private async void OnUrunSilClicked(object sender, EventArgs e)
    {
        var ekran = Application.Current?.Windows[0].Page;

        if (sender is Button btn && btn.CommandParameter is Urun urun && ekran != null)
        {
            if (await ekran.DisplayAlert("Onay", $"{urun.Ad} silinsin mi?", "Evet", "Hayır"))
            {
                if (await _servis.UrunSil(urun.Id))
                {
                    App.Urunler.Remove(urun);

                    // Eğer silinen ürünle birlikte kategori boşaldıysa, listeyi yenile
                    if (!App.Urunler.Any(x => x.Kategori == seciliKategori))
                    {
                        seciliKategori = "";
                        BtnKategoriSil.IsVisible = false;
                        KategorileriOlustur();
                    }

                    ListeyiGuncelle();
                }
            }
        }
    }

    // 🚨 YENİ EKLENEN KATEGORİ SİLME METODU 🚨
    private async void OnKategoriSilClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(seciliKategori)) return;

        var ekran = Application.Current?.Windows[0].Page;
        if (ekran == null) return;

        bool onay = await ekran.DisplayAlert("DİKKAT", $"'{seciliKategori}' kategorisini ve içindeki TÜM ürünleri silmek istediğinize emin misiniz?", "Evet, Tamamen Sil", "İptal");

        if (onay)
        {
            // O kategoriye ait ürünleri (hayalet ürün dahil) bul
            var silinecekler = App.Urunler.Where(x => x.Kategori == seciliKategori).ToList();

            foreach (var urun in silinecekler)
            {
                bool silindi = await _servis.UrunSil(urun.Id);
                if (silindi)
                {
                    App.Urunler.Remove(urun);
                }
            }

            await ekran.DisplayAlert("Başarılı", "Kategori ve içindeki tüm ürünler silindi.", "Tamam");

            // Ekranı sıfırla
            seciliKategori = "";
            BtnKategoriSil.IsVisible = false;
            KategorileriOlustur();
            ListeyiGuncelle();
        }
    }

    private void ListeyiGuncelle()
    {
        // 🚨 HAYALET ÜRÜNÜ GİZLEME KISMI: Adı "Kategori Başlatıcı" olanları ekrana basmaz!
        FiltreliUrunList.ItemsSource = App.Urunler?
            .Where(x => (string.IsNullOrEmpty(seciliKategori) || x.Kategori == seciliKategori) && x.Ad != "Kategori Başlatıcı")
            .ToList();
    }

    private async void OnYeniKategoriClicked(object sender, EventArgs e)
    {
        var ekran = Application.Current?.Windows[0].Page;
        if (ekran == null) return;

        string result = await ekran.DisplayPromptAsync("Yeni Kategori", "Kategori Adı:");
        if (!string.IsNullOrWhiteSpace(result))
        {
            // Arka planda kategoriyi tutacak hayalet ürün
            var urun = new Urun { Ad = "Kategori Başlatıcı", Fiyat = 0, Kategori = result, Miktar = 0 };

            bool basarili = await _servis.UrunEkle(urun);
            if (basarili)
            {
                App.Urunler.Add(urun);
                KategorileriOlustur();
                await ekran.DisplayAlert("Başarılı", "Yeni kategori eklendi. Artık içine ürün ekleyebilirsiniz.", "Tamam");
            }
        }
    }
}