using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Destroys a Component of an Object.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class DestroyComponent : FsmStateAction
	{
		[Tooltip("The GameObject that owns the Component.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("The name of the Component to destroy.")]
		[RequiredField]
		[UIHint(UIHint.ScriptComponent)]
		public FsmString component;

		private Component aComponent;

		public override void Reset()
		{
			aComponent = null;
			gameObject = null;
			component = null;
		}

		public override void OnEnter()
		{
			DoDestroyComponent((gameObject.OwnerOption != 0) ? gameObject.GameObject.Value : base.Owner);
			Finish();
		}

		private void DoDestroyComponent(GameObject go)
		{
			aComponent = go.GetComponent(ReflectionUtils.GetGlobalType(component.Value));
			if (aComponent == null)
			{
				LogError("No such component: " + component.Value);
			}
			else
			{
				Object.Destroy(aComponent);
			}
		}
	}
}
