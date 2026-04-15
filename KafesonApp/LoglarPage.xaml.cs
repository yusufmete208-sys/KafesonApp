#nullable disable
using KafesonApp.Data;
using Kafeson.Shared.Models;

namespace KafesonApp;

public partial class LoglarPage : ContentPage
{
    private readonly VeriServisi _servis = new VeriServisi();

    public LoglarPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoglariYukle();
    }

    // 🚨 DÜZELTME: "private" yerine "public" yaptık ki Ayarlar sayfası bunu tetikleyebilsin! 🚨
    public async Task LoglariYukle()
    {
        try
        {
            var liste = await _servis.LoglariGetir();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LogListesi.ItemsSource = liste.OrderByDescending(x => x.Tarih).ToList();
            });
        }
        catch { }
    }
}