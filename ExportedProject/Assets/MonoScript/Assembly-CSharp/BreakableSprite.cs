using UnityEngine;

public class BreakableSprite : BraveBehaviour
{
	public bool animations = true;

	public BreakFrame[] breakFrames;

	public void Start()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnDamaged += OnHealthHaverDamaged;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnHealthHaverDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		for (int num = breakFrames.Length - 1; num >= 0; num--)
		{
			if (resultValue / maxValue <= breakFrames[num].healthPercentage / 100f)
			{
				string text = breakFrames[num].sprite;
				if (animations)
				{
					base.spriteAnimator.Play(text);
				}
				else
				{
					base.sprite.SetSprite(breakFrames[num].sprite);
				}
				break;
			}
		}
	}
}
