using System;
using UnityEngine;

public class ExplosionDebrisLauncher : BraveBehaviour
{
	public int minShards = 4;

	public int maxShards = 4;

	public float minExpulsionForce = 15f;

	public float maxExpulsionForce = 15f;

	public bool specifyArcDegrees;

	[ShowInInspectorIf("specifyArcDegrees", true)]
	public float arcDegrees;

	public float angleVariance = 20f;

	public DebrisObject[] debrisSources;

	public bool UsesCustomAxialVelocity;

	[ShowInInspectorIf("UsesCustomAxialVelocity", false)]
	public Vector3 CustomAxialVelocity = Vector3.zero;

	public AIActor SpecifyActor;

	public bool LaunchOnActorPreDeath;

	public bool LaunchOnActorDeath;

	public bool LaunchOnAnimationEvent;

	[ShowInInspectorIf("LaunchOnAnimationEvent", true)]
	public tk2dSpriteAnimator SpecifyAnimator;

	[ShowInInspectorIf("LaunchOnAnimationEvent", true)]
	public string EventName;

	[ShowInInspectorIf("LaunchOnAnimationEvent", true)]
	public bool UseDeathDir = true;

	private Vector2 m_deathDir;

	private void Start()
	{
		if (LaunchOnActorDeath)
		{
			if (!SpecifyActor)
			{
				SpecifyActor = base.aiActor;
			}
			SpecifyActor.healthHaver.OnDeath += OnDeath;
		}
		if (LaunchOnActorPreDeath || LaunchOnAnimationEvent)
		{
			if (!SpecifyActor)
			{
				SpecifyActor = base.aiActor;
			}
			SpecifyActor.healthHaver.OnPreDeath += OnPreDeath;
		}
		if (LaunchOnAnimationEvent)
		{
			if (!SpecifyAnimator)
			{
				SpecifyAnimator = base.spriteAnimator;
			}
			tk2dSpriteAnimator specifyAnimator = SpecifyAnimator;
			specifyAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(specifyAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnPreDeath(Vector2 deathDir)
	{
		m_deathDir = deathDir;
		if (LaunchOnActorPreDeath)
		{
			Launch(deathDir);
		}
	}

	private void OnDeath(Vector2 deathDir)
	{
		Launch(deathDir);
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (clip.GetFrame(frame).eventInfo == EventName)
		{
			Vector2 vector = Vector2.zero;
			if (!UseDeathDir)
			{
				vector = base.transform.position.XY() - SpecifyAnimator.aiActor.CenterPosition;
			}
			if (UseDeathDir || vector == Vector2.zero)
			{
				vector = m_deathDir;
			}
			Launch(vector);
		}
	}

	public void Launch()
	{
		int num = UnityEngine.Random.Range(minShards, maxShards + 1);
		float num2 = UnityEngine.Random.Range(0f, 360f);
		float num3 = 360f / (float)num;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = SpawnManager.SpawnDebris(debrisSources[UnityEngine.Random.Range(0, debrisSources.Length)].gameObject, base.transform.position, Quaternion.identity);
			DebrisObject component = gameObject.GetComponent<DebrisObject>();
			Vector3 vector = Quaternion.Euler(0f, 0f, num2 + num3 * (float)i + UnityEngine.Random.Range(0f - angleVariance, angleVariance)) * Vector3.right * UnityEngine.Random.Range(minExpulsionForce, maxExpulsionForce);
			vector = vector.WithZ(2f);
			if (UsesCustomAxialVelocity)
			{
				vector = Vector3.Scale(vector, CustomAxialVelocity);
			}
			component.Trigger(vector, 1f);
			component.additionalHeightBoost = -3f;
		}
	}

	public void Launch(Vector2 surfaceNormal)
	{
		int num = UnityEngine.Random.Range(minShards, maxShards + 1);
		if (num == 0)
		{
			return;
		}
		float num2 = surfaceNormal.ToAngle();
		float num3 = 0f;
		if (specifyArcDegrees)
		{
			num2 -= arcDegrees / 2f;
			num3 = arcDegrees / (float)(num - 1);
		}
		else if (num == 2)
		{
			num2 -= 45f;
			num3 = 90f;
		}
		else if (num > 2)
		{
			num2 -= 90f;
			num3 = 180f / (float)(num - 1);
		}
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = SpawnManager.SpawnDebris(debrisSources[UnityEngine.Random.Range(0, debrisSources.Length)].gameObject, base.transform.position, Quaternion.identity);
			DebrisObject component = gameObject.GetComponent<DebrisObject>();
			float value = num2 + num3 * (float)i + UnityEngine.Random.Range(0f - angleVariance, angleVariance);
			value = Mathf.Clamp(value, num2, num2 + 180f);
			Vector3 vector = Quaternion.Euler(0f, 0f, value) * Vector3.right * UnityEngine.Random.Range(minExpulsionForce, maxExpulsionForce);
			vector = vector.WithZ(UnityEngine.Random.Range(1.5f, 3f));
			if (UsesCustomAxialVelocity)
			{
				vector = Vector3.Scale(vector, CustomAxialVelocity);
			}
			component.Trigger(vector, UnityEngine.Random.Range(1f, 2f));
		}
	}
}
