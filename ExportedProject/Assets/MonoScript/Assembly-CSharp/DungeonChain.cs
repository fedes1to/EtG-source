using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DungeonChain : ScriptableObject
{
	public string initialChainPrototype = "n";

	public List<ChainRule> chainRules;

	public int minChainLength = 3;

	public int maxChainLength = 8;

	public List<ChainRoom> mandatoryIncludedRooms;

	public List<ChainRoom> possiblyIncludedRooms;

	public List<ChainRoom> capRooms;

	public IntVector2 GetMandatoryDifficultyRating()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < mandatoryIncludedRooms.Count; i++)
		{
			if (!(mandatoryIncludedRooms[i].prototypeRoom == null))
			{
				num += mandatoryIncludedRooms[i].prototypeRoom.MinDifficultyRating;
				num2 += mandatoryIncludedRooms[i].prototypeRoom.MaxDifficultyRating;
			}
		}
		return new IntVector2(num, num2);
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
