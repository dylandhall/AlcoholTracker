using System.Text.Json.Serialization;

namespace AlcoholTracker;

public class StandardDrinkBase : IDrinkBase
{
    public int DrinkHash => (int) (StandardDrinks * 1000);
    public double StandardDrinks { get; set; }
    public override string ToString() => $"{StandardDrinks:N1} standard drinks";
}

public class StandardDrink: StandardDrinkBase, IDrink
{
    public StandardDrink()
    {
    }

    public StandardDrink(DateTimeOffset time, double standardDrinks)
    {
        Time = time;
        StandardDrinks = standardDrinks;
        DrinkId = Guid.NewGuid();
    }


    public Guid DrinkId { get; set; }
    public DateTimeOffset Time { get; set; }

    [JsonIgnore]
    public double Grams => StandardDrinks * 10;
}