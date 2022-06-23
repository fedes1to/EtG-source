using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkPassiveItem : PassiveItem
{
	public bool ModifiesDodgeRoll;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public float DodgeRollTimeMultiplier = 0.9f;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public float DodgeRollDistanceMultiplier = 1.25f;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public int AdditionalInvulnerabilityFrames;

	public ScarfAttachmentDoer ScarfPrefab;

	public GameObject BlinkpoofVfx;

	private ScarfAttachmentDoer m_scarf;

	private AfterImageTrailController afterimage;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (player.IsDodgeRolling)
			{
				player.ForceStopDodgeRoll();
			}
			if ((bool)ScarfPrefab)
			{
				m_scarf = UnityEngine.Object.Instantiate(ScarfPrefab.gameObject).GetComponent<ScarfAttachmentDoer>();
				m_scarf.Initialize(player);
			}
			if (ModifiesDodgeRoll)
			{
				player.rollStats.rollDistanceMultiplier *= DodgeRollDistanceMultiplier;
				player.rollStats.rollTimeMultiplier *= DodgeRollTimeMultiplier;
				player.rollStats.additionalInvulnerabilityFrames += AdditionalInvulnerabilityFrames;
			}
			if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
			{
				PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
			}
			if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
			}
			afterimage = player.gameObject.AddComponent<AfterImageTrailController>();
			afterimage.spawnShadows = false;
			afterimage.shadowTimeDelay = 0.05f;
			afterimage.shadowLifetime = 0.3f;
			afterimage.minTranslation = 0.05f;
			afterimage.dashColor = Color.black;
			afterimage.maxEmission = 0f;
			afterimage.minEmission = 0f;
			afterimage.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
			player.OnRollStarted += OnRollStarted;
			player.OnBlinkShadowCreated = (Action<tk2dSprite>)Delegate.Combine(player.OnBlinkShadowCreated, new Action<tk2dSprite>(OnBlinkCloneCreated));
			base.Pickup(player);
		}
	}

	public void OnBlinkCloneCreated(tk2dSprite cloneSprite)
	{
		SpawnManager.SpawnVFX(BlinkpoofVfx, cloneSprite.WorldCenter, Quaternion.identity);
	}

	private void OnRollStarted(PlayerController obj, Vector2 dirVec)
	{
		if (!GameManager.Instance.Dungeon || !GameManager.Instance.Dungeon.IsEndTimes)
		{
			obj.StartCoroutine(HandleAfterImageStop(obj));
		}
	}

	private IEnumerator HandleAfterImageStop(PlayerController player)
	{
		player.PlayEffectOnActor(BlinkpoofVfx, Vector3.zero, false, true);
		AkSoundEngine.PostEvent("Play_CHR_ninja_dash_01", base.gameObject);
		if (!player.IsDodgeRolling)
		{
			yield return null;
		}
		else
		{
			afterimage.spawnShadows = true;
			while (player.IsDodgeRolling)
			{
				yield return null;
			}
			if ((bool)afterimage)
			{
				afterimage.spawnShadows = false;
			}
		}
		player.PlayEffectOnActor(BlinkpoofVfx, Vector3.zero, false, true);
		AkSoundEngine.PostEvent("Play_CHR_ninja_dash_01", base.gameObject);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
			player.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
			player.rollStats.additionalInvulnerabilityFrames -= AdditionalInvulnerabilityFrames;
			player.rollStats.additionalInvulnerabilityFrames = Mathf.Max(player.rollStats.additionalInvulnerabilityFrames, 0);
		}
		if (PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		if ((bool)m_scarf)
		{
			UnityEngine.Object.Destroy(m_scarf.gameObject);
			m_scarf = null;
		}
		player.OnRollStarted -= OnRollStarted;
		player.OnBlinkShadowCreated = (Action<tk2dSprite>)Delegate.Remove(player.OnBlinkShadowCreated, new Action<tk2dSprite>(OnBlinkCloneCreated));
		if ((bool)afterimage)
		{
			UnityEngine.Object.Destroy(afterimage);
		}
		afterimage = null;
		debrisObject.GetComponent<BlinkPassiveItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_scarf)
		{
			UnityEngine.Object.Destroy(m_scarf.gameObject);
			m_scarf = null;
		}
		if (m_pickedUp && (bool)m_owner && PassiveItem.ActiveFlagItems != null && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= OnRollStarted;
			PlayerController owner = m_owner;
			owner.OnBlinkShadowCreated = (Action<tk2dSprite>)Delegate.Remove(owner.OnBlinkShadowCreated, new Action<tk2dSprite>(OnBlinkCloneCreated));
			if ((bool)afterimage)
			{
				UnityEngine.Object.Destroy(afterimage);
			}
			afterimage = null;
		}
		base.OnDestroy();
	}
}
