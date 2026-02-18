using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KafesonApp.Models
{
    public class Urun : INotifyPropertyChanged
    {
        // Ürün Adı
        public string Ad { get; set; } = string.Empty;

        // Kategori (Filtreleme ve gruplama için gerekli)
        public string Kategori { get; set; } = string.Empty;

        // Seçim kutucukları için (Checkbox)
        private bool _isSecili;
        public bool IsSecili
        {
            get => _isSecili;
            set { _isSecili = value; OnPropertyChanged(); }
        }

        // FİYAT
        private double _fiyat;
        public double Fiyat
        {
            get => _fiyat;
            set
            {
                if (_fiyat != value)
                {
                    _fiyat = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ToplamFiyat)); // Fiyat değişirse toplamı güncelle
                }
            }
        }

        // MİKTAR (Adisyondaki adet)
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
                    OnPropertyChanged(nameof(ToplamFiyat)); // Miktar değişirse toplamı güncelle
                }
            }
        }

        // ÖDEME EKRANI İÇİN (Parçalı ödeme vb.)
        private int _odenecekAdet;
        public int OdenecekAdet
        {
            get => _odenecekAdet;
            set
            {
                if (_odenecekAdet != value)
                {
                    _odenecekAdet = value;
                    OnPropertyChanged();
                }
            }
        }

        // TOPLAM FİYAT (Otomatik Hesaplanır)
        // Hatalı kısmı düzelttim:
        public double ToplamFiyat => Fiyat * Miktar;

        // --- INotifyPropertyChanged Uygulaması (Arayüz güncellemesi için şart) ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}