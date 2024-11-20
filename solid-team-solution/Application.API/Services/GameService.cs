
using Application.API.Interfaces;

namespace Application.API.Services;


public class GameService : IGameService
{
    private const int FiringRange = 4;
    private string _currentDirection = "S"; // Default direction (S = South)
    private int _fieldSize = 13;

    public string GetNextMove(string[][] field, int narrowingIn)
    {
        (int x, int y) playerPosition = FindPlayer(field);

        // Step 1: Fire if an enemy is within range on the same axis
        if (EnemyInFiringRange(field, playerPosition))
        {
            return "F";
        }
     
        if (narrowingIn < 5)
        {
            if (IsNearEdge(playerPosition))
            {
                return AdjustToSafeZone(field, playerPosition);
            }
        }
        
        // Step 2: Avoid being too close to narrowing edges
        // if (IsNearEdge(playerPosition))
        // {
        //     return AdjustToSafeZone(field, playerPosition, narrowingIn);
        // }

        // Step 3: Move in a circular path around the perimeter
        return MoveAlongCircularPerimeter(field, playerPosition, narrowingIn);
    }

    private (int x, int y) FindPlayer(string[][] field)
    {
        for (int i = 0; i < field.Length; i++)
        {
            for (int j = 0; j < field[i].Length; j++)
            {
                if (field[i][j].StartsWith("P")) // Find the player
                {
                    _currentDirection = field[i][j][1].ToString(); // Update current direction
                    return (i, j);
                }
            }
        }
        throw new Exception("Player not found on the field.");
    }

    private bool EnemyInFiringRange(string[][] field, (int x, int y) playerPosition)
    {
        int x = playerPosition.x;
        int y = playerPosition.y;

        for (int i = 1; i <= FiringRange; i++)
        {
            int targetX = x, targetY = y;

            switch (_currentDirection)
            {
                case "N": targetX -= i; break;
                case "S": targetX += i; break;
                case "W": targetY -= i; break;
                case "E": targetY += i; break;
            }

            if (IsOutOfBounds(targetX, targetY) || field[targetX][targetY] == "A") break;
            if (field[targetX][targetY].StartsWith("E")) return true; // Enemy found
        }

        return false;
    }

    private bool IsNearEdge((int x, int y) position)
    {
        int buffer = 1; // Минимальный буфер 1 клетка от границы
        return position.x <= buffer || position.y <= buffer || 
               position.x >= _fieldSize - buffer - 1 || position.y >= _fieldSize - buffer - 1;
    }

    private string AdjustToSafeZone(string[][] field, (int x, int y) position)
    {
        List<(int x, int y, string move)> possibleMoves = new List<(int x, int y, string move)>
        {
            (position.x - 1, position.y, "M"), // Move North
            (position.x + 1, position.y, "M"), // Move South
            (position.x, position.y - 1, "L"), // Rotate Left
            (position.x, position.y + 1, "R")  // Rotate Right
        };

        foreach (var move in possibleMoves)
        {
            if (!IsOutOfBounds(move.x, move.y) && field[move.x][move.y] == "_")
            {
                return move.move;
            }
        }

        return "R"; // Rotate right as fallback
    }

    private string MoveAlongCircularPerimeter(string[][] field, (int x, int y) position, int narrowingIn)
{
    int minBoundary = 1; // Буфер 1 клетка от края
    int maxBoundary = _fieldSize - 2; // Буфер 1 клетка от другой стороны

    // Если narrowingIn меньше 5, сужаем поле
    if (narrowingIn < 5)
    {
        minBoundary = narrowingIn + 1;
        maxBoundary = _fieldSize - narrowingIn - 2;

        // Логика экстренного отхода от краёв с учётом астероидов
        if (position.x <= minBoundary)
        {
            if (IsSafeMove(field, position.x + 1, position.y)) return "M"; // Двигаться вниз
            return RotateToSafeDirection(field, position, new[] { "E", "W" }); // Вправо или влево
        }
        else if (position.x >= maxBoundary)
        {
            if (IsSafeMove(field, position.x - 1, position.y)) return "M"; // Двигаться вверх
            return RotateToSafeDirection(field, position, new[] { "W", "E" }); // Влево или вправо
        }
        else if (position.y <= minBoundary)
        {
            if (IsSafeMove(field, position.x, position.y + 1)) return "M"; // Двигаться вправо
            return RotateToSafeDirection(field, position, new[] { "N", "S" }); // Вверх или вниз
        }
        else if (position.y >= maxBoundary)
        {
            if (IsSafeMove(field, position.x, position.y - 1)) return "M"; // Двигаться влево
            return RotateToSafeDirection(field, position, new[] { "S", "N" }); // Вниз или вверх
        }
    }

    // Обычное движение по кругу с учётом астероидов
    switch (_currentDirection)
    {
        case "N": // Движение на север
            if (position.x > minBoundary && IsSafeMove(field, position.x - 1, position.y))
                return "M";
            return RotateToSafeDirection(field, position, new[] { "E", "W" });

        case "S": // Движение на юг
            if (position.x < maxBoundary && IsSafeMove(field, position.x + 1, position.y))
                return "M";
            return RotateToSafeDirection(field, position, new[] { "W", "E" });

        case "W": // Движение на запад
            if (position.y > minBoundary && IsSafeMove(field, position.x, position.y - 1))
                return "M";
            return RotateToSafeDirection(field, position, new[] { "N", "S" });

        case "E": // Движение на восток
            if (position.y < maxBoundary && IsSafeMove(field, position.x, position.y + 1))
                return "M";
            return RotateToSafeDirection(field, position, new[] { "S", "N" });
    }

    return "R"; // По умолчанию fallback
}

// Проверка безопасного направления для поворота
private string RotateToSafeDirection(string[][] field, (int x, int y) position, string[] directions)
{
    foreach (var direction in directions)
    {
        int targetX = position.x;
        int targetY = position.y;

        switch (direction)
        {
            case "N": targetX--; break;
            case "S": targetX++; break;
            case "W": targetY--; break;
            case "E": targetY++; break;
        }

        if (IsSafeMove(field, targetX, targetY))
        {
            _currentDirection = direction;
            return "R"; // Поворот направо
        }
    }

    return "L"; // fallback поворот в другую сторону
}

// Проверка, является ли клетка безопасной для движения
private bool IsSafeMove(string[][] field, int x, int y)
{
    return !IsOutOfBounds(x, y) && field[x][y] == "_";
}
    
    private bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || y < 0 || x >= _fieldSize || y >= _fieldSize;
    }
}