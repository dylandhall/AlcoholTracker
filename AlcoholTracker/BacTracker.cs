using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using MudBlazor;

namespace AlcoholTracker;

public class BacTracker
{
    private const int StartXAxisMinutesAgo = -60;
    private const int NumberOfMinutesGraphed = 121;
    private const string AxisLabel = "BAC over time";

    [JsonIgnore]
    public Action<BacTracker>? UpdateCallback { get; set; }

    private List<ChartSeries> _timeSeries = new()
    {
        new()
        {
            Data = Enumerable.Range(StartXAxisMinutesAgo, NumberOfMinutesGraphed).Select(_ => 0d).ToArray(),
            Name = AxisLabel
        }
    };

    private int _hashOfList;
    private DateTimeOffset _lastUpdated = DateTimeOffset.UtcNow;


    public int Weight { get; set; } = 80;
    public Gender Gender { get; set; } = Gender.Male;
    public int HeightInCm { get; set; } = 165;
    public DateTime? BirthDate { get; set; } = DateTimeOffset.UtcNow.AddYears(-30).DateTime;
    
    private double EliminationRate =>
        Gender switch
        {
            Gender.Male => 0.68,
            Gender.Female => 0.55,
            _ => throw new Exception("Unsupported")
        };


    public List<StandardDrink> StandardDrinks { get; set; } = new();
    public List<DrinkInMl> DrinkInMls { get; set; } = new();

    public Dictionary<Guid, DateTimeOffset> RemovedDrinkIds { get; set; } = new();

    [JsonIgnore]
    public List<IDrinkBase> RecentDrinks
    {
        get
        {
            var combinedHash = HashCode.Combine(CountByHash.Keys);
            if (_recentDrinksHash == combinedHash) return _recentDrinks ?? new();

            _recentDrinksHash = combinedHash;
            _recentDrinks = GetRecentDrinks();
            return _recentDrinks;
        }
    }

    private int _recentDrinksHash;
    private List<IDrinkBase>? _recentDrinks;

    private List<IDrinkBase> GetRecentDrinks()
    {
        return StandardDrinksBase
            .Where(d => CountByHash.ContainsKey(d.Key))
            .Select(d => d.Value as IDrinkBase)
            .Concat(DrinkInMlsBase
                .Where(d => CountByHash.ContainsKey(d.Key))
                .Select(d => d.Value as IDrinkBase))
            .OrderByDescending(d => CountByHash.TryGetValue(d.DrinkHash, out var v) ? v : 0)
            .Take(10)
            .ToList();
    }

    public Dictionary<int, StandardDrinkBase> StandardDrinksBase { get; set; } = new();
    public Dictionary<int, DrinkInMlBase> DrinkInMlsBase { get; set; } = new();
    public ConcurrentDictionary<int, int> CountByHash { get; set; } = new();

    [JsonIgnore]
    public List<IDrink> Drinks =>
        StandardDrinks.Cast<IDrink>().Concat(DrinkInMls).ToList();

    public double GetPercentage() => GetPercentage(DateTimeOffset.UtcNow);

    public double GetPercentage(DateTimeOffset now)
    {
        var rate = EliminationRate;
        return Drinks.Where(d => d.Time<=now).Select(d => d.GetBac(Weight, rate, now)).Where(v => v>0).Sum();
    }

    public void RemoveDrink(IDrink drink)
    {
        RemovedDrinkIds.TryAdd(drink.DrinkId, DateTimeOffset.UtcNow);
        var r = StandardDrinks.RemoveAll(d => d.DrinkId == drink.DrinkId);
        if (r > 0) return;
        DrinkInMls.RemoveAll(d => d.DrinkId == drink.DrinkId);
    }
    
    
    
    private void PurgeOldDrinks()
    {
        var now = DateTimeOffset.UtcNow;
        StandardDrinks.RemoveAll(d => d.IsDrinkPastCutoff(now));
        DrinkInMls.RemoveAll(d => d.IsDrinkPastCutoff(now));
        var cutoff = DrinkExt.GetCutoff(now);
        foreach (var (drinkId, _) in RemovedDrinkIds.Where(v => v.Value < cutoff))
            RemovedDrinkIds.Remove(drinkId);
    }

