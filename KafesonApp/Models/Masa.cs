using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SQLite; // VERİTABANI İÇİN EKLENDİ
using System.Text.Json; // JSON ÇEVİRİSİ İÇİN EKLENDİ

namespace KafesonApp.Models
{
    public class Masa : INotifyPropertyChanged
    {
        // Masanın Numarasını Kimlik (Primary Key) yapıyoruz (Masa 1, Masa 2 benzersizdir)
        [PrimaryKey]
        public int No { get; set; }

        private bool _isDolu;
        private string _mekan = "İç Mekan";

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

        public double NakitBirikim { get; set; }
        public double KartBirikim { get; set; }
        public double OdenmisTutar { get; set; }
        public DateTime? AcilisZamani { get; set; }

        // --- HESAPLAMA ALANLARI (Veritabanına kaydedilmez) ---
        [Ignore]
        public double ToplamTutar => Siparisler?.Sum(x => x.Miktar * x.Fiyat) ?? 0;

        [Ignore]
        public double KalanTutar => ToplamTutar - OdenmisTutar;

        // ==============================================================================
        // --- LİSTELER VE GÖLGE DEĞİŞKENLER (VERİTABANI İÇİN) ---
        // ==============================================================================

        // 1. SİPARİŞLER
        [Ignore] // SQLite bunu görmezden gelir, ekranda kullanılır
        public ObservableCollection<Urun> Siparisler { get; set; } = new ObservableCollection<Urun>();

        // SQLite bunu kaydeder (Siparisler listesini JSON metnine çevirip saklar)
        public string SiparislerDb
        {
            get => JsonSerializer.Serialize(Siparisler);
            set => Siparisler = string.IsNullOrWhiteSpace(value) ? new ObservableCollection<Urun>() : JsonSerializer.Deserialize<ObservableCollection<Urun>>(value);
        }

        // 2. SEPET
        [Ignore]
        public ObservableCollection<Urun> Sepet { get; set; } = new ObservableCollection<Urun>();

        public string SepetDb
        {
            get => JsonSerializer.Serialize(Sepet);
            set => Sepet = string.IsNullOrWhiteSpace(value) ? new ObservableCollection<Urun>() : JsonSerializer.Deserialize<ObservableCollection<Urun>>(value);
        }

        // 3. KAPANIŞ ÜRÜNLERİ
        [Ignore]
        public List<Urun> KapanisUrunleri { get; set; } = new List<Urun>();

        public string KapanisUrunleriDb
        {
            get => JsonSerializer.Serialize(KapanisUrunleri);
            set => KapanisUrunleri = string.IsNullOrWhiteSpace(value) ? new List<Urun>() : JsonSerializer.Deserialize<List<Urun>>(value);
        }

        // --- UI GÜNCELLEME SİSTEMİ ---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void YenidenHesapla()
        {
            OnPropertyChanged(nameof(ToplamTutar));
            OnPropertyChanged(nameof(KalanTutar));
        }
    }
}