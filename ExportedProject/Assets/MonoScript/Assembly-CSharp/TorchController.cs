using System;
using Dungeonator;
using UnityEngine;

public class TorchController : BraveBehaviour
{
	[Header("VFX")]
	public VFXPool sparkVfx;

	public VFXPool douseVfx;

	public Transform douseOffset;

	[Header("Animations")]
	public tk2dSpriteAnimator flameAnimator;

	public string flameAnim;

	public string dousedAnim;

	public Renderer[] renderers;

	[Header("Other")]
	public bool canBeRelit = true;

	public bool anyProjectileBreaks;

	public bool disappearAfterDouse;

	public bool igniteGoop = true;

	[ShowInInspectorIf("igniteGoop", false)]
	public float igniteRadius = 0.5f;

	public bool deployGoop;

	[ShowInInspectorIf("deployGoop", false)]
	public GoopDefinition goopToDeploy;

	[ShowInInspectorIf("deployGoop", false)]
	public float goopRadius = 3f;

	public bool createsShardsOnDouse;

	public ShardCluster[] douseShards;

	private bool m_isLit = true;

	public CellData Cell { get; set; }

	public void Start()
	{
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
		}
		if (base.sprite.FlipX)
		{
			douseOffset.transform.localPosition = douseOffset.transform.localPosition.Scale(-1f, 1f, 1f);
		}
		if ((bool)base.specRigidbody)
		{
			RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
			if (absoluteRoom.IsWinchesterArcadeRoom)
			{
				base.specRigidbody.enabled = false;
			}
		}
	}

	protected override void OnDestroy()
	{
		if (base.specRigidbody != null)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
		}
		base.OnDestroy();
	}

	public void BeamCollision(Projectile p)
	{
		AnyCollision(p);
	}

	private void AnyCollision(Projectile p)
	{
		if (m_isLit && ((p.damageTypes & CoreDamageTypes.Water) == CoreDamageTypes.Water || anyProjectileBreaks))
		{
			m_isLit = false;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].enabled = false;
				base.visibilityManager.AddIgnoredRenderer(renderers[i]);
			}
			if (Cell != null && Cell.cellVisualData.lightObject != null)
			{
				Cell.cellVisualData.lightObject.SetActive(false);
			}
			Vector3 vector = ((!douseOffset) ? ((Vector3)base.specRigidbody.UnitCenter) : douseOffset.transform.position);
			VFXPool vFXPool = douseVfx;
			Vector3 position = vector;
			tk2dBaseSprite spriteParent = base.sprite;
			vFXPool.SpawnAtPosition(position, 0f, null, null, null, null, true, null, spriteParent);
			if (createsShardsOnDouse)
			{
				for (int j = 0; j < douseShards.Length; j++)
				{
					douseShards[j].SpawnShards(douseOffset.position + (p.LastVelocity * -1f).normalized.ToVector3ZUp(), p.LastVelocity * -1f, -30f, 30f, 0f, 0.5f, 1f, null);
				}
			}
			if (disappearAfterDouse)
			{
				if (!canBeRelit)
				{
					if (!string.IsNullOrEmpty(dousedAnim))
					{
						flameAnimator.PlayAndDestroyObject(dousedAnim);
					}
					else
					{
						UnityEngine.Object.Destroy(flameAnimator.gameObject);
					}
				}
				else if (!string.IsNullOrEmpty(dousedAnim))
				{
					flameAnimator.PlayAndDisableRenderer(dousedAnim);
				}
				else
				{
					flameAnimator.GetComponent<Renderer>().enabled = false;
				}
			}
			else if (!string.IsNullOrEmpty(dousedAnim))
			{
				flameAnimator.Play(dousedAnim);
			}
		}
		else if (!m_isLit && canBeRelit && (p.damageTypes & CoreDamageTypes.Fire) == CoreDamageTypes.Fire)
		{
			m_isLit = true;
			for (int k = 0; k < renderers.Length; k++)
			{
				renderers[k].enabled = true;
				base.visibilityManager.RemoveIgnoredRenderer(renderers[k]);
			}
			if (Cell != null && Cell.cellVisualData.lightObject != null)
			{
				Cell.cellVisualData.lightObject.SetActive(true);
			}
			douseVfx.DestroyAll();
			flameAnimator.GetComponent<Renderer>().enabled = true;
			flameAnimator.Play(flameAnim);
		}
	}

	private void OnEnterTrigger(SpeculativeRigidbody mySpecRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!collisionData.OtherRigidbody.projectile)
		{
			return;
		}
		if (m_isLit)
		{
			sparkVfx.SpawnAtPosition(base.specRigidbody.UnitCenter);
			if (deployGoop)
			{
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopToDeploy).TimedAddGoopCircle(base.specRigidbody.UnitBottomCenter, goopRadius);
			}
			if (igniteGoop)
			{
				for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
				{
					Vector2 center = new Vector2(base.specRigidbody.UnitCenter.x, base.transform.position.y);
					StaticReferenceManager.AllGoops[i].IgniteGoopCircle(center, igniteRadius);
				}
			}
		}
		AnyCollision(collisionData.OtherRigidbody.projectile);
	}
}
