using KafesonApp.Models;

namespace KafesonApp;

public partial class UrunListesiPage : ContentPage
{
    public UrunListesiPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ListeyiOlustur();
    }

    private void ListeyiOlustur()
    {
        // XAML'da ismi tanýmladýðýn an bu kýrmýzý çizgi gidecek
        UrunlerListesiContainer.Children.Clear();

        var kategoriler = App.Urunler.GroupBy(u => u.Kategori).ToList();

        foreach (var grup in kategoriler)
        {
            var frame = new Frame { BackgroundColor = Color.FromArgb("#FDFEFE"), Margin = new Thickness(0, 5) };
            var stack = new VerticalStackLayout { Spacing = 5 };

            stack.Children.Add(new Label { Text = grup.Key.ToUpper(), FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.Orange });

            foreach (var urun in grup)
            {
                // Grid tanýmýný hatasýz þekilde yapýyoruz
                var urunGrid = new Grid
                {
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
                };

                var adLabel = new Label { Text = urun.Ad, VerticalOptions = LayoutOptions.Center };
                var fiyatLabel = new Label { Text = $"{urun.Fiyat:N2} TL", FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };

                // Elemanlarý Grid sütunlarýna hatasýz yerleþtiriyoruz
                urunGrid.Add(adLabel, 0, 0); // 0. Sütun
                urunGrid.Add(fiyatLabel, 1, 0); // 1. Sütun

                stack.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray });
                stack.Children.Add(urunGrid);
            }
            frame.Content = stack;
            UrunlerListesiContainer.Children.Add(frame);
        }
    }

    private async void GeriDon_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();
}