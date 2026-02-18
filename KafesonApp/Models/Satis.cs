using System;
using System.Collections.Generic;

namespace KafesonApp.Models
{
    public class Satis
    {
        public int MasaNo { get; set; }
        public DateTime? AcilisZamani { get; set; }
        public DateTime KapanisZamani { get; set; }
        public double ToplamTutar { get; set; }
        public List<Urun> Urunler { get; set; } = new List<Urun>();
    }
}