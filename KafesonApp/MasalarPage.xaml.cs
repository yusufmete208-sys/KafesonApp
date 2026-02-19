using KafesonApp.Models;

namespace KafesonApp;

public partial class MasalarPage : ContentPage
{
    // Varsayýlan olarak Ýç Mekan seçili gelir
    string _seciliMekan = "Ýç Mekan";

    public MasalarPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MasalariYukle();
    }

    // Mekan butonlarýna basýldýðýnda çalýþýr
    private void MekanSec_Clicked(object sender, EventArgs e)
    {
        var buton = (Button)sender;
        _seciliMekan = buton.CommandParameter.ToString();

        // Buton görünümlerini güncelle (Hangisi seçiliyse o mavi olur)
        BtnIcMekan.BackgroundColor = _seciliMekan == "Ýç Mekan" ? Color.FromArgb("#3498DB") : Color.FromArgb("#34495E");
        BtnIcMekan.TextColor = _seciliMekan == "Ýç Mekan" ? Colors.White : Color.FromArgb("#BDC3C7");

        BtnBahce.BackgroundColor = _seciliMekan == "Bahçe" ? Color.FromArgb("#3498DB") : Color.FromArgb("#34495E");
        BtnBahce.TextColor = _seciliMekan == "Bahçe" ? Colors.White : Color.FromArgb("#BDC3C7");

        MasalariYukle();
    }

    private void MasalariYukle()
    {
        MasalarContainer.Children.Clear();
        var filtrelenmisMasalar = App.Masalar.Where(m => m.Mekan == _seciliMekan).ToList();

        foreach (var masa in filtrelenmisMasalar)
        {
            string masaMetni;

            if (masa.IsDolu)
            {
                // Dakika hesabý
                string gecenSure = "";
                if (masa.AcilisZamani.HasValue)
                {
                    var fark = DateTime.Now - masa.AcilisZamani.Value;
                    gecenSure = $"\n{Math.Floor(fark.TotalMinutes)} dk";
                }

                // DÜZELTME: Masa No, DURUM ve Fiyat artýk tamamen alt alta
                // Her bir bilgi için yeni satýr (\n) ekledik
                masaMetni = $"MASA {masa.No}\nDOLU\n{masa.ToplamTutar:N2} TL{gecenSure}";
            }
            else
            {
                // Boþ masa yine sade
                masaMetni = $"MASA {masa.No}";
            }

            // --- PREMÝUM TASARIM AYARLARI ---
            var premiumYesil = Color.FromArgb("#1DB954");
            var premiumKirmizi = Color.FromArgb("#E50914");

            var masaButon = new Button
            {
                Text = masaMetni,
                WidthRequest = 180, // Okunabilirlik için biraz daha geniþlettik
                HeightRequest = 180,
                Margin = 12,
                FontSize = 18,
                LineBreakMode = LineBreakMode.WordWrap, // Yazýlarýn taþmasýný engeller
                CornerRadius = 25,
                BackgroundColor = masa.IsDolu ? premiumKirmizi : premiumYesil,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                Padding = 10,
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Offset = new Point(0, 8),
                    Opacity = 0.25f,
                    Radius = 15
                }
            };

            masaButon.Clicked += async (s, e) => {
                await Navigation.PushAsync(new SiparisPage(masa));
            };

            MasalarContainer.Children.Add(masaButon);
        }
    }

    private async void AnaMenuyeGit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}