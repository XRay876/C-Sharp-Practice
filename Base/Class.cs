class Person
{
    //поле
    private string name;

    //свойство
    public string Name { get; set; } // можно читать и писать
    public string Name
    {
        get { return name; }
        set { name = value;  }
    }
    public int Age { get; private set; } //  публичный get, приватный set
    public Guid Id { get; init; } // доступно только в инициализации

    
 
    // конструктор
    public Person(string name)
    {
        Name = name;
    }

    // метод
    public void SayHello() => Console.WriteLine($"Hi, Im {name}");
}

class Animal
{
    public virtual void Speak() => Console.WriteLine("...");
}

// inheritance 
class Dog : Animal
{
    public override void Speak()
    {
        base.Speak();
    }
}


//interface
interface IRun {
    void Run();
}

class Cat : Animal, IRun
{
    public override void Speak()
    {
        Console.WriteLine("meow");
    }

    public void Run() => Console.WriteLine("Run");
}


// пример для свойств
class Circle
{
    private double radius;
    public double Radius
    {
        get => radius;
        set => radius = value < 0 ? 0 : value; // валидация
    }
    public double Area => Math.PI * radius * radius; // вычисляемое свойство
}