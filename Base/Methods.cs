int Add(int x, int y)
{
    return x + y;
}

//out используется для передачи переменной в метод так, чтобы метод мог изменить её значение и вернуть это изменённое значение вызывающему коду
bool TryParse(string s, out int number)
{
    return int.TryParse(s, out number);
}

// Another example for "out"
bool TryDivide(int a, int b, out int result)
{
    if (b == 0)
    {
        result = 0;
        return false;
    }
    result = a / b;
    return true;
}

if (TryDivide(10, 2, out int res)) Console.WriteLine(res);


void Log(string msg, bool isError = false)
{

}

int Sum(params int[] numbers) => numbers.Sum();
