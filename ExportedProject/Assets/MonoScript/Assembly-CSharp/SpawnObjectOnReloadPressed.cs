using System;
using System.Collections;
using UnityEngine;

public class SpawnObjectOnReloadPressed : MonoBehaviour
{
	public GameObject SpawnObject;

	public float tossForce;

	public float DelayTime;

	public bool canBounce = true;

	[ShowInInspectorIf("tossForce", 0, false)]
	public bool orphaned = true;

	[ShowInInspectorIf("tossForce", 0, false)]
	public bool preventRotation;

	[CheckAnimation(null)]
	public string AnimToPlay;

	public bool RequiresSynergy;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool RequiresActualReload;

	private Gun m_gun;

	private PlayerController m_playerOwner;

	private bool m_semaphore;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnInitializedWithOwner = (Action<GameActor>)Delegate.Combine(gun.OnInitializedWithOwner, new Action<GameActor>(OnGunInitialized));
		Gun gun2 = m_gun;
		gun2.OnDropped = (Action)Delegate.Combine(gun2.OnDropped, new Action(OnGunDroppedOrDestroyed));
		if (RequiresActualReload)
		{
			Gun gun3 = m_gun;
			gun3.OnAutoReload = (Action<PlayerController, Gun>)Delegate.Combine(gun3.OnAutoReload, new Action<PlayerController, Gun>(HandleAutoReload));
		}
		Gun gun4 = m_gun;
		gun4.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun4.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
		if (m_gun.CurrentOwner != null)
		{
			OnGunInitialized(m_gun.CurrentOwner);
		}
	}

	private void HandleAutoReload(PlayerController arg1, Gun arg2)
	{
		HandleReloadPressed(arg1, arg2, false);
	}

	private void HandleReloadPressed(PlayerController user, Gun sourceGun, bool actual)
	{
		if ((RequiresSynergy && (!user || !user.HasActiveBonusSynergy(RequiredSynergy))) || m_semaphore || (RequiresActualReload && sourceGun.ClipShotsRemaining == sourceGun.ClipCapacity))
		{
			return;
		}
		m_semaphore = RequiresActualReload;
		if (!m_gun.IsFiring || RequiresActualReload)
		{
			if (!string.IsNullOrEmpty(AnimToPlay))
			{
				if (!sourceGun.spriteAnimator.IsPlaying(AnimToPlay))
				{
					user.StartCoroutine(DoSpawn(user, 0f));
				}
			}
			else
			{
				user.StartCoroutine(DoSpawn(user, 0f));
			}
		}
		if (m_semaphore)
		{
			user.StartCoroutine(HandleReloadDelay(sourceGun));
		}
	}

	private IEnumerator HandleReloadDelay(Gun sourceGun)
	{
		yield return new WaitForSeconds(sourceGun.reloadTime);
		m_semaphore = false;
	}

	protected IEnumerator DoSpawn(PlayerController user, float angleFromAim)
	{
		if (!string.IsNullOrEmpty(AnimToPlay))
		{
			m_gun.spriteAnimator.Play(AnimToPlay);
		}
		if (DelayTime > 0f)
		{
			float ela = 0f;
			while (ela < DelayTime)
			{
				ela += BraveTime.DeltaTime;
				yield return null;
			}
		}
		if (!this)
		{
			yield break;
		}
		Projectile spawnProj = SpawnObject.GetComponent<Projectile>();
		if (spawnProj != null)
		{
			Vector2 v = user.unadjustedAimPoint - user.LockedApproximateSpriteCenter;
			UnityEngine.Object.Instantiate(SpawnObject, m_gun.barrelOffset.position, Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(v)));
		}
		else if (tossForce == 0f)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(SpawnObject, m_gun.barrelOffset.position, Quaternion.identity);
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			if (component != null)
			{
				component.PlaceAtPositionByAnchor(m_gun.barrelOffset.position, tk2dBaseSprite.Anchor.MiddleCenter);
				if (component.specRigidbody != null)
				{
					component.specRigidbody.RegisterGhostCollisionException(user.specRigidbody);
				}
			}
			gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
			if (!orphaned)
			{
				gameObject.transform.parent = m_gun.barrelOffset;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localPosition = Vector3.zero;
			}
			else
			{
				gameObject.transform.rotation = ((!preventRotation) ? m_gun.barrelOffset.rotation : Quaternion.identity);
				gameObject.transform.localScale = m_gun.barrelOffset.lossyScale;
				gameObject.transform.position = m_gun.barrelOffset.position;
			}
		}
		else
		{
			Vector3 vector = user.unadjustedAimPoint - user.LockedApproximateSpriteCenter;
			Vector3 position = m_gun.barrelOffset.position;
			if (vector.x < 0f)
			{
				position += Vector3.left;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(SpawnObject, position, Quaternion.identity);
			Vector2 vector2 = user.unadjustedAimPoint - user.LockedApproximateSpriteCenter;
			vector2 = Quaternion.Euler(0f, 0f, angleFromAim) * vector2;
			DebrisObject debrisObject = LootEngine.DropItemWithoutInstantiating(gameObject2, gameObject2.transform.position, vector2, tossForce, false, false, true);
			debrisObject.Priority = EphemeralObject.EphemeralPriority.Critical;
			debrisObject.bounceCount = (canBounce ? 1 : 0);
		}
		DeadlyDeadlyGoopManager.IgniteGoopsLine(m_gun.barrelOffset.position.XY(), m_gun.barrelOffset.position.XY() + (m_gun.barrelOffset.rotation * Vector3.right * 2.5f).XY(), 2f);
	}

	private void OnGunInitialized(GameActor obj)
	{
		if (m_playerOwner != null)
		{
			OnGunDroppedOrDestroyed();
		}
		if (!(obj == null) && obj is PlayerController)
		{
			m_playerOwner = obj as PlayerController;
		}
	}

	private void OnDestroy()
	{
		OnGunDroppedOrDestroyed();
	}

	private void OnGunDroppedOrDestroyed()
	{
		if (m_playerOwner != null)
		{
			m_playerOwner = null;
		}
	}
}
