using System.Collections.Generic;

public class SpiceItem : PlayerItem
{
	public static float ONE_SPICE_WEIGHT = 0.1f;

	public static float TWO_SPICE_WEIGHT = 0.3f;

	public static float THREE_SPICE_WEIGHT = 0.5f;

	public static float FOUR_PLUS_SPICE_WEIGHT = 0.8f;

	public List<StatModifier> FirstTimeStatModifiers;

	public List<StatModifier> SecondTimeStatModifiers;

	public List<StatModifier> ThirdTimeStatModifiers;

	public List<StatModifier> FourthAndBeyondStatModifiers;

	public static float GetSpiceWeight(int spiceCount)
	{
		if (spiceCount <= 0)
		{
			return 0f;
		}
		switch (spiceCount)
		{
		case 1:
			return ONE_SPICE_WEIGHT;
		case 2:
			return TWO_SPICE_WEIGHT;
		case 3:
			return THREE_SPICE_WEIGHT;
		default:
			return FOUR_PLUS_SPICE_WEIGHT;
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
		if (user.spiceCount == 0)
		{
			for (int i = 0; i < FirstTimeStatModifiers.Count; i++)
			{
				user.ownerlessStatModifiers.Add(FirstTimeStatModifiers[i]);
			}
		}
		else if (user.spiceCount == 1)
		{
			for (int j = 0; j < SecondTimeStatModifiers.Count; j++)
			{
				user.ownerlessStatModifiers.Add(SecondTimeStatModifiers[j]);
			}
		}
		else if (user.spiceCount == 2)
		{
			for (int k = 0; k < ThirdTimeStatModifiers.Count; k++)
			{
				user.ownerlessStatModifiers.Add(ThirdTimeStatModifiers[k]);
			}
		}
		else if (user.spiceCount > 2)
		{
			for (int l = 0; l < FourthAndBeyondStatModifiers.Count; l++)
			{
				user.ownerlessStatModifiers.Add(FourthAndBeyondStatModifiers[l]);
			}
		}
		user.stats.RecalculateStats(user);
		user.spiceCount++;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
