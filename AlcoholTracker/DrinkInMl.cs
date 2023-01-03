using System.Text.Json.Serialization;

namespace AlcoholTracker;

public class DrinkInMl:IDrink
{
    public DrinkInMl()
    {
    }

    public DrinkInMl(DateTimeOffset time, double sizeInMl, double drinkPercent)
    {
        DrinkId = Guid.NewGuid();
        Time = time;
        SizeInMl = sizeInMl;
        DrinkPercent = drinkPercent;
    }
    
    public Guid DrinkId { get; set; }
    public DateTimeOffset Time { get; set; }

    [JsonIgnore]
    public double Grams =>
        SizeInMl * (DrinkPercent / 100);

    
    public double SizeInMl { get; set; }
    public double DrinkPercent { get; set; }

    public override string ToString() => $"{SizeInMl} mL at {DrinkPercent}%";


}