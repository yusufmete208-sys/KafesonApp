using KafesonApp.Models; // Modelleri tanýmasý için

namespace KafesonApp;

public partial class MasalarPage : ContentPage
{
    public MasalarPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa her açýldýðýnda (örneðin ayarlardan geri dönünce) 
        // içerideki her þeyi silip en güncel listeyi baþtan çizer.
        MasalariYukle();
    }

    private void MasalariYukle()
    {
        if (MasalarContainer == null) return;
        MasalarContainer.Children.Clear();

        var siraliMasalar = App.Masalar.OrderBy(m => m.No).ToList();

        foreach (var masa in siraliMasalar)
        {
            // Metni sadece Masa No ve Durum olarak ayarlýyoruz
            string butonMetni = $"Masa {masa.No}\n{(masa.IsDolu ? "DOLU" : "BOÞ")}";

            var masaButonu = new Button
            {
                Text = butonMetni,
                WidthRequest = 160,
                HeightRequest = 160,
                Margin = 10,
                CornerRadius = 20,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                // Doluysa Kýrmýzý, Boþsa Yeþil renk mantýðý devam ediyor
                BackgroundColor = masa.IsDolu ? Color.FromArgb("#FF0000") : Color.FromArgb("#008000"),
                TextColor = Colors.White
            };

            masaButonu.Clicked += async (s, e) =>
            {
                await Navigation.PushAsync(new SiparisPage(masa));
            };

            MasalarContainer.Children.Add(masaButonu);
        }
    }






    private async void Masa_Clicked(object sender, EventArgs e)
    {
        var buton = (Button)sender;

        // Casting iþlemini (Masa) olarak deðil, tam yolunu belirterek yapalým
        if (buton.BindingContext is KafesonApp.Models.Masa secilenMasa)
        {
            // Sipariþ sayfasýna Masa objesini gönderiyoruz
            await Navigation.PushAsync(new SiparisPage(secilenMasa));
        }
    }


    // YENÝ BUTONUN KODU BURAYA (Sýnýfýn en altýna ama son parantezin içine)
    private async void AnaMenuyeGit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); //
    }
}