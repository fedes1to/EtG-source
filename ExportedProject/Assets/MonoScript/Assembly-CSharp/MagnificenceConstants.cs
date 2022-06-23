using UnityEngine;

public static class MagnificenceConstants
{
	public const float COMMON_MAGNFIICENCE_ADJUSTMENT = 0f;

	public const float A_MAGNIFICENCE_ADJUSTMENT = 1f;

	public const float S_MAGNIFICENCE_ADJUSTMENT = 1f;

	public static PickupObject.ItemQuality ModifyQualityByMagnificence(PickupObject.ItemQuality targetQuality, float CurrentMagnificence, float dChance, float cChance, float bChance)
	{
		if (targetQuality == PickupObject.ItemQuality.S || targetQuality == PickupObject.ItemQuality.A)
		{
			float value = 0.006260342f + 0.9935921f * Mathf.Exp(-1.626339f * CurrentMagnificence);
			value = Mathf.Clamp01(value);
			float value2 = Random.value;
			if (Random.value > value)
			{
				float num = dChance + cChance + bChance;
				value2 = Random.value * num;
				if (value2 < dChance)
				{
					return PickupObject.ItemQuality.D;
				}
				if (value2 < dChance + cChance)
				{
					return PickupObject.ItemQuality.C;
				}
				if (value2 < dChance + cChance + bChance)
				{
					return PickupObject.ItemQuality.B;
				}
				return (!(Random.value < 0.5f)) ? PickupObject.ItemQuality.B : PickupObject.ItemQuality.C;
			}
			return targetQuality;
		}
		return targetQuality;
	}
}
