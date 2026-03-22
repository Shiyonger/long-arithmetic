using Reqnroll;

namespace LongArithmetic.BddTests.Steps;

public static class ScenarioContextExtensions
{
    private const string WorldKey = "BDD_WORLD";

    public static BddWorld GetWorld(this ScenarioContext scenarioContext)
    {
        if (scenarioContext.TryGetValue(WorldKey, out BddWorld? existing) && existing != null)
        {
            return existing;
        }

        var created = new BddWorld();
        scenarioContext[WorldKey] = created;
        return created;
    }
}
