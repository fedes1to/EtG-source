using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

[Serializable]
public class HeatRingModule
{
	private HeatIndicatorController m_indicator;

	public bool IsActive
	{
		get
		{
			return m_indicator;
		}
	}

	public void Trigger(float Radius, float Duration, GameActorFireEffect HeatEffect, tk2dBaseSprite sprite)
	{
		if (!m_indicator)
		{
			sprite.StartCoroutine(HandleHeatEffectsCR(Radius, Duration, HeatEffect, sprite));
		}
	}

	private IEnumerator HandleHeatEffectsCR(float Radius, float Duration, GameActorFireEffect HeatEffect, tk2dBaseSprite sprite)
	{
		HandleRadialIndicator(Radius, sprite);
		float elapsed = 0f;
		RoomHandler r = sprite.transform.position.GetAbsoluteRoom();
		Vector3 tableCenter = sprite.WorldCenter.ToVector3ZisY();
		Action<AIActor, float> AuraAction = delegate(AIActor actor, float dist)
		{
			actor.ApplyEffect(HeatEffect);
		};
		while (elapsed < Duration)
		{
			elapsed += BraveTime.DeltaTime;
			r.ApplyActionToNearbyEnemies(tableCenter.XY(), Radius, AuraAction);
			yield return null;
		}
		UnhandleRadialIndicator();
	}

	private void HandleRadialIndicator(float Radius, tk2dBaseSprite sprite)
	{
		if (!m_indicator)
		{
			Vector3 position = sprite.WorldCenter.ToVector3ZisY();
			m_indicator = ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), position, Quaternion.identity, sprite.transform)).GetComponent<HeatIndicatorController>();
			m_indicator.CurrentRadius = Radius;
		}
	}

	private void UnhandleRadialIndicator()
	{
		if ((bool)m_indicator)
		{
			m_indicator.EndEffect();
			m_indicator = null;
		}
	}
}
