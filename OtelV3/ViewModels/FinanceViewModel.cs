using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Material.Icons;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public class ForecastItem
{
    public string Period { get; set; } = string.Empty;
    public double Value { get; set; }
    public double PreviousValue { get; set; }
    public bool IsForecast { get; set; }
    
    // Max 200px height, scale based on value
    public double BarHeight => Math.Max(Value > 0 ? Math.Min(Value / 100, 180) : 20, 15);
    public double PrevBarHeight => Math.Max(PreviousValue > 0 ? Math.Min(PreviousValue / 100, 180) : 15, 10);
    
    // Eski uyumluluk için
    public double HeightFactor => BarHeight / 200.0;
    public double PrevHeightFactor => PrevBarHeight / 200.0;
    
    // Formatlanmış değer
    public string FormattedValue => Value > 1000 ? $"₺{Value/1000:F1}K" : $"₺{Value:F0}";
}


public class FinancialTransaction
{
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPositive => Amount > 0;
    public string FormattedAmount => (Amount > 0 ? "+" : "") + Amount.ToString("C2");
}

public class AiInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.Lightbulb;
    public string ColorClass { get; set; } = "Blue";
}

public class BudgetItem
{
    public string Category { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal SuggestedAmount { get; set; }
    public string SavingTip { get; set; } = string.Empty;
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.Cash;
    
    public decimal SavingAmount => CurrentAmount - SuggestedAmount;
    public double SavingPercent => CurrentAmount > 0 ? (double)(SavingAmount / CurrentAmount * 100) : 0;
    public double ProgressPercent => CurrentAmount > 0 ? Math.Min((double)(SuggestedAmount / CurrentAmount) * 100, 100) : 0;
}

public class RevenueDistributionDisplay
{
    private static readonly string[] CategoryColors = { "#11b4d4", "#22c55e", "#f59e0b", "#8b5cf6", "#ec4899" };
    
    public string Category { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public int ColorIndex { get; set; }
    
    public string ColorHex => CategoryColors[ColorIndex % CategoryColors.Length];
    public Avalonia.Media.IBrush BarBrush => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(ColorHex));
    public string PercentageText => $"%{Percentage:F0}";
    public double BarWidth => Math.Max(Percentage * 3, 10); // Min 10px, max ~300px
    public string AmountText { get; set; } = string.Empty;
}


public partial class FinanceViewModel : ObservableObject
{
    // KPIs
    [ObservableProperty] private string _totalRevenue = "₺0";
    [ObservableProperty] private string _totalExpense = "₺0";
    [ObservableProperty] private string _netProfit = "₺0";
    [ObservableProperty] private int _aiScore = 0;
    [ObservableProperty] private bool _isLoading;
    
    // AI Analysis Results
    [ObservableProperty] private string _aiForecast = "Analiz bekleniyor...";
    [ObservableProperty] private string _aiRecommendations = string.Empty;

    // Collections
    public ObservableCollection<ForecastItem> ForecastData { get; } = new();
    public ObservableCollection<FinancialTransaction> Transactions { get; } = new();
    public ObservableCollection<AiInsight> AiInsights { get; } = new();
    public ObservableCollection<BudgetItem> BudgetItems { get; } = new();
    public ObservableCollection<RevenueDistributionDisplay> RevenueDistribution { get; } = new();


