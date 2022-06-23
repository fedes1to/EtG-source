using Brave.BulletScript;

public abstract class BulletKingDirectedFire : Script
{
	public bool IsHard
	{
		get
		{
			return this is BulletKingDirectedFireHard;
		}
	}

	protected void DirectedShots(float x, float y, float direction)
	{
		direction -= 90f;
		if (IsHard)
		{
			direction += 15f;
		}
		Fire(new Offset(x, y, 0f, string.Empty), new Direction(direction), new Speed((!IsHard) ? 12 : 16), new Bullet("directedfire"));
		if (IsHard)
		{
			direction += 30f;
			Fire(new Offset(x, y, 0f, string.Empty), new Direction(direction), new Speed((!IsHard) ? 12 : 16), new Bullet("directedfire"));
		}
	}
}
