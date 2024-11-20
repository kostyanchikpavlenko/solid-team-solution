using Application.API.Interfaces;

namespace Application.API.Services;

public class GameService : IGameService
{
    private static readonly int FieldSize = 13;

    public string GetNextMove(string[][] field, int narrowingIn)
    {
        // Знайти координати корабля
        var (myX, myY, myDirection) = FindMyShip(field);
        if (myX == -1 || myY == -1) return "M"; // У разі помилки рухаємося вперед

        // Спроба рухатися вперед
        var (nextX, nextY) = GetNextCell(myX, myY, myDirection);
        if (IsSafeCell(field, nextX, nextY, narrowingIn)) return "M";

        // Якщо не можемо рухатися вперед, спробуємо повернути
        return "M"; // Повертаємо праворуч
    }

    private static (int x, int y, char direction) FindMyShip(string[][] field)
    {
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (field[x][y].StartsWith("P"))
                {
                    return (x, y, field[x][y][1]);
                }
            }
        }
        return (-1, -1, '_'); // Корабель не знайдено
    }

    private static (int x, int y) GetNextCell(int x, int y, char direction)
    {
        return direction switch
        {
            'N' => (x - 1, y),
            'S' => (x + 1, y),
            'E' => (x, y + 1),
            'W' => (x, y - 1),
            _ => (x, y)
        };
    }

    private static bool IsSafeCell(string[][] field, int x, int y, int narrowingIn)
    {
        // Перевірка на межі поля та зону звуження
        if (x < narrowingIn || y < narrowingIn || x >= FieldSize - narrowingIn || y >= FieldSize - narrowingIn)
            return false; // Клітина знаходиться у зоні звуження
        // Перевірка на астероїди
        return x >= 0 && y >= 0 && x < FieldSize && y < FieldSize && field[x][y] == "_";
    }
}