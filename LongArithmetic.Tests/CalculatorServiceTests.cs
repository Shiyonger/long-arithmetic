using FluentAssertions;
using LongArithmetic.Core.Interfaces;
using LongArithmetic.Core.Models;
using Moq;

namespace LongArithmetic.Tests;

[TestFixture]
public class CalculatorServiceTests
{
    private Mock<IConsoleIO> _mockConsoleIO;
    private Mock<IDataIO> _mockDataIO;
    private CalculatorService _service;

    [SetUp]
    public void SetUp()
    {
        _mockConsoleIO = new Mock<IConsoleIO>();
        _mockDataIO = new Mock<IDataIO>();
        _service = new CalculatorService(_mockConsoleIO.Object, _mockDataIO.Object);
    }

    #region Run - Single Operation from Console Tests

    [Test]
    [Category("ConsoleMode")]
    public void Run_ConsoleMode_AskForAddOperation()
    {
        _mockConsoleIO
            .Setup(x => x.AskUseFileInput())
            .Returns(false);

        var request = new OperationRequest
        {
            Id = "test-1",
            Operation = "add",
            Method = "",
            Operands = new List<string> { "5", "3" }
        };

        _mockConsoleIO
            .Setup(x => x.AskOperationFromConsole())
            .Returns(request);

        _mockConsoleIO
            .Setup(x => x.AskWriteToFile())
            .Returns(false);

        _service.Run();

        _mockConsoleIO.Verify(x => x.AskUseFileInput(), Times.Once);
        _mockConsoleIO.Verify(x => x.AskOperationFromConsole(), Times.Once);
        _mockConsoleIO.Verify(x => x.ShowResult(It.IsAny<OperationResult>()), Times.Once);
    }

    [Test]
    [Category("ConsoleMode")]
    public void Run_ConsoleMode_PerformsAddition()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);

        var request = new OperationRequest
        {
            Id = "test-add",
            Operation = "add",
            Method = "",
            Operands = new List<string> { "10", "20" }
        };

        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);


        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Should().NotBeNull();
        capturedResult.Result.Should().Be("30");
    }

    [Test]
    [Category("ConsoleMode")]
    public void Run_ConsoleMode_SavesResultToFile()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);

        var request = new OperationRequest
        {
            Id = "test-save",
            Operation = "add",
            Method = "",
            Operands = new List<string> { "1", "1" }
        };

        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("output.json");


        List<OperationResult> capturedResults = null;
        _mockDataIO
            .Setup(x => x.WriteResults(It.IsAny<string>(), It.IsAny<List<OperationResult>>()))
            .Callback<string, List<OperationResult>>((path, results) => capturedResults = results);

        _service.Run();

        _mockDataIO.Verify(x => x.WriteResults("output.json", It.IsAny<List<OperationResult>>()), Times.Once);
        capturedResults.Should().NotBeNull();
        capturedResults.Count.Should().Be(1);
    }

    #endregion

    #region Validation Tests

    [Test]
    [Category("Validation")]
    public void Run_ConsoleMode_InvalidOperand_ReturnsIncorrectData()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "-5", "3" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("Некорректные данные");
    }

    [Test]
    [Category("Validation")]
    public void Run_ConsoleMode_NonNumericOperand_ReturnsIncorrectData()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "subtract", Method = "", Operands = new List<string> { "abc", "1" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("Некорректные данные");
    }

    #endregion

    #region Run - File Input Mode Tests

    [Test]
    [Category("FileMode")]
    public void Run_FileMode_ReadsOperationsFromFile()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskFilePath(It.IsAny<string>())).Returns("input.json");

        var operations = new List<OperationRequest>
        {
            new OperationRequest
                { Id = "op1", Operation = "add", Method = "", Operands = new List<string> { "5", "5" } }
        };

        _mockDataIO.Setup(x => x.ReadOperations("input.json")).Returns(operations);

        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("output.json");

        _service.Run();

        _mockDataIO.Verify(x => x.ReadOperations("input.json"), Times.Once);
    }

    [Test]
    [Category("FileMode")]
    public void Run_FileMode_ProcessesMultipleOperations()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskFilePath(It.IsAny<string>())).Returns("input.json");

        var operations = new List<OperationRequest>
        {
            new OperationRequest
                { Id = "op1", Operation = "add", Method = "", Operands = new List<string> { "5", "5" } },
            new OperationRequest
                { Id = "op2", Operation = "multiply", Method = "classic", Operands = new List<string> { "3", "4" } },
            new OperationRequest
                { Id = "op3", Operation = "subtract", Method = "", Operands = new List<string> { "10", "3" } }
        };

        _mockDataIO.Setup(x => x.ReadOperations("input.json")).Returns(operations);
        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("output.json");

        List<OperationResult> capturedResults = null;
        _mockDataIO
            .Setup(x => x.WriteResults(It.IsAny<string>(), It.IsAny<List<OperationResult>>()))
            .Callback<string, List<OperationResult>>((path, results) => capturedResults = results);

        _service.Run();

        capturedResults.Should().NotBeNull();
        capturedResults.Count.Should().Be(3);
        capturedResults[0].Result.Should().Be("10");
        capturedResults[1].Result.Should().Be("12");
        capturedResults[2].Result.Should().Be("7");
    }

    [Test]
    [Category("FileMode")]
    public void Run_FileMode_SavesResultsToFile()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskFilePath(It.IsAny<string>())).Returns("input.json");

        var operations = new List<OperationRequest>
        {
            new OperationRequest
                { Id = "op1", Operation = "add", Method = "", Operands = new List<string> { "1", "1" } }
        };

        _mockDataIO.Setup(x => x.ReadOperations("input.json")).Returns(operations);
        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("results.json");

        _service.Run();

        _mockDataIO.Verify(x => x.WriteResults("results.json", It.IsAny<List<OperationResult>>()), Times.Once);
    }

    #endregion

    #region Mock Tests - ConsoleIO

    [Test]
    [Category("Mocking")]
    [Category("MockConsoleIO")]
    public void Mock_ConsoleIO_VerifyCallCount()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "2" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        _service.Run();

        _mockConsoleIO.Verify(x => x.AskWriteToFile(), Times.Once);
    }

    [Test]
    [Category("Mocking")]
    [Category("MockConsoleIO")]
    public void Mock_ConsoleIO_VerifyCallWithMatcher()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "2" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        _mockConsoleIO.Verify(x => x.ShowResult(It.IsAny<OperationResult>()), Times.Once);
        capturedResult.Result.Should().Be("3");
    }

    [Test]
    [Category("Mocking")]
    [Category("MockConsoleIO")]
    public void Mock_ConsoleIO_VerifyCalledWithSpecificMatcher()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "multiply", Method = "classic", Operands = new List<string> { "5", "6" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        _service.Run();

        _mockConsoleIO.Verify(
            x => x.ShowResult(It.Is<OperationResult>(r => r.Result == "30")),
            Times.Once);
    }

    #endregion

    #region Mock Tests - DataIO

    [Test]
    [Category("Mocking")]
    [Category("MockDataIO")]
    public void Mock_DataIO_VerifyReadCalledWithFile()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskFilePath(It.IsAny<string>())).Returns("test.json");
        _mockDataIO.Setup(x => x.ReadOperations("test.json")).Returns(new List<OperationRequest>());
        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("out.json");

        _service.Run();

        _mockDataIO.Verify(x => x.ReadOperations("test.json"), Times.Once);
    }

    [Test]
    [Category("Mocking")]
    [Category("MockDataIO")]
    public void Mock_DataIO_VerifyWriteFormat()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskFilePath(It.IsAny<string>())).Returns("input.json");

        var ops = new List<OperationRequest>
        {
            new OperationRequest { Id = "1", Operation = "add", Method = "", Operands = new List<string> { "7", "8" } }
        };

        _mockDataIO.Setup(x => x.ReadOperations("input.json")).Returns(ops);
        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("output.json");

        _service.Run();

        _mockDataIO.Verify(
            x => x.WriteResults(
                "output.json",
                It.Is<List<OperationResult>>(r => r.Count > 0 && r[0].Result == "15")),
            Times.Once);
    }

    [Test]
    [Category("Mocking")]
    [Category("MockDataIO")]
    public void Mock_DataIO_VerifyNeverCalledInConsoleOnlyMode()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "1" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        _service.Run();

        _mockDataIO.Verify(x => x.ReadOperations(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Spy Tests - Verify Operation Routing

    [Test]
    [Category("Mocking")]
    [Category("Spy")]
    public void Spy_VerifyAddOperationCalled()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "100", "200" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("300");
    }

    [Test]
    [Category("Mocking")]
    [Category("Spy")]
    public void Spy_VerifySubtractOperationCalled()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "subtract", Method = "", Operands = new List<string> { "500", "100" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("400");
    }

    [Test]
    [Category("Mocking")]
    [Category("Spy")]
    public void Spy_VerifyMultiplyOperationCalled()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "multiply", Method = "classic", Operands = new List<string> { "12", "11" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("132");
    }

    [Test]
    [Category("Mocking")]
    [Category("Spy")]
    public void Spy_VerifyDivideOperationCalled()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "divide", Method = "classic", Operands = new List<string> { "144", "12" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("12");
    }

    [Test]
    [Category("Mocking")]
    [Category("Spy")]
    public void Spy_VerifyPowOperationCalled()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "pow", Method = "binary", Operands = new List<string> { "2", "10" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("1024");
    }

    #endregion

    #region Parameterized Tests

    [TestCase("add", new[] { "15", "25" }, "40")]
    [TestCase("add", new[] { "9", "1" }, "10")]
    [TestCase("add", new[] { "99", "1" }, "100")]
    [TestCase("subtract", new[] { "50", "20" }, "30")]
    [TestCase("subtract", new[] { "100", "1" }, "99")]
    [TestCase("multiply", new[] { "6", "7" }, "42")]
    [TestCase("multiply", new[] { "9", "9" }, "81")]
    [TestCase("divide", new[] { "100", "10" }, "10")]
    [TestCase("divide", new[] { "50", "5" }, "10")]
    [Category("Parameterized")]
    public void Parameterized_AllOperations(string operation, string[] operands, string expected)
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
        {
            Id = "test",
            Operation = operation,
            Method = "",
            Operands = new List<string>(operands)
        };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be(expected);
    }

    #endregion

    #region Matcher Tests

    [Test]
    [Category("Matchers")]
    public void Matcher_IsAny_ConsoleIO()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "1" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => { });

        _service.Run();

        _mockConsoleIO.Verify(x => x.ShowResult(It.IsAny<OperationResult>()), Times.Once);
    }

    [Test]
    [Category("Matchers")]
    public void Matcher_Predicate_ConsoleIO()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "10", "20" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        _service.Run();

        _mockConsoleIO.Verify(
            x => x.ShowResult(It.Is<OperationResult>(r => r.Result == "30")),
            Times.Once);
    }

    [Test]
    [Category("Matchers")]
    public void Matcher_FilePath_DataIO()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(true);
        _mockConsoleIO.Setup(x => x.AskFilePath(It.IsAny<string>())).Returns("data.json");
        _mockDataIO.Setup(x => x.ReadOperations(It.IsAny<string>())).Returns(new List<OperationRequest>());
        _mockConsoleIO.Setup(x => x.AskOutputFilePath()).Returns("out.json");

        _service.Run();

        _mockDataIO.Verify(
            x => x.ReadOperations(It.Is<string>(f => f.Contains(".json"))),
            Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Test]
    [Category("ErrorHandling")]
    public void ErrorHandling_UnsupportedOperation_ReturnsError()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "unknown", Method = "", Operands = new List<string> { "1", "1" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Contain("Unsupported operation");
    }

    [Test]
    [Category("ErrorHandling")]
    public void ErrorHandling_InvalidOperands_ReturnsError()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "pow", Method = "", Operands = new List<string> { "abc", "xyz" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("Некорректные данные");
    }

    [Test]
    [Category("ErrorHandling")]
    public void ErrorHandling_DivideByZero_ReturnsError()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "divide", Method = "", Operands = new List<string> { "100", "0" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult capturedResult = null;
        _mockConsoleIO
            .Setup(x => x.ShowResult(It.IsAny<OperationResult>()))
            .Callback<OperationResult>(r => capturedResult = r);

        _service.Run();

        capturedResult.Result.Should().Be("Error: Attempted to divide by zero.");
    }

    #endregion

    #region Assumption Tests

    [Test]
    [Category("Assumptions")]
    public void Assumption_RequestNotNull()
    {
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "1" } };
        Assume.That(request, Is.Not.Null);

        request.Operation.Should().NotBeNull();
    }

    [Test]
    [Category("Assumptions")]
    public void Assumption_OperandsLengthValid()
    {
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "1" } };
        Assume.That(request.Operands.Count, Is.GreaterThanOrEqualTo(2));

        request.Operands.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Test]
    [Category("Assumptions")]
    public void Assumption_ConsoleIONotNull()
    {
        Assume.That(_mockConsoleIO, Is.Not.Null);

        _mockConsoleIO.Should().NotBeNull();
    }

    [Test]
    [Category("Assumptions")]
    public void Assumption_DataIONotNull()
    {
        Assume.That(_mockDataIO, Is.Not.Null);

        _mockDataIO.Should().NotBeNull();
    }

    #endregion

    #region Assertion Techniques

    [Test]
    [Category("Assertions")]
    public void Assertion_Equal()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "2", "2" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult result = null;
        _mockConsoleIO.Setup(x => x.ShowResult(It.IsAny<OperationResult>())).Callback<OperationResult>(r => result = r);

        _service.Run();

        Assert.That(result.Result, Is.EqualTo("4"));
    }

    [Test]
    [Category("Assertions")]
    public void Assertion_NotEqual()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "2" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult result = null;
        _mockConsoleIO.Setup(x => x.ShowResult(It.IsAny<OperationResult>())).Callback<OperationResult>(r => result = r);

        _service.Run();

        Assert.That(result.Result, Is.Not.EqualTo("5"));
    }

    [Test]
    [Category("Assertions")]
    public void Assertion_Null()
    {
        var nullValue = _mockConsoleIO.Object.GetType().GetProperty("Any") ?? null;

        Assert.That(nullValue, Is.Null);
    }

    [Test]
    [Category("Assertions")]
    public void Assertion_NotNull()
    {
        _mockConsoleIO.Setup(x => x.AskUseFileInput()).Returns(false);
        var request = new OperationRequest
            { Id = "test", Operation = "add", Method = "", Operands = new List<string> { "1", "1" } };
        _mockConsoleIO.Setup(x => x.AskOperationFromConsole()).Returns(request);
        _mockConsoleIO.Setup(x => x.AskWriteToFile()).Returns(false);

        OperationResult result = null;
        _mockConsoleIO.Setup(x => x.ShowResult(It.IsAny<OperationResult>())).Callback<OperationResult>(r => result = r);

        _service.Run();

        Assert.That(result, Is.Not.Null);
    }

    #endregion
}