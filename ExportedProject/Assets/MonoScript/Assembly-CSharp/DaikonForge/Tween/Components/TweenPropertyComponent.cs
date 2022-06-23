using System;
using System.Collections.Generic;
//using DaikonForge.Editor;
using DaikonForge.Tween.Interpolation;
using UnityEngine;

namespace DaikonForge.Tween.Components
{
//	[InspectorGroupOrder(new string[] { "General", "Animation", "Looping", "Property", "Values" })]
	public class TweenPropertyComponent<T> : TweenComponent<T>, ITweenPropertyBase
	{
	//	[Inspector("Property", Label = "Target", Order = 0)]
		[SerializeField]
		protected GameObject target;

		[SerializeField]
		protected string componentType;

	//	[Inspector("Property", Label = "Field", Order = 1)]
		[SerializeField]
		protected string memberName;

		private Component component;

		private TweenEasingCallback easingFunc;

		public GameObject Target
		{
			get
			{
				return target;
			}
			set
			{
				if (value != target)
				{
					target = value;
					if (State != 0)
					{
						Stop();
						Play();
					}
				}
			}
		}

		public string ComponentType
		{
			get
			{
				return componentType;
			}
			set
			{
				if (value != componentType)
				{
					componentType = value;
					if (State != 0)
					{
						Stop();
						Play();
					}
				}
			}
		}

		public string MemberName
		{
			get
			{
				return memberName;
			}
			set
			{
				if (value != memberName)
				{
					memberName = value;
					if (State != 0)
					{
						Stop();
						Play();
					}
				}
			}
		}

		public override void OnEnable()
		{
			if (target == null)
			{
				target = base.gameObject;
			}
			base.OnEnable();
		}

		protected override void validateTweenConfiguration()
		{
			base.validateTweenConfiguration();
			if (!(target == null) && !string.IsNullOrEmpty(componentType) && !string.IsNullOrEmpty(memberName))
			{
				Component component = target.GetComponent(componentType);
				if (component == null)
				{
					throw new NullReferenceException(string.Format("Object {0} does not contain a {1} component", target.name, componentType));
				}
				Interpolator<T> interpolator = Interpolators.Get<T>();
				if (interpolator == null)
				{
					throw new KeyNotFoundException(string.Format("There is no default interpolator defined for type '{0}'", typeof(T).Name));
				}
			}
		}

		protected override void configureTween()
		{
			if (tween == null)
			{
				if (target == null || string.IsNullOrEmpty(componentType) || string.IsNullOrEmpty(memberName))
				{
					return;
				}
				component = target.GetComponent(componentType);
				if (component == null)
				{
					return;
				}
				easingFunc = TweenEasingFunctions.GetFunction(easingType);
				tween = (Tween<T>)component.TweenProperty<T>(memberName).SetEasing(modifyEasing).OnStarted(delegate
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
			T currentValue = TweenNamedProperty<T>.GetCurrentValue(component, memberName);
			Interpolator<T> interpolator = Interpolators.Get<T>();
			T rhs = startValue;
			if (startValueType == TweenStartValueType.SyncOnRun)
			{
				rhs = currentValue;
			}
			T lhs = endValue;
			if (endValueType == TweenEndValueType.SyncOnRun)
			{
				lhs = currentValue;
			}
			else if (endValueType == TweenEndValueType.Relative)
			{
				lhs = interpolator.Add(lhs, rhs);
			}
			tween.SetStartValue(rhs).SetEndValue(lhs).SetDelay(startDelay, assignStartValueBeforeDelay)
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
