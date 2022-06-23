public class BulletArmorItem : PassiveItem
{
	public tk2dSpriteAnimation knightLibrary;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			if (!m_pickedUpThisRun)
			{
				m_player.healthHaver.Armor += 1f;
			}
			base.Pickup(player);
			GameManager.Instance.OnNewLevelFullyLoaded += GainArmorOnLevelLoad;
			ProcessLegendaryStatus(player);
		}
	}

	private void ProcessLegendaryStatus(PlayerController player)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		PassiveItem passiveItem = null;
		PassiveItem passiveItem2 = null;
		PassiveItem passiveItem3 = null;
		PassiveItem passiveItem4 = null;
		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i].DisplayName == "Gunknight Gauntlet")
			{
				flag = true;
				passiveItem3 = player.passiveItems[i];
			}
			if (player.passiveItems[i].DisplayName == "Gunknight Armor")
			{
				flag2 = true;
				passiveItem2 = player.passiveItems[i];
			}
			if (player.passiveItems[i].DisplayName == "Gunknight Helmet")
			{
				flag3 = true;
				passiveItem = player.passiveItems[i];
			}
			if (player.passiveItems[i].DisplayName == "Gunknight Greaves")
			{
				flag4 = true;
				passiveItem4 = player.passiveItems[i];
			}
		}
		if (flag && flag2 && flag3 && flag4)
		{
			passiveItem.CanBeDropped = false;
			passiveItem.CanBeSold = false;
			passiveItem2.CanBeDropped = false;
			passiveItem2.CanBeSold = false;
			passiveItem3.CanBeDropped = false;
			passiveItem3.CanBeSold = false;
			passiveItem4.CanBeDropped = false;
			passiveItem4.CanBeSold = false;
			player.OverrideAnimationLibrary = knightLibrary;
			player.SetOverrideShader(ShaderCache.Acquire(player.LocalShaderName));
			if (player.characterIdentity == PlayableCharacters.Eevee)
			{
				player.GetComponent<CharacterAnimationRandomizer>().AddOverrideAnimLibrary(knightLibrary);
			}
			StatModifier statModifier = new StatModifier();
			statModifier.amount = -1000f;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier.statToBoost = PlayerStats.StatType.ReloadSpeed;
			player.ownerlessStatModifiers.Add(statModifier);
			StatModifier statModifier2 = new StatModifier();
			statModifier2.amount = 3f;
			statModifier2.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier2.statToBoost = PlayerStats.StatType.Curse;
			player.ownerlessStatModifiers.Add(statModifier2);
			player.stats.RecalculateStats(player);
			if (!PassiveItem.IsFlagSetForCharacter(player, typeof(BulletArmorItem)))
			{
				PassiveItem.IncrementFlag(player, typeof(BulletArmorItem));
			}
		}
		else
		{
			if (!PassiveItem.IsFlagSetForCharacter(player, typeof(BulletArmorItem)))
			{
				return;
			}
			PassiveItem.DecrementFlag(player, typeof(BulletArmorItem));
			player.OverrideAnimationLibrary = null;
			player.ClearOverrideShader();
			if (player.characterIdentity == PlayableCharacters.Eevee)
			{
				player.GetComponent<CharacterAnimationRandomizer>().RemoveOverrideAnimLibrary(knightLibrary);
			}
			for (int j = 0; j < player.ownerlessStatModifiers.Count; j++)
			{
				if (player.ownerlessStatModifiers[j].statToBoost == PlayerStats.StatType.ReloadSpeed && player.ownerlessStatModifiers[j].amount == -1000f)
				{
					player.ownerlessStatModifiers.RemoveAt(j);
					break;
				}
			}
			player.stats.RecalculateStats(player);
		}
	}

	public void GainArmorOnLevelLoad()
	{
		m_player.healthHaver.Armor += 1f;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<BulletArmorItem>().m_pickedUpThisRun = true;
		GameManager.Instance.OnNewLevelFullyLoaded -= GainArmorOnLevelLoad;
		ProcessLegendaryStatus(player);
		m_player = null;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp && GameManager.HasInstance)
		{
			GameManager.Instance.OnNewLevelFullyLoaded -= GainArmorOnLevelLoad;
		}
		base.OnDestroy();
	}
}
