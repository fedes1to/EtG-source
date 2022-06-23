using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class CardboardBoxItem : PlayerItem
{
	public GameObject prefabToAttachToPlayer;

	public float SneakAttackDamageMultiplier = 2f;

	private GameObject instanceBox;

	private tk2dSprite instanceBoxSprite;

	private PlayerController m_player;

	protected override void OnPreDrop(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			BreakStealth(user);
		}
		base.OnPreDrop(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		m_player = user;
		if ((bool)m_player && (bool)m_player.CurrentGun)
		{
			m_player.CurrentGun.CeaseAttack(false);
		}
		base.IsCurrentlyActive = true;
		bool flag = CanAnyBossSee(user);
		m_player.OnDidUnstealthyAction += BreakStealth;
		m_player.PostProcessProjectile += SneakAttackProcessor;
		m_player.healthHaver.OnDamaged += OnDamaged;
		if (!flag)
		{
			user.SetIsStealthed(true, "box");
			user.SetCapableOfStealing(true, "CardboardBoxItem");
		}
		instanceBox = user.RegisterAttachedObject(prefabToAttachToPlayer, string.Empty);
		instanceBoxSprite = instanceBox.GetComponent<tk2dSprite>();
		instanceBoxSprite.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.LowerLeft);
		instanceBoxSprite.spriteAnimator.Play("cardboard_on");
		user.StartCoroutine(HandlePutOn(user, instanceBoxSprite));
	}

	private void SneakAttackProcessor(Projectile arg1, float arg2)
	{
		if ((bool)m_player && m_player.IsStealthed)
		{
			arg1.baseData.damage *= SneakAttackDamageMultiplier;
		}
	}

	private bool CanAnyBossSee(PlayerController user)
	{
		Vector2 centerPosition = user.CenterPosition;
		for (int i = 0; i < StaticReferenceManager.AllNpcs.Count; i++)
		{
			if (StaticReferenceManager.AllNpcs[i].ParentRoom != user.CurrentRoom)
			{
				continue;
			}
			Vector2 unitCenter = StaticReferenceManager.AllNpcs[i].specRigidbody.UnitCenter;
			int standardEnemyVisibilityMask = CollisionMask.StandardEnemyVisibilityMask;
			RaycastResult result;
			if (!PhysicsEngine.Instance.Raycast(unitCenter, centerPosition - unitCenter, (centerPosition - unitCenter).magnitude, out result, true, true, standardEnemyVisibilityMask, null, false, null, StaticReferenceManager.AllNpcs[i].specRigidbody))
			{
				RaycastResult.Pool.Free(ref result);
				continue;
			}
			if (result.SpeculativeRigidbody == null || result.SpeculativeRigidbody.gameObject != user.gameObject)
			{
				RaycastResult.Pool.Free(ref result);
				continue;
			}
			RaycastResult.Pool.Free(ref result);
			return true;
		}
		if (user.CurrentRoom != null)
		{
			List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				for (int j = 0; j < activeEnemies.Count; j++)
				{
					if (!activeEnemies[j] || !activeEnemies[j].specRigidbody || !activeEnemies[j].healthHaver || !activeEnemies[j].healthHaver.IsBoss)
					{
						continue;
					}
					Vector2 unitCenter2 = activeEnemies[j].specRigidbody.UnitCenter;
					int standardEnemyVisibilityMask2 = CollisionMask.StandardEnemyVisibilityMask;
					RaycastResult result2;
					if (!PhysicsEngine.Instance.Raycast(unitCenter2, centerPosition - unitCenter2, (centerPosition - unitCenter2).magnitude, out result2, true, true, standardEnemyVisibilityMask2, null, false, null, activeEnemies[j].specRigidbody))
					{
						RaycastResult.Pool.Free(ref result2);
						continue;
					}
					if (result2.SpeculativeRigidbody == null || result2.SpeculativeRigidbody.gameObject != user.gameObject)
					{
						RaycastResult.Pool.Free(ref result2);
						continue;
					}
					RaycastResult.Pool.Free(ref result2);
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator HandlePutOn(PlayerController user, tk2dBaseSprite instanceBoxSprite)
	{
		yield return new WaitForSeconds(0.2f);
		if (base.IsCurrentlyActive)
		{
			user.IsVisible = false;
			instanceBoxSprite.renderer.enabled = true;
		}
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		BreakStealth(m_player);
	}

	private void BreakStealth(PlayerController obj)
	{
		m_player.OnDidUnstealthyAction -= BreakStealth;
		m_player.healthHaver.OnDamaged -= OnDamaged;
		m_player.PostProcessProjectile -= SneakAttackProcessor;
		base.IsCurrentlyActive = false;
		obj.IsVisible = true;
		obj.SetIsStealthed(false, "box");
		obj.SetCapableOfStealing(false, "CardboardBoxItem");
		obj.DeregisterAttachedObject(instanceBox, false);
		instanceBoxSprite.spriteAnimator.PlayAndDestroyObject("cardboard_off");
		instanceBoxSprite = null;
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		BreakStealth(user);
	}

	public void LateUpdate()
	{
		if (!base.IsCurrentlyActive)
		{
			return;
		}
		if (instanceBoxSprite.FlipX != m_player.sprite.FlipX)
		{
			instanceBoxSprite.FlipX = m_player.sprite.FlipX;
		}
		instanceBoxSprite.PlaceAtPositionByAnchor(m_player.SpriteBottomCenter + ((!m_player.sprite.FlipX) ? 1 : (-1)) * new Vector3(-0.5f, 0f, 0f), tk2dBaseSprite.Anchor.LowerCenter);
		if (instanceBoxSprite.spriteAnimator.IsPlaying("cardboard_on"))
		{
			return;
		}
		if (m_player.specRigidbody.Velocity == Vector2.zero)
		{
			if (m_player.spriteAnimator.CurrentClip.name.Contains("backward") || m_player.spriteAnimator.CurrentClip.name.Contains("_bw"))
			{
				instanceBoxSprite.spriteAnimator.Play("idle_back");
			}
			else
			{
				instanceBoxSprite.spriteAnimator.Play("idle");
			}
		}
		else if (m_player.spriteAnimator.CurrentClip.name.Contains("run_up") || m_player.spriteAnimator.CurrentClip.name.Contains("_bw"))
		{
			instanceBoxSprite.spriteAnimator.Play("move_right_backwards");
		}
		else
		{
			instanceBoxSprite.spriteAnimator.Play("move_right_forward");
		}
	}

	public override void OnItemSwitched(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			DoActiveEffect(user);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
