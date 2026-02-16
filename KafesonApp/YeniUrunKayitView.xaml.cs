using KafesonApp.Models;

namespace KafesonApp;

public partial class YeniUrunKayitView : ContentView
{
    private string seciliKategori = "";

    public YeniUrunKayitView()
    {
        InitializeComponent();
        KategorileriOlustur();
    }

    private void KategorileriOlustur()
    {
        if (KategoriStack == null || KategoriStack.Children.Count == 0) return;

        var ilkButon = KategoriStack.Children[0];
        KategoriStack.Children.Clear();
        KategoriStack.Children.Add(ilkButon);

        var kategoriler = App.Urunler.Select(u => u.Kategori).Distinct().ToList();
        foreach (var kat in kategoriler)
        {
            var btn = new Button
            {
                Text = kat,
                CornerRadius = 20,
                Margin = new Thickness(5, 0),
                BackgroundColor = Color.FromArgb("#F1F2F6"),
                TextColor = Color.FromArgb("#2F3640")
            };
            btn.Clicked += (s, e) => KategoriSec(btn);
            KategoriStack.Children.Add(btn);
        }
    }

    private void KategoriSec(Button secilenBtn)
    {
        foreach (var b in KategoriStack.Children.OfType<Button>().Where(x => x.Text != "+ Yeni Kategori Ekle"))
        {
            b.BackgroundColor = Color.FromArgb("#F1F2F6");
            b.TextColor = Color.FromArgb("#2F3640");
        }

        secilenBtn.BackgroundColor = Color.FromArgb("#FFC300");
        seciliKategori = secilenBtn.Text;
        ListeBaslikLabel.Text = $"{seciliKategori} Ürünleri";
        ListeyiFiltrele();
    }

    private void ListeyiFiltrele()
    {
        FiltreliUrunList.ItemsSource = null;
        FiltreliUrunList.ItemsSource = App.Urunler.Where(u => u.Kategori == seciliKategori).ToList();
    }

    private async void OnUrunEkleClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(seciliKategori))
        {
            await Application.Current.MainPage.DisplayAlert("Uyarý", "Kategori seçin!", "Tamam"); return;
        }

        if (double.TryParse(UrunFiyatEntry.Text, out double fiyat))
        {
            App.Urunler.Add(new Urun { Ad = UrunAdEntry.Text, Fiyat = fiyat, Kategori = seciliKategori, Miktar = 1 });
            App.VerileriKaydet();
            ListeyiFiltrele();
            UrunAdEntry.Text = ""; UrunFiyatEntry.Text = "";
        }
    }

    private async void OnUrunSilClicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (await Application.Current.MainPage.DisplayAlert("Sil", $"{urun.Ad} silinsin mi?", "Evet", "Hayýr"))
        {
            App.Urunler.Remove(urun);
            App.VerileriKaydet();
            ListeyiFiltrele();
        }
    }

    private async void OnYeniKategoriClicked(object sender, EventArgs e)
    {
        string result = await Application.Current.MainPage.DisplayPromptAsync("Yeni Kategori", "Kategori Adý Giriniz:");

        if (!string.IsNullOrWhiteSpace(result))
        {
            // Mevcut listede bu isimde bir kategori olup olmadýðýný kontrol et
            bool varMi = App.Urunler.Any(u => u.Kategori.Equals(result, StringComparison.OrdinalIgnoreCase));

            if (!varMi)
            {
                // Yeni kategorinin butonlarda gözükmesi için bu kategoride 'geçici' bir ürün eklemeliyiz 
                // Çünkü 'KategorileriOlustur' metodu App.Urunler listesindeki 'Distinct' kategorileri çeker.
                App.Urunler.Add(new Urun
                {
                    Ad = "Yeni Kategori Örneði",
                    Kategori = result,
                    Fiyat = 0,
                    Miktar = 0
                });

                App.VerileriKaydet(); // Veriyi kalýcý hale getir
                KategorileriOlustur(); // Buton listesini yenile

                await Application.Current.MainPage.DisplayAlert("Baþarýlý", $"{result} kategorisi eklendi.", "Tamam");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Uyarý", "Bu kategori zaten mevcut!", "Tamam");
            }
        }
    }
}