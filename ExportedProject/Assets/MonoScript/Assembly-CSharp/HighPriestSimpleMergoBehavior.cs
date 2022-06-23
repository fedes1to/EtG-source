using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/HighPriest/SimpleMergoBehavior")]
public class HighPriestSimpleMergoBehavior : BasicAttackBehavior
{
	public BulletScriptSelector wallBulletScript;

	public int numShots = 2;

	private const float c_wallBuffer = 5f;

	private float m_timer;

	private float m_wallShotTimer;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		for (int i = 0; i < numShots; i++)
		{
			ShootWallBulletScript();
		}
		UpdateCooldowns();
		return BehaviorResult.Continue;
	}

	private void ShootWallBulletScript()
	{
		float rotation;
		Vector2 vector = RandomWallPoint(out rotation);
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (!playerController || playerController.healthHaver.IsDead || Vector2.Distance(vector, playerController.CenterPosition) < 8f)
			{
				return;
			}
		}
		GameObject gameObject = new GameObject("Mergo wall shoot point");
		BulletScriptSource orAddComponent = gameObject.GetOrAddComponent<BulletScriptSource>();
		gameObject.GetOrAddComponent<BulletSourceKiller>();
		orAddComponent.transform.position = vector;
		orAddComponent.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
		orAddComponent.BulletManager = m_aiActor.bulletBank;
		orAddComponent.BulletScript = wallBulletScript;
		orAddComponent.Initialize();
	}

	private Vector2 RandomWallPoint(out float rotation)
	{
		float num = 4f;
		CellArea area = m_aiActor.ParentRoom.area;
		Vector2 vector = area.basePosition.ToVector2() + new Vector2(0.5f, 1.5f);
		Vector2 vector2 = (area.basePosition + area.dimensions).ToVector2() - new Vector2(0.5f, 0.5f);
		if (BraveUtility.RandomBool())
		{
			if (BraveUtility.RandomBool())
			{
				rotation = -90f;
				return new Vector2(Random.Range(vector.x + 5f, vector2.x - 5f), vector2.y + num + 2f);
			}
			rotation = 90f;
			return new Vector2(Random.Range(vector.x + 5f, vector2.x - 5f), vector.y - num);
		}
		if (BraveUtility.RandomBool())
		{
			rotation = 0f;
			return new Vector2(vector.x - num, Random.Range(vector.y + 5f, vector2.y - 5f));
		}
		rotation = 180f;
		return new Vector2(vector2.x + num, Random.Range(vector.y + 5f, vector2.y - 5f));
	}
}
