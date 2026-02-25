using KafesonApp.Models;
using Microsoft.Maui.Controls.Shapes; // Yuvarlak hatlar (RoundRectangle) için gerekli kütüphane

namespace KafesonApp;

public partial class MasalarPage : ContentPage
{
    string _seciliMekan = "İç Mekan";

    public MasalarPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MasalariYukle();
    }

    private void MekanSec_Clicked(object sender, EventArgs e)
    {
        var buton = (Button)sender;
        _seciliMekan = buton.CommandParameter.ToString();

        // Seçilen mekana göre butonların renklerini güncelle (Animasyonlu geçiş hissi verir)
        BtnIcMekan.BackgroundColor = _seciliMekan == "İç Mekan" ? Color.FromArgb("#3B82F6") : Colors.Transparent;
        BtnIcMekan.TextColor = _seciliMekan == "İç Mekan" ? Colors.White : Color.FromArgb("#94A3B8");

        BtnBahce.BackgroundColor = _seciliMekan == "Bahçe" ? Color.FromArgb("#3B82F6") : Colors.Transparent;
        BtnBahce.TextColor = _seciliMekan == "Bahçe" ? Colors.White : Color.FromArgb("#94A3B8");

        MasalariYukle();
    }

    private void MasalariYukle()
    {
        MasalarContainer.Children.Clear();
        var filtrelenmisMasalar = App.Masalar.Where(m => m.Mekan == _seciliMekan).ToList();

        foreach (var masa in filtrelenmisMasalar)
        {
            // Modern Zümrüt Yeşili ve Canlı Kırmızı
            var bosRenk = Color.FromArgb("#10B981");
            var doluRenk = Color.FromArgb("#EF4444");
            var arkaPlan = masa.IsDolu ? doluRenk : bosRenk;

            // Masayı temsil eden ana kart
            var cardBorder = new Border
            {
                WidthRequest = 160,
                HeightRequest = 160,
                Margin = new Thickness(10),
                BackgroundColor = arkaPlan,
                StrokeThickness = 0,
                Padding = new Thickness(15),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 6), Opacity = 0.2f, Radius = 10 }
            };

            // Kartın içindeki yazıları alt alta dizecek yapı
            var layout = new VerticalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };

            layout.Children.Add(new Label
            {
                Text = $"MASA {masa.No}",
                TextColor = Colors.White,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            });

            if (masa.IsDolu)
            {
                string gecenSure = "Yeni";
                if (masa.AcilisZamani.HasValue)
                {
                    var fark = DateTime.Now - masa.AcilisZamani.Value;
                    gecenSure = $"{Math.Floor(fark.TotalMinutes)} dk";
                }

                layout.Children.Add(new Label { Text = "DOLU", TextColor = Colors.White, FontSize = 12, HorizontalOptions = LayoutOptions.Center, Opacity = 0.8 });
                layout.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.White, Opacity = 0.3, Margin = new Thickness(0, 5) });
                layout.Children.Add(new Label { Text = $"{masa.ToplamTutar:N2} ₺", TextColor = Colors.White, FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center });
                layout.Children.Add(new Label { Text = $"🕒 {gecenSure}", TextColor = Colors.White, FontSize = 12, HorizontalOptions = LayoutOptions.Center, Opacity = 0.9 });
            }
            else
            {
                layout.Children.Add(new Label { Text = "BOŞ", TextColor = Colors.White, FontSize = 14, HorizontalOptions = LayoutOptions.Center, Opacity = 0.9, Margin = new Thickness(0, 10, 0, 0) });
            }

            cardBorder.Content = layout;

            // Karta Tıklama Özelliği Ekleme
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => {
                await Navigation.PushAsync(new SiparisPage(masa));
            };
            cardBorder.GestureRecognizers.Add(tapGesture);

            MasalarContainer.Children.Add(cardBorder);
        }
    }

    private async void AnaMenuyeGit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}