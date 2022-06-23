using System.Collections;
using Dungeonator;
using UnityEngine;

public class ShellCasing : MonoBehaviour
{
	private Vector3 startPosition;

	public float heightVariance = 0.5f;

	private float amountToFall;

	public float angularVelocity = 1080f;

	public float pushStrengthMultiplier = 1f;

	public bool usesCustomTargetHeight;

	public float customTargetHeight;

	public float overallMultiplier = 1f;

	public bool usesCustomVelocity;

	public Vector2 customVelocity = Vector2.zero;

	public string audioEventName;

	private Vector3 velocity;

	private bool isDone;

	private bool hasBeenAboveTargetHeight;

	private Transform m_transform;

	private Renderer m_renderer;

	private bool m_hasBeenTriggered;

	private void Start()
	{
		m_transform = base.transform;
	}

	public void Trigger()
	{
		m_transform = base.transform;
		m_renderer = GetComponent<Renderer>();
		float num = ((!usesCustomTargetHeight) ? GameManager.Instance.PrimaryPlayer.transform.position.y : customTargetHeight);
		startPosition = m_transform.position;
		int num2 = ((m_transform.right.x > 0f) ? 1 : (-1));
		if (usesCustomVelocity)
		{
			velocity = customVelocity.ToVector3ZUp();
		}
		else
		{
			velocity = Vector3.up * (Random.value * 1.5f + 1f) + -1.5f * Vector3.right * num2 * (Random.value + 1.5f);
		}
		amountToFall = startPosition.y - num + Random.value * heightVariance;
		if (amountToFall > 0f)
		{
			hasBeenAboveTargetHeight = true;
		}
		GetComponent<tk2dSprite>().automaticallyManagesDepth = false;
		DepthLookupManager.ProcessRenderer(m_renderer, DepthLookupManager.GungeonSortingLayer.PLAYFIELD);
		DepthLookupManager.UpdateRendererWithWorldYPosition(m_renderer, num);
		isDone = false;
		m_hasBeenTriggered = true;
	}

	private void Update()
	{
		if (BraveUtility.isLoadingLevel || !m_hasBeenTriggered || isDone)
		{
			return;
		}
		IntVector2 vec = new IntVector2(Mathf.FloorToInt(m_transform.position.x), Mathf.FloorToInt(m_transform.position.y));
		if (!GameManager.Instance.Dungeon.data.CheckInBounds(vec))
		{
			isDone = true;
			return;
		}
		CellData cellData = GameManager.Instance.Dungeon.data.cellData[vec.x][vec.y];
		if (cellData.type == CellType.WALL)
		{
			velocity.x = 0f - velocity.x;
		}
		float num = BraveTime.DeltaTime * overallMultiplier;
		if (velocity.y < 0f)
		{
			hasBeenAboveTargetHeight = true;
		}
		if (m_transform.position.y > startPosition.y - amountToFall || (cellData != null && !cellData.IsPassable) || !hasBeenAboveTargetHeight)
		{
			m_transform.Rotate(0f, 0f, angularVelocity * num);
			m_transform.position += velocity * num;
			velocity += Vector3.down * 10f * num;
			return;
		}
		isDone = true;
		if (!string.IsNullOrEmpty(audioEventName))
		{
			AkSoundEngine.PostEvent(audioEventName, base.gameObject);
		}
		DepthLookupManager.UpdateRendererWithWorldYPosition(m_renderer, m_transform.position.y);
		MinorBreakable component = GetComponent<MinorBreakable>();
		if (component != null)
		{
			component.Break(Random.insideUnitCircle);
		}
	}

	public void ApplyVelocity(Vector2 vel)
	{
		StartCoroutine(HandlePush(vel * pushStrengthMultiplier));
	}

	private IEnumerator HandlePush(Vector2 vel)
	{
		Vector3 pushVel = new Vector3(vel.x, vel.y, 0f);
		while (pushVel != Vector3.zero)
		{
			pushVel *= 0.97f;
			if (pushVel.magnitude < 0.025f)
			{
				pushVel = Vector3.zero;
			}
			IntVector2 nextGridCellX = new IntVector2(Mathf.FloorToInt(m_transform.position.x + pushVel.x * BraveTime.DeltaTime), Mathf.FloorToInt(m_transform.position.y));
			IntVector2 nextGridCellY = new IntVector2(Mathf.FloorToInt(m_transform.position.x), Mathf.FloorToInt(m_transform.position.y + pushVel.y * BraveTime.DeltaTime));
			CellData tileX = GameManager.Instance.Dungeon.data.cellData[nextGridCellX.x][nextGridCellX.y];
			CellData tileY = GameManager.Instance.Dungeon.data.cellData[nextGridCellY.x][nextGridCellY.y];
			if (!tileX.IsPassable)
			{
				pushVel = new Vector3(0f, pushVel.y, 0f);
			}
			if (!tileY.IsPassable)
			{
				pushVel = new Vector3(pushVel.x, 0f, 0f);
			}
			m_transform.position += pushVel * BraveTime.DeltaTime;
			yield return null;
		}
	}
}
