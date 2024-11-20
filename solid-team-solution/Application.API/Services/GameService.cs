using System;
using System.Collections.Generic;
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

        // Step 2: Avoid being too close to narrowing edges
        if (IsNearEdge(playerPosition, narrowingIn))
        {
            return AdjustToSafeZone(field, playerPosition, narrowingIn);
        }

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

    private bool IsNearEdge((int x, int y) position, int narrowingIn)
    {
        int buffer = narrowingIn + 1;
        return position.x <= buffer || position.y <= buffer || position.x >= _fieldSize - buffer - 1 || position.y >= _fieldSize - buffer - 1;
    }

    private string AdjustToSafeZone(string[][] field, (int x, int y) position, int narrowingIn)
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
        int minBoundary = narrowingIn + 1; // Maintain 1-cell buffer from shrinking edges
        int maxBoundary = _fieldSize - narrowingIn - 2;

        // Determine the next move based on current direction and position
        switch (_currentDirection)
        {
            case "N": // Moving North
                if (position.x > minBoundary && field[position.x - 1][position.y] == "_") return "M";
                return "R"; // Rotate right if can't move forward

            case "S": // Moving South
                if (position.x < maxBoundary && field[position.x + 1][position.y] == "_") return "M";
                return "R";

            case "W": // Moving West
                if (position.y > minBoundary && field[position.x][position.y - 1] == "_") return "M";
                return "R";
                
            case "E": // Moving East
                if (position.y < maxBoundary && field[position.x][position.y + 1] == "_") return "M";
                return "R";
        }

        return "R"; // Default to rotating right if no other move is possible
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || y < 0 || x >= _fieldSize || y >= _fieldSize;
    }
}