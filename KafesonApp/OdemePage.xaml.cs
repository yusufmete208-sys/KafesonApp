#nullable disable
using Kafeson.Shared.Models;
using KafesonApp.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KafesonApp;

public partial class OdemePage : ContentPage
{
    Masa _masa;
    private readonly VeriServisi _servis = new VeriServisi();
    private Entry _tutarEntry;
    private bool _isUpdating = false;

    public OdemePage(Masa masa)
    {
        InitializeComponent();
        _tutarEntry = this.FindByName<Entry>("OdenecekTutarEntry");
        _masa = masa;
        BindingContext = _masa;

        if (_masa.Siparisler != null)
        {
            foreach (var urun in _masa.Siparisler)
            {
                urun.OdenecekAdet = 0;
                urun.IsSecili = false;
                urun.PropertyChanged += Urun_PropertyChanged;
            }
        }
        TutarGuncelle();
    }

    private void Urun_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_isUpdating) return;
        if (sender is Urun urun)
        {
            _isUpdating = true;
            if (e.PropertyName == nameof(Urun.IsSecili))
            {
                if (urun.IsSecili && urun.OdenecekAdet == 0) urun.OdenecekAdet = urun.Miktar;
                else if (!urun.IsSecili) urun.OdenecekAdet = 0;
                TutarGuncelle();
            }
            else if (e.PropertyName == nameof(Urun.OdenecekAdet)) TutarGuncelle();
            _isUpdating = false;
        }
    }

    private void TutarGuncelle()
    {
        try
        {
            if (_masa.Siparisler == null || _tutarEntry == null) return;
            double toplam = _masa.Siparisler.Where(x => x.OdenecekAdet > 0).Sum(x => x.OdenecekAdet * x.Fiyat);
            _tutarEntry.Text = toplam > 0 ? toplam.ToString("N2") : _masa.KalanTutar.ToString("N2");
        }
        catch { }
    }

    private void OdenenMiktarArtir_Clicked(object sender, EventArgs e) { if (sender is Button btn && btn.CommandParameter is Urun urun && urun.OdenecekAdet < urun.Miktar) { urun.OdenecekAdet++; urun.IsSecili = true; } }
    private void OdenenMiktarAzalt_Clicked(object sender, EventArgs e) { if (sender is Button btn && btn.CommandParameter is Urun urun && urun.OdenecekAdet > 0) { urun.OdenecekAdet--; if (urun.OdenecekAdet == 0) urun.IsSecili = false; } }

    private async void OdemeYap_Clicked(object sender, EventArgs e)
    {
        string girilen = _tutarEntry?.Text?.Replace(",", ".");
        if (!double.TryParse(girilen, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double tutar) || tutar <= 0) return;

        var seciliUrunler = _masa.Siparisler.Where(x => x.OdenecekAdet > 0).ToList();
        string tip = ((Button)sender).CommandParameter?.ToString() ?? "Nakit";

        if (seciliUrunler.Any())
        {
            foreach (var urun in seciliUrunler)
            {
                if (_masa.KapanisUrunleri == null) _masa.KapanisUrunleri = new List<Urun>();
                _masa.KapanisUrunleri.Add(new Urun { Ad = urun.Ad, Miktar = urun.OdenecekAdet, Fiyat = urun.Fiyat });
                urun.Miktar -= urun.OdenecekAdet;
                if (urun.Miktar <= 0) { urun.PropertyChanged -= Urun_PropertyChanged; _masa.Siparisler.Remove(urun); }
                else { _isUpdating = true; urun.OdenecekAdet = 0; urun.IsSecili = false; _isUpdating = false; }
            }
        }

        if (tip == "Nakit") _masa.NakitBirikim += tutar; else _masa.KartBirikim += tutar;
        await _servis.LogGonder(_masa.No, "Ödeme Tahsil", $"{tutar:N2} ₺ {tip} ödendi.");

        if ((_masa.KalanTutar - tutar) <= 0.01)
        {
            foreach (var kalanUrun in _masa.Siparisler.ToList()) { if (kalanUrun.Miktar > 0) _masa.KapanisUrunleri.Add(new Urun { Ad = kalanUrun.Ad, Miktar = kalanUrun.Miktar, Fiyat = kalanUrun.Fiyat }); }

            var rapor = new SatisRaporu
            {
                MasaNo = _masa.No,
                Tutar = _masa.NakitBirikim + _masa.KartBirikim,
                OdemeTuru = $"Nakit: {_masa.NakitBirikim:N2} / Kart: {_masa.KartBirikim:N2}",
                Tarih = DateTime.Now,
                UrunDetaylari = string.Join(", ", _masa.KapanisUrunleri.Where(x => x.Miktar > 0).GroupBy(x => x.Ad).Select(g => $"{g.Sum(x => x.Miktar)}x {g.Key}")),
                PersonelAdi = App.AktifKullanici != null ? App.AktifKullanici.KullaniciAdi : "Admin"
            };

            await _servis.RaporGonder(rapor);
            _masa.IsDolu = false; _masa.Siparisler.Clear();
            await _servis.MasaGuncelle(_masa.No, _masa);
            await Navigation.PopAsync();
        }
        else { await _servis.MasaGuncelle(_masa.No, _masa); TutarGuncelle(); }
    }
}