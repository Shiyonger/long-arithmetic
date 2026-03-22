using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LongArithmetic.Core;
using LongArithmetic.Core.Models;
using NUnit.Framework;
using Reqnroll;

namespace LongArithmetic.BddTests.Steps;

[Binding]
public sealed class ArithmeticStepDefinitions(ScenarioContext scenarioContext)
{
    private readonly BddWorld _world = scenarioContext.GetWorld();

    [Given("the calculator works with positive integer strings")]
    public void GivenTheCalculatorWorksWithPositiveIntegerStrings()
    {
    }

    [Given("the operands are {string} and {string}")]
    public void GivenTheOperandsAreAnd(string first, string second)
    {
        _world.FirstOperand = first;
        _world.SecondOperand = second;
        _world.Method = string.Empty;
        _world.LastResult = string.Empty;
        _world.AlgorithmResults.Clear();
        _world.AlgorithmDurationsMs.Clear();
    }

    [Given("the base is {string} and the exponent is {int}")]
    public void GivenTheBaseIsAndTheExponentIs(string baseValue, int exponent)
    {
        _world.BaseOperand = baseValue;
        _world.Exponent = exponent;
        _world.Method = string.Empty;
        _world.LastResult = string.Empty;
        _world.AlgorithmResults.Clear();
        _world.AlgorithmDurationsMs.Clear();
    }

    [Given("the calculation method is {string}")]
    public void GivenTheCalculationMethodIs(string method)
    {
        _world.Method = method;
    }

    [Given("the multiplication algorithms {string} and {string} are available")]
    public void GivenTheMultiplicationAlgorithmsAndAreAvailable(string firstMethod, string secondMethod)
    {
        _world.AlgorithmResults.Clear();
        _world.AlgorithmDurationsMs.Clear();
        _world.Method = string.Empty;
    }

    [Given("multiplication inputs generated for {string} data with {int} digits per operand")]
    public void GivenMultiplicationInputsGeneratedForDataWithDigitsPerOperand(string profile, int digits)
    {
        _world.FirstOperand = GenerateOperand(profile, digits, reverse: false);
        _world.SecondOperand = GenerateOperand(profile, digits, reverse: true);
        _world.BenchmarkProfile = profile;
        _world.BenchmarkDigits = digits;
        _world.AlgorithmResults.Clear();
        _world.AlgorithmDurationsMs.Clear();
    }

    [Given("the candidate multiplication sizes are")]
    public void GivenTheCandidateMultiplicationSizesAre(Table table)
    {
        _world.CandidateSizes.Clear();
        foreach (var row in table.Rows)
        {
            _world.CandidateSizes.Add(int.Parse(row["Digits"]));
        }
    }

