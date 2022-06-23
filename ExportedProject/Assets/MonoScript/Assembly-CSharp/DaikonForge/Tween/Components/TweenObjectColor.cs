using System;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	[AddComponentMenu("Daikon Forge/Tween/Object Color")]
	public class TweenObjectColor : TweenComponent<Color>
	{
		[SerializeField]
		protected Component target;

		private TweenEasingCallback easingFunc;

		public Component Target
		{
			get
			{
				return target;
			}
			set
			{
				target = value;
				Stop();
			}
		}

		protected override void validateTweenConfiguration()
		{
			if (target == null)
			{
				throw new InvalidOperationException("The Target cannot be NULL");
			}
			base.validateTweenConfiguration();
		}

		protected override void configureTween()
		{
			if (target == null)
			{
				target = base.gameObject.GetComponent<Renderer>();
				if (target == null)
				{
					if (tween != null)
					{
						tween.Stop();
						tween.Release();
						tween = null;
					}
					return;
				}
			}
			if (tween == null)
			{
				easingFunc = TweenEasingFunctions.GetFunction(easingType);
				tween = (Tween<Color>)Target.TweenColor().SetEasing(modifyEasing).OnStarted(delegate
				{
					onStarted();
				})
					.OnStopped(delegate
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
			Color currentValue = tween.CurrentValue;
			Color color = startValue;
			if (startValueType == TweenStartValueType.SyncOnRun)
			{
				color = currentValue;
			}
			Color color2 = endValue;
			if (endValueType == TweenEndValueType.SyncOnRun)
			{
				color2 = currentValue;
			}
			else if (endValueType == TweenEndValueType.Relative)
			{
				color2 += color;
			}
			tween.SetStartValue(color).SetEndValue(color2).SetDelay(startDelay, assignStartValueBeforeDelay)
				.SetDuration(duration)
				.SetLoopType(base.LoopType)
				.SetLoopCount(loopCount)
				.SetPlayDirection(playDirection);
		}

		private float modifyEasing(float time)
		{
			if (animCurve != null)
			{
				time = animCurve.Evaluate(time);
			}
			return easingFunc(time);
		}
	}
}
