using System.Collections.Generic;
using UnityEngine;

public class FireSubBeamSynergyProcessor : MonoBehaviour
{
	public enum SubBeamMode
	{
		FROM_BEAM,
		FROM_PROJECTILE_CENTER
	}

	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public SubBeamMode Mode;

	public Projectile SubBeamProjectile;

	public int NumberBeams = 3;

	public float BeamAngle = 90f;

	private BasicBeamController m_beam;

	private Projectile m_projectile;

	public float FromProjectileDamageModifier = 0.5f;

	private List<SubbeamData> m_subbeams = new List<SubbeamData>();

	public void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		m_beam = GetComponent<BasicBeamController>();
	}

	public void Update()
	{
		bool flag = true;
		if (Mode == SubBeamMode.FROM_BEAM)
		{
			if (!(m_beam.Owner is PlayerController) || !(m_beam.Owner as PlayerController).HasActiveBonusSynergy(SynergyToCheck))
			{
				return;
			}
			flag = m_beam.State == BasicBeamController.BeamState.Firing;
		}
		else if (!(m_projectile.Owner is PlayerController) || !(m_projectile.Owner as PlayerController).HasActiveBonusSynergy(SynergyToCheck))
		{
			return;
		}
		float num = ((!m_beam) ? 0f : Vector2.Distance(m_beam.GetPointOnBeam(0f), m_beam.GetPointOnBeam(1f)));
		if (flag && (Mode != 0 || num > 1.5f))
		{
			if (m_subbeams.Count > 0)
			{
				for (int i = 0; i < m_subbeams.Count; i++)
				{
					Vector2 direction;
					if (Mode == SubBeamMode.FROM_BEAM)
					{
						m_subbeams[i].subbeam.Origin = m_beam.GetPointOnBeam(m_subbeams[i].percent);
						direction = m_beam.Direction;
					}
					else
					{
						m_subbeams[i].subbeam.Origin = m_projectile.specRigidbody.UnitCenter;
						direction = m_projectile.Direction;
					}
					m_subbeams[i].subbeam.Direction = Quaternion.Euler(0f, 0f, m_subbeams[i].angle) * direction;
					m_subbeams[i].subbeam.LateUpdatePosition(m_subbeams[i].subbeam.Origin);
				}
				return;
			}
			for (int j = 0; j < NumberBeams; j++)
			{
				SubbeamData subbeamData = new SubbeamData();
				float num2 = 1f / (float)(NumberBeams + 1) * (float)(j + 1);
				float num3 = BeamAngle;
				if (Mode == SubBeamMode.FROM_PROJECTILE_CENTER)
				{
					num3 = 360f / (float)NumberBeams * (float)j;
				}
				Vector2 pos = ((Mode != 0) ? m_projectile.specRigidbody.UnitCenter : m_beam.GetPointOnBeam(num2));
				Vector2 vector = ((Mode != 0) ? m_projectile.Direction : m_beam.Direction);
				subbeamData.subbeam = CreateSubBeam(SubBeamProjectile, pos, Quaternion.Euler(0f, 0f, num3) * vector);
				subbeamData.angle = num3;
				subbeamData.percent = num2;
				m_subbeams.Add(subbeamData);
				if (Mode == SubBeamMode.FROM_BEAM)
				{
					SubbeamData subbeamData2 = new SubbeamData();
					num3 = 0f - BeamAngle;
					subbeamData2.subbeam = CreateSubBeam(SubBeamProjectile, m_beam.GetPointOnBeam(num2), Quaternion.Euler(0f, 0f, num3) * m_beam.Direction);
					subbeamData2.percent = num2;
					subbeamData2.angle = num3;
					m_subbeams.Add(subbeamData2);
				}
			}
			if ((bool)m_projectile && (bool)m_projectile.sprite)
			{
				m_projectile.sprite.ForceRotationRebuild();
			}
		}
		else if (m_subbeams.Count > 0)
		{
			int num4;
			for (num4 = 0; num4 < m_subbeams.Count; num4++)
			{
				m_subbeams[num4].subbeam.CeaseAttack();
				m_subbeams.RemoveAt(num4);
				num4--;
			}
		}
	}

	private void OnDestroy()
	{
		if (m_subbeams.Count > 0)
		{
			int num;
			for (num = 0; num < m_subbeams.Count; num++)
			{
				m_subbeams[num].subbeam.CeaseAttack();
				m_subbeams.RemoveAt(num);
				num--;
			}
		}
	}

	private BeamController CreateSubBeam(Projectile subBeamProjectilePrefab, Vector2 pos, Vector2 dir)
	{
		BeamController component = subBeamProjectilePrefab.GetComponent<BeamController>();
		if (component is BasicBeamController)
		{
			GameObject gameObject = Object.Instantiate(subBeamProjectilePrefab.gameObject);
			gameObject.name = base.gameObject.name + " (Subbeam)";
			BasicBeamController component2 = gameObject.GetComponent<BasicBeamController>();
			component2.State = BasicBeamController.BeamState.Firing;
			component2.HitsPlayers = false;
			component2.HitsEnemies = true;
			component2.Origin = pos;
			component2.Direction = dir;
			component2.usesChargeDelay = false;
			component2.muzzleAnimation = string.Empty;
			component2.chargeAnimation = string.Empty;
			component2.beamStartAnimation = string.Empty;
			component2.projectile.Owner = m_projectile.Owner;
			if (Mode == SubBeamMode.FROM_BEAM)
			{
				component2.Owner = m_beam.Owner;
				component2.Gun = m_beam.Gun;
				component2.DamageModifier = m_beam.DamageModifier;
				component2.playerStatsModified = m_beam.playerStatsModified;
			}
			else
			{
				component2.Owner = m_projectile.Owner;
				component2.Gun = m_projectile.PossibleSourceGun;
				component2.DamageModifier = FromProjectileDamageModifier;
			}
			component2.HeightOffset = -0.25f;
			return component2;
		}
		if (component is RaidenBeamController)
		{
			GameObject gameObject2 = Object.Instantiate(subBeamProjectilePrefab.gameObject);
			gameObject2.name = base.gameObject.name + " (Subbeam)";
			RaidenBeamController component3 = gameObject2.GetComponent<RaidenBeamController>();
			component3.SelectRandomTarget = true;
			component3.HitsPlayers = false;
			component3.HitsEnemies = true;
			component3.Origin = pos;
			component3.Direction = dir;
			component3.usesChargeDelay = false;
			component3.projectile.Owner = m_projectile.Owner;
			if (Mode == SubBeamMode.FROM_BEAM)
			{
				component3.Owner = m_beam.Owner;
				component3.Gun = m_beam.Gun;
				component3.DamageModifier = m_beam.DamageModifier;
			}
			else
			{
				component3.Owner = m_projectile.Owner;
				component3.Gun = m_projectile.PossibleSourceGun;
				component3.DamageModifier = FromProjectileDamageModifier;
			}
			return component3;
		}
		return null;
	}
}
