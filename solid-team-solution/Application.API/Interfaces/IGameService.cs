namespace Application.API.Interfaces;

public interface IGameService
{
    public string GetNextMove(string[][] field, int narrowingIn);
}