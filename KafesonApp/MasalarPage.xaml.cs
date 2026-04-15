using Kafeson.Shared.Models;
using KafesonApp.Data;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching; // Timer için gerekli kütüphane eklendi

namespace KafesonApp.Views;

public partial class MasalarPage : ContentPage
{
    private string _seciliMekan = "İç Mekan";

    // 🚨 1. ADIM: CANLI YENİLEME MOTORU TANIMLANDI
    private IDispatcherTimer _canliMotor;

    public MasalarPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await VerileriYukle();

        // 🚨 2. ADIM: MOTORU ÇALIŞTIR (Her 2 saniyede bir sessizce verileri günceller)
        if (_canliMotor == null)
        {
            _canliMotor = Application.Current.Dispatcher.CreateTimer();
            _canliMotor.Interval = TimeSpan.FromSeconds(2); // ŞAK diye düşmesi için 2 saniye idealdir
            _canliMotor.Tick += async (s, e) => await VerileriYukle();
        }
        _canliMotor.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // 🚨 3. ADIM: BAŞKA SAYFAYA GEÇİNCE MOTORU DURDUR (İnterneti ve pili sömürmemesi için)
        if (_canliMotor != null && _canliMotor.IsRunning)
        {
            _canliMotor.Stop();
        }
    }

    private async Task VerileriYukle()
    {
        await App.ApiVerileriniCek();
        MasaListesiniCiz();
    }

    private void MasaListesiniCiz()
    {
        MasalarContainer.Children.Clear();

        var gosterilecekMasalar = App.Masalar
            .Where(m => m.Mekan == _seciliMekan)
            .OrderBy(m => m.No)
            .ToList();

        foreach (var masa in gosterilecekMasalar)
        {
            var border = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = masa.IsDolu ? Color.FromArgb("#EF4444") : Color.FromArgb("#10B981"),
                WidthRequest = 160,
                HeightRequest = 140,
                Margin = new Thickness(10, 10),
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                Shadow = new Shadow { Brush = Brush.Black, Offset = new Point(0, 4), Radius = 8, Opacity = 0.15f }
            };

            var stack = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 8
            };

            stack.Children.Add(new Label
            {
                Text = $"MASA {masa.No}",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            });

            if (masa.IsDolu)
            {
                stack.Children.Add(new Label { Text = $"{masa.ToplamTutar:N2} ₺", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center });

                if (!string.IsNullOrEmpty(masa.GecenSure))
                {
                    stack.Children.Add(new Label { Text = masa.GecenSure, FontSize = 14, TextColor = Color.FromArgb("#FEF08A"), HorizontalOptions = LayoutOptions.Center });
                }
            }
            else
            {
                stack.Children.Add(new Label { Text = "BOŞ", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center });
            }

            border.Content = stack;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => { await Navigation.PushAsync(new SiparisPage(masa)); };
            border.GestureRecognizers.Add(tapGesture);

            MasalarContainer.Children.Add(border);
        }
    }

    private void MekanSec_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            _seciliMekan = btn.CommandParameter.ToString();

            if (_seciliMekan == "İç Mekan")
            {
                BtnIcMekan.BackgroundColor = Color.FromArgb("#3B82F6");
                BtnIcMekan.TextColor = Colors.White;
                BtnBahce.BackgroundColor = Colors.Transparent;
                BtnBahce.TextColor = Color.FromArgb("#94A3B8");
            }
            else
            {
                BtnBahce.BackgroundColor = Color.FromArgb("#3B82F6");
                BtnBahce.TextColor = Colors.White;
                BtnIcMekan.BackgroundColor = Colors.Transparent;
                BtnIcMekan.TextColor = Color.FromArgb("#94A3B8");
            }

            MasaListesiniCiz();
        }
    }

    private async void AnaMenuyeGit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}