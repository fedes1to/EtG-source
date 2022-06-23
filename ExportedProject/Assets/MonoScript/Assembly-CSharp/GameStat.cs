public class GameStat
{
	public string statName = string.Empty;

	public float statValue;

	public GameStat(string name, float val)
	{
		statName = name;
		statValue = val;
	}

	public void Modify(float change)
	{
		statValue += change;
	}
}
