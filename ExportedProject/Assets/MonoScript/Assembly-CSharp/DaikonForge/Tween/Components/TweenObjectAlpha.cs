using System;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
	[AddComponentMenu("Daikon Forge/Tween/Object Alpha")]
	public class TweenObjectAlpha : TweenComponent<float>
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
			if (startValue < 0f || startValue > 1f)
			{
				throw new InvalidOperationException("The Start Value must be between 0 and 1");
			}
			if (endValue < 0f || endValue > 1f)
			{
				throw new InvalidOperationException("The End Value must be between 0 and 1");
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
				tween = (Tween<float>)Target.TweenAlpha().SetEasing(modifyEasing).OnStarted(delegate
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
			float currentValue = tween.CurrentValue;
			float num = startValue;
			if (startValueType == TweenStartValueType.SyncOnRun)
			{
				num = currentValue;
			}
			float num2 = endValue;
			if (endValueType == TweenEndValueType.SyncOnRun)
			{
				num2 = currentValue;
			}
			else if (endValueType == TweenEndValueType.Relative)
			{
				num2 += num;
			}
			tween.SetStartValue(num).SetEndValue(num2).SetDelay(startDelay, assignStartValueBeforeDelay)
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
