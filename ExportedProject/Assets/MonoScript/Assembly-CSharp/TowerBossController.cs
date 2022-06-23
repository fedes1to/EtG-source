using System;
using System.Collections;
using UnityEngine;

public class TowerBossController : DungeonPlaceableBehaviour
{
	public enum TowerBossPhase
	{
		PHASE_ONE,
		PHASE_TWO
	}

	public HealthHaver cockpitHealthHaver;

	public tk2dSprite towerSprite;

	public TowerBossIrisController irisLeft;

	public TowerBossIrisController irisRight;

	public TowerBossEmitterController[] laserEmitters;

	public Vector2 ellipseCenter;

	public Vector2 ellipseAxes;

	public Projectile beamProjectile;

	public float spinSpeed = 60f;

	private TowerBossBatteryController m_batteryLeft;

	private TowerBossBatteryController m_batteryRight;

	private bool m_alive = true;

	public TowerBossPhase currentPhase;

	private void Start()
	{
		TowerBossBatteryController[] array = (TowerBossBatteryController[])UnityEngine.Object.FindObjectsOfType(typeof(TowerBossBatteryController));
		BraveUtility.Assert(array.Length != 2, "Trying to initialize TowerBoss with more or less than 2 batteries in world.");
		if (array[0].transform.position.x < array[1].transform.position.x)
		{
			m_batteryLeft = array[0];
			m_batteryRight = array[1];
		}
		else
		{
			m_batteryLeft = array[1];
			m_batteryRight = array[0];
		}
		m_batteryLeft.tower = this;
		m_batteryRight.tower = this;
		m_batteryLeft.linkedIris = irisLeft;
		m_batteryRight.linkedIris = irisRight;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			MeshRenderer componentInChildren = child.GetComponentInChildren<MeshRenderer>();
			if (child.GetComponent<Renderer>() != null)
			{
				DepthLookupManager.PinRendererToRenderer(componentInChildren, towerSprite.GetComponent<MeshRenderer>());
			}
		}
		towerSprite.IsPerpendicular = false;
		cockpitHealthHaver.IsVulnerable = false;
		cockpitHealthHaver.OnDeath += Die;
		StartCoroutine(HandleBatteryCycle());
		StartCoroutine(HandleBeamCycle());
	}

	private void Die(Vector2 lastDirection)
	{
		m_alive = false;
	}

	private void Update()
	{
		if (m_alive)
		{
			for (int i = 0; i < laserEmitters.Length; i++)
			{
				float num = laserEmitters[i].currentAngle + spinSpeed * BraveTime.DeltaTime * ((float)Math.PI / 180f);
				Vector3 vector = base.transform.position.XY() + ellipseCenter;
				float x = vector.x + ellipseAxes.x * Mathf.Cos(num);
				float y = vector.y + ellipseAxes.y * Mathf.Sin(num);
				laserEmitters[i].transform.position = BraveUtility.QuantizeVector(new Vector3(x, y, laserEmitters[i].transform.position.z));
				laserEmitters[i].UpdateAngle(num % ((float)Math.PI * 2f));
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private IEnumerator HandleBeamCycle()
	{
		while (m_alive)
		{
			yield return new WaitForSeconds(5f);
			for (int i = 0; i < laserEmitters.Length; i++)
			{
			}
			yield return new WaitForSeconds(5f);
		}
	}

	private IEnumerator HandleBatteryCycle()
	{
		float left_elapsed = 0f;
		float right_elapsed = 0f;
		float cycleLength = m_batteryLeft.cycleTime;
		m_batteryLeft.IsVulnerable = true;
		while (m_alive)
		{
			if (m_batteryLeft.linkedIris.fuseAlive && !m_batteryLeft.linkedIris.IsOpen)
			{
				left_elapsed += BraveTime.DeltaTime;
				if (left_elapsed > cycleLength)
				{
					left_elapsed -= cycleLength;
					m_batteryLeft.IsVulnerable = !m_batteryLeft.IsVulnerable;
				}
			}
			else
			{
				m_batteryLeft.IsVulnerable = false;
			}
			if (m_batteryRight.linkedIris.fuseAlive && !m_batteryRight.linkedIris.IsOpen)
			{
				right_elapsed += BraveTime.DeltaTime;
				if (right_elapsed > cycleLength)
				{
					right_elapsed -= cycleLength;
					m_batteryRight.IsVulnerable = !m_batteryRight.IsVulnerable;
				}
			}
			else
			{
				m_batteryRight.IsVulnerable = false;
			}
			yield return null;
		}
	}

	private void PhaseTransition()
	{
		m_batteryLeft.linkedIris = irisRight;
		m_batteryRight.linkedIris = irisLeft;
		irisLeft.fuseAlive = true;
		irisRight.fuseAlive = true;
		currentPhase = TowerBossPhase.PHASE_TWO;
	}

	public void NotifyFuseDestruction(TowerBossIrisController source)
	{
		if (!irisLeft.fuseAlive && !irisRight.fuseAlive)
		{
			cockpitHealthHaver.IsVulnerable = true;
			GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor)).HandleRoomAction(RoomEventTriggerAction.UNSEAL_ROOM);
		}
	}
}
