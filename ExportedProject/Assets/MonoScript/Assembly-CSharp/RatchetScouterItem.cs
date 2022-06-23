using System;
using UnityEngine;

public class RatchetScouterItem : PassiveItem
{
	public GameObject VFXHealthBar;

	public override void Pickup(PlayerController player)
	{
		player.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Combine(player.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(AnyDamageDealt));
		base.Pickup(player);
	}

	private void AnyDamageDealt(float damageAmount, bool fatal, HealthHaver target)
	{
		int a = Mathf.RoundToInt(damageAmount);
		Vector3 worldPosition = target.transform.position;
		float heightOffGround = 1f;
		SpeculativeRigidbody component = target.GetComponent<SpeculativeRigidbody>();
		if ((bool)component)
		{
			worldPosition = component.UnitCenter.ToVector3ZisY();
			heightOffGround = worldPosition.y - component.UnitBottomCenter.y;
			if ((bool)component.healthHaver && !component.healthHaver.HasHealthBar && !component.healthHaver.HasRatchetHealthBar && !component.healthHaver.IsBoss)
			{
				component.healthHaver.HasRatchetHealthBar = true;
				GameObject gameObject = UnityEngine.Object.Instantiate(VFXHealthBar);
				SimpleHealthBarController component2 = gameObject.GetComponent<SimpleHealthBarController>();
				component2.Initialize(component, component.healthHaver);
			}
		}
		else
		{
			AIActor component3 = target.GetComponent<AIActor>();
			if ((bool)component3)
			{
				worldPosition = component3.CenterPosition.ToVector3ZisY();
				if ((bool)component3.sprite)
				{
					heightOffGround = worldPosition.y - component3.sprite.WorldBottomCenter.y;
				}
			}
		}
		a = Mathf.Max(a, 1);
		GameUIRoot.Instance.DoDamageNumber(worldPosition, heightOffGround, a);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		if ((bool)player)
		{
			player.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Remove(player.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(AnyDamageDealt));
		}
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if ((bool)base.Owner)
		{
			PlayerController owner = base.Owner;
			owner.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Remove(owner.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(AnyDamageDealt));
		}
		base.OnDestroy();
	}
}
