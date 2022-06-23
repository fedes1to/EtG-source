using UnityEngine;

public class MirrorDweller : BraveBehaviour
{
	public tk2dBaseSprite TargetSprite;

	public PlayerController TargetPlayer;

	public tk2dBaseSprite MirrorSprite;

	public bool UsesOverrideTintColor;

	public Color OverrideTintColor;

	private void Start()
	{
		base.sprite.usesOverrideMaterial = true;
		base.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Effects/StencilMasked");
		base.sprite.renderer.material.SetColor("_TintColor", new Color(0.4f, 0.4f, 0.8f, 0.5f));
		if (UsesOverrideTintColor)
		{
			base.sprite.renderer.material.SetColor("_TintColor", OverrideTintColor);
		}
	}

	private void LateUpdate()
	{
		if (TargetSprite != null && (bool)MirrorSprite)
		{
			if (Mathf.Abs(base.transform.position.x - TargetSprite.transform.position.x) < 5f)
			{
				base.sprite.renderer.enabled = true;
				base.sprite.SetSprite(TargetSprite.Collection, TargetSprite.spriteId);
				float num = MirrorSprite.transform.position.y - TargetSprite.transform.position.y;
				num /= 2f;
				num += 0.5f;
				base.transform.position = base.transform.position.WithX(TargetSprite.transform.position.x).WithY(MirrorSprite.transform.position.y + num).Quantize(0.0625f);
			}
			else
			{
				base.sprite.renderer.enabled = false;
			}
		}
		else
		{
			if (!(TargetPlayer != null) || !MirrorSprite)
			{
				return;
			}
			if (Mathf.Abs(base.transform.position.x - TargetPlayer.sprite.transform.position.x) < 5f)
			{
				base.sprite.renderer.enabled = true;
				base.sprite.SetSprite(TargetPlayer.sprite.Collection, TargetPlayer.GetMirrorSpriteID());
				float num2 = MirrorSprite.transform.position.y - TargetPlayer.transform.position.y;
				num2 /= 2f;
				num2 += 0.5f;
				base.transform.position = base.transform.position.WithX(TargetPlayer.transform.position.x).WithY(MirrorSprite.transform.position.y + num2).Quantize(0.0625f);
				base.sprite.HeightOffGround = num2 - 0.5f;
				base.sprite.FlipX = TargetPlayer.sprite.FlipX;
				if (base.sprite.FlipX)
				{
					base.transform.position += new Vector3(TargetPlayer.sprite.GetBounds().size.x, 0f, 0f);
				}
			}
			else
			{
				base.sprite.renderer.enabled = false;
			}
		}
	}
}
