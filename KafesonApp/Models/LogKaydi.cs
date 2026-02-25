namespace KafesonApp.Models;

public class LogKaydi
{
    public DateTime Tarih { get; set; } = DateTime.Now;
    public string Mesaj { get; set; } // Adının 'Mesaj' olduğundan emin olun
    public string IslemTipi { get; set; }
}