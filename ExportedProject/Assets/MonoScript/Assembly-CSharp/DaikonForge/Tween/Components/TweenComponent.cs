using System;
using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	[InspectorGroupOrder(new string[] { "General", "Animation", "Looping", "Values" })]
	public abstract class TweenComponent<T> : TweenComponentBase
	{
		[Inspector("Animation", Order = 4, Tooltip = "How long the Tween should take to complete the animation")]
		[SerializeField]
		protected float duration = 1f;

		[Inspector("Animation", Order = 2, Tooltip = "The type of easing, if any, to apply to the animation")]
		[SerializeField]
		protected EasingType easingType;

		[Inspector("Animation", Order = 3, Label = "Curve", Tooltip = "An animation curve can be used to modify the animation timeline")]
		[SerializeField]
		protected AnimationCurve animCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

		[SerializeField]
		[Inspector("Animation", Order = 5, Label = "Direction")]
		protected TweenDirection playDirection;

		[Inspector("Values", Order = 0)]
		[SerializeField]
		protected TweenStartValueType startValueType;

		[Inspector("Values", Order = 1)]
		[SerializeField]
		protected T startValue;

		[SerializeField]
		[Inspector("Values", Order = 2)]
		protected TweenEndValueType endValueType;

		[SerializeField]
		[Inspector("Values", Order = 3)]
		protected T endValue;

		protected Tween<T> tween;

		public override TweenBase BaseTween
		{
			get
			{
				configureTween();
				return tween;
			}
		}

		public AnimationCurve AnimationCurve
		{
			get
			{
				return animCurve;
			}
			set
			{
				animCurve = value;
			}
		}

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

		public T StartValue
		{
			get
			{
				return startValue;
			}
			set
			{
				startValue = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public TweenStartValueType StartValueType
		{
			get
			{
				return startValueType;
			}
			set
			{
				startValueType = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public T EndValue
		{
			get
			{
				return endValue;
			}
			set
			{
				endValue = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public TweenEndValueType EndValueType
		{
			get
			{
				return endValueType;
			}
			set
			{
				endValueType = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
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

		protected virtual void cleanup()
		{
			if (tween != null)
			{
				tween.Stop();
				tween.Release();
				tween = null;
			}
		}

		protected virtual void validateTweenConfiguration()
		{
			loopCount = Mathf.Max(0, loopCount);
			if (tween == null)
			{
				throw new InvalidOperationException("The tween has not been properly configured");
			}
		}

		protected abstract void configureTween();
	}
}
