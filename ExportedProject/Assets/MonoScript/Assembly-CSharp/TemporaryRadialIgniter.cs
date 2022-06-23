using System;
using Dungeonator;
using UnityEngine;

public class TemporaryRadialIgniter : MonoBehaviour
{
	public float Radius = 5f;

	public float Lifespan = 5f;

	public GameActorFireEffect igniteEffect;

	private bool m_radialIndicatorActive;

	private HeatIndicatorController m_radialIndicator;

	private Action<AIActor, float> AuraAction;

	private void Start()
	{
		HandleRadialIndicator();
		UnityEngine.Object.Destroy(base.gameObject, Lifespan);
	}

	private void Update()
	{
		DoAura();
	}

	protected virtual void DoAura()
	{
		if (AuraAction == null)
		{
			AuraAction = delegate(AIActor actor, float dist)
			{
				actor.ApplyEffect(igniteEffect);
			};
		}
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom != null)
		{
			if ((bool)m_radialIndicator)
			{
				m_radialIndicator.CurrentRadius = Radius;
			}
			absoluteRoom.ApplyActionToNearbyEnemies(base.transform.position, Radius, AuraAction);
		}
	}

	private void OnDestroy()
	{
		UnhandleRadialIndicator();
	}

	private void HandleRadialIndicator()
	{
		if (!m_radialIndicatorActive)
		{
			m_radialIndicatorActive = true;
			m_radialIndicator = ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), base.transform.position, Quaternion.identity, base.transform)).GetComponent<HeatIndicatorController>();
		}
	}

	private void UnhandleRadialIndicator()
	{
		if (m_radialIndicatorActive)
		{
			m_radialIndicatorActive = false;
			if ((bool)m_radialIndicator)
			{
				m_radialIndicator.EndEffect();
			}
			m_radialIndicator = null;
		}
	}
}
