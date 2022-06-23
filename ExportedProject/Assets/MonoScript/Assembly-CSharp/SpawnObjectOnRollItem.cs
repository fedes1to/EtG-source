using System.Collections;
using UnityEngine;

public class SpawnObjectOnRollItem : PassiveItem
{
	public GameObject ObjectToSpawn;

	public bool DoBounce;

	public float BounceDuration = 1f;

	public float BounceStartVelocity = 5f;

	public float GravityAcceleration = 10f;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			player.OnRollStarted += OnRollStarted;
			base.Pickup(player);
		}
	}

	private void OnRollStarted(PlayerController obj, Vector2 dirVec)
	{
		if ((bool)ObjectToSpawn)
		{
			GameObject gameObject = Object.Instantiate(ObjectToSpawn, obj.transform.position, Quaternion.identity);
			gameObject.GetComponent<tk2dSprite>().PlaceAtPositionByAnchor(obj.CenterPosition, tk2dBaseSprite.Anchor.MiddleCenter);
			if (DoBounce)
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleObjectBounce(gameObject.transform));
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.OnRollStarted -= OnRollStarted;
		debrisObject.GetComponent<SpawnObjectOnRollItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	private IEnumerator HandleObjectBounce(Transform target)
	{
		float elapsed = 0f;
		Vector3 startPos = target.position;
		Vector3 adjPos = startPos;
		float yVelocity = BounceStartVelocity;
		while (elapsed < BounceDuration && (bool)target)
		{
			elapsed += BraveTime.DeltaTime;
			yVelocity -= GravityAcceleration * BraveTime.DeltaTime;
			adjPos += new Vector3(0f, yVelocity * BraveTime.DeltaTime, 0f);
			target.position = adjPos;
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= OnRollStarted;
		}
		base.OnDestroy();
	}
}
