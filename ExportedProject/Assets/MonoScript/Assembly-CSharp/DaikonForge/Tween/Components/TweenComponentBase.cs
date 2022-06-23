using System;
using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	[Serializable]
	public abstract class TweenComponentBase : TweenPlayableComponent
	{
		[Inspector("General", Order = -1, Label = "Name", Tooltip = "For your convenience, you may specify a name for this Tween")]
		[SerializeField]
		protected string tweenName;

		[SerializeField]
		[Inspector("Animation", Order = 0, Label = "Delay", Tooltip = "The amount of time in seconds to delay before starting the animation")]
		protected float startDelay;

		[Inspector("Animation", Order = 1, Label = "Assign Start First", Tooltip = "If set, the StartValue will be assigned to the target before the delay (if any) is performed")]
		[SerializeField]
		protected bool assignStartValueBeforeDelay = true;

		[SerializeField]
		[Inspector("Looping", Order = 1, Label = "Type", Tooltip = "Specify whether the animation will loop at the end")]
		protected TweenLoopType loopType;

		[Inspector("Looping", Order = 1, Label = "Count", Tooltip = "If set to 0, the animation will loop forever")]
		[SerializeField]
		protected int loopCount;

		protected bool wasAutoStarted;

		public float StartDelay
		{
			get
			{
				return startDelay;
			}
			set
			{
				startDelay = value;
			}
		}

		public bool AssignStartValueBeforeDelay
		{
			get
			{
				return assignStartValueBeforeDelay;
			}
			set
			{
				assignStartValueBeforeDelay = value;
			}
		}

		public TweenLoopType LoopType
		{
			get
			{
				return loopType;
			}
			set
			{
				loopType = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public int LoopCount
		{
			get
			{
				return loopCount;
			}
			set
			{
				loopCount = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				return base.enabled && (State == TweenState.Started || State == TweenState.Playing);
			}
		}

		public bool IsPaused
		{
			get
			{
				return State == TweenState.Paused;
			}
		}

		private static bool IsLoopCountVisible(object target)
		{
			return true;
		}

		public override void Start()
		{
			base.Start();
			if (autoRun && !wasAutoStarted)
			{
				wasAutoStarted = true;
				Play();
			}
		}

		public override void OnEnable()
		{
			base.OnEnable();
			if (autoRun && !wasAutoStarted)
			{
				wasAutoStarted = true;
				Play();
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();
			if (IsPlaying)
			{
				Stop();
			}
			wasAutoStarted = false;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1} '{2}'", base.gameObject.name, GetType().Name, tweenName);
		}
	}
}
