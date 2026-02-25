using KafesonApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace KafesonApp;

public class OdemeUrunu : INotifyPropertyChanged
{
    public Urun OrijinalUrun { get; set; }
    public string Ad => OrijinalUrun.Ad;
    public double Fiyat => OrijinalUrun.Fiyat;
    public int ToplamAdet => OrijinalUrun.Miktar;

    private int _secilenAdet;
    public int SecilenAdet
    {
        get => _secilenAdet;
        set { _secilenAdet = value; OnPropertyChanged(nameof(SecilenAdet)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class OdemePage : ContentPage
{
    private Masa _masa;
    public ObservableCollection<OdemeUrunu> OdemeListesi { get; set; }

    public OdemePage(Masa masa)
    {
        InitializeComponent();
        _masa = masa;

        KalanLabel.Text = $"{_masa.KalanTutar:N2} ₺";

        OdemeListesi = new ObservableCollection<OdemeUrunu>();
        ListeyiDoldur();
        UrunlerView.ItemsSource = OdemeListesi;
    }

    private void ListeyiDoldur()
    {
        OdemeListesi.Clear();
        foreach (var urun in _masa.Siparisler)
        {
            OdemeListesi.Add(new OdemeUrunu { OrijinalUrun = urun, SecilenAdet = 0 });
        }
    }

    private void Arti_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is OdemeUrunu u)
        {
            if (u.SecilenAdet < u.ToplamAdet)
            {
                u.SecilenAdet++;
                Hesapla();
            }
        }
    }

    private void Eksi_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is OdemeUrunu u)
        {
            if (u.SecilenAdet > 0)
            {
                u.SecilenAdet--;
                Hesapla();
            }
        }
    }

    private void Hesapla()
    {
        double toplam = OdemeListesi.Sum(x => x.SecilenAdet * x.Fiyat);
        SecilenTutarLabel.Text = $"{toplam:N2} ₺";

        if (toplam > 0) AlinanTutarEntry.Text = "";
    }

    private void Nakit_Clicked(object sender, EventArgs e) => OdemeIsleminiYap("Nakit");
    private void Kart_Clicked(object sender, EventArgs e) => OdemeIsleminiYap("Kart");

    private async void OdemeIsleminiYap(string tur)
    {
        double secilenUrunTutari = OdemeListesi.Sum(x => x.SecilenAdet * x.Fiyat);
        double islemTutari = 0;
        bool urunBazliOdeme = false;

        if (secilenUrunTutari > 0)
        {
            islemTutari = secilenUrunTutari;
            urunBazliOdeme = true;
        }
        else if (double.TryParse(AlinanTutarEntry.Text, out double manuelTutar))
        {
            islemTutari = manuelTutar;
        }
        else
        {
            await DisplayAlert("Uyarı", "Lütfen ödenecek ürünleri seçin veya bir tutar girin.", "Tamam");
            return;
        }

        if (islemTutari <= 0 || islemTutari > _masa.KalanTutar + 0.1)
        {
            await DisplayAlert("Hata", "Geçersiz veya kalan hesaptan fazla bir tutar girdiniz.", "Tamam");
            return;
        }

        if (tur == "Nakit") _masa.NakitBirikim += islemTutari;
        else _masa.KartBirikim += islemTutari;

        if (urunBazliOdeme)
        {
            foreach (var odemeUrunu in OdemeListesi.Where(x => x.SecilenAdet > 0))
            {
                var kapanisUrunu = _masa.KapanisUrunleri.FirstOrDefault(x => x.Ad == odemeUrunu.Ad);
                if (kapanisUrunu != null) kapanisUrunu.Miktar += odemeUrunu.SecilenAdet;
                else _masa.KapanisUrunleri.Add(new Urun { Ad = odemeUrunu.Ad, Fiyat = odemeUrunu.Fiyat, Miktar = odemeUrunu.SecilenAdet });

                var masaUrunu = _masa.Siparisler.FirstOrDefault(x => x.Ad == odemeUrunu.Ad);
                if (masaUrunu != null)
                {
                    masaUrunu.Miktar -= odemeUrunu.SecilenAdet;
                    if (masaUrunu.Miktar <= 0) _masa.Siparisler.Remove(masaUrunu);
                }
            }
        }
        else
        {
            _masa.OdenmisTutar += islemTutari;
        }

        _masa.YenidenHesapla();
        App.VerileriKaydet();

        // 🟢 SİSTEME LOG EKLİYORUZ 🟢
        App.LogEkle("Ödeme Alındı", $"Masa {_masa.No}'dan {islemTutari:N2} ₺ tutarında {tur} ödemesi alındı.");

        await DisplayAlert("Başarılı", $"{islemTutari:N2} ₺ {tur} ödemesi alındı.", "Tamam");

        KalanLabel.Text = $"{_masa.KalanTutar:N2} ₺";
        AlinanTutarEntry.Text = "";
        SecilenTutarLabel.Text = "0.00 ₺";
        ListeyiDoldur();
    }

    private async void Iptal_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();
}