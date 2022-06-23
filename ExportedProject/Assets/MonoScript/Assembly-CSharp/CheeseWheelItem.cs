using System;
using System.Collections;
using UnityEngine;

public class CheeseWheelItem : PlayerItem
{
	public float duration = 10f;

	public float BossContactDamage = 30f;

	public GameObject TransformationVFX;

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		user.StartCoroutine(HandleDuration(user));
	}

	private IEnumerator HandleDuration(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			yield break;
		}
		base.IsCurrentlyActive = true;
		GameObject instanceVFX = user.PlayEffectOnActor(TransformationVFX, Vector3.zero, true, true);
		instanceVFX.transform.localPosition = instanceVFX.transform.localPosition.QuantizeFloor(0.0625f);
		tk2dSprite instanceVFXSprite = instanceVFX.GetComponent<tk2dSprite>();
		tk2dSpriteAnimator instanceVFXAnimator = instanceVFX.GetComponent<tk2dSpriteAnimator>();
		user.IsVisible = false;
		user.ToggleShadowVisiblity(true);
		user.SetIsFlying(true, "pacman", false);
		m_activeElapsed = 0f;
		m_activeDuration = duration;
		SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePrerigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = user.specRigidbody;
		speculativeRigidbody2.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		bool hasPlayedBody = false;
		while (m_activeElapsed < m_activeDuration && base.IsCurrentlyActive)
		{
			user.healthHaver.IsVulnerable = false;
			user.IsOnFire = false;
			user.CurrentPoisonMeterValue = 0f;
			if (user.IsVisible)
			{
				user.IsVisible = false;
				user.ToggleShadowVisiblity(true);
			}
			if ((bool)instanceVFXAnimator)
			{
				if (!hasPlayedBody)
				{
					if (!instanceVFXAnimator.IsPlaying("Resourceful_Rat_pac_intro"))
					{
						hasPlayedBody = true;
						instanceVFXAnimator.Play("Resourceful_Rat_pac_player");
					}
				}
				else if (m_activeElapsed > m_activeDuration - 0.9f && !instanceVFXAnimator.IsPlaying("Resourceful_Rat_pac_outro"))
				{
					instanceVFXAnimator.Play("Resourceful_Rat_pac_outro");
				}
			}
			if (user.specRigidbody.Velocity != Vector2.zero)
			{
				float z = user.specRigidbody.Velocity.ToAngle();
				if ((bool)instanceVFX)
				{
					instanceVFX.transform.localRotation = Quaternion.Euler(0f, 0f, z);
					instanceVFXSprite.ForceRotationRebuild();
				}
			}
			yield return null;
		}
		user.healthHaver.IsVulnerable = true;
		SpeculativeRigidbody speculativeRigidbody3 = user.specRigidbody;
		speculativeRigidbody3.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody3.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePrerigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody4 = user.specRigidbody;
		speculativeRigidbody4.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody4.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		user.SetIsFlying(false, "pacman", false);
		user.IsVisible = true;
		if ((bool)instanceVFX)
		{
			SpawnManager.Despawn(instanceVFX);
		}
		base.IsCurrentlyActive = false;
	}

	private void HandlePrerigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody && (bool)otherRigidbody.healthHaver && otherRigidbody.healthHaver.IsDead)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		AIActor component = rigidbodyCollision.OtherRigidbody.GetComponent<AIActor>();
		bool flag = false;
		if ((bool)component && component.IsNormalEnemy && (bool)component.healthHaver && component.healthHaver.IsVulnerable)
		{
			if (component.FlagToSetOnDeath == GungeonFlags.BOSSKILLED_DEMONWALL)
			{
				flag = true;
				component.healthHaver.ApplyDamage(BossContactDamage, rigidbodyCollision.Normal * -1f, "pakku pakku");
				if ((bool)rigidbodyCollision.MyRigidbody && (bool)rigidbodyCollision.MyRigidbody.knockbackDoer)
				{
					rigidbodyCollision.MyRigidbody.knockbackDoer.ApplyKnockback(Vector2.down, 80f);
				}
			}
			else if (component.healthHaver.IsBoss)
			{
				flag = true;
				component.healthHaver.ApplyDamage(BossContactDamage, rigidbodyCollision.Normal * -1f, "pakku pakku");
				if ((bool)rigidbodyCollision.MyRigidbody && (bool)rigidbodyCollision.MyRigidbody.knockbackDoer)
				{
					rigidbodyCollision.MyRigidbody.knockbackDoer.ApplyKnockback(rigidbodyCollision.Normal, 40f);
				}
			}
			else
			{
				KeyBulletManController component2 = component.GetComponent<KeyBulletManController>();
				if ((bool)component2)
				{
					component2.ForceHandleRewards();
				}
				GameManager.Instance.Dungeon.StartCoroutine(HandleEnemySuck(component, rigidbodyCollision.MyRigidbody));
				component.EraseFromExistenceWithRewards();
			}
		}
		else
		{
			MajorBreakable component3 = rigidbodyCollision.OtherRigidbody.GetComponent<MajorBreakable>();
			BodyPartController component4 = rigidbodyCollision.OtherRigidbody.GetComponent<BodyPartController>();
			if ((bool)component4 && (bool)component3)
			{
				flag = true;
				Vector2 normalized = (rigidbodyCollision.MyRigidbody.UnitCenter - rigidbodyCollision.OtherRigidbody.UnitCenter).normalized;
				component3.ApplyDamage(BossContactDamage / 2f, normalized * -1f, false);
				if ((bool)component3.healthHaver)
				{
					component3.healthHaver.ApplyDamage(BossContactDamage / 2f, normalized * -1f, "pakku pakku");
				}
				if ((bool)rigidbodyCollision.MyRigidbody && (bool)rigidbodyCollision.MyRigidbody.knockbackDoer)
				{
					rigidbodyCollision.MyRigidbody.knockbackDoer.ApplyKnockback(normalized.normalized, 40f);
				}
			}
		}
		if (flag)
		{
			rigidbodyCollision.MyRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 0.5f);
		}
	}

	private IEnumerator HandleEnemySuck(AIActor target, SpeculativeRigidbody ownerRigidbody)
	{
		if (!target || !ownerRigidbody)
		{
			yield break;
		}
		Transform copySprite = CreateEmptySprite(target);
		tk2dSprite copySpriteSprite = copySprite.GetComponentInChildren<tk2dSprite>();
		if ((bool)copySpriteSprite)
		{
			copySpriteSprite.HeightOffGround = -1.25f;
		}
		Vector3 startPosition = copySprite.transform.position;
		float elapsed = 0f;
		float duration = 0.25f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if ((bool)ownerRigidbody && (bool)copySprite)
			{
				Vector3 b = ownerRigidbody.UnitCenter;
				float t = elapsed / duration * (elapsed / duration);
				copySprite.position = Vector3.Lerp(startPosition, b, t);
				copySprite.rotation = Quaternion.Euler(0f, 0f, 720f * BraveTime.DeltaTime) * copySprite.rotation;
				copySprite.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), t);
				if ((bool)copySpriteSprite)
				{
					copySpriteSprite.UpdateZDepth();
				}
			}
			yield return null;
		}
		if ((bool)copySprite)
		{
			UnityEngine.Object.Destroy(copySprite.gameObject);
		}
	}

	private Transform CreateEmptySprite(AIActor target)
	{
		GameObject gameObject = new GameObject("suck image");
		gameObject.layer = target.gameObject.layer;
		tk2dSprite tk2dSprite2 = gameObject.AddComponent<tk2dSprite>();
		gameObject.transform.parent = SpawnManager.Instance.VFX;
		tk2dSprite2.SetSprite(target.sprite.Collection, target.sprite.spriteId);
		tk2dSprite2.transform.position = target.sprite.transform.position;
		GameObject gameObject2 = new GameObject("image parent");
		gameObject2.transform.position = tk2dSprite2.WorldCenter;
		tk2dSprite2.transform.parent = gameObject2.transform;
		if (target.optionalPalette != null)
		{
			tk2dSprite2.renderer.material.SetTexture("_PaletteTex", target.optionalPalette);
		}
		return gameObject2.transform;
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.IsCurrentlyActive = false;
		base.OnPreDrop(user);
	}
}