    [When("I add the numbers")]
    public void WhenIAddTheNumbers()
    {
        _world.LastResult = BigMath.Add(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand)).Value;
    }

    [When("I subtract the second number from the first")]
    public void WhenISubtractTheSecondNumberFromTheFirst()
    {
        _world.LastResult = BigMath.Subtract(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand)).Value;
    }

    [When("I multiply the numbers")]
    public void WhenIMultiplyTheNumbers()
    {
        _world.LastResult = BigMath
            .Multiply(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand), ResolveMethod("auto"))
            .Value;
    }

    [When("I multiply the numbers using {string}")]
    public void WhenIMultiplyTheNumbersUsing(string method)
    {
        _world.AlgorithmResults[method] = BigMath
            .Multiply(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand), method)
            .Value;
    }

    [When("I divide the first number by the second")]
    public void WhenIDivideTheFirstNumberByTheSecond()
    {
        _world.LastResult = BigMath
            .Divide(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand), ResolveMethod("classic"))
            .Value;
    }

    [When("I divide the first number by the second using {string}")]
    public void WhenIDivideTheFirstNumberByTheSecondUsing(string method)
    {
        _world.AlgorithmResults[method] = BigMath
            .Divide(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand), method)
            .Value;
    }

    [When("I raise the base to the exponent")]
    public void WhenIRaiseTheBaseToTheExponent()
    {
        _world.LastResult = BigMath
            .Pow(new BigNumber(_world.BaseOperand), _world.Exponent, ResolveMethod("binary"))
            .Value;
    }

    [When("I raise the base to the exponent using {string}")]
    public void WhenIRaiseTheBaseToTheExponentUsing(string method)
    {
        _world.AlgorithmResults[method] = BigMath
            .Pow(new BigNumber(_world.BaseOperand), _world.Exponent, method)
            .Value;
    }

    [When("I benchmark multiplication algorithm {string}")]
    public void WhenIBenchmarkMultiplicationAlgorithm(string method)
    {
        var watch = Stopwatch.StartNew();
        var result = BigMath
            .Multiply(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand), method)
            .Value;
        watch.Stop();

        _world.AlgorithmResults[method] = result;
        _world.AlgorithmDurationsMs[method] = watch.ElapsedMilliseconds;
    }

    [When("I run the preliminary multiplication experiment for {string} data")]
    public void WhenIRunThePreliminaryMultiplicationExperimentForData(string profile)
    {
        _world.SelectedSafeSize = 0;
        _world.TotalExperimentMilliseconds = 0;

        foreach (var digits in _world.CandidateSizes)
        {
            GivenMultiplicationInputsGeneratedForDataWithDigitsPerOperand(profile, digits);
            WhenIBenchmarkMultiplicationAlgorithm("classic");
            WhenIBenchmarkMultiplicationAlgorithm("karatsuba");

            AssertAlgorithmResultsMatch();

            var slowest = _world.AlgorithmDurationsMs.Values.DefaultIfEmpty().Max();
            _world.TotalExperimentMilliseconds += _world.AlgorithmDurationsMs.Values.Sum();
            if (slowest <= 5000)
            {
                _world.SelectedSafeSize = digits;
            }
        }
    }

    [Then("the result should be {string}")]
    public void ThenTheResultShouldBe(string expected)
    {
        Assert.That(_world.LastResult, Is.EqualTo(expected));
    }

    [Then("the normalized result should not start with unnecessary zeros")]
    public void ThenTheNormalizedResultShouldNotStartWithUnnecessaryZeros()
    {
        Assert.That(_world.LastResult.Length == 1 || !_world.LastResult.StartsWith('0'), Is.True);
    }

    [Then("the result should remain a positive integer")]
    public void ThenTheResultShouldRemainAPositiveInteger()
    {
        Assert.That(_world.LastResult.All(char.IsDigit), Is.True);
    }

    [Then("the algorithm results should match")]
    public void ThenTheAlgorithmResultsShouldMatch()
    {
        AssertAlgorithmResultsMatch();
    }

    [Then("the shared algorithm result should be {string}")]
    public void ThenTheSharedAlgorithmResultShouldBe(string expected)
    {
        Assert.That(_world.AlgorithmResults.Values.Distinct().Single(), Is.EqualTo(expected));
    }

    [Then("the multiplication result should satisfy the unit-test comparison oracle")]
    public void ThenTheMultiplicationResultShouldSatisfyTheUnitTestComparisonOracle()
    {
        var expected = BigMath
            .Multiply(new BigNumber(_world.FirstOperand), new BigNumber(_world.SecondOperand), "classic")
            .Value;
        Assert.That(_world.AlgorithmResults.Values.Distinct().Single(), Is.EqualTo(expected));
    }

    [Then("the timings should be collected for all benchmarked algorithms")]
    public void ThenTheTimingsShouldBeCollectedForAllBenchmarkedAlgorithms()
    {
        Assert.That(_world.AlgorithmDurationsMs.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(_world.AlgorithmDurationsMs.Values.All(value => value >= 0), Is.True);
    }

    [Then("each benchmark should finish within {int} milliseconds")]
    public void ThenEachBenchmarkShouldFinishWithinMilliseconds(int maxMilliseconds)
    {
        Assert.That(_world.AlgorithmDurationsMs.Values.All(value => value <= maxMilliseconds), Is.True);
    }

    [Then("the benchmark profile should be recorded as {string} with size {int}")]
    public void ThenTheBenchmarkProfileShouldBeRecordedAsWithSize(string profile, int digits)
    {
        Assert.That(_world.BenchmarkProfile, Is.EqualTo(profile));
        Assert.That(_world.BenchmarkDigits, Is.EqualTo(digits));
    }

    [Then("a maximal safe multiplication size should be selected")]
    public void ThenAMaximalSafeMultiplicationSizeShouldBeSelected()
    {
        Assert.That(_world.SelectedSafeSize, Is.GreaterThan(0));
    }

    [Then("the selected size should be at least {int} digits")]
    public void ThenTheSelectedSizeShouldBeAtLeastDigits(int minimumDigits)
    {
        Assert.That(_world.SelectedSafeSize, Is.GreaterThanOrEqualTo(minimumDigits));
    }

    [Then("the full experiment should finish within {int} milliseconds")]
    public void ThenTheFullExperimentShouldFinishWithinMilliseconds(int maxMilliseconds)
    {
        Assert.That(_world.TotalExperimentMilliseconds, Is.LessThanOrEqualTo(maxMilliseconds));
    }

    private void AssertAlgorithmResultsMatch()
    {
        Assert.That(_world.AlgorithmResults, Is.Not.Empty);
        Assert.That(_world.AlgorithmResults.Values.Distinct().Count(), Is.EqualTo(1));
    }

    private string ResolveMethod(string defaultMethod)
    {
        return string.IsNullOrWhiteSpace(_world.Method) ? defaultMethod : _world.Method;
    }

    private static string GenerateOperand(string profile, int digits, bool reverse)
    {
        var content = profile.Equals("sparse", StringComparison.OrdinalIgnoreCase)
            ? GenerateSparseDigits(digits)
            : GenerateDenseDigits(digits);

        if (reverse)
        {
            content = new string(content.Reverse().ToArray());
        }

        if (content[0] == '0')
        {
            content = "1" + content[1..];
        }

        return content;
    }

    private static string GenerateDenseDigits(int digits)
    {
        var pattern = "98765432101234567890";
        return RepeatPattern(pattern, digits);
    }

    private static string GenerateSparseDigits(int digits)
    {
        var builder = new StringBuilder(digits);
        for (var i = 0; i < digits; i++)
        {
            builder.Append(i % 11 == 0 ? '9' : '0');
        }

        return builder.ToString();
    }

    private static string RepeatPattern(string pattern, int digits)
    {
        var builder = new StringBuilder(digits);
        while (builder.Length < digits)
        {
            builder.Append(pattern);
        }

        return builder.ToString(0, digits);
    }
}
