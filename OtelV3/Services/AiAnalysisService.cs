using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OtelV3.Models;

namespace OtelV3.Services;

/// <summary>
/// Gelişmiş kural tabanlı yapay zeka finansal analiz servisi
/// 15+ akıllı kural ile çalışır
/// </summary>
public class AiAnalysisService
{
    /// <summary>
    /// Finansal verileri analiz eder ve çok detaylı öneriler sunar
    /// </summary>
    public async Task<AiAnalysisResult> AnalyzeAsync(FinancialDataDto data)
    {
        // Asenkron simülasyonu
        await Task.Delay(500);
        
        var result = new AiAnalysisResult
        {
            Recommendations = new List<string>()
        };
        
        // Temel finansal metriks
        var cashFlow = data.NetProfit;
        var profitMargin = data.ProfitMargin;
        var expenseRatio = data.TotalRevenue > 0 ? (double)(data.TotalExpense / data.TotalRevenue) : 0;
        
        // --- KURAL 1: KRİTİK NAKİT AKIŞI RİSKİ ---
        if (data.TotalExpense > data.TotalRevenue)
        {
            result.Score = 25;
            result.Forecast = "🚨 KRİTİK: Nakit akışı RİSKİ! Giderler gelirleri aşıyor.";
            result.Recommendations.Add("🔴 ACİL: Acil olmayan tüm harcamaları DERHAL durdurun");
            result.Recommendations.Add("💰 Peşin ödeme teşvikleri başlatın (%10 indirim)");
            result.Recommendations.Add("📞 Borç yapılandırması için muhasebeci ile görüşün");
            result.Recommendations.Add("🎯 Hızlı gelir: Early check-in/late check-out ücretli hizmetler");
        }
        // --- KURAL 2: YÜKSEK RİSK (Gider/Gelir > %80) ---
        else if (expenseRatio > 0.80)
        {
            result.Score = 40;
            result.Forecast = "⚠️ YÜKSEK RİSK: Giderler çok yüksek, kâr marjı kritik seviyede.";
            result.Recommendations.Add("🟠 ÖNEMLİ: Gider kalemleri incelemesi yapın");
            result.Recommendations.Add("📊 Her departmanın bütçesini %10 azaltın");
            result.Recommendations.Add("🔍 Gereksiz abonelik ve sözleşmeleri iptal edin");
        }
        // --- KURAL 3: ORTA RİSK (Gider/Gelir %70-80) ---
        else if (expenseRatio > 0.70)
        {
            result.Score = 58;
            result.Forecast = "⚡ DİKKAT: Maliyetler yükselişte. Optimizasyon gerekli.";
            result.Recommendations.Add("🟡 ORTA: Tedarikçi fiyatlarını yeniden müzakere edin");
            result.Recommendations.Add("💡 Enerji tasarrufu programı başlatın (LED, termostat)");
            result.Recommendations.Add("📦 Toplu alımlarla indirim sağlayın");
        }
        // --- KURAL 4: SAĞLIKLI DURUM (Gelir > Gider*1.3) ---
        else if (data.TotalRevenue > data.TotalExpense * 1.3m)
        {
            result.Score = 82;
            result.Forecast = "✅ İYİ: Finansal sağlık dengeli ve olumlu.";
            result.Recommendations.Add("🟢 FIRSATLAR: Yatırım yapmak için uygun dönem");
            result.Recommendations.Add("📈 Pazarlama kampanyalarına bütçe ayırın");
            result.Recommendations.Add("🏨 Müşteri deneyimi iyileştirmelerine odaklanın");
        }
        // --- KURAL 5: MÜKEMMEL DURUM (Gelir > Gider*1.5) ---
        else if (data.TotalRevenue > data.TotalExpense * 1.5m)
        {
            result.Score = 93;
            result.Forecast = "🌟 MÜKEMMEL: Finansal performans olağanüstü!";
            result.Recommendations.Add("💎 STRATEJİK: Büyüme planlarına başlayın");
            result.Recommendations.Add("🎁 Çalışan motivasyonu için bonus/prim sistemi");
            result.Recommendations.Add("🏆 Premium segment yatırımları düşünün");
            result.Recommendations.Add("📊 Franchise/şube açılışı fırsatlarını değerlendirin");
        }
        // --- KURAL 6: NORMAL SEVİYE ---
        else
        {
            result.Score = 68;
            result.Forecast = "📊 STABIL: Finansal durum dengeli görünüyor.";
            result.Recommendations.Add("🔵 Mevcut performansı koruyun");
            result.Recommendations.Add("📋 Aylık bütçe takibini disiplinli yapın");
        }
        
        // --- KURAL 7: ENERJİ MALİYETİ KONTROLÜ ---
        if (data.ExpenseBreakdown.TryGetValue("Enerji", out var energyCost) ||
            data.ExpenseBreakdown.TryGetValue("Elektrik", out energyCost))
        {
            var energyRatio = data.TotalExpense > 0 ? (double)(energyCost / data.TotalExpense) : 0;
            
            if (energyRatio > 0.25)
            {
                result.Score = Math.Max(result.Score - 15, 15);
                result.Recommendations.Insert(0, $"⚡ ENERJI ALARM: Enerji giderleri %{energyRatio:F1} seviyesinde! LED, güneş paneli, akıllı termostat uygulamalarına GEÇİN");
            }
            else if (energyRatio > 0.18)
            {
                result.Score = Math.Max(result.Score - 8, 20);
                result.Recommendations.Add($"⚡ ENERJI: %{energyRatio:F1} enerji maliyeti var. Enerji denetimi yapın");
            }
        }
        
        // --- KURAL 8: PERSONEL MALİYETİ KONTROLÜ ---
        if (data.ExpenseBreakdown.TryGetValue("Personel", out var personnelCost) ||
            data.ExpenseBreakdown.TryGetValue("Maaş", out personnelCost))
        {
            var personnelRatio = data.TotalExpense > 0 ? (double)(personnelCost / data.TotalExpense) : 0;
            
            if (personnelRatio > 0.40)
            {
                result.Recommendations.Add($"👥 PERSONEL: %{personnelRatio:F1} personel maliyeti çok yüksek. Vardiya optimizasyonu, part-time çalışanlar, çapraz eğitim değerlendirin");
            }
            else if (personnelRatio < 0.20)
            {
                result.Recommendations.Add("👥 PERSONEL: Personel yetersizliği olabilir. Hizmet kalitesini izleyin");
            }
        }
        
        // --- KURAL 9: GELİR TRENDİ ANALİZİ ---
        if (data.RevenueChangePercent > 20)
        {
            result.Score = Math.Min(result.Score + 8, 100);
            result.Recommendations.Add($"🚀 TREND: Gelirde %{data.RevenueChangePercent:F1} BÜYÜME! Başarılı stratejileri sürdürün ve ölçeklendirin");
        }
        else if (data.RevenueChangePercent > 10)
        {
            result.Score = Math.Min(result.Score + 4, 100);
            result.Recommendations.Add($"📈 POZITIF: %{data.RevenueChangePercent:F1} gelir artışı. İyi gidiyorsunuz!");
        }
        else if (data.RevenueChangePercent < -15)
        {
            result.Score = Math.Max(result.Score - 12, 15);
            result.Recommendations.Insert(0, $"📉 ALARM: Gelirlerde %{Math.Abs(data.RevenueChangePercent):F1} DÜŞÜŞ! Pazarlama kampanyası, fiyat revizyonu, müşteri geri kazanımı ACIL");
        }
        else if (data.RevenueChangePercent < -5)
        {
            result.Score = Math.Max(result.Score - 6, 20);
            result.Recommendations.Add($"📉 DİKKAT: %{Math.Abs(data.RevenueChangePercent):F1} gelir azalması. Rekabet analizi ve pazarlama yapın");
        }
        
        // --- KURAL 10: GİDER ARTIŞI KONTROLÜ ---
        if (data.ExpenseChangePercent > 25)
        {
            result.Recommendations.Insert(0, $"💸 UYARI: Giderler %{data.ExpenseChangePercent:F1} ARTTI! Hangi kalemde artış olduğunu ACIL araştırın");
        }
        else if (data.ExpenseChangePercent > 15)
        {
            result.Recommendations.Add($"💸 DİKKAT: Giderler %{data.ExpenseChangePercent:F1} arttı. Bütçe kontrolü yapın");
        }
        
        // --- KURAL 11: DOLULUK ORANI KORELASYONU ---
        if (data.OccupancyRate > 90)
        {
            result.Recommendations.Add($"🏨 DOLULUK %{data.OccupancyRate:F0}: Yüksek doluluk! Fiyatları artırabilir veya overbooking stratejisi kullanabilirsiniz");
        }
        else if (data.OccupancyRate > 70 && data.OccupancyRate <= 90)
        {
            result.Recommendations.Add($"🏨 DOLULUK %{data.OccupancyRate:F0}: Sağlıklı seviye. Müşteri memnuniyetini koruyun");
        }
        else if (data.OccupancyRate < 50)
        {
            result.Score = Math.Max(result.Score - 10, 20);
            result.Recommendations.Insert(0, $"🏨 DOLULUK %{data.OccupancyRate:F0}: DÜŞÜK! OTA'larda görünürlük artırın, flash sale kampanyaları, influencer iş birlikleri");
        }
        else if (data.OccupancyRate < 70)
        {
            result.Recommendations.Add($"🏨 DOLULUK %{data.OccupancyRate:F0}: Orta seviye. Rezervasyon kanallarını çeşitlendirin");
        }
        
        // --- KURAL 12: KAR MARJI DEĞERLENDİRME ---
        if (profitMargin > 30)
        {
            result.Recommendations.Add($"💰 KAR MARJI %{profitMargin:F1}: Mükemmel! Rekabet avantajınız var");
        }
        else if (profitMargin < 10 && profitMargin > 0)
        {
            result.Recommendations.Add($"💰 KAR MARJI %{profitMargin:F1}: Çok düşük. Fiyatlandırma stratejisi gözden geçirin");
        }
        
        // --- KURAL 13: MEVSIMSELLIK TAVSİYESİ ---
        var currentMonth = DateTime.Now.Month;
        if (currentMonth >= 6 && currentMonth <= 8) // Yaz sezonu
        {
            result.Recommendations.Add("🌞 SEZON: Yaz sezonu! Havuz, açık alan etkinlikleri, aileler için paketler sunun");
        }
        else if (currentMonth >= 11 || currentMonth <= 2) // Kış sezonu
        {
            result.Recommendations.Add("❄️ SEZON: Kış dönemi. Kurumsal müşteriler, konferans paketleri, sıcak içecek promosyonları");
        }
        
        // --- KURAL 14: GİDER DAĞILIMI ANALİZİ ---
        if (data.ExpenseBreakdown.Any())
        {
            var topExpense = data.ExpenseBreakdown.OrderByDescending(x => x.Value).First();
            if (topExpense.Value > data.TotalExpense * 0.35m)
            {
                result.Recommendations.Add($"📊 EN YÜKSEK GİDER: '{topExpense.Key}' toplam giderin %{(topExpense.Value/data.TotalExpense)*100:F0}'ini oluştuyor. Bu alanda optimizasyon fırsatları arayın");
            }
        }
        
        // --- KURAL 15: GELECEK TAHMİNİ İYİLEŞTİRME ---
        if (result.Score >= 80)
        {
            result.Forecast += " Önümüzdeki ay için tahmin: Büyüme devam edecek.";
        }
        else if (result.Score < 40)
        {
            result.Forecast += " Önümüzdeki 2 hafta kritik! Acil aksiyonlar şart.";
        }
        
        // Skor sınırlaması
        result.Score = Math.Clamp(result.Score, 0, 100);
        
        // Öneri yoksa genel bir öneri ekle
        if (result.Recommendations.Count == 0)
        {
            result.Recommendations.Add("📊 Mevcut performansı izlemeye devam edin");
        }
        
        return result;
    }
    
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static AiAnalysisService Instance { get; } = new();
}
