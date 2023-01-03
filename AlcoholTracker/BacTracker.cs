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

    public string Gender { get; set; } = "male";

    private double EliminationRate =>
        Gender switch
        {
            "male" => 0.68,
            "female" => 0.55,
            _ => 0.62
        };


    public List<StandardDrink> StandardDrinks { get; set; } = new();
    public List<DrinkInMl> DrinkInMls { get; set; } = new();


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
        int r = StandardDrinks.RemoveAll(d => d.DrinkId == drink.DrinkId);
        if (r > 0) return;
        DrinkInMls.RemoveAll(d => d.DrinkId == drink.DrinkId);
    }

    private void PurgeOldDrinks()
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-1);
        StandardDrinks.RemoveAll(d => d.Time < cutoff);
        DrinkInMls.RemoveAll(d => d.Time < cutoff);
    }

    [JsonIgnore] 
    public string[] XAxisLabels { get; set; } = GetXAxisLabels();

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
    }
    public void AddDrink(StandardDrink standardDrink)
    {
        StandardDrinks.Add(standardDrink);
    }
}