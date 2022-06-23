using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ConveyorBelt : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	[DwarfConfigurable]
	public float ConveyorWidth = 4f;

	[DwarfConfigurable]
	public float ConveyorHeight = 3f;

	[DwarfConfigurable]
	public float VelocityX;

	[DwarfConfigurable]
	public float VelocityY;

	public bool IsHorizontal;

	public List<tk2dBaseSprite> ShadowObjects;

	public List<tk2dSpriteAnimator> ModuleAnimators;

	public List<string> NegativeVelocityAnims;

	public List<string> PositiveVelocityAnims;

	private Vector2 Velocity;

	private List<SpeculativeRigidbody> m_rigidbodiesOnPlatform = new List<SpeculativeRigidbody>();

	private IEnumerator Start()
	{
		Velocity = new Vector2(VelocityX, VelocityY);
		IntVector2 Size = new IntVector2(Mathf.FloorToInt(ConveyorWidth), Mathf.FloorToInt(ConveyorHeight));
		if (IsHorizontal)
		{
			ModuleAnimators[0].transform.position = base.transform.position + new Vector3(0f, 0f, 0f);
			ModuleAnimators[0].GetComponent<tk2dTiledSprite>().dimensions = new Vector2(Size.x * 16, 16f);
			ModuleAnimators[1].GetComponent<tk2dTiledSprite>().dimensions = new Vector2(Size.x * 16, (Size.y - 2) * 16);
			ModuleAnimators[1].transform.position = base.transform.position + new Vector3(0f, 1f, 0f);
			ModuleAnimators[2].GetComponent<tk2dTiledSprite>().dimensions = new Vector2(Size.x * 16, 16f);
			ModuleAnimators[2].transform.position = base.transform.position + new Vector3(0f, Size.y - 1, 0f);
			ShadowObjects[0].transform.position = base.transform.position + new Vector3(0f, 0f, 0f);
			ShadowObjects[1].transform.position = base.transform.position + new Vector3(0f, 1f, 0f);
			(ShadowObjects[1] as tk2dTiledSprite).dimensions = new Vector2(16f, (Size.y - 2) * 16);
			ShadowObjects[2].transform.position = base.transform.position + new Vector3(0f, Size.y - 1, 0f);
			ShadowObjects[3].transform.position = base.transform.position + new Vector3(Size.x - 1, 0f, 0f);
			ShadowObjects[4].transform.position = base.transform.position + new Vector3((float)(Size.x - 1) + 0.3125f, 1f, 0f);
			(ShadowObjects[4] as tk2dTiledSprite).dimensions = new Vector2(16f, (Size.y - 2) * 16);
			ShadowObjects[5].transform.position = base.transform.position + new Vector3(Size.x - 1, Size.y - 1, 0f);
		}
		else
		{
			ModuleAnimators[0].transform.position = base.transform.position + new Vector3(0f, 0f, 0f);
			ModuleAnimators[0].GetComponent<tk2dTiledSprite>().dimensions = new Vector2(16f, Size.y * 16);
			ModuleAnimators[1].GetComponent<tk2dTiledSprite>().dimensions = new Vector2((Size.x - 2) * 16, Size.y * 16);
			ModuleAnimators[1].transform.position = base.transform.position + new Vector3(1f, 0f, 0f);
			ModuleAnimators[2].GetComponent<tk2dTiledSprite>().dimensions = new Vector2(16f, Size.y * 16);
			ModuleAnimators[2].transform.position = base.transform.position + new Vector3(Size.x - 1, 0f, 0f);
			ShadowObjects[0].transform.position = base.transform.position + new Vector3(0f, 0f, 0f);
			ShadowObjects[1].transform.position = base.transform.position + new Vector3(1f, 0f, 0f);
			(ShadowObjects[1] as tk2dTiledSprite).dimensions = new Vector2((Size.x - 2) * 16, 16f);
			ShadowObjects[2].transform.position = base.transform.position + new Vector3(Size.x - 1, 0f, 0f);
			ShadowObjects[3].transform.position = base.transform.position + new Vector3(0f, Size.y - 1, 0f);
			ShadowObjects[4].transform.position = base.transform.position + new Vector3(1f, (float)(Size.y - 1) + 0.3125f, 0f);
			(ShadowObjects[4] as tk2dTiledSprite).dimensions = new Vector2((Size.x - 2) * 16, 16f);
			ShadowObjects[5].transform.position = base.transform.position + new Vector3(Size.x - 1, Size.y - 1, 0f);
		}
		for (int i = 0; i < ModuleAnimators.Count; i++)
		{
			ModuleAnimators[i].Sprite.UpdateZDepth();
		}
		for (int j = 0; j < ShadowObjects.Count; j++)
		{
			ShadowObjects[j].IsPerpendicular = false;
			ShadowObjects[j].UpdateZDepth();
		}
		base.specRigidbody.PrimaryPixelCollider.ManualWidth = Size.x * 16;
		base.specRigidbody.PrimaryPixelCollider.ManualHeight = Size.y * 16;
		if (IsHorizontal)
		{
			base.specRigidbody.PrimaryPixelCollider.ManualOffsetY = 4;
			base.specRigidbody.PrimaryPixelCollider.ManualHeight = base.specRigidbody.PrimaryPixelCollider.ManualHeight - 8;
		}
		else
		{
			base.specRigidbody.PrimaryPixelCollider.ManualOffsetX = 4;
			base.specRigidbody.PrimaryPixelCollider.ManualWidth = base.specRigidbody.PrimaryPixelCollider.ManualWidth - 8;
		}
		base.specRigidbody.Reinitialize();
		base.specRigidbody.RegenerateColliders = true;
		yield return null;
		base.specRigidbody.RegenerateColliders = true;
		PhysicsEngine.UpdatePosition(base.specRigidbody);
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody2.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(OnExitTrigger));
		yield return new WaitForSeconds(0.5f);
		PhysicsEngine.UpdatePosition(base.specRigidbody);
	}

	public void Update()
	{
		tk2dSpriteAnimator tk2dSpriteAnimator2;
		for (int i = 0; i < ModuleAnimators.Count; i++)
		{
			tk2dSpriteAnimator2 = ModuleAnimators[i];
			string text = PositiveVelocityAnims[i];
			string text2 = NegativeVelocityAnims[i];
			if (!tk2dSpriteAnimator2)
			{
				continue;
			}
			if (tk2dSpriteAnimator2.CurrentClip != null && tk2dSpriteAnimator2.CurrentClip.frames != null && tk2dSpriteAnimator2.CurrentClip.frames.Length > 0)
			{
				float num = 1f / ((float)(tk2dSpriteAnimator2.CurrentClip.frames.Length * 2) / tk2dSpriteAnimator2.CurrentClip.fps);
				float num2 = Velocity.magnitude / 8f / num;
				tk2dSpriteAnimator2.ClipFps = tk2dSpriteAnimator2.CurrentClip.fps * num2;
			}
			if (Velocity.x != 0f)
			{
				if (Velocity.x > 0f && !tk2dSpriteAnimator2.IsPlaying(text))
				{
					tk2dSpriteAnimator2.Play(text);
				}
				else if (Velocity.x < 0f && !tk2dSpriteAnimator2.IsPlaying(text2))
				{
					tk2dSpriteAnimator2.Play(text2);
				}
			}
			else if (Velocity.y > 0f && !tk2dSpriteAnimator2.IsPlaying(text))
			{
				tk2dSpriteAnimator2.Play(text);
			}
			else if (Velocity.y < 0f && !tk2dSpriteAnimator2.IsPlaying(text2))
			{
				tk2dSpriteAnimator2.Play(text2);
			}
		}
		tk2dSpriteAnimator2 = ModuleAnimators[0];
		int num3 = (int)tk2dSpriteAnimator2.clipTime;
		int num4 = (int)(tk2dSpriteAnimator2.clipTime + tk2dSpriteAnimator2.ClipFps * BraveTime.DeltaTime);
		int num5 = (num4 - num3) * 2;
		IntVector2 impartedPixelsToMove = IntVector2.Zero;
		if (Velocity.x != 0f)
		{
			impartedPixelsToMove = new IntVector2((!(Velocity.x > 0f)) ? (-num5) : num5, 0);
		}
		else if (Velocity.y != 0f)
		{
			impartedPixelsToMove = new IntVector2(0, (!(Velocity.y > 0f)) ? (-num5) : num5);
		}
		for (int j = 0; j < m_rigidbodiesOnPlatform.Count; j++)
		{
			if (!m_rigidbodiesOnPlatform[j] || (!GameManager.Instance.Dungeon.CellSupportsFalling(m_rigidbodiesOnPlatform[j].UnitCenter) && !base.specRigidbody.ContainsPoint(m_rigidbodiesOnPlatform[j].UnitCenter, int.MaxValue, true)))
			{
				continue;
			}
			if ((bool)m_rigidbodiesOnPlatform[j].gameActor)
			{
				if (m_rigidbodiesOnPlatform[j].gameActor.IsGrounded)
				{
					m_rigidbodiesOnPlatform[j].specRigidbody.ImpartedPixelsToMove = impartedPixelsToMove;
				}
			}
			else
			{
				m_rigidbodiesOnPlatform[j].Velocity += Velocity;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnEnterTrigger(SpeculativeRigidbody obj, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (m_rigidbodiesOnPlatform.Contains(obj))
		{
			return;
		}
		if ((bool)obj.gameActor && obj.gameActor is PlayerController)
		{
			PlayerController player = obj.gameActor as PlayerController;
			if (PassiveItem.IsFlagSetForCharacter(player, typeof(HeavyBootsItem)))
			{
				return;
			}
		}
		m_rigidbodiesOnPlatform.Add(obj);
		base.specRigidbody.RegisterCarriedRigidbody(obj);
	}

	private void OnExitTrigger(SpeculativeRigidbody obj, SpeculativeRigidbody source)
	{
		if (m_rigidbodiesOnPlatform.Contains(obj))
		{
			m_rigidbodiesOnPlatform.Remove(obj);
			if ((bool)this)
			{
				base.specRigidbody.DeregisterCarriedRigidbody(obj);
			}
		}
	}

	public void PostFieldConfiguration(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY();
		for (int i = 0; (float)i < ConveyorWidth; i++)
		{
			for (int j = 0; (float)j < ConveyorHeight; j++)
			{
				IntVector2 key = intVector + new IntVector2(i, j);
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				if (cellData != null)
				{
					cellData.containsTrap = true;
					cellData.cellVisualData.RequiresPitBordering = true;
				}
			}
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
	}
}
