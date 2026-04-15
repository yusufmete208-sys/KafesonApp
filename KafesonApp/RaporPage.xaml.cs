#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;

#if WINDOWS
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
#endif

namespace KafesonApp;

public partial class RaporPage : ContentPage
{
    string seciliMod = "Gun";
    private readonly VeriServisi _servis = new VeriServisi();
    private List<SatisRaporu> _tumRaporlar = new List<SatisRaporu>();
    private List<SatisRaporu> _filtreliRaporlar = new List<SatisRaporu>();

    public RaporPage()
    {
        InitializeComponent();
        RaporTarihSecici.Date = DateTime.Now;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            _tumRaporlar = await _servis.RaporlariGetir();
            RaporuHesapla();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bağlantı Hatası", "Rapor verileri çekilemedi: " + ex.Message, "Tamam");
        }
    }

    private void ModDegistir(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        seciliMod = btn.CommandParameter.ToString();

        BtnGun.BackgroundColor = seciliMod == "Gun" ? Microsoft.Maui.Graphics.Color.FromArgb("#34495E") : Microsoft.Maui.Graphics.Colors.Transparent;
        BtnGun.TextColor = seciliMod == "Gun" ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#34495E");

        BtnAy.BackgroundColor = seciliMod == "Ay" ? Microsoft.Maui.Graphics.Color.FromArgb("#34495E") : Microsoft.Maui.Graphics.Colors.Transparent;
        BtnAy.TextColor = seciliMod == "Ay" ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#34495E");

        BtnYil.BackgroundColor = seciliMod == "Yil" ? Microsoft.Maui.Graphics.Color.FromArgb("#34495E") : Microsoft.Maui.Graphics.Colors.Transparent;
        BtnYil.TextColor = seciliMod == "Yil" ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#34495E");

        RaporuHesapla();
    }

    private void RaporuHesapla()
    {
        if (_tumRaporlar == null) return;

        DateTime tarih = RaporTarihSecici.Date;

        if (seciliMod == "Gun")
            _filtreliRaporlar = _tumRaporlar.Where(x => x.Tarih.Date == tarih.Date).ToList();
        else if (seciliMod == "Ay")
            _filtreliRaporlar = _tumRaporlar.Where(x => x.Tarih.Month == tarih.Month && x.Tarih.Year == tarih.Year).ToList();
        else
            _filtreliRaporlar = _tumRaporlar.Where(x => x.Tarih.Year == tarih.Year).ToList();

        CiroLabel.Text = $"{_filtreliRaporlar.Sum(x => x.Tutar):N2} ₺";
        AdetLabel.Text = $"{_filtreliRaporlar.Count} İşlem Gerçekleşti";

        double ortalama = _filtreliRaporlar.Count > 0 ? _filtreliRaporlar.Average(x => x.Tutar) : 0;
        OrtalamaLabel.Text = $"{ortalama:N2} ₺";

        UrunStatContainer.Children.Clear();
        SaatlikStatContainer.Children.Clear();

        if (_filtreliRaporlar.Count == 0)
        {
            UrunStatContainer.Children.Add(new Label { Text = "Bu tarihte veri yok.", TextColor = Microsoft.Maui.Graphics.Colors.Gray, HorizontalOptions = LayoutOptions.Center });
            SaatlikStatContainer.Children.Add(new Label { Text = "Bu tarihte veri yok.", TextColor = Microsoft.Maui.Graphics.Colors.Gray, HorizontalOptions = LayoutOptions.Center });
            return;
        }

        var urunSatislar = UrunSatislariniHesapla();
        var topUrunler = urunSatislar.OrderByDescending(x => x.Value).Take(3).ToList();

        int sira = 1;
        foreach (var urun in topUrunler)
        {
            UrunStatContainer.Children.Add(new Label { Text = $"{sira}. {urun.Key} ({urun.Value} adet)", TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2C3E50"), FontAttributes = FontAttributes.Bold });
            sira++;
        }

        var yogunSaatler = YogunSaatleriHesapla().Take(3).ToList();
        sira = 1;
        foreach (var saat in yogunSaatler)
        {
            string saatAraligi = $"{saat.Saat:D2}:00 - {saat.Saat + 1:D2}:00";
            SaatlikStatContainer.Children.Add(new Label { Text = $"{sira}. {saatAraligi} ({saat.IslemSayisi} sipariş)", TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2C3E50"), FontAttributes = FontAttributes.Bold });
            sira++;
        }
    }

    private Dictionary<string, int> UrunSatislariniHesapla()
    {
        var urunSatislar = new Dictionary<string, int>();
        foreach (var rapor in _filtreliRaporlar)
        {
            if (string.IsNullOrEmpty(rapor.UrunDetaylari)) continue;
            var kalemler = rapor.UrunDetaylari.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kalem in kalemler)
            {
                var parcalar = kalem.Split(new[] { "x " }, StringSplitOptions.None);
                if (parcalar.Length == 2 && int.TryParse(parcalar[0], out int miktar))
                {
                    string urunAdi = parcalar[1];
                    if (!urunSatislar.ContainsKey(urunAdi)) urunSatislar[urunAdi] = 0;
                    urunSatislar[urunAdi] += miktar;
                }
            }
        }
        return urunSatislar;
    }

    private List<(int Saat, int IslemSayisi)> YogunSaatleriHesapla()
    {
        return _filtreliRaporlar.GroupBy(x => x.Tarih.Hour)
                                .Select(g => (Saat: g.Key, IslemSayisi: g.Count()))
                                .OrderByDescending(x => x.IslemSayisi)
                                .ToList();
    }

    private void TarihDegisti(object sender, DateChangedEventArgs e) => RaporuHesapla();
    private async void Geri_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();

    // --- PAYLAŞMA SİSTEMİ (PC'DE PDF MASAÜSTÜNE, TELEFONDA WHATSAPP METNİ) ---
    private async void RaporPaylas_Clicked(object sender, EventArgs e)
    {
        if (_filtreliRaporlar == null || _filtreliRaporlar.Count == 0)
        {
            await DisplayAlert("Hata", "İndirilecek veri bulunamadı.", "Tamam");
            return;
        }

        string donemBaslik = seciliMod == "Gun" ? $"{RaporTarihSecici.Date:dd MMMM yyyy} Günlük" :
                             seciliMod == "Ay" ? $"{RaporTarihSecici.Date:MMMM yyyy} Aylık" :
                             $"{RaporTarihSecici.Date:yyyy} Yıllık";

        double toplamNakit = 0, toplamKart = 0;
        foreach (var r in _filtreliRaporlar)
        {
            if (!string.IsNullOrEmpty(r.OdemeTuru))
            {
                var parcalar = r.OdemeTuru.Split('/');
                foreach (var p in parcalar)
                {
                    if (p.Contains("Nakit:")) { if (double.TryParse(p.Replace("Nakit:", "").Trim(), out double n)) toplamNakit += n; }
                    if (p.Contains("Kart:")) { if (double.TryParse(p.Replace("Kart:", "").Trim(), out double k)) toplamKart += k; }
                }
            }
        }

        try
        {
#if WINDOWS
            // 💻 BİLGİSAYAR: QuestPDF KULLANARAK MASAÜSTÜNE A4 PDF KAYDEDER
            QuestPDF.Settings.License = LicenseType.Community;
            var belge = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(QuestPDF.Helpers.Fonts.Arial));

                    page.Header().PaddingBottom(10).Column(col =>
                    {
                        col.Item().Text("KAFESON").FontSize(26).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                        col.Item().Text($"{donemBaslik} İşletme Raporu").FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                        col.Item().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(5).Text("1. FİNANSAL ÖZET").FontSize(12).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                        col.Item().PaddingBottom(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); });
                            table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(4).Text("Toplam Ciro:");
                            table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{_filtreliRaporlar.Sum(x => x.Tutar):N2} TL").SemiBold();
                            table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(4).Text("Tahsilat Dağılımı:");
                            table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"Nakit: {toplamNakit:N2} TL  |  Kart: {toplamKart:N2} TL");
                            table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(4).Text("Kapatılan Masa Sayısı:");
                            table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{_filtreliRaporlar.Count} Adet");
                        });

                        col.Item().PaddingBottom(5).Text("2. ÜRÜN SATIŞ ANALİZİ (TOP 10)").FontSize(12).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                        col.Item().PaddingBottom(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns => { columns.ConstantColumn(30); columns.RelativeColumn(); columns.ConstantColumn(80); });
                            table.Header(header =>
                            {
                                header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(4).Text("#").SemiBold();
                                header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(4).Text("Ürün Adı").SemiBold();
                                header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(4).AlignRight().Text("Satış Adedi").SemiBold();
                            });
                            var urunler = UrunSatislariniHesapla().OrderByDescending(x => x.Value).Take(10).ToList();
                            int i = 1;
                            foreach (var u in urunler)
                            {
                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(4).Text(i.ToString());
                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(4).Text(u.Key);
                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(4).AlignRight().Text(u.Value.ToString());
                                i++;
                            }
                        });
                    });
                });
            });

            string dosyaAdi = $"Kafeson_Rapor_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            string masaustuYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string dosyaYolu = Path.Combine(masaustuYolu, dosyaAdi);
            belge.GeneratePdf(dosyaYolu);
            await DisplayAlert("Başarılı", $"Rapor Masaüstüne kaydedildi.\n\nDosya: {dosyaAdi}", "Tamam");

