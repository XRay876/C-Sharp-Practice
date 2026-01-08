using System;
using System.IO;

class Program
{
    static void Main()
    {
        // объект FileStream -> куча
        using var fs = new FileStream("data.txt", FileMode.Open);
        using var sr = new StreamReader(fs);

        string? line;
        while ((line = sr.ReadLine()) != null) // line хранится в куче
        {
            // parse -> создаётся новый int (на стеке)
            if (int.TryParse(line, out int number)) // number на стеке
            {
                Console.WriteLine(number);
            }
        } // line становится недостижимым -> GC потом очистит
    }
}

// FileStream и StreamReader — объекты → куча. Освобождаются через Dispose() (using var).
// line — ссылка на строку (строка хранится в куче).
// number — значение int, хранится в стеке.
// Когда метод Main заканчивается, стек очищается автоматически.



ReadOnlySpan<char> span = "12345".AsSpan();
if (int.TryParse(span.Slice(0, 3), out int result))
{
    Console.WriteLine(result); // 123
}

//Тут мы работаем с участком памяти без создания новых строк — экономия GC.