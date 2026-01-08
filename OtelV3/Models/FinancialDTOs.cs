using System.Collections.Generic;

namespace OtelV3.Models;

/// <summary>
/// Finansal analiz için ham veri modeli
/// </summary>
public class FinancialDataDto
{
    /// <summary>
    /// Toplam gelir
    /// </summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>
    /// Toplam gider
    /// </summary>
    public decimal TotalExpense { get; set; }
    
    /// <summary>
    /// En yüksek harcama kalemi
    /// </summary>
    public string HighestExpenseCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// En yüksek harcama tutarı
    /// </summary>
    public decimal HighestExpenseAmount { get; set; }
    
    /// <summary>
    /// Geçen aya göre gelir değişim oranı (%)
    /// </summary>
    public double RevenueChangePercent { get; set; }
    
    /// <summary>
    /// Geçen aya göre gider değişim oranı (%)
    /// </summary>
    public double ExpenseChangePercent { get; set; }
    
    /// <summary>
    /// Otel doluluk oranı (%)
    /// </summary>
    public double OccupancyRate { get; set; }
    
    /// <summary>
    /// Kategori bazlı gider dağılımı
    /// </summary>
    public Dictionary<string, decimal> ExpenseBreakdown { get; set; } = new();
    
    /// <summary>
    /// Net kar
    /// </summary>
    public decimal NetProfit => TotalRevenue - TotalExpense;
    
    /// <summary>
    /// Kar marjı (%)
    /// </summary>
    public double ProfitMargin => TotalRevenue > 0 ? (double)((NetProfit / TotalRevenue) * 100) : 0;
}

/// <summary>
/// AI analiz sonucu modeli
/// </summary>
public class AiAnalysisResult
{
    /// <summary>
    /// AI güven/sağlık skoru (0-100)
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// Gelecek tahmini metni
    /// </summary>
    public string Forecast { get; set; } = string.Empty;
    
    /// <summary>
    /// AI önerileri listesi
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}
