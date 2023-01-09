using System.Diagnostics;

namespace AlcoholTracker;

public static class DrinkExt
{
    public static double WidmarkFormula(double alcoholConsumedInGrams, double weightInKg, double rValue,
        double eliminationRate, double timeElapsedInMinutes)
    {
        var bac = alcoholConsumedInGrams / (rValue * weightInKg * 1000d) * 100d;
        return bac - (timeElapsedInMinutes / 60) * eliminationRate;
    }

    public static double GetBac(this IDrink drink, Person person, DateTimeOffset now) =>
        WidmarkFormula(drink.Grams, person.WeightInKg, person.GetAvgRValue(), person.EliminationRate, (now - drink.Time).TotalMinutes);
    public static DateTimeOffset GetCutoff(DateTimeOffset now) => now.AddDays(-1);
    
    // i hate that i'm calculating the cutoff more than once but i want it centralised
    public static bool IsDrinkPastCutoff(this IDrink drink, DateTimeOffset now) => drink.Time < GetCutoff(now);
}