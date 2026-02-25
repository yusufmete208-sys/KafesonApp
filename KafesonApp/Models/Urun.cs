using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using SQLite; // VERİTABANI İÇİN EKLENDİ

namespace KafesonApp.Models
{
    public class Urun : INotifyPropertyChanged
    {
        // VERİTABANI KİMLİĞİ (Her ürün eklendiğinde 1, 2, 3 diye otomatik artar)
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Ad { get; set; } = "";
        public double Fiyat { get; set; }
        public string Kategori { get; set; } = "";

        private int _miktar;
        public int Miktar
        {
            get => _miktar;
            set
            {
                if (_miktar != value)
                {
                    _miktar = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ToplamFiyat));
                }
            }
        }

        // Sadece hesaplama yapıyor, veritabanına boşuna yük olmasın diye Ignore ekledik
        [Ignore]
        public double ToplamFiyat => Fiyat * Miktar;

        // --- EKRAN İŞLEMLERİ (Veritabanına Kaydedilmeyecekler) ---

        private bool _isSecili;
        [JsonIgnore]
        [Ignore] // Veritabanında sütun açılmasını engeller
        public bool IsSecili
        {
            get => _isSecili;
            set { if (_isSecili != value) { _isSecili = value; OnPropertyChanged(); } }
        }

        private int _odenecekAdet;
        [JsonIgnore]
        [Ignore] // Veritabanında sütun açılmasını engeller
        public int OdenecekAdet
        {
            get => _odenecekAdet;
            set { if (_odenecekAdet != value) { _odenecekAdet = value; OnPropertyChanged(); } }
        }
        // ------------------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}