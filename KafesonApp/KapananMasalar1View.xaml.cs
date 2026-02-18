namespace KafesonApp;

public partial class KapananMasalar1View : ContentView
{
    public KapananMasalar1View()
    {
        InitializeComponent();
        // App içindeki statik listeyi baðlýyoruz
        KapananMasalarListesi.ItemsSource = App.KapananMasalar;
    }
}