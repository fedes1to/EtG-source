using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class RadialSlowInterface
{
	public float EffectRadius = 100f;

	public float RadialSlowInTime;

	public float RadialSlowHoldTime = 1f;

	public float RadialSlowOutTime = 0.5f;

	public float RadialSlowTimeModifier = 0.25f;

	public string audioEvent;

	public bool DoesSepia;

	public bool DoesCirclePass;

	public bool UpdatesForNewEnemies = true;

	public void DoRadialSlow(Vector2 centerPoint, RoomHandler targetRoom)
	{
		List<AIActor> activeEnemies = targetRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (!string.IsNullOrEmpty(audioEvent))
		{
			AkSoundEngine.PostEvent(audioEvent, GameManager.Instance.BestActivePlayer.gameObject);
		}
		if (activeEnemies != null && activeEnemies.Count > 0)
		{
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AIActor aIActor = activeEnemies[i];
				if ((bool)aIActor && aIActor.IsNormalEnemy && (bool)aIActor.healthHaver && !aIActor.IsGone)
				{
					aIActor.StartCoroutine(ProcessSlow(centerPoint, aIActor, 0f));
				}
			}
		}
		if (DoesSepia)
		{
			Pixelator.Instance.StartCoroutine(ProcessEnsepia());
		}
		if (DoesCirclePass)
		{
			Pixelator.Instance.StartCoroutine(ProcessCirclePass(centerPoint));
		}
		if (UpdatesForNewEnemies)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleNewEnemies(centerPoint, targetRoom));
		}
	}

	private IEnumerator HandleNewEnemies(Vector2 centerPoint, RoomHandler targetRoom)
	{
		float totalDuration = RadialSlowHoldTime + RadialSlowInTime + RadialSlowOutTime;
		float elapsed = 0f;
		Action<AIActor> enemyAdded = delegate(AIActor a)
		{
			if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver && !a.IsGone)
			{
				a.StartCoroutine(ProcessSlow(centerPoint, a, elapsed));
			}
		};
		targetRoom.OnEnemyRegistered = (Action<AIActor>)Delegate.Combine(targetRoom.OnEnemyRegistered, enemyAdded);
		while (elapsed < totalDuration)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		targetRoom.OnEnemyRegistered = (Action<AIActor>)Delegate.Remove(targetRoom.OnEnemyRegistered, enemyAdded);
	}

	private IEnumerator ProcessCirclePass(Vector2 centerPoint)
	{
		Material newPass = new Material(Shader.Find("Brave/Effects/PartialDesaturationEffect"));
		newPass.SetVector("_WorldCenter", new Vector4(centerPoint.x, centerPoint.y, 0f, 0f));
		Pixelator.Instance.RegisterAdditionalRenderPass(newPass);
		float elapsed3 = 0f;
		while (elapsed3 < RadialSlowInTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			newPass.SetFloat("_Radius", Mathf.Lerp(0f, EffectRadius, elapsed3 / RadialSlowInTime));
			yield return null;
		}
		elapsed3 = 0f;
		newPass.SetFloat("_Radius", EffectRadius);
		while (elapsed3 < RadialSlowHoldTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			yield return null;
		}
		elapsed3 = 0f;
		newPass.SetFloat("_Radius", EffectRadius);
		while (elapsed3 < RadialSlowOutTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			newPass.SetFloat("_Radius", Mathf.Lerp(EffectRadius, 0f, elapsed3 / RadialSlowOutTime));
			yield return null;
		}
		Pixelator.Instance.DeregisterAdditionalRenderPass(newPass);
	}

	private IEnumerator ProcessEnsepia()
	{
		float elapsed3 = 0f;
		while (elapsed3 < RadialSlowInTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			Pixelator.Instance.SetFreezeFramePower(elapsed3 / RadialSlowInTime);
			yield return null;
		}
		elapsed3 = 0f;
		Pixelator.Instance.SetFreezeFramePower(1f);
		while (elapsed3 < RadialSlowHoldTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			yield return null;
		}
		elapsed3 = 0f;
		while (elapsed3 < RadialSlowOutTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			Pixelator.Instance.SetFreezeFramePower(1f - elapsed3 / RadialSlowOutTime);
			yield return null;
		}
		Pixelator.Instance.SetFreezeFramePower(0f);
	}

	private IEnumerator ProcessSlow(Vector2 centerPoint, AIActor target, float startTime)
	{
		float elapsed3 = startTime;
		float sqrRadius = EffectRadius * EffectRadius;
		if (RadialSlowInTime > 0f)
		{
			while (elapsed3 < RadialSlowInTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				float t2 = elapsed3 / RadialSlowInTime;
				if ((target.CenterPosition - centerPoint).sqrMagnitude > sqrRadius)
				{
					t2 = 0f;
				}
				target.LocalTimeScale = Mathf.Lerp(1f, RadialSlowTimeModifier, t2);
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (RadialSlowHoldTime > 0f)
		{
			while (elapsed3 < RadialSlowHoldTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				float timeTarget = (target.LocalTimeScale = ((!((target.CenterPosition - centerPoint).sqrMagnitude > sqrRadius)) ? RadialSlowTimeModifier : 1f));
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (RadialSlowOutTime > 0f)
		{
			while (elapsed3 < RadialSlowOutTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				float t = elapsed3 / RadialSlowOutTime;
				if ((target.CenterPosition - centerPoint).sqrMagnitude > sqrRadius)
				{
					t = 1f;
				}
				target.LocalTimeScale = Mathf.Lerp(RadialSlowTimeModifier, 1f, t);
				yield return null;
			}
		}
		if ((bool)target)
		{
			target.LocalTimeScale = 1f;
		}
	}
}
