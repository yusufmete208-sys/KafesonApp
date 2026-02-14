using KafesonApp.Models;

namespace KafesonApp;

public class GosterimGrup : List<Urun>
{
    public string Baslik { get; set; }
    public GosterimGrup(string baslik, List<Urun> urunler) : base(urunler)
    {
        Baslik = baslik;
    }
}

public partial class MenuGosterimView : ContentView
{
    public MenuGosterimView()
    {
        InitializeComponent(); // Hata buradaysa yukarýdaki x:Class ismini kontrol edin
        VerileriYukle();
    }

    private void VerileriYukle()
    {
        if (App.Urunler == null || App.Urunler.Count == 0) return;

        var gruplar = App.Urunler
            .GroupBy(u => u.Kategori)
            .Select(g => new GosterimGrup(string.IsNullOrWhiteSpace(g.Key) ? "Genel" : g.Key, g.ToList()))
            .ToList();

        MenuCollectionView.ItemsSource = gruplar;
    }
}