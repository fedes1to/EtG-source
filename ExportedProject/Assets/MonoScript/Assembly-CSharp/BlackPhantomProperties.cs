using System;

[Serializable]
public class BlackPhantomProperties
{
	public static float GlobalPercentIncrease = 0.3f;

	public static float GlobalFlatIncrease = 10f;

	public static float GlobalBossPercentIncrease = 0.2f;

	public static float GlobalBossFlatIncrease = 100f;

	public float BonusHealthPercentIncrease;

	public float BonusHealthFlatIncrease;

	public float MaxTotalHealth = 175f;

	public float CooldownMultiplier = 0.66f;

	public float MovementSpeedMultiplier = 1.5f;

	public float LocalTimeScaleMultiplier = 1f;

	public float BulletSpeedMultiplier = 1f;

	public float GradientScale = 0.75f;

	public float ContrastPower = 1.3f;
}
