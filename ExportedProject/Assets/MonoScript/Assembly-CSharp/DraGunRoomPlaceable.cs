using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DraGunRoomPlaceable : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public static int HallHeight = 18;

	public float roomEmbers = 100f;

	public float pitEmbers = 300f;

	public float nearDeathPitEmbers = 600f;

	public float idlePitEmbers = 100f;

	public float transitioningEmbers = 500f;

	private RoomHandler m_room;

	private DraGunController m_dragunController;

	private MovingPlatform m_deathBridge;

	private Vector2 m_pitMin;

	private Vector2 m_pitMax;

	private Vector2 m_roomMin;

	private Vector2 m_roomMax;

	private Vector2 m_bodyMin;

	private Vector2 m_bodyMax;

	public bool UseInvariantTime { get; set; }

	public bool DraGunKilled { get; set; }

	public IEnumerator Start()
	{
		yield return null;
		m_dragunController = m_room.GetComponentsAbsoluteInRoom<DraGunController>()[0];
		m_deathBridge = GetComponentInChildren<MovingPlatform>();
		FindPitBounds();
	}

	public void Update()
	{
		if (!m_dragunController && !DraGunKilled)
		{
			List<DraGunController> componentsAbsoluteInRoom = m_room.GetComponentsAbsoluteInRoom<DraGunController>();
			if (componentsAbsoluteInRoom.Count > 0)
			{
				m_dragunController = m_room.GetComponentsAbsoluteInRoom<DraGunController>()[0];
			}
		}
		if ((bool)m_dragunController && !m_dragunController.HasDoneIntro && GameManager.Instance.PrimaryPlayer.CurrentRoom == m_room)
		{
			float t = (GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter.y - m_roomMin.y) / (float)HallHeight;
			GameManager.Instance.MainCameraController.OverrideZoomScale = Mathf.Lerp(1f, 0.75f, t);
		}
		if (!GameManager.Instance.IsLoadingLevel && GameManager.Instance.IsAnyPlayerInRoom(m_room) && GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			float num = ((!UseInvariantTime) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
			float num2 = ((!m_dragunController || !m_dragunController.healthHaver.IsAlive) ? idlePitEmbers : ((!m_dragunController.IsNearDeath) ? pitEmbers : nearDeathPitEmbers));
			float num3 = 1f;
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.MEDIUM)
			{
				num3 = 0.25f;
			}
			GlobalSparksDoer.DoRandomParticleBurst((int)(num2 * num * num3), m_pitMin.ToVector3ZUp(100f), m_pitMax.ToVector3ZUp(100f), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			if ((bool)m_dragunController && m_dragunController.healthHaver.IsAlive)
			{
				GlobalSparksDoer.DoRandomParticleBurst((int)(roomEmbers * num * num3), m_roomMin.ToVector3ZisY(), m_roomMax.ToVector3ZisY(), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			}
			if ((bool)m_dragunController && m_dragunController.IsTransitioning)
			{
				GlobalSparksDoer.DoRandomParticleBurst((int)(transitioningEmbers * num * num3), m_bodyMin.ToVector3ZUp(m_bodyMin.y - 5f), m_bodyMax.ToVector3ZUp(m_bodyMin.y - 5f), Vector3.up * 1.5f, 180f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			}
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH && (bool)GlobalSparksDoer.EmberParticles)
			{
				GlobalSparksDoer.EmberParticles.maxParticles = 10000;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ExtendDeathBridge()
	{
		m_deathBridge.specRigidbody.enabled = true;
		m_deathBridge.specRigidbody.Initialize();
		m_deathBridge.spriteAnimator.Play();
		m_deathBridge.MarkCells();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
	}

	private void FindPitBounds()
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		m_pitMin = intVector.ToVector2() + new Vector2(0f, 14f);
		m_pitMax = intVector.ToVector2() + new Vector2(36f, 29f);
		m_roomMin = m_room.area.UnitBottomLeft;
		m_roomMax = m_room.area.UnitTopRight;
		m_bodyMin = m_pitMin + new Vector2(15f, 0f);
		m_bodyMax = m_pitMin + new Vector2(21f, 15f);
	}
}
