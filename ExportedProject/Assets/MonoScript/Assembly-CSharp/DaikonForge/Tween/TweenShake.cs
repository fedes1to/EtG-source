using System.Collections.Generic;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class TweenShake : TweenBase, IPoolableObject
	{
		public Vector3 StartValue;

		public float ShakeMagnitude;

		public float ShakeDuration;

		public float ShakeSpeed;

		public TweenAssignmentCallback<Vector3> Execute;

		public TweenCallback ShakeCompleted;

		protected Vector3 currentValue;

		private static List<TweenShake> pool = new List<TweenShake>();

		public TweenShake()
		{
			ShakeSpeed = 10f;
		}

		public TweenShake(Vector3 StartValue, float ShakeMagnitude, float ShakeDuration, float ShakeSpeed, float StartDelay, bool AutoCleanup, TweenAssignmentCallback<Vector3> OnExecute)
		{
			SetStartValue(StartValue).SetShakeMagnitude(ShakeMagnitude).SetDuration(ShakeDuration).SetShakeSpeed(ShakeSpeed)
				.SetDelay(StartDelay)
				.SetAutoCleanup(AutoCleanup)
				.OnExecute(OnExecute);
		}

		public static TweenShake Obtain()
		{
			if (pool.Count > 0)
			{
				TweenShake result = pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
				return result;
			}
			return new TweenShake();
		}

		public void Release()
		{
			Stop();
			StartValue = Vector3.zero;
			currentValue = Vector3.zero;
			CurrentTime = 0f;
			Delay = 0f;
			ShakeCompleted = null;
			Execute = null;
			pool.Add(this);
		}

		public TweenShake SetTimeScaleIndependent(bool timeScaleIndependent)
		{
			IsTimeScaleIndependent = timeScaleIndependent;
			return this;
		}

		public TweenShake SetAutoCleanup(bool autoCleanup)
		{
			AutoCleanup = autoCleanup;
			return this;
		}

		public TweenShake SetDuration(float duration)
		{
			ShakeDuration = duration;
			return this;
		}

		public TweenShake SetStartValue(Vector3 value)
		{
			StartValue = value;
			return this;
		}

		public TweenShake SetDelay(float seconds)
		{
			Delay = seconds;
			return this;
		}

		public TweenShake SetShakeMagnitude(float magnitude)
		{
			ShakeMagnitude = magnitude;
			return this;
		}

		public TweenShake SetShakeSpeed(float speed)
		{
			ShakeSpeed = speed;
			return this;
		}

		public TweenShake OnExecute(TweenAssignmentCallback<Vector3> Execute)
		{
			this.Execute = Execute;
			return this;
		}

		public TweenShake OnComplete(TweenCallback Complete)
		{
			ShakeCompleted = Complete;
			return this;
		}

		public override void Update()
		{
			float currentTime = getCurrentTime();
			if (base.State == TweenState.Started)
			{
				if (currentTime < startTime + Delay)
				{
					return;
				}
				startTime = currentTime;
				CurrentTime = 0f;
				base.State = TweenState.Playing;
			}
			else if (base.State != TweenState.Playing)
			{
				return;
			}
			CurrentTime = Mathf.MoveTowards(CurrentTime, 1f, getDeltaTime() / ShakeDuration);
			float num = 1f - CurrentTime;
			num *= ShakeMagnitude;
			float x = Mathf.PerlinNoise(0.33f, currentTime * ShakeSpeed) * 2f - 1f;
			float y = Mathf.PerlinNoise(0.66f, currentTime * ShakeSpeed) * 2f - 1f;
			float z = Mathf.PerlinNoise(1f, currentTime * ShakeSpeed) * 2f - 1f;
			currentValue = StartValue + new Vector3(x, y, z) * num;
			if (Execute != null)
			{
				Execute(currentValue);
			}
			if (CurrentTime >= 1f)
			{
				Stop();
				raiseCompleted();
				if (AutoCleanup)
				{
					Release();
				}
			}
		}
	}
}
