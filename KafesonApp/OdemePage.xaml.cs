using KafesonApp.Models;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls; // CheckBox ve Event'ler için þart
using System.Linq; // Where ve Sum (LINQ) iþlemleri için þart

namespace KafesonApp;

public partial class OdemePage : ContentPage
{
    Masa _masa;

    public OdemePage(Masa masa)
    {
        InitializeComponent();
        _masa = masa;
        BindingContext = _masa;

        // Baþlangýçta seçimleri temizle
        foreach (var urun in _masa.Siparisler)
        {
            urun.OdenecekAdet = 0;
            urun.IsSecili = false;
        }

        if (OdenecekTutarLabel != null)
            OdenecekTutarLabel.Text = _masa.KalanTutar.ToString("N2");
    }

    // HATA DÜZELTÝLDÝ: Eriþim belirleyicisi public yapýldý ve tip kontrolü eklendi
    public void SecimDegisti(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox cb || cb.BindingContext is not Urun urun) return;

        if (urun.IsSecili && urun.OdenecekAdet == 0)
            urun.OdenecekAdet = urun.Miktar;
        else if (!urun.IsSecili)
            urun.OdenecekAdet = 0;

        TutarGuncelle();
    }

    private void TutarGuncelle()
    {
        // System.Linq sayesinde Where ve Sum artýk hata vermez
        double toplam = _masa.Siparisler.Where(x => x.IsSecili).Sum(x => x.OdenecekAdet * x.Fiyat);
        OdenecekTutarLabel.Text = toplam > 0 ? toplam.ToString("N2") : _masa.KalanTutar.ToString("N2");
    }

    public async void HizliMiktar_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null && urun.Miktar > 0)
        {
            urun.OdenecekAdet = 1;
            urun.IsSecili = true;
            TutarGuncelle();
        }
    }

    public void OdenenMiktarArtir_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null && urun.OdenecekAdet < urun.Miktar)
        {
            urun.OdenecekAdet++;
            urun.IsSecili = true;
            TutarGuncelle();
        }
    }

    public void OdenenMiktarAzalt_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null && urun.OdenecekAdet > 0)
        {
            urun.OdenecekAdet--;
            if (urun.OdenecekAdet == 0) urun.IsSecili = false;
            TutarGuncelle();
        }
    }

    private async void OdemeYap_Clicked(object sender, EventArgs e)
    {
        if (!double.TryParse(OdenecekTutarLabel.Text, out double tutar) || tutar <= 0) return;

        string tip = ((Button)sender).CommandParameter.ToString();

        // Önce Log kaydýný düþüyoruz
        App.LogEkle($"Masa {_masa.No}: {tip} ile {tutar:N2} TL ödeme alýndý.", "Ödeme");

        if (tip == "Nakit")
            _masa.NakitBirikim += tutar;
        else
            _masa.KartBirikim += tutar;

        var seciliUrunler = _masa.Siparisler.Where(x => x.IsSecili).ToList();
        foreach (var urun in seciliUrunler)
        {
            _masa.KapanisUrunleri.Add(new Urun { Ad = urun.Ad, Fiyat = urun.Fiyat, Miktar = urun.OdenecekAdet });

            urun.Miktar -= urun.OdenecekAdet;
            if (urun.Miktar <= 0) _masa.Siparisler.Remove(urun);

            urun.OdenecekAdet = 0;
            urun.IsSecili = false;
        }

        App.SatisRaporlari.Add(new SatisRaporu { MasaNo = _masa.No, Fiyat = tutar, OdemeTuru = tip, Tarih = DateTime.Now });

        App.VerileriKaydet();
        await Navigation.PopModalAsync();
    }

    private async void Kapat_Clicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
}