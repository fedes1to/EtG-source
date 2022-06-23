using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Destroys GameObjects in an array.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class DestroyObjects : FsmStateAction
	{
		[Tooltip("The GameObjects to destroy.")]
		[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
		[RequiredField]
		public FsmArray gameObjects;

		[Tooltip("Optional delay before destroying the Game Objects.")]
		[HasFloatSlider(0f, 5f)]
		public FsmFloat delay;

		[Tooltip("Detach children before destroying the Game Objects.")]
		public FsmBool detachChildren;

		public override void Reset()
		{
			gameObjects = null;
			delay = 0f;
		}

		public override void OnEnter()
		{
			if (gameObjects.Values != null)
			{
				object[] values = gameObjects.Values;
				for (int i = 0; i < values.Length; i++)
				{
					GameObject gameObject = (GameObject)values[i];
					if (gameObject != null)
					{
						if (delay.Value <= 0f)
						{
							Object.Destroy(gameObject);
						}
						else
						{
							Object.Destroy(gameObject, delay.Value);
						}
						if (detachChildren.Value)
						{
							gameObject.transform.DetachChildren();
						}
					}
				}
			}
			Finish();
		}
	}
}
