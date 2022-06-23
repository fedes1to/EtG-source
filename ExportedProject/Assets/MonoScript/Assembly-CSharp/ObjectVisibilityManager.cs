using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ObjectVisibilityManager : BraveBehaviour
{
	public RoomHandler parentRoom;

	private RoomHandler.VisibilityStatus currentVisibility;

	private List<Component> m_renderers;

	private bool m_initialized;

	private GameObject m_object;

	private List<Renderer> m_ignoredRenderers = new List<Renderer>();

	public bool SuppressPlayerEnteredRoom;

	public Action OnToggleRenderers;

	private bool m_activatingLight;

	private void Start()
	{
		if (!m_initialized)
		{
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
			if (roomHandler == null)
			{
				roomHandler = GameManager.Instance.Dungeon.data[base.transform.position.IntXY()].nearestRoom;
			}
			Initialize(roomHandler);
		}
	}

	public void Initialize(RoomHandler room, bool allowEngagement = false)
	{
		if (room == null || !this || !base.gameObject)
		{
			Debug.LogWarning("Failing to initialize OVM!");
			return;
		}
		m_initialized = true;
		parentRoom = room;
		currentVisibility = room.visibility;
		parentRoom.BecameVisible += HandleParentRoomEntered;
		parentRoom.BecameInvisible += HandleParentRoomExited;
		m_object = base.gameObject;
		ChangeToVisibility(currentVisibility, allowEngagement);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (parentRoom != null)
		{
			parentRoom.BecameVisible -= HandleParentRoomEntered;
			parentRoom.BecameInvisible -= HandleParentRoomExited;
		}
	}

	public void ResetRenderersList()
	{
		m_renderers.Clear();
	}

	private void AcquireRenderers()
	{
		m_renderers = new List<Component>();
		m_renderers.AddRange(m_object.GetComponentsInChildren<ParticleSystem>());
		m_renderers.AddRange(m_object.GetComponentsInChildren<AIActor>());
		m_renderers.AddRange(m_object.GetComponentsInChildren<MeshRenderer>());
		m_renderers.AddRange(m_object.GetComponentsInChildren<Light>());
	}

	private void ToggleRenderers(bool simpleEnabled, RoomHandler.VisibilityStatus visibilityStatus, bool allowEngagement)
	{
		for (int i = 0; i < m_renderers.Count; i++)
		{
			bool flag = simpleEnabled;
			Component component = m_renderers[i];
			if (!component)
			{
				continue;
			}
			if (component is Renderer)
			{
				Renderer renderer = component as Renderer;
				if (!m_ignoredRenderers.Contains(renderer) && renderer.enabled != flag)
				{
					renderer.enabled = flag;
				}
			}
			else if (component is Light)
			{
				flag = visibilityStatus == RoomHandler.VisibilityStatus.CURRENT;
				if (!base.gameObject.activeSelf)
				{
					continue;
				}
				Light light = component as Light;
				if (light.enabled != flag)
				{
					if (flag)
					{
						StartCoroutine(ActivateLight(light));
					}
					else
					{
						StartCoroutine(DeactivateLight(light));
					}
				}
			}
			else if (component is ParticleSystem)
			{
				ParticleSystem particleSystem = component as ParticleSystem;
				particleSystem.GetComponent<Renderer>().enabled = flag;
			}
			else if (component is AIActor)
			{
				AIActor aIActor = component as AIActor;
				aIActor.enabled = flag;
				if (allowEngagement && flag && base.gameObject.activeSelf && !aIActor.healthHaver.IsBoss)
				{
					aIActor.HasBeenEngaged = true;
				}
			}
			else if (component is Behaviour)
			{
				Behaviour behaviour = component as Behaviour;
				if (!behaviour || behaviour.enabled != flag)
				{
					behaviour.enabled = flag;
				}
			}
		}
		if ((bool)base.aiShooter)
		{
			base.aiShooter.UpdateGunRenderers();
			base.aiShooter.UpdateHandRenderers();
		}
		if (OnToggleRenderers != null)
		{
			OnToggleRenderers();
		}
	}

	public void ChangeToVisibility(RoomHandler.VisibilityStatus status, bool allowEngagement = true)
	{
		if (!this)
		{
			return;
		}
		if (m_renderers == null || m_renderers.Count == 0)
		{
			AcquireRenderers();
		}
		if (m_renderers == null || m_renderers.Count == 0)
		{
			BraveUtility.Log("Expensive visibility management on unmanaged object...", Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
			return;
		}
		currentVisibility = status;
		bool simpleEnabled = false;
		switch (currentVisibility)
		{
		case RoomHandler.VisibilityStatus.CURRENT:
			simpleEnabled = true;
			break;
		case RoomHandler.VisibilityStatus.OBSCURED:
			simpleEnabled = false;
			break;
		case RoomHandler.VisibilityStatus.VISITED:
			simpleEnabled = true;
			break;
		case RoomHandler.VisibilityStatus.REOBSCURED:
			simpleEnabled = false;
			break;
		}
		ToggleRenderers(simpleEnabled, status, allowEngagement);
	}

	private IEnumerator DeactivateLight(Light l)
	{
		while (m_activatingLight)
		{
			yield return null;
		}
		m_activatingLight = true;
		float startIntensity = l.intensity;
		float elapsed = 0f;
		while (elapsed < 0.5f)
		{
			elapsed += BraveTime.DeltaTime;
			l.intensity = Mathf.Lerp(startIntensity, 0f, Mathf.Pow(elapsed / 0.5f, 2f));
			yield return null;
		}
		l.enabled = false;
		l.intensity = startIntensity;
		m_activatingLight = false;
	}

	private IEnumerator ActivateLight(Light l)
	{
		while (m_activatingLight)
		{
			yield return null;
		}
		m_activatingLight = true;
		float targetIntensity = l.intensity;
		l.intensity = 0f;
		l.enabled = true;
		float elapsed = 0f;
		while (elapsed < 0.5f)
		{
			elapsed += BraveTime.DeltaTime;
			l.intensity = Mathf.Lerp(0f, targetIntensity, Mathf.Pow(elapsed / 0.5f, 2f));
			yield return null;
		}
		l.intensity = targetIntensity;
		m_activatingLight = false;
	}

	private void HandleParentRoomEntered(float delay)
	{
		if (!SuppressPlayerEnteredRoom)
		{
			ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		}
	}

	private IEnumerator DelayedBecameVisible(float delay)
	{
		yield return new WaitForSeconds(delay);
		ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
	}

	private void HandleParentRoomExited()
	{
		ChangeToVisibility(RoomHandler.VisibilityStatus.VISITED);
	}

	public void AddIgnoredRenderer(Renderer renderer)
	{
		if (!m_ignoredRenderers.Contains(renderer))
		{
			m_ignoredRenderers.Add(renderer);
		}
	}

	public void RemoveIgnoredRenderer(Renderer renderer)
	{
		m_ignoredRenderers.Remove(renderer);
	}
}
