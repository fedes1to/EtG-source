using System;
//using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	//[InspectorGroupOrder(new string[] { "General", "Path", "Animation", "Looping" })]
	[AddComponentMenu("Daikon Forge/Tween/Move Along Path")]
	public class TweenObjectPath : TweenComponentBase
	{
	//	[Inspector("Animation", 0, Label = "Duration", Tooltip = "How long the Tween should take to complete the animation")]
		[SerializeField]
		protected float duration = 1f;

	//	[Inspector("Path", 0, Label = "Path", Tooltip = "The path for the object to follow")]
		[SerializeField]
		protected SplineObject path;

		[SerializeField]
	//	[Inspector("Animation", 1, Label = "Orient To Path", Tooltip = "If set to TRUE, will orient the object to face the direction of the path")]
		protected bool orientToPath = true;

		[SerializeField]
		protected TweenDirection playDirection;

		protected Tween<float> tween;

		public float Duration
		{
			get
			{
				return duration;
			}
			set
			{
				duration = Mathf.Max(0f, value);
			}
		}

		public override TweenBase BaseTween
		{
			get
			{
				configureTween();
				return tween;
			}
		}

		public SplineObject Path
		{
			get
			{
				return path;
			}
			set
			{
				cleanup();
				path = value;
			}
		}

		public override TweenState State
		{
			get
			{
				if (tween == null)
				{
					return TweenState.Stopped;
				}
				return tween.State;
			}
		}

		public TweenDirection PlayDirection
		{
			get
			{
				return playDirection;
			}
			set
			{
				playDirection = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public bool OrientToPath
		{
			get
			{
				return orientToPath;
			}
			set
			{
				orientToPath = value;
				if (State != 0)
				{
					Stop();
					tween.Release();
					tween = null;
					Play();
				}
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
			tween.Play();
		}

		public override void Stop()
		{
			if (base.IsPlaying)
			{
				validateTweenConfiguration();
				tween.Stop();
			}
		}

		public override void Pause()
		{
			if (base.IsPlaying)
			{
				validateTweenConfiguration();
				tween.Pause();
			}
		}

		public override void Resume()
		{
			if (base.IsPaused)
			{
				validateTweenConfiguration();
				tween.Resume();
			}
		}

		public override void Rewind()
		{
			validateTweenConfiguration();
			tween.Rewind();
		}

		public override void FastForward()
		{
			validateTweenConfiguration();
			tween.FastForward();
		}

		protected void cleanup()
		{
			if (tween != null)
			{
				tween.Stop();
				tween.Release();
				tween = null;
			}
		}

		protected void validateTweenConfiguration()
		{
			loopCount = Mathf.Max(0, loopCount);
			if (Path == null)
			{
				throw new InvalidOperationException("The Path property cannot be NULL");
			}
		}

		protected void configureTween()
		{
			Path.CalculateSpline();
			if (tween == null)
			{
				tween = (Tween<float>)base.transform.TweenPath(Path.Spline, orientToPath).OnStarted(delegate
				{
					onStarted();
				}).OnStopped(delegate
				{
					onStopped();
				})
					.OnPaused(delegate
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
			Path.CalculateSpline();
			tween.SetDelay(startDelay).SetDuration(duration).SetLoopType(loopType)
				.SetLoopCount(loopCount)
				.SetPlayDirection(playDirection);
		}
	}
}
