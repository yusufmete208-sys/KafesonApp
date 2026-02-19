using System;
using System.Collections.ObjectModel; // ObservableCollection için
using System.ComponentModel; // INotifyPropertyChanged için
using System.Runtime.CompilerServices; // CallerMemberName için
using System.Linq; // .Sum() hesaplaması için

namespace KafesonApp.Models
{
    public class Masa : INotifyPropertyChanged
    {
        private bool _isDolu;
        private double _odenmisTutar;
        private DateTime? _acilisZamani;

        public int No { get; set; }

        public bool IsDolu
        {
            get => _isDolu;
            set { _isDolu = value; OnPropertyChanged(); OnPropertyChanged(nameof(GecenSure)); }
        }

        public double OdenmisTutar
        {
            get => _odenmisTutar;
            set { _odenmisTutar = value; OnPropertyChanged(); }
        }

        // Masanın açıldığı anı tutan alan
        public DateTime? AcilisZamani
        {
            get => _acilisZamani;
            set { _acilisZamani = value; OnPropertyChanged(); OnPropertyChanged(nameof(GecenSure)); }
        }

        public ObservableCollection<Urun> Siparisler { get; set; } = new();
        public ObservableCollection<Urun> Sepet { get; set; } = new();
        public double NakitTutari { get; set; }

        public double KartTutari { get; set; }
        // Models/Masa.cs içindeki sınıfa ekleyin
        public double NakitBirikim { get; set; }
        public double KartBirikim { get; set; }
        // Models/Masa.cs içine ekle
        public List<SatisRaporu> Odemeler { get; set; } = new List<SatisRaporu>();
        public List<Urun> KapanisUrunleri { get; set; } = new List<Urun>();

        // Kalan borç hesabı: Siparişlerin toplamından ödenmiş tutarı düşer
        public double KalanTutar => Siparisler.Sum(x => x.Fiyat * x.Miktar) - OdenmisTutar;

        // Masanın ne kadar süredir açık olduğunu hesaplayan özellik
        public string GecenSure
        {
            get
            {
                if (AcilisZamani == null || !IsDolu) return "";
                var fark = DateTime.Now - AcilisZamani.Value;
                return $"{(int)fark.TotalMinutes} dk";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            // Siparişler veya ödenen tutar değiştiğinde KalanTutar'ı da güncelle
            if (name == nameof(OdenmisTutar))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KalanTutar)));
        }
    }
}