using KafesonApp.Models;
using System.Linq;
using System.IO;
using QuestPDF.Fluent;
using Microsoft.Maui.ApplicationModel;

namespace KafesonApp;

public partial class RaporPage : ContentPage
{
    string seciliMod = "Gun";

    public RaporPage()
    {
        InitializeComponent();
        RaporTarihSecici.Date = DateTime.Now;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RaporuHesapla();
    }

    private void ModDegistir(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        seciliMod = btn.CommandParameter.ToString();

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
        if (App.SatisRaporlari == null) return;

        DateTime tarih = RaporTarihSecici.Date;
        List<SatisRaporu> filtreliRaporlar;

        if (seciliMod == "Gun")
        {
            filtreliRaporlar = App.SatisRaporlari.Where(x => x.Tarih.Date == tarih.Date).ToList();
            RaporBaslikLabel.Text = $"{tarih:dd MMMM yyyy} TOPLAM KAZANÇ";
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

        double toplamCiro = filtreliRaporlar.Sum(x => x.Tutar);
        int islemSayisi = filtreliRaporlar.Count;

        CiroLabel.Text = $"{toplamCiro:N2} ₺";
        AdetLabel.Text = $"{islemSayisi} Masa Hesabı Kapatıldı";

        OrtalamaLabel.Text = islemSayisi > 0 ? $"{(toplamCiro / islemSayisi):N2} ₺" : "0.00 ₺";

        AnalizleriDoldur(filtreliRaporlar);
    }

    private void AnalizleriDoldur(List<SatisRaporu> raporlar)
    {
        UrunStatContainer.Children.Clear();
        SaatlikStatContainer.Children.Clear();

        if (raporlar.Count == 0)
        {
            UrunStatContainer.Children.Add(new Label { Text = "Veri yok", TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center });
            SaatlikStatContainer.Children.Add(new Label { Text = "Veri yok", TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center });
            return;
        }

        var urunSayilari = new Dictionary<string, int>();

        foreach (var rapor in raporlar)
        {
            if (!string.IsNullOrEmpty(rapor.UrunDetaylari))
            {
                var parcalar = rapor.UrunDetaylari.Split(',');
                foreach (var parca in parcalar)
                {
                    var temizMetin = parca.Trim();
                    int xIndex = temizMetin.IndexOf('x');

                    if (xIndex > 0 && int.TryParse(temizMetin.Substring(0, xIndex), out int adet))
                    {
                        string urunAdi = temizMetin.Substring(xIndex + 1).Trim();
                        if (!urunSayilari.ContainsKey(urunAdi)) urunSayilari[urunAdi] = 0;
                        urunSayilari[urunAdi] += adet;
                    }
                }
            }
        }

        var enCokSatanlar = urunSayilari.OrderByDescending(x => x.Value).Take(3).ToList();
        foreach (var u in enCokSatanlar)
        {
            UrunStatContainer.Children.Add(new Label { Text = $"• {u.Key} ({u.Value} adet)", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2C3E50") });
        }

        var saatler = raporlar.GroupBy(x => x.Tarih.Hour)
                              .Select(g => new { Saat = g.Key, Tutar = g.Sum(s => s.Tutar) })
                              .OrderByDescending(o => o.Tutar)
                              .Take(3).ToList();

        foreach (var s in saatler)
        {
            SaatlikStatContainer.Children.Add(new Label { Text = $"• {s.Saat:00}:00 - {s.Tutar:N2} ₺", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2C3E50") });
        }
    }

    private void TarihDegisti(object sender, DateChangedEventArgs e) => RaporuHesapla();

    private async void Geri_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();

    // ==============================================================
    // PDF OLUŞTURMA VE CİHAZDA AÇMA BÖLÜMÜ
    // ==============================================================
    private async void RaporPaylas_Clicked(object sender, EventArgs e)
    {
        try
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            string klasor = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dosyaAdi = $"Kafeson_Rapor_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            string tamYol = Path.Combine(klasor, dosyaAdi);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("KAFESON İŞLETME RAPORU")
                        .SemiBold().FontSize(24).FontColor("#2C3E50");

                    page.Content().PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text($"Rapor Tarihi: {RaporBaslikLabel.Text}").FontSize(14).Bold();
                        col.Item().LineHorizontal(1).LineColor("#BDC3C7");

                        col.Item().Text($"Toplam Ciro: {CiroLabel.Text}").Bold().FontSize(20).FontColor("#27AE60");
                        col.Item().Text($"İşlem Sayısı: {AdetLabel.Text}");
                        col.Item().Text($"Masa Başı Ortalama Harcama: {OrtalamaLabel.Text}");

                        col.Item().LineHorizontal(1).LineColor("#BDC3C7");

                        col.Item().Text("LİDER ÜRÜNLER").Bold().FontSize(14).FontColor("#E67E22");
                        foreach (var child in UrunStatContainer.Children)
                        {
                            if (child is Label lbl && lbl.Text != "Veri yok")
                                col.Item().Text(lbl.Text);
                        }

                        col.Item().PaddingTop(10).Text("ZİRVE SAATLER").Bold().FontSize(14).FontColor("#8E44AD");
                        foreach (var child in SaatlikStatContainer.Children)
                        {
                            if (child is Label lbl && lbl.Text != "Veri yok")
                                col.Item().Text(lbl.Text);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Sayfa ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(tamYol);

            await Launcher.OpenAsync(new OpenFileRequest
            {
                Title = "PDF Raporunu Görüntüle",
                File = new ReadOnlyFile(tamYol)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"PDF oluşturulurken bir sorun oluştu:\n{ex.Message}", "Tamam");
        }
    }
}