using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class ProceduralFlowModifierData
	{
		public enum FlowModifierPlacementType
		{
			BEFORE_ANY_COMBAT_ROOM,
			END_OF_CHAIN,
			HUB_ADJACENT_CHAIN_START,
			HUB_ADJACENT_NO_LINK,
			RANDOM_NODE_CHILD,
			COMBAT_FRAME,
			NO_LINKS,
			AFTER_BOSS,
			BLACK_MARKET
		}

		public string annotation;

		public bool DEBUG_FORCE_SPAWN;

		public bool OncePerRun;

		public List<FlowModifierPlacementType> placementRules;

		public GenericRoomTable roomTable;

		public PrototypeDungeonRoom exactRoom;

		public bool IsWarpWing;

		public bool RequiresMasteryToken;

		public float chanceToLock;

		public float selectionWeight = 1f;

		public float chanceToSpawn = 1f;

		[SerializeField]
		public DungeonPlaceable RequiredValidPlaceable;

		public DungeonPrerequisite[] prerequisites;

		public bool CanBeForcedSecret = true;

		[Header("For Random Node Child")]
		public int RandomNodeChildMinDistanceFromEntrance;

		[Header("For Combat Frame")]
		public PrototypeDungeonRoom exactSecondaryRoom;

		public int framedCombatNodes;

		public bool PrerequisitesMet
		{
			get
			{
				for (int i = 0; i < prerequisites.Length; i++)
				{
					if (!prerequisites[i].CheckConditionsFulfilled())
					{
						return false;
					}
				}
				if (RequiresMasteryToken && GameManager.HasInstance && (bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.MasteryTokensCollectedThisRun <= 0)
				{
					return false;
				}
				return true;
			}
		}

		public FlowModifierPlacementType GetPlacementRule()
		{
			return placementRules[BraveRandom.GenerationRandomRange(0, placementRules.Count)];
		}
	}
}
