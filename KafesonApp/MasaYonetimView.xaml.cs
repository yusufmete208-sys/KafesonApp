using KafesonApp.Models;
using System.Linq;

namespace KafesonApp;

// DÝKKAT: ContentPage yerine ContentView olmalý
public partial class MasaYonetimiView : ContentView
{
    string _seciliMekan = "Ýç Mekan";

    public MasaYonetimiView()
    {
        InitializeComponent();
        MekanPicker.SelectedIndex = 0;
        // ContentView'larda App.Masalar'ýn dolu olduðundan emin olmalýyýz
        if (App.Masalar != null) ListeGuncelle();
    }

    private void MasaEkle_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(MasaNoEntry.Text, out int no) && MekanPicker.SelectedItem != null)
        {
            string mekan = MekanPicker.SelectedItem.ToString();
            if (App.Masalar.Any(x => x.No == no && x.Mekan == mekan)) return;

            App.Masalar.Add(new Masa { No = no, Mekan = mekan, IsDolu = false });
            MasaNoEntry.Text = "";
            ListeGuncelle();
        }
    }

    private void Filtrele_Clicked(object sender, EventArgs e)
    {
        var buton = (Button)sender;
        _seciliMekan = buton.CommandParameter.ToString();
        ListeGuncelle();
    }

    private void ListeGuncelle()
    {
        MasaListesi.ItemsSource = App.Masalar
            .Where(x => x.Mekan == _seciliMekan)
            .OrderBy(x => x.No)
            .ToList();
    }

    private void MasaSil_Clicked(object sender, EventArgs e)
    {
        var masa = (Masa)((Button)sender).CommandParameter;
        if (masa.IsDolu) return;
        App.Masalar.Remove(masa);
        ListeGuncelle();
    }
}