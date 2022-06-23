using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	public class DungeonPlaceable : ScriptableObject
	{
		public int width;

		public int height;

		[SerializeField]
		public bool isPassable = true;

		[SerializeField]
		public bool roomSequential;

		[SerializeField]
		public bool respectsEncounterableDifferentiator;

		[SerializeField]
		public bool UsePrefabTransformOffset;

		[SerializeField]
		public bool MarkSpawnedItemsAsRatIgnored;

		[SerializeField]
		public bool DebugThisPlaceable;

		[SerializeField]
		public bool IsAnnexTable;

		[SerializeField]
		public List<DungeonPlaceableVariant> variantTiers;

		public bool ContainsEnemy
		{
			get
			{
				for (int i = 0; i < variantTiers.Count; i++)
				{
					if (!string.IsNullOrEmpty(variantTiers[i].enemyPlaceableGuid))
					{
						EnemyDatabaseEntry entry = EnemyDatabase.GetEntry(variantTiers[i].enemyPlaceableGuid);
						if (entry != null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool ContainsEnemylikeObjectForReinforcement
		{
			get
			{
				for (int i = 0; i < variantTiers.Count; i++)
				{
					if ((bool)variantTiers[i].nonDatabasePlaceable && (bool)variantTiers[i].nonDatabasePlaceable.GetComponent<ForgeHammerController>())
					{
						return true;
					}
				}
				return false;
			}
		}

		public int GetHeight()
		{
			return height;
		}

		public int GetWidth()
		{
			return width;
		}

		public bool IsValidMirrorPlaceable()
		{
			for (int i = 0; i < variantTiers.Count; i++)
			{
				if ((bool)variantTiers[i].nonDatabasePlaceable && !PrototypeDungeonRoom.GameObjectCanBeMirrored(variantTiers[i].nonDatabasePlaceable))
				{
					return false;
				}
			}
			return true;
		}

		public bool HasValidPlaceable()
		{
			for (int i = 0; i < variantTiers.Count; i++)
			{
				bool flag = true;
				if (variantTiers[i] == null)
				{
					continue;
				}
				if (variantTiers[i].prerequisites == null)
				{
					return true;
				}
				for (int j = 0; j < variantTiers[i].prerequisites.Length; j++)
				{
					if (!variantTiers[i].prerequisites[j].CheckConditionsFulfilled())
					{
						flag = false;
					}
				}
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		private GameObject InstantiateInternal(DungeonPlaceableVariant selectedVariant, RoomHandler targetRoom, IntVector2 location, bool deferConfiguration)
		{
			if (selectedVariant != null && selectedVariant.GetOrLoadPlaceableObject != null)
			{
				GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(selectedVariant.GetOrLoadPlaceableObject, targetRoom, location, deferConfiguration);
				if (gameObject != null && selectedVariant.unitOffset != Vector2.zero)
				{
					gameObject.transform.position += selectedVariant.unitOffset.ToVector3ZUp();
					SpeculativeRigidbody componentInChildren = gameObject.GetComponentInChildren<SpeculativeRigidbody>();
					if ((bool)componentInChildren)
					{
						componentInChildren.Reinitialize();
					}
				}
				if (gameObject != null && UsePrefabTransformOffset)
				{
					gameObject.transform.position += selectedVariant.GetOrLoadPlaceableObject.transform.position;
				}
				if (selectedVariant.forceBlackPhantom && (bool)gameObject)
				{
					AIActor component = gameObject.GetComponent<AIActor>();
					if ((bool)component)
					{
						component.ForceBlackPhantom = true;
					}
				}
				if (selectedVariant.addDebrisObject && (bool)gameObject)
				{
					DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
					orAddComponent.shouldUseSRBMotion = true;
					orAddComponent.angularVelocity = 0f;
					orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
					orAddComponent.canRotate = false;
				}
				if (MarkSpawnedItemsAsRatIgnored && (bool)gameObject)
				{
					PickupObject component2 = gameObject.GetComponent<PickupObject>();
					if ((bool)component2)
					{
						component2.IgnoredByRat = true;
					}
				}
				return gameObject;
			}
			return null;
		}

		private GameObject InstantiateInternalOnlyActors(DungeonPlaceableVariant selectedVariant, RoomHandler targetRoom, IntVector2 location, bool deferConfiguration)
		{
			if (selectedVariant != null && selectedVariant.GetOrLoadPlaceableObject != null)
			{
				GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceableOnlyActors(selectedVariant.GetOrLoadPlaceableObject, targetRoom, location, deferConfiguration);
				if (selectedVariant.forceBlackPhantom && (bool)gameObject)
				{
					AIActor component = gameObject.GetComponent<AIActor>();
					if ((bool)component)
					{
						component.ForceBlackPhantom = true;
					}
				}
				return gameObject;
			}
			return null;
		}

		public GameObject InstantiateObjectDirectional(RoomHandler targetRoom, IntVector2 location, DungeonData.Direction direction)
		{
			List<DungeonPlaceableVariant> list = new List<DungeonPlaceableVariant>();
			if (variantTiers.Count == 4)
			{
				switch (direction)
				{
				case DungeonData.Direction.NORTH:
					list.Add(variantTiers[0]);
					break;
				case DungeonData.Direction.EAST:
					list.Add(variantTiers[1]);
					break;
				case DungeonData.Direction.SOUTH:
					list.Add(variantTiers[2]);
					break;
				case DungeonData.Direction.WEST:
					list.Add(variantTiers[3]);
					break;
				}
				return InstantiateInternal(SelectVariantByWeighting(list), targetRoom, location, false);
			}
			foreach (DungeonPlaceableVariant variantTier in variantTiers)
			{
				variantTier.percentChanceMultiplier = 1f;
				if (!ProcessVariantPrerequisites(variantTier, location, targetRoom))
				{
					continue;
				}
				DungeonDoorController component = variantTier.nonDatabasePlaceable.GetComponent<DungeonDoorController>();
				DungeonDoorSubsidiaryBlocker component2 = variantTier.nonDatabasePlaceable.GetComponent<DungeonDoorSubsidiaryBlocker>();
				if (component != null)
				{
					if (component.northSouth && (direction == DungeonData.Direction.NORTH || direction == DungeonData.Direction.SOUTH))
					{
						list.Add(variantTier);
					}
					else if (!component.northSouth && (direction == DungeonData.Direction.EAST || direction == DungeonData.Direction.WEST))
					{
						list.Add(variantTier);
					}
				}
				else if (component2 != null)
				{
					if (component2.northSouth && (direction == DungeonData.Direction.NORTH || direction == DungeonData.Direction.SOUTH))
					{
						list.Add(variantTier);
					}
					else if (!component2.northSouth && (direction == DungeonData.Direction.EAST || direction == DungeonData.Direction.WEST))
					{
						list.Add(variantTier);
					}
				}
				else
				{
					list.Add(variantTier);
				}
			}
			return InstantiateInternal(SelectVariantByWeighting(list), targetRoom, location, false);
		}

		public GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 location, bool onlyActors = false, bool deferConfiguration = false)
		{
			int variantIndex = -1;
			return InstantiateObject(targetRoom, location, out variantIndex, -1, onlyActors, deferConfiguration);
		}

		public void ModifyWeightsByDifficulty(List<DungeonPlaceableVariant> validVariants)
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				return;
			}
			if (GameManager.Instance.PrimaryPlayer == null || GameManager.Instance.PrimaryPlayer.stats == null)
			{
				Debug.LogError("No player yet--can't check curse stat in DungeonPlaceable.");
				return;
			}
			Dictionary<DungeonPlaceableBehaviour.PlaceableDifficulty, float> dictionary = new Dictionary<DungeonPlaceableBehaviour.PlaceableDifficulty, float>();
			float num = 0f;
			for (int i = 0; i < validVariants.Count; i++)
			{
				DungeonPlaceableBehaviour.PlaceableDifficulty difficulty = validVariants[i].difficulty;
				if (!dictionary.ContainsKey(difficulty))
				{
					dictionary.Add(difficulty, 0f);
				}
				dictionary[difficulty] += validVariants[i].GetPercentChance();
				num += validVariants[i].GetPercentChance();
			}
			if (dictionary.Count <= 1)
			{
				return;
			}
			float num2 = PlayerStats.GetTotalCurse();
			float num3 = Mathf.Clamp01(num2 / 10f);
			float num4 = ((!dictionary.ContainsKey(DungeonPlaceableBehaviour.PlaceableDifficulty.BASE)) ? 0f : dictionary[DungeonPlaceableBehaviour.PlaceableDifficulty.BASE]) / num;
			float num5 = ((!dictionary.ContainsKey(DungeonPlaceableBehaviour.PlaceableDifficulty.HARD)) ? 0f : dictionary[DungeonPlaceableBehaviour.PlaceableDifficulty.HARD]) / num;
			float num6 = ((!dictionary.ContainsKey(DungeonPlaceableBehaviour.PlaceableDifficulty.HARDER)) ? 0f : dictionary[DungeonPlaceableBehaviour.PlaceableDifficulty.HARDER]) / num;
			float num7 = ((!dictionary.ContainsKey(DungeonPlaceableBehaviour.PlaceableDifficulty.HARDEST)) ? 0f : dictionary[DungeonPlaceableBehaviour.PlaceableDifficulty.HARDEST]) / num;
			if (num4 > num3)
			{
				float num8 = num4 - num3;
				for (int j = 0; j < validVariants.Count; j++)
				{
					if (validVariants[j].difficultyRating == 0)
					{
						validVariants[j].percentChanceMultiplier = num8 / num4;
					}
				}
			}
			else if (num4 + num5 > num3)
			{
				float num9 = num4 + num5 - num3;
				for (int k = 0; k < validVariants.Count; k++)
				{
					if (validVariants[k].difficultyRating <= 0)
					{
						validVariants.RemoveAt(k);
						k--;
					}
					else if (validVariants[k].difficultyRating == 1)
					{
						validVariants[k].percentChanceMultiplier = num9 / num5;
					}
				}
			}
			else if (num4 + num5 + num6 > num3)
			{
				float num10 = num4 + num5 + num6 - num3;
				for (int l = 0; l < validVariants.Count; l++)
				{
					if (validVariants[l].difficultyRating <= 1)
					{
						validVariants.RemoveAt(l);
						l--;
					}
					else if (validVariants[l].difficultyRating == 2)
					{
						validVariants[l].percentChanceMultiplier = num10 / num6;
					}
				}
			}
			else
			{
				if (!(num4 + num5 + num6 + num7 >= num3))
				{
					return;
				}
				for (int m = 0; m < validVariants.Count; m++)
				{
					if (validVariants[m].difficultyRating <= 2)
					{
						validVariants.RemoveAt(m);
						m--;
					}
				}
			}
		}

		public GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 location, out int variantIndex, int forceVariant = -1, bool onlyActors = false, bool deferConfiguration = false)
		{
			variantIndex = -1;
			List<DungeonPlaceableVariant> list = new List<DungeonPlaceableVariant>();
			int num = int.MaxValue;
			for (int i = 0; i < variantTiers.Count; i++)
			{
				DungeonPlaceableVariant dungeonPlaceableVariant = variantTiers[i];
				dungeonPlaceableVariant.percentChanceMultiplier = 1f;
				if (!ProcessVariantPrerequisites(dungeonPlaceableVariant, location, targetRoom))
				{
					continue;
				}
				if (respectsEncounterableDifferentiator)
				{
					int? num2 = null;
					if (dungeonPlaceableVariant.nonDatabasePlaceable != null)
					{
						EncounterTrackable component = dungeonPlaceableVariant.nonDatabasePlaceable.GetComponent<EncounterTrackable>();
						if (component != null)
						{
							num2 = GameStatsManager.Instance.QueryEncounterableDifferentiator(component);
						}
					}
					else if (!string.IsNullOrEmpty(dungeonPlaceableVariant.enemyPlaceableGuid))
					{
						EnemyDatabaseEntry entry = EnemyDatabase.GetEntry(dungeonPlaceableVariant.enemyPlaceableGuid);
						if (entry != null && !string.IsNullOrEmpty(entry.encounterGuid))
						{
							EncounterDatabaseEntry entry2 = EncounterDatabase.GetEntry(entry.encounterGuid);
							if (entry2 != null)
							{
								num2 = GameStatsManager.Instance.QueryEncounterableDifferentiator(entry2);
							}
						}
					}
					if (num2.HasValue)
					{
						if (num2.Value < num)
						{
							list.Clear();
							num = num2.Value;
						}
						else if (num2.Value > num)
						{
							continue;
						}
					}
				}
				if (targetRoom == null || !roomSequential || i <= targetRoom.distanceFromEntrance / 2)
				{
					list.Add(dungeonPlaceableVariant);
				}
			}
			DungeonPlaceableVariant dungeonPlaceableVariant2 = null;
			ModifyWeightsByDifficulty(list);
			if (forceVariant == -1)
			{
				dungeonPlaceableVariant2 = SelectVariantByWeighting(list);
			}
			else if (list.Count > forceVariant)
			{
				dungeonPlaceableVariant2 = list[forceVariant];
			}
			if (dungeonPlaceableVariant2 != null)
			{
				variantIndex = variantTiers.IndexOf(dungeonPlaceableVariant2);
			}
			if (respectsEncounterableDifferentiator && dungeonPlaceableVariant2 != null && dungeonPlaceableVariant2.GetOrLoadPlaceableObject != null)
			{
				EncounterTrackable component2 = dungeonPlaceableVariant2.GetOrLoadPlaceableObject.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					component2.HandleEncounter_GeneratedObjects();
				}
			}
			if (dungeonPlaceableVariant2 != null && dungeonPlaceableVariant2.GetOrLoadPlaceableObject != null)
			{
				DungeonPlaceableBehaviour component3 = dungeonPlaceableVariant2.GetOrLoadPlaceableObject.GetComponent<DungeonPlaceableBehaviour>();
				if (component3 != null)
				{
					GameObject gameObject = null;
					gameObject = ((!onlyActors) ? component3.InstantiateObject(targetRoom, location, deferConfiguration) : component3.InstantiateObjectOnlyActors(targetRoom, location, deferConfiguration));
					if (gameObject != null && dungeonPlaceableVariant2.unitOffset != Vector2.zero)
					{
						gameObject.transform.position += dungeonPlaceableVariant2.unitOffset.ToVector3ZUp();
						SpeculativeRigidbody componentInChildren = gameObject.GetComponentInChildren<SpeculativeRigidbody>();
						if ((bool)componentInChildren)
						{
							componentInChildren.Reinitialize();
						}
					}
					if (dungeonPlaceableVariant2.forceBlackPhantom && (bool)gameObject)
					{
						AIActor component4 = gameObject.GetComponent<AIActor>();
						if ((bool)component4)
						{
							component4.ForceBlackPhantom = true;
						}
					}
					return gameObject;
				}
			}
			if (onlyActors)
			{
				return InstantiateInternalOnlyActors(dungeonPlaceableVariant2, targetRoom, location, deferConfiguration);
			}
			return InstantiateInternal(dungeonPlaceableVariant2, targetRoom, location, deferConfiguration);
		}

		private bool ProcessVariantPrerequisites(DungeonPlaceableVariant dpv, IntVector2? targetPosition = null, RoomHandler targetRoom = null)
		{
			if (targetRoom != null && targetPosition.HasValue && dpv.materialRequirements != null && dpv.materialRequirements.Length > 0)
			{
				bool flag = true;
				for (int i = 0; i < dpv.materialRequirements.Length; i++)
				{
					if (dpv.materialRequirements[i].TargetTileset != GameManager.Instance.Dungeon.tileIndices.tilesetId)
					{
						if (dpv.materialRequirements[i].RequireMaterial)
						{
							return false;
						}
						continue;
					}
					int roomVisualTypeIndex = GameManager.Instance.Dungeon.data[targetRoom.area.basePosition + targetPosition.Value].cellVisualData.roomVisualTypeIndex;
					if ((!dpv.materialRequirements[i].RequireMaterial || dpv.materialRequirements[i].RoomMaterial != roomVisualTypeIndex) && (dpv.materialRequirements[i].RequireMaterial || dpv.materialRequirements[i].RoomMaterial == roomVisualTypeIndex))
					{
						flag = false;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (dpv.prerequisites != null && dpv.prerequisites.Length > 0)
			{
				bool result = true;
				for (int j = 0; j < dpv.prerequisites.Length; j++)
				{
					if (!dpv.prerequisites[j].CheckConditionsFulfilled())
					{
						result = false;
						break;
					}
				}
				return result;
			}
			return true;
		}

		public DungeonPlaceableVariant SelectFromTiersFull()
		{
			List<DungeonPlaceableVariant> list = new List<DungeonPlaceableVariant>();
			int num = int.MaxValue;
			for (int i = 0; i < variantTiers.Count; i++)
			{
				DungeonPlaceableVariant dungeonPlaceableVariant = variantTiers[i];
				dungeonPlaceableVariant.percentChanceMultiplier = 1f;
				if (!ProcessVariantPrerequisites(dungeonPlaceableVariant))
				{
					continue;
				}
				if (respectsEncounterableDifferentiator)
				{
					int? num2 = null;
					if (dungeonPlaceableVariant.nonDatabasePlaceable != null)
					{
						EncounterTrackable component = dungeonPlaceableVariant.nonDatabasePlaceable.GetComponent<EncounterTrackable>();
						if (component != null)
						{
							num2 = GameStatsManager.Instance.QueryEncounterableDifferentiator(component);
						}
					}
					else if (!string.IsNullOrEmpty(dungeonPlaceableVariant.enemyPlaceableGuid))
					{
						EnemyDatabaseEntry entry = EnemyDatabase.GetEntry(dungeonPlaceableVariant.enemyPlaceableGuid);
						if (entry != null && !string.IsNullOrEmpty(entry.encounterGuid))
						{
							EncounterDatabaseEntry entry2 = EncounterDatabase.GetEntry(entry.encounterGuid);
							if (entry2 != null)
							{
								num2 = GameStatsManager.Instance.QueryEncounterableDifferentiator(entry2);
							}
						}
					}
					if (num2.HasValue)
					{
						if (num2.Value < num)
						{
							list.Clear();
							num = num2.Value;
						}
						else if (num2.Value > num)
						{
							continue;
						}
					}
				}
				list.Add(dungeonPlaceableVariant);
			}
			DungeonPlaceableVariant dungeonPlaceableVariant2 = null;
			ModifyWeightsByDifficulty(list);
			return SelectVariantByWeighting(list);
		}

		private DungeonPlaceableVariant SelectVariantByWeighting(List<DungeonPlaceableVariant> variants)
		{
			float num = 0f;
			float num2 = 0f;
			bool flag = IsAnnexTable;
			if (flag)
			{
				flag = GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON && (bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.MasteryTokensCollectedThisRun > 0;
			}
			for (int i = 0; i < variants.Count; i++)
			{
				if (!flag || !variants[i].nonDatabasePlaceable || !(variants[i].nonDatabasePlaceable.name == "SellPit"))
				{
					num2 += variants[i].GetPercentChance() * variants[i].percentChanceMultiplier;
				}
			}
			float num3 = UnityEngine.Random.value * num2;
			DungeonPlaceableVariant result = null;
			for (int j = 0; j < variants.Count; j++)
			{
				if (!flag || !variants[j].nonDatabasePlaceable || !(variants[j].nonDatabasePlaceable.name == "SellPit"))
				{
					num += variants[j].GetPercentChance() * variants[j].percentChanceMultiplier;
					if (num >= num3)
					{
						result = variants[j];
						break;
					}
				}
			}
			return result;
		}

		public int GetMinimumDifficulty()
		{
			int num = int.MaxValue;
			foreach (DungeonPlaceableVariant variantTier in variantTiers)
			{
				num = Math.Min(num, variantTier.difficultyRating);
			}
			return num;
		}

		public int GetMaximumDifficulty()
		{
			int num = int.MinValue;
			foreach (DungeonPlaceableVariant variantTier in variantTiers)
			{
				num = Math.Max(num, variantTier.difficultyRating);
			}
			return num;
		}
	}
}
