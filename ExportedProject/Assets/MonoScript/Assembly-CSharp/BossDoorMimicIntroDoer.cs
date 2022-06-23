using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BossDoorMimicIntroDoer : SpecificIntroDoer
{
	private bool m_finished;

	private PlayerController m_enteringPlayer;

	private DungeonDoorController m_bossDoor;

	private Vector2 m_bossStartingPosition;

	private float m_cachedHeightOffGround;

	public DungeonDoorSubsidiaryBlocker PhantomDoorBlocker { get; set; }

	public override Vector2? OverrideIntroPosition
	{
		get
		{
			return m_bossDoor.transform.position;
		}
	}

	public override bool IsIntroFinished
	{
		get
		{
			return m_finished;
		}
	}

	protected override void OnDestroy()
	{
		if (GameManager.HasInstance)
		{
			if ((bool)PhantomDoorBlocker)
			{
				PhantomDoorBlocker.Unseal();
			}
			if ((bool)m_bossDoor)
			{
				tk2dBaseSprite tk2dBaseSprite2 = m_bossDoor.sealAnimators[0].sprite;
				Renderer[] componentsInChildren = tk2dBaseSprite2.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = true;
				}
			}
		}
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		if (m_bossDoor != null)
		{
			return;
		}
		m_bossDoor = null;
		float num = float.MaxValue;
		DungeonDoorController[] array = Object.FindObjectsOfType<DungeonDoorController>();
		foreach (DungeonDoorController dungeonDoorController in array)
		{
			if (dungeonDoorController.name.StartsWith("GungeonBossDoor"))
			{
				SpeculativeRigidbody componentInChildren = dungeonDoorController.GetComponentInChildren<SpeculativeRigidbody>();
				float num2 = Vector2.Distance(player.specRigidbody.UnitCenter, componentInChildren.UnitCenter);
				if (m_bossDoor == null || num2 < num)
				{
					m_bossDoor = dungeonDoorController;
					num = num2;
				}
			}
		}
		tk2dSpriteAnimator[] componentsInChildren = m_bossDoor.GetComponentsInChildren<tk2dSpriteAnimator>();
		foreach (tk2dSpriteAnimator tk2dSpriteAnimator2 in componentsInChildren)
		{
			if (tk2dSpriteAnimator2.name == "Eye Fire")
			{
				animators.Add(tk2dSpriteAnimator2);
			}
		}
		m_enteringPlayer = player;
		m_bossStartingPosition = base.transform.position;
		m_cachedHeightOffGround = base.sprite.HeightOffGround;
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		tk2dSpriteAnimator[] componentsInChildren = GetComponentsInChildren<tk2dSpriteAnimator>(true);
		foreach (tk2dSpriteAnimator tk2dSpriteAnimator2 in componentsInChildren)
		{
			if (tk2dSpriteAnimator2 != base.spriteAnimator)
			{
				animators.Add(tk2dSpriteAnimator2);
			}
		}
		StartCoroutine(DoIntro());
	}

	public override void EndIntro()
	{
		StopAllCoroutines();
		tk2dBaseSprite tk2dBaseSprite2 = m_bossDoor.sealAnimators[0].sprite;
		Renderer[] componentsInChildren = tk2dBaseSprite2.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
		}
		base.transform.position = m_bossStartingPosition;
		base.specRigidbody.Reinitialize();
		tk2dBaseSprite component = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		component.color = component.color.WithAlpha(1f);
		base.aiAnimator.LockFacingDirection = false;
		base.aiAnimator.FacingDirection = 90f;
		base.aiAnimator.EndAnimation();
		base.sprite.HeightOffGround = m_cachedHeightOffGround;
		base.sprite.UpdateZDepth();
		SpawnDoorBlocker();
	}

	private IEnumerator DoIntro()
	{
		base.specRigidbody.Initialize();
		Vector3 offset = base.specRigidbody.UnitBottomLeft - base.transform.position.XY();
		Vector2 goalPosition = base.specRigidbody.UnitCenter;
		float startingDirection = 0f;
		float playerDirection = 0f;
		Vector2 majorAxisToPlayer = m_enteringPlayer.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter;
		if (majorAxisToPlayer.y < -0.5f)
		{
			startingDirection = 270f;
			playerDirection = 90f;
		}
		else if (majorAxisToPlayer.x > 0.5f)
		{
			startingDirection = 360f;
			playerDirection = 180f;
			base.sprite.HeightOffGround += 2f;
			base.sprite.UpdateZDepth();
		}
		else if (majorAxisToPlayer.x < -0.5f)
		{
			startingDirection = 180f;
			playerDirection = 360f;
			base.sprite.HeightOffGround += 2f;
			base.sprite.UpdateZDepth();
		}
		else
		{
			Debug.LogError("UNSUPPORTED BOSS DOOR MIMIC ENTER DIRECTION!");
		}
		tk2dBaseSprite doorSprite = m_bossDoor.sealAnimators[0].sprite;
		base.transform.position = doorSprite.transform.position - offset;
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = startingDirection;
		base.aiAnimator.Update();
		tk2dBaseSprite shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		shadowSprite.color = shadowSprite.color.WithAlpha(0f);
		Renderer[] componentsInChildren = doorSprite.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
		}
		float elapsed5 = 0f;
		float duration5;
		for (duration5 = 1f; elapsed5 < duration5; elapsed5 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		elapsed5 = 0f;
		duration5 = BraveMathCollege.AbsAngleBetween(playerDirection, startingDirection) / 360f * 2f;
		while (elapsed5 < duration5)
		{
			yield return null;
			elapsed5 += GameManager.INVARIANT_DELTA_TIME;
			base.aiAnimator.FacingDirection = Mathf.Lerp(startingDirection, playerDirection, elapsed5 / duration5);
			base.aiAnimator.Update();
		}
		elapsed5 = 0f;
		for (duration5 = 1f; elapsed5 < duration5; elapsed5 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		base.aiAnimator.PlayUntilCancelled("teleport_out");
		while (base.aiAnimator.IsPlaying("teleport_out"))
		{
			yield return null;
		}
		base.aiAnimator.renderer.enabled = false;
		base.specRigidbody.Reinitialize();
		Vector2 offsetPosition = base.specRigidbody.UnitCenter - base.transform.position.XY();
		CameraController cameraController = GameManager.Instance.MainCameraController;
		Vector2 cameraStartPosition = cameraController.transform.position;
		elapsed5 = 0f;
		duration5 = 2f;
		while (elapsed5 < duration5)
		{
			yield return null;
			elapsed5 += GameManager.INVARIANT_DELTA_TIME;
			cameraController.OverridePosition = Vector2.Lerp(cameraStartPosition, goalPosition, Mathf.SmoothStep(0f, 1f, elapsed5 / duration5));
		}
		base.transform.position = goalPosition - offsetPosition;
		base.aiAnimator.renderer.enabled = true;
		base.aiAnimator.FacingDirection = -90f;
		base.aiAnimator.PlayUntilCancelled("teleport_in");
		base.aiAnimator.Update();
		while (base.aiAnimator.IsPlaying("teleport_in"))
		{
			yield return null;
			shadowSprite.color = shadowSprite.color.WithAlpha(base.aiAnimator.CurrentClipProgress);
		}
		base.aiAnimator.PlayUntilCancelled("intro");
		elapsed5 = 0f;
		for (duration5 = 2f; elapsed5 < duration5; elapsed5 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		base.aiAnimator.EndAnimation();
		base.aiAnimator.Update();
		m_finished = true;
	}

	private void SpawnDoorBlocker()
	{
		if (!(GameManager.Instance.Dungeon.phantomBlockerDoorObjects == null) && !PhantomDoorBlocker)
		{
			DungeonData.Direction direction = DungeonData.Direction.NORTH;
			IntVector2 location = new IntVector2(22, -5);
			GameObject gameObject = GameManager.Instance.Dungeon.phantomBlockerDoorObjects.InstantiateObjectDirectional(base.aiActor.ParentRoom, location, direction);
			PhantomDoorBlocker = gameObject.GetComponent<DungeonDoorSubsidiaryBlocker>();
			PhantomDoorBlocker.Seal();
		}
	}
}
