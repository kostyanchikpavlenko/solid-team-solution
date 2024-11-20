using Application.API.Interfaces;

namespace Application.API.Services;

public class GameService : IGameService
{
    private const string Empty = "_";
    private const string Asteroid = "A";
    private const string Coin = "C";
    private const string Player = "P";
    private const string Enemy = "E";

    // Размер поля
    private const int FieldSize = 13;

    // Направления движения
    private static readonly (int, int)[] Directions = 
    {
        (0, 1),   // Вправо
        (1, 0),   // Вниз
        (0, -1),  // Влево
        (-1, 0)   // Вверх
    };

    // Метод для получения следующего хода
    public string GetNextMove(string[][] field, int narrowingIn)
    {
        // Поиск текущего положения игрока
        (int row, int col, string direction) = FindPlayerPosition(field);

        // Преобразуем строку направления в индекс направления
        int dirIndex = GetDirectionIndex(direction);
        
        // Проверяем, если narrowingIn меньше 5, создаем буфер от края
        if (narrowingIn < 5)
        {
            if (IsAtEdge(row, col, narrowingIn))
            {
                return MovePerimeter(field, row, col, dirIndex);
            }
        }

        // Проверяем, есть ли враг в пределах 4 клеток по оси
        if (IsAsteroidOrEnemyNearby(field, row, col, dirIndex))
        {
            return "F"; // Осуществляем выстрел
        }

        // Продолжаем движение вдоль периметра
        return MovePerimeter(field, row, col, dirIndex);
    }

    // Поиск позиции игрока
    private (int, int, string) FindPlayerPosition(string[][] field)
    {
        for (int row = 0; row < field.Length; row++)
        {
            for (int col = 0; col < field[row].Length; col++)
            {
                if (field[row][col].StartsWith(Player))
                {
                    return (row, col, field[row][col].Substring(1)); // Возвращаем координаты и направление
                }
            }
        }
        throw new Exception("Player not found");
    }

    // Проверка на астероиды или врагов в пределах 4 клеток по оси
    private bool IsAsteroidOrEnemyNearby(string[][] field, int row, int col, int dirIndex)
    {
        for (int i = 1; i <= 4; i++)
        {
            int newRow = row + Directions[dirIndex].Item1 * i;
            int newCol = col + Directions[dirIndex].Item2 * i;

            if (newRow < 0 || newCol < 0 || newRow >= FieldSize || newCol >= FieldSize) continue;

            string cell = field[newRow][newCol];
            if (cell == Enemy) return true; // Враг найден
        }
        return false;
    }

    // Проверка на нахождение корабля у края поля
    private bool IsAtEdge(int row, int col, int narrowingIn)
    {
        return row <= narrowingIn || col <= narrowingIn || row >= FieldSize - narrowingIn - 1 || col >= FieldSize - narrowingIn - 1;
    }

    // Движение вдоль периметра
    private string MovePerimeter(string[][] field, int row, int col, int dirIndex)
    {
        var nextRow = row + Directions[dirIndex].Item1;
        var nextCol = col + Directions[dirIndex].Item2;

        // Если на следующей клетке астероид или край поля, меняем направление
        if (nextRow < 0 || nextCol < 0 || nextRow >= FieldSize || nextCol >= FieldSize || field[nextRow][nextCol] == Asteroid)
        {
            dirIndex = (dirIndex + 1) % Directions.Length; // Поворот по часовой стрелке
            return "R"; // Повернуть вправо
        }

        // Двигаемся вперед
        return "M";
    }

    // Преобразование направления в индекс
    private int GetDirectionIndex(string direction)
    {
        switch (direction)
        {
            case "N": return 0;
            case "E": return 1;
            case "S": return 2;
            case "W": return 3;
            default: throw new ArgumentException("Invalid direction");
        }
    }
}
