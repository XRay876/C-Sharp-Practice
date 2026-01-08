int a = 10;
var b = "строка";     
object c = 42;        
dynamic d = 5;        
const double Pi = 3.14;
readonly Guid Id = Guid.NewGuid();  // для полей

// Целые: int, long, short, byte, sbyte, uint, ulong, ushort
// С плавающей точкой: float, double, decimal
// Символы/строки: char, string
// Логический: bool
// Универсальные: object, dynamic, var


if (x > 0) { ... }
else if (x == 0) { ... }
else { ... }

switch (day)
{
    case "Mon": Console.WriteLine("Понедельник"); break;
    case "Tue": Console.WriteLine("Вторник"); break;
    default: Console.WriteLine("Другой день"); break;
}

// Современный switch (C# 8+)
string msg = day switch
{
    "Mon" => "Понедельник",
    "Tue" => "Вторник",
    _ => "Другой"
};

// Циклы
for (int i = 0; i < 10; i++) { ... }
while (cond) { ... }
do { ... } while (cond);
foreach (var item in items) { ... }