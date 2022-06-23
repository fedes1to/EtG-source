using System;
using System.Collections;
using UnityEngine;

public class GunnerGunController : MonoBehaviour
{
	public float ChanceToSkull = 1f;

	public GameObject SkullPrefab;

	public float Lifespan = 4f;

	public int AmmoLossOnDamage;

	public float ChanceToTriggerSynergy = 0.25f;

	private Gun m_gun;

	private bool m_initialized;

	private PlayerController m_player;

	private GameObject m_extantSkull;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void HandleReceivedDamage(PlayerController p)
	{
		float num = 0f;
		bool flag = (bool)p.CurrentGun && p.CurrentGun == m_gun;
		if (flag)
		{
			num = ChanceToSkull;
		}
		if ((bool)p && p.HasActiveBonusSynergy(CustomSynergyType.WHALE_OF_A_TIME))
		{
			num = ChanceToTriggerSynergy;
		}
		if (UnityEngine.Random.value < num && !m_extantSkull && (!flag || (m_gun.ammo >= AmmoLossOnDamage && m_gun.ammo > 0)))
		{
			if (flag)
			{
				m_gun.LoseAmmo(AmmoLossOnDamage);
			}
			m_extantSkull = SpawnManager.SpawnDebris(SkullPrefab, p.CenterPosition.ToVector3ZisY(), Quaternion.identity);
			DebrisObject component = m_extantSkull.GetComponent<DebrisObject>();
			component.FlagAsPickup();
			component.Trigger((UnityEngine.Random.insideUnitCircle.normalized * 20f).ToVector3ZUp(3f), 1f, 0f);
			SpeculativeRigidbody component2 = m_extantSkull.GetComponent<SpeculativeRigidbody>();
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(component2);
			component2.RegisterTemporaryCollisionException(p.specRigidbody, 0.25f);
			component2.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component2.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleSkullTrigger));
			component2.StartCoroutine(HandleLifespan(m_extantSkull));
		}
		else if ((bool)m_extantSkull)
		{
			LootEngine.DoDefaultPurplePoof(m_extantSkull.transform.position + new Vector3(0.75f, 0.5f, 0f));
			UnityEngine.Object.Destroy(m_extantSkull.gameObject);
			m_extantSkull = null;
		}
	}

	private IEnumerator HandleLifespan(GameObject source)
	{
		float elapsed = 0f;
		while (elapsed < Lifespan)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if ((bool)m_extantSkull && (bool)source && m_extantSkull == source)
		{
			LootEngine.DoDefaultPurplePoof(m_extantSkull.transform.position + new Vector3(0.75f, 0.5f, 0f));
			UnityEngine.Object.Destroy(m_extantSkull.gameObject);
			m_extantSkull = null;
		}
	}

	private void HandleSkullTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!specRigidbody)
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if (!component || ((!component.CurrentGun || !(component.CurrentGun == m_gun)) && !component.HasActiveBonusSynergy(CustomSynergyType.WHALE_OF_A_TIME)))
		{
			return;
		}
		sourceSpecRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Remove(sourceSpecRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleSkullTrigger));
		tk2dSpriteAnimator component2 = m_extantSkull.GetComponent<tk2dSpriteAnimator>();
		component2.PlayAndDestroyObject("gonner_skull_pickup_vfx");
		m_extantSkull = null;
		if (component.characterIdentity == PlayableCharacters.Robot)
		{
			component.healthHaver.Armor = component.healthHaver.Armor + 1f;
			AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
			GameObject gameObject = BraveResources.Load<GameObject>("Global VFX/VFX_Healing_Sparkles_001");
			if (gameObject != null)
			{
				component.PlayEffectOnActor(gameObject, Vector3.zero);
			}
		}
		else
		{
			component.healthHaver.ApplyHealing(0.5f);
			AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
			GameObject gameObject2 = BraveResources.Load<GameObject>("Global VFX/VFX_Healing_Sparkles_001");
			if (gameObject2 != null)
			{
				component.PlayEffectOnActor(gameObject2, Vector3.zero);
			}
		}
	}

	private void Update()
	{
		if (m_initialized && (!m_gun.CurrentOwner || m_gun.CurrentOwner.CurrentGun != m_gun))
		{
			Disengage();
		}
		else if (!m_initialized && (bool)m_gun.CurrentOwner && m_gun.CurrentOwner.CurrentGun == m_gun)
		{
			Engage();
		}
	}

	private void OnDestroy()
	{
		Disengage();
	}

	private void Engage()
	{
		m_initialized = true;
		m_player = m_gun.CurrentOwner as PlayerController;
		m_player.OnReceivedDamage += HandleReceivedDamage;
	}

	private void Disengage()
	{
		if ((bool)m_player)
		{
			m_player.OnReceivedDamage -= HandleReceivedDamage;
		}
		if ((bool)m_extantSkull)
		{
			LootEngine.DoDefaultPurplePoof(m_extantSkull.transform.position + new Vector3(0.75f, 0.5f, 0f));
			UnityEngine.Object.Destroy(m_extantSkull.gameObject);
			m_extantSkull = null;
		}
		m_player = null;
		m_initialized = false;
	}
}
