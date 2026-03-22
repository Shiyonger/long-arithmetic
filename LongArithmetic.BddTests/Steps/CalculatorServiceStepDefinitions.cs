using System.Linq;
using LongArithmetic;
using LongArithmetic.Core.Models;
using NUnit.Framework;
using Reqnroll;

namespace LongArithmetic.BddTests.Steps;

[Binding]
public sealed class CalculatorServiceStepDefinitions(ScenarioContext scenarioContext)
{
    private readonly BddWorld _world = scenarioContext.GetWorld();

    [Given("the calculator service is ready")]
    public void GivenTheCalculatorServiceIsReady()
    {
    }

    [Given("console mode is selected for the calculator service")]
    public void GivenConsoleModeIsSelectedForTheCalculatorService()
    {
        _world.Console.UseFileInput = false;
        _world.Console.WriteToFile = false;
        _world.Console.FilePath = string.Empty;
        _world.Console.OutputFilePath = string.Empty;
        _world.Console.DisplayedResults.Clear();
        _world.Data.Reset();
    }

    [Given("file mode is selected with input path {string}")]
    public void GivenFileModeIsSelectedWithInputPath(string inputPath)
    {
        _world.Console.UseFileInput = true;
        _world.Console.FilePath = inputPath;
        _world.Console.OutputFilePath = string.Empty;
        _world.Console.DisplayedResults.Clear();
        _world.Data.Reset();
    }

    [Given("the output path is {string}")]
    public void GivenTheOutputPathIs(string outputPath)
    {
        _world.Console.OutputFilePath = outputPath;
    }

    [Given("a console request with id {string} for operation {string} and method {string} using operands {string} and {string}")]
    public void GivenAConsoleRequestWithIdForOperationAndMethodUsingOperandsAnd(
        string id,
        string operation,
        string method,
        string firstOperand,
        string secondOperand)
    {
        _world.Console.ConsoleRequest = new OperationRequest
        {
            Id = id,
            Operation = operation,
            Method = method,
            Operands = [firstOperand, secondOperand]
        };
    }

    [Given("the input operations are")]
    public void GivenTheInputOperationsAre(Table table)
    {
        _world.Data.OperationsToRead = table.Rows
            .Select(row => new OperationRequest
            {
                Id = row["Id"],
                Operation = row["Operation"],
                Method = row["Method"],
                Operands = [row["Operand1"], row["Operand2"]]
            })
            .ToList();
    }

    [When("I run the calculator service")]
    public void WhenIRunTheCalculatorService()
    {
        var service = new CalculatorService(_world.Console, _world.Data);
        service.Run();
    }

    [Then("the last shown result should be {string}")]
    public void ThenTheLastShownResultShouldBe(string expectedResult)
    {
        Assert.That(_world.Console.DisplayedResults.Last().Result, Is.EqualTo(expectedResult));
    }

    [Then("the last shown result id should be {string}")]
    public void ThenTheLastShownResultIdShouldBe(string expectedId)
    {
        Assert.That(_world.Console.DisplayedResults.Last().Id, Is.EqualTo(expectedId));
    }

    [Then("no results should be written to a file")]
    public void ThenNoResultsShouldBeWrittenToAFile()
    {
        Assert.That(_world.Data.WrittenResults, Is.Empty);
        Assert.That(_world.Data.WrittenPath, Is.Null.Or.Empty);
    }

    [Then("the service should still report the original request identifier {string}")]
    public void ThenTheServiceShouldStillReportTheOriginalRequestIdentifier(string expectedId)
    {
        Assert.That(_world.Console.DisplayedResults.Last().Id, Is.EqualTo(expectedId));
    }

    [Then("the saved results should be")]
    public void ThenTheSavedResultsShouldBe(Table table)
    {
        var expected = table.Rows
            .Select(row => new OperationResult
            {
                Id = row["Id"],
                Result = row["Result"]
            })
            .ToList();

        Assert.That(_world.Data.WrittenResults.Count, Is.EqualTo(expected.Count));
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.That(_world.Data.WrittenResults[i].Id, Is.EqualTo(expected[i].Id));
            Assert.That(_world.Data.WrittenResults[i].Result, Is.EqualTo(expected[i].Result));
        }
    }

    [Then("the output should be written to {string}")]
    public void ThenTheOutputShouldBeWrittenTo(string expectedPath)
    {
        Assert.That(_world.Data.WrittenPath, Is.EqualTo(expectedPath));
    }
}
