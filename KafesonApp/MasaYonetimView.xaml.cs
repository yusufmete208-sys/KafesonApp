using KafesonApp.Data;
using Kafeson.Shared.Models; // Model adresi düzeltildi
using System.Collections.ObjectModel;
using System.Linq;

namespace KafesonApp.Views;

public partial class MasaYonetimView : ContentView
{
    string _seciliMekan = "İç Mekan";
    private readonly VeriServisi _servis = new VeriServisi();

    public MasaYonetimView()
    {
        InitializeComponent();
        MekanPicker.SelectedIndex = 0;
        VerileriYukle();
    }

    private async void VerileriYukle()
    {
        try
        {
            var liste = await _servis.MasalarGetir();
            if (App.Masalar == null) App.Masalar = new ObservableCollection<Masa>();

            App.Masalar.Clear();
            foreach (var m in liste) App.Masalar.Add(m);
            ListeGuncelle();
        }
        catch { }
    }

    private async void MasaEkle_Clicked(object sender, EventArgs e)
    {
        // 1. KONTROL: Kutu boş mu?
        if (string.IsNullOrWhiteSpace(MasaNoEntry.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Eksik Bilgi", "Lütfen bir Masa Numarası girin!", "Tamam");
            return;
        }

        // 2. KONTROL: Girilen şey sayı mı?
        if (!int.TryParse(MasaNoEntry.Text, out int no))
        {
            await Application.Current.MainPage.DisplayAlert("Hata", "Masa numarası sadece sayı olabilir!", "Tamam");
            return;
        }

        // 3. KONTROL: Mekan seçili mi?
        if (MekanPicker.SelectedItem == null)
        {
            await Application.Current.MainPage.DisplayAlert("Hata", "Lütfen bir mekan seçin!", "Tamam");
            return;
        }

        string mekan = MekanPicker.SelectedItem.ToString();
        var yeniMasa = new Masa { No = no, Mekan = mekan, IsDolu = false };

        // 4. İŞLEM: Sunucuya Gönder
        bool sonuc = await _servis.MasaEkle(yeniMasa);

        if (sonuc)
        {
            App.Masalar.Add(yeniMasa);
            MasaNoEntry.Text = ""; // Kutuyu temizle
            ListeGuncelle();
            await Application.Current.MainPage.DisplayAlert("Başarılı", $"Masa {no} oluşturuldu.", "Tamam");
        }
        else
        {
            // Arka plandaki siyah ekranda gördüğün kırmızı yazılar sunucu hatasıdır.
            await Application.Current.MainPage.DisplayAlert("Sunucu Hatası", "Masa eklenemedi.\nBilgisayardaki siyah konsol penceresinde hata var mı kontrol et.", "Tamam");
        }
    }
    // EKSİK OLAN METOT EKLENDİ
    private async void MasaSil_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Masa masa)
        {
            if (masa.IsDolu)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Dolu masa silinemez!", "Tamam");
                return;
            }

            bool cevap = await Application.Current.MainPage.DisplayAlert("Sil?", $"Masa {masa.No} silinsin mi?", "Evet", "Hayır");
            if (cevap)
            {
                if (await _servis.MasaSil(masa.No))
                {
                    App.Masalar.Remove(masa);
                    ListeGuncelle();
                }
            }
        }
    }

    private void Filtrele_Clicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn?.CommandParameter == null) return;
        _seciliMekan = btn.CommandParameter.ToString();

        // Tasarım renk değişimi
        if (_seciliMekan == "İç Mekan")
        {
            BtnFiltreIcMekan.BackgroundColor = Color.FromArgb("#3B82F6"); BtnFiltreIcMekan.TextColor = Colors.White;
            BtnFiltreBahce.BackgroundColor = Colors.Transparent; BtnFiltreBahce.TextColor = Color.FromArgb("#94A3B8");
        }
        else
        {
            BtnFiltreBahce.BackgroundColor = Color.FromArgb("#3B82F6"); BtnFiltreBahce.TextColor = Colors.White;
            BtnFiltreIcMekan.BackgroundColor = Colors.Transparent; BtnFiltreIcMekan.TextColor = Color.FromArgb("#94A3B8");
        }

        ListeGuncelle();
    }

    private void ListeGuncelle()
    {
        if (App.Masalar != null)
            MasaListesi.ItemsSource = App.Masalar.Where(x => x.Mekan == _seciliMekan).OrderBy(x => x.No).ToList();
    }
}