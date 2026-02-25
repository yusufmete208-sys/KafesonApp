namespace KafesonApp;

public partial class LoglarPage : ContentPage
{
    public LoglarPage()
    {
        InitializeComponent();

        // App.xaml.cs içindeki merkezi log listesine baðlanýyoruz
        BindingContext = App.Loglar;
    }
}