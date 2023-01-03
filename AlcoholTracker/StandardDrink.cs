using System.Text.Json.Serialization;

namespace AlcoholTracker;

public class StandardDrink:IDrink
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
    public double StandardDrinks { get; set; }

    [JsonIgnore]
    public double Grams => StandardDrinks * 10;
    
    public override string ToString() => $"{StandardDrinks:N1} standard drinks";

}