using System;
using System.Collections;
using UnityEngine;

public class ElectricMushroom : DungeonPlaceableBehaviour
{
	public string[] ValidIdleAnims;

	public string[] ValidHitAnims;

	public float MinEmissive = 10f;

	public float MaxEmissive = 30f;

	public float PulsesPerSecond = 1f;

	public float DamageToEnemies = 6f;

	public tk2dSpriteAnimator ElectrifyVFX;

	private int AnimIndex = -1;

	private int EmissivePowerID = -1;

	private static bool m_updatedEmissiveThisFrame;

	private bool m_isFiring;

	private float m_fireCooldownTime;

	private float m_remainingFireTime;

	private void Awake()
	{
		EmissivePowerID = Shader.PropertyToID("_EmissivePower");
		AnimIndex = UnityEngine.Random.Range(0, ValidIdleAnims.Length);
		IntVector2 intVector = base.transform.position.IntXY();
		if (!StaticReferenceManager.MushroomMap.ContainsKey(intVector))
		{
			StaticReferenceManager.MushroomMap.Add(intVector, this);
		}
		else
		{
			Debug.LogError("Duplicate mushroom at: " + intVector);
		}
	}

	private IEnumerator Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEnter));
		base.sprite.SetSprite(base.spriteAnimator.GetClipByName(ValidIdleAnims[AnimIndex]).frames[0].spriteId);
		yield return new WaitForSeconds(UnityEngine.Random.value);
		base.sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
	}

	private void Update()
	{
		if (!m_updatedEmissiveThisFrame)
		{
			m_updatedEmissiveThisFrame = true;
			base.sprite.renderer.sharedMaterial.SetFloat(EmissivePowerID, Mathf.Lerp(MinEmissive, MaxEmissive, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.realtimeSinceStartup * PulsesPerSecond, 1f))));
		}
	}

	private void LateUpdate()
	{
		m_updatedEmissiveThisFrame = false;
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.MushroomMap.Remove(base.transform.position.IntXY());
		base.OnDestroy();
	}

	private void TriggerNearby()
	{
		IntVector2 intVector = base.transform.position.IntXY();
		for (int i = 0; i < 4; i++)
		{
			if (StaticReferenceManager.MushroomMap.ContainsKey(intVector + IntVector2.Cardinals[i]))
			{
				StaticReferenceManager.MushroomMap[intVector + IntVector2.Cardinals[i]].Trigger();
			}
		}
	}

	public void Trigger(bool IsPrimaryTarget = false)
	{
		if (m_fireCooldownTime > 0f)
		{
			if (IsPrimaryTarget)
			{
				StartCoroutine(FrameDelayedBreak());
			}
		}
		else
		{
			StartCoroutine(Trigger_CR(IsPrimaryTarget));
		}
	}

	public IEnumerator Trigger_CR(bool IsPrimaryTarget = false)
	{
		m_fireCooldownTime = 0.5f;
		m_remainingFireTime = 2f;
		if (!IsPrimaryTarget)
		{
			base.spriteAnimator.PlayForDuration(ValidIdleAnims[AnimIndex], 2f);
		}
		Electrify();
		if (!m_isFiring)
		{
			StartCoroutine(HandleFiring());
		}
		yield return new WaitForSeconds(0.1f);
		TriggerNearby();
		if (IsPrimaryTarget)
		{
			StartCoroutine(FrameDelayedBreak());
		}
	}

	private IEnumerator FrameDelayedBreak()
	{
		yield return null;
		base.minorBreakable.Break();
	}

	private IEnumerator HandleFiring()
	{
		m_isFiring = true;
		while (m_remainingFireTime > 0f)
		{
			m_fireCooldownTime -= BraveTime.DeltaTime;
			m_remainingFireTime -= BraveTime.DeltaTime;
			yield return null;
		}
		m_isFiring = false;
		m_fireCooldownTime = 0f;
		m_remainingFireTime = 0f;
		base.spriteAnimator.StopAndResetFrame();
	}

	public void Electrify()
	{
		ElectrifyVFX.renderer.enabled = true;
		ElectrifyVFX.PlayAndDisableRenderer(string.Empty);
		for (int i = 0; i < base.specRigidbody.PrimaryPixelCollider.TriggerCollisions.Count; i++)
		{
			TriggerCollisionData triggerCollisionData = base.specRigidbody.PrimaryPixelCollider.TriggerCollisions[i];
			if (!(triggerCollisionData.SpecRigidbody.gameActor != null) || triggerCollisionData.SpecRigidbody.gameActor.IsFlying)
			{
				continue;
			}
			if (triggerCollisionData.SpecRigidbody.gameActor is AIActor)
			{
				if ((bool)triggerCollisionData.SpecRigidbody.healthHaver)
				{
					triggerCollisionData.SpecRigidbody.healthHaver.ApplyDamage(DamageToEnemies, Vector2.zero, StringTableManager.GetEnemiesString("#MUSHROOM"), CoreDamageTypes.Electric, DamageCategory.Environment);
				}
			}
			else if ((bool)triggerCollisionData.SpecRigidbody.healthHaver)
			{
				triggerCollisionData.SpecRigidbody.healthHaver.ApplyDamage(0.5f, Vector2.zero, StringTableManager.GetEnemiesString("#MUSHROOM"), CoreDamageTypes.Electric, DamageCategory.Environment);
			}
		}
	}

	private void HandleTriggerEnter(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (specRigidbody.gameActor != null && !specRigidbody.gameActor.IsFlying && ((bool)specRigidbody.spriteAnimator || !specRigidbody.spriteAnimator.QueryGroundedFrame()))
		{
			base.spriteAnimator.PlayForDuration(ValidHitAnims[AnimIndex], -1f, ValidIdleAnims[AnimIndex]);
			Trigger(true);
		}
	}
}
