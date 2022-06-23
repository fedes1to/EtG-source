using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionTarget(typeof(GameObject), "gameObject", true)]
	[Tooltip("Creates a Game Object, usually using a Prefab.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class CreateObject : FsmStateAction
	{
		[Tooltip("GameObject to create. Usually a Prefab.")]
		[RequiredField]
		public FsmGameObject gameObject;

		[Tooltip("Optional Spawn Point.")]
		public FsmGameObject spawnPoint;

		[Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
		public FsmVector3 position;

		[Tooltip("Rotation. NOTE: Overrides the rotation of the Spawn Point.")]
		public FsmVector3 rotation;

		[Tooltip("Optionally store the created object.")]
		[UIHint(UIHint.Variable)]
		public FsmGameObject storeObject;

		[Tooltip("Use Network.Instantiate to create a Game Object on all clients in a networked game.")]
		public FsmBool networkInstantiate;

		[Tooltip("Usually 0. The group number allows you to group together network messages which allows you to filter them if so desired.")]
		public FsmInt networkGroup;

		public override void Reset()
		{
			gameObject = null;
			spawnPoint = null;
			position = new FsmVector3
			{
				UseVariable = true
			};
			rotation = new FsmVector3
			{
				UseVariable = true
			};
			storeObject = null;
			networkInstantiate = false;
			networkGroup = 0;
		}

		public override void OnEnter()
		{
			GameObject value = gameObject.Value;
			if (value != null)
			{
				Vector3 vector = Vector3.zero;
				Vector3 euler = Vector3.zero;
				if (spawnPoint.Value != null)
				{
					vector = spawnPoint.Value.transform.position;
					if (!position.IsNone)
					{
						vector += position.Value;
					}
					euler = (rotation.IsNone ? spawnPoint.Value.transform.eulerAngles : rotation.Value);
				}
				else
				{
					if (!position.IsNone)
					{
						vector = position.Value;
					}
					if (!rotation.IsNone)
					{
						euler = rotation.Value;
					}
				}
				GameObject value2 = (networkInstantiate.Value ? ((GameObject)Network.Instantiate(value, vector, Quaternion.Euler(euler), networkGroup.Value)) : Object.Instantiate(value, vector, Quaternion.Euler(euler)));
				storeObject.Value = value2;
			}
			Finish();
		}
	}
}
