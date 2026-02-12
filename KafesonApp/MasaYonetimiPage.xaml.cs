using KafesonApp.Models; // Masa modeli için
using System.Collections.ObjectModel; // ObservableCollection için

namespace KafesonApp;

public partial class MasaYonetimiPage : ContentPage
{
    public MasaYonetimiPage()
    {
        InitializeComponent();
        MasaListesi.ItemsSource = App.Masalar; // Listeyi baðla
    }

    private async void MasaEkle_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(MasaNoEntry.Text, out int masaNo))
        {
            // 1-20 arasýný koruyoruz, buraya müdahale ettirme
            if (masaNo <= 20)
            {
                await DisplayAlert("Bilgi", "1-20 arasý masalar ana planda sabittir.", "Tamam");
                return;
            }

            // Eðer masa zaten varsa ekleme
            if (App.Masalar.Any(m => m.No == masaNo))
            {
                await DisplayAlert("Hata", "Bu masa numarasý zaten mevcut!", "Tamam");
                return;
            }

            // Masayý ekle ve JSON'a kalýcý olarak yaz
            App.Masalar.Add(new Masa { No = masaNo, IsDolu = false });
            App.VerileriKaydet();

            MasaNoEntry.Text = "";
            await DisplayAlert("Baþarýlý", $"Masa {masaNo} listeye eklendi.", "Tamam");
        }
    }

    private async void MasaSil_Clicked(object sender, EventArgs e)
    {
        var masa = (sender as Button).CommandParameter as Masa;

        // Sabit 20 masanýn silinmesini engelle
        if (masa.No <= 20)
        {
            await DisplayAlert("Hata", "Sabit masalar silinemez!", "Tamam");
            return;
        }

        if (await DisplayAlert("Onay", $"Masa {masa.No} silinecek?", "Evet", "Hayýr"))
        {
            App.Masalar.Remove(masa);
            App.VerileriKaydet();
        }
    }

    private async void GeriDon_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}