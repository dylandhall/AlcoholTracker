using AlcoholTracker;

namespace AlcoholTrackingUnitTests;

[TestClass]
public class TestFormula
{
    [TestMethod]
    [DataTestMethod]
    [Ignore]
    [DataRow(6d, 0.0283d)]
    [DataRow(12d, 0.0283d)]
    [DataRow(18d, 0.0283d)]
    [DataRow(24d, 0.0283d)]
    public void WatsonFormula_23Male188Cm_OneDrink_ValidValues(double timeSinceConsumption, double expectedBac)
    {
        var drinkMl = 73.9338;
        var alcoholPercent = 75.5;

        var drinkInMl = new DrinkInMl(DateTimeOffset.UtcNow.AddMinutes(-timeSinceConsumption), drinkMl, alcoholPercent);
        
        
    }
}