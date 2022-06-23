using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackInputDirectionalPad : BraveBehaviour
{
	public enum TrackInputSequenceKey
	{
		UP,
		RIGHT,
		DOWN,
		LEFT,
		A,
		B
	}

	protected struct TrackedKeyInput
	{
		public TrackInputSequenceKey sourceKey;

		public float sourceTime;

		public TrackedKeyInput(TrackInputSequenceKey key, float t)
		{
			sourceKey = key;
			sourceTime = t;
		}
	}

	public float InputLifetime = 2f;

	public Projectile HadoukenProjectile;

	public GrappleModule grappleModule;

	private List<TrackedKeyInput> m_trackedInput;

	private TrackedKeyInput? m_lastInput;

	private bool m_hasNulledInput;

	private Gun m_gun;

	private int m_hadoukenCounter;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		m_trackedInput = new List<TrackedKeyInput>();
		grappleModule.sourceGameObject = base.gameObject;
	}

	private void Update()
	{
		if ((bool)m_gun && (bool)m_gun.CurrentOwner && m_gun.CurrentOwner.CurrentGun == m_gun)
		{
			AddNewInputs();
			CheckSequences();
			DropOldInputs();
		}
	}

	private void CheckSequences()
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			return;
		}
		for (int i = 0; i < m_trackedInput.Count; i++)
		{
			TrackedKeyInput trackedKeyInput = m_trackedInput[i];
			if (trackedKeyInput.sourceKey == TrackInputSequenceKey.DOWN && i < m_trackedInput.Count - 2 && m_trackedInput[i + 1].sourceKey == TrackInputSequenceKey.RIGHT && m_trackedInput[i + 2].sourceKey == TrackInputSequenceKey.A)
			{
				m_trackedInput.RemoveAt(i + 2);
				m_trackedInput.RemoveAt(i + 1);
				m_trackedInput.RemoveAt(i);
				i--;
				m_hadoukenCounter = 0;
				Gun gun = m_gun;
				gun.OnPreFireProjectileModifier = (Func<Gun, Projectile, ProjectileModule, Projectile>)Delegate.Combine(gun.OnPreFireProjectileModifier, new Func<Gun, Projectile, ProjectileModule, Projectile>(HadoukenPrefireProjectileModifier));
			}
			if (trackedKeyInput.sourceKey == TrackInputSequenceKey.LEFT && i < m_trackedInput.Count - 2 && m_trackedInput[i + 1].sourceKey == TrackInputSequenceKey.LEFT && m_trackedInput[i + 2].sourceKey == TrackInputSequenceKey.A)
			{
				m_trackedInput.RemoveAt(i + 2);
				m_trackedInput.RemoveAt(i + 1);
				m_trackedInput.RemoveAt(i);
				i--;
				grappleModule.ForceEndGrappleImmediate();
				grappleModule.Trigger(m_gun.CurrentOwner as PlayerController);
			}
		}
	}

	private Projectile HadoukenPrefireProjectileModifier(Gun sourceGun, Projectile sourceProjectile, ProjectileModule sourceModule)
	{
		m_hadoukenCounter++;
		if ((bool)m_gun && m_hadoukenCounter >= sourceGun.Volley.projectiles.Count)
		{
			Gun gun = m_gun;
			gun.OnPreFireProjectileModifier = (Func<Gun, Projectile, ProjectileModule, Projectile>)Delegate.Remove(gun.OnPreFireProjectileModifier, new Func<Gun, Projectile, ProjectileModule, Projectile>(HadoukenPrefireProjectileModifier));
		}
		return HadoukenProjectile;
	}

	private void DropOldInputs()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (m_trackedInput.Count > 0 && realtimeSinceStartup - m_trackedInput[0].sourceTime > InputLifetime)
		{
			m_trackedInput.RemoveAt(0);
		}
	}

	private void AddNewInputs()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (m_trackedInput.Count == 0)
		{
			m_lastInput = null;
		}
		bool flag = false;
		TrackedKeyInput trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.A, realtimeSinceStartup);
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer((m_gun.CurrentOwner as PlayerController).PlayerIDX);
		if ((bool)instanceForPlayer && instanceForPlayer.ActiveActions != null)
		{
			GungeonActions activeActions = instanceForPlayer.ActiveActions;
			if (activeActions.ShootAction.WasPressed)
			{
				flag = true;
				trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.A, realtimeSinceStartup);
			}
			if (activeActions.DodgeRollAction.WasPressed)
			{
				flag = true;
				trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.B, realtimeSinceStartup);
			}
			if (!flag)
			{
				Vector2 vector = activeActions.Move.Vector;
				Vector2 majorAxis = BraveUtility.GetMajorAxis(vector);
				if (Mathf.Abs(vector.x) < 0.1f && Mathf.Abs(vector.y) < 0.1f)
				{
					m_hasNulledInput = true;
				}
				else if (majorAxis.x > 0f)
				{
					flag = true;
					trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.RIGHT, realtimeSinceStartup);
				}
				else if (majorAxis.x < 0f)
				{
					flag = true;
					trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.LEFT, realtimeSinceStartup);
				}
				else if (majorAxis.y > 0f)
				{
					flag = true;
					trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.UP, realtimeSinceStartup);
				}
				else if (majorAxis.y < 0f)
				{
					flag = true;
					trackedKeyInput = new TrackedKeyInput(TrackInputSequenceKey.DOWN, realtimeSinceStartup);
				}
			}
		}
		if (flag && !m_hasNulledInput && m_lastInput.HasValue && m_lastInput.Value.sourceKey == trackedKeyInput.sourceKey)
		{
			flag = false;
		}
		if (flag)
		{
			m_trackedInput.Add(trackedKeyInput);
			m_lastInput = trackedKeyInput;
			m_hasNulledInput = false;
		}
	}
}
