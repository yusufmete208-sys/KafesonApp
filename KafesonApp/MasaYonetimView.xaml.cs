using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

public partial class MasaYonetimView : ContentView // i harfi silindi
{
    string _seciliMekan = "İç Mekan";

    public MasaYonetimView() // i harfi silindi
    {
        InitializeComponent();
        MekanPicker.SelectedIndex = 0;

        if (App.Masalar != null) ListeGuncelle();
    }

    private void MasaEkle_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(MasaNoEntry.Text, out int no) && MekanPicker.SelectedItem != null)
        {
            string mekan = MekanPicker.SelectedItem.ToString();

            if (App.Masalar.Any(x => x.No == no && x.Mekan == mekan))
            {
                Application.Current.MainPage.DisplayAlert("Hata", "Bu masa numarası zaten mevcut!", "Tamam");
                return;
            }

            App.Masalar.Add(new Masa { No = no, Mekan = mekan, IsDolu = false });
            App.VerileriKaydet();

            MasaNoEntry.Text = "";
            ListeGuncelle();
        }
        else
        {
            Application.Current.MainPage.DisplayAlert("Uyarı", "Geçerli bir masa numarası giriniz.", "Tamam");
        }
    }

    private void Filtrele_Clicked(object sender, EventArgs e)
    {
        var buton = (Button)sender;
        _seciliMekan = buton.CommandParameter.ToString();

        BtnFiltreIcMekan.BackgroundColor = _seciliMekan == "İç Mekan" ? Color.FromArgb("#3B82F6") : Colors.Transparent;
        BtnFiltreIcMekan.TextColor = _seciliMekan == "İç Mekan" ? Colors.White : Color.FromArgb("#94A3B8");

        BtnFiltreBahce.BackgroundColor = _seciliMekan == "Bahçe" ? Color.FromArgb("#3B82F6") : Colors.Transparent;
        BtnFiltreBahce.TextColor = _seciliMekan == "Bahçe" ? Colors.White : Color.FromArgb("#94A3B8");

        ListeGuncelle();
    }

    private void ListeGuncelle()
    {
        MasaListesi.ItemsSource = App.Masalar
            .Where(x => x.Mekan == _seciliMekan)
            .OrderBy(x => x.No)
            .ToList();
    }

    private async void MasaSil_Clicked(object sender, EventArgs e)
    {
        var masa = (Masa)((Button)sender).CommandParameter;

        if (masa.IsDolu)
        {
            await Application.Current.MainPage.DisplayAlert("Hata", "Müşteri olan (dolu) bir masayı silemezsiniz. Önce hesabı kapatın.", "Tamam");
            return;
        }

        bool onay = await Application.Current.MainPage.DisplayAlert("Onay", $"Masa {masa.No} silinecek, emin misiniz?", "Evet", "Hayır");
        if (onay)
        {
            App.Masalar.Remove(masa);
            App.VerileriKaydet();
            ListeGuncelle();
        }
    }
}