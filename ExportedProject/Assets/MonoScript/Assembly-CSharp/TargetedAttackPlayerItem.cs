using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class TargetedAttackPlayerItem : PlayerItem
{
	public float minDistance = 1f;

	public float maxDistance = 15f;

	public GameObject reticleQuad;

	public bool doesGoop;

	public GoopDefinition goopDefinition;

	public float goopRadius = 3f;

	public bool doesStrike = true;

	public GameObject strikeVFX;

	public ExplosionData strikeExplosionData;

	public bool DoScreenFlash = true;

	public float FlashHoldtime = 0.1f;

	public float FlashFadetime = 0.5f;

	public bool TransmogrifySurvivors;

	public float TransmogrifyRadius = 15f;

	public float TransmogrifyChance = 0.5f;

	[EnemyIdentifier]
	public string TransmogrifyTargetGuid;

	private PlayerController m_currentUser;

	private tk2dBaseSprite m_extantReticleQuad;

	private float m_currentAngle;

	private float m_currentDistance = 5f;

	public override void Update()
	{
		base.Update();
		if (base.IsCurrentlyActive)
		{
			if ((bool)m_extantReticleQuad)
			{
				UpdateReticlePosition();
				return;
			}
			base.IsCurrentlyActive = false;
			ClearCooldowns();
		}
	}

	private void UpdateReticlePosition()
	{
		if (BraveInput.GetInstanceForPlayer(m_currentUser.PlayerIDX).IsKeyboardAndMouse())
		{
			Vector2 vector = m_currentUser.unadjustedAimPoint.XY();
			Vector2 vector2 = vector - m_extantReticleQuad.GetBounds().extents.XY();
			m_extantReticleQuad.transform.position = vector2;
			return;
		}
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(m_currentUser.PlayerIDX);
		Vector2 vector3 = m_currentUser.CenterPosition + (Quaternion.Euler(0f, 0f, m_currentAngle) * Vector2.right).XY() * m_currentDistance;
		vector3 += instanceForPlayer.ActiveActions.Aim.Vector * 8f * BraveTime.DeltaTime;
		m_currentAngle = BraveMathCollege.Atan2Degrees(vector3 - m_currentUser.CenterPosition);
		m_currentDistance = Vector2.Distance(vector3, m_currentUser.CenterPosition);
		m_currentDistance = Mathf.Min(m_currentDistance, maxDistance);
		vector3 = m_currentUser.CenterPosition + (Quaternion.Euler(0f, 0f, m_currentAngle) * Vector2.right).XY() * m_currentDistance;
		Vector2 vector4 = vector3 - m_extantReticleQuad.GetBounds().extents.XY();
		m_extantReticleQuad.transform.position = vector4;
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.OnPreDrop(user);
		if ((bool)m_extantReticleQuad)
		{
			Object.Destroy(m_extantReticleQuad.gameObject);
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		base.IsCurrentlyActive = true;
		m_currentUser = user;
		GameObject gameObject = Object.Instantiate(reticleQuad);
		m_extantReticleQuad = gameObject.GetComponent<tk2dBaseSprite>();
		m_currentAngle = BraveMathCollege.Atan2Degrees(m_currentUser.unadjustedAimPoint.XY() - m_currentUser.CenterPosition);
		m_currentDistance = 5f;
		UpdateReticlePosition();
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		Vector2 worldCenter = m_extantReticleQuad.WorldCenter;
		if ((bool)m_extantReticleQuad)
		{
			Object.Destroy(m_extantReticleQuad.gameObject);
		}
		base.IsCurrentlyActive = true;
		if (doesStrike)
		{
			DoStrike(worldCenter);
		}
		if (doesGoop)
		{
			HandleEngoopening(worldCenter, goopRadius);
		}
		if (itemName == "Nuke" && (bool)user && user.HasActiveBonusSynergy(CustomSynergyType.MELTDOWN))
		{
			user.CurrentGun.GainAmmo(100);
			AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", base.gameObject);
		}
		if (TransmogrifySurvivors && (bool)user && user.CurrentRoom != null)
		{
			List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				int count = activeEnemies.Count;
				for (int i = 0; i < count; i++)
				{
					if ((bool)activeEnemies[i] && activeEnemies[i].HasBeenEngaged && (bool)activeEnemies[i].healthHaver && activeEnemies[i].IsNormalEnemy && !activeEnemies[i].healthHaver.IsDead && !activeEnemies[i].healthHaver.IsBoss && !activeEnemies[i].IsTransmogrified && Random.value < TransmogrifyChance && Vector2.Distance(activeEnemies[i].CenterPosition, worldCenter) < TransmogrifyRadius)
					{
						activeEnemies[i].Transmogrify(EnemyDatabase.GetOrLoadByGuid(TransmogrifyTargetGuid), null);
					}
				}
			}
		}
		if (DoScreenFlash)
		{
			Pixelator.Instance.FadeToColor(FlashFadetime, Color.white, true, FlashHoldtime);
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(0.15f, 1f, false);
		}
		base.IsCurrentlyActive = false;
	}

	protected void HandleEngoopening(Vector2 startPoint, float radius)
	{
		float duration = 1f;
		DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
		goopManagerForGoopType.TimedAddGoopCircle(startPoint, radius, duration);
	}

	private void DoStrike(Vector2 currentTarget)
	{
		Exploder.Explode(currentTarget, strikeExplosionData, Vector2.zero);
	}

	protected override void OnDestroy()
	{
		if ((bool)m_extantReticleQuad)
		{
			Object.Destroy(m_extantReticleQuad.gameObject);
		}
		base.OnDestroy();
	}
}
