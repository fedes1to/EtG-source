using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class BounceProjModifier : BraveBehaviour
{
	public int numberOfBounces = 1;

	public float chanceToDieOnBounce;

	public float percentVelocityToLoseOnBounce;

	public float damageMultiplierOnBounce = 1f;

	public bool usesAdditionalScreenShake;

	[ShowInInspectorIf("usesAdditionalScreenShake", false)]
	public ScreenShakeSettings additionalScreenShake;

	public bool useLayerLimit;

	[ShowInInspectorIf("useLayerLimit", false)]
	public CollisionLayer layerLimit;

	public Action<BounceProjModifier, SpeculativeRigidbody> OnBounceContext;

	public bool ExplodeOnEnemyBounce;

	[FormerlySerializedAs("removeBulletMlControl")]
	public bool removeBulletScriptControl = true;

	public bool suppressHitEffectsOnBounce;

	public bool onlyBounceOffTiles;

	public bool bouncesTrackEnemies;

	public float bounceTrackRadius = 5f;

	public float TrackEnemyChance = 1f;

	private AIActor m_lastSmartBounceTarget;

	private int m_cachedNumberOfBounces;

	private Vector2 m_lastBouncePos;

	public event Action OnBounce;

	public void OnSpawned()
	{
		m_cachedNumberOfBounces = numberOfBounces;
		m_lastBouncePos = Vector2.zero;
	}

	public void OnDespawned()
	{
		numberOfBounces = m_cachedNumberOfBounces;
	}

	public Vector2 AdjustBounceVector(Projectile source, Vector2 inVec, SpeculativeRigidbody hitRigidbody)
	{
		Vector2 result = inVec;
		if (bouncesTrackEnemies && UnityEngine.Random.value < TrackEnemyChance)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(source.specRigidbody.UnitCenter.ToIntVector2());
			Vector2 unitCenter = source.specRigidbody.UnitCenter;
			Vector2 w = unitCenter + inVec.normalized * 50f;
			List<AIActor> activeEnemies = absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			float num = bounceTrackRadius * bounceTrackRadius;
			AIActor aIActor = null;
			float num2 = float.MaxValue;
			if (activeEnemies != null)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if ((bool)activeEnemies[i] && !(activeEnemies[i] == m_lastSmartBounceTarget) && !(activeEnemies[i].specRigidbody == hitRigidbody))
					{
						Vector2 vector = BraveMathCollege.ClosestPointOnLineSegment(activeEnemies[i].CenterPosition, unitCenter, w);
						float num3 = Vector2.SqrMagnitude(activeEnemies[i].CenterPosition - vector);
						if (num3 < num && num3 < num2)
						{
							num2 = num3;
							aIActor = activeEnemies[i];
						}
					}
				}
			}
			m_lastSmartBounceTarget = aIActor;
			if (aIActor != null)
			{
				Vector2 centerPosition = aIActor.CenterPosition;
				result = (centerPosition - unitCenter).normalized * inVec.magnitude;
			}
		}
		return result;
	}

	public bool FarFromPreviousBounce(Vector2 pos)
	{
		return Vector2.Distance(pos, m_lastBouncePos) > 0.25f;
	}

	public void Bounce(Projectile p, Vector2 bouncePos, SpeculativeRigidbody otherRigidbody = null)
	{
		m_lastBouncePos = bouncePos;
		Bounce(p, otherRigidbody);
	}

	public void Bounce(Projectile p, SpeculativeRigidbody otherRigidbody = null)
	{
		if ((bool)this && (bool)p)
		{
			PierceProjModifier pierceProjModifier = null;
			if ((bool)otherRigidbody && (bool)otherRigidbody.projectile)
			{
				pierceProjModifier = otherRigidbody.GetComponent<PierceProjModifier>();
			}
			if ((bool)pierceProjModifier && pierceProjModifier.BeastModeLevel == PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE)
			{
				numberOfBounces -= 2;
			}
			else
			{
				numberOfBounces--;
			}
			p.baseData.damage *= damageMultiplierOnBounce;
			if (usesAdditionalScreenShake)
			{
				GameManager.Instance.MainCameraController.DoScreenShake(additionalScreenShake, p.specRigidbody.UnitCenter);
			}
			if (numberOfBounces <= 0 && (bool)p.TrailRendererController)
			{
				p.TrailRendererController.Stop();
			}
			if (this.OnBounce != null)
			{
				this.OnBounce();
			}
			if (OnBounceContext != null)
			{
				OnBounceContext(this, otherRigidbody);
			}
		}
	}

	public void HandleChanceToDie()
	{
		if (chanceToDieOnBounce > 0f && UnityEngine.Random.value < chanceToDieOnBounce)
		{
			numberOfBounces = 0;
		}
	}
}
