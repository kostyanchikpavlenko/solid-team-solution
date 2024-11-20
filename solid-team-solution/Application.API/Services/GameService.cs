using Application.API.Interfaces;

namespace Application.API.Services;

public class GameService : IGameService
{
    private static readonly int FieldSize = 13;

    public string GetNextMove(string[][] field, int narrowingIn)
    {
        // Найти корабль игрока
        var (myX, myY, myDirection) = FindMyShip(field);
        if (myX == -1 || myY == -1) return "M"; // Ошибка, двигаемся вперед

        // Приоритетная логика:
        // 1. Сбор монет
        var coin = FindNearestEntity(field, myX, myY, "C");
        if (coin != null)
        {
            return GetMoveTowards(myX, myY, myDirection, coin.Value.x, coin.Value.y);
        }

        // 2. Атака врагов
        var enemy = FindNearestEntity(field, myX, myY, "E");
        if (enemy != null && IsEnemyInFireRange(myX, myY, myDirection, enemy.Value.x, enemy.Value.y))
        {
            return "F"; // Стреляем
        }

        // 3. Движение вперед, избегая препятствий
        var (nextX, nextY) = GetNextCell(myX, myY, myDirection);
        if (IsSafeCell(field, nextX, nextY, narrowingIn)) {return "M";}

        // 4. Если не можем двигаться вперед, поворачиваем
        return "R"; // Поворот вправо
    }

    private (int x, int y, char direction) FindMyShip(string[][] field)
    {
        for (int y = 0; y < FieldSize; y++)
        {
            for (int x = 0; x < FieldSize; x++)
            {
                if (field[y][x].StartsWith("P"))
                {
                    return (x, y, field[y][x][1]);
                }
            }
        }

        return (-1, -1, '_'); // Корабль не найден
    }

    private (int x, int y)? FindNearestEntity(string[][] field, int startX, int startY, string entityType)
    {
        int minDistance = int.MaxValue;
        (int x, int y)? nearest = null;

        for (int y = 0; y < FieldSize; y++)
        {
            for (int x = 0; x < FieldSize; x++)
            {
                if (field[y][x].StartsWith(entityType))
                {
                    int distance = Math.Abs(x - startX) + Math.Abs(y - startY);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = (x, y);
                    }
                }
            }
        }

        return nearest;
    }

    private bool IsEnemyInFireRange(int myX, int myY, char myDirection, int enemyX, int enemyY)
    {
        return myDirection switch
        {
            'N' => myX == enemyX && myY > enemyY && myY - enemyY <= 4,
            'S' => myX == enemyX && myY < enemyY && enemyY - myY <= 4,
            'E' => myY == enemyY && myX < enemyX && enemyX - myX <= 4,
            'W' => myY == enemyY && myX > enemyX && myX - enemyX <= 4,
            _ => false
        };
    }

    private string GetMoveTowards(int myX, int myY, char myDirection, int targetX, int targetY)
    {
        if (myY > targetY && myDirection != 'N') return "L"; // Нужно вверх
        if (myY < targetY && myDirection != 'S') return "R"; // Нужно вниз
        if (myX > targetX && myDirection != 'W') return "L"; // Нужно влево
        if (myX < targetX && myDirection != 'E') return "R"; // Нужно вправо
        return "M"; // Если уже направлены правильно, двигаться
    }

    private (int x, int y) GetNextCell(int x, int y, char direction)
    {
        return direction switch
        {
            'N' => (x, y - 1),
            'S' => (x, y + 1),
            'E' => (x + 1, y),
            'W' => (x - 1, y),
            _ => (x, y)
        };
    }

    private bool IsSafeCell(string[][] field, int x, int y, int narrowingIn)
    {
        // Проверка на границы и зону сужения
        if (x < narrowingIn || y < narrowingIn || x >= FieldSize - narrowingIn || y >= FieldSize - narrowingIn)
            return false; // Клетка в зоне сужения
        // Проверка на астероиды и границы
        return x >= 0 && y >= 0 && x < FieldSize && y < FieldSize && field[y][x] == "_";
    }
}