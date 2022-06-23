using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class TweenGroup : TweenBase, IPoolableObject, IEnumerable<TweenBase>, IEnumerable
	{
		private static List<TweenGroup> pool = new List<TweenGroup>();

		public TweenGroupMode Mode;

		protected List<TweenBase> tweenList = new List<TweenBase>();

		protected TweenBase currentTween;

		protected int currentIndex;

		protected bool autoCleanup;

		public static TweenGroup Obtain()
		{
			if (pool.Count > 0)
			{
				TweenGroup result = pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
				return result;
			}
			return new TweenGroup();
		}

		public void Release()
		{
			Stop();
			if (!pool.Contains(this))
			{
				Reset();
				pool.Add(this);
			}
		}

		public TweenGroup SetAutoCleanup(bool autoCleanup)
		{
			AutoCleanup = true;
			return this;
		}

		public override TweenBase SetIsTimeScaleIndependent(bool isTimeScaleIndependent)
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				tweenList[i].SetIsTimeScaleIndependent(isTimeScaleIndependent);
			}
			return base.SetIsTimeScaleIndependent(isTimeScaleIndependent);
		}

		public TweenGroup SetMode(TweenGroupMode mode)
		{
			Mode = mode;
			return this;
		}

		public TweenGroup SetDelay(float seconds)
		{
			Delay = seconds;
			return this;
		}

		public TweenGroup SetLoopType(TweenLoopType loopType)
		{
			if (loopType != 0 && loopType != TweenLoopType.Loop)
			{
				throw new ArgumentException("LoopType may only be one of the following values: TweenLoopType.None, TweenLoopType.Loop");
			}
			LoopType = loopType;
			return this;
		}

		public TweenGroup SetLoopCount(int loopCount)
		{
			LoopCount = loopCount;
			return this;
		}

		public TweenGroup AppendTween(params TweenBase[] tweens)
		{
			if (tweens == null || tweens.Length == 0)
			{
				throw new ArgumentException("You must provide at least one Tween");
			}
			tweenList.AddRange(tweens);
			return this;
		}

		public TweenGroup AppendDelay(float seconds)
		{
			tweenList.Add(TweenWait.Obtain(seconds));
			return this;
		}

		public TweenGroup ClearTweens()
		{
			tweenList.Clear();
			return this;
		}

		public override TweenBase Play()
		{
			if (LoopType != 0 && LoopType != TweenLoopType.Loop)
			{
				throw new ArgumentException("LoopType may only be one of the following values: TweenLoopType.None, TweenLoopType.Loop");
			}
			currentTween = null;
			currentIndex = -1;
			base.Play();
			return this;
		}

		public override TweenBase Stop()
		{
			if (base.State != 0)
			{
				for (int i = 0; i < tweenList.Count; i++)
				{
					tweenList[i].Stop();
				}
				currentTween = null;
				currentIndex = -1;
			}
			return base.Stop();
		}

		public override TweenBase Pause()
		{
			if (base.State == TweenState.Playing || base.State == TweenState.Started)
			{
				if (Mode == TweenGroupMode.Concurrent)
				{
					pauseAllTweens();
				}
				else if (currentTween != null)
				{
					currentTween.Pause();
				}
			}
			return base.Pause();
		}

		public override TweenBase Resume()
		{
			if (base.State == TweenState.Paused)
			{
				if (Mode == TweenGroupMode.Concurrent)
				{
					resumeAllTweens();
				}
				else if (currentTween != null)
				{
					currentTween.Resume();
				}
			}
			base.Resume();
			return this;
		}

		public override TweenBase Rewind()
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				tweenList[i].Rewind();
			}
			currentTween = null;
			currentIndex = -1;
			return base.Rewind();
		}

		public override void Update()
		{
			if (tweenList.Count == 0)
			{
				return;
			}
			if (base.State == TweenState.Started)
			{
				float currentTime = getCurrentTime();
				if (currentTime < startTime + Delay)
				{
					return;
				}
				if (Mode == TweenGroupMode.Concurrent)
				{
					startAllTweens();
				}
				else if (!nextTween())
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
			if (Mode == TweenGroupMode.Concurrent)
			{
				if (!allTweensComplete())
				{
					return;
				}
				if (LoopType == TweenLoopType.Loop && --LoopCount != 0)
				{
					if (TweenLoopCompleted != null)
					{
						TweenLoopCompleted(this);
					}
					if (base.State == TweenState.Playing)
					{
						Rewind();
						Play();
					}
				}
				else
				{
					onGroupComplete();
				}
			}
			else if (currentTween.State == TweenState.Stopped && !nextTween())
			{
				Stop();
				raiseCompleted();
			}
		}

		protected override void Reset()
		{
			Stop();
			if (AutoCleanup)
			{
				cleanUp();
			}
			base.Reset();
			Mode = TweenGroupMode.Sequential;
			AutoCleanup = false;
			tweenList.Clear();
		}

		internal override float CalculateTotalDuration()
		{
			float num = 0f;
			if (Mode == TweenGroupMode.Sequential)
			{
				for (int i = 0; i < tweenList.Count; i++)
				{
					TweenBase tweenBase = tweenList[i];
					if (tweenBase != null)
					{
						num += tweenBase.CalculateTotalDuration();
					}
				}
			}
			else
			{
				for (int j = 0; j < tweenList.Count; j++)
				{
					TweenBase tweenBase2 = tweenList[j];
					if (tweenBase2 != null)
					{
						num = Mathf.Max(num, tweenBase2.CalculateTotalDuration());
					}
				}
			}
			if (LoopCount > 0)
			{
				num *= (float)LoopCount;
			}
			else if (LoopType != 0)
			{
				num = float.PositiveInfinity;
			}
			return Delay + num;
		}

		private void onGroupComplete()
		{
			Stop();
			raiseCompleted();
			if (autoCleanup)
			{
				cleanUp();
			}
		}

		private void startAllTweens()
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				TweenBase tweenBase = tweenList[i];
				if (tweenBase != null)
				{
					tweenBase.Play();
				}
			}
		}

		private void pauseAllTweens()
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				TweenBase tweenBase = tweenList[i];
				if (tweenBase != null)
				{
					tweenBase.Pause();
				}
			}
		}

		private void resumeAllTweens()
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				TweenBase tweenBase = tweenList[i];
				if (tweenBase != null)
				{
					tweenBase.Resume();
				}
			}
		}

		private bool nextTween()
		{
			if (Mode == TweenGroupMode.Concurrent)
			{
				return true;
			}
			if (base.State == TweenState.Started)
			{
				currentIndex = 0;
				currentTween = tweenList[currentIndex];
				currentTween.Play();
				return true;
			}
			if (currentIndex == tweenList.Count - 1)
			{
				if (LoopType != TweenLoopType.Loop || --LoopCount == 0)
				{
					return false;
				}
				if (TweenLoopCompleted != null)
				{
					TweenLoopCompleted(this);
				}
				currentIndex = 0;
				if (base.State == TweenState.Stopped)
				{
					return false;
				}
			}
			else
			{
				currentIndex++;
			}
			currentTween = tweenList[currentIndex];
			currentTween.Play();
			return true;
		}

		private bool allTweensComplete()
		{
			if (Mode == TweenGroupMode.Sequential && currentTween != null)
			{
				return currentTween.State == TweenState.Stopped;
			}
			for (int i = 0; i < tweenList.Count; i++)
			{
				if (tweenList[i].State != 0)
				{
					return false;
				}
			}
			return true;
		}

		private void cleanUp()
		{
			int num = 0;
			while (num < tweenList.Count)
			{
				TweenBase tweenBase = tweenList[num];
				if (tweenBase != null && tweenBase.AutoCleanup)
				{
					if (tweenBase is IPoolableObject)
					{
						((IPoolableObject)tweenBase).Release();
					}
					tweenList.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}

		public IEnumerator<TweenBase> GetEnumerator()
		{
			return tweenList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return tweenList.GetEnumerator();
		}
	}
}
