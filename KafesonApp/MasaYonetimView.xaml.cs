using KafesonApp.Models;

namespace KafesonApp;

public partial class MasaYonetimView : ContentView
{
    public MasaYonetimView()
    {
        InitializeComponent();
        MasaListesiniYenile();
    }

    private void MasaListesiniYenile()
    {
        if (App.Masalar == null) return;

        // Masalarý numarasýna göre sýralayarak listele
        MasaCollectionView.ItemsSource = null;
        MasaCollectionView.ItemsSource = App.Masalar.OrderBy(x => x.No).ToList();
    }

    private async void OnMasaEkleClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(MasaNoEntry.Text, out int masaNo))
        {
            await Application.Current.MainPage.DisplayAlert("Uyarý", "Geçerli bir numara girin.", "Tamam");
            return;
        }

        // Ayný numara kontrolü
        if (App.Masalar.Any(x => x.No == masaNo))
        {
            await Application.Current.MainPage.DisplayAlert("Hata", "Bu masa zaten mevcut.", "Tamam");
            return;
        }

        // Yeni masayý ekle
        App.Masalar.Add(new Masa { No = masaNo, IsDolu = false });
        App.VerileriKaydet(); // JSON'a kalýcý olarak kaydet

        MasaNoEntry.Text = string.Empty;
        MasaListesiniYenile();
    }

    private async void OnMasaSilClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var masa = (Masa)btn.CommandParameter;

        // Dolu masa korumasý
        if (masa.IsDolu || (masa.Siparisler != null && masa.Siparisler.Count > 0))
        {
            await Application.Current.MainPage.DisplayAlert("Uyarý", "Dolu veya hesabý olan masa silinemez!", "Tamam");
            return;
        }

        bool onay = await Application.Current.MainPage.DisplayAlert("Onay", $"Masa {masa.No} silinsin mi?", "Evet", "Hayýr");
        if (onay)
        {
            App.Masalar.Remove(masa);
            App.VerileriKaydet(); // Deðiþikliði kaydet
            MasaListesiniYenile();
        }
    }
}