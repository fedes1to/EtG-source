using UnityEngine;

public class ShootVolleyFromObjectSynergyProcessor : MonoBehaviour
{
	public enum TriggerType
	{
		CONTINUOUS,
		ON_SHOOT
	}

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public TriggerType trigger;

	public bool usePlayerAim;

	public ProjectileModule singleModule;

	public ProjectileVolleyData volley;

	public float cooldown = 3f;

	public float maxRange = 30f;

	public Transform optionalShootPoint;

	private float m_cooldown;

	private PlayerController m_player;

	private void Awake()
	{
		m_cooldown = cooldown;
	}

	private void Start()
	{
		PlayerOrbital component = GetComponent<PlayerOrbital>();
		if ((bool)component)
		{
			m_player = component.Owner;
		}
		if (!m_player)
		{
			m_player = GetComponentInParent<PlayerController>();
		}
	}

	private void Update()
	{
		m_cooldown -= BraveTime.DeltaTime;
		if (!(m_cooldown <= 0f))
		{
			return;
		}
		bool flag = false;
		if (trigger == TriggerType.CONTINUOUS)
		{
			m_cooldown = cooldown;
			flag = true;
		}
		else if (trigger == TriggerType.ON_SHOOT)
		{
			flag = (bool)m_player && m_player.IsFiring;
		}
		if (!flag)
		{
			return;
		}
		int count = -1;
		if (!PlayerController.AnyoneHasActiveBonusSynergy(RequiredSynergy, out count))
		{
			return;
		}
		if (trigger == TriggerType.ON_SHOOT)
		{
			m_cooldown = cooldown;
		}
		Vector2 vector = ((!optionalShootPoint) ? base.transform.position.XY() : optionalShootPoint.position.XY());
		bool flag2 = false;
		Vector2 vector2 = Vector2.up;
		if (usePlayerAim)
		{
			flag2 = true;
			Vector2 vector3 = m_player.unadjustedAimPoint.XY();
			if (!BraveInput.GetInstanceForPlayer(m_player.PlayerIDX).IsKeyboardAndMouse() && (bool)m_player.CurrentGun)
			{
				vector3 = m_player.CenterPosition + BraveMathCollege.DegreesToVector(m_player.CurrentGun.CurrentAngle, 10f);
			}
			vector2 = vector3 - vector;
		}
		else
		{
			float nearestDistance = -1f;
			AIActor nearestEnemy = vector.GetAbsoluteRoom().GetNearestEnemy(vector, out nearestDistance);
			if ((bool)nearestEnemy)
			{
				vector2 = nearestEnemy.CenterPosition - vector;
				flag2 = nearestDistance < maxRange;
			}
		}
		if (flag2)
		{
			if ((bool)volley)
			{
				VolleyUtility.FireVolley(volley, vector, vector2, GameManager.Instance.BestActivePlayer);
			}
			else
			{
				VolleyUtility.ShootSingleProjectile(singleModule, null, vector, vector2.ToAngle(), 0f, GameManager.Instance.BestActivePlayer);
			}
		}
	}
}
