using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletLimbController : BraveBehaviour
{
	public string LimitPrefix;

	public bool OverrideHeightOffGround;

	[ShowInInspectorIf("OverrideHeightOffGround", true)]
	public float HeightOffGround;

	public string OverrideLimbName;

	public string OverrideFinalLimbName;

	public bool CollideWithOthers = true;

	public bool DisableTileMapCollisions;

	public bool RotateToMatchTransforms;

	public bool WarpBullets;

	private bool m_doingTell;

	private AIActor m_body;

	private List<Transform> m_transforms = new List<Transform>();

	private List<Projectile> m_projectiles = new List<Projectile>();

	public bool HideBullets { get; set; }

	public bool DoingTell
	{
		set
		{
			m_doingTell = value;
			for (int i = 0; i < m_projectiles.Count; i++)
			{
				Projectile projectile = m_projectiles[i];
				if ((bool)projectile)
				{
					if (m_doingTell)
					{
						projectile.spriteAnimator.Play();
					}
					else
					{
						projectile.spriteAnimator.StopAndResetFrameToDefault();
					}
				}
			}
		}
	}

	public void Start()
	{
		m_body = base.transform.parent.GetComponent<AIActor>();
		if (m_body == null)
		{
			m_body = base.aiAnimator.SpecifyAiActor;
		}
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i] == base.transform) && (string.IsNullOrEmpty(LimitPrefix) || componentsInChildren[i].name.StartsWith(LimitPrefix)))
			{
				m_transforms.Add(componentsInChildren[i]);
			}
		}
		for (int j = 0; j < m_transforms.Count; j++)
		{
			m_projectiles.Add(CreateProjectile(m_transforms[j]));
		}
		m_body.bulletBank.transforms.AddRange(m_transforms);
		m_body.specRigidbody.CanCarry = true;
		SpeculativeRigidbody speculativeRigidbody = m_body.specRigidbody;
		speculativeRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(speculativeRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(PostBodyMovement));
	}

	public void Update()
	{
		base.renderer.enabled = false;
	}

	public void LateUpdate()
	{
		if (BraveTime.DeltaTime == 0f)
		{
			PostBodyMovement(base.specRigidbody, Vector2.zero, IntVector2.Zero);
		}
	}

	public void PostBodyMovement(SpeculativeRigidbody specRigidbody, Vector2 unitDelta, IntVector2 pixelDelta)
	{
		if (!m_body)
		{
			return;
		}
		for (int i = 0; i < m_transforms.Count; i++)
		{
			GameObject gameObject = m_transforms[i].gameObject;
			Projectile projectile = m_projectiles[i];
			if ((bool)projectile && (bool)m_body && m_body.IsBlackPhantom != projectile.IsBlackBullet)
			{
				if (m_body.IsBlackPhantom)
				{
					projectile.ForceBlackBullet = true;
					projectile.BecomeBlackBullet();
				}
				else
				{
					projectile.ForceBlackBullet = false;
					projectile.ReturnFromBlackBullet();
				}
			}
			if (gameObject.activeSelf && !HideBullets)
			{
				if (!projectile)
				{
					m_projectiles[i] = CreateProjectile(gameObject.transform);
					continue;
				}
				if (!projectile.gameObject.activeSelf)
				{
					projectile.gameObject.SetActive(true);
					projectile.specRigidbody.enabled = true;
					projectile.transform.position = gameObject.transform.position;
					projectile.specRigidbody.Reinitialize();
				}
				if (BraveTime.DeltaTime > 0f)
				{
					if (WarpBullets)
					{
						projectile.specRigidbody.Velocity = Vector2.zero;
						projectile.specRigidbody.transform.position = gameObject.transform.position;
						projectile.specRigidbody.Reinitialize();
						projectile.sprite.UpdateZDepth();
					}
					else
					{
						projectile.specRigidbody.Velocity = (gameObject.transform.position.XY() - projectile.specRigidbody.Position.UnitPosition) / BraveTime.DeltaTime;
					}
				}
				else
				{
					projectile.specRigidbody.Velocity = Vector2.zero;
					projectile.transform.position = gameObject.transform.position;
					projectile.transform.position = projectile.transform.position.WithZ(projectile.transform.position.y);
					projectile.specRigidbody.sprite.UpdateZDepth();
				}
				if (RotateToMatchTransforms)
				{
					projectile.transform.rotation = gameObject.transform.rotation;
				}
			}
			else if ((bool)projectile && projectile.gameObject.activeSelf)
			{
				projectile.gameObject.SetActive(false);
				projectile.specRigidbody.enabled = false;
				projectile.specRigidbody.Velocity = Vector2.zero;
			}
		}
	}

	protected override void OnDestroy()
	{
		DestroyProjectiles();
		base.OnDestroy();
	}

	public void DestroyProjectiles()
	{
		for (int i = 0; i < m_projectiles.Count; i++)
		{
			Projectile projectile = m_projectiles[i];
			if ((bool)projectile)
			{
				if (projectile.gameObject.activeSelf)
				{
					projectile.gameObject.SetActive(false);
					projectile.specRigidbody.enabled = false;
					projectile.specRigidbody.Velocity = Vector2.zero;
				}
				projectile.DieInAir(!projectile.gameObject.activeSelf);
			}
		}
	}

	private Projectile CreateProjectile(Transform transform)
	{
		if (BossKillCam.BossDeathCamRunning)
		{
			return null;
		}
		string bulletName = "limb";
		if (!string.IsNullOrEmpty(OverrideFinalLimbName) && IsFinalLimb(transform))
		{
			bulletName = OverrideFinalLimbName;
		}
		else if (!string.IsNullOrEmpty(OverrideLimbName))
		{
			bulletName = OverrideLimbName;
		}
		GameObject gameObject = m_body.bulletBank.CreateProjectileFromBank(transform.position, 0f, bulletName);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.ManualControl = true;
		component.SkipDistanceElapsedCheck = true;
		component.BulletScriptSettings.surviveRigidbodyCollisions = true;
		component.BulletScriptSettings.surviveTileCollisions = true;
		component.gameObject.SetActive(false);
		component.specRigidbody.enabled = false;
		component.specRigidbody.Velocity = Vector2.zero;
		component.specRigidbody.CanBeCarried = true;
		component.specRigidbody.CollideWithOthers = CollideWithOthers;
		if (DisableTileMapCollisions)
		{
			component.specRigidbody.CollideWithTileMap = false;
		}
		if ((bool)m_body && m_body.IsBlackPhantom)
		{
			component.ForceBlackBullet = true;
		}
		if (OverrideHeightOffGround)
		{
			component.sprite.HeightOffGround = HeightOffGround;
		}
		return component;
	}

	private bool IsFinalLimb(Transform transform)
	{
		return transform == m_transforms[m_transforms.Count - 1];
	}
}
