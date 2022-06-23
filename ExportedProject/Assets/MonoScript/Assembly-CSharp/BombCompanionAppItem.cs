using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BombCompanionAppItem : PlayerItem
{
	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_computer_boop_01", base.gameObject);
		RoomHandler currentRoom = user.CurrentRoom;
		if (currentRoom == null)
		{
			return;
		}
		for (int i = 0; i < StaticReferenceManager.AllMinorBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = StaticReferenceManager.AllMinorBreakables[i];
			if (minorBreakable.transform.position.GetAbsoluteRoom() == currentRoom && !minorBreakable.IsBroken && minorBreakable.explodesOnBreak)
			{
				minorBreakable.Break();
			}
		}
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies != null)
		{
			for (int num = activeEnemies.Count - 1; num >= 0; num--)
			{
				AIActor aIActor = activeEnemies[num];
				if ((bool)aIActor && !aIActor.IsSignatureEnemy)
				{
					HealthHaver healthHaver = aIActor.healthHaver;
					if ((bool)healthHaver && !healthHaver.IsDead && !healthHaver.IsBoss)
					{
						ExplodeOnDeath component = aIActor.GetComponent<ExplodeOnDeath>();
						if ((bool)component && !component.immuneToIBombApp)
						{
							healthHaver.ApplyDamage(2.14748365E+09f, Vector2.zero, "iBomb", CoreDamageTypes.None, DamageCategory.Normal, true);
						}
					}
				}
			}
		}
		List<Projectile> list = new List<Projectile>();
		for (int j = 0; j < StaticReferenceManager.AllProjectiles.Count; j++)
		{
			if ((bool)StaticReferenceManager.AllProjectiles[j] && StaticReferenceManager.AllProjectiles[j].GetComponent<ExplosiveModifier>() != null)
			{
				list.Add(StaticReferenceManager.AllProjectiles[j]);
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			list[k].DieInAir();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
