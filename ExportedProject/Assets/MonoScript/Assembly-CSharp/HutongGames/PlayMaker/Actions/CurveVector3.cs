using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.AnimateVariables)]
	[Tooltip("Animates the value of a Vector3 Variable FROM-TO with assistance of Deformation Curves.")]
	public class CurveVector3 : CurveFsmAction
	{
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmVector3 vectorVariable;

		[RequiredField]
		public FsmVector3 fromValue;

		[RequiredField]
		public FsmVector3 toValue;

		[RequiredField]
		public FsmAnimationCurve curveX;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to otherwise linear move between fromValue.x and toValue.x.")]
		public Calculation calculationX;

		[RequiredField]
		public FsmAnimationCurve curveY;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to otherwise linear move between fromValue.y and toValue.y.")]
		public Calculation calculationY;

		[RequiredField]
		public FsmAnimationCurve curveZ;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to otherwise linear move between fromValue.z and toValue.z.")]
		public Calculation calculationZ;

		private Vector3 vct;

		private bool finishInNextStep;

		public override void Reset()
		{
			base.Reset();
			vectorVariable = new FsmVector3
			{
				UseVariable = true
			};
			toValue = new FsmVector3
			{
				UseVariable = true
			};
			fromValue = new FsmVector3
			{
				UseVariable = true
			};
		}

		public override void OnEnter()
		{
			base.OnEnter();
			finishInNextStep = false;
			resultFloats = new float[3];
			fromFloats = new float[3];
			fromFloats[0] = ((!fromValue.IsNone) ? fromValue.Value.x : 0f);
			fromFloats[1] = ((!fromValue.IsNone) ? fromValue.Value.y : 0f);
			fromFloats[2] = ((!fromValue.IsNone) ? fromValue.Value.z : 0f);
			toFloats = new float[3];
			toFloats[0] = ((!toValue.IsNone) ? toValue.Value.x : 0f);
			toFloats[1] = ((!toValue.IsNone) ? toValue.Value.y : 0f);
			toFloats[2] = ((!toValue.IsNone) ? toValue.Value.z : 0f);
			curves = new AnimationCurve[3];
			curves[0] = curveX.curve;
			curves[1] = curveY.curve;
			curves[2] = curveZ.curve;
			calculations = new Calculation[3];
			calculations[0] = calculationX;
			calculations[1] = calculationY;
			calculations[2] = calculationZ;
			Init();
		}

		public override void OnExit()
		{
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!vectorVariable.IsNone && isRunning)
			{
				vct = new Vector3(resultFloats[0], resultFloats[1], resultFloats[2]);
				vectorVariable.Value = vct;
			}
			if (finishInNextStep && !looping)
			{
				Finish();
				if (finishEvent != null)
				{
					base.Fsm.Event(finishEvent);
				}
			}
			if (finishAction && !finishInNextStep)
			{
				if (!vectorVariable.IsNone)
				{
					vct = new Vector3(resultFloats[0], resultFloats[1], resultFloats[2]);
					vectorVariable.Value = vct;
				}
				finishInNextStep = true;
			}
		}
	}
}