    public FinanceViewModel()
    {
        // Register for finance data change messages
        WeakReferenceMessenger.Default.Register<Models.FinanceDataChangedMessage>(this, (r, m) =>
        {
            // Refresh finance data when notified
            _ = LoadDataAsync();
        });
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load financial summary
            var summary = await DatabaseService.Instance.GetFinancialSummaryAsync();
            TotalRevenue = $"₺{summary.TotalRevenue:N0}";
            TotalExpense = $"₺{summary.TotalExpense:N0}";
            NetProfit = $"₺{summary.NetProfit:N0}";
            
            // Get advanced analytics data
            var expenseBreakdown = await DatabaseService.Instance.GetExpenseBreakdownAsync();
            var revenueMonths = await DatabaseService.Instance.GetMonthlyRevenueAsync();
            var expenseMonths = await DatabaseService.Instance.GetMonthlyExpenseAsync();
            var occupancyRate = await DatabaseService.Instance.GetCurrentOccupancyRateAsync();
            
            // Calculate trends
            var revenueChange = revenueMonths.LastMonth > 0 
                ? (double)((revenueMonths.CurrentMonth - revenueMonths.LastMonth) / revenueMonths.LastMonth * 100)
                : 0;
            var expenseChange = expenseMonths.LastMonth > 0
                ? (double)((expenseMonths.CurrentMonth - expenseMonths.LastMonth) / expenseMonths.LastMonth * 100)
                : 0;
            
            // Find highest expense category
            var highestExpense = expenseBreakdown.OrderByDescending(x => x.Value).FirstOrDefault();
            
            // 🏨 COLLECT COMPREHENSIVE HOTEL DATA FROM ALL TABLES
            var roomStats = await DatabaseService.Instance.GetRoomStatsAsync();
            var allCustomers = await DatabaseService.Instance.GetAllCustomersAsync();
            var allReservations = await DatabaseService.Instance.GetAllReservationsAsync();
            var allPersonnel = await DatabaseService.Instance.GetAllPersonnelAsync();
            
            // Prepare comprehensive hotel data for Gemini
            var comprehensiveData = new Services.ComprehensiveHotelData
            {
                Financial = new Models.FinancialDataDto
                {
                    TotalRevenue = summary.TotalRevenue,
                    TotalExpense = summary.TotalExpense,
                    HighestExpenseCategory = highestExpense.Key ?? "Genel",
                    HighestExpenseAmount = highestExpense.Value,
                    RevenueChangePercent = revenueChange,
                    ExpenseChangePercent = expenseChange,
                    OccupancyRate = occupancyRate,
                    ExpenseBreakdown = expenseBreakdown
                },
                
                // ODA VERİLERİ
                TotalRooms = roomStats.Total,
                AvailableRooms = roomStats.Available,
                OccupiedRooms = roomStats.Occupied,
                CleaningRooms = roomStats.Cleaning,
                MaintenanceRooms = roomStats.Maintenance,
                
                // REZERVASYON VERİLERİ  
                ActiveReservations = allReservations.Count(r => r.Status == Models.ReservationStatus.CheckedIn),
                PendingReservations = allReservations.Count(r => r.Status == Models.ReservationStatus.Pending),
                CompletedReservations = allReservations.Count(r => r.Status == Models.ReservationStatus.CheckedOut),
                CancelledReservations = 0, // Model'de cancel yok, default 0
                
                // MÜŞTERİ VERİLERİ
                TotalCustomers = allCustomers.Count,
                ActiveCustomers = allReservations.Count(r => r.Status == Models.ReservationStatus.CheckedIn),
                
                // PERSONEL VERİLERİ
                TotalPersonnel = allPersonnel.Count,
                PersonnelByDepartment = string.Join(", ", allPersonnel.GroupBy(p => p.Department).Select(g => $"{g.Key}: {g.Count()}"))
            };
            
            // 🤖 GEMINI AI COMPREHENSIVE ANALYSIS
            var geminiResult = await GeminiAiService.Instance.AnalyzeComprehensiveHotelDataAsync(comprehensiveData);
            
            // Update AI properties from Gemini
            AiScore = geminiResult.Score;
            AiForecast = geminiResult.Forecast;
            AiRecommendations = string.Join("\n", geminiResult.Recommendations);
            
            // Populate AI Insights collection for UI
            AiInsights.Clear();
            
            if (geminiResult.Recommendations.Any())
            {
                foreach (var recommendation in geminiResult.Recommendations.Take(5))
                {
                    var icon = GetIconForRecommendation(recommendation);
                    AiInsights.Add(new AiInsight
                    {
                        Title = recommendation.Length > 50 ? recommendation.Substring(0, 47) + "..." : recommendation,
                        Description = recommendation,
                        Icon = icon,
                        ColorClass = GetColorForRecommendation(recommendation)
                    });
                }
            }
            else
            {
                AiInsights.Add(new AiInsight
                {
                    Title = "AI Analizi Bekleniyor",
                    Description = "Gemini AI analiz sonuçlarını hazırlıyor...",
                    Icon = MaterialIconKind.Robot,
                    ColorClass = "Blue"
                });
            }
            
            // 📊 POPULATE WEEKLY FORECAST DATA FROM GEMINI
            ForecastData.Clear();
            var days = new[] { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };
            
            if (geminiResult.WeeklyForecast.Any())
            {
                // Geçmiş hafta verileri (simüle)
                var dailyAvg = summary.TotalRevenue / 30m; // Aylık ortalamanın günlük karşılığı
                for (int i = 0; i < 7; i++)
                {
                    var dayIndex = ((int)DateTime.Now.DayOfWeek - 7 + i + 7) % 7;
                    var variance = 0.8 + (i * 0.05);
                    ForecastData.Add(new ForecastItem
                    {
                        Period = days[dayIndex],
                        Value = (double)dailyAvg * variance,
                        PreviousValue = (double)dailyAvg * (variance - 0.1),
                        IsForecast = false
                    });
                }
                
                // Gelecek hafta - Gemini tahminleri
                foreach (var forecast in geminiResult.WeeklyForecast.Take(7))
                {
                    ForecastData.Add(new ForecastItem
                    {
                        Period = forecast.Day.Length > 3 ? forecast.Day.Substring(0, 3) : forecast.Day,
                        Value = forecast.Revenue,
                        PreviousValue = forecast.Revenue * 0.85,
                        IsForecast = true
                    });
                }
            }
            else
            {
                // Fallback: Basit haftalık forecast
                LoadWeeklyForecastFallback(summary.TotalRevenue > 0 ? summary.TotalRevenue : 100000m);
            }
            
            // 📊 POPULATE REVENUE DISTRIBUTION FROM GEMINI
            RevenueDistribution.Clear();
            
            if (geminiResult.RevenueDistribution.Any())
            {
                int colorIndex = 0;
                foreach (var dist in geminiResult.RevenueDistribution)
                {
                    RevenueDistribution.Add(new RevenueDistributionDisplay
                    {
                        Category = dist.Category,
                        Percentage = dist.Percentage,
                        ColorIndex = colorIndex,
                        AmountText = $"₺{(summary.TotalRevenue * (decimal)dist.Percentage / 100):N0}"
                    });
                    colorIndex++;
                }
            }
            else
            {
                // Fallback: Varsayılan dağılım
                RevenueDistribution.Add(new RevenueDistributionDisplay { Category = "Oda Geliri", Percentage = 65, ColorIndex = 0 });
                RevenueDistribution.Add(new RevenueDistributionDisplay { Category = "Yiyecek-İçecek", Percentage = 20, ColorIndex = 1 });
                RevenueDistribution.Add(new RevenueDistributionDisplay { Category = "Ekstra Hizmetler", Percentage = 10, ColorIndex = 2 });
                RevenueDistribution.Add(new RevenueDistributionDisplay { Category = "Diğer", Percentage = 5, ColorIndex = 3 });
            }
            
            // 💰 POPULATE BUDGET INSIGHTS FROM GEMINI
            BudgetItems.Clear();
            if (geminiResult.BudgetInsights.Any())
            {
                foreach (var insight in geminiResult.BudgetInsights)
                {
                    BudgetItems.Add(new BudgetItem
                    {
                        Category = insight.Category,
                        CurrentAmount = (decimal)insight.Current,
                        SuggestedAmount = (decimal)insight.Suggested,
                        SavingTip = insight.SavingTip,
                        Icon = GetIconForCategory(insight.Category)
                    });
                }
            }
            else
            {
                // Fallback bütçe önerileri
                BudgetItems.Add(new BudgetItem
                {
                    Category = "Genel",
                    CurrentAmount = summary.TotalExpense,
                    SuggestedAmount = summary.TotalExpense * 0.9m,
                    SavingTip = "Maliyet optimizasyonu yapın",
                    Icon = MaterialIconKind.Cash
                });
            }
            
            // Load transactions
            Transactions.Clear();
            var transactions = await DatabaseService.Instance.GetAllTransactionsAsync();
            foreach (var t in transactions)
            {
                Transactions.Add(t);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading finance data: {ex.Message}");
            AiForecast = "Analiz yapılırken hata oluştu.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadWeeklyForecastFallback(decimal currentRevenue)
    {
        ForecastData.Clear();
        
        var days = new[] { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };
        var dailyAvg = (double)(currentRevenue / 30m);
        
        // Geçmiş 7 gün
        for (int i = 0; i < 7; i++)
        {
            var variance = 0.8 + (i * 0.04);
            ForecastData.Add(new ForecastItem
            {
                Period = days[i],
                Value = dailyAvg * variance,
                PreviousValue = dailyAvg * (variance - 0.1),
                IsForecast = false
            });
        }
        
        // Gelecek 7 gün (tahmin)
        for (int i = 0; i < 7; i++)
        {
            var growthFactor = 1.0 + (i * 0.02);
            ForecastData.Add(new ForecastItem
            {
                Period = days[i],
                Value = dailyAvg * growthFactor,
                PreviousValue = dailyAvg * (growthFactor - 0.15),
                IsForecast = true
            });
        }
    }


    private MaterialIconKind GetIconForRecommendation(string recommendation)
    {
        if (recommendation.Contains("ALARM") || recommendation.Contains("KRİTİK") || recommendation.Contains("acil", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.Alert;
        if (recommendation.Contains("ENERJI") || recommendation.Contains("⚡") || recommendation.Contains("enerji", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.Lightbulb;
        if (recommendation.Contains("PERSONEL") || recommendation.Contains("👥") || recommendation.Contains("personel", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.Account;
        if (recommendation.Contains("DOLULUK") || recommendation.Contains("🏨") || recommendation.Contains("doluluk", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.Hotel;
        if (recommendation.Contains("TREND") || recommendation.Contains("📈") || recommendation.Contains("gelir", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.TrendingUp;
        if (recommendation.Contains("FIRSATLAR") || recommendation.Contains("YATIRIM") || recommendation.Contains("yatırım", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.Cash;
        if (recommendation.Contains("pazarlama", StringComparison.OrdinalIgnoreCase) || recommendation.Contains("kampanya", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.Bullhorn;
        if (recommendation.Contains("müşteri", StringComparison.OrdinalIgnoreCase))
            return MaterialIconKind.AccountGroup;
        
        return MaterialIconKind.Star;
    }
    
    private string GetColorForRecommendation(string recommendation)
    {
        if (recommendation.Contains("🔴") || recommendation.Contains("ALARM") || recommendation.Contains("acil", StringComparison.OrdinalIgnoreCase))
            return "Red";
        if (recommendation.Contains("🟠") || recommendation.Contains("UYARI") || recommendation.Contains("dikkat", StringComparison.OrdinalIgnoreCase))
            return "Orange";
        if (recommendation.Contains("🟡"))
            return "Yellow";
        if (recommendation.Contains("🟢") || recommendation.Contains("FIRSATLAR") || recommendation.Contains("olumlu", StringComparison.OrdinalIgnoreCase))
            return "Green";
        
        return "Blue";
    }
    
    private MaterialIconKind GetIconForCategory(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "enerji" or "elektrik" => MaterialIconKind.Lightbulb,
            "personel" or "maaş" => MaterialIconKind.Account,
            "temizlik" => MaterialIconKind.Broom,
            "yiyecek" or "içecek" or "mutfak" => MaterialIconKind.Food,
            "bakım" or "tamir" => MaterialIconKind.Wrench,
            "pazarlama" or "reklam" => MaterialIconKind.Bullhorn,
            _ => MaterialIconKind.Cash
        };
    }


    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private void DownloadReport()
    {
        // TODO: Implement Download Logic
    }

    [RelayCommand]
    private void Filter()
    {
        // TODO: Implement Filter Logic
    }
}
