using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BreakableColumn : DungeonPlaceableBehaviour
{
	private enum State
	{
		Default,
		Damaged,
		Destroyed
	}

	[FormerlySerializedAs("damagedAnimation")]
	public string damagedSprite;

	[FormerlySerializedAs("destroyAnimation")]
	public string destroyedSprite;

	[Header("Flake Data")]
	public GameObject flake;

	public VFXPool puff;

	public int flakeCount;

	public float flakeAreaWidth;

	public float flakeAreaHeight;

	public float flakeSpawnDuration;

	[Header("Explosion Data")]
	public ExplosionData explosionData;

	private State m_state;

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
	}

	public void Update()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!otherRigidbody.projectile || (!otherRigidbody.name.StartsWith("TankTreader_Fast_Projectile") && !otherRigidbody.name.StartsWith("TankTreader_Scatter_Projectile") && !otherRigidbody.name.StartsWith("TankTreader_Spawn_Projectile") && !otherRigidbody.name.StartsWith("TankTreader_Rocket_Projectile")))
		{
			return;
		}
		if (m_state == State.Default)
		{
			base.sprite.SetSprite(damagedSprite);
			m_state = State.Damaged;
			SpawnFlakes();
			if (!PhysicsEngine.PendingCastResult.Overlap)
			{
				return;
			}
		}
		if (m_state == State.Damaged)
		{
			PhysicsEngine.SkipCollision = true;
			Exploder.Explode(PhysicsEngine.PendingCastResult.Contact, explosionData, PhysicsEngine.PendingCastResult.Normal);
			base.sprite.SetSprite(destroyedSprite);
			base.specRigidbody.enabled = false;
			SetAreaPassable();
			base.sprite.IsPerpendicular = false;
			base.sprite.HeightOffGround = -1.95f;
			base.sprite.UpdateZDepth();
			base.gameObject.layer = LayerMask.NameToLayer("BG_Critical");
			BreakableChunk component = GetComponent<BreakableChunk>();
			if ((bool)component)
			{
				component.Trigger(false, PhysicsEngine.PendingCastResult.Contact);
			}
			m_state = State.Destroyed;
		}
	}

	private void SpawnFlakes()
	{
		if (flakeCount <= 0)
		{
			return;
		}
		for (int i = 0; i < flakeCount; i++)
		{
			if (flakeSpawnDuration == 0f)
			{
				SpawnRandomizeFlakes();
			}
			else
			{
				Invoke("SpawnRandomizeFlakes", UnityEngine.Random.Range(0f, flakeSpawnDuration));
			}
		}
	}

	private void SpawnRandomizeFlakes()
	{
		Vector3 position = base.transform.position + new Vector3(UnityEngine.Random.Range(0f, flakeAreaWidth), UnityEngine.Random.Range(0f, flakeAreaHeight));
		puff.SpawnAtPosition(position, 0f, null, Vector2.zero, Vector2.zero);
		GameObject gameObject = UnityEngine.Object.Instantiate(flake, position, Quaternion.identity);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.HeightOffGround = 0.1f;
		base.sprite.AttachRenderer(component);
		component.UpdateZDepth();
	}
}
