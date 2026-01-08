// reference type classes:
class ClassExample { }

abstract class AbstractExample { } // нельзя создать экземпляр, служит как «каркас».

static class StaticExample { } // нельзя создать экземпляр, содержит только static члены.

sealed class SealedExample { } // от него нельзя наследоваться


// value type classes:
// Нельзя наследоваться, но можно реализовывать интерфейсы.
// Хороши для лёгких объектов (Point, DateTime).
struct StructExample { };

// Записи:
record RecordExample { } //ссылочный тип с value-based equality (сравнение по содержимому)
record struct RSExample { } // значимый тип, но с семантикой records (удобное сравнение и with).

// enum
enum Status
{
    New, Progress, Done
}
// Status s = Status.InProgress;

//interface 
// Контракт (методы/свойства без реализации).
// Класс или структура может реализовывать несколько интерфейсов.
interface InterfaceExample { }

// delegate
// тип который укзазывает на метод
delegate int Operate(int x, int y);

//event
// обертка над делегатом
// для паттерна *наблюдатель*, например, нажатие кнопки
event Action OnClick;

// tuple (кортежи)
// var tuple = (5, 10);