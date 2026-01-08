using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OtelV3.Models;
using OtelV3.ViewModels;

namespace OtelV3.Services;

public class DatabaseService
{
    private static DatabaseService? _instance;
    private static readonly object _lock = new object();
    private readonly string _connectionString;

    private DatabaseService()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public static DatabaseService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DatabaseService();
                }
            }
            return _instance;
        }
    }

    private SqlConnection GetConnection() => new SqlConnection(_connectionString);

    #region Odalar (Rooms)

    public async Task<List<RoomViewModel>> GetAllRoomsAsync()
    {
        var rooms = new List<RoomViewModel>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        // Get rooms with current guest info if occupied
        var query = @"SELECT o.OdaNo, o.OdaTipi, o.Kapasite, o.Ozellikler, o.Alan, o.Fiyat, o.Durum,
                      (SELECT TOP 1 m.AdSoyad FROM Tbl_Rezervasyonlar r 
                       JOIN Tbl_Musteriler m ON r.MusteriId = m.Id 
                       WHERE r.OdaNo = o.OdaNo AND r.Durum = 1 
                       ORDER BY r.GirisTarihi DESC) as MisafirAdi
                      FROM Tbl_Odalar o";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            rooms.Add(new RoomViewModel
            {
                RoomNumber = reader["OdaNo"]?.ToString() ?? string.Empty,
                RoomType = reader["OdaTipi"]?.ToString() ?? string.Empty,
                Capacity = reader["Kapasite"]?.ToString() + " Kişi" ?? string.Empty,
                Features = reader["Ozellikler"]?.ToString() ?? string.Empty,
                Area = reader["Alan"]?.ToString() ?? string.Empty,
                Price = reader["Fiyat"] != DBNull.Value ? Convert.ToDecimal(reader["Fiyat"]) : 0,
                Status = (RoomStatus)(reader["Durum"] != DBNull.Value ? Convert.ToInt32(reader["Durum"]) : 1),
                GuestName = reader["MisafirAdi"]?.ToString()
            });
        }
        
        return rooms;
    }

    public async Task<(int Total, int Available, int Occupied, int Cleaning, int Maintenance)> GetRoomStatsAsync()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"
            SELECT 
                COUNT(*) as Total,
                SUM(CASE WHEN Durum = 1 THEN 1 ELSE 0 END) as Available,
                SUM(CASE WHEN Durum = 0 THEN 1 ELSE 0 END) as Occupied,
                SUM(CASE WHEN Durum = 2 THEN 1 ELSE 0 END) as Cleaning,
                SUM(CASE WHEN Durum = 3 THEN 1 ELSE 0 END) as Maintenance
            FROM Tbl_Odalar";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToInt32(reader["Total"]),
                Convert.ToInt32(reader["Available"]),
                Convert.ToInt32(reader["Occupied"]),
                Convert.ToInt32(reader["Cleaning"]),
                Convert.ToInt32(reader["Maintenance"])
            );
        }
        
        return (0, 0, 0, 0, 0);
    }

    public async Task<bool> AddRoomAsync(string roomNumber, string roomType, int capacity, string features, string area, decimal price, int status)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Tbl_Odalar (OdaNo, OdaTipi, Kapasite, Ozellikler, Alan, Fiyat, Durum) 
                      VALUES (@OdaNo, @OdaTipi, @Kapasite, @Ozellikler, @Alan, @Fiyat, @Durum)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OdaNo", roomNumber);
        command.Parameters.AddWithValue("@OdaTipi", roomType);
        command.Parameters.AddWithValue("@Kapasite", capacity);
        command.Parameters.AddWithValue("@Ozellikler", features);
        command.Parameters.AddWithValue("@Alan", area);
        command.Parameters.AddWithValue("@Fiyat", price);
        command.Parameters.AddWithValue("@Durum", status);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> UpdateRoomStatusAsync(string roomNumber, int status)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = "UPDATE Tbl_Odalar SET Durum = @Durum WHERE OdaNo = @OdaNo";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OdaNo", roomNumber);
        command.Parameters.AddWithValue("@Durum", status);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> UpdateRoomDetailsAsync(string roomNumber, string roomType, int capacity, string features, string area, decimal price, int status)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"UPDATE Tbl_Odalar 
                      SET OdaTipi = @OdaTipi, 
                          Kapasite = @Kapasite, 
                          Ozellikler = @Ozellikler, 
                          Alan = @Alan, 
                          Fiyat = @Fiyat, 
                          Durum = @Durum 
                      WHERE OdaNo = @OdaNo";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OdaNo", roomNumber);
        command.Parameters.AddWithValue("@OdaTipi", roomType);
        command.Parameters.AddWithValue("@Kapasite", capacity);
        command.Parameters.AddWithValue("@Ozellikler", features);
        command.Parameters.AddWithValue("@Alan", area);
        command.Parameters.AddWithValue("@Fiyat", price);
        command.Parameters.AddWithValue("@Durum", status);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> DeleteRoomAsync(string roomNumber)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = "DELETE FROM Tbl_Odalar WHERE OdaNo = @OdaNo";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OdaNo", roomNumber);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    #endregion

    #region Müşteriler (Customers)

    public async Task<List<CustomerModel>> GetAllCustomersAsync()
    {
        var customers = new List<CustomerModel>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT Id, AdSoyad, Telefon, Email, TCNo, SonZiyaretTarihi, 
                      SonZiyaretDurumu, VIP, KaraListe, ZiyaretSayisi, AvatarYolu 
                      FROM Tbl_Musteriler
                      ORDER BY Id DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            customers.Add(new CustomerModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["AdSoyad"]?.ToString() ?? string.Empty,
                Phone = reader["Telefon"]?.ToString() ?? string.Empty,
                Email = reader["Email"]?.ToString() ?? string.Empty,
                IdentityNumber = reader["TCNo"]?.ToString() ?? string.Empty,
                LastVisitDate = reader["SonZiyaretTarihi"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["SonZiyaretTarihi"]) 
                    : null,
                LastVisitStatus = reader["SonZiyaretDurumu"]?.ToString() ?? string.Empty,
                IsVip = reader["VIP"] != DBNull.Value && Convert.ToBoolean(reader["VIP"]),
                IsBlacklisted = reader["KaraListe"] != DBNull.Value && Convert.ToBoolean(reader["KaraListe"]),
                VisitCount = reader["ZiyaretSayisi"] != DBNull.Value ? Convert.ToInt32(reader["ZiyaretSayisi"]) : 0,
                AvatarUrl = reader["AvatarYolu"]?.ToString()
            });
        }
        
        return customers;
    }

    public async Task<bool> AddCustomerAsync(string name, string phone, string email, string identityNumber, 
        bool isVip = false, bool isBlacklisted = false)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Tbl_Musteriler (AdSoyad, Telefon, Email, TCNo, VIP, KaraListe, ZiyaretSayisi) 
                      VALUES (@AdSoyad, @Telefon, @Email, @TCNo, @VIP, @KaraListe, 0)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AdSoyad", name);
        command.Parameters.AddWithValue("@Telefon", phone);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@TCNo", identityNumber);
        command.Parameters.AddWithValue("@VIP", isVip);
        command.Parameters.AddWithValue("@KaraListe", isBlacklisted);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = "DELETE FROM Tbl_Musteriler WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", customerId);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> UpdateCustomerStatusAsync(int customerId, bool isVip, bool isBlacklisted)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"UPDATE Tbl_Musteriler 
                      SET VIP = @VIP, KaraListe = @KaraListe 
                      WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", customerId);
        command.Parameters.AddWithValue("@VIP", isVip);
        command.Parameters.AddWithValue("@KaraListe", isBlacklisted);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    #endregion

    #region Personel (Personnel)

    public async Task<List<PersonnelModel>> GetAllPersonnelAsync()
    {
        var personnel = new List<PersonnelModel>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT Id, AdSoyad, TCNo, Pozisyon, Departman, Email, Telefon, 
                      IseBaslamaTarihi, Maas, Durum, AvatarYolu 
                      FROM Tbl_Personeller
                      ORDER BY Id DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            personnel.Add(new PersonnelModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                FullName = reader["AdSoyad"]?.ToString() ?? string.Empty,
                TcNo = reader["TCNo"]?.ToString() ?? string.Empty,
                Position = reader["Pozisyon"]?.ToString() ?? string.Empty,
                Department = reader["Departman"]?.ToString() ?? string.Empty,
                Email = reader["Email"]?.ToString() ?? string.Empty,
                Phone = reader["Telefon"]?.ToString() ?? string.Empty,
                StartDate = reader["IseBaslamaTarihi"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["IseBaslamaTarihi"]) 
                    : DateTime.MinValue,
                Salary = reader["Maas"] != DBNull.Value ? Convert.ToDecimal(reader["Maas"]) : 0,
                Status = (PersonnelStatus)(reader["Durum"] != DBNull.Value ? Convert.ToInt32(reader["Durum"]) : 0),
                AvatarUrl = reader["AvatarYolu"]?.ToString()
            });
        }
        
        return personnel;
    }

    public async Task<bool> AddPersonnelAsync(string fullName, string tcNo, string position, string department,
        string email, string phone, DateTime startDate, decimal salary, int status)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Tbl_Personeller (AdSoyad, TCNo, Pozisyon, Departman, Email, Telefon, 
                      IseBaslamaTarihi, Maas, Durum) 
                      VALUES (@AdSoyad, @TCNo, @Pozisyon, @Departman, @Email, @Telefon, 
                      @IseBaslamaTarihi, @Maas, @Durum)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AdSoyad", fullName);
        command.Parameters.AddWithValue("@TCNo", tcNo);
        command.Parameters.AddWithValue("@Pozisyon", position);
        command.Parameters.AddWithValue("@Departman", department);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Telefon", phone);
        command.Parameters.AddWithValue("@IseBaslamaTarihi", startDate);
        command.Parameters.AddWithValue("@Maas", salary);
        command.Parameters.AddWithValue("@Durum", status);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> DeletePersonnelAsync(int personnelId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = "DELETE FROM Tbl_Personeller WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", personnelId);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    #endregion

    #region Rezervasyonlar (Reservations)

    public async Task<List<ReservationModel>> GetAllReservationsAsync()
    {
        var reservations = new List<ReservationModel>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT r.Id, m.AdSoyad as MusteriAdi, m.Telefon, r.OdaNo, 
                      o.OdaTipi, r.GirisTarihi, r.CikisTarihi, r.Tutar, r.Durum, r.KisiSayisi
                      FROM Tbl_Rezervasyonlar r
                      LEFT JOIN Tbl_Musteriler m ON r.MusteriId = m.Id
                      LEFT JOIN Tbl_Odalar o ON r.OdaNo = o.OdaNo
                      ORDER BY r.Id DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var guestName = reader["MusteriAdi"]?.ToString() ?? string.Empty;
            reservations.Add(new ReservationModel
            {
                Id = "#RZ-" + reader["Id"]?.ToString(),
                GuestName = guestName,
                GuestContact = reader["Telefon"]?.ToString() ?? string.Empty,
                GuestInitials = GetInitials(guestName),
                RoomNumber = reader["OdaNo"]?.ToString() ?? string.Empty,
                RoomType = reader["OdaTipi"]?.ToString() ?? string.Empty,
                CheckInDate = reader["GirisTarihi"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["GirisTarihi"]) 
                    : DateTime.MinValue,
                CheckOutDate = reader["CikisTarihi"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["CikisTarihi"]) 
                    : DateTime.MinValue,
                Amount = reader["Tutar"] != DBNull.Value ? Convert.ToDecimal(reader["Tutar"]) : 0,
                Status = (ReservationStatus)(reader["Durum"] != DBNull.Value ? Convert.ToInt32(reader["Durum"]) : 0),
                GuestCount = reader["KisiSayisi"]?.ToString() + " Kişi" ?? string.Empty
            });
        }
        
        return reservations;
    }

    public async Task<bool> AddReservationAsync(int customerId, string roomNumber, DateTime checkIn, 
        DateTime checkOut, decimal amount, int status, int guestCount)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Tbl_Rezervasyonlar (MusteriId, OdaNo, GirisTarihi, CikisTarihi, Tutar, Durum, KisiSayisi) 
                      VALUES (@MusteriId, @OdaNo, @GirisTarihi, @CikisTarihi, @Tutar, @Durum, @KisiSayisi)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MusteriId", customerId);
        command.Parameters.AddWithValue("@OdaNo", roomNumber);
        command.Parameters.AddWithValue("@GirisTarihi", checkIn);
        command.Parameters.AddWithValue("@CikisTarihi", checkOut);
        command.Parameters.AddWithValue("@Tutar", amount);
        command.Parameters.AddWithValue("@Durum", status);
        command.Parameters.AddWithValue("@KisiSayisi", guestCount);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<List<(string RoomInfo, string RoomNumber, decimal Price)>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        var rooms = new List<(string RoomInfo, string RoomNumber, decimal Price)>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT OdaNo, OdaTipi, Fiyat FROM Tbl_Odalar 
                      WHERE Durum = 1 
                      AND OdaNo NOT IN (
                          SELECT OdaNo FROM Tbl_Rezervasyonlar 
                          WHERE (GirisTarihi <= @CikisTarihi AND CikisTarihi >= @GirisTarihi)
                          AND Durum NOT IN (3) -- Exclude checked out
                      )";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@GirisTarihi", checkIn);
        command.Parameters.AddWithValue("@CikisTarihi", checkOut);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var roomNo = reader["OdaNo"]?.ToString() ?? string.Empty;
            var roomType = reader["OdaTipi"]?.ToString() ?? string.Empty;
            var price = reader["Fiyat"] != DBNull.Value ? Convert.ToDecimal(reader["Fiyat"]) : 0;
            rooms.Add(($"{roomNo} - {roomType} ({price:N0} ₺/gece)", roomNo, price));
        }
        
        return rooms;
    }

    #endregion

    #region Finansal İşlemler (Transactions)

    public async Task<List<FinancialTransaction>> GetAllTransactionsAsync()
    {
        var transactions = new List<FinancialTransaction>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = "SELECT Id, Tarih, Aciklama, Kategori, Tutar, Durum FROM Tbl_Islemler ORDER BY Tarih DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var date = reader["Tarih"] != DBNull.Value 
                ? Convert.ToDateTime(reader["Tarih"]) 
                : DateTime.MinValue;
                
            transactions.Add(new FinancialTransaction
            {
                Date = date.ToString("dd MMM yyyy, HH:mm"),
                Description = reader["Aciklama"]?.ToString() ?? string.Empty,
                Category = reader["Kategori"]?.ToString() ?? string.Empty,
                Amount = reader["Tutar"] != DBNull.Value ? Convert.ToDecimal(reader["Tutar"]) : 0,
                Status = reader["Durum"]?.ToString() ?? string.Empty
            });
        }
        
        return transactions;
    }

    public async Task<(decimal TotalRevenue, decimal TotalExpense, decimal NetProfit)> GetFinancialSummaryAsync()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT 
                      ISNULL(SUM(CASE WHEN Tutar > 0 THEN Tutar ELSE 0 END), 0) as TotalRevenue,
                      ISNULL(SUM(CASE WHEN Tutar < 0 THEN ABS(Tutar) ELSE 0 END), 0) as TotalExpense
                      FROM Tbl_Islemler";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            var revenue = Convert.ToDecimal(reader["TotalRevenue"]);
            var expense = Convert.ToDecimal(reader["TotalExpense"]);
            return (revenue, expense, revenue - expense);
        }
        
        return (0, 0, 0);
    }

    public async Task<bool> AddTransactionAsync(string description, string category, decimal amount, string status)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Tbl_Islemler (Tarih, Aciklama, Kategori, Tutar, Durum) 
                      VALUES (@Tarih, @Aciklama, @Kategori, @Tutar, @Durum)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Tarih", DateTime.Now);
        command.Parameters.AddWithValue("@Aciklama", description);
        command.Parameters.AddWithValue("@Kategori", category);
        command.Parameters.AddWithValue("@Tutar", amount);
        command.Parameters.AddWithValue("@Durum", status);
        
        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    #endregion

    #region Gelişmiş Finansal Analiz (AI için)

    /// <summary>
    /// Kategori bazlı gider analizi
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetExpenseBreakdownAsync()
    {
        var breakdown = new Dictionary<string, decimal>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT Kategori, SUM(ABS(Tutar)) as Total 
                      FROM Tbl_Islemler 
                      WHERE Tutar < 0 
                      GROUP BY Kategori";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var category = reader["Kategori"]?.ToString() ?? "Diğer";
            var total = Convert.ToDecimal(reader["Total"]);
            breakdown[category] = total;
        }
        
        return breakdown;
    }

    /// <summary>
    /// Aylık gelir trendi - Son 3 ay
    /// </summary>
    public async Task<(decimal CurrentMonth, decimal LastMonth, decimal TwoMonthsAgo)> GetMonthlyRevenueAsync()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT 
                        SUM(CASE WHEN MONTH(Tarih) = MONTH(GETDATE()) AND YEAR(Tarih) = YEAR(GETDATE()) 
                            AND Tutar > 0 THEN Tutar ELSE 0 END) as CurrentMonth,
                        SUM(CASE WHEN MONTH(Tarih) = MONTH(DATEADD(MONTH, -1, GETDATE())) AND YEAR(Tarih) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
                            AND Tutar > 0 THEN Tutar ELSE 0 END) as LastMonth,
                        SUM(CASE WHEN MONTH(Tarih) = MONTH(DATEADD(MONTH, -2, GETDATE())) AND YEAR(Tarih) = YEAR(DATEADD(MONTH, -2, GETDATE())) 
                            AND Tutar > 0 THEN Tutar ELSE 0 END) as TwoMonthsAgo
                      FROM Tbl_Islemler";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToDecimal(reader["CurrentMonth"]),
                Convert.ToDecimal(reader["LastMonth"]),
                Convert.ToDecimal(reader["TwoMonthsAgo"])
            );
        }
        
        return (0, 0, 0);
    }

    /// <summary>
    /// Aylık gider trendi - Son 3 ay
    /// </summary>
    public async Task<(decimal CurrentMonth, decimal LastMonth, decimal TwoMonthsAgo)> GetMonthlyExpenseAsync()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = @"SELECT 
                        SUM(CASE WHEN MONTH(Tarih) = MONTH(GETDATE()) AND YEAR(Tarih) = YEAR(GETDATE()) 
                            AND Tutar < 0 THEN ABS(Tutar) ELSE 0 END) as CurrentMonth,
                        SUM(CASE WHEN MONTH(Tarih) = MONTH(DATEADD(MONTH, -1, GETDATE())) AND YEAR(Tarih) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
                            AND Tutar < 0 THEN ABS(Tutar) ELSE 0 END) as LastMonth,
                        SUM(CASE WHEN MONTH(Tarih) = MONTH(DATEADD(MONTH, -2, GETDATE())) AND YEAR(Tarih) = YEAR(DATEADD(MONTH, -2, GETDATE())) 
                            AND Tutar < 0 THEN ABS(Tutar) ELSE 0 END) as TwoMonthsAgo
                      FROM Tbl_Islemler";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToDecimal(reader["CurrentMonth"]),
                Convert.ToDecimal(reader["LastMonth"]),
                Convert.ToDecimal(reader["TwoMonthsAgo"])
            );
        }
        
        return (0, 0, 0);
    }

    /// <summary>
    /// Doluluk oranı - AI korelasyonu için
    /// </summary>
    public async Task<double> GetCurrentOccupancyRateAsync()
    {
        var stats = await GetRoomStatsAsync();
        if (stats.Total == 0) return 0;
        
        return (double)stats.Occupied / stats.Total * 100;
    }

    #endregion

    #region Dashboard İstatistikleri

    public async Task<(int TodayCheckIn, int TodayCheckOut, int CleaningRooms, double OccupancyRate)> GetDashboardStatsAsync()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();

        var today = DateTime.Today;
        
        // Today's check-ins
        var checkInQuery = "SELECT COUNT(*) FROM Tbl_Rezervasyonlar WHERE CAST(GirisTarihi AS DATE) = @Today";
        using var checkInCmd = new SqlCommand(checkInQuery, connection);
        checkInCmd.Parameters.AddWithValue("@Today", today);
        var todayCheckIn = Convert.ToInt32(await checkInCmd.ExecuteScalarAsync());
        
        // Today's check-outs
        var checkOutQuery = "SELECT COUNT(*) FROM Tbl_Rezervasyonlar WHERE CAST(CikisTarihi AS DATE) = @Today";
        using var checkOutCmd = new SqlCommand(checkOutQuery, connection);
        checkOutCmd.Parameters.AddWithValue("@Today", today);
        var todayCheckOut = Convert.ToInt32(await checkOutCmd.ExecuteScalarAsync());
        
        // Room stats
        var roomStats = await GetRoomStatsAsync();
        var cleaningRooms = roomStats.Cleaning;
        var occupancyRate = roomStats.Total > 0 
            ? (double)roomStats.Occupied / roomStats.Total * 100 
            : 0;
        
        return (todayCheckIn, todayCheckOut, cleaningRooms, occupancyRate);
    }

    public async Task<List<Reservation>> GetRecentReservationsAsync(int count = 5)
    {
        var reservations = new List<Reservation>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = $@"SELECT TOP {count} r.Id, m.AdSoyad as MusteriAdi, m.Email,  
                       o.OdaTipi, o.OdaNo, r.GirisTarihi, r.CikisTarihi, r.Tutar, r.Durum
                       FROM Tbl_Rezervasyonlar r
                       LEFT JOIN Tbl_Musteriler m ON r.MusteriId = m.Id
                       LEFT JOIN Tbl_Odalar o ON r.OdaNo = o.OdaNo
                       ORDER BY r.Id DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var guestName = reader["MusteriAdi"]?.ToString() ?? string.Empty;
            var checkIn = reader["GirisTarihi"] != DBNull.Value 
                ? Convert.ToDateTime(reader["GirisTarihi"]) 
                : DateTime.MinValue;
            var checkOut = reader["CikisTarihi"] != DBNull.Value 
                ? Convert.ToDateTime(reader["CikisTarihi"]) 
                : DateTime.MinValue;
                
            reservations.Add(new Reservation
            {
                ReservationId = "#RZ-" + reader["Id"]?.ToString(),
                GuestName = guestName,
                GuestEmail = reader["Email"]?.ToString() ?? string.Empty,
                GuestInitials = GetInitials(guestName),
                RoomType = $"{reader["OdaTipi"]} ({reader["OdaNo"]})",
                CheckInDate = checkIn.ToString("dd MMM yyyy"),
                CheckOutDate = checkOut.ToString("dd MMM yyyy"),
                Amount = "₺" + (reader["Tutar"] != DBNull.Value ? Convert.ToDecimal(reader["Tutar"]).ToString("N0") : "0"),
                Status = (ReservationStatus)(reader["Durum"] != DBNull.Value ? Convert.ToInt32(reader["Durum"]) : 0)
            });
        }
        
        return reservations;
    }

    public async Task<List<Room>> GetRoomsForDashboardAsync()
    {
        var rooms = new List<Room>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        // Get rooms with current guest info if occupied
        var query = @"SELECT o.OdaNo, o.OdaTipi, o.Durum, 
                      (SELECT TOP 1 m.AdSoyad FROM Tbl_Rezervasyonlar r 
                       JOIN Tbl_Musteriler m ON r.MusteriId = m.Id 
                       WHERE r.OdaNo = o.OdaNo AND r.Durum = 1 
                       ORDER BY r.GirisTarihi DESC) as MisafirAdi
                      FROM Tbl_Odalar o";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            rooms.Add(new Room
            {
                RoomNumber = reader["OdaNo"]?.ToString() ?? string.Empty,
                RoomType = reader["OdaTipi"]?.ToString() ?? string.Empty,
                Status = (RoomStatus)(reader["Durum"] != DBNull.Value ? Convert.ToInt32(reader["Durum"]) : 1),
                GuestName = reader["MisafirAdi"]?.ToString()
            });
        }
        
        return rooms;
    }

    #endregion

    #region Check-Out İşlemleri

    public async Task<List<GuestCheckoutModel>> GetCheckoutListAsync()
    {
        var checkoutList = new List<GuestCheckoutModel>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        // Durum = 1 (Giriş Yapmış/Aktif) VEYA (Durum=3 Ve Çıkış=Bugün)
        // NOT: Enum ReservationStatus { Confirmed=0, CheckedIn=1, Pending=2, CheckedOut=3 }
        var query = @"SELECT r.Id, r.OdaNo, m.AdSoyad, r.GirisTarihi, r.CikisTarihi, r.Tutar as OdaUcreti, m.AvatarYolu, r.Durum
                      FROM Tbl_Rezervasyonlar r
                      JOIN Tbl_Musteriler m ON r.MusteriId = m.Id
                      WHERE (r.Durum = 0 OR r.Durum = 1) OR (r.Durum = 3 AND CAST(r.CikisTarihi AS DATE) = CAST(GETDATE() AS DATE))
                      ORDER BY r.Id DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var resId = Convert.ToInt32(reader["Id"]);
            var roomPrice = reader["OdaUcreti"] != DBNull.Value ? Convert.ToDecimal(reader["OdaUcreti"]) : 0;
            
            // Bakiyeyi hesapla (Oda Ücreti + Ekstralar - Ödemeler)
            // NOT: Transaction takibi için description içinde #RZ-ID formatını kullanıyoruz
            var balanceStruct = await CalculateReservationBalanceInternalAsync(connection, resId, roomPrice);
            
            // Durum mapping
            var dbStatus = Convert.ToInt32(reader["Durum"]);
            var status = CheckoutStatus.Pending;
            
            // 3 = CheckedOut
            if (dbStatus == 3) 
            {
                status = CheckoutStatus.Completed;
            }
            else if (balanceStruct.Remaining <= 0)
            {
                // Giriş yapmış ama borcu yok
                status = CheckoutStatus.Completed; // veya ReadyToCheckout diyebiliriz ama UI Completed ile yeşil gösteriyor
            }
            // else Pending

            checkoutList.Add(new GuestCheckoutModel
            {
                Id = resId,
                RoomNo = reader["OdaNo"]?.ToString() ?? "?",
                GuestName = reader["AdSoyad"]?.ToString() ?? "Bilinmiyor",
                GuestInitials = GetInitials(reader["AdSoyad"]?.ToString() ?? ""),
                CheckInDate = Convert.ToDateTime(reader["GirisTarihi"]),
                CheckOutDate = Convert.ToDateTime(reader["CikisTarihi"]),
                Balance = balanceStruct.Remaining,
                AvatarUrl = reader["AvatarYolu"]?.ToString(),
                Status = status
            });
        }
        
        return checkoutList;
    }

    public async Task<List<InvoiceItemModel>> GetReservationDetailsAsync(int reservationId)
    {
        var items = new List<InvoiceItemModel>();
        
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        // 1. Oda Ücreti
        var roomQuery = "SELECT Tutar, DATEDIFF(day, GirisTarihi, CikisTarihi) as Gece FROM Tbl_Rezervasyonlar WHERE Id = @Id";
        using (var cmd = new SqlCommand(roomQuery, connection))
        {
            cmd.Parameters.AddWithValue("@Id", reservationId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var totalAmount = Convert.ToDecimal(reader["Tutar"]);
                var nights = Convert.ToInt32(reader["Gece"]);
                items.Add(new InvoiceItemModel 
                { 
                    Description = $"Konaklama Ücreti ({nights} gece)", 
                    Amount = totalAmount, 
                    Category = "Konaklama" 
                });
            }
        }
        
        // 2. Ekstralar (Tbl_Islemler'den çekilecek - Description like #RZ-ID and Amount < 0 [Gider/Harcama])
        // Bizim yapımızda Tbl_Islemler genelde 'Gelir/Gider' tutuyor.
        // Müşteri Harcaması (Ekstra) -> İşletme için GELİR potansiyeli ama Folio için BORÇ.
        // Ancak genellikle Tbl_Islemler "Kasa Hareketi"dir.
        // Eğer "Ekstra Harcama" eklersek, bu henüz kasaya girmediği için Tbl_Islemler'e kaydetmek yerine
        // Sadece "Borç" olarak bir yere yazmalıyız. 
        // Ancak User "Rezervasyonlar ve İşlemler tablosu ile yürütelim" dedi.
        // Bu durumda Tbl_Islemler'e "Tahakkuk" (Borç yansıtma) gibi bir kayıt atabiliriz veya
        // Daha basiti: Ödenmemiş ekstraları burada tutarız.
        
        // SADELEŞTİRME:
        // Tbl_Islemler tablosunda "Borç" diye bir durum olmadığı için,
        // Biz buraya 'Ekstra Hizmet' kategorisinde, tutarı pozitif (Alacak) olarak girelim.
        // Ödeme alındığında da 'Tahsilat' kategorisinde girelim.
        
        var transQuery = "SELECT Aciklama, Tutar, Kategori FROM Tbl_Islemler WHERE Aciklama LIKE @SearchTerm";
        using (var cmd = new SqlCommand(transQuery, connection))
        {
            cmd.Parameters.AddWithValue("@SearchTerm", $"%#RZ-{reservationId}%");
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var desc = reader["Aciklama"]?.ToString() ?? "";
                var cat = reader["Kategori"]?.ToString() ?? "";
                var amount = Convert.ToDecimal(reader["Tutar"]);
                
                // Description'dan #RZ-ID kısmını temizle
                var cleanDesc = desc.Replace($"#RZ-{reservationId}", "").Trim();
                if (cleanDesc.StartsWith("-")) cleanDesc = cleanDesc.Substring(1).Trim();
                
                // Eğer Kategori Ekstra ise listeye ekle
                if (cat == "Ekstra")
                {
                     items.Add(new InvoiceItemModel { Description = cleanDesc, Amount = amount, Category = cat });
                }
            }
        }
        
        return items;
    }
    
    // Yardımcı bakiye hesaplama (Connection açıkken kullanılır)
    private async Task<(decimal Total, decimal Paid, decimal Remaining)> CalculateReservationBalanceInternalAsync(SqlConnection connection, int reservationId, decimal roomPrice)
    {
        decimal extras = 0;
        decimal paid = 0;
        
        // Transactionları tara
        // Kural: 
        // Kategori = 'Ekstra' -> Borç artırır (Oda fiyatına eklenir)
        // Kategori = 'Tahsilat' -> Borç azaltır (Ödeme)
        
        // Not: Mevcut bir connection üzerinde reader hatası almamak için yeni command kullanıyoruz (MARS kapalı olabilir)
        // Bu yüzden güvenli yol: Veriyi alıp bellekte işlemek veya ayrı connection açmak (ama transaction scope yoksa pahalı).
        // Azure SQL/LocalDB genelde MARS destekler ama garanti değil.
        // Basitlik için ExecuteScalar veya ayrı connection kullanmak yerine, reader döngüsü bitince çağırıldığından emin olacağız.
        // GetCheckoutListAsync içindeki döngüdeyiz, MARS gerekir.
        // O yüzden ayrı connection açmak en güvenlisi bu kapsamda, performans ikinci planda şu an.
        
        using var subConn = GetConnection();
        await subConn.OpenAsync();
        
        var query = "SELECT Kategori, Tutar FROM Tbl_Islemler WHERE Aciklama LIKE @SearchTerm";
        using var cmd = new SqlCommand(query, subConn);
        cmd.Parameters.AddWithValue("@SearchTerm", $"%#RZ-{reservationId}%");
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var cat = reader["Kategori"]?.ToString();
            var amount = Convert.ToDecimal(reader["Tutar"]);
            
             if (cat == "Ekstra") extras += amount;
             else if (cat == "Tahsilat") paid += amount;
        }
        
        decimal total = roomPrice + extras;
        return (total, paid, total - paid);
    }
    
    public async Task<bool> AddReservationTransactionAsync(int reservationId, string description, string category, decimal amount)
    {
        // Description içine ID gömüyoruz
        var fullDesc = $"{description} #RZ-{reservationId}";
        // Durum genellikle 'Onaylandı'
        return await AddTransactionAsync(fullDesc, category, amount, "Onaylandı");
    }
    
    public async Task<bool> CheckoutReservationAsync(int reservationId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        // 1. Rezervasyonun Oda Numarasını Bul
        var roomQuery = "SELECT OdaNo FROM Tbl_Rezervasyonlar WHERE Id = @Id";
        string roomNo = "";
        
        using (var cmdCheck = new SqlCommand(roomQuery, connection))
        {
            cmdCheck.Parameters.AddWithValue("@Id", reservationId);
            roomNo = (await cmdCheck.ExecuteScalarAsync())?.ToString() ?? "";
        }
        
        if (!string.IsNullOrEmpty(roomNo))
        {
            // 2. Odayı 'Temizlenecek' (Durum=2) olarak güncelle
            var updateRoom = "UPDATE Tbl_Odalar SET Durum = 2 WHERE OdaNo = @OdaNo";
            using (var cmdRoom = new SqlCommand(updateRoom, connection))
            {
                cmdRoom.Parameters.AddWithValue("@OdaNo", roomNo);
                await cmdRoom.ExecuteNonQueryAsync();
            }
        }
        
        // 3. Rezervasyonu Tamamlandı (CheckedOut=3) yap
        var query = "UPDATE Tbl_Rezervasyonlar SET Durum = 3 WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", reservationId);
        
        return await command.ExecuteNonQueryAsync() > 0;
    }

    #endregion

    #region Helper Methods

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "??";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][..1].ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
