using System;
using UnityEngine;

public class ThrownGoopItem : MonoBehaviour
{
	public GoopDefinition goop;

	public float goopRadius = 3f;

	public bool CreatesProjectiles;

	public int NumProjectiles;

	public Projectile SourceProjectile;

	public bool UsesSynergyOverrideProjectile;

	public CustomSynergyType SynergyToCheck;

	public Projectile SynergyProjectile;

	public string burstAnim;

	public VFXPool burstVFX;

	private void Start()
	{
		AkSoundEngine.PostEvent("Play_OBJ_item_throw_01", base.gameObject);
		DebrisObject component = GetComponent<DebrisObject>();
		component.killTranslationOnBounce = false;
		if ((bool)component)
		{
			component.OnBounced = (Action<DebrisObject>)Delegate.Combine(component.OnBounced, new Action<DebrisObject>(OnBounced));
			component.OnGrounded = (Action<DebrisObject>)Delegate.Combine(component.OnGrounded, new Action<DebrisObject>(OnHitGround));
		}
	}

	private void OnBounced(DebrisObject obj)
	{
		if (goop != null)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goop).TimedAddGoopCircle(obj.sprite.WorldCenter, goopRadius);
		}
		if (CreatesProjectiles)
		{
			float num = 360f / (float)NumProjectiles;
			float num2 = UnityEngine.Random.Range(0f, num);
			Projectile projectile = SourceProjectile;
			if (UsesSynergyOverrideProjectile && GameManager.Instance.PrimaryPlayer.HasActiveBonusSynergy(SynergyToCheck))
			{
				projectile = SynergyProjectile;
			}
			for (int i = 0; i < NumProjectiles; i++)
			{
				float z = num2 + num * (float)i;
				GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, obj.sprite.WorldCenter, Quaternion.Euler(0f, 0f, z));
				Projectile component = gameObject.GetComponent<Projectile>();
				component.Owner = GameManager.Instance.PrimaryPlayer;
				component.Shooter = GameManager.Instance.PrimaryPlayer.specRigidbody;
				component.collidesWithPlayer = false;
				component.collidesWithEnemies = true;
			}
		}
	}

	private void OnHitGround(DebrisObject obj)
	{
		AkSoundEngine.PostEvent("Play_WPN_molotov_impact_01", base.gameObject);
		OnBounced(obj);
		burstVFX.SpawnAtPosition(GetComponent<tk2dSprite>().WorldCenter);
		if (!string.IsNullOrEmpty(burstAnim))
		{
			GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject(burstAnim);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