    [JsonIgnore] 
    public string[] XAxisLabels { get; set; } = GetXAxisLabels();

    public void AddRecentDrinkByHash(DateTimeOffset time, int hash)
    {
        if (StandardDrinksBase.ContainsKey(hash))
        {
            var standardDrink = StandardDrinksBase[hash];
            AddDrink(new StandardDrink(time, standardDrink.StandardDrinks));
            return;
        }

        if (!DrinkInMlsBase.ContainsKey(hash)) return;

        var drinkInMls = DrinkInMlsBase[hash];

        AddDrink(new DrinkInMl(time, drinkInMls.SizeInMl, drinkInMls.DrinkPercent));
    }
    
    public void DuplicateDrink(Guid drinkId)
    {
        var drinkInMl = DrinkInMls.SingleOrDefault(d => d.DrinkId==drinkId);
        if (drinkInMl != null)
        {
            AddDrink(new DrinkInMl(DateTimeOffset.UtcNow, drinkInMl.SizeInMl, drinkInMl.DrinkPercent));
            return;
        }

        var standardDrink = StandardDrinks.SingleOrDefault(d => d.DrinkId == drinkId);
        
        if (standardDrink==null) return;
        
        AddDrink(new StandardDrink(DateTimeOffset.UtcNow, standardDrink.StandardDrinks));
    }
    
    public static string[] GetXAxisLabels() =>
        Enumerable.Range(StartXAxisMinutesAgo, NumberOfMinutesGraphed).Select(n => n%10==0 ? DateTime.Now.AddMinutes(n).ToString("H:mm"):String.Empty).ToArray();
    public List<ChartSeries> GetTimeSeries() // => GetSeries(); 
    {
        
        var hashOfList = GetHashOfCurrentData();
        var now = DateTimeOffset.UtcNow;
        var dataChanged = hashOfList != _hashOfList;
        if (!dataChanged && (now - _lastUpdated).TotalSeconds<60) return _timeSeries;
    
        PurgeOldDrinks();

        XAxisLabels = GetXAxisLabels();
        _hashOfList = GetHashOfCurrentData();
        _lastUpdated = now;
        _timeSeries = GetSeries();

        if (dataChanged && UpdateCallback != null)
        {
            UpdateCallback(this);
        }

        return _timeSeries;
    }

    private int GetHashOfCurrentData() => HashCode.Combine(HashCode.Combine(Drinks), Weight, Gender);


    public void ClearRecentDrinks()
    {
        CountByHash.Clear();
        StandardDrinksBase.Clear();
        DrinkInMlsBase.Clear();
    }
    
    private List<ChartSeries> GetSeries() => new() {
        new ChartSeries
        {
            Data = GetDrinksOverTime(),
            Name = AxisLabel
        }
    };

    private double[] GetDrinksOverTime()
    {
        var now = DateTimeOffset.UtcNow;
        return Enumerable.Range(StartXAxisMinutesAgo, NumberOfMinutesGraphed).Select(n => now.AddMinutes(n)).Select(GetPercentage).ToArray();
    }

    public void AddDrink(DrinkInMl drinkInMl)
    {
        DrinkInMls.Add(drinkInMl);
        var h = drinkInMl.DrinkHash;

        IncrementDrinkCount(h);

        DrinkInMlsBase.TryAdd(h, drinkInMl);
    }

    private void IncrementDrinkCount(int h) =>
        CountByHash.AddOrUpdate(h, _ => 1, (_, v) => v + 1);

    public void AddDrink(StandardDrink standardDrink)
    {
        StandardDrinks.Add(standardDrink);
        var h = standardDrink.DrinkHash;
        
        IncrementDrinkCount(h);
        StandardDrinksBase.TryAdd(h, standardDrink);
    }
}