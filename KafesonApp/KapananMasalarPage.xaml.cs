using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

public partial class KapananMasalar1View : ContentPage
{
    public KapananMasalar1View()
    {
        InitializeComponent();

        // Ödeme Yöntemlerini Doldur
        OdemeFiltre.ItemsSource = new List<string> { "Tümü", "Sadece Nakit", "Sadece Kart", "Parçalı (Nakit+Kart)" };
        OdemeFiltre.SelectedIndex = 0;

        // Personelleri Otomatik Çekip Doldur (Yeni Rapor Sistemimize Uygun)
        if (App.SatisRaporlari != null)
        {
            var personeller = App.SatisRaporlari
                     .Where(x => !string.IsNullOrEmpty(x.PersonelAdi) && x.PersonelAdi != "Bilinmiyor") // Bilinmiyor olanları gizle
                     .Select(x => x.PersonelAdi)
                     .Distinct()
                     .ToList();

            personeller.Insert(0, "Tüm Personeller");
            PersonelFiltre.ItemsSource = personeller;
            PersonelFiltre.SelectedIndex = 0;
        }

        TarihFiltre.Date = DateTime.Now;
        ListeyiGuncelle();
    }

    private void ListeyiGuncelle()
    {
        if (App.SatisRaporlari == null) return;

        var sonuc = App.SatisRaporlari.AsEnumerable();

        // 1. TARİH FİLTRESİ (Tarih verisi üzerinden)
        sonuc = sonuc.Where(x => x.Tarih.Date == TarihFiltre.Date);

        // 2. MASA NO FİLTRESİ
        if (!string.IsNullOrWhiteSpace(MasaFiltre.Text) && int.TryParse(MasaFiltre.Text, out int mNo))
        {
            sonuc = sonuc.Where(x => x.MasaNo == mNo);
        }

        // 3. ÖDEME TÜRÜ FİLTRESİ (OdemeTuru metninin içindeki değere bakarak anlıyoruz)
        if (OdemeFiltre.SelectedIndex > 0 && OdemeFiltre.SelectedItem != null)
        {
            string odeme = OdemeFiltre.SelectedItem.ToString();
            if (odeme == "Sadece Nakit")
                sonuc = sonuc.Where(x => x.OdemeTuru.Contains("Kart: 0")); // Kart 0 ise sadece nakittir
            else if (odeme == "Sadece Kart")
                sonuc = sonuc.Where(x => x.OdemeTuru.Contains("Nakit: 0")); // Nakit 0 ise sadece karttır
            else if (odeme == "Parçalı (Nakit+Kart)")
                sonuc = sonuc.Where(x => !x.OdemeTuru.Contains("Nakit: 0") && !x.OdemeTuru.Contains("Kart: 0")); // İkisi de 0 değilse parçalıdır
        }

        // 4. PERSONEL FİLTRESİ
        if (PersonelFiltre.SelectedIndex > 0 && PersonelFiltre.SelectedItem != null)
        {
            string secilenPers = PersonelFiltre.SelectedItem.ToString();
            sonuc = sonuc.Where(x => x.PersonelAdi == secilenPers);
        }

        // Sonucu listeye bağla ve en yeni en üstte olacak şekilde sırala
        KapananMasalarListesi.ItemsSource = sonuc.OrderByDescending(x => x.Tarih).ToList();
    }

    private void Filtre_Changed(object sender, EventArgs e) => ListeyiGuncelle();

    private void Temizle_Clicked(object sender, EventArgs e)
    {
        TarihFiltre.Date = DateTime.Now;
        MasaFiltre.Text = string.Empty;
        OdemeFiltre.SelectedIndex = 0;
        PersonelFiltre.SelectedIndex = 0;

        ListeyiGuncelle();
    }

    private async void GeriDon_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();

    // HATA VEREN DETAY SAYFASI YERİNE, ŞIK BİR FİŞ PENCERESİ AÇIYORUZ
    private async void OnMasaTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.GestureRecognizers.Count > 0)
        {
            var tapGesture = border.GestureRecognizers[0] as TapGestureRecognizer;
            if (tapGesture?.CommandParameter is SatisRaporu secilenSatis)
            {
                // Tıklanan masanın içindeki ürünleri ekrana şık bir fiş gibi yansıt
                string fisDetayi = $"İşlemi Yapan: {secilenSatis.PersonelAdi}\n" +
                                   $"Saat: {secilenSatis.Tarih:HH:mm}\n" +
                                   $"Ödeme: {secilenSatis.OdemeTuru}\n\n" +
                                   $"--- SATILAN ÜRÜNLER ---\n{secilenSatis.UrunDetaylari}";

                await DisplayAlert($"Masa {secilenSatis.MasaNo} Adisyonu", fisDetayi, "Kapat");
            }
        }
    }
}