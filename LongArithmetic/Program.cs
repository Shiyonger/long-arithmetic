using LongArithmetic;
using LongArithmetic.DataLayer;

var console = new ConsoleIO();
var dataIo = new DataIO();
var service = new CalculatorService(console, dataIo);
service.Run();
