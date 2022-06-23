using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class TrapSpawnerController : BraveBehaviour
{
	[Header("Spawn Info")]
	public GameObject Trap;

	public GameObject PoofVfx;

	public List<Vector2> RoomPositionOffsets;

	public List<float> SpawnDelays;

	public Vector2 VfxOffset;

	public float VfxLeadTime;

	public float AdditionalTriggerDelayTime;

	[Header("Spawn Triggers")]
	public bool SpawnOnIntroFinished;

	[Header("Destroy Triggers")]
	public bool DestroyOnDeath;

	private RoomHandler m_room;

	private List<GameObject> m_traps = new List<GameObject>();

	public void Start()
	{
		m_room = GetComponent<AIActor>().ParentRoom;
		if (SpawnOnIntroFinished)
		{
			GenericIntroDoer component = GetComponent<GenericIntroDoer>();
			component.OnIntroFinished = (Action)Delegate.Combine(component.OnIntroFinished, new Action(OnIntroFinished));
		}
		if (DestroyOnDeath)
		{
			base.healthHaver.OnDeath += OnDeath;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnIntroFinished()
	{
		if (SpawnOnIntroFinished)
		{
			StartCoroutine(SpawnTraps());
		}
	}

	private void OnDeath(Vector2 vector2)
	{
		if (DestroyOnDeath)
		{
			DestroyTraps();
		}
	}

	private IEnumerator SpawnTraps()
	{
		for (int i = 0; i < RoomPositionOffsets.Count; i++)
		{
			if (i < SpawnDelays.Count && SpawnDelays[i] > 0f)
			{
				yield return new WaitForSeconds(SpawnDelays[i]);
			}
			Vector2 pos = m_room.area.UnitBottomLeft + RoomPositionOffsets[i];
			StartCoroutine(SpawnTrap(pos));
		}
	}

	private IEnumerator SpawnTrap(Vector2 pos)
	{
		if ((bool)PoofVfx)
		{
			SpawnManager.SpawnVFX(PoofVfx, pos + VfxOffset, Quaternion.identity);
		}
		if (VfxLeadTime > 0f)
		{
			yield return new WaitForSeconds(VfxLeadTime);
		}
		GameObject trap = UnityEngine.Object.Instantiate(Trap, pos, Quaternion.identity);
		if (AdditionalTriggerDelayTime > 0f)
		{
			BasicTrapController component = trap.GetComponent<BasicTrapController>();
			if ((bool)component)
			{
				component.TemporarilyDisableTrap(AdditionalTriggerDelayTime);
			}
		}
		m_traps.Add(trap);
	}

	private void DestroyTraps()
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach)
		{
			for (int i = 0; i < m_traps.Count; i++)
			{
				GameManager.Instance.StartCoroutine(DestroyTrap(m_traps[i]));
			}
		}
	}

	private IEnumerator DestroyTrap(GameObject trap)
	{
		if ((bool)PoofVfx)
		{
			SpawnManager.SpawnVFX(PoofVfx, trap.transform.position.XY() + VfxOffset, Quaternion.identity);
		}
		if (VfxLeadTime > 0f)
		{
			yield return new WaitForSeconds(VfxLeadTime);
		}
		UnityEngine.Object.Destroy(trap);
	}
}
