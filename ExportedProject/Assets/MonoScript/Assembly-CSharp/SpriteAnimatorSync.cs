public class SpriteAnimatorSync : BraveBehaviour
{
	public tk2dBaseSprite otherSprite;

	public void Start()
	{
		if ((bool)otherSprite.spriteAnimator)
		{
			otherSprite.spriteAnimator.alwaysUpdateOffscreen = true;
		}
		otherSprite.SpriteChanged += OtherSpriteChanged;
		base.sprite.SetSprite(otherSprite.Collection, otherSprite.spriteId);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OtherSpriteChanged(tk2dBaseSprite tk2DBaseSprite)
	{
		base.sprite.SetSprite(otherSprite.spriteId);
	}
}
