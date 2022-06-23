using System.Collections;
using UnityEngine;

public class ConsumableStealthItem : PlayerItem
{
	public float Duration = 10f;

	public GameObject poofVfx;

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
		user.StartCoroutine(HandleStealth(user));
	}

	private IEnumerator HandleStealth(PlayerController user)
	{
		float elapsed = 0f;
		user.ChangeSpecialShaderFlag(1, 1f);
		user.SetIsStealthed(true, "smoke");
		user.SetCapableOfStealing(true, "ConsumableStealthItem");
		user.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
		user.PlayEffectOnActor(poofVfx, Vector3.zero, false, true);
		user.OnDidUnstealthyAction += BreakStealth;
		user.OnItemStolen += BreakStealthOnSteal;
		while (elapsed < Duration)
		{
			elapsed += BraveTime.DeltaTime;
			if (!user.IsStealthed)
			{
				break;
			}
			yield return null;
		}
		if (user.IsStealthed)
		{
			BreakStealth(user);
		}
	}

	private void BreakStealthOnSteal(PlayerController arg1, ShopItemController arg2)
	{
		BreakStealth(arg1);
	}

	private void BreakStealth(PlayerController obj)
	{
		obj.PlayEffectOnActor(poofVfx, Vector3.zero, false, true);
		obj.OnDidUnstealthyAction -= BreakStealth;
		obj.OnItemStolen -= BreakStealthOnSteal;
		obj.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
		obj.ChangeSpecialShaderFlag(1, 0f);
		obj.SetIsStealthed(false, "smoke");
		obj.SetCapableOfStealing(false, "ConsumableStealthItem");
		AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
	}
}
