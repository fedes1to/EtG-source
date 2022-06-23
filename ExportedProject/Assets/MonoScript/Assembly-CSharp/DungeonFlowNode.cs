using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class DungeonFlowNode
{
	public enum ControlNodeType
	{
		ROOM,
		SUBCHAIN,
		SELECTOR
	}

	public enum NodePriority
	{
		MANDATORY,
		OPTIONAL
	}

	public enum ForcedDoorType
	{
		NONE,
		LOCKED,
		ONE_WAY
	}

	public bool isSubchainStandin;

	public ControlNodeType nodeType;

	public PrototypeDungeonRoom.RoomCategory roomCategory;

	public float percentChance = 1f;

	public NodePriority priority;

	public PrototypeDungeonRoom overrideExactRoom;

	public GenericRoomTable overrideRoomTable;

	public bool capSubchain;

	public string subchainIdentifier;

	public bool limitedCopiesOfSubchain;

	public int maxCopiesOfSubchain = 1;

	public List<string> subchainIdentifiers;

	public bool receivesCaps;

	public bool isWarpWingEntrance;

	public bool handlesOwnWarping;

	public ForcedDoorType forcedDoorType;

	public ForcedDoorType loopForcedDoorType;

	public bool nodeExpands;

	public string initialChainPrototype = "n";

	public List<ChainRule> chainRules;

	public int minChainLength = 3;

	public int maxChainLength = 8;

	public int minChildrenToBuild = 1;

	public int maxChildrenToBuild = 1;

	public bool canBuildDuplicateChildren;

	public string parentNodeGuid;

	public List<string> childNodeGuids;

	public string loopTargetNodeGuid;

	public bool loopTargetIsOneWay;

	[HideInInspector]
	public string guidAsString;

	public DungeonFlow flow;

	public bool UsesGlobalBossData
	{
		get
		{
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH)
			{
				return false;
			}
			return overrideExactRoom == null && roomCategory == PrototypeDungeonRoom.RoomCategory.BOSS;
		}
	}

	public DungeonFlowNode(DungeonFlow parentFlow)
	{
		flow = parentFlow;
		childNodeGuids = new List<string>();
		guidAsString = Guid.NewGuid().ToString();
	}

	public static bool operator ==(DungeonFlowNode a, DungeonFlowNode b)
	{
		if (object.ReferenceEquals(a, b))
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.guidAsString == b.guidAsString;
	}

	public static bool operator !=(DungeonFlowNode a, DungeonFlowNode b)
	{
		return !(a == b);
	}

	protected bool Equals(DungeonFlowNode other)
	{
		return string.Equals(guidAsString, other.guidAsString);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((DungeonFlowNode)obj);
	}

	public override int GetHashCode()
	{
		return (guidAsString != null) ? guidAsString.GetHashCode() : 0;
	}

	public int GetAverageNumberNodes()
	{
		if (nodeExpands)
		{
			return Mathf.Max(Mathf.FloorToInt((float)(minChainLength + maxChainLength) / 2f), 1);
		}
		if (nodeType == ControlNodeType.SELECTOR)
		{
			return 0;
		}
		return 1;
	}

	public string EvolveChainToCompletion()
	{
		int num = BraveRandom.GenerationRandomRange(minChainLength, maxChainLength + 1);
		string text = initialChainPrototype;
		while (text.Length < num)
		{
			int length = text.Length;
			text = EvolveChain(text);
			if (text.Length >= num)
			{
				bool flag = true;
				while (flag)
				{
					flag = false;
					string text2 = text;
					text = ApplyMandatoryRule(text);
					if (text2 != text)
					{
						flag = true;
					}
				}
			}
			if (text.Length == length)
			{
				break;
			}
		}
		return text;
	}

	private string ApplyMandatoryRule(string current)
	{
		List<ChainRule> list = new List<ChainRule>();
		for (int i = 0; i < chainRules.Count; i++)
		{
			ChainRule chainRule = chainRules[i];
			if (chainRule.mandatory && current.Contains(chainRule.form))
			{
				list.Add(chainRule);
			}
		}
		if (list.Count == 0)
		{
			return current;
		}
		ChainRule chainRule2 = SelectRuleByWeighting(list);
		MatchCollection matchCollection = Regex.Matches(current, chainRule2.form);
		Match match = matchCollection[BraveRandom.GenerationRandomRange(0, matchCollection.Count)];
		string text = ((match.Index == 0) ? string.Empty : current.Substring(0, match.Index));
		string text2 = ((match.Index == current.Length - 1) ? string.Empty : current.Substring(match.Index + chainRule2.form.Length));
		return text + chainRule2.target + text2;
	}

	public string EvolveChain(string current)
	{
		List<ChainRule> list = new List<ChainRule>();
		for (int i = 0; i < chainRules.Count; i++)
		{
			ChainRule chainRule = chainRules[i];
			if (current.Contains(chainRule.form))
			{
				list.Add(chainRule);
			}
		}
		if (list.Count == 0)
		{
			BraveUtility.Log("A DungeonChain has no associated rules. This works if no evolution is desired, but here's a warning just in case...", Color.yellow);
			return current;
		}
		ChainRule chainRule2 = SelectRuleByWeighting(list);
		MatchCollection matchCollection = Regex.Matches(current, chainRule2.form);
		Match match = matchCollection[BraveRandom.GenerationRandomRange(0, matchCollection.Count)];
		string text = ((match.Index == 0) ? string.Empty : current.Substring(0, match.Index));
		string text2 = ((match.Index == current.Length - 1) ? string.Empty : current.Substring(match.Index + chainRule2.form.Length));
		return text + chainRule2.target + text2;
	}

	private ChainRule SelectRuleByWeighting(List<ChainRule> source)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < source.Count; i++)
		{
			num2 += source[i].weight;
		}
		float num3 = BraveRandom.GenerationRandomValue() * num2;
		for (int j = 0; j < source.Count; j++)
		{
			num += source[j].weight;
			if (num >= num3)
			{
				return source[j];
			}
		}
		return null;
	}
}
