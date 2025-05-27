namespace LaserTagBox.Model.Shared;

public class TeamScore(string name, Color color, int gamePoints)
{
    public string Name { get; set; } = name;
    public Color Color { get; set; } = color;
    public int GamePoints { get; set; } = gamePoints;
}