namespace AlcoholTracker;

public interface IDrink
{
    Guid DrinkId { get; set; }
    DateTimeOffset Time { get; }
    double Grams { get; }
}