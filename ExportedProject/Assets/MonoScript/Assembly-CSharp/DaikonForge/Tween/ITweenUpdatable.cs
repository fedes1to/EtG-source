namespace DaikonForge.Tween
{
	public interface ITweenUpdatable
	{
		TweenState State { get; }

		void Update();
	}
}
