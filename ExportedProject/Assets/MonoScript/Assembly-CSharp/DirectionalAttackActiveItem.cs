using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalAttackActiveItem : PlayerItem
{
	public float initialWidth = 3f;

	public float finalWidth = 3f;

	public float startDistance = 1f;

	public float attackLength = 10f;

	public GameObject reticleQuad;

	public bool doesGoop;

	public GoopDefinition goopDefinition;

	public bool doesBarrage;

	public int BarrageColumns = 1;

	public GameObject barrageVFX;

	public ExplosionData barrageExplosionData;

	public float barrageRadius = 1.5f;

	public float delayBetweenStrikes = 0.25f;

	public bool SkipTargetingStep;

	public string AudioEvent;

	private PlayerController m_currentUser;

	private tk2dSlicedSprite m_extantReticleQuad;

	private bool m_airstrikeSynergyProcessed;

	private bool m_isDoingBarrage;

	public override void Update()
	{
		base.Update();
		if ((bool)m_currentUser && m_currentUser.HasActiveBonusSynergy(CustomSynergyType.EXPLOSIVE_AIRSTRIKE) && itemName == "Airstrike" && !m_airstrikeSynergyProcessed)
		{
			m_airstrikeSynergyProcessed = true;
			BarrageColumns = 3;
			initialWidth *= 3f;
			finalWidth *= 3f;
		}
		else if ((bool)m_currentUser && !m_currentUser.HasActiveBonusSynergy(CustomSynergyType.EXPLOSIVE_AIRSTRIKE) && m_airstrikeSynergyProcessed)
		{
			m_airstrikeSynergyProcessed = false;
			BarrageColumns = 1;
			initialWidth /= 3f;
			finalWidth /= 3f;
		}
		if (base.IsCurrentlyActive && (bool)m_extantReticleQuad)
		{
			Vector2 centerPosition = m_currentUser.CenterPosition;
			Vector2 normalized = (m_currentUser.unadjustedAimPoint.XY() - centerPosition).normalized;
			m_extantReticleQuad.transform.localRotation = Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(normalized));
			Vector2 vector = centerPosition + normalized * startDistance + (Quaternion.Euler(0f, 0f, -90f) * normalized * (initialWidth / 2f)).XY();
			m_extantReticleQuad.transform.position = vector;
		}
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
		Vector2 centerPosition = user.CenterPosition;
		Vector2 normalized = (user.unadjustedAimPoint.XY() - centerPosition).normalized;
		if ((bool)m_currentUser && m_currentUser.HasActiveBonusSynergy(CustomSynergyType.EXPLOSIVE_AIRSTRIKE) && itemName == "Airstrike" && !m_airstrikeSynergyProcessed)
		{
			m_airstrikeSynergyProcessed = true;
			BarrageColumns = 3;
			initialWidth *= 3f;
			finalWidth *= 3f;
		}
		else if ((bool)m_currentUser && !m_currentUser.HasActiveBonusSynergy(CustomSynergyType.EXPLOSIVE_AIRSTRIKE) && m_airstrikeSynergyProcessed)
		{
			m_airstrikeSynergyProcessed = false;
			BarrageColumns = 1;
			initialWidth /= 3f;
			finalWidth /= 3f;
		}
		if (SkipTargetingStep)
		{
			DoActiveEffect(user);
			return;
		}
		GameObject gameObject = Object.Instantiate(reticleQuad);
		m_extantReticleQuad = gameObject.GetComponent<tk2dSlicedSprite>();
		m_extantReticleQuad.dimensions = new Vector2(attackLength * 16f, initialWidth * 16f);
		if (normalized != Vector2.zero)
		{
			m_extantReticleQuad.transform.localRotation = Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(normalized));
		}
		Vector2 vector = centerPosition + normalized * startDistance + (Quaternion.Euler(0f, 0f, -90f) * normalized * (initialWidth / 2f)).XY();
		m_extantReticleQuad.transform.position = vector;
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		if (!m_isDoingBarrage)
		{
			if ((bool)m_extantReticleQuad)
			{
				Object.Destroy(m_extantReticleQuad.gameObject);
			}
			Vector2 centerPosition = user.CenterPosition;
			Vector2 normalized = (user.unadjustedAimPoint.XY() - centerPosition).normalized;
			centerPosition += normalized * startDistance;
			if (doesGoop)
			{
				HandleEngoopening(centerPosition, normalized);
			}
			if (doesBarrage)
			{
				List<Vector2> targets = AcquireBarrageTargets(centerPosition, normalized);
				user.StartCoroutine(HandleBarrage(targets));
			}
			else
			{
				base.IsCurrentlyActive = false;
			}
			if (!string.IsNullOrEmpty(AudioEvent))
			{
				AkSoundEngine.PostEvent(AudioEvent, base.gameObject);
			}
		}
	}

	protected void HandleEngoopening(Vector2 startPoint, Vector2 direction)
	{
		float duration = 1f;
		DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
		goopManagerForGoopType.TimedAddGoopLine(startPoint, startPoint + direction * attackLength, barrageRadius, duration);
	}

	private IEnumerator HandleBarrage(List<Vector2> targets)
	{
		m_isDoingBarrage = true;
		while (targets.Count > 0)
		{
			Vector2 currentTarget = targets[0];
			targets.RemoveAt(0);
			Exploder.Explode(currentTarget, barrageExplosionData, Vector2.zero);
			yield return new WaitForSeconds(delayBetweenStrikes / (float)BarrageColumns);
		}
		yield return new WaitForSeconds(0.25f);
		m_isDoingBarrage = false;
		base.IsCurrentlyActive = false;
	}

	protected List<Vector2> AcquireBarrageTargets(Vector2 startPoint, Vector2 direction)
	{
		List<Vector2> list = new List<Vector2>();
		float num = (0f - barrageRadius) / 2f;
		float z = BraveMathCollege.Atan2Degrees(direction);
		Quaternion quaternion = Quaternion.Euler(0f, 0f, z);
		for (; num < attackLength; num += barrageRadius)
		{
			float t = Mathf.Clamp01(num / attackLength);
			float num2 = Mathf.Lerp(initialWidth, finalWidth, t);
			float x = Mathf.Clamp(num, 0f, attackLength);
			for (int i = 0; i < BarrageColumns; i++)
			{
				float num3 = Mathf.Lerp(0f - num2, num2, ((float)i + 1f) / ((float)BarrageColumns + 1f));
				float num4 = Random.Range((0f - num2) / (4f * (float)BarrageColumns), num2 / (4f * (float)BarrageColumns));
				Vector2 vector = new Vector2(x, num3 + num4);
				Vector2 vector2 = (quaternion * vector).XY();
				list.Add(startPoint + vector2);
			}
		}
		return list;
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
