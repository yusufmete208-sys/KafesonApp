using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // LINQ sorguları (Sum) için şart

namespace KafesonApp.Models
{
    public class Masa : INotifyPropertyChanged
    {
        private bool _isDolu;
        private string _mekan = "İç Mekan"; // Varsayılan mekan

        public int No { get; set; }

        // Mekan bilgisi (İç Mekan veya Bahçe)
        public string Mekan
        {
            get => _mekan;
            set { _mekan = value; OnPropertyChanged(nameof(Mekan)); }
        }

        public bool IsDolu
        {
            get => _isDolu;
            set { _isDolu = value; OnPropertyChanged(nameof(IsDolu)); }
        }

        // --- HESAPLAMA VE ÖDEME ALANLARI ---

        // Masadaki aktif siparişler (Ekranda anlık görünmesi için)
        public ObservableCollection<Urun> Siparisler { get; set; } = new ObservableCollection<Urun>();

        // Sepetteki onay bekleyen ürünler
        public ObservableCollection<Urun> Sepet { get; set; } = new ObservableCollection<Urun>();

        // Kapanışta rapora gidecek ürünlerin yedeği
        public List<Urun> KapanisUrunleri { get; set; } = new List<Urun>();

        // Raporlama için nakit ve kart birikimleri
        public double NakitBirikim { get; set; }
        public double KartBirikim { get; set; }
        public double OdenmisTutar { get; set; }

        public DateTime? AcilisZamani { get; set; }

        // HATAYI ÇÖZEN ÖZELLİK: Masanın toplam borcunu hesaplar
        public double ToplamTutar => Siparisler?.Sum(x => x.Miktar * x.Fiyat) ?? 0;

        // Kalan borç (Toplam - Ödenen)
        public double KalanTutar => ToplamTutar - OdenmisTutar;

        // --- UI GÜNCELLEME SİSTEMİ ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Sipariş listesi değiştiğinde toplam tutarı güncellemesi için tetikleyici
        public void YenidenHesapla()
        {
            OnPropertyChanged(nameof(ToplamTutar));
            OnPropertyChanged(nameof(KalanTutar));
        }
    }
}