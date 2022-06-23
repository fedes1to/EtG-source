using UnityEngine;

namespace DaikonForge.Tween.Components
{
	[AddComponentMenu("Daikon Forge/Tween/Object Rotation")]
	public class TweenObjectRotation : TweenComponent<Vector3>
	{
		[SerializeField]
		protected bool useLocalRotation;

		[SerializeField]
		protected bool useShortestPath = true;

		private TweenEasingCallback easingFunc;

		public bool UseLocalRotation
		{
			get
			{
				return useLocalRotation;
			}
			set
			{
				useLocalRotation = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		public bool UseShortestPath
		{
			get
			{
				return useShortestPath;
			}
			set
			{
				useShortestPath = value;
				if (State != 0)
				{
					Stop();
					Play();
				}
			}
		}

		protected override void configureTween()
		{
			if (tween == null)
			{
				easingFunc = TweenEasingFunctions.GetFunction(easingType);
				tween = (Tween<Vector3>)base.transform.TweenRotation(useShortestPath, useLocalRotation).SetEasing(modifyEasing).OnStarted(delegate
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
			Vector3 vector = ((!useLocalRotation) ? base.transform.eulerAngles : base.transform.localEulerAngles);
			Vector3 vector2 = startValue;
			if (startValueType == TweenStartValueType.SyncOnRun)
			{
				vector2 = vector;
			}
			Vector3 vector3 = endValue;
			if (endValueType == TweenEndValueType.SyncOnRun)
			{
				vector3 = vector;
			}
			else if (endValueType == TweenEndValueType.Relative)
			{
				vector3 += vector2;
			}
			tween.SetStartValue(vector2).SetEndValue(vector3).SetDelay(startDelay, assignStartValueBeforeDelay)
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
