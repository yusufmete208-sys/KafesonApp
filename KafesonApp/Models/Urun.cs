using System.ComponentModel; //
using System.Runtime.CompilerServices; //

namespace KafesonApp.Models
{
    public class Urun : INotifyPropertyChanged
    {
        public string Ad { get; set; } = string.Empty;
        public double Fiyat { get; set; }
        public string Kategori { get; set; } = string.Empty; // CS0117/CS1061 HATASINI ÇÖZER
        public bool IsSecili { get; set; }

        private int _miktar; // Adisyondaki miktar (Örn: 4)
        public int Miktar
        {
            get => _miktar;
            set { _miktar = value; OnPropertyChanged(); OnPropertyChanged(nameof(ToplamFiyat)); }
        }

        private int _odenecekAdet; // Ödeme ekranında seçilen (Örn: 2)
        public int OdenecekAdet
        {
            get => _odenecekAdet;
            set { _odenecekAdet = value; OnPropertyChanged(); }
        }

        public double ToplamFiyat => Fiyat * Miktar;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}