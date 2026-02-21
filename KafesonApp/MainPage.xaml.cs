namespace KafesonApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Masalar butonuna basıldığında (Eğer ayrı bir sayfan varsa)
    private async void Masalar_Clicked(object sender, EventArgs e)
    {
        // ARTIK BİLGİ KUTUSU ÇIKMAYACAK. Doğrudan masalara gidiyoruz:
        await Navigation.PushAsync(new MasalarPage());
    }

    // HATA VEREN METOD: Raporlar butonuna basıldığında
    private async void Raporlar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RaporPage());
    }

    // AYARLAR BUTONU: Ayarlar sayfasına gitmek için
    private async void Ayarlar_Clicked(object sender, EventArgs e)
    {
        // Eski kod: await Navigation.PushAsync(new UrunEklePage());
        // Yeni kod:
        await Navigation.PushAsync(new AyarlarPage()); // Ayarlar ana sayfasına yönlendirir
    }

    private async void KapananMasalar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new KapananMasalar1View());
    }

    private async void Loglar_Clicked(object sender, EventArgs e)
    {
        // LoglarPage henüz oluşturulmadıysa hata verebilir, 3. adımı tamamlayın.
        await Navigation.PushAsync(new LoglarPage());
    }

}