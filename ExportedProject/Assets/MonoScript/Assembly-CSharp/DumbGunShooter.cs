using System.Collections;
using UnityEngine;

public class DumbGunShooter : GameActor
{
	public Gun gunToUse;

	public float continueShootTime;

	public float shootPauseTime;

	public bool overridesInaccuracy = true;

	public float inaccuracyFraction;

	private GunInventory inventory;

	public override Gun CurrentGun
	{
		get
		{
			return inventory.CurrentGun;
		}
	}

	public override Transform GunPivot
	{
		get
		{
			return base.transform;
		}
	}

	public override bool SpriteFlipped
	{
		get
		{
			return false;
		}
	}

	public override Vector3 SpriteDimensions
	{
		get
		{
			return Vector3.one;
		}
	}

	public override void Start()
	{
		inventory = new GunInventory(this);
		inventory.maxGuns = 1;
		inventory.AddGunToInventory(gunToUse, true);
		SpriteOutlineManager.AddOutlineToSprite(inventory.CurrentGun.sprite, Color.black, 0.1f, 0.05f);
		StartCoroutine(HandleGunShoot());
	}

	private IEnumerator HandleGunShoot()
	{
		yield return null;
		CurrentGun.ammo = 100000;
		CurrentGun.ClearReloadData();
		CurrentGun.HandleAimRotation(base.transform.position + Vector3.right * 100f);
		CurrentGun.Attack();
		if (continueShootTime > 0f)
		{
			float elapsed = 0f;
			while (elapsed < continueShootTime)
			{
				elapsed += BraveTime.DeltaTime;
				CurrentGun.ContinueAttack();
				yield return null;
			}
			CurrentGun.CeaseAttack();
		}
		yield return new WaitForSeconds(shootPauseTime);
		StartCoroutine(HandleGunShoot());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
