list.ForEach(x => Console.WriteLine(x)); // лямбда



Func<int, int, int> add = (a, b) => a + b;
Action<string> log = Console.WriteLine;

Operation add = (a, b) => a + b;
delegate int Operation(int x, int y);  // делегаты (указатель на метод)




event Action OnClick; // события

class Button
{
    public event Action? Clicked;
    public void Click() => Clicked?.Invoke();
}

// использование
var btn = new Button();
btn.Clicked += () => Console.WriteLine("Клик!");
btn.Click();