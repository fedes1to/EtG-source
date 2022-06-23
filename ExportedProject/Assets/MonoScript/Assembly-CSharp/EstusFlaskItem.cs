using System;
using System.Collections;
using UnityEngine;

public class EstusFlaskItem : PlayerItem
{
	public int numDrinksPerFloor = 2;

	public float healingAmount = 1f;

	public float drinkDuration = 1f;

	public string HasDrinkSprite;

	public string NoDrinkSprite;

	public GameObject healVFX;

	private PlayerController m_owner;

	private int m_remainingDrinksThisFloor;

	public int RemainingDrinks
	{
		get
		{
			return m_remainingDrinksThisFloor;
		}
	}

	public override void Pickup(PlayerController player)
	{
		m_owner = player;
		if (!m_pickedUpThisRun)
		{
			m_remainingDrinksThisFloor = numDrinksPerFloor;
		}
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(ResetFlaskForFloor));
		base.Pickup(player);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		user.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(user.OnNewFloorLoaded, new Action<PlayerController>(ResetFlaskForFloor));
		m_owner = null;
		base.OnPreDrop(user);
	}

	private void ResetFlaskForFloor(PlayerController obj)
	{
		m_remainingDrinksThisFloor = numDrinksPerFloor;
		base.sprite.SetSprite(HasDrinkSprite);
	}

	public override bool CanBeUsed(PlayerController user)
	{
		return m_remainingDrinksThisFloor > 0;
	}

	protected override void DoEffect(PlayerController user)
	{
		if (m_remainingDrinksThisFloor > 0)
		{
			m_remainingDrinksThisFloor--;
			user.StartCoroutine(HandleDrinkEstus(user));
		}
		if (m_remainingDrinksThisFloor <= 0)
		{
			base.sprite.SetSprite(NoDrinkSprite);
		}
	}

	private IEnumerator HandleDrinkEstus(PlayerController user)
	{
		float elapsed = 0f;
		if (healVFX != null)
		{
			user.PlayEffectOnActor(healVFX, Vector3.zero);
		}
		user.SetInputOverride("estus");
		while (elapsed < drinkDuration)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		user.ClearInputOverride("estus");
		user.healthHaver.ApplyHealing(healingAmount);
		AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
	}

	protected override void CopyStateFrom(PlayerItem other)
	{
		base.CopyStateFrom(other);
		EstusFlaskItem estusFlaskItem = other as EstusFlaskItem;
		if ((bool)estusFlaskItem)
		{
			m_remainingDrinksThisFloor = estusFlaskItem.m_remainingDrinksThisFloor;
		}
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			PlayerController owner = m_owner;
			owner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(owner.OnNewFloorLoaded, new Action<PlayerController>(ResetFlaskForFloor));
		}
		base.OnDestroy();
	}
}
