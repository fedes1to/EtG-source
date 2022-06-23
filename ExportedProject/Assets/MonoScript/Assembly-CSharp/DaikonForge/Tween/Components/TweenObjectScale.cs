using UnityEngine;

namespace DaikonForge.Tween.Components
{
	[AddComponentMenu("Daikon Forge/Tween/Object Scale")]
	public class TweenObjectScale : TweenComponent<Vector3>
	{
		private TweenEasingCallback easingFunc;

		protected override void configureTween()
		{
			if (tween == null)
			{
				easingFunc = TweenEasingFunctions.GetFunction(easingType);
				tween = (Tween<Vector3>)base.transform.TweenScale().SetEasing(modifyEasing).OnStarted(delegate
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
			Vector3 localScale = base.transform.localScale;
			Vector3 vector = startValue;
			if (startValueType == TweenStartValueType.SyncOnRun)
			{
				vector = localScale;
			}
			Vector3 vector2 = endValue;
			if (endValueType == TweenEndValueType.SyncOnRun)
			{
				vector2 = localScale;
			}
			else if (endValueType == TweenEndValueType.Relative)
			{
				vector2 += vector;
			}
			tween.SetStartValue(vector).SetEndValue(vector2).SetDelay(startDelay, assignStartValueBeforeDelay)
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
