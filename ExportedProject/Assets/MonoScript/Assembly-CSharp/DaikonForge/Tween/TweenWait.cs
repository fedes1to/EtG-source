using System.Collections.Generic;

namespace DaikonForge.Tween
{
	public class TweenWait : TweenBase
	{
		private static List<TweenWait> pool = new List<TweenWait>();

		private float elapsed;

		public TweenWait(float seconds)
		{
			Delay = seconds;
		}

		public static TweenWait Obtain(float seconds)
		{
			if (pool.Count > 0)
			{
				TweenWait tweenWait = pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
				tweenWait.Delay = seconds;
				return tweenWait;
			}
			TweenWait tweenWait2 = new TweenWait(seconds);
			tweenWait2.AutoCleanup = true;
			return tweenWait2;
		}

		public void Release()
		{
			if (!pool.Contains(this))
			{
				Reset();
				pool.Add(this);
			}
		}

		public override TweenBase Rewind()
		{
			elapsed = 0f;
			return base.Rewind();
		}

		public override TweenBase FastForward()
		{
			elapsed = Delay;
			return base.FastForward();
		}

		public override void Update()
		{
			if (base.State != TweenState.Playing && base.State != TweenState.Started)
			{
				return;
			}
			if (base.State == TweenState.Started)
			{
				elapsed = 0f;
				startTime = getCurrentTime();
				base.State = TweenState.Playing;
				return;
			}
			elapsed += getDeltaTime();
			if (elapsed >= Delay)
			{
				Stop();
				raiseCompleted();
			}
		}
	}
}
