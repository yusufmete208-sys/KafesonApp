using KafesonApp.Models;
using SQLite;

namespace KafesonApp.Data;

public class YerelVeritabani
{
    // Veritabanı bağlantımızı tutacak değişken
    private SQLiteAsyncConnection _database;

    // Veritabanına ilk bağlandığımızda dosyayı ve tabloları oluşturacak metod
    public async Task Init()
    {
        // Eğer veritabanı zaten oluşturulmuşsa tekrar işlem yapma
        if (_database is not null)
            return;

        // Veritabanı dosyasının cihaz içindeki gizli yolunu ve adını belirliyoruz
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "KafesonVeri.db3");

        // Bağlantıyı kur (Dosya yoksa otomatik oluşturur)
        _database = new SQLiteAsyncConnection(dbPath);

        // TABLOLARI OLUŞTUR (Urun ve Masa modellerini buraya ekleyeceğiz)
        await _database.CreateTableAsync<Urun>();
        await _database.CreateTableAsync<Masa>();
    }
}