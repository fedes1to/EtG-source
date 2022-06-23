using System;
//using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	//[InspectorGroupOrder(new string[] { "General", "Animation", "Looping", "Parameters" })]
	[AddComponentMenu("Daikon Forge/Tween/Camera Shake")]
	public class TweenCameraShake : TweenComponentBase
	{
		[SerializeField]
		//[Inspector("Parameters", 0, Label = "Duration")]
		protected float duration = 1f;

		//[Inspector("Parameters", 0, Label = "Magnitude")]
		[SerializeField]
		protected float shakeMagnitude = 0.25f;

		//[Inspector("Parameters", 0, Label = "Speed")]
		[SerializeField]
		protected float shakeSpeed = 13f;

		protected TweenShake tween;

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

		public float ShakeMagnitude
		{
			get
			{
				return shakeMagnitude;
			}
			set
			{
				if (value != shakeMagnitude)
				{
					shakeMagnitude = value;
					Stop();
				}
			}
		}

		public float ShakeSpeed
		{
			get
			{
				return shakeSpeed;
			}
			set
			{
				if (value != shakeSpeed)
				{
					shakeSpeed = value;
					Stop();
				}
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
			if (base.gameObject.GetComponent<Camera>() == null)
			{
				throw new InvalidOperationException("Camera not found");
			}
		}

		protected void configureTween()
		{
			Camera component = base.gameObject.GetComponent<Camera>();
			if (tween == null)
			{
				tween = (TweenShake)component.ShakePosition(true).OnStarted(delegate
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
			tween.SetDelay(startDelay).SetDuration(Duration).SetShakeMagnitude(ShakeMagnitude)
				.SetShakeSpeed(ShakeSpeed);
		}
	}
}
