using KafesonApp.Models;

namespace KafesonApp;

public partial class SistemLogsPage : ContentPage
{
    public SistemLogsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Logları listeye bağlıyoruz
        LogListesi.ItemsSource = App.SistemLoglari;
    }

    private async void GeriDon_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();
}