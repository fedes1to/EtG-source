using System;

[Serializable]
public class FleePlayerData
{
	public float StartDistance = 6f;

	public float DeathDistance = 9f;

	public float StopDistance = 12f;

	public PlayerController Player { get; set; }
}
