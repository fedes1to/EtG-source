using System;
using System.Collections.Generic;
using DaikonForge.Tween.Interpolation;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class Tween<T> : TweenBase, IPoolableObject
	{
		public T StartValue;

		public T EndValue;

		public Interpolator<T> Interpolator;

		public TweenAssignmentCallback<T> Execute;

		public TweenDirection PlayDirection;

		public TweenSyncCallback<T> TweenSyncStartValue;

		public TweenSyncCallback<T> TweenSyncEndValue;

		protected bool assignStartValueBeforeDelay = true;

		private static List<object> pool = new List<object>();

		public T CurrentValue { get; protected set; }

		public bool EndIsOffset { get; protected set; }

		public static Tween<T> Obtain()
		{
			if (pool.Count > 0)
			{
				Tween<T> result = (Tween<T>)pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
				return result;
			}
			return new Tween<T>();
		}

		public void Release()
		{
			Stop();
			Reset();
			pool.Add(this);
		}

		public Tween<T> SetEndRelative(bool relative)
		{
			EndIsOffset = relative;
			return this;
		}

		public Tween<T> SetAutoCleanup(bool autoCleanup)
		{
			AutoCleanup = autoCleanup;
			return this;
		}

		public Tween<T> SetPlayDirection(TweenDirection direction)
		{
			PlayDirection = direction;
			return this;
		}

		public Tween<T> SetEasing(TweenEasingCallback easingFunction)
		{
			Easing = easingFunction;
			return this;
		}

		public Tween<T> SetDuration(float duration)
		{
			Duration = duration;
			return this;
		}

		public Tween<T> SetEndValue(T value)
		{
			EndValue = value;
			return this;
		}

		public Tween<T> SetStartValue(T value)
		{
			T val2 = (StartValue = (CurrentValue = value));
			return this;
		}

		public Tween<T> SetDelay(float seconds)
		{
			return SetDelay(seconds, assignStartValueBeforeDelay);
		}

		public Tween<T> SetDelay(float seconds, bool assignStartValueBeforeDelay)
		{
			Delay = seconds;
			this.assignStartValueBeforeDelay = assignStartValueBeforeDelay;
			return this;
		}

		public Tween<T> SetLoopType(TweenLoopType loopType)
		{
			LoopType = loopType;
			return this;
		}

		public Tween<T> SetLoopCount(int loopCount)
		{
			LoopCount = loopCount;
			return this;
		}

		public Tween<T> SetTimeScaleIndependent(bool timeScaleIndependent)
		{
			IsTimeScaleIndependent = timeScaleIndependent;
			return this;
		}

		public override TweenBase Play()
		{
			if (TweenSyncStartValue != null)
			{
				StartValue = TweenSyncStartValue();
			}
			if (TweenSyncEndValue != null)
			{
				EndValue = TweenSyncEndValue();
			}
			base.Play();
			ensureInterpolator();
			if (assignStartValueBeforeDelay)
			{
				evaluateAtTime(CurrentTime);
			}
			return this;
		}

		public override TweenBase Rewind()
		{
			base.Rewind();
			ensureInterpolator();
			evaluateAtTime(CurrentTime);
			return this;
		}

		public override TweenBase FastForward()
		{
			base.FastForward();
			ensureInterpolator();
			evaluateAtTime(CurrentTime);
			return this;
		}

		public virtual TweenBase Seek(float time)
		{
			CurrentTime = Mathf.Clamp01(time);
			evaluateAtTime(CurrentTime);
			return this;
		}

		public virtual TweenBase ReversePlayDirection()
		{
			PlayDirection = ((PlayDirection == TweenDirection.Forward) ? TweenDirection.Reverse : TweenDirection.Forward);
			return this;
		}

		public Tween<T> SetInterpolator(Interpolator<T> interpolator)
		{
			Interpolator = interpolator;
			return this;
		}

		public Tween<T> OnExecute(TweenAssignmentCallback<T> function)
		{
			Execute = function;
			return this;
		}

		public Tween<T> OnSyncStartValue(TweenSyncCallback<T> function)
		{
			TweenSyncStartValue = function;
			return this;
		}

		public Tween<T> OnSyncEndValue(TweenSyncCallback<T> function)
		{
			TweenSyncEndValue = function;
			return this;
		}

		public override void Update()
		{
			if (base.State == TweenState.Started)
			{
				float currentTime = getCurrentTime();
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
			float deltaTime = getDeltaTime();
			CurrentTime = Mathf.MoveTowards(CurrentTime, 1f, deltaTime / Duration);
			float time = CurrentTime;
			if (Easing != null)
			{
				time = Easing(CurrentTime);
			}
			evaluateAtTime(time);
			if (!(CurrentTime >= 1f))
			{
				return;
			}
			if (LoopType == TweenLoopType.Loop && --LoopCount != 0)
			{
				if (TweenLoopCompleted != null)
				{
					TweenLoopCompleted(this);
				}
				if (EndIsOffset)
				{
					StartValue = CurrentValue;
				}
				Rewind();
				Play();
			}
			else if (LoopType == TweenLoopType.Pingpong && --LoopCount != 0)
			{
				if (TweenLoopCompleted != null)
				{
					TweenLoopCompleted(this);
				}
				ReversePlayDirection();
				Play();
			}
			else
			{
				Stop();
				raiseCompleted();
				if (AutoCleanup)
				{
					Release();
				}
			}
		}

		private void ensureInterpolator()
		{
			if (Interpolator == null)
			{
				Interpolator = Interpolators.Get<T>();
			}
		}

		protected override void Reset()
		{
			base.Reset();
			StartValue = default(T);
			EndValue = default(T);
			CurrentValue = default(T);
			Duration = 1f;
			EndIsOffset = false;
			PlayDirection = TweenDirection.Forward;
			LoopCount = -1;
			assignStartValueBeforeDelay = true;
			Interpolator = null;
			Execute = null;
		}

		private void evaluateAtTime(float time)
		{
			if (Interpolator == null)
			{
				throw new InvalidOperationException(string.Format("No interpolator for type '{0}' has been assigned", typeof(T).Name));
			}
			T val = EndValue;
			if (EndIsOffset)
			{
				val = Interpolator.Add(StartValue, EndValue);
			}
			if (PlayDirection == TweenDirection.Reverse)
			{
				CurrentValue = Interpolator.Interpolate(val, StartValue, time);
			}
			else
			{
				CurrentValue = Interpolator.Interpolate(StartValue, val, time);
			}
			if (Execute != null)
			{
				Execute(CurrentValue);
			}
		}
	}
}
