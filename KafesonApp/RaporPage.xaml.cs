using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

public partial class RaporPage : ContentPage
{
    string seciliMod = "Gun";

    public RaporPage()
    {
        InitializeComponent();
        RaporTarihSecici.Date = DateTime.Now;
        RaporuHesapla();
    }

    private void ModDegistir(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        seciliMod = btn.CommandParameter.ToString();

        // Buton renklerini güncelle
        BtnGun.BackgroundColor = seciliMod == "Gun" ? Color.FromArgb("#34495E") : Colors.Transparent;
        BtnGun.TextColor = seciliMod == "Gun" ? Colors.White : Color.FromArgb("#34495E");

        BtnAy.BackgroundColor = seciliMod == "Ay" ? Color.FromArgb("#34495E") : Colors.Transparent;
        BtnAy.TextColor = seciliMod == "Ay" ? Colors.White : Color.FromArgb("#34495E");

        BtnYil.BackgroundColor = seciliMod == "Yil" ? Color.FromArgb("#34495E") : Colors.Transparent;
        BtnYil.TextColor = seciliMod == "Yil" ? Colors.White : Color.FromArgb("#34495E");

        RaporuHesapla();
    }

    private void RaporuHesapla()
    {
        DateTime tarih = RaporTarihSecici.Date;
        List<SatisRaporu> filtreliRaporlar;

        // 1. ZAMANA GÖRE FİLTRELE
        if (seciliMod == "Gun")
        {
            filtreliRaporlar = App.SatisRaporlari.Where(x => x.Tarih.Date == tarih.Date).ToList();
            RaporBaslikLabel.Text = "BUGÜNKÜ TOPLAM KAZANÇ";
        }
        else if (seciliMod == "Ay")
        {
            filtreliRaporlar = App.SatisRaporlari.Where(x => x.Tarih.Month == tarih.Month && x.Tarih.Year == tarih.Year).ToList();
            RaporBaslikLabel.Text = $"{tarih:MMMM yyyy} TOPLAM KAZANÇ";
        }
        else
        {
            filtreliRaporlar = App.SatisRaporlari.Where(x => x.Tarih.Year == tarih.Year).ToList();
            RaporBaslikLabel.Text = $"{tarih.Year} YILI TOPLAM KAZANÇ";
        }

        // 2. HESAPLAMALAR
        double toplam = filtreliRaporlar.Sum(x => x.Fiyat);
        GunlukCiroLabel.Text = $"{toplam:N2} TL";
        GunlukAdetLabel.Text = $"{filtreliRaporlar.Count} İşlem Kaydedildi";

        NakitToplamLabel.Text = filtreliRaporlar.Where(x => x.OdemeTuru == "Nakit").Sum(x => x.Fiyat).ToString("N0");
        KartToplamLabel.Text = filtreliRaporlar.Where(x => x.OdemeTuru == "Kart").Sum(x => x.Fiyat).ToString("N0");
        OrtalamaAdisyonLabel.Text = filtreliRaporlar.Count > 0 ? (toplam / filtreliRaporlar.Count).ToString("N0") : "0";

        // 3. ÜRÜN VE SAAT ANALİZİ
        AnalizleriDoldur(filtreliRaporlar);
    }

    private void AnalizleriDoldur(List<SatisRaporu> raporlar)
    {
        UrunStatContainer.Children.Clear();
        var urunler = raporlar.GroupBy(x => x.UrunAd).Select(g => new { Ad = g.Key, Tutar = g.Sum(s => s.Fiyat) }).OrderByDescending(o => o.Tutar).Take(3);
        foreach (var u in urunler) UrunStatContainer.Children.Add(new Label { Text = $"• {u.Ad}", FontSize = 13, FontAttributes = FontAttributes.Bold });

        SaatlikStatContainer.Children.Clear();
        var saatler = raporlar.GroupBy(x => x.Tarih.Hour).Select(g => new { Saat = g.Key, Tutar = g.Sum(s => s.Fiyat) }).OrderByDescending(o => o.Tutar).Take(2);
        foreach (var s in saatler) SaatlikStatContainer.Children.Add(new Label { Text = $"• {s.Saat:00}:00 - {s.Tutar:N0} TL", FontSize = 13 });
    }

    private void TarihDegisti(object sender, DateChangedEventArgs e) => RaporuHesapla();
    private async void RaporPaylas_Clicked(object sender, EventArgs e) => await Share.RequestAsync(new ShareTextRequest { Text = $"{RaporBaslikLabel.Text}: {GunlukCiroLabel.Text}", Title = "Kafeson Rapor" });


    // RaporPage.xaml.cs dosyanızın içine bu metodu ekleyin
    private async void Geri_Clicked(object sender, EventArgs e)
    {
        // Sayfayı kapatıp bir önceki ekrana (Ana Menü) döner
        await Navigation.PopAsync();
    }
}