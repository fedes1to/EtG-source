using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class AdditionalBraveLight : BraveBehaviour
{
	public Color LightColor = Color.white;

	public float LightIntensity = 3f;

	public float LightRadius = 5f;

	public bool FadeOnActorDeath;

	[ShowInInspectorIf("FadeOnActorDeath", true)]
	public AIActor SpecifyActor;

	public bool TriggeredOnBulletBank;

	public bool UseProjectileCreatedEvent;

	public AIBulletBank RelevantBulletBank;

	public float BulletBankHoldTime;

	public float BulletBankFadeTime = 0.5f;

	public string BulletBankTransformName;

	public float BulletBankIntensity = 3f;

	public bool UsesCone;

	public float LightAngle = 180f;

	public float LightOrient;

	public bool UsesCustomMaterial;

	public Material CustomLightMaterial;

	private bool m_initialized;

	private Coroutine m_activeCoroutine;

	private bool isFading;

	private void Awake()
	{
		if (TriggeredOnBulletBank)
		{
			LightIntensity = 0f;
		}
	}

	public IEnumerator Start()
	{
		yield return null;
		Initialize();
	}

	public void Initialize()
	{
		if (m_initialized)
		{
			return;
		}
		if (TriggeredOnBulletBank && RelevantBulletBank != null)
		{
			if (UseProjectileCreatedEvent)
			{
				AIBulletBank relevantBulletBank = RelevantBulletBank;
				relevantBulletBank.OnProjectileCreatedWithSource = (Action<string, Projectile>)Delegate.Combine(relevantBulletBank.OnProjectileCreatedWithSource, new Action<string, Projectile>(HandleProjectileCreated));
			}
			else
			{
				RelevantBulletBank.OnBulletSpawned += HandleBulletSpawned;
			}
		}
		Pixelator.Instance.AdditionalBraveLights.Add(this);
		if (FadeOnActorDeath)
		{
			if (!SpecifyActor)
			{
				SpecifyActor = base.aiActor;
			}
			SpecifyActor.healthHaver.OnPreDeath += OnPreDeath;
		}
		m_initialized = true;
	}

	private void HandleProjectileCreated(string arg1, Projectile arg2)
	{
		if (arg1 == BulletBankTransformName)
		{
			if (m_activeCoroutine != null)
			{
				StopCoroutine(m_activeCoroutine);
			}
			m_activeCoroutine = StartCoroutine(HandleBulletBankFade());
		}
	}

	public void ManuallyDoBulletSpawnedFade()
	{
		if (m_activeCoroutine != null)
		{
			StopCoroutine(m_activeCoroutine);
		}
		m_activeCoroutine = StartCoroutine(HandleBulletBankFade());
	}

	public void EndEarly()
	{
		if (m_activeCoroutine != null)
		{
			isFading = true;
		}
	}

	private void HandleBulletSpawned(Bullet arg1, Projectile arg2)
	{
		if (arg1.RootTransform != null && BulletBankTransformName == arg1.RootTransform.name)
		{
			if (m_activeCoroutine != null)
			{
				StopCoroutine(m_activeCoroutine);
			}
			m_activeCoroutine = StartCoroutine(HandleBulletBankFade());
		}
	}

	private IEnumerator HandleBulletBankFade()
	{
		float elapsed2 = 0f;
		isFading = false;
		if (BulletBankHoldTime > 0f)
		{
			while (elapsed2 < BulletBankHoldTime && !isFading)
			{
				elapsed2 += BraveTime.DeltaTime;
				LightIntensity = BulletBankIntensity;
				yield return null;
			}
		}
		isFading = true;
		elapsed2 = 0f;
		while (elapsed2 < BulletBankFadeTime)
		{
			elapsed2 += BraveTime.DeltaTime;
			LightIntensity = Mathf.Lerp(BulletBankIntensity, 0f, elapsed2 / BulletBankFadeTime);
			yield return null;
		}
		isFading = false;
		LightIntensity = 0f;
		m_activeCoroutine = null;
	}

	protected override void OnDestroy()
	{
		if (Pixelator.HasInstance)
		{
			Pixelator.Instance.AdditionalBraveLights.Remove(this);
		}
		if (FadeOnActorDeath)
		{
			SpecifyActor.healthHaver.OnPreDeath -= OnPreDeath;
		}
		base.OnDestroy();
	}

	private void OnPreDeath(Vector2 deathDir)
	{
		tk2dSpriteAnimationClip deathClip = SpecifyActor.healthHaver.GetDeathClip(deathDir.ToAngle());
		StartCoroutine(FadeLight(deathClip.BaseClipLength));
	}

	private IEnumerator FadeLight(float fadeTime)
	{
		float timer = 0f;
		float startRadius = LightRadius;
		float startIntensity = LightIntensity;
		for (; timer < fadeTime; timer += BraveTime.DeltaTime)
		{
			yield return null;
			LightRadius = Mathf.Lerp(startRadius, 0f, timer / fadeTime);
			LightRadius = Mathf.Lerp(startIntensity, 1f, timer / fadeTime);
		}
	}
}
