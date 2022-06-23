using System.Collections.Generic;

public class RandomizeSprite : BraveBehaviour
{
	[CheckSprite(null)]
	public List<string> spriteNames;

	[CheckAnimation(null)]
	public List<string> animationNames;

	public bool UseStaticIndex;

	private static int s_index;

	public void Start()
	{
		if (UseStaticIndex)
		{
			if (spriteNames.Count > 0)
			{
				base.sprite.SetSprite(spriteNames[s_index % spriteNames.Count]);
			}
			if (animationNames.Count > 0)
			{
				base.spriteAnimator.Play(animationNames[s_index % animationNames.Count]);
			}
			s_index++;
			if (s_index < 0)
			{
				s_index = 0;
			}
		}
		else
		{
			if (spriteNames.Count > 0)
			{
				base.sprite.SetSprite(BraveUtility.RandomElement(spriteNames));
			}
			if (animationNames.Count > 0)
			{
				base.spriteAnimator.Play(BraveUtility.RandomElement(animationNames));
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
