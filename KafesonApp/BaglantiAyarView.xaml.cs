using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using System.IO;

namespace KafesonApp;

public partial class BaglantiAyarView : ContentView // BİZE KIZAN YER BURASIYDI, DÜZELTTİK!
{
    public BaglantiAyarView()
    {
        InitializeComponent();

        string kayitliIp = Preferences.Default.Get("SunucuIP", "192.168.1.108");

        if (IpEntry != null)
            IpEntry.Text = kayitliIp;

        QrKodunuGuncelle(kayitliIp);
    }

    private async void Kaydet_Clicked(object sender, EventArgs e)
    {
        string yeniIp = IpEntry.Text?.Trim();

        if (string.IsNullOrEmpty(yeniIp))
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Hata", "Lütfen geçerli bir IP adresi girin.", "Tamam");
            return;
        }

        Preferences.Default.Set("SunucuIP", yeniIp);
        QrKodunuGuncelle(yeniIp);

        await App.ApiVerileriniCek();

        if (Application.Current?.MainPage != null)
            await Application.Current.MainPage.DisplayAlert("Başarılı", $"Bağlantı ayarlandı! Uygulama artık yeni sunucuya ({yeniIp}) bağlandı.", "Tamam");
    }

    private void QrKodunuGuncelle(string ip)
    {
        string menuLinki = $"http://{ip}:5000/menu.html";

        if (QrImage != null)
            QrImage.Source = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={menuLinki}";
    }

    private async void QrYazdir_Clicked(object sender, EventArgs e)
    {
        string ip = Preferences.Default.Get("SunucuIP", "192.168.1.108");
        string menuLinki = $"http://{ip}:5000/menu.html";
        string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=500x500&data={menuLinki}";

        try
        {
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(qrUrl);

            string tempDosyaYolu = Path.Combine(FileSystem.CacheDirectory, "Kafeson_QR_Menu.png");
            File.WriteAllBytes(tempDosyaYolu, imageBytes);

#if WINDOWS
            Process.Start(new ProcessStartInfo
            {
                FileName = tempDosyaYolu,
                Verb = "print",
                UseShellExecute = true
            });
#else
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "QR Menüyü Yazdır",
                File = new ShareFile(tempDosyaYolu)
            });
#endif
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Hata", "Yazdırma başlatılamadı: " + ex.Message, "Tamam");
        }
    }
}