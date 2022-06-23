using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dungeonator
{
	[Serializable]
	public class DungeonPlaceableVariant
	{
		[SerializeField]
		public float percentChance = 0.1f;

		[SerializeField]
		public Vector2 unitOffset = Vector2.zero;

		[SerializeField]
		[FormerlySerializedAs("nonenemyPlaceable")]
		public GameObject nonDatabasePlaceable;

		[FormerlySerializedAs("enemyGuid")]
		[EnemyIdentifier]
		public string enemyPlaceableGuid;

		[PickupIdentifier]
		public int pickupObjectPlaceableId = -1;

		[SerializeField]
		public bool forceBlackPhantom;

		[SerializeField]
		public bool addDebrisObject;

		[SerializeField]
		public DungeonPrerequisite[] prerequisites;

		[SerializeField]
		public DungeonPlaceableRoomMaterialRequirement[] materialRequirements;

		[NonSerialized]
		public float percentChanceMultiplier = 1f;

		public GameObject GetOrLoadPlaceableObject
		{
			get
			{
				if ((bool)nonDatabasePlaceable)
				{
					return nonDatabasePlaceable;
				}
				if (!string.IsNullOrEmpty(enemyPlaceableGuid))
				{
					AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(enemyPlaceableGuid);
					if ((bool)orLoadByGuid)
					{
						return orLoadByGuid.gameObject;
					}
				}
				if (pickupObjectPlaceableId >= 0)
				{
					PickupObject byId = PickupObjectDatabase.GetById(pickupObjectPlaceableId);
					if ((bool)byId)
					{
						return byId.gameObject;
					}
				}
				return null;
			}
		}

		public DungeonPlaceableBehaviour.PlaceableDifficulty difficulty
		{
			get
			{
				if (nonDatabasePlaceable != null)
				{
					DungeonPlaceableBehaviour component = nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
					if (component != null)
					{
						return component.difficulty;
					}
				}
				if (!string.IsNullOrEmpty(enemyPlaceableGuid))
				{
					EnemyDatabaseEntry entry = EnemyDatabase.GetEntry(enemyPlaceableGuid);
					if (entry == null)
					{
						return DungeonPlaceableBehaviour.PlaceableDifficulty.BASE;
					}
					return entry.difficulty;
				}
				return DungeonPlaceableBehaviour.PlaceableDifficulty.BASE;
			}
		}

		public int difficultyRating
		{
			get
			{
				return (int)difficulty;
			}
		}

		public float GetPercentChance()
		{
			return percentChance;
		}
	}
}
