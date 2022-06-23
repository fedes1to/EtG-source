using System;
using System.Collections;
using UnityEngine;

namespace DaikonForge.Tween
{
	public abstract class TweenBase : ITweenUpdatable
	{
		public string Name;

		public float CurrentTime;

		public float Duration;

		public float Delay;

		public TweenLoopType LoopType;

		public int LoopCount;

		public TweenEasingCallback Easing;

		public bool AutoCleanup;

		public bool IsTimeScaleIndependent;

		public TweenCallback TweenStarted;

		public TweenCallback TweenStopped;

		public TweenCallback TweenPaused;

		public TweenCallback TweenResumed;

		public TweenCallback TweenCompleted;

		public TweenCallback TweenLoopCompleted;

		protected float startTime;

		protected bool registered;

		public float ElapsedTime
		{
			get
			{
				return getCurrentTime() - startTime;
			}
		}

		public TweenState State { get; protected set; }

		public virtual TweenBase Play()
		{
			State = TweenState.Started;
			CurrentTime = 0f;
			startTime = getCurrentTime();
			registerWithTweenManager();
			raiseStarted();
			return this;
		}

		public virtual TweenBase Pause()
		{
			if (State != TweenState.Playing && State != TweenState.Started)
			{
				return this;
			}
			State = TweenState.Paused;
			raisePaused();
			return this;
		}

		public virtual TweenBase Resume()
		{
			if (State != TweenState.Paused)
			{
				return this;
			}
			State = TweenState.Playing;
			raiseResumed();
			return this;
		}

		public virtual TweenBase Stop()
		{
			if (State == TweenState.Stopped)
			{
				return this;
			}
			unregisterWithTweenManager();
			State = TweenState.Stopped;
			raiseStopped();
			return this;
		}

		public virtual TweenBase Rewind()
		{
			CurrentTime = 0f;
			startTime = getCurrentTime();
			return this;
		}

		public virtual TweenBase FastForward()
		{
			CurrentTime = 1f;
			return this;
		}

		public virtual IEnumerator WaitForCompletion()
		{
			do
			{
				yield return null;
			}
			while (State != 0);
		}

		public virtual TweenBase Chain(TweenBase tween)
		{
			return Chain(tween, null);
		}

		public virtual TweenBase Chain(TweenBase tween, Action initFunction)
		{
			if (tween == null)
			{
				throw new ArgumentNullException("tween");
			}
			TweenCallback completedCallback = TweenCompleted;
			TweenCompleted = delegate(TweenBase sender)
			{
				if (completedCallback != null)
				{
					completedCallback(sender);
				}
				if (initFunction != null)
				{
					initFunction();
				}
				tween.Play();
			};
			return tween;
		}

		internal virtual float CalculateTotalDuration()
		{
			float num = Delay + Duration;
			if (LoopCount > 0)
			{
				num *= (float)LoopCount;
			}
			else if (LoopType != 0)
			{
				num = float.PositiveInfinity;
			}
			return num;
		}

		public virtual TweenBase SetIsTimeScaleIndependent(bool isTimeScaleIndependent)
		{
			IsTimeScaleIndependent = isTimeScaleIndependent;
			return this;
		}

		public abstract void Update();

		protected virtual void Reset()
		{
			Easing = TweenEasingFunctions.Linear;
			LoopType = TweenLoopType.None;
			CurrentTime = 0f;
			Delay = 0f;
			AutoCleanup = false;
			IsTimeScaleIndependent = false;
			startTime = 0f;
			TweenLoopCompleted = null;
			TweenCompleted = null;
			TweenPaused = null;
			TweenResumed = null;
			TweenStarted = null;
			TweenStopped = null;
		}

		protected void registerWithTweenManager()
		{
			if (!registered)
			{
				TweenManager.Instance.RegisterTween(this);
				registered = true;
			}
		}

		protected void unregisterWithTweenManager()
		{
			if (registered)
			{
				TweenManager.Instance.UnregisterTween(this);
				registered = false;
			}
		}

		protected float getTimeElapsed()
		{
			if (State == TweenState.Playing || State == TweenState.Started)
			{
				return Mathf.Min(getCurrentTime() - startTime, Duration);
			}
			return 0f;
		}

		protected float getCurrentTime()
		{
			if (IsTimeScaleIndependent)
			{
				return TweenManager.Instance.realTimeSinceStartup;
			}
			return Time.time;
		}

		protected float getDeltaTime()
		{
			if (IsTimeScaleIndependent)
			{
				return TweenManager.realDeltaTime;
			}
			return BraveTime.DeltaTime;
		}

		public TweenBase OnLoopCompleted(TweenCallback function)
		{
			TweenLoopCompleted = function;
			return this;
		}

		public TweenBase OnCompleted(TweenCallback function)
		{
			TweenCompleted = function;
			return this;
		}

		public TweenBase OnPaused(TweenCallback function)
		{
			TweenPaused = function;
			return this;
		}

		public TweenBase OnResumed(TweenCallback function)
		{
			TweenResumed = function;
			return this;
		}

		public TweenBase OnStarted(TweenCallback function)
		{
			TweenStarted = function;
			return this;
		}

		public TweenBase OnStopped(TweenCallback function)
		{
			TweenStopped = function;
			return this;
		}

		public virtual TweenBase Wait(float seconds)
		{
			return Chain(new TweenWait(seconds));
		}

		protected virtual void raisePaused()
		{
			if (TweenPaused != null)
			{
				TweenPaused(this);
			}
		}

		protected virtual void raiseResumed()
		{
			if (TweenResumed != null)
			{
				TweenResumed(this);
			}
		}

		protected virtual void raiseStarted()
		{
			if (TweenStarted != null)
			{
				TweenStarted(this);
			}
		}

		protected virtual void raiseStopped()
		{
			if (TweenStopped != null)
			{
				TweenStopped(this);
			}
		}

		protected virtual void raiseCompleted()
		{
			if (TweenCompleted != null)
			{
				TweenCompleted(this);
			}
		}

		public override string ToString()
		{
			return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
		}
	}
}