#else
            // 📱 TELEFON: ÇÖKMEYİ %100 ÖNLER! WhatsApp İçin Düzenli Metin Oluşturup Paylaşır.
            string ozetMetin = $"KAFESON {donemBaslik.ToUpper()} RAPORU\n" +
                               $"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                               $"-----------------------------------\n" +
                               $"💰 TOPLAM CİRO: {_filtreliRaporlar.Sum(x => x.Tutar):N2} TL\n" +
                               $"💵 Nakit: {toplamNakit:N2} TL\n" +
                               $"💳 Kart: {toplamKart:N2} TL\n" +
                               $"🍽️ Kapatılan Masa: {_filtreliRaporlar.Count} Adet\n" +
                               $"-----------------------------------\n" +
                               $"🏆 LİDER ÜRÜNLER (TOP 3)\n";

            var topUrunler = UrunSatislariniHesapla().OrderByDescending(x => x.Value).Take(3).ToList();
            int sira = 1;
            foreach (var u in topUrunler)
            {
                ozetMetin += $"{sira}. {u.Key}: {u.Value} Adet\n";
                sira++;
            }

            // Telefonun paylaşım menüsünü (WhatsApp, Mail vb.) açar
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Kafeson Raporunu Paylaş",
                Text = ozetMetin,
                Subject = "Kafeson İşletme Özeti"
            });
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"İşlem sırasında bir sorun oluştu:\n{ex.Message}", "Tamam");
        }
    }
}