using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeonator
{
	public class LoopFlowBuilder
	{
		public enum FallbackLevel
		{
			NOT_FALLBACK,
			FALLBACK_STANDARD,
			FALLBACK_EMERGENCY
		}

		public Dictionary<DungeonFlowNode, int> usedSubchainData = new Dictionary<DungeonFlowNode, int>();

		public bool DEBUG_RENDER_CANVASES_SEPARATELY;

		protected DungeonFlow m_flow;

		protected LoopDungeonGenerator m_generator;

		protected List<BuilderFlowNode> allBuilderNodes = new List<BuilderFlowNode>();

		protected Dictionary<PrototypeDungeonRoom, int> m_usedPrototypeRoomData = new Dictionary<PrototypeDungeonRoom, int>();

		protected List<PrototypeDungeonRoom> m_excludedRoomData = new List<PrototypeDungeonRoom>();

		protected int m_currentMaxLengthProceduralHallway;

		protected Dictionary<DungeonFlowSubtypeRestriction, int> roomsOfSubtypeRemaining;

		public static ObjectPool<List<BuilderFlowNode>> BuilderFlowNodeListPool = new ObjectPool<List<BuilderFlowNode>>(() => new List<BuilderFlowNode>(), 10);

		protected bool AttachWarpCanvasSuccess;

		protected bool AttachNewCanvasSuccess;

		protected const int MAX_LOOP_REGENERATION_ATTEMPTS = 100;

		protected const int MAX_NONLOOP_REGENERATION_ATTEMPTS = 5;

		public SemioticLayoutManager DeferredGeneratedLayout;

		public bool DeferredGenerationSuccess;

		private List<RuntimeInjectionMetadata> m_postprocessInjectionData = new List<RuntimeInjectionMetadata>();

		private RuntimeInjectionFlags m_runtimeInjectionFlags = new RuntimeInjectionFlags();

		private Dictionary<SharedInjectionData, RuntimeInjectionMetadata> m_previouslyGeneratedRuntimeMetadata = new Dictionary<SharedInjectionData, RuntimeInjectionMetadata>();

		public LoopFlowBuilder(DungeonFlow flow, LoopDungeonGenerator generator)
		{
			m_flow = flow;
			m_generator = generator;
		}

		public BuilderFlowNode ConstructNodeForInjection(PrototypeDungeonRoom exactRoom, ProceduralFlowModifierData modData, RuntimeInjectionMetadata optionalMetadata)
		{
			DungeonFlowNode dungeonFlowNode = new DungeonFlowNode(m_flow);
			dungeonFlowNode.overrideExactRoom = exactRoom;
			dungeonFlowNode.priority = DungeonFlowNode.NodePriority.MANDATORY;
			if (BraveRandom.GenerationRandomValue() < modData.chanceToLock)
			{
				dungeonFlowNode.forcedDoorType = DungeonFlowNode.ForcedDoorType.LOCKED;
			}
			BuilderFlowNode builderFlowNode = new BuilderFlowNode(dungeonFlowNode);
			builderFlowNode.assignedPrototypeRoom = exactRoom;
			builderFlowNode.childBuilderNodes = new List<BuilderFlowNode>();
			builderFlowNode.IsInjectedNode = true;
			if (optionalMetadata != null && optionalMetadata.forceSecret)
			{
				dungeonFlowNode.roomCategory = PrototypeDungeonRoom.RoomCategory.SECRET;
				builderFlowNode.usesOverrideCategory = true;
				builderFlowNode.overrideCategory = PrototypeDungeonRoom.RoomCategory.SECRET;
			}
			return builderFlowNode;
		}

		protected void InjectValidator_RandomCombatRoom(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.parentBuilderNode != null && current.IsOfDepth(modData.RandomNodeChildMinDistanceFromEntrance) && !metastructure.ContainedInBidirectionalLoop(current) && !current.node.isWarpWingEntrance && current.IsStandardCategory && current.assignedPrototypeRoom != null && current.assignedPrototypeRoom.ContainsEnemies)
			{
				validNodes.Add(current);
			}
		}

		protected void InjectValidator_EndOfChain(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.parentBuilderNode != null && !current.node.isWarpWingEntrance && current.node.roomCategory != PrototypeDungeonRoom.RoomCategory.EXIT && current.childBuilderNodes.Count == 0 && current.Category != PrototypeDungeonRoom.RoomCategory.SECRET && (current.parentBuilderNode == null || !current.parentBuilderNode.node.isWarpWingEntrance) && (current.loopConnectedBuilderNode == null || current.node.loopTargetIsOneWay))
			{
				validNodes.Add(current);
			}
		}

		protected void InjectValidator_HubAdjacentChainStart(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.parentBuilderNode != null && !current.node.isWarpWingEntrance && current.parentBuilderNode.Category == PrototypeDungeonRoom.RoomCategory.HUB)
			{
				validNodes.Add(current);
			}
		}

		protected void InjectValidator_HubAdjacentNoLink(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.Category == PrototypeDungeonRoom.RoomCategory.HUB)
			{
				validNodes.Add(current);
			}
		}

		protected void InjectValidator_RandomNodeChild(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.IsStandardCategory && !current.node.isWarpWingEntrance && current.node.roomCategory != PrototypeDungeonRoom.RoomCategory.EXIT && current.IsOfDepth(modData.RandomNodeChildMinDistanceFromEntrance) && (current.parentBuilderNode == null || !current.parentBuilderNode.node.isWarpWingEntrance))
			{
				validNodes.Add(current);
			}
		}

		protected void InjectValidator_AfterBoss(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.parentBuilderNode != null && !current.node.isWarpWingEntrance && current.parentBuilderNode.Category == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				validNodes.Add(current);
			}
		}

		protected void InjectValidator_BlackMarket(BuilderFlowNode current, List<BuilderFlowNode> validNodes, ProceduralFlowModifierData modData, FlowCompositeMetastructure metastructure)
		{
			if (current.assignedPrototypeRoom != null && current.assignedPrototypeRoom.name.Contains("Black Market"))
			{
				validNodes.Add(current);
			}
		}

		protected void InjectNodeNoLinks(ProceduralFlowModifierData modData, PrototypeDungeonRoom exactRoom, BuilderFlowNode root, FlowCompositeMetastructure metastructure, RuntimeInjectionMetadata optionalMetadata)
		{
			BuilderFlowNode builderFlowNode = ConstructNodeForInjection(exactRoom, modData, optionalMetadata);
			builderFlowNode.node.isWarpWingEntrance = true;
			builderFlowNode.node.handlesOwnWarping = true;
			root.childBuilderNodes.Add(builderFlowNode);
			builderFlowNode.parentBuilderNode = root;
			builderFlowNode.InjectionTarget = root;
			allBuilderNodes.Add(builderFlowNode);
		}

		protected bool InjectNodeBefore(ProceduralFlowModifierData modData, PrototypeDungeonRoom exactRoom, BuilderFlowNode root, Action<BuilderFlowNode, List<BuilderFlowNode>, ProceduralFlowModifierData, FlowCompositeMetastructure> validator, FlowCompositeMetastructure metastructure, RuntimeInjectionMetadata optionalMetadata)
		{
			optionalMetadata.forceSecret = false;
			BuilderFlowNode builderFlowNode = ConstructNodeForInjection(exactRoom, modData, optionalMetadata);
			List<BuilderFlowNode> list = new List<BuilderFlowNode>();
			Stack<BuilderFlowNode> stack = new Stack<BuilderFlowNode>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				BuilderFlowNode builderFlowNode2 = stack.Pop();
				validator(builderFlowNode2, list, modData, metastructure);
				for (int i = 0; i < builderFlowNode2.childBuilderNodes.Count; i++)
				{
					stack.Push(builderFlowNode2.childBuilderNodes[i]);
				}
			}
			if (list.Count <= 0)
			{
				return false;
			}
			BuilderFlowNode builderFlowNode3 = list[BraveRandom.GenerationRandomRange(0, list.Count)];
			BuilderFlowNode parentBuilderNode = builderFlowNode3.parentBuilderNode;
			parentBuilderNode.childBuilderNodes.Remove(builderFlowNode3);
			parentBuilderNode.childBuilderNodes.Add(builderFlowNode);
			builderFlowNode.parentBuilderNode = parentBuilderNode;
			builderFlowNode3.parentBuilderNode = builderFlowNode;
			builderFlowNode.childBuilderNodes.Add(builderFlowNode3);
			builderFlowNode.InjectionTarget = builderFlowNode3;
			allBuilderNodes.Add(builderFlowNode);
			return true;
		}

		protected bool InjectNodeAfter(ProceduralFlowModifierData modData, PrototypeDungeonRoom exactRoom, BuilderFlowNode root, Action<BuilderFlowNode, List<BuilderFlowNode>, ProceduralFlowModifierData, FlowCompositeMetastructure> validator, FlowCompositeMetastructure metastructure, RuntimeInjectionMetadata optionalMetadata)
		{
			BuilderFlowNode builderFlowNode = ConstructNodeForInjection(exactRoom, modData, optionalMetadata);
			builderFlowNode.node.isWarpWingEntrance = modData.IsWarpWing;
			List<BuilderFlowNode> list = new List<BuilderFlowNode>();
			Stack<BuilderFlowNode> stack = new Stack<BuilderFlowNode>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				BuilderFlowNode builderFlowNode2 = stack.Pop();
				validator(builderFlowNode2, list, modData, metastructure);
				for (int i = 0; i < builderFlowNode2.childBuilderNodes.Count; i++)
				{
					stack.Push(builderFlowNode2.childBuilderNodes[i]);
				}
			}
			if (list.Count <= 0)
			{
				return false;
			}
			BuilderFlowNode builderFlowNode3 = list[BraveRandom.GenerationRandomRange(0, list.Count)];
			builderFlowNode3.childBuilderNodes.Add(builderFlowNode);
			builderFlowNode.parentBuilderNode = builderFlowNode3;
			builderFlowNode.childBuilderNodes = new List<BuilderFlowNode>();
			builderFlowNode.InjectionTarget = builderFlowNode3;
			allBuilderNodes.Add(builderFlowNode);
			return true;
		}

		protected void RecurseCombatRooms(BuilderFlowNode currentCheckNode, List<BuilderFlowNode> currentSequence, int desiredDepth, List<List<BuilderFlowNode>> validSequences)
		{
			bool flag = currentSequence.Count == desiredDepth - 1 || currentCheckNode.childBuilderNodes.Count > 0;
			if ((currentSequence.Count == desiredDepth - 1 && currentCheckNode.loopConnectedBuilderNode != null) || !currentCheckNode.IsStandardCategory || !(currentCheckNode.assignedPrototypeRoom != null) || !currentCheckNode.assignedPrototypeRoom.ContainsEnemies || !flag)
			{
				return;
			}
			List<BuilderFlowNode> list = new List<BuilderFlowNode>(currentSequence);
			list.Add(currentCheckNode);
			if (list.Count == desiredDepth)
			{
				validSequences.Add(list);
				return;
			}
			for (int i = 0; i < currentCheckNode.childBuilderNodes.Count; i++)
			{
				RecurseCombatRooms(currentCheckNode.childBuilderNodes[i], list, desiredDepth, validSequences);
			}
		}

		protected void HandleInjectionFrame(ProceduralFlowModifierData modData, BuilderFlowNode root, RuntimeInjectionMetadata optionalMetadata, FlowCompositeMetastructure metastructure)
		{
			int framedCombatNodes = modData.framedCombatNodes;
			optionalMetadata.forceSecret = false;
			BuilderFlowNode builderFlowNode = ConstructNodeForInjection(modData.exactRoom, modData, optionalMetadata);
			BuilderFlowNode builderFlowNode2 = ConstructNodeForInjection(modData.exactSecondaryRoom, modData, optionalMetadata);
			List<List<BuilderFlowNode>> list = new List<List<BuilderFlowNode>>();
			Stack<BuilderFlowNode> stack = new Stack<BuilderFlowNode>();
			stack.Push(root);
			List<BuilderFlowNode> currentSequence = new List<BuilderFlowNode>();
			while (stack.Count > 0)
			{
				BuilderFlowNode builderFlowNode3 = stack.Pop();
				RecurseCombatRooms(builderFlowNode3, currentSequence, framedCombatNodes, list);
				for (int i = 0; i < builderFlowNode3.childBuilderNodes.Count; i++)
				{
					stack.Push(builderFlowNode3.childBuilderNodes[i]);
				}
			}
			if (list.Count > 0)
			{
				List<BuilderFlowNode> list2 = list[BraveRandom.GenerationRandomRange(0, list.Count)];
				List<BuilderFlowNode> list3 = new List<BuilderFlowNode>();
				list3.Add(builderFlowNode);
				list3.AddRange(list2);
				list3.Add(builderFlowNode2);
				BuilderFlowNode builderFlowNode4 = list2[0];
				BuilderFlowNode parentBuilderNode = builderFlowNode4.parentBuilderNode;
				parentBuilderNode.childBuilderNodes.Remove(builderFlowNode4);
				parentBuilderNode.childBuilderNodes.Add(builderFlowNode);
				builderFlowNode.parentBuilderNode = parentBuilderNode;
				builderFlowNode4.parentBuilderNode = builderFlowNode;
				builderFlowNode.childBuilderNodes.Add(builderFlowNode4);
				builderFlowNode.InjectionFrameSequence = list3;
				allBuilderNodes.Add(builderFlowNode);
				BuilderFlowNode builderFlowNode5 = list2[list2.Count - 1];
				builderFlowNode5.childBuilderNodes.Add(builderFlowNode2);
				builderFlowNode2.parentBuilderNode = builderFlowNode5;
				builderFlowNode2.childBuilderNodes = new List<BuilderFlowNode>();
				builderFlowNode2.InjectionFrameSequence = list3;
				allBuilderNodes.Add(builderFlowNode2);
			}
		}

		protected bool ProcessSingleNodeInjection(ProceduralFlowModifierData currentInjectionData, BuilderFlowNode root, RuntimeInjectionFlags injectionFlags, FlowCompositeMetastructure metastructure, RuntimeInjectionMetadata optionalMetadata = null)
		{
			bool flag = false;
			if (currentInjectionData.RequiredValidPlaceable != null && !currentInjectionData.RequiredValidPlaceable.HasValidPlaceable())
			{
				if (flag)
				{
					Debug.LogError("Failing Injection because " + currentInjectionData.RequiredValidPlaceable.name + " has no valid placeable.");
				}
				return false;
			}
			bool flag2 = false;
			if (!flag2 && !currentInjectionData.PrerequisitesMet)
			{
				if (flag)
				{
					Debug.Log("Failing Injection because " + currentInjectionData.annotation + " has unmet prerequisites.");
				}
				return false;
			}
			if (!flag2 && currentInjectionData.exactRoom != null && !currentInjectionData.exactRoom.CheckPrerequisites())
			{
				if (flag)
				{
					Debug.Log("Failing Injection because " + currentInjectionData.exactRoom.name + " has unmet prerequisites.");
				}
				return false;
			}
			PrototypeDungeonRoom prototypeDungeonRoom = null;
			prototypeDungeonRoom = currentInjectionData.exactRoom;
			if (currentInjectionData.roomTable != null && currentInjectionData.exactRoom == null)
			{
				WeightedRoom weightedRoom = currentInjectionData.roomTable.SelectByWeight();
				if (weightedRoom != null)
				{
					prototypeDungeonRoom = weightedRoom.room;
				}
			}
			if (prototypeDungeonRoom == null)
			{
				if (currentInjectionData.roomTable != null)
				{
					if (flag)
					{
						Debug.Log("Failing Injection because " + currentInjectionData.roomTable.name + " has no valid rooms in its table.");
					}
					return false;
				}
				if (flag)
				{
					Debug.Log("Failing Injection because " + currentInjectionData.annotation + " is a NULL room injection!");
				}
				return true;
			}
			if (optionalMetadata != null && optionalMetadata.SucceededRandomizationCheckMap.ContainsKey(currentInjectionData))
			{
				if (!optionalMetadata.SucceededRandomizationCheckMap[currentInjectionData])
				{
					if (flag)
					{
						Debug.Log("Failing Injection on " + currentInjectionData.annotation + " by CACHED RNG.");
					}
					return false;
				}
			}
			else
			{
				if (!flag2 && BraveRandom.GenerationRandomValue() > currentInjectionData.chanceToSpawn)
				{
					if (flag)
					{
						Debug.Log("Failing Injection on " + currentInjectionData.annotation + " by RNG.");
					}
					if (optionalMetadata != null)
					{
						optionalMetadata.SucceededRandomizationCheckMap.Add(currentInjectionData, false);
					}
					return false;
				}
				if (optionalMetadata != null)
				{
					optionalMetadata.SucceededRandomizationCheckMap.Add(currentInjectionData, true);
				}
			}
			if (!flag2 && !prototypeDungeonRoom.injectionFlags.IsValid(injectionFlags))
			{
				if (flag)
				{
					Debug.Log("Failing Injection because " + prototypeDungeonRoom.name + " has invalid injection flags state.");
				}
				return false;
			}
			if (injectionFlags.Merge(prototypeDungeonRoom.injectionFlags))
			{
				Debug.Log("Assigning FIREPLACE from room: " + prototypeDungeonRoom.name);
			}
			ProceduralFlowModifierData.FlowModifierPlacementType flowModifierPlacementType = currentInjectionData.GetPlacementRule();
			if (optionalMetadata != null && optionalMetadata.forceSecret && !currentInjectionData.DEBUG_FORCE_SPAWN)
			{
				if (!currentInjectionData.CanBeForcedSecret)
				{
					if (flag)
					{
						Debug.Log("Failing Injection because " + currentInjectionData.annotation + " cannot be forced SECRET.");
					}
					return false;
				}
				flowModifierPlacementType = ProceduralFlowModifierData.FlowModifierPlacementType.RANDOM_NODE_CHILD;
			}
			if (flag && prototypeDungeonRoom != null)
			{
				Debug.Log("Succeeding injection of room : " + prototypeDungeonRoom.name);
			}
			bool flag3 = true;
			switch (flowModifierPlacementType)
			{
			case ProceduralFlowModifierData.FlowModifierPlacementType.BEFORE_ANY_COMBAT_ROOM:
				flag3 = InjectNodeBefore(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_RandomCombatRoom, metastructure, optionalMetadata);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.END_OF_CHAIN:
				flag3 = InjectNodeAfter(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_EndOfChain, metastructure, optionalMetadata);
				if (!flag3)
				{
					flag3 = InjectNodeAfter(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_RandomNodeChild, metastructure, optionalMetadata);
				}
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.HUB_ADJACENT_CHAIN_START:
				flag3 = InjectNodeBefore(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_HubAdjacentChainStart, metastructure, optionalMetadata);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.HUB_ADJACENT_NO_LINK:
				flag3 = InjectNodeAfter(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_HubAdjacentNoLink, metastructure, optionalMetadata);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.RANDOM_NODE_CHILD:
				flag3 = InjectNodeAfter(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_RandomNodeChild, metastructure, optionalMetadata);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.COMBAT_FRAME:
				HandleInjectionFrame(currentInjectionData, root, optionalMetadata, metastructure);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.NO_LINKS:
				InjectNodeNoLinks(currentInjectionData, prototypeDungeonRoom, root, metastructure, optionalMetadata);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.AFTER_BOSS:
				flag3 = InjectNodeBefore(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_AfterBoss, metastructure, optionalMetadata);
				break;
			case ProceduralFlowModifierData.FlowModifierPlacementType.BLACK_MARKET:
				flag3 = InjectNodeAfter(currentInjectionData, prototypeDungeonRoom, root, InjectValidator_BlackMarket, metastructure, optionalMetadata);
				break;
			}
			if (flag3 && prototypeDungeonRoom.requiredInjectionData != null)
			{
				RuntimeInjectionMetadata sourceMetadata = new RuntimeInjectionMetadata(prototypeDungeonRoom.requiredInjectionData);
				HandleNodeInjection(root, sourceMetadata, injectionFlags, metastructure);
			}
			return flag3;
		}

		protected void HandleNodeInjection(BuilderFlowNode root, RuntimeInjectionMetadata sourceMetadata, RuntimeInjectionFlags injectionFlags, FlowCompositeMetastructure metastructure)
		{
			SharedInjectionData injectionData = sourceMetadata.injectionData;
			if (injectionData != null && injectionData.InjectionData.Count > 0)
			{
				List<int> input = Enumerable.Range(0, injectionData.InjectionData.Count).ToList();
				input = input.GenerationShuffle();
				if (injectionData.OnlyOne)
				{
					ProceduralFlowModifierData proceduralFlowModifierData = null;
					float num = injectionData.ChanceToSpawnOne;
					bool flag = false;
					if (injectionData.IsNPCCell)
					{
						num += (float)GameStatsManager.Instance.NumberRunsValidCellWithoutSpawn / 50f;
						if (MetaInjectionData.CellGeneratedForCurrentBlueprint || BraveRandom.IgnoreGenerationDifferentiator)
						{
							num = 0f;
						}
						if (injectionData.InjectionData.Count > 1 && GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
						{
							num = 0f;
						}
						if (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON && !GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_ACTIVE_IN_FOYER))
						{
							flag = true;
							num = 1f;
						}
					}
					if (BraveRandom.GenerationRandomValue() < num)
					{
						float num2 = 0f;
						for (int i = 0; i < injectionData.InjectionData.Count; i++)
						{
							ProceduralFlowModifierData proceduralFlowModifierData2 = injectionData.InjectionData[i];
							if ((!proceduralFlowModifierData2.OncePerRun || !MetaInjectionData.InjectionSetsUsedThisRun.Contains(proceduralFlowModifierData2)) && (!injectionData.IsNPCCell || proceduralFlowModifierData2.PrerequisitesMet) && (!injectionData.IgnoreUnmetPrerequisiteEntries || proceduralFlowModifierData2.PrerequisitesMet))
							{
								num2 += proceduralFlowModifierData2.selectionWeight;
							}
						}
						float num3 = BraveRandom.GenerationRandomValue() * num2;
						float num4 = 0f;
						ProceduralFlowModifierData lostAdventurerSet;
						if (sourceMetadata != null && sourceMetadata.HasAssignedModDataExactRoom)
						{
							proceduralFlowModifierData = sourceMetadata.AssignedModifierData;
						}
						else if (ShouldDoLostAdventurerHelp(injectionData, out lostAdventurerSet))
						{
							proceduralFlowModifierData = lostAdventurerSet;
							if (flag)
							{
								proceduralFlowModifierData = injectionData.InjectionData[0];
							}
						}
						else
						{
							for (int j = 0; j < injectionData.InjectionData.Count; j++)
							{
								ProceduralFlowModifierData proceduralFlowModifierData3 = injectionData.InjectionData[j];
								if ((!proceduralFlowModifierData3.OncePerRun || !MetaInjectionData.InjectionSetsUsedThisRun.Contains(proceduralFlowModifierData3)) && (!injectionData.IsNPCCell || proceduralFlowModifierData3.PrerequisitesMet) && (!injectionData.IgnoreUnmetPrerequisiteEntries || proceduralFlowModifierData3.PrerequisitesMet))
								{
									num4 += proceduralFlowModifierData3.selectionWeight;
									if (num4 > num3)
									{
										proceduralFlowModifierData = proceduralFlowModifierData3;
										break;
									}
								}
							}
							if (flag)
							{
								proceduralFlowModifierData = injectionData.InjectionData[0];
							}
						}
						if (sourceMetadata != null && !sourceMetadata.HasAssignedModDataExactRoom)
						{
							sourceMetadata.HasAssignedModDataExactRoom = true;
							if (proceduralFlowModifierData != null)
							{
								Debug.Log("Assigning METADATA: " + proceduralFlowModifierData.annotation);
							}
							sourceMetadata.AssignedModifierData = proceduralFlowModifierData;
							if (proceduralFlowModifierData != null && proceduralFlowModifierData.OncePerRun)
							{
								MetaInjectionData.InjectionSetsUsedThisRun.Add(proceduralFlowModifierData);
							}
						}
						if (proceduralFlowModifierData != null && !ProcessSingleNodeInjection(proceduralFlowModifierData, root, injectionFlags, metastructure, sourceMetadata))
						{
							proceduralFlowModifierData = null;
						}
					}
				}
				else
				{
					for (int k = 0; k < injectionData.InjectionData.Count; k++)
					{
						ProceduralFlowModifierData currentInjectionData = injectionData.InjectionData[input[k]];
						bool flag2 = ProcessSingleNodeInjection(currentInjectionData, root, injectionFlags, metastructure, sourceMetadata);
					}
				}
			}
			if (injectionData != null && injectionData.AttachedInjectionData.Count > 0)
			{
				for (int l = 0; l < injectionData.AttachedInjectionData.Count; l++)
				{
					RuntimeInjectionMetadata runtimeInjectionMetadata = new RuntimeInjectionMetadata(injectionData.AttachedInjectionData[l]);
					runtimeInjectionMetadata.CopyMetadata(sourceMetadata);
					HandleNodeInjection(root, runtimeInjectionMetadata, injectionFlags, metastructure);
				}
			}
		}

		private bool ShouldDoLostAdventurerHelp(SharedInjectionData injectionData, out ProceduralFlowModifierData lostAdventurerSet)
		{
			lostAdventurerSet = null;
			for (int i = 0; i < injectionData.InjectionData.Count; i++)
			{
				ProceduralFlowModifierData proceduralFlowModifierData = injectionData.InjectionData[i];
				if ((!proceduralFlowModifierData.OncePerRun || !MetaInjectionData.InjectionSetsUsedThisRun.Contains(proceduralFlowModifierData)) && (!injectionData.IsNPCCell || proceduralFlowModifierData.PrerequisitesMet) && (!injectionData.IgnoreUnmetPrerequisiteEntries || proceduralFlowModifierData.PrerequisitesMet) && !(proceduralFlowModifierData.annotation != "lost adventurer"))
				{
					GungeonFlags? gungeonFlags = LostAdventurerGetFlagFromFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId);
					if (!gungeonFlags.HasValue)
					{
						return false;
					}
					if (LostAdventurerGetFloorsHelped() == 4 && !GameStatsManager.Instance.GetFlag(gungeonFlags.Value))
					{
						lostAdventurerSet = proceduralFlowModifierData;
						return true;
					}
					return false;
				}
			}
			return false;
		}

		private GungeonFlags? LostAdventurerGetFlagFromFloor(GlobalDungeonData.ValidTilesets floor)
		{
			switch (floor)
			{
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				return GungeonFlags.LOST_ADVENTURER_HELPED_CASTLE;
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				return GungeonFlags.LOST_ADVENTURER_HELPED_GUNGEON;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
				return GungeonFlags.LOST_ADVENTURER_HELPED_MINES;
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
				return GungeonFlags.LOST_ADVENTURER_HELPED_CATACOMBS;
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				return GungeonFlags.LOST_ADVENTURER_HELPED_FORGE;
			default:
				return null;
			}
		}

		private int LostAdventurerGetFloorsHelped()
		{
			List<GungeonFlags> list = new List<GungeonFlags>(new GungeonFlags[5]
			{
				GungeonFlags.LOST_ADVENTURER_HELPED_CASTLE,
				GungeonFlags.LOST_ADVENTURER_HELPED_GUNGEON,
				GungeonFlags.LOST_ADVENTURER_HELPED_MINES,
				GungeonFlags.LOST_ADVENTURER_HELPED_CATACOMBS,
				GungeonFlags.LOST_ADVENTURER_HELPED_FORGE
			});
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (GameStatsManager.Instance.GetFlag(list[i]))
				{
					num++;
				}
			}
			return num;
		}

		protected void HandleNodeInjection(BuilderFlowNode root, List<ProceduralFlowModifierData> flowInjectionData, RuntimeInjectionFlags injectionFlags, FlowCompositeMetastructure metastructure)
		{
			if (flowInjectionData != null && flowInjectionData.Count > 0)
			{
				for (int i = 0; i < flowInjectionData.Count; i++)
				{
					ProceduralFlowModifierData currentInjectionData = flowInjectionData[i];
					ProcessSingleNodeInjection(currentInjectionData, root, injectionFlags, metastructure);
				}
			}
		}

		protected BuilderFlowNode ComposeFlowTree()
		{
			Stack<BuilderFlowNode> stack = new Stack<BuilderFlowNode>();
			stack.Push(new BuilderFlowNode(m_flow.FirstNode));
			BuilderFlowNode builderFlowNode = stack.Peek();
			int num = (builderFlowNode.identifier = 0) + 1;
			while (stack.Count > 0)
			{
				BuilderFlowNode builderFlowNode2 = stack.Pop();
				if (builderFlowNode2.childBuilderNodes == null)
				{
					builderFlowNode2.childBuilderNodes = m_flow.NewGetNodeChildrenToBuild(builderFlowNode2, this);
				}
				allBuilderNodes.Add(builderFlowNode2);
				for (int i = 0; i < builderFlowNode2.childBuilderNodes.Count; i++)
				{
					if (!stack.Contains(builderFlowNode2.childBuilderNodes[i]))
					{
						if (builderFlowNode2.childBuilderNodes[i].identifier < 0)
						{
							builderFlowNode2.childBuilderNodes[i].identifier = num;
							num++;
						}
						else
						{
							Debug.Log("assigning already-assigned identifier");
						}
						stack.Push(builderFlowNode2.childBuilderNodes[i]);
					}
				}
			}
			for (int j = 0; j < allBuilderNodes.Count; j++)
			{
				if (string.IsNullOrEmpty(allBuilderNodes[j].node.loopTargetNodeGuid))
				{
					continue;
				}
				DungeonFlowNode nodeFromGuid = m_flow.GetNodeFromGuid(allBuilderNodes[j].node.loopTargetNodeGuid);
				for (int k = 0; k < allBuilderNodes.Count; k++)
				{
					if (allBuilderNodes[k].node == nodeFromGuid)
					{
						allBuilderNodes[j].loopConnectedBuilderNode = allBuilderNodes[k];
						allBuilderNodes[k].loopConnectedBuilderNode = allBuilderNodes[j];
					}
				}
			}
			return builderFlowNode;
		}

		protected BuilderFlowNode RerootTreeAtHighestConnectivity(BuilderFlowNode root)
		{
			int connectivity = root.Connectivity;
			BuilderFlowNode builderFlowNode = root;
			Queue<BuilderFlowNode> queue = new Queue<BuilderFlowNode>();
			queue.Enqueue(root);
			while (queue.Count > 0)
			{
				BuilderFlowNode builderFlowNode2 = queue.Dequeue();
				if (builderFlowNode2.Connectivity > connectivity)
				{
					connectivity = builderFlowNode2.Connectivity;
					builderFlowNode = builderFlowNode2;
				}
				for (int i = 0; i < builderFlowNode2.childBuilderNodes.Count; i++)
				{
					queue.Enqueue(builderFlowNode2.childBuilderNodes[i]);
				}
			}
			builderFlowNode.MakeNodeTreeRoot();
			return builderFlowNode;
		}

		protected void PerformOperationOnTreeNodes(BuilderFlowNode root, Action<BuilderFlowNode> action)
		{
			Queue<BuilderFlowNode> queue = new Queue<BuilderFlowNode>();
			queue.Enqueue(root);
			while (queue.Count > 0)
			{
				BuilderFlowNode builderFlowNode = queue.Dequeue();
				action(builderFlowNode);
				for (int i = 0; i < builderFlowNode.childBuilderNodes.Count; i++)
				{
					queue.Enqueue(builderFlowNode.childBuilderNodes[i]);
				}
			}
		}

		protected DungeonFlowSubtypeRestriction GetSubtypeRestrictionFromRoom(PrototypeDungeonRoom room)
		{
			foreach (DungeonFlowSubtypeRestriction key in roomsOfSubtypeRemaining.Keys)
			{
				if (key.baseCategoryRestriction == room.category && ((room.category == PrototypeDungeonRoom.RoomCategory.BOSS && room.subCategoryBoss == key.bossSubcategoryRestriction) || (room.category == PrototypeDungeonRoom.RoomCategory.NORMAL && room.subCategoryNormal == key.normalSubcategoryRestriction) || (room.category == PrototypeDungeonRoom.RoomCategory.SPECIAL && room.subCategorySpecial == key.specialSubcategoryRestriction) || (room.category == PrototypeDungeonRoom.RoomCategory.SECRET && room.subCategorySecret == key.secretSubcategoryRestriction)))
				{
					return key;
				}
			}
			return null;
		}

		protected bool CheckRoomAgainstRestrictedSubtypes(PrototypeDungeonRoom room)
		{
			DungeonFlowSubtypeRestriction subtypeRestrictionFromRoom = GetSubtypeRestrictionFromRoom(room);
			if (subtypeRestrictionFromRoom != null && roomsOfSubtypeRemaining[subtypeRestrictionFromRoom] <= 0)
			{
				return true;
			}
			return false;
		}

		protected List<WeightedRoom> GetViableAvailableRooms(PrototypeDungeonRoom.RoomCategory category, int requiredExits, List<WeightedRoom> source, out float totalAvailableWeight, FallbackLevel fallback = FallbackLevel.NOT_FALLBACK)
		{
			List<WeightedRoom> list = new List<WeightedRoom>();
			List<int> list2 = Enumerable.Range(0, source.Count).ToList().GenerationShuffle();
			totalAvailableWeight = 0f;
			for (int i = 0; i < source.Count; i++)
			{
				int index = list2[i];
				WeightedRoom weightedRoom = source[index];
				PrototypeDungeonRoom room = weightedRoom.room;
				float num = weightedRoom.weight;
				if (!(weightedRoom.room == null) && !CheckRoomAgainstRestrictedSubtypes(room) && room.exitData.exits.Count >= requiredExits && (requiredExits != 1 || room.category != PrototypeDungeonRoom.RoomCategory.NORMAL || room.subCategoryNormal != PrototypeDungeonRoom.RoomNormalSubCategory.TRAP) && (!Enum.IsDefined(typeof(PrototypeDungeonRoom.RoomCategory), category) || room.category == category) && (fallback != 0 || weightedRoom.room.ForceAllowDuplicates || !m_usedPrototypeRoomData.ContainsKey(weightedRoom.room)))
				{
					int num2 = GameStatsManager.Instance.QueryRoomDifferentiator(weightedRoom.room);
					if (fallback == FallbackLevel.NOT_FALLBACK && !weightedRoom.room.ForceAllowDuplicates && weightedRoom.room.category != PrototypeDungeonRoom.RoomCategory.SPECIAL && num2 > 0)
					{
						num *= Mathf.Clamp01(1f - 0.33f * (float)num2);
					}
					if (!m_excludedRoomData.Contains(weightedRoom.room) && weightedRoom.CheckPrerequisites() && room.CheckPrerequisites() && room.injectionFlags.IsValid(m_runtimeInjectionFlags) && (fallback == FallbackLevel.FALLBACK_EMERGENCY || category == PrototypeDungeonRoom.RoomCategory.NORMAL || !weightedRoom.limitedCopies || !m_usedPrototypeRoomData.ContainsKey(weightedRoom.room) || m_usedPrototypeRoomData[weightedRoom.room] < weightedRoom.maxCopies))
					{
						list.Add(weightedRoom);
						totalAvailableWeight += num;
					}
				}
			}
			return list;
		}

		public PrototypeDungeonRoom GetAvailableRoomByExitDirection(PrototypeDungeonRoom.RoomCategory category, int requiredExits, List<DungeonData.Direction> exitDirections, List<WeightedRoom> source, FallbackLevel fallback = FallbackLevel.NOT_FALLBACK)
		{
			float totalAvailableWeight = 0f;
			List<WeightedRoom> viableAvailableRooms = GetViableAvailableRooms(category, requiredExits, source, out totalAvailableWeight, fallback);
			for (int i = 0; i < viableAvailableRooms.Count; i++)
			{
				WeightedRoom weightedRoom = viableAvailableRooms[i];
				bool flag = false;
				for (int j = 0; j < weightedRoom.room.exitData.exits.Count; j++)
				{
					PrototypeRoomExit prototypeRoomExit = weightedRoom.room.exitData.exits[j];
					if (exitDirections.Contains(prototypeRoomExit.exitDirection))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					totalAvailableWeight -= weightedRoom.weight;
					viableAvailableRooms.RemoveAt(i);
					i--;
				}
			}
			if (viableAvailableRooms.Count == 0 && fallback == FallbackLevel.NOT_FALLBACK)
			{
				return GetAvailableRoomByExitDirection(category, requiredExits, exitDirections, source, FallbackLevel.FALLBACK_STANDARD);
			}
			if (viableAvailableRooms.Count == 0 && fallback == FallbackLevel.FALLBACK_EMERGENCY)
			{
				return GetAvailableRoomByExitDirection(category, requiredExits, exitDirections, source, FallbackLevel.FALLBACK_EMERGENCY);
			}
			if (viableAvailableRooms.Count == 0)
			{
				if (category == PrototypeDungeonRoom.RoomCategory.CONNECTOR)
				{
					return GetAvailableRoomByExitDirection(PrototypeDungeonRoom.RoomCategory.NORMAL, requiredExits, exitDirections, source);
				}
				Debug.LogError("Falling back due to lack of non-duplicate rooms FAILED. This should never happen.");
			}
			float num = BraveRandom.GenerationRandomValue() * totalAvailableWeight;
			for (int k = 0; k < viableAvailableRooms.Count; k++)
			{
				num -= viableAvailableRooms[k].weight;
				if (num <= 0f)
				{
					return viableAvailableRooms[k].room;
				}
			}
			if (viableAvailableRooms == null || viableAvailableRooms.Count == 0)
			{
				return null;
			}
			return viableAvailableRooms[0].room;
		}

		public PrototypeDungeonRoom GetAvailableRoom(PrototypeDungeonRoom.RoomCategory category, int requiredExits, List<WeightedRoom> source, FallbackLevel fallback = FallbackLevel.NOT_FALLBACK)
		{
			float totalAvailableWeight = 0f;
			List<WeightedRoom> viableAvailableRooms = GetViableAvailableRooms(category, requiredExits, source, out totalAvailableWeight, fallback);
			if (viableAvailableRooms.Count == 0 && fallback == FallbackLevel.NOT_FALLBACK)
			{
				return GetAvailableRoom(category, requiredExits, source, FallbackLevel.FALLBACK_STANDARD);
			}
			if (viableAvailableRooms.Count == 0 && fallback == FallbackLevel.FALLBACK_STANDARD)
			{
				return GetAvailableRoom(category, requiredExits, source, FallbackLevel.FALLBACK_EMERGENCY);
			}
			if (viableAvailableRooms.Count == 0)
			{
				switch (category)
				{
				case PrototypeDungeonRoom.RoomCategory.CONNECTOR:
				case PrototypeDungeonRoom.RoomCategory.HUB:
					Debug.LogError("Replacing failed CONNECTOR/HUB room with room of type NORMAL.");
					return GetAvailableRoom(PrototypeDungeonRoom.RoomCategory.NORMAL, requiredExits, source);
				default:
					Debug.LogError("Falling back due to lack of non-duplicate rooms (" + requiredExits + "," + source.Count + ") in list of length: " + source.Count + ". FAILED: " + category.ToString() + ". This should never happen.");
					break;
				case PrototypeDungeonRoom.RoomCategory.SECRET:
					break;
				}
				return null;
			}
			float num = BraveRandom.GenerationRandomValue() * totalAvailableWeight;
			for (int i = 0; i < viableAvailableRooms.Count; i++)
			{
				num -= viableAvailableRooms[i].weight;
				if (num <= 0f)
				{
					return viableAvailableRooms[i].room;
				}
			}
			return viableAvailableRooms[0].room;
		}

		public void ClearPlacedRoomData(BuilderFlowNode buildData)
		{
			if (!(buildData.assignedPrototypeRoom != null))
			{
				return;
			}
			DungeonFlowSubtypeRestriction subtypeRestrictionFromRoom = GetSubtypeRestrictionFromRoom(buildData.assignedPrototypeRoom);
			if (subtypeRestrictionFromRoom != null)
			{
				roomsOfSubtypeRemaining[subtypeRestrictionFromRoom] += 1;
			}
			if (m_usedPrototypeRoomData.ContainsKey(buildData.assignedPrototypeRoom))
			{
				if (m_usedPrototypeRoomData[buildData.assignedPrototypeRoom] > 1)
				{
					m_usedPrototypeRoomData[buildData.assignedPrototypeRoom] = m_usedPrototypeRoomData[buildData.assignedPrototypeRoom] - 1;
				}
				else
				{
					m_usedPrototypeRoomData.Remove(buildData.assignedPrototypeRoom);
				}
			}
			for (int i = 0; i < buildData.assignedPrototypeRoom.excludedOtherRooms.Count; i++)
			{
				m_excludedRoomData.Remove(buildData.assignedPrototypeRoom.excludedOtherRooms[i]);
			}
			if (buildData.assignedPrototypeRoom.injectionFlags.CastleFireplace)
			{
				m_runtimeInjectionFlags.CastleFireplace = false;
			}
			buildData.assignedPrototypeRoom = null;
		}

		private bool PostprocessInjectionDataContains(SharedInjectionData test)
		{
			for (int i = 0; i < m_postprocessInjectionData.Count; i++)
			{
				if (m_postprocessInjectionData[i].injectionData == test)
				{
					return true;
				}
			}
			return false;
		}

		public void NotifyPlacedRoomData(PrototypeDungeonRoom assignedRoom)
		{
			PrototypeDungeonRoom prototypeDungeonRoom = ((!(assignedRoom.MirrorSource != null)) ? assignedRoom : assignedRoom.MirrorSource);
			DungeonFlowSubtypeRestriction subtypeRestrictionFromRoom = GetSubtypeRestrictionFromRoom(prototypeDungeonRoom);
			if (subtypeRestrictionFromRoom != null)
			{
				roomsOfSubtypeRemaining[subtypeRestrictionFromRoom] -= 1;
			}
			if (m_usedPrototypeRoomData.ContainsKey(prototypeDungeonRoom))
			{
				m_usedPrototypeRoomData[prototypeDungeonRoom] += 1;
			}
			else
			{
				m_usedPrototypeRoomData.Add(prototypeDungeonRoom, 1);
			}
			for (int i = 0; i < prototypeDungeonRoom.excludedOtherRooms.Count; i++)
			{
				m_excludedRoomData.Add(prototypeDungeonRoom.excludedOtherRooms[i]);
			}
			if (prototypeDungeonRoom.requiredInjectionData != null && !PostprocessInjectionDataContains(prototypeDungeonRoom.requiredInjectionData))
			{
				m_postprocessInjectionData.Add(new RuntimeInjectionMetadata(prototypeDungeonRoom.requiredInjectionData));
			}
			if (m_runtimeInjectionFlags.Merge(prototypeDungeonRoom.injectionFlags))
			{
				Debug.Log("Assigning FIREPLACE from room " + prototypeDungeonRoom.name);
			}
		}

		protected void HandleBossFoyerAcquisition(BuilderFlowNode buildData)
		{
			HandleBossFoyerAcquisition(buildData, false);
		}

		protected void HandleBossFoyerAcquisition(BuilderFlowNode buildData, bool isFallback)
		{
			BuilderFlowNode builderFlowNode = null;
			for (int i = 0; i < buildData.childBuilderNodes.Count; i++)
			{
				if (buildData.childBuilderNodes[i].Category == PrototypeDungeonRoom.RoomCategory.BOSS)
				{
					builderFlowNode = buildData.childBuilderNodes[i];
				}
			}
			if (builderFlowNode == null)
			{
				return;
			}
			ClearPlacedRoomData(buildData);
			if (buildData.node.overrideExactRoom != null)
			{
				buildData.assignedPrototypeRoom = buildData.node.overrideExactRoom;
			}
			else
			{
				GenericRoomTable genericRoomTable = m_flow.fallbackRoomTable;
				if (buildData.node.overrideRoomTable != null)
				{
					genericRoomTable = buildData.node.overrideRoomTable;
				}
				List<WeightedRoom> list = new List<WeightedRoom>(genericRoomTable.GetCompiledList());
				for (int j = 0; j < list.Count; j++)
				{
					PrototypeDungeonRoom room = list[j].room;
					if (!isFallback && !room.CheckPrerequisites())
					{
						list.RemoveAt(j);
						j--;
						continue;
					}
					bool flag = false;
					if (room != null)
					{
						for (int k = 0; k < builderFlowNode.assignedPrototypeRoom.exitData.exits.Count; k++)
						{
							if (builderFlowNode.assignedPrototypeRoom.exitData.exits[k].exitType != PrototypeRoomExit.ExitType.EXIT_ONLY)
							{
								List<PrototypeRoomExit> exitsMatchingDirection = room.GetExitsMatchingDirection((DungeonData.Direction)((int)(builderFlowNode.assignedPrototypeRoom.exitData.exits[k].exitDirection + 4) % 8), PrototypeRoomExit.ExitType.EXIT_ONLY);
								if (exitsMatchingDirection.Count > 0)
								{
									flag = true;
									break;
								}
							}
						}
					}
					if (!flag)
					{
						list.RemoveAt(j);
						j--;
					}
				}
				PrototypeDungeonRoom prototypeDungeonRoom = null;
				float num = 0f;
				for (int l = 0; l < list.Count; l++)
				{
					num += list[l].weight;
				}
				float num2 = BraveRandom.GenerationRandomValue() * num;
				for (int m = 0; m < list.Count; m++)
				{
					num2 -= list[m].weight;
					if (num2 <= 0f)
					{
						prototypeDungeonRoom = list[m].room;
						break;
					}
				}
				if (list.Count > 0 && prototypeDungeonRoom == null)
				{
					prototypeDungeonRoom = list[list.Count - 1].room;
				}
				if (prototypeDungeonRoom != null)
				{
					buildData.assignedPrototypeRoom = prototypeDungeonRoom;
				}
				else
				{
					if (!isFallback)
					{
						HandleBossFoyerAcquisition(buildData, true);
						return;
					}
					Debug.LogError("Failed to acquire a boss foyer! Something has gone wrong, or there is somehow not a boss foyer that matches the entrance direction for this boss chamber.");
				}
			}
			if (buildData.assignedPrototypeRoom != null)
			{
				NotifyPlacedRoomData(buildData.assignedPrototypeRoom);
			}
		}

		protected void AcquirePrototypeRoom(BuilderFlowNode buildData)
		{
			if (roomsOfSubtypeRemaining == null)
			{
				roomsOfSubtypeRemaining = new Dictionary<DungeonFlowSubtypeRestriction, int>();
				for (int i = 0; i < m_flow.subtypeRestrictions.Count; i++)
				{
					roomsOfSubtypeRemaining.Add(m_flow.subtypeRestrictions[i], m_flow.subtypeRestrictions[i].maximumRoomsOfSubtype);
				}
			}
			ClearPlacedRoomData(buildData);
			if (buildData.node.UsesGlobalBossData)
			{
				buildData.assignedPrototypeRoom = GameManager.Instance.BossManager.SelectBossRoom();
			}
			else if (buildData.node.overrideExactRoom != null)
			{
				buildData.assignedPrototypeRoom = buildData.node.overrideExactRoom;
			}
			else
			{
				PrototypeDungeonRoom.RoomCategory roomCategory = ((!buildData.usesOverrideCategory) ? buildData.node.roomCategory : buildData.overrideCategory);
				if (roomCategory == PrototypeDungeonRoom.RoomCategory.CONNECTOR)
				{
					buildData.AcquiresRoomAsNecessary = true;
				}
				else
				{
					GenericRoomTable genericRoomTable = m_flow.fallbackRoomTable;
					if (buildData.node.overrideRoomTable != null)
					{
						genericRoomTable = buildData.node.overrideRoomTable;
					}
					List<WeightedRoom> compiledList = genericRoomTable.GetCompiledList();
					PrototypeDungeonRoom availableRoom = GetAvailableRoom(roomCategory, buildData.Connectivity, compiledList);
					if (availableRoom != null)
					{
						buildData.assignedPrototypeRoom = availableRoom;
					}
					else if (roomCategory != PrototypeDungeonRoom.RoomCategory.SECRET)
					{
						Debug.LogError("Failed to acquire a prototype room. This means the list is too sparse for the relevant category (" + roomCategory.ToString() + ") or something has gone terribly wrong.");
					}
				}
			}
			if (buildData.assignedPrototypeRoom != null)
			{
				NotifyPlacedRoomData(buildData.assignedPrototypeRoom);
			}
			else if (!buildData.AcquiresRoomAsNecessary && buildData.node.priority != DungeonFlowNode.NodePriority.OPTIONAL)
			{
			}
		}

		protected void AssignInjectionDataToRoomHandler(BuilderFlowNode buildData)
		{
			if (buildData.instanceRoom == null)
			{
				return;
			}
			if (buildData.InjectionTarget != null)
			{
				buildData.instanceRoom.injectionTarget = buildData.InjectionTarget.instanceRoom;
			}
			if (buildData.InjectionFrameSequence != null)
			{
				List<RoomHandler> list = new List<RoomHandler>();
				for (int i = 0; i < buildData.InjectionFrameSequence.Count; i++)
				{
					list.Add(buildData.InjectionFrameSequence[i].instanceRoom);
				}
				buildData.instanceRoom.injectionFrameData = list;
			}
		}

		protected void DebugPrintTree(BuilderFlowNode root)
		{
			Stack<BuilderFlowNode> stack = new Stack<BuilderFlowNode>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				BuilderFlowNode builderFlowNode = stack.Pop();
				if (builderFlowNode.node != null)
				{
					Debug.Log(builderFlowNode.identifier + "|" + builderFlowNode.node.roomCategory.ToString());
				}
				for (int i = 0; i < builderFlowNode.childBuilderNodes.Count; i++)
				{
					stack.Push(builderFlowNode.childBuilderNodes[i]);
				}
			}
		}

		public List<BuilderFlowNode> FindPathBetweenNodesAdvanced(BuilderFlowNode origin, BuilderFlowNode target, List<Tuple<BuilderFlowNode, BuilderFlowNode>> excludedConnections)
		{
			Dictionary<BuilderFlowNode, int> dictionary = new Dictionary<BuilderFlowNode, int>();
			Dictionary<BuilderFlowNode, BuilderFlowNode> dictionary2 = new Dictionary<BuilderFlowNode, BuilderFlowNode>();
			for (int i = 0; i < allBuilderNodes.Count; i++)
			{
				int value = int.MaxValue;
				if (allBuilderNodes[i] == origin)
				{
					value = 0;
				}
				dictionary.Add(allBuilderNodes[i], value);
			}
			BuilderFlowNode builderFlowNode = origin;
			int num = 1;
			while (true)
			{
				List<BuilderFlowNode> obj = BuilderFlowNodeListPool.Allocate();
				for (int j = 0; j < excludedConnections.Count; j++)
				{
					Tuple<BuilderFlowNode, BuilderFlowNode> tuple = excludedConnections[j];
					if (tuple.First == builderFlowNode)
					{
						obj.Add(tuple.Second);
					}
				}
				List<BuilderFlowNode> allConnectedNodes = builderFlowNode.GetAllConnectedNodes(obj);
				obj.Clear();
				BuilderFlowNodeListPool.Free(ref obj);
				int num2 = 0;
				while (true)
				{
					if (num2 < allConnectedNodes.Count)
					{
						if (allConnectedNodes[num2] != target)
						{
							if (dictionary.ContainsKey(allConnectedNodes[num2]) && dictionary[allConnectedNodes[num2]] > num)
							{
								dictionary[allConnectedNodes[num2]] = num;
								if (dictionary2.ContainsKey(allConnectedNodes[num2]))
								{
									dictionary2[allConnectedNodes[num2]] = builderFlowNode;
								}
								else
								{
									dictionary2.Add(allConnectedNodes[num2], builderFlowNode);
								}
							}
							num2++;
							continue;
						}
						dictionary2.Add(allConnectedNodes[num2], builderFlowNode);
					}
					else
					{
						dictionary.Remove(builderFlowNode);
						if (dictionary.Count == 0)
						{
							return null;
						}
						builderFlowNode = null;
						num = int.MaxValue;
						foreach (BuilderFlowNode key in dictionary.Keys)
						{
							if (dictionary[key] < num)
							{
								builderFlowNode = key;
								num = dictionary[key];
							}
						}
						if (builderFlowNode != null)
						{
							break;
						}
					}
					if (!dictionary2.ContainsKey(target))
					{
						return null;
					}
					List<BuilderFlowNode> list = new List<BuilderFlowNode>();
					for (BuilderFlowNode builderFlowNode2 = target; builderFlowNode2 != null; builderFlowNode2 = ((!dictionary2.ContainsKey(builderFlowNode2)) ? null : dictionary2[builderFlowNode2]))
					{
						list.Insert(0, builderFlowNode2);
					}
					return list;
				}
			}
		}

		public List<BuilderFlowNode> FindPathBetweenNodes(BuilderFlowNode origin, BuilderFlowNode target, bool excludeDirect = false, params BuilderFlowNode[] excluded)
		{
			Dictionary<BuilderFlowNode, int> dictionary = new Dictionary<BuilderFlowNode, int>();
			Dictionary<BuilderFlowNode, BuilderFlowNode> dictionary2 = new Dictionary<BuilderFlowNode, BuilderFlowNode>();
			for (int i = 0; i < allBuilderNodes.Count; i++)
			{
				int value = int.MaxValue;
				if (allBuilderNodes[i] == origin)
				{
					value = 0;
				}
				dictionary.Add(allBuilderNodes[i], value);
			}
			BuilderFlowNode builderFlowNode = origin;
			int num = 1;
			BuilderFlowNode[] array = ((excluded != null) ? new BuilderFlowNode[excluded.Length + 1] : new BuilderFlowNode[1] { target });
			if (excluded != null)
			{
				array[array.Length - 1] = target;
				for (int j = 0; j < excluded.Length; j++)
				{
					array[j] = excluded[j];
				}
			}
			while (true)
			{
				List<BuilderFlowNode> list = ((!excludeDirect || builderFlowNode != origin) ? builderFlowNode.GetAllConnectedNodes(excluded) : builderFlowNode.GetAllConnectedNodes(array));
				int num2 = 0;
				while (true)
				{
					if (num2 < list.Count)
					{
						if (list[num2] != target)
						{
							if (dictionary.ContainsKey(list[num2]) && dictionary[list[num2]] > num)
							{
								dictionary[list[num2]] = num;
								if (dictionary2.ContainsKey(list[num2]))
								{
									dictionary2[list[num2]] = builderFlowNode;
								}
								else
								{
									dictionary2.Add(list[num2], builderFlowNode);
								}
							}
							num2++;
							continue;
						}
						dictionary2.Add(list[num2], builderFlowNode);
					}
					else
					{
						dictionary.Remove(builderFlowNode);
						if (dictionary.Count == 0)
						{
							return null;
						}
						builderFlowNode = null;
						num = int.MaxValue;
						foreach (BuilderFlowNode key in dictionary.Keys)
						{
							if (dictionary[key] < num)
							{
								builderFlowNode = key;
								num = dictionary[key];
							}
						}
						if (builderFlowNode != null)
						{
							break;
						}
					}
					if (!dictionary2.ContainsKey(target))
					{
						return null;
					}
					List<BuilderFlowNode> list2 = new List<BuilderFlowNode>();
					for (BuilderFlowNode builderFlowNode2 = target; builderFlowNode2 != null; builderFlowNode2 = ((!dictionary2.ContainsKey(builderFlowNode2)) ? null : dictionary2[builderFlowNode2]))
					{
						list2.Insert(0, builderFlowNode2);
					}
					return list2;
				}
			}
		}

		public List<BuilderFlowNode> GetSubloopsFromLoop(LoopBuilderComposite loopComposite)
		{
			List<Tuple<BuilderFlowNode, BuilderFlowNode>> list = new List<Tuple<BuilderFlowNode, BuilderFlowNode>>();
			for (int i = 0; i < loopComposite.Nodes.Count; i++)
			{
				BuilderFlowNode builderFlowNode = loopComposite.Nodes[i];
				List<BuilderFlowNode> allConnectedNodes = builderFlowNode.GetAllConnectedNodes();
				for (int j = 0; j < allConnectedNodes.Count; j++)
				{
					BuilderFlowNode builderFlowNode2 = allConnectedNodes[j];
					if (loopComposite.Nodes.Contains(builderFlowNode2))
					{
						list.Add(new Tuple<BuilderFlowNode, BuilderFlowNode>(builderFlowNode, builderFlowNode2));
					}
				}
			}
			for (int k = 0; k < loopComposite.Nodes.Count; k++)
			{
				BuilderFlowNode origin = loopComposite.Nodes[k];
				for (int l = k + 1; l < loopComposite.Nodes.Count; l++)
				{
					BuilderFlowNode target = loopComposite.Nodes[l];
					List<BuilderFlowNode> list2 = FindPathBetweenNodesAdvanced(origin, target, list);
					if (list2 != null)
					{
						return list2;
					}
				}
			}
			return null;
		}

		public List<BuilderFlowNode> FindSimplestContainingLoop(BuilderFlowNode origin, List<BuilderFlowNode> usedNodes)
		{
			List<BuilderFlowNode> allConnectedNodes = origin.GetAllConnectedNodes();
			List<BuilderFlowNode> result = null;
			int num = int.MaxValue;
			for (int i = 0; i < allConnectedNodes.Count; i++)
			{
				List<BuilderFlowNode> list = FindPathBetweenNodes(origin, allConnectedNodes[i], true, usedNodes.ToArray());
				if (list != null && list.Count < num)
				{
					num = list.Count;
					result = list;
				}
			}
			return result;
		}

		public void ConvertTreeToCompositeStructure(BuilderFlowNode currentRoot, List<BuilderFlowNode> currentRunningList, FlowCompositeMetastructure currentMetastructure)
		{
			List<BuilderFlowNode> list = FindSimplestContainingLoop(currentRoot, currentMetastructure.usedList);
			if (list != null)
			{
				currentMetastructure.loopLists.Add(list);
				LoopBuilderComposite loopBuilderComposite = new LoopBuilderComposite(list, m_flow, this, LoopBuilderComposite.CompositeStyle.LOOP);
				currentMetastructure.usedList.AddRange(list);
				List<BuilderFlowNode> externalConnectedNodes = loopBuilderComposite.ExternalConnectedNodes;
				for (int i = 0; i < externalConnectedNodes.Count; i++)
				{
					BuilderFlowNode builderFlowNode = externalConnectedNodes[i];
					BuilderFlowNode connectedInternalNode = loopBuilderComposite.GetConnectedInternalNode(builderFlowNode);
					if (builderFlowNode.loopConnectedBuilderNode != connectedInternalNode && connectedInternalNode.loopConnectedBuilderNode != builderFlowNode && !currentMetastructure.usedList.Contains(builderFlowNode))
					{
						ConvertTreeToCompositeStructure(builderFlowNode, null, currentMetastructure);
					}
				}
				return;
			}
			if (currentRoot.node.isWarpWingEntrance)
			{
				currentRunningList = null;
			}
			else if (currentRoot.IsInjectedNode && currentRoot.node.childNodeGuids.Count == 0)
			{
				currentRunningList = null;
			}
			if (currentRunningList == null)
			{
				currentRunningList = new List<BuilderFlowNode>();
				currentMetastructure.compositeLists.Add(currentRunningList);
			}
			currentRunningList.Add(currentRoot);
			currentMetastructure.usedList.Add(currentRoot);
			for (int j = 0; j < currentRoot.childBuilderNodes.Count; j++)
			{
				BuilderFlowNode builderFlowNode2 = currentRoot.childBuilderNodes[j];
				if (!currentMetastructure.usedList.Contains(builderFlowNode2))
				{
					ConvertTreeToCompositeStructure(builderFlowNode2, currentRunningList, currentMetastructure);
				}
			}
		}

		protected bool ConnectTwoPlacedLayoutNodes(BuilderFlowNode internalNode, BuilderFlowNode externalNode, SemioticLayoutManager layout)
		{
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> list = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> list2 = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			bool flag = false;
			for (int i = 0; i < externalNode.instanceRoom.area.prototypeRoom.exitData.exits.Count; i++)
			{
				for (int j = 0; j < internalNode.instanceRoom.area.prototypeRoom.exitData.exits.Count; j++)
				{
					PrototypeRoomExit prototypeRoomExit = externalNode.instanceRoom.area.prototypeRoom.exitData.exits[i];
					PrototypeRoomExit prototypeRoomExit2 = internalNode.instanceRoom.area.prototypeRoom.exitData.exits[j];
					if (!externalNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit) && !internalNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit2))
					{
						RuntimeRoomExitData runtimeRoomExitData = new RuntimeRoomExitData(prototypeRoomExit);
						RuntimeRoomExitData runtimeRoomExitData2 = new RuntimeRoomExitData(prototypeRoomExit2);
						Tuple<RuntimeRoomExitData, RuntimeRoomExitData> item = new Tuple<RuntimeRoomExitData, RuntimeRoomExitData>(runtimeRoomExitData, runtimeRoomExitData2);
						if ((runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.EAST && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.WEST) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.WEST && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.EAST) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.NORTH && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.SOUTH) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.SOUTH && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.NORTH))
						{
							list.Add(item);
						}
						else if (runtimeRoomExitData.referencedExit.exitDirection != runtimeRoomExitData2.referencedExit.exitDirection)
						{
							list2.Add(item);
						}
					}
				}
			}
			list.AddRange(list2);
			RuntimeRoomExitData runtimeRoomExitData3 = null;
			RuntimeRoomExitData runtimeRoomExitData4 = null;
			List<IntVector2> list3 = null;
			int num = int.MaxValue;
			for (int k = 0; k < list.Count; k++)
			{
				RuntimeRoomExitData first = list[k].First;
				RuntimeRoomExitData second = list[k].Second;
				PrototypeRoomExit referencedExit = first.referencedExit;
				PrototypeRoomExit referencedExit2 = second.referencedExit;
				IntVector2 startPosition = externalNode.instanceRoom.area.basePosition + referencedExit.GetExitOrigin(referencedExit.exitLength + 3) - IntVector2.One;
				IntVector2 endPosition = internalNode.instanceRoom.area.basePosition + referencedExit2.GetExitOrigin(referencedExit2.exitLength + 3) - IntVector2.One;
				SemioticLayoutManager semioticLayoutManager = new SemioticLayoutManager();
				semioticLayoutManager.MergeLayout(layout);
				RuntimeRoomExitData runtimeRoomExitData5 = new RuntimeRoomExitData(referencedExit);
				runtimeRoomExitData5.additionalExitLength = 1;
				RuntimeRoomExitData runtimeRoomExitData6 = new RuntimeRoomExitData(referencedExit2);
				runtimeRoomExitData6.additionalExitLength = 1;
				semioticLayoutManager.StampComplexExitTemporary(runtimeRoomExitData5, externalNode.instanceRoom.area);
				semioticLayoutManager.StampComplexExitTemporary(runtimeRoomExitData6, internalNode.instanceRoom.area);
				List<IntVector2> list4 = semioticLayoutManager.PathfindHallway(startPosition, endPosition);
				semioticLayoutManager.ClearTemporary();
				semioticLayoutManager.OnDestroy();
				if (list4 != null && list4.Count > 0 && list4.Count < num)
				{
					runtimeRoomExitData3 = first;
					runtimeRoomExitData4 = second;
					list3 = list4;
					num = list4.Count;
					flag = true;
				}
			}
			if (flag)
			{
				runtimeRoomExitData3.additionalExitLength = 0;
				runtimeRoomExitData4.additionalExitLength = 0;
				RoomHandler roomHandler = LoopBuilderComposite.PlaceProceduralPathRoom(list3, runtimeRoomExitData3, runtimeRoomExitData4, externalNode.instanceRoom, internalNode.instanceRoom, layout);
				if (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
				{
					runtimeRoomExitData3.oneWayDoor = true;
				}
				m_currentMaxLengthProceduralHallway = Mathf.Max(m_currentMaxLengthProceduralHallway, list3.Count);
			}
			return flag;
		}

		protected IEnumerable AttachWarpCanvasToLayout(BuilderFlowNode externalNode, BuilderFlowNode internalNode, SemioticLayoutManager canvas, SemioticLayoutManager layout)
		{
			AttachWarpCanvasSuccess = false;
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairs = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			for (int i = 0; i < externalNode.instanceRoom.area.prototypeRoom.exitData.exits.Count; i++)
			{
				PrototypeRoomExit prototypeRoomExit = externalNode.instanceRoom.area.prototypeRoom.exitData.exits[i];
				for (int j = 0; j < internalNode.instanceRoom.area.prototypeRoom.exitData.exits.Count; j++)
				{
					PrototypeRoomExit prototypeRoomExit2 = internalNode.instanceRoom.area.prototypeRoom.exitData.exits[j];
					if (!externalNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit) && !internalNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit2))
					{
						RuntimeRoomExitData first = new RuntimeRoomExitData(prototypeRoomExit);
						RuntimeRoomExitData second = new RuntimeRoomExitData(prototypeRoomExit2);
						Tuple<RuntimeRoomExitData, RuntimeRoomExitData> item = new Tuple<RuntimeRoomExitData, RuntimeRoomExitData>(first, second);
						exitPairs.Add(item);
						break;
					}
				}
			}
			if (GameManager.Instance.GeneratingLevelOverrideState != GameManager.LevelOverrideState.FOYER && !externalNode.node.handlesOwnWarping && !internalNode.node.handlesOwnWarping)
			{
				if (exitPairs.Count == 0)
				{
					Debug.LogError("A warp wing has no exits and is not flagged as handling its own warping!");
					AttachWarpCanvasSuccess = false;
					yield break;
				}
				RuntimeRoomExitData placedExitData = exitPairs[0].First;
				RuntimeRoomExitData newExitData = exitPairs[0].Second;
				PrototypeRoomExit placedExit = placedExitData.referencedExit;
				PrototypeRoomExit newExit = newExitData.referencedExit;
				placedExitData.additionalExitLength = 4;
				newExitData.additionalExitLength = 4;
				placedExitData.isWarpWingStart = true;
				newExitData.isWarpWingStart = true;
				internalNode.exitToNodeMap.Add(newExit, externalNode);
				internalNode.nodeToExitMap.Add(externalNode, newExit);
				externalNode.exitToNodeMap.Add(placedExit, internalNode);
				externalNode.nodeToExitMap.Add(internalNode, placedExit);
				layout.StampComplexExitToLayout(placedExitData, externalNode.instanceRoom.area);
				layout.StampComplexExitToLayout(newExitData, internalNode.instanceRoom.area);
				placedExitData.linkedExit = newExitData;
				newExitData.linkedExit = placedExitData;
				externalNode.instanceRoom.RegisterConnectedRoom(internalNode.instanceRoom, placedExitData);
				internalNode.instanceRoom.RegisterConnectedRoom(externalNode.instanceRoom, newExitData);
				yield return null;
			}
			IntVector2 canvasTranslationToZero = canvas.NegativeDimensions;
			int canvasDistanceApart = 10;
			if (GameManager.Instance.GeneratingLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				canvasDistanceApart = 25;
			}
			IntVector2 additionalTranslation = new IntVector2(layout.PositiveDimensions.x + canvasDistanceApart, 0);
			canvas.HandleOffsetRooms(canvasTranslationToZero + additionalTranslation);
			layout.MergeLayout(canvas);
			AttachWarpCanvasSuccess = true;
		}

		protected bool NodeHasExitGroupsToCheck(BuilderFlowNode node)
		{
			List<PrototypeRoomExit.ExitGroup> definedExitGroups = node.assignedPrototypeRoom.exitData.GetDefinedExitGroups();
			bool result = definedExitGroups.Count > 1;
			for (int i = 0; i < node.instanceRoom.area.instanceUsedExits.Count; i++)
			{
				definedExitGroups.Remove(node.instanceRoom.area.instanceUsedExits[i].exitGroup);
			}
			if (definedExitGroups.Count == 0)
			{
				result = false;
			}
			return result;
		}

		protected IEnumerable AttachNewCanvasToLayout(BuilderFlowNode externalNode, BuilderFlowNode internalNode, SemioticLayoutManager canvas, SemioticLayoutManager layout)
		{
			AttachNewCanvasSuccess = false;
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairs = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> jointedPairs = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			bool success = false;
			bool supportsJointedPairs = true;
			if ((externalNode.assignedPrototypeRoom != null && externalNode.Category == PrototypeDungeonRoom.RoomCategory.SECRET) || (internalNode.assignedPrototypeRoom != null && internalNode.Category == PrototypeDungeonRoom.RoomCategory.SECRET))
			{
				supportsJointedPairs = false;
			}
			if (externalNode.instanceRoom == null)
			{
				BraveUtility.Log(externalNode.node.guidAsString, Color.magenta, BraveUtility.LogVerbosity.IMPORTANT);
			}
			bool externalNodeHasExitGroups = NodeHasExitGroupsToCheck(externalNode);
			bool internalNodeHasExitGroups = NodeHasExitGroupsToCheck(internalNode);
			for (int j = 0; j < externalNode.instanceRoom.area.prototypeRoom.exitData.exits.Count; j++)
			{
				PrototypeRoomExit prototypeRoomExit = externalNode.instanceRoom.area.prototypeRoom.exitData.exits[j];
				if (externalNodeHasExitGroups)
				{
					bool flag = false;
					for (int k = 0; k < externalNode.instanceRoom.area.instanceUsedExits.Count; k++)
					{
						if (externalNode.instanceRoom.area.instanceUsedExits[k].exitGroup == prototypeRoomExit.exitGroup)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				for (int l = 0; l < internalNode.instanceRoom.area.prototypeRoom.exitData.exits.Count; l++)
				{
					PrototypeRoomExit prototypeRoomExit2 = internalNode.instanceRoom.area.prototypeRoom.exitData.exits[l];
					if (internalNodeHasExitGroups)
					{
						bool flag2 = false;
						for (int m = 0; m < internalNode.instanceRoom.area.instanceUsedExits.Count; m++)
						{
							if (internalNode.instanceRoom.area.instanceUsedExits[m].exitGroup == prototypeRoomExit2.exitGroup)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							continue;
						}
					}
					if (!externalNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit) && !internalNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit2))
					{
						RuntimeRoomExitData runtimeRoomExitData = new RuntimeRoomExitData(prototypeRoomExit);
						RuntimeRoomExitData runtimeRoomExitData2 = new RuntimeRoomExitData(prototypeRoomExit2);
						Tuple<RuntimeRoomExitData, RuntimeRoomExitData> item = new Tuple<RuntimeRoomExitData, RuntimeRoomExitData>(runtimeRoomExitData, runtimeRoomExitData2);
						if ((runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.EAST && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.WEST) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.WEST && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.EAST) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.NORTH && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.SOUTH) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.SOUTH && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.NORTH))
						{
							exitPairs.Add(item);
						}
						else if (runtimeRoomExitData.referencedExit.exitDirection != runtimeRoomExitData2.referencedExit.exitDirection)
						{
							jointedPairs.Add(item);
						}
					}
				}
			}
			if (supportsJointedPairs)
			{
				exitPairs.AddRange(jointedPairs);
			}
			for (int i = 0; i < exitPairs.Count; i++)
			{
				RuntimeRoomExitData placedExitData = exitPairs[i].First;
				RuntimeRoomExitData newExitData = exitPairs[i].Second;
				PrototypeRoomExit placedExit = placedExitData.referencedExit;
				PrototypeRoomExit newExit = newExitData.referencedExit;
				IEnumerator CanPlaceLayoutTracker = layout.CanPlaceLayoutAtPoint(canvas, placedExitData, newExitData, externalNode.instanceRoom.area.basePosition, internalNode.instanceRoom.area.basePosition).GetEnumerator();
				while (CanPlaceLayoutTracker.MoveNext())
				{
					yield return null;
				}
				if (layout.CanPlaceLayoutAtPointSuccess)
				{
					if (!internalNode.exitToNodeMap.ContainsKey(newExit) && !externalNode.exitToNodeMap.ContainsKey(placedExit))
					{
						IntVector2 intVector = externalNode.instanceRoom.area.basePosition + placedExitData.ExitOrigin - IntVector2.One;
						IntVector2 intVector2 = internalNode.instanceRoom.area.basePosition + newExitData.ExitOrigin - IntVector2.One;
						IntVector2 offset = intVector - intVector2;
						canvas.HandleOffsetRooms(offset);
						layout.MergeLayout(canvas);
						internalNode.exitToNodeMap.Add(newExit, externalNode);
						internalNode.nodeToExitMap.Add(externalNode, newExit);
						externalNode.exitToNodeMap.Add(placedExit, internalNode);
						externalNode.nodeToExitMap.Add(internalNode, placedExit);
						layout.StampComplexExitToLayout(placedExitData, externalNode.instanceRoom.area);
						layout.StampComplexExitToLayout(newExitData, internalNode.instanceRoom.area);
						placedExitData.linkedExit = newExitData;
						newExitData.linkedExit = placedExitData;
						if ((externalNode.parentBuilderNode == internalNode && externalNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.ONE_WAY) || (internalNode.parentBuilderNode == externalNode && internalNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.ONE_WAY))
						{
							placedExitData.oneWayDoor = true;
							newExitData.oneWayDoor = true;
						}
						if ((externalNode.parentBuilderNode == internalNode && externalNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.LOCKED) || (internalNode.parentBuilderNode == externalNode && internalNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.LOCKED))
						{
							placedExitData.isLockedDoor = true;
							newExitData.isLockedDoor = true;
						}
						externalNode.instanceRoom.RegisterConnectedRoom(internalNode.instanceRoom, placedExitData);
						internalNode.instanceRoom.RegisterConnectedRoom(externalNode.instanceRoom, newExitData);
						success = true;
						break;
					}
				}
				else
				{
					yield return null;
				}
			}
			AttachNewCanvasSuccess = success;
		}

		public SemioticLayoutManager Build(out bool generationSucceeded)
		{
			IEnumerator enumerator = DeferredBuild().GetEnumerator();
			while (enumerator.MoveNext())
			{
			}
			generationSucceeded = DeferredGenerationSuccess;
			return DeferredGeneratedLayout;
		}

		private void AttachPregeneratedInjectionData()
		{
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH || GameManager.Instance.GeneratingLevelOverrideState != 0)
			{
				return;
			}
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId;
			if (!MetaInjectionData.CurrentRunBlueprint.ContainsKey(tilesetId))
			{
				return;
			}
			for (int i = 0; i < MetaInjectionData.CurrentRunBlueprint[tilesetId].Count; i++)
			{
				m_postprocessInjectionData.Add(MetaInjectionData.CurrentRunBlueprint[tilesetId][i]);
				if (MetaInjectionData.CurrentRunBlueprint[tilesetId][i].injectionData.name.Contains("Subshop"))
				{
					m_runtimeInjectionFlags.ShopAnnexed = true;
				}
			}
		}

		private bool IsCompositeWarpWing(LoopBuilderComposite composite)
		{
			for (int i = 0; i < composite.ExternalConnectedNodes.Count; i++)
			{
				BuilderFlowNode external = composite.ExternalConnectedNodes[i];
				BuilderFlowNode connectedInternalNode = composite.GetConnectedInternalNode(external);
				if (connectedInternalNode != null && connectedInternalNode.node.isWarpWingEntrance)
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerable DeferredBuild()
		{
			DeferredGenerationSuccess = true;
			m_postprocessInjectionData.Clear();
			m_runtimeInjectionFlags.Clear();
			AttachPregeneratedInjectionData();
			BuilderFlowNode initialRoot = ComposeFlowTree();
			yield return null;
			PerformOperationOnTreeNodes(initialRoot, AcquirePrototypeRoom);
			PerformOperationOnTreeNodes(initialRoot, HandleBossFoyerAcquisition);
			FlowCompositeMetastructure preinjectionMetastructure = new FlowCompositeMetastructure();
			ConvertTreeToCompositeStructure(initialRoot, null, preinjectionMetastructure);
			for (int k = 0; k < m_postprocessInjectionData.Count; k++)
			{
				HandleNodeInjection(initialRoot, m_postprocessInjectionData[k], m_runtimeInjectionFlags, preinjectionMetastructure);
			}
			if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SUPERBOSSRUSH)
			{
				HandleNodeInjection(initialRoot, m_flow.flowInjectionData, m_runtimeInjectionFlags, preinjectionMetastructure);
				for (int l = 0; l < m_flow.sharedInjectionData.Count; l++)
				{
					if (m_previouslyGeneratedRuntimeMetadata.ContainsKey(m_flow.sharedInjectionData[l]))
					{
						Debug.Log("Previously injected: " + m_flow.sharedInjectionData[l].name);
						if (m_previouslyGeneratedRuntimeMetadata[m_flow.sharedInjectionData[l]].AssignedModifierData != null)
						{
							Debug.Log("Using prior: " + m_previouslyGeneratedRuntimeMetadata[m_flow.sharedInjectionData[l]].AssignedModifierData.annotation);
						}
						HandleNodeInjection(initialRoot, m_previouslyGeneratedRuntimeMetadata[m_flow.sharedInjectionData[l]], m_runtimeInjectionFlags, preinjectionMetastructure);
					}
					else
					{
						Debug.Log("No prior injection: " + m_flow.sharedInjectionData[l].name);
						RuntimeInjectionMetadata runtimeInjectionMetadata = new RuntimeInjectionMetadata(m_flow.sharedInjectionData[l]);
						m_previouslyGeneratedRuntimeMetadata.Add(m_flow.sharedInjectionData[l], runtimeInjectionMetadata);
						HandleNodeInjection(initialRoot, runtimeInjectionMetadata, m_runtimeInjectionFlags, preinjectionMetastructure);
					}
				}
			}
			BuilderFlowNode connectedRoot = RerootTreeAtHighestConnectivity(initialRoot);
			yield return null;
			FlowCompositeMetastructure metastructure = new FlowCompositeMetastructure();
			ConvertTreeToCompositeStructure(connectedRoot, null, metastructure);
			yield return null;
			List<LoopBuilderComposite> composites = new List<LoopBuilderComposite>();
			Dictionary<BuilderFlowNode, LoopBuilderComposite> nodeToCompositeMap = new Dictionary<BuilderFlowNode, LoopBuilderComposite>();
			for (int m = 0; m < metastructure.loopLists.Count; m++)
			{
				LoopBuilderComposite loopBuilderComposite = new LoopBuilderComposite(metastructure.loopLists[m], m_flow, this, LoopBuilderComposite.CompositeStyle.LOOP);
				composites.Add(loopBuilderComposite);
				for (int n = 0; n < metastructure.loopLists[m].Count; n++)
				{
					nodeToCompositeMap.Add(metastructure.loopLists[m][n], loopBuilderComposite);
				}
			}
			for (int num = 0; num < metastructure.compositeLists.Count; num++)
			{
				LoopBuilderComposite loopBuilderComposite2 = new LoopBuilderComposite(metastructure.compositeLists[num], m_flow, this);
				composites.Add(loopBuilderComposite2);
				for (int num2 = 0; num2 < metastructure.compositeLists[num].Count; num2++)
				{
					nodeToCompositeMap.Add(metastructure.compositeLists[num][num2], loopBuilderComposite2);
				}
			}
			List<SemioticLayoutManager> canvases = new List<SemioticLayoutManager>();
			int regenerationAttemptCounter = 0;
			int j = 0;
			while (true)
			{
				if (j < composites.Count)
				{
					LoopBuilderComposite composite = composites[j];
					IEnumerator compositeTracker = composite.Build(IntVector2.Zero).GetEnumerator();
					while (compositeTracker.MoveNext())
					{
						yield return null;
					}
					SemioticLayoutManager compositeCanvas = composite.CompletedCanvas;
					int COMPOSITE_REGENERATION_ATTEMPTS = ((composite.loopStyle == LoopBuilderComposite.CompositeStyle.NON_LOOP) ? 5 : 100);
					if (composite.RequiresRegeneration && regenerationAttemptCounter < COMPOSITE_REGENERATION_ATTEMPTS)
					{
						regenerationAttemptCounter++;
						LoopBuilderComposite loopBuilderComposite3 = new LoopBuilderComposite(composite.Nodes, m_flow, this, composite.loopStyle);
						for (int num3 = 0; num3 < loopBuilderComposite3.Nodes.Count; num3++)
						{
							if (loopBuilderComposite3.Nodes[num3].assignedPrototypeRoom != null && loopBuilderComposite3.Nodes[num3].assignedPrototypeRoom.injectionFlags.CastleFireplace)
							{
								Debug.LogWarning(" ======> NOT Reacquiring for this room. <====== ");
							}
							else
							{
								AcquirePrototypeRoom(loopBuilderComposite3.Nodes[num3]);
							}
							loopBuilderComposite3.Nodes[num3].ClearData();
							nodeToCompositeMap[loopBuilderComposite3.Nodes[num3]] = loopBuilderComposite3;
						}
						composites[j] = loopBuilderComposite3;
						j--;
					}
					else
					{
						if (composite.RequiresRegeneration)
						{
							LoopDungeonGenerator.NUM_FAILS_COMPOSITE_REGENERATION++;
							break;
						}
						regenerationAttemptCounter = 0;
						canvases.Add(compositeCanvas);
					}
					j++;
					continue;
				}
				PerformOperationOnTreeNodes(connectedRoot, AssignInjectionDataToRoomHandler);
				SemioticLayoutManager layout = new SemioticLayoutManager();
				if (!DEBUG_RENDER_CANVASES_SEPARATELY)
				{
					Queue<LoopBuilderComposite> compositesToProcess = new Queue<LoopBuilderComposite>();
					List<LoopBuilderComposite> mergedComposites = new List<LoopBuilderComposite>();
					List<LoopBuilderComposite> warpCompositesToQueueLater = new List<LoopBuilderComposite>();
					compositesToProcess.Enqueue(composites[0]);
					while (compositesToProcess.Count > 0)
					{
						LoopBuilderComposite composite2 = compositesToProcess.Dequeue();
						int compositeIndex = composites.IndexOf(composite2);
						SemioticLayoutManager canvas = canvases[compositeIndex];
						bool compositeHasBeenAttached = false;
						if (mergedComposites.Count == 0)
						{
							layout.MergeLayout(canvas);
							compositeHasBeenAttached = true;
						}
						if (mergedComposites.Contains(composite2))
						{
							compositeHasBeenAttached = true;
						}
						List<LoopBuilderComposite> compositesToEnqueue = new List<LoopBuilderComposite>();
						List<BuilderFlowNode> externalLinks = composite2.ExternalConnectedNodes;
						for (int i = 0; i < externalLinks.Count; i++)
						{
							BuilderFlowNode externalNode = externalLinks[i];
							LoopBuilderComposite externalLinkComposite = nodeToCompositeMap[externalNode];
							if (mergedComposites.Contains(externalLinkComposite))
							{
								BuilderFlowNode internalNode = composite2.GetConnectedInternalNode(externalNode);
								if (compositeHasBeenAttached)
								{
									bool flag = ConnectTwoPlacedLayoutNodes(internalNode, externalNode, layout);
									Debug.Log("Attempting to connect two extant nodes... " + flag);
									continue;
								}
								bool success2 = false;
								if (internalNode.node.isWarpWingEntrance)
								{
									IEnumerator AttachTracker2 = AttachWarpCanvasToLayout(externalNode, internalNode, canvas, layout).GetEnumerator();
									while (AttachTracker2.MoveNext())
									{
										yield return null;
									}
									success2 = AttachWarpCanvasSuccess;
								}
								else
								{
									IEnumerator AttachTracker = AttachNewCanvasToLayout(externalNode, internalNode, canvas, layout).GetEnumerator();
									while (AttachTracker.MoveNext())
									{
										yield return null;
									}
									success2 = AttachNewCanvasSuccess;
								}
								compositeHasBeenAttached = true;
								if (success2)
								{
									continue;
								}
								goto IL_0a78;
							}
							if (IsCompositeWarpWing(externalLinkComposite))
							{
								if (!warpCompositesToQueueLater.Contains(externalLinkComposite))
								{
									warpCompositesToQueueLater.Add(externalLinkComposite);
								}
							}
							else if (!compositesToEnqueue.Contains(externalLinkComposite))
							{
								compositesToEnqueue.Add(externalLinkComposite);
							}
						}
						mergedComposites.Add(composite2);
						compositesToEnqueue.Sort((LoopBuilderComposite a, LoopBuilderComposite b) => b.Nodes.Count - a.Nodes.Count);
						for (int num4 = 0; num4 < compositesToEnqueue.Count; num4++)
						{
							compositesToProcess.Enqueue(compositesToEnqueue[num4]);
						}
						if (compositesToProcess.Count == 0 && warpCompositesToQueueLater.Count > 0)
						{
							while (warpCompositesToQueueLater.Count > 0)
							{
								compositesToProcess.Enqueue(warpCompositesToQueueLater[0]);
								warpCompositesToQueueLater.RemoveAt(0);
							}
						}
						yield return null;
					}
				}
				else
				{
					IntVector2 zero = IntVector2.Zero;
					for (int num5 = 0; num5 < canvases.Count; num5++)
					{
						SemioticLayoutManager semioticLayoutManager = canvases[num5];
						zero += semioticLayoutManager.NegativeDimensions;
						semioticLayoutManager.HandleOffsetRooms(new IntVector2(zero.x, 0));
						zero += semioticLayoutManager.PositiveDimensions + new IntVector2(10, 0);
						layout.MergeLayout(semioticLayoutManager);
					}
				}
				if (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
				{
					BuilderFlowNode builderFlowNode = null;
					BuilderFlowNode builderFlowNode2 = null;
					for (int num6 = 0; num6 < allBuilderNodes.Count; num6++)
					{
						BuilderFlowNode builderFlowNode3 = allBuilderNodes[num6];
						if (builderFlowNode3 != null && !(builderFlowNode3.assignedPrototypeRoom == null))
						{
							if (builderFlowNode3.assignedPrototypeRoom.name == "OFFICE_03_OFFICE_01")
							{
								builderFlowNode2 = builderFlowNode3;
							}
							if (builderFlowNode3.assignedPrototypeRoom.name == "OFFICE_10_RND_01")
							{
								builderFlowNode = builderFlowNode3;
							}
						}
					}
					if (builderFlowNode != null && builderFlowNode2 != null)
					{
						ConnectTwoPlacedLayoutNodes(builderFlowNode, builderFlowNode2, layout);
					}
				}
				SanityCheckRooms(layout);
				DeferredGenerationSuccess = true;
				DeferredGeneratedLayout = layout;
				yield break;
				IL_0a78:
				LoopDungeonGenerator.NUM_FAILS_COMPOSITE_ATTACHMENT++;
				break;
			}
			DeferredGenerationSuccess = false;
		}

		private void SanityCheckRooms(SemioticLayoutManager layout)
		{
			for (int i = 0; i < layout.Rooms.Count; i++)
			{
				RoomHandler roomHandler = layout.Rooms[i];
				if (roomHandler.area.IsProceduralRoom)
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < allBuilderNodes.Count; j++)
				{
					BuilderFlowNode builderFlowNode = allBuilderNodes[j];
					if (builderFlowNode.instanceRoom == roomHandler)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					layout.Rooms.RemoveAt(i);
					i--;
				}
			}
		}
	}
}
