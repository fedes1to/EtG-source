using System.Collections.Generic;
using UnityEngine;

public class AkGameObjEnvironmentData
{
	private readonly List<AkEnvironment> activeEnvironments = new List<AkEnvironment>();

	private readonly List<AkEnvironment> activeEnvironmentsFromPortals = new List<AkEnvironment>();

	private readonly List<AkEnvironmentPortal> activePortals = new List<AkEnvironmentPortal>();

	private readonly AkAuxSendArray auxSendValues = new AkAuxSendArray();

	private Vector3 lastPosition = Vector3.zero;

	private bool hasEnvironmentListChanged = true;

	private bool hasActivePortalListChanged = true;

	private bool hasSentZero;

	private void AddHighestPriorityEnvironmentsFromPortals(Vector3 position)
	{
		for (int i = 0; i < activePortals.Count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				AkEnvironment akEnvironment = activePortals[i].environments[j];
				if (!(akEnvironment != null))
				{
					continue;
				}
				int num = activeEnvironmentsFromPortals.BinarySearch(akEnvironment, AkEnvironment.s_compareByPriority);
				if (num >= 0 && num < 4)
				{
					auxSendValues.Add(akEnvironment.GetAuxBusID(), activePortals[i].GetAuxSendValueForPosition(position, j));
					if (auxSendValues.isFull)
					{
						return;
					}
				}
			}
		}
	}

	private void AddHighestPriorityEnvironments(Vector3 position)
	{
		if (auxSendValues.isFull || auxSendValues.Count() >= activeEnvironments.Count)
		{
			return;
		}
		for (int i = 0; i < activeEnvironments.Count; i++)
		{
			AkEnvironment akEnvironment = activeEnvironments[i];
			uint auxBusID = akEnvironment.GetAuxBusID();
			if ((!akEnvironment.isDefault || i == 0) && !auxSendValues.Contains(auxBusID))
			{
				auxSendValues.Add(auxBusID, akEnvironment.GetAuxSendValueForPosition(position));
				if (akEnvironment.excludeOthers || auxSendValues.isFull)
				{
					break;
				}
			}
		}
	}

	public void UpdateAuxSend(GameObject gameObject, Vector3 position)
	{
		if (hasEnvironmentListChanged || hasActivePortalListChanged || !(lastPosition == position))
		{
			auxSendValues.Reset();
			AddHighestPriorityEnvironmentsFromPortals(position);
			AddHighestPriorityEnvironments(position);
			bool flag = auxSendValues.Count() == 0;
			if (!hasSentZero || !flag)
			{
				AkSoundEngine.SetEmitterAuxSendValues(gameObject, auxSendValues, (uint)auxSendValues.Count());
			}
			hasSentZero = flag;
			lastPosition = position;
			hasActivePortalListChanged = false;
			hasEnvironmentListChanged = false;
		}
	}

	private void TryAddEnvironment(AkEnvironment env)
	{
		if (!(env != null))
		{
			return;
		}
		int num = activeEnvironmentsFromPortals.BinarySearch(env, AkEnvironment.s_compareByPriority);
		if (num < 0)
		{
			activeEnvironmentsFromPortals.Insert(~num, env);
			num = activeEnvironments.BinarySearch(env, AkEnvironment.s_compareBySelectionAlgorithm);
			if (num < 0)
			{
				activeEnvironments.Insert(~num, env);
			}
			hasEnvironmentListChanged = true;
		}
	}

	private void RemoveEnvironment(AkEnvironment env)
	{
		activeEnvironmentsFromPortals.Remove(env);
		activeEnvironments.Remove(env);
		hasEnvironmentListChanged = true;
	}

	public void AddAkEnvironment(Collider environmentCollider, Collider gameObjectCollider)
	{
		AkEnvironmentPortal component = environmentCollider.GetComponent<AkEnvironmentPortal>();
		if (component != null)
		{
			activePortals.Add(component);
			hasActivePortalListChanged = true;
			for (int i = 0; i < 2; i++)
			{
				TryAddEnvironment(component.environments[i]);
			}
		}
		else
		{
			AkEnvironment component2 = environmentCollider.GetComponent<AkEnvironment>();
			TryAddEnvironment(component2);
		}
	}

	private bool AkEnvironmentBelongsToActivePortals(AkEnvironment env)
	{
		for (int i = 0; i < activePortals.Count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (env == activePortals[i].environments[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveAkEnvironment(Collider environmentCollider, Collider gameObjectCollider)
	{
		AkEnvironmentPortal component = environmentCollider.GetComponent<AkEnvironmentPortal>();
		if (component != null)
		{
			for (int i = 0; i < 2; i++)
			{
				AkEnvironment akEnvironment = component.environments[i];
				if (akEnvironment != null && !gameObjectCollider.bounds.Intersects(akEnvironment.GetCollider().bounds))
				{
					RemoveEnvironment(akEnvironment);
				}
			}
			activePortals.Remove(component);
			hasActivePortalListChanged = true;
		}
		else
		{
			AkEnvironment component2 = environmentCollider.GetComponent<AkEnvironment>();
			if (component2 != null && !AkEnvironmentBelongsToActivePortals(component2))
			{
				RemoveEnvironment(component2);
			}
		}
	}
}
