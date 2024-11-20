using Application.API.Interfaces;

namespace Application.API.Services;

public class GameService : IGameService
{
    private static readonly int FieldSize = 13;

    public string GetNextMove(string[][] field, int narrowingIn)
    {
        // Найти корабль игрока
        var (myX, myY, myDirection) = FindMyShip(field);

        if (myX == -1 || myY == -1) 
            throw new Exception("Ship was not found"); // Ошибка, корабль не найден

        // 1. Проверяем, есть ли астероид перед кораблем
        if (IsAsteroidAhead(field, myX, myY, myDirection))
        {
            // Ищем безопасное направление
            var safeDirection = FindSafeDirection(field, myX, myY, myDirection);
            if (safeDirection != myDirection)
            {
                return "R"; // Поворачиваем вправо, чтобы избежать столкновения
            }

            return "M"; // Если безопасное направление впереди, продолжаем движение
        }

        // 2. Стреляем, если враг в зоне досягаемости
        var enemy = FindNearestEntity(field, myX, myY, "E");
        if (enemy != null && IsEnemyInFireRange(field, myX, myY, myDirection, enemy.Value.x, enemy.Value.y))
        {
            return "F"; // Стреляем
        }

        // 3. Двигаемся к ближайшей монете
        var coin = FindNearestEntity(field, myX, myY, "C");
        if (coin != null)
        {
            return GetMoveTowards(myX, myY, myDirection, coin.Value.x, coin.Value.y);
        }

        // 4. Движение вперед, если путь безопасен
        var (nextX, nextY) = GetNextCell(myX, myY, myDirection);
        if (IsSafeCell(field, nextX, nextY, narrowingIn))
        {
            return "M"; // Двигаемся вперед
        }

        // 5. Если не можем двигаться вперед, поворачиваем
        return "R"; // Поворот вправо
    }

    private bool IsAsteroidAhead(string[][] field, int myX, int myY, char myDirection)
    {
        var (nextX, nextY) = GetNextCell(myX, myY, myDirection);
        return IsWithinBounds(nextX, nextY) && field[nextY][nextX] == "A";
    }

    private char FindSafeDirection(string[][] field, int myX, int myY, char myDirection)
    {
        var directions = new[] { 'N', 'E', 'S', 'W' };

        foreach (var direction in directions)
        {
            if (direction == myDirection) continue; // Пропускаем текущее направление

            var (nextX, nextY) = GetNextCell(myX, myY, direction);
            if (IsWithinBounds(nextX, nextY) && IsSafeCell(field, nextX, nextY, 0))
            {
                return direction; // Возвращаем первое безопасное направление
            }
        }

        return myDirection; // Если безопасного направления нет, возвращаем текущее
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

    private bool IsSafeCell(string[][] field, int x, int y, int narrowingIn)
    {
        if (x < narrowingIn || y < narrowingIn || x >= FieldSize - narrowingIn || y >= FieldSize - narrowingIn)
            return false; // Клетка в зоне сужения
        return IsWithinBounds(x, y) && field[y][x] == "_"; // Свободная клетка
    }

    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < FieldSize && y < FieldSize;
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
    
    private bool IsEnemyInFireRange(string[][] field, int myX, int myY, char myDirection, int enemyX, int enemyY)
    {
        const int fireRange = 4;

        // Проверяем, находится ли враг в пределах радиуса и на одной оси
        if (myDirection == 'N' && myX == enemyX && myY > enemyY && (myY - enemyY) <= fireRange)
        {
            for (int y = myY - 1; y > enemyY; y--)
            {
                if (field[y][myX] != "_" && field[y][myX] != "C") return false; // Препятствие на пути
            }
            return true;
        }
        if (myDirection == 'S' && myX == enemyX && myY < enemyY && (enemyY - myY) <= fireRange)
        {
            for (int y = myY + 1; y < enemyY; y++)
            {
                if (field[y][myX] != "_" && field[y][myX] != "C") return false; // Препятствие на пути
            }
            return true;
        }
        if (myDirection == 'E' && myY == enemyY && myX < enemyX && (enemyX - myX) <= fireRange)
        {
            for (int x = myX + 1; x < enemyX; x++)
            {
                if (field[myY][x] != "_" && field[myY][x] != "C") return false; // Препятствие на пути
            }
            return true;
        }
        if (myDirection == 'W' && myY == enemyY && myX > enemyX && (myX - enemyX) <= fireRange)
        {
            for (int x = myX - 1; x > enemyX; x--)
            {
                if (field[myY][x] != "_" && field[myY][x] != "C") return false; // Препятствие на пути
            }
            return true;
        }

        return false;
    }

}
