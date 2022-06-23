using System;
using System.Collections;
using UnityEngine;

public class TankTreaderController : BraveBehaviour
{
	public GameObject mainGun;

	public float backTurretOffset = 30f;

	public tk2dSpriteAnimator guy;

	public tk2dSpriteAnimator hatch;

	public GameObject hatchPopObject;

	public VFXPool hatchPopVfx;

	public float hatchFlyTime = 1f;

	public Vector2 hatchFlySpeed = new Vector3(8f, 20f);

	private TankTreaderMiniTurretController[] m_miniTurrets;

	private ParticleSystem[] m_exhaustParticleSystems;

	private int m_exhaustFrameCount;

	private bool m_hasPoppedHatch;

	public void Start()
	{
		m_miniTurrets = GetComponentsInChildren<TankTreaderMiniTurretController>();
		m_exhaustParticleSystems = GetComponentsInChildren<ParticleSystem>();
		base.aiActor.OverrideHitEnemies = true;
		tk2dSpriteAnimator obj = guy;
		obj.OnPlayAnimationCalled = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.OnPlayAnimationCalled, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnGuyAnimation));
		base.healthHaver.bodySprites.Add(hatch.sprite);
		base.healthHaver.bodySprites.Add(guy.sprite);
	}

	public void Update()
	{
		if ((bool)base.aiActor.TargetRigidbody)
		{
			Vector2 unitCenter = base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			float num = Vector2.Distance(mainGun.transform.position, unitCenter);
			float num2 = (mainGun.transform.position.XY() - unitCenter).ToAngle();
			for (int i = 0; i < m_miniTurrets.Length; i++)
			{
				TankTreaderMiniTurretController tankTreaderMiniTurretController = m_miniTurrets[i];
				float num3 = Vector2.Distance(tankTreaderMiniTurretController.transform.position, unitCenter);
				if (num3 < num)
				{
					tankTreaderMiniTurretController.aimMode = TankTreaderMiniTurretController.AimMode.AtPlayer;
					tankTreaderMiniTurretController.OverrideAngle = null;
					continue;
				}
				tankTreaderMiniTurretController.aimMode = TankTreaderMiniTurretController.AimMode.Away;
				float num4 = (tankTreaderMiniTurretController.transform.position.XY() - unitCenter).ToAngle();
				float num5 = ((BraveMathCollege.ClampAngle180(num4 - num2) < 0f) ? 1 : (-1));
				tankTreaderMiniTurretController.OverrideAngle = (unitCenter - tankTreaderMiniTurretController.transform.position.XY()).ToAngle() + num5 * backTurretOffset;
			}
		}
		bool flag = true;
		if (base.aiActor.BehaviorVelocity != Vector2.zero)
		{
			float a = base.aiActor.BehaviorVelocity.ToAngle();
			float facingDirection = base.aiAnimator.FacingDirection;
			if (BraveMathCollege.AbsAngleBetween(a, facingDirection) > 170f)
			{
				flag = false;
			}
		}
		if (flag)
		{
			if (m_exhaustFrameCount++ < 5)
			{
				flag = false;
			}
		}
		else
		{
			m_exhaustFrameCount = 0;
		}
		for (int j = 0; j < m_exhaustParticleSystems.Length; j++)
		{
			BraveUtility.EnableEmission(m_exhaustParticleSystems[j], flag);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		AkSoundEngine.PostEvent("Stop_BOSS_tank_idle_01", base.gameObject);
	}

	private void OnGuyAnimation(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if (!m_hasPoppedHatch && (clip.name == "guy_in_gun" || clip.name.StartsWith("guy_fire")))
		{
			StartCoroutine(PopHatchCR());
			m_hasPoppedHatch = true;
		}
	}

	private IEnumerator PopHatchCR()
	{
		hatch.Play("hatch_open");
		hatchPopVfx.SpawnAtLocalPosition(Vector3.zero, 0f, hatch.transform);
		tk2dSprite flyingHatch = UnityEngine.Object.Instantiate(hatchPopObject, hatch.transform.position, Quaternion.identity).GetComponent<tk2dSprite>();
		flyingHatch.HeightOffGround = 7f;
		float elapsed = 0f;
		for (float duration = hatchFlyTime; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			flyingHatch.transform.position += (Vector3)(hatchFlySpeed * BraveTime.DeltaTime);
			flyingHatch.transform.localScale = Vector3.one * (1f - elapsed / duration);
			flyingHatch.UpdateZDepth();
			yield return null;
		}
		UnityEngine.Object.Destroy(flyingHatch);
	}
}
