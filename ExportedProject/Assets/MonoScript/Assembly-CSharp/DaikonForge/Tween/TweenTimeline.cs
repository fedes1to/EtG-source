using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class TweenTimeline : TweenBase, IEnumerable<TweenBase>, IEnumerable
	{
		private struct Entry : IComparable<Entry>
		{
			public float Time;

			public TweenBase Tween;

			public int CompareTo(Entry other)
			{
				return Time.CompareTo(other.Time);
			}
		}

		private List<Entry> tweenList = new List<Entry>();

		private List<Entry> pending = new List<Entry>();

		private List<Entry> triggered = new List<Entry>();

		private static List<object> pool = new List<object>();

		public static TweenTimeline Obtain()
		{
			if (pool.Count > 0)
			{
				TweenTimeline result = (TweenTimeline)pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
				return result;
			}
			return new TweenTimeline();
		}

		public void Release()
		{
			Stop();
			Reset();
			pool.Add(this);
		}

		public TweenTimeline Add(float time, params TweenBase[] tweens)
		{
			foreach (TweenBase tweenBase in tweens)
			{
				Duration = Mathf.Max(Delay + Duration, time + tweenBase.Delay + tweenBase.Duration + Delay);
				tweenList.Add(new Entry
				{
					Time = time,
					Tween = tweenBase
				});
			}
			return this;
		}

		public override TweenBase Play()
		{
			pending.AddRange(tweenList);
			pending.Sort();
			triggered.Clear();
			if (Delay > 0f)
			{
				for (int i = 0; i < pending.Count; i++)
				{
					pending[i] = new Entry
					{
						Time = pending[i].Time + Delay,
						Tween = pending[i].Tween
					};
				}
			}
			base.State = TweenState.Playing;
			CurrentTime = 0f;
			startTime = getCurrentTime();
			registerWithTweenManager();
			raiseStarted();
			return this;
		}

		public override TweenBase Stop()
		{
			if (base.State == TweenState.Stopped)
			{
				return this;
			}
			for (int i = 0; i < tweenList.Count; i++)
			{
				tweenList[i].Tween.Stop();
			}
			pending.Clear();
			triggered.Clear();
			return base.Stop();
		}

		public override TweenBase Pause()
		{
			if (base.State != TweenState.Playing && base.State != TweenState.Started)
			{
				return this;
			}
			for (int i = 0; i < triggered.Count; i++)
			{
				triggered[i].Tween.Pause();
			}
			return base.Pause();
		}

		public override TweenBase Resume()
		{
			if (base.State != TweenState.Paused)
			{
				return this;
			}
			for (int i = 0; i < triggered.Count; i++)
			{
				triggered[i].Tween.Resume();
			}
			return base.Resume();
		}

		public override TweenBase SetIsTimeScaleIndependent(bool isTimeScaleIndependent)
		{
			for (int i = 0; i < tweenList.Count; i++)
			{
				TweenBase tween = tweenList[i].Tween;
				tween.SetIsTimeScaleIndependent(isTimeScaleIndependent);
			}
			return base.SetIsTimeScaleIndependent(isTimeScaleIndependent);
		}

		public TweenTimeline SetLoopType(TweenLoopType loopType)
		{
			if (loopType != 0 && loopType != TweenLoopType.Loop)
			{
				throw new ArgumentException("LoopType may only be one of the following values: TweenLoopType.None, TweenLoopType.Loop");
			}
			LoopType = loopType;
			return this;
		}

		public TweenTimeline SetLoopCount(int loopCount)
		{
			LoopCount = loopCount;
			return this;
		}

		internal override float CalculateTotalDuration()
		{
			float num = 0f;
			for (int i = 0; i < tweenList.Count; i++)
			{
				Entry entry = tweenList[i];
				if (entry.Tween != null)
				{
					num = Mathf.Max(num, entry.Time + entry.Tween.CalculateTotalDuration());
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

		protected override void Reset()
		{
			tweenList.Clear();
			pending.Clear();
			triggered.Clear();
			base.Reset();
		}

		public override void Update()
		{
			if (base.State != TweenState.Started && base.State != TweenState.Playing)
			{
				return;
			}
			float num = getCurrentTime() - startTime;
			while (pending.Count > 0)
			{
				Entry item = pending[0];
				if (item.Time > num)
				{
					break;
				}
				pending.RemoveAt(0);
				triggered.Add(item);
				item.Tween.Play();
			}
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
				Rewind();
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

		private bool allTweensComplete()
		{
			if (pending.Count > 0)
			{
				return false;
			}
			for (int i = 0; i < triggered.Count; i++)
			{
				if (triggered[i].Tween.State != 0)
				{
					return false;
				}
			}
			return true;
		}

		public IEnumerator<TweenBase> GetEnumerator()
		{
			return enumerateTweens();
		}

		private IEnumerator<TweenBase> enumerateTweens()
		{
			int index = 0;
			while (index < tweenList.Count)
			{
				yield return tweenList[index++].Tween;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return enumerateTweens();
		}
	}
}
