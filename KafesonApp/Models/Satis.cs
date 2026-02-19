using System;
using System.Collections.Generic;

namespace KafesonApp.Models
{
    public class Satis
    {
        public int SiraNo { get; set; } // Günlük kaçıncı masa olduğu
                                        // Models/Satis.cs içine ekleyin
        
        public int MasaNo { get; set; }
        public DateTime? AcilisZamani { get; set; }
        public DateTime KapanisZamani { get; set; }
        public double ToplamTutar { get; set; }
        public double NakitTutari { get; set; } // Nakit ödenen kısım
        public double KartTutari { get; set; }  // Kartla ödenen kısım
        public List<Urun> Urunler { get; set; } = new List<Urun>();
    }
}