using LongArithmetic.Core.Models;

namespace LongArithmetic.Core.Interfaces;

public interface IConsoleIO
{
    bool AskUseFileInput();
    string AskFilePath(string prompt);
    OperationRequest AskOperationFromConsole();
    void ShowResult(OperationResult result);
    bool AskWriteToFile();
    string AskOutputFilePath();
}
