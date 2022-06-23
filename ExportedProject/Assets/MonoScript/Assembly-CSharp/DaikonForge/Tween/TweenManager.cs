using System.Collections.Generic;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class TweenManager : MonoBehaviour
	{
		private static TweenManager instance;

		internal static float realDeltaTime;

		private static float lastFrameTime;

		internal float realTimeSinceStartup;

		private List<ITweenUpdatable> playingTweens = new List<ITweenUpdatable>();

		private Queue<ITweenUpdatable> addTweenQueue = new Queue<ITweenUpdatable>();

		private Queue<ITweenUpdatable> removeTweenQueue = new Queue<ITweenUpdatable>();

		public static TweenManager Instance
		{
			get
			{
				lock (typeof(TweenManager))
				{
					if (instance == null)
					{
						GameObject gameObject = new GameObject("_TweenManager_");
						gameObject.hideFlags = HideFlags.HideInHierarchy;
						instance = gameObject.AddComponent<TweenManager>();
					}
					return instance;
				}
			}
		}

		static TweenManager()
		{
			lastFrameTime = 0f;
			realDeltaTime = 0f;
		}

		public void RegisterTween(ITweenUpdatable tween)
		{
			lock (playingTweens)
			{
				if (playingTweens.Contains(tween) && !removeTweenQueue.Contains(tween))
				{
					return;
				}
				lock (addTweenQueue)
				{
					addTweenQueue.Enqueue(tween);
				}
			}
		}

		public void UnregisterTween(ITweenUpdatable tween)
		{
			lock (removeTweenQueue)
			{
				if (playingTweens.Contains(tween) && !removeTweenQueue.Contains(tween))
				{
					removeTweenQueue.Enqueue(tween);
				}
			}
		}

		public void Clear()
		{
			lock (playingTweens)
			{
				playingTweens.Clear();
				removeTweenQueue.Clear();
			}
		}

		public virtual void OnDestroy()
		{
			instance = null;
		}

		public virtual void Update()
		{
			realTimeSinceStartup = Time.realtimeSinceStartup;
			realDeltaTime = realTimeSinceStartup - lastFrameTime;
			lastFrameTime = realTimeSinceStartup;
			lock (playingTweens)
			{
				lock (addTweenQueue)
				{
					while (addTweenQueue.Count > 0)
					{
						playingTweens.Add(addTweenQueue.Dequeue());
					}
				}
				lock (removeTweenQueue)
				{
					while (removeTweenQueue.Count > 0)
					{
						playingTweens.Remove(removeTweenQueue.Dequeue());
					}
				}
				int count = playingTweens.Count;
				for (int i = 0; i < count; i++)
				{
					ITweenUpdatable tweenUpdatable = playingTweens[i];
					TweenState state = tweenUpdatable.State;
					if (state == TweenState.Playing || state == TweenState.Started)
					{
						tweenUpdatable.Update();
					}
				}
			}
		}
	}
}
