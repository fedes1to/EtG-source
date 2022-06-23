using System.Collections;
using UnityEngine;

public class CuccoController : CompanionController
{
	public int HitsRequired = 5;

	public float HitDecayTime = 5f;

	public int NumToSpawn = 20;

	public float SpawnDuration = 5f;

	public float InternalCooldown;

	public GameObject MicroCuccoPrefab;

	private float m_elapsed;

	private int m_numRecentHits;

	private float m_internalCooldown;

	private void Start()
	{
		base.healthHaver.OnDamaged += HandleDamaged;
	}

	public override void Update()
	{
		base.Update();
		m_elapsed += BraveTime.DeltaTime;
		m_internalCooldown = Mathf.Max(0f, m_internalCooldown - BraveTime.DeltaTime);
		if (m_elapsed > HitDecayTime)
		{
			if (m_numRecentHits > 0)
			{
				m_numRecentHits--;
			}
			m_elapsed -= HitDecayTime;
		}
	}

	private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		base.healthHaver.FullHeal();
		if (!(m_internalCooldown > 0f))
		{
			AkSoundEngine.PostEvent("Play_PET_chicken_cluck_01", base.gameObject);
			m_numRecentHits++;
			if (PassiveItem.IsFlagSetAtAll(typeof(BattleStandardItem)) || ((bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.IsLuteCompanionBuff))
			{
				m_numRecentHits++;
			}
			if (m_numRecentHits >= HitsRequired)
			{
				StartCoroutine(HandleAggro());
			}
		}
	}

	private IEnumerator HandleAggro()
	{
		m_internalCooldown = InternalCooldown;
		float elapsed = 0f;
		base.aiAnimator.PlayForDuration("angry", SpawnDuration);
		AkSoundEngine.PostEvent("Play_PET_chicken_summon_01", base.gameObject);
		float cuccoElapsed = 0f;
		while (elapsed < SpawnDuration)
		{
			elapsed += BraveTime.DeltaTime;
			cuccoElapsed += BraveTime.DeltaTime;
			if (cuccoElapsed > SpawnDuration / (float)NumToSpawn)
			{
				cuccoElapsed -= SpawnDuration / (float)NumToSpawn;
				Vector2 vector = GameManager.Instance.MainCameraController.transform.position.XY() + Random.insideUnitCircle.normalized * GameManager.Instance.MainCameraController.Camera.orthographicSize * 2f;
				GameObject gameObject = Object.Instantiate(MicroCuccoPrefab, vector, Quaternion.identity);
				gameObject.GetComponent<MicroCuccoController>().Initialize(m_owner);
			}
			yield return null;
		}
	}
}
