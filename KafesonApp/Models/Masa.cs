using System.Collections.ObjectModel; // ObservableCollection için
using System.ComponentModel; // INotifyPropertyChanged için
using System.Runtime.CompilerServices; // CallerMemberName için
using System.Linq; // .Sum() hesaplaması için ŞART

namespace KafesonApp.Models
{
    public class Masa : INotifyPropertyChanged
    {
        public int No { get; set; }

        // CS1061 hatasını bu iki satır çözer
        public bool IsDolu { get; set; }
        public double OdenmisTutar { get; set; }

        public ObservableCollection<Urun> Siparisler { get; set; } = new();
        public ObservableCollection<Urun> Sepet { get; set; } = new();

        // Negatif bakiye hatasını (image_87389e) bu mantık çözer: 
        // Kalan borç, sadece o an listede duran ürünlerin toplamıdır.
        public double KalanTutar => Siparisler.Sum(x => x.Fiyat * x.Miktar);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}