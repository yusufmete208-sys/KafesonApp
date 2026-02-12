using KafesonApp.Models;
using System.Collections.ObjectModel;

namespace KafesonApp;

public partial class OdemePage : ContentPage
{
    Masa _masa;

    public OdemePage(Masa masa)
    {
        InitializeComponent();
        _masa = masa;
        BindingContext = _masa;

        // Baþlangýçta tüm ürünlerin ödenecek adedini 0 yap ki seçim temiz baþlasýn
        foreach (var urun in _masa.Siparisler)
        {
            urun.OdenecekAdet = 0;
            urun.IsSecili = false;
        }

        // Ýlk açýlýþta toplam borcu göster
        if (OdenecekTutarLabel != null)
            OdenecekTutarLabel.Text = _masa.KalanTutar.ToString("N2");
    }

    // --- SARI "1" BUTONU: HIZLI ÞEKÝLDE 1 ADET ÖDEME SEÇER ---
    private void HizliMiktar_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null && urun.Miktar > 0)
        {
            urun.OdenecekAdet = 1; // Sadece 1 adet öde
            urun.IsSecili = true;
            TutarGuncelle();
        }
    }

    // --- MÝKTAR ARTIR (+) ---
    private void OdenenMiktarArtir_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null && urun.OdenecekAdet < urun.Miktar)
        {
            urun.OdenecekAdet++; // Mevcut miktardan fazlasýný ödeyemezsin
            urun.IsSecili = true;
            TutarGuncelle();
        }
    }

    // --- MÝKTAR AZALT (-) ---
    private void OdenenMiktarAzalt_Clicked(object sender, EventArgs e)
    {
        var urun = (Urun)((Button)sender).CommandParameter;
        if (urun != null && urun.OdenecekAdet > 0)
        {
            urun.OdenecekAdet--;
            if (urun.OdenecekAdet == 0) urun.IsSecili = false;
            TutarGuncelle();
        }
    }

    // --- CHECKBOX DEÐÝÞÝMÝ ---
    private void SecimDegisti(object sender, CheckedChangedEventArgs e)
    {
        var checkbox = (CheckBox)sender;
        var urun = (Urun)checkbox.BindingContext;

        if (urun != null)
        {
            // Eðer kutucuk iþaretlendiyse ve miktar 0 ise, otomatik olarak tümünü ödet
            if (urun.IsSecili && urun.OdenecekAdet == 0)
                urun.OdenecekAdet = urun.Miktar;
            else if (!urun.IsSecili)
                urun.OdenecekAdet = 0;

            TutarGuncelle();
        }
    }
    private void TutarGuncelle()
    {
        // Hesaplama: Seçilen ürünlerin (ÖdenecekAdet * BirimFiyat) toplamý
        double toplam = _masa.Siparisler.Where(x => x.IsSecili).Sum(x => x.OdenecekAdet * x.Fiyat);
        OdenecekTutarLabel.Text = toplam > 0 ? toplam.ToString("N2") : _masa.KalanTutar.ToString("N2");
    }
    private async void OdemeYap_Clicked(object sender, EventArgs e)
    {
        if (!double.TryParse(OdenecekTutarLabel.Text, out double tutar) || tutar <= 0) return;

        string tip = ((Button)sender).CommandParameter.ToString();

        // 1. ADÝSYONDAN DÜÞME: 4 kahveden 2'sini düþ
        var seciliUrunler = _masa.Siparisler.Where(x => x.IsSecili).ToList();
        foreach (var urun in seciliUrunler)
        {
            urun.Miktar -= urun.OdenecekAdet; // 4 - 2 = 2 kalýr

            if (urun.Miktar <= 0) _masa.Siparisler.Remove(urun); // Hepsi bittiyse sil

            urun.OdenecekAdet = 0; // Sýfýrla
            urun.IsSecili = false;
        }

        // 2. Raporu Kaydet (Masa.KalanTutar otomatik olarak listeden düþen miktar kadar azalacaktýr)
        App.SatisRaporlari.Add(new SatisRaporu { MasaNo = _masa.No, Fiyat = tutar, OdemeTuru = tip, Tarih = DateTime.Now });

        App.VerileriKaydet();
        await Navigation.PopModalAsync(); // Geri dön
    }

    private async void Kapat_Clicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
}