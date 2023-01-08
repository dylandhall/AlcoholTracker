namespace AlcoholTracker;

public static class DrinkExt
{

    public static double GetQValue(
        Gender gender,
        double bodyWeightInKg,
        double heightInCm,
        double ageInYears) =>
        gender switch
        {
            Gender.Male => MaleWatsonTbwFormula(bodyWeightInKg, heightInCm, ageInYears),
            Gender.Female => FemaleWatsonTbwFormula(bodyWeightInKg, heightInCm),
            _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
        };

    public static double MaleWatsonTbwFormula(
        double bodyWeightInKg,
        double heightInCm,
        double ageInYears)
    {
        return 0.39834 + ((12.725 * (heightInCm / 100)) / bodyWeightInKg) - (0.11275 * (ageInYears)) + (2.8993 / bodyWeightInKg);
    }

    public static double FemaleWatsonTbwFormula(
        double bodyWeightInKg,
        double heightInCm)
    {
        return 0.29218 + ((12.666 * (heightInCm / 100)) / bodyWeightInKg) - 2.4846 / bodyWeightInKg;
    }


    public static double UpdatedWidmarkFormula(double alcoholConsumedInGrams, double qValue, double eliminationRate, double timeElapsedInMinutes)
    {
        var bac = (0.844 * alcoholConsumedInGrams) / qValue;

        return bac - eliminationRate * (timeElapsedInMinutes / 60);
    }

    
    public static double WidmarkFormula(double alcoholConsumed, double bodyWeight, double baseRate, double timeElapsedInMinutes)
    {
        var bac = alcoholConsumed / ((bodyWeight*1000) * baseRate) * 100;
        return bac - (timeElapsedInMinutes / 60) * 0.015;
    }
    

    public static double GetBac(this IDrink drink, double weight, double rate, DateTimeOffset now) => WidmarkFormula(drink.Grams, weight, rate, (now - drink.Time).TotalMinutes);

    public static double GetBac(this IDrink drink, Gender gender, double heightInCm, double weightInKg, double ageInYears,
        double eliminationRate, DateTimeOffset now) =>
        UpdatedWidmarkFormula(drink.Grams, GetQValue(gender, weightInKg, heightInCm, ageInYears), eliminationRate, (now - drink.Time).TotalMinutes);
    public static DateTimeOffset GetCutoff(DateTimeOffset now) => now.AddDays(-1);
    
    // i hate that i'm calculating the cutoff more than once but i want it centralised
    public static bool IsDrinkPastCutoff(this IDrink drink, DateTimeOffset now) => drink.Time < GetCutoff(now);
}