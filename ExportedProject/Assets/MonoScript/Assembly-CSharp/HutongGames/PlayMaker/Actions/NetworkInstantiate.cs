using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Creates a Game Object on all clients in a network game.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkInstantiate : FsmStateAction
	{
		[Tooltip("The prefab will be instanted on all clients in the game.")]
		[RequiredField]
		public FsmGameObject prefab;

		[Tooltip("Optional Spawn Point.")]
		public FsmGameObject spawnPoint;

		[Tooltip("Spawn Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
		public FsmVector3 position;

		[Tooltip("Spawn Rotation. NOTE: Overrides the rotation of the Spawn Point.")]
		public FsmVector3 rotation;

		[UIHint(UIHint.Variable)]
		[Tooltip("Optionally store the created object.")]
		public FsmGameObject storeObject;

		[Tooltip("Usually 0. The group number allows you to group together network messages which allows you to filter them if so desired.")]
		public FsmInt networkGroup;

		public override void Reset()
		{
			prefab = null;
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
			networkGroup = 0;
		}

		public override void OnEnter()
		{
			GameObject value = prefab.Value;
			if (value != null)
			{
				Vector3 vector = Vector3.zero;
				Vector3 euler = Vector3.up;
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
				GameObject value2 = (GameObject)Network.Instantiate(value, vector, Quaternion.Euler(euler), networkGroup.Value);
				storeObject.Value = value2;
			}
			Finish();
		}
	}
}
