using System.Text.Json;
using System.Text.Json.Serialization;
using KafesonApp.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace KafesonApp.Data;

// Windows'tan gelen "Büyük Paketi" temsil eden yardımcı sınıf
public class KasaPaketi
{
    public List<Masa> Masalar { get; set; } = new();
    public List<Urun> Urunler { get; set; } = new();
    public List<Kullanici> Kullanicilar { get; set; } = new();
}

public class VeriServisi
{
    private static readonly HttpClient _haberci = new HttpClient();
    private const string KasaAdresi = "http://10.0.2.2:5000";

    private readonly JsonSerializerOptions _ayarlar = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    // --- 1. FULL PAKETİ GETİR (Masalar + Ürünler + Kullanıcılar) ---
    public async Task<KasaPaketi> KasadanHerSeyiGetir()
    {
        try
        {
            _haberci.Timeout = TimeSpan.FromSeconds(5);
            var cevap = await _haberci.GetStringAsync($"{KasaAdresi}/masalar");

            if (string.IsNullOrEmpty(cevap)) return null;

            // Artık direkt liste değil, KasaPaketi nesnesini çözüyoruz
            var paket = JsonSerializer.Deserialize<KasaPaketi>(cevap, _ayarlar);
            return paket;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ BAĞLANTI HATASI: {ex.Message}");
            return null;
        }
    }

    // --- 2. VERİ GÖNDERME (POST) ---
    public async Task<bool> MasaGuncelle(Masa guncelMasa)
    {
        try
        {
            string json = JsonSerializer.Serialize(guncelMasa, _ayarlar);
            var icerik = new StringContent(json, Encoding.UTF8, "application/json");
            var cevap = await _haberci.PostAsync($"{KasaAdresi}/guncelle", icerik);
            return cevap.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ GÜNCELLEME HATASI: {ex.Message}");
            return false;
        }
    }
}