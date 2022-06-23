using System;
using System.Collections.Generic;
//using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	//[InspectorGroupOrder(new string[] { "General", "Animation", "Looping", "Tweens" })]
	[AddComponentMenu("Daikon Forge/Tween/Group")]
	public class TweenComponentGroup : TweenComponentBase
	{
		[SerializeField]
		//[Inspector("General", 1, Label = "Mode")]
		protected TweenGroupMode groupMode;

		//[Inspector("Tweens", 0, Label = "Tweens")]
		[SerializeField]
		protected List<TweenPlayableComponent> tweens = new List<TweenPlayableComponent>();

		protected TweenGroup group;

		public override TweenBase BaseTween
		{
			get
			{
				configureTween();
				return group;
			}
		}

		public List<TweenPlayableComponent> Tweens
		{
			get
			{
				return tweens;
			}
		}

		public TweenGroupMode GroupMode
		{
			get
			{
				return groupMode;
			}
			set
			{
				if (value != groupMode)
				{
					groupMode = value;
					Stop();
				}
			}
		}

		public override TweenState State
		{
			get
			{
				if (group == null)
				{
					return TweenState.Stopped;
				}
				return group.State;
			}
		}

		public virtual void OnApplicationQuit()
		{
			cleanup();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			cleanup();
		}

		public override void Play()
		{
			if (State != 0)
			{
				Stop();
			}
			configureTween();
			validateTweenConfiguration();
			group.Play();
		}

		public override void Stop()
		{
			if (base.IsPlaying)
			{
				validateTweenConfiguration();
				group.Stop();
			}
		}

		public override void Pause()
		{
			if (base.IsPlaying)
			{
				validateTweenConfiguration();
				group.Pause();
			}
		}

		public override void Resume()
		{
			if (base.IsPaused)
			{
				validateTweenConfiguration();
				group.Resume();
			}
		}

		public override void Rewind()
		{
			validateTweenConfiguration();
			group.Rewind();
		}

		public override void FastForward()
		{
			validateTweenConfiguration();
			group.FastForward();
		}

		protected void cleanup()
		{
			if (group != null)
			{
				group.Stop();
				group.Release();
				group = null;
			}
		}

		protected void validateTweenConfiguration()
		{
			loopCount = Mathf.Max(0, loopCount);
			if (loopType != 0 && loopType != TweenLoopType.Loop)
			{
				throw new ArgumentException("LoopType may only be one of the following values: TweenLoopType.None, TweenLoopType.Loop");
			}
		}

		protected void configureTween()
		{
			if (group == null)
			{
				group = (TweenGroup)new TweenGroup().OnStarted(delegate
				{
					onStarted();
				}).OnStopped(delegate
				{
					onStopped();
				}).OnPaused(delegate
				{
					onPaused();
				})
					.OnResumed(delegate
					{
						onResumed();
					})
					.OnLoopCompleted(delegate
					{
						onLoopCompleted();
					})
					.OnCompleted(delegate
					{
						onCompleted();
					});
			}
			group.ClearTweens().SetMode(groupMode).SetDelay(startDelay)
				.SetLoopType(loopType)
				.SetLoopCount(loopCount);
			for (int i = 0; i < tweens.Count; i++)
			{
				TweenPlayableComponent tweenPlayableComponent = tweens[i];
				if (tweenPlayableComponent != null)
				{
					tweenPlayableComponent.AutoRun = false;
					TweenBase baseTween = tweenPlayableComponent.BaseTween;
					if (baseTween == null)
					{
						Debug.LogError("Base tween not set", tweenPlayableComponent);
						continue;
					}
					group.AppendTween(baseTween);
				}
			}
		}
	}
}
