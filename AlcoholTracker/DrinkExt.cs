namespace AlcoholTracker;

public static class DrinkExt
{
    private static double WidmarkFormula(double alcoholConsumed, double bodyWeight, double baseRate, double timeElapsed)
    {        
        var bac = alcoholConsumed / ((bodyWeight*1000) * baseRate) * 100;
        return bac - (timeElapsed / 60) * 0.015;
    }
    public static double GetBac(this IDrink drink, double weight, double rate, DateTimeOffset now) => WidmarkFormula(drink.Grams, weight, rate, (now - drink.Time).TotalMinutes);
    public static DateTimeOffset GetCutoff(DateTimeOffset now) => now.AddDays(-1);
    public static bool IsDrinkPastCutoff(this IDrink drink, DateTimeOffset now) => drink.Time < GetCutoff(now);
}