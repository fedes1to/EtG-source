using System;
using System.Collections.Generic;
using UnityEngine;

public class MetronomeItem : PassiveItem
{
	public float damageBoostPerKill = 0.05f;

	public float damageBoostPerKillSynergy = 0.04f;

	public float damageMultiplierCap = 3f;

	public float synergyMultiplierCap = 5f;

	public tk2dSprite eighthNoteSprite;

	public tk2dSprite doubleEighthNoteSprite;

	public Gradient colorGradient;

	public Gradient synergyColorGradient;

	[NonSerialized]
	private Gun m_cachedGunReference;

	[NonSerialized]
	private int m_sequentialKills;

	[NonSerialized]
	private PlayerController m_player;

	private float ModifiedBoost
	{
		get
		{
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.KEEPING_THE_BEAT))
			{
				return damageBoostPerKillSynergy;
			}
			return damageBoostPerKill;
		}
	}

	private float ModifiedCap
	{
		get
		{
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.KEEPING_THE_BEAT))
			{
				return synergyMultiplierCap;
			}
			return damageMultiplierCap;
		}
	}

	private Gradient ModifiedGradient
	{
		get
		{
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.KEEPING_THE_BEAT))
			{
				return synergyColorGradient;
			}
			return colorGradient;
		}
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		if (m_cachedGunReference != null)
		{
			data.Add(m_cachedGunReference.PickupObjectId);
			data.Add(m_sequentialKills);
		}
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (!m_player || m_player.inventory == null || data.Count != 2)
		{
			return;
		}
		m_sequentialKills = (int)data[1];
		int num = (int)data[0];
		for (int i = 0; i < m_player.inventory.AllGuns.Count; i++)
		{
			if ((bool)m_player.inventory.AllGuns[i] && m_player.inventory.AllGuns[i].PickupObjectId == num)
			{
				m_cachedGunReference = m_player.inventory.AllGuns[i];
			}
		}
	}

	public float GetCurrentMultiplier()
	{
		return Mathf.Clamp(1f + (float)m_sequentialKills * ModifiedBoost, 0f, ModifiedCap);
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			m_player = player;
			player.OnKilledEnemy += OnKilledEnemy;
			player.GunChanged += OnGunChanged;
			player.healthHaver.OnDamaged += OnReceivedDamage;
			m_cachedGunReference = player.CurrentGun;
		}
	}

	private void OnGunChanged(Gun old, Gun current, bool newGun)
	{
		bool flag = false;
		if ((bool)m_player && m_player.CharacterUsesRandomGuns)
		{
			flag = true;
		}
		bool flag2 = false;
		if ((bool)m_player && m_player.inventory != null && m_player.inventory.GunChangeForgiveness)
		{
			flag2 = true;
		}
		if (old != current && !newGun && !flag && !flag2)
		{
			DoMetronomeBroken(current);
		}
		m_cachedGunReference = current;
	}

	private void DoMetronomeUp()
	{
		m_sequentialKills++;
		m_player.stats.RecalculateStats(m_player);
		AkSoundEngine.SetRTPCValue("Pitch_Metronome", m_sequentialKills);
		AkSoundEngine.PostEvent("Play_OBJ_metronome_jingle_01", m_player.gameObject);
		float currentMultiplier = GetCurrentMultiplier();
		float time = Mathf.InverseLerp(1f, ModifiedCap, currentMultiplier);
		Color tintColor = ModifiedGradient.Evaluate(time);
		if (currentMultiplier >= 2f)
		{
			m_player.BloopItemAboveHead(doubleEighthNoteSprite, string.Empty, tintColor);
		}
		else
		{
			m_player.BloopItemAboveHead(eighthNoteSprite, string.Empty, tintColor);
		}
	}

	private void DoMetronomeBroken(Gun current)
	{
		float currentMultiplier = GetCurrentMultiplier();
		if (currentMultiplier > 1f)
		{
			AkSoundEngine.PostEvent("Play_OBJ_metronome_fail_01", m_player.gameObject);
			float time = Mathf.InverseLerp(1f, ModifiedCap, currentMultiplier);
			Color color = ModifiedGradient.Evaluate(time);
			GameObject gameObject = m_player.PlayEffectOnActor((!(currentMultiplier >= 2f)) ? eighthNoteSprite.gameObject : doubleEighthNoteSprite.gameObject, Vector3.up * 1.5f);
			gameObject.GetComponent<tk2dBaseSprite>().color = color;
		}
		AkSoundEngine.SetRTPCValue("Pitch_Metronome", 0f);
		m_sequentialKills = 0;
		m_cachedGunReference = current;
		m_player.stats.RecalculateStats(m_player);
	}

	private void OnReceivedDamage(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		DoMetronomeBroken(m_cachedGunReference);
	}

	private void OnKilledEnemy(PlayerController source)
	{
		if (source.CurrentGun != m_cachedGunReference)
		{
			DoMetronomeBroken(source.CurrentGun);
		}
		DoMetronomeUp();
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.OnKilledEnemy -= OnKilledEnemy;
		player.GunChanged -= OnGunChanged;
		player.healthHaver.OnDamaged -= OnReceivedDamage;
		debrisObject.GetComponent<MetronomeItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			m_owner.OnKilledEnemy -= OnKilledEnemy;
			m_owner.GunChanged -= OnGunChanged;
			m_owner.healthHaver.OnDamaged -= OnReceivedDamage;
		}
		base.OnDestroy();
	}
}
