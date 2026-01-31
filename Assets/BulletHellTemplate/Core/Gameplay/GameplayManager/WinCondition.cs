namespace BulletHellTemplate
{
    public enum WinCondition
    {
        SurvivalTime,           // Win by surviving until the time runs out
        KillBoss,               // Win by killing a specific boss monster within the time limit
        SurvivalTimeAndKillBoss // Win by surviving until the time runs out and then killing the final boss
    }
}