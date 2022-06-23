using System.Collections;
//using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	public abstract class TweenPlayableComponent : MonoBehaviour
	{
		[SerializeField]
		protected bool autoRun;

		public virtual string TweenName { get; set; }

		public abstract TweenState State { get; }

		public abstract TweenBase BaseTween { get; }

	//	[Inspector("General", 1, BackingField = "autoRun", Tooltip = "If set to TRUE, this Tween will automatically play when the scene starts")]
		public bool AutoRun
		{
			get
			{
				return autoRun;
			}
			set
			{
				autoRun = value;
			}
		}

		public event TweenComponentNotification TweenStarted;

		public event TweenComponentNotification TweenStopped;

		public event TweenComponentNotification TweenPaused;

		public event TweenComponentNotification TweenResumed;

		public event TweenComponentNotification TweenLoopCompleted;

		public event TweenComponentNotification TweenCompleted;

		public abstract void Play();

		public abstract void Stop();

		public abstract void Rewind();

		public abstract void FastForward();

		public abstract void Pause();

		public abstract void Resume();

		public virtual void Awake()
		{
		}

		public virtual void Start()
		{
		}

		public virtual void OnEnable()
		{
		}

		public virtual void OnDisable()
		{
		}

		public virtual void OnDestroy()
		{
		}

		public virtual void Enable()
		{
			base.enabled = true;
		}

		public virtual void Disable()
		{
			base.enabled = false;
		}

		public virtual IEnumerator WaitForCompletion()
		{
			while (State != 0)
			{
				yield return null;
			}
		}

		protected virtual void onPaused()
		{
			if (this.TweenPaused != null)
			{
				this.TweenPaused(this);
			}
		}

		protected virtual void onResumed()
		{
			if (this.TweenResumed != null)
			{
				this.TweenResumed(this);
			}
		}

		protected virtual void onStarted()
		{
			if (this.TweenStarted != null)
			{
				this.TweenStarted(this);
			}
		}

		protected virtual void onStopped()
		{
			if (this.TweenStopped != null)
			{
				this.TweenStopped(this);
			}
		}

		protected virtual void onLoopCompleted()
		{
			if (this.TweenLoopCompleted != null)
			{
				this.TweenLoopCompleted(this);
			}
		}

		protected virtual void onCompleted()
		{
			if (this.TweenCompleted != null)
			{
				this.TweenCompleted(this);
			}
		}

		public override string ToString()
		{
			return TweenName + " - " + base.ToString();
		}
	}
}
