using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KafesonApp.Models;

namespace KafesonApp.Data;

public class YerelSunucu
{
    private HttpListener _dinleyici;

    public void Baslat()
    {
        Task.Run(async () =>
        {
            try
            {
                _dinleyici = new HttpListener();
                _dinleyici.Prefixes.Add("http://*:5000/");
                _dinleyici.Start();
                Console.WriteLine("Kasa Sunucusu Başlatıldı (Port: 5000)...");

                while (true)
                {
                    var icerik = await _dinleyici.GetContextAsync();
                    var istek = icerik.Request;
                    var cevap = icerik.Response;

                    // --- 1. FULL PAKET GÖNDERME (GET /masalar) ---
                    // Telefon artık buradan sadece masaları değil, menüyü de alacak!
                    if (istek.HttpMethod == "GET" && istek.Url.AbsolutePath == "/masalar")
                    {
                        var ayarlar = new JsonSerializerOptions
                        {
                            ReferenceHandler = ReferenceHandler.IgnoreCycles,
                            WriteIndented = true
                        };

                        // Dükkanın tüm verisini tek bir paket yapıyoruz
                        var veriPaketi = new
                        {
                            Masalar = App.Masalar.ToList(),
                            Urunler = App.Urunler.ToList(), // KRİTİK: Menü burada gidiyor!
                            Kullanicilar = App.Kullanicilar.ToList()
                        };

                        string jsonPaket = JsonSerializer.Serialize(veriPaketi, ayarlar);
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonPaket);

                        cevap.ContentType = "application/json";
                        cevap.ContentLength64 = buffer.Length;
                        await cevap.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }

                    // --- 2. VERİ ALMA (POST /guncelle) ---
                    else if (istek.HttpMethod == "POST" && istek.Url.AbsolutePath == "/guncelle")
                    {
                        using (var okuyucu = new StreamReader(istek.InputStream, istek.ContentEncoding))
                        {
                            string gelenJson = await okuyucu.ReadToEndAsync();
                            var guncelMasa = JsonSerializer.Deserialize<Masa>(gelenJson);

                            if (guncelMasa != null)
                            {
                                var hedefMasa = App.Masalar.FirstOrDefault(m => m.No == guncelMasa.No);
                                if (hedefMasa != null)
                                {
                                    hedefMasa.IsDolu = guncelMasa.IsDolu;
                                    hedefMasa.Siparisler = guncelMasa.Siparisler;
                                    hedefMasa.NakitBirikim = guncelMasa.NakitBirikim;
                                    hedefMasa.KartBirikim = guncelMasa.KartBirikim;
                                    hedefMasa.OdenmisTutar = guncelMasa.OdenmisTutar;
                                    hedefMasa.AcilisZamani = guncelMasa.AcilisZamani;

                                    hedefMasa.YenidenHesapla();
                                    App.VerileriKaydet();

                                    Console.WriteLine($"Masa {guncelMasa.No} güncellendi ve kaydedildi.");
                                }
                            }
                        }
                    }

                    cevap.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sunucu Hatası: " + ex.Message);
            }
        });
    }
}