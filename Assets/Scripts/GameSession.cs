public static class GameSession
{
    public static int Score { get; private set; }
    public static int Lives { get; private set; }
    public static bool DidWin { get; private set; }

    public static void ResetForNewGame()
    {
        Score = 0;
        Lives = 3;
        DidWin = false;
    }

    public static void AddScore(int points)
    {
        Score += points;
    }

    public static void AddLife(int amount = 1)
    {
        Lives += amount;
    }

    public static void LoseLife(int amount = 1)
    {
        Lives -= amount;
        if (Lives < 0)
        {
            Lives = 0;
        }
    }

    public static void SetResult(bool didWin)
    {
        DidWin = didWin;
    }
}
