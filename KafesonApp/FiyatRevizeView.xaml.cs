using KafesonApp.Models;

namespace KafesonApp;

public partial class FiyatRevizeView : ContentView
{
    public FiyatRevizeView()
    {
        InitializeComponent();
        KatMenusuOlustur();
    }

    private void KatMenusuOlustur()
    {
        KatMenuStack.Children.Clear();
        var kategoriler = App.Urunler.Select(u => u.Kategori).Distinct().ToList();

        foreach (var kat in kategoriler)
        {
            var btn = new Button
            {
                Text = kat,
                CornerRadius = 20,
                BackgroundColor = Color.FromArgb("#F1F2F6"),
                TextColor = Color.FromArgb("#2F3640"),
                Padding = new Thickness(15, 0)
            };
            btn.Clicked += (s, e) => ÜrünleriFiltrele(kat, btn);
            KatMenuStack.Children.Add(btn);
        }
    }

    private void ÜrünleriFiltrele(string kategori, Button secilenBtn)
    {
        // Buton renklerini sýfýrla
        foreach (var child in KatMenuStack.Children)
            if (child is Button b) { b.BackgroundColor = Color.FromArgb("#F1F2F6"); b.TextColor = Color.FromArgb("#2F3640"); }

        // Seçileni vurgula
        secilenBtn.BackgroundColor = Color.FromArgb("#FFC300");

        // Listeyi filtrele
        FiyatListeView.ItemsSource = App.Urunler.Where(u => u.Kategori == kategori).ToList();
    }

    private async void OnFiyatGuncelleClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var urun = (Urun)btn.CommandParameter;

        // Fiyat güncellendi, JSON'a kaydet
        App.VerileriKaydet();

        await Application.Current.MainPage.DisplayAlert("Baþarýlý", $"{urun.Ad} fiyatý güncellendi.", "Tamam");
    }
}