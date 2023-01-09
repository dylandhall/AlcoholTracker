namespace AlcoholTracker;

public class Person
{
    public int GetDetailsHash() => HashCode.Combine(WeightInKg, Gender, HeightInCm, BirthDate??default);

    public int WeightInKg { get; set; } = 80;
    public Gender Gender { get; set; } = Gender.Male;
    public int HeightInCm { get; set; } = 165;
    public DateTime? BirthDate { get; set; } = DateTimeOffset.UtcNow.AddYears(-30).DateTime;
    
    public double EliminationRate =>
        Gender switch
        {
            Gender.Male => 0.02d,
            Gender.Female => 0.015d,
            _ => throw new Exception("Unsupported")
        };

    private double _rValue;
    private int _rValueHash;

    public double GetAvgRValue()
    {
        if (_rValueHash == GetDetailsHash())
            return _rValue;
        
        _rValueHash = GetDetailsHash();
        _rValue = Gender switch
            {
                Gender.Male => MaleAvgTbwFormula(WeightInKg, HeightInCm,
                    (DateTimeOffset.UtcNow - (BirthDate ?? DateTimeOffset.UtcNow.AddYears(-30))).TotalDays / 365),
                Gender.Female => FemaleAvgTbwFormula(WeightInKg, HeightInCm),
                _ => throw new Exception("Unsupported")
            };

        return _rValue;
    }

    private static double MaleAvgTbwFormula(
        double bodyWeightInKg,
        double heightInCm,
        double ageInYears)
    {
        var heightInMeters = heightInCm / 100d;
        var maleAvgTbw = 0.62544d + 
                         0.13664d * heightInMeters - 
                         bodyWeightInKg * (0.00189d + 0.002425d/(heightInMeters*heightInMeters)) + 
                         1 / (bodyWeightInKg * (0.57986d + 2.545d*heightInMeters - 0.02255d * ageInYears));
        return maleAvgTbw;
    }


    private static double FemaleAvgTbwFormula(
        double bodyWeightInKg,
        double heightInCm)
    {
        var heightInMeters = heightInCm / 100d;
        var femaleAvgTbwFormula = 0.50766d + 0.11165d * heightInMeters - bodyWeightInKg * (0.001612d + 0.0031 / (heightInMeters*heightInMeters)) -
                                  1 / bodyWeightInKg * (0.62115d - 3.1665d * heightInMeters);
        return femaleAvgTbwFormula;
    }
}