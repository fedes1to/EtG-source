using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class FlippableCover : BraveBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public enum FlipStyle
	{
		ANY,
		ONLY_FLIPS_UP_DOWN,
		ONLY_FLIPS_LEFT_RIGHT,
		NO_FLIPS
	}

	public FlipStyle flipStyle;

	public tk2dSprite shadowSprite;

	public float DamageReceivedOnSlide = 30f;

	[Header("Unflipped Animations")]
	public string unflippedBreakAnimation;

	[Header("Directional Animations")]
	[HelpBox("{0} = east/west/south/north")]
	public string flipAnimation;

	public string shadowFlipAnimation;

	public string pitfallAnimation;

	public string breakAnimation;

	public BreakFrame[] prebreakFrames;

	public BreakFrame[] prebreakFramesUnflipped;

	public bool BreaksOnBreakAnimation;

	[Header("SubElements (for coffins)")]
	public List<FlippableSubElement> flipSubElements = new List<FlippableSubElement>();

	[Header("Directional Outline Sprite")]
	public GameObject outlineNorth;

	public GameObject outlineEast;

	public GameObject outlineSouth;

	public GameObject outlineWest;

	public bool UsesCustomHeightsOffGround;

	public float CustomStartHeightOffGround;

	public float CustomNorthFlippedHeightOffGround = -1.5f;

	public float CustomEastFlippedHeightOffGround = -1.5f;

	public float CustomSouthFlippedHeightOffGround = -1.5f;

	public float CustomWestFlippedHeightOffGround = -1.5f;

	public bool DelayMoveable;

	public float MoveableDelay = 1f;

	public float VibrationDelay = 0.25f;

	private SlideSurface m_slide;

	private bool m_hasRoomEnteredProcessed;

	private bool m_isGilded;

	private PlayerController m_flipperPlayer;

	protected tk2dSpriteAnimator m_shadowSpriteAnimator;

	protected OccupiedCells m_occupiedCells;

	protected MajorBreakable m_breakable;

	protected bool m_flipped;

	protected DungeonData.Direction m_flipDirection;

	protected bool m_shouldDisplayOutline;

	protected PlayerController m_lastInteractingPlayer;

	protected DungeonData.Direction m_lastOutlineDirection = (DungeonData.Direction)(-1);

	protected float m_makeBreakableTimer = -1f;

	public bool IsBroken
	{
		get
		{
			if (m_breakable == null)
			{
				return false;
			}
			return m_breakable.IsDestroyed;
		}
	}

	public bool IsFlipped
	{
		get
		{
			return m_flipped;
		}
	}

	public DungeonData.Direction DirectionFlipped
	{
		get
		{
			return m_flipDirection;
		}
	}

	public bool PreventPitFalls { get; set; }

	public bool IsGilded
	{
		get
		{
			return m_isGilded;
		}
	}

	public void Awake()
	{
		base.specRigidbody = GetComponentInChildren<SpeculativeRigidbody>();
		m_slide = GetComponentInChildren<SlideSurface>();
	}

	private void Start()
	{
		if (base.sprite == null)
		{
			base.sprite = base.transform.GetChild(0).GetComponent<tk2dSprite>();
		}
		if (base.spriteAnimator == null)
		{
			base.spriteAnimator = base.transform.GetChild(0).GetComponent<tk2dSpriteAnimator>();
		}
		base.sprite.AdditionalFlatForwardPercentage = 0.125f;
		base.sprite.IsPerpendicular = flipStyle == FlipStyle.NO_FLIPS && base.sprite.IsPerpendicular;
		base.sprite.HeightOffGround = ((!UsesCustomHeightsOffGround) ? 0f : CustomStartHeightOffGround);
		base.sprite.UpdateZDepth();
		if (shadowSprite != null)
		{
			shadowSprite.IsPerpendicular = false;
			shadowSprite.usesOverrideMaterial = true;
			shadowSprite.HeightOffGround = -1f;
			shadowSprite.UpdateZDepth();
			m_shadowSpriteAnimator = shadowSprite.GetComponent<tk2dSpriteAnimator>();
		}
		m_breakable = GetComponentInChildren<MajorBreakable>();
		if (m_breakable != null)
		{
			MajorBreakable breakable = m_breakable;
			breakable.OnDamaged = (Action<float>)Delegate.Combine(breakable.OnDamaged, new Action<float>(Damaged));
			MajorBreakable breakable2 = m_breakable;
			breakable2.OnBreak = (Action)Delegate.Combine(breakable2.OnBreak, new Action(DestroyCover));
			if (prebreakFrames.Length > 0)
			{
				m_breakable.MinHitPointsFromNonExplosions = 1f;
			}
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(speculativeRigidbody2.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(OnPostMovement));
		if (base.specRigidbody.PixelColliders.Count > 1)
		{
			base.specRigidbody.PixelColliders[1].Enabled = false;
		}
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom != null)
		{
			absoluteRoom.Entered += HandleParentRoomEntered;
			if ((bool)GameManager.Instance.BestActivePlayer && absoluteRoom == GameManager.Instance.BestActivePlayer.CurrentRoom)
			{
				m_hasRoomEnteredProcessed = true;
			}
		}
		if (flipStyle == FlipStyle.NO_FLIPS)
		{
			RemoveFromRoomHierarchy();
			base.specRigidbody.CanBePushed = true;
		}
	}

	private void HandleParentRoomEntered(PlayerController p)
	{
		if (m_hasRoomEnteredProcessed)
		{
			return;
		}
		m_hasRoomEnteredProcessed = true;
		if (!p || !p.HasActiveBonusSynergy(CustomSynergyType.GILDED_TABLES) || !(UnityEngine.Random.value < 0.15f))
		{
			return;
		}
		m_isGilded = true;
		base.sprite.usesOverrideMaterial = true;
		tk2dSprite tk2dSprite2 = base.sprite as tk2dSprite;
		tk2dSprite2.GenerateUV2 = true;
		Material material = UnityEngine.Object.Instantiate(base.sprite.renderer.material);
		material.DisableKeyword("TINTING_OFF");
		material.EnableKeyword("TINTING_ON");
		material.SetColor("_OverrideColor", new Color(1f, 0.77f, 0f));
		material.DisableKeyword("EMISSIVE_OFF");
		material.EnableKeyword("EMISSIVE_ON");
		material.SetFloat("_EmissivePower", 1.75f);
		material.SetFloat("_EmissiveColorPower", 1f);
		base.sprite.renderer.material = material;
		Shader shader = Shader.Find("Brave/ItemSpecific/MetalSkinLayerShader");
		MeshRenderer component = base.sprite.GetComponent<MeshRenderer>();
		Material[] array = component.sharedMaterials;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].shader == shader)
			{
				return;
			}
		}
		Array.Resize(ref array, array.Length + 1);
		Material material2 = new Material(shader);
		material2.SetTexture("_MainTex", array[0].GetTexture("_MainTex"));
		array[array.Length - 1] = material2;
		component.sharedMaterials = array;
		tk2dSprite2.ForceBuild();
	}

	protected void ClearOutlines()
	{
		outlineNorth.SetActive(false);
		outlineEast.SetActive(false);
		outlineSouth.SetActive(false);
		outlineWest.SetActive(false);
		m_lastOutlineDirection = (DungeonData.Direction)(-1);
	}

	protected void ToggleOutline(DungeonData.Direction dir)
	{
		if (IsBroken || flipStyle == FlipStyle.NO_FLIPS)
		{
			return;
		}
		switch (dir)
		{
		case DungeonData.Direction.NORTH:
			if (flipStyle != FlipStyle.ONLY_FLIPS_LEFT_RIGHT)
			{
				outlineNorth.SetActive(!outlineNorth.activeSelf);
			}
			break;
		case DungeonData.Direction.EAST:
			if (flipStyle != FlipStyle.ONLY_FLIPS_UP_DOWN)
			{
				outlineEast.SetActive(!outlineEast.activeSelf);
			}
			break;
		case DungeonData.Direction.SOUTH:
			if (flipStyle != FlipStyle.ONLY_FLIPS_LEFT_RIGHT)
			{
				outlineSouth.SetActive(!outlineSouth.activeSelf);
			}
			break;
		case DungeonData.Direction.WEST:
			if (flipStyle != FlipStyle.ONLY_FLIPS_UP_DOWN)
			{
				outlineWest.SetActive(!outlineWest.activeSelf);
			}
			break;
		}
		base.sprite.UpdateZDepth();
	}

	private void Update()
	{
		if (base.spriteAnimator.IsPlaying(base.spriteAnimator.CurrentClip))
		{
			base.spriteAnimator.ForceInvisibleSpriteUpdate();
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.ForceRegenerate();
			}
		}
		if (m_shouldDisplayOutline)
		{
			DungeonData.Direction inverseDirection = DungeonData.GetInverseDirection(GetFlipDirection(m_lastInteractingPlayer.specRigidbody));
			if (inverseDirection != m_lastOutlineDirection)
			{
				ToggleOutline(m_lastOutlineDirection);
				ToggleOutline(inverseDirection);
			}
			m_lastOutlineDirection = inverseDirection;
		}
		if (!(m_makeBreakableTimer > 0f))
		{
			return;
		}
		m_makeBreakableTimer -= BraveTime.DeltaTime;
		if (m_makeBreakableTimer <= 0f)
		{
			m_breakable.MinHitPointsFromNonExplosions = 0f;
			if (!m_flipped && (bool)m_breakable && !GameManager.Instance.InTutorial)
			{
				m_breakable.ApplyDamage(DamageReceivedOnSlide, Vector2.zero, false);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.specRigidbody == null || base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.sprite.GetBounds().size);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		m_lastInteractingPlayer = interactor;
		if ((bool)this)
		{
			m_shouldDisplayOutline = true;
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			ClearOutlines();
			m_shouldDisplayOutline = false;
		}
	}

	public void Interact(PlayerController player)
	{
		Flip(player.specRigidbody);
		player.DoVibration(Vibration.Time.Quick, Vibration.Strength.UltraLight);
		ClearOutlines();
		m_shouldDisplayOutline = false;
	}

	public DungeonData.Direction GetFlipDirection(SpeculativeRigidbody flipperRigidbody)
	{
		bool flag = flipperRigidbody.UnitRight <= base.specRigidbody.UnitLeft;
		bool flag2 = flipperRigidbody.UnitLeft >= base.specRigidbody.UnitRight;
		bool flag3 = flipperRigidbody.UnitBottom >= base.specRigidbody.UnitTop;
		bool flag4 = flipperRigidbody.UnitTop <= base.specRigidbody.UnitBottom;
		if (flag && !flag3 && !flag4)
		{
			return DungeonData.Direction.EAST;
		}
		if (flag2 && !flag3 && !flag4)
		{
			return DungeonData.Direction.WEST;
		}
		if (flag3 && !flag && !flag2)
		{
			return DungeonData.Direction.SOUTH;
		}
		if (flag4 && !flag && !flag2)
		{
			return DungeonData.Direction.NORTH;
		}
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		PlayerController component = flipperRigidbody.GetComponent<PlayerController>();
		bool flag5 = (bool)component && component.IsSlidingOverSurface;
		if (flag && flag3)
		{
			vector = flipperRigidbody.UnitBottomRight;
			vector2 = base.specRigidbody.UnitTopLeft;
		}
		else if (flag2 && flag3)
		{
			vector = flipperRigidbody.UnitBottomLeft;
			vector2 = base.specRigidbody.UnitTopRight;
		}
		else if (flag && flag4)
		{
			vector = flipperRigidbody.UnitTopRight;
			vector2 = base.specRigidbody.UnitBottomLeft;
		}
		else if (flag2 && flag4)
		{
			vector = flipperRigidbody.UnitTopLeft;
			vector2 = base.specRigidbody.UnitBottomRight;
		}
		else if ((bool)m_slide && flag5)
		{
			vector = flipperRigidbody.UnitCenter;
			vector2 = base.specRigidbody.UnitCenter;
		}
		else
		{
			Debug.LogError("Something about this table and flipper is TOTALLY WRONG MAN (way #1)");
		}
		Vector2 vector3 = vector - vector2;
		if (vector3 == Vector2.zero)
		{
			if (flag4)
			{
				return DungeonData.Direction.NORTH;
			}
			if (flag3)
			{
				return DungeonData.Direction.SOUTH;
			}
		}
		if ((bool)m_slide && flag5)
		{
			vector3 = -component.Velocity;
			if (!component.IsSlidingOverSurface)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_SLID_OVER_TABLE, 1f);
			}
			component.IsSlidingOverSurface = true;
			if (!component.TablesDamagedThisSlide.Contains(this))
			{
				component.TablesDamagedThisSlide.Add(this);
				if ((bool)m_breakable && !GameManager.Instance.InTutorial)
				{
					m_breakable.ApplyDamage(DamageReceivedOnSlide, Vector2.zero, false);
				}
			}
		}
		Vector2 majorAxis = BraveUtility.GetMajorAxis(vector3);
		if (majorAxis.x < 0f)
		{
			return DungeonData.Direction.EAST;
		}
		if (majorAxis.x > 0f)
		{
			return DungeonData.Direction.WEST;
		}
		if (majorAxis.y < 0f)
		{
			return DungeonData.Direction.NORTH;
		}
		if (majorAxis.y > 0f)
		{
			return DungeonData.Direction.SOUTH;
		}
		Debug.LogError("Something about this table and flipper is TOTALLY WRONG MAN (way #2)");
		return DungeonData.Direction.NORTH;
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		switch (GetFlipDirection(interactor.specRigidbody))
		{
		case DungeonData.Direction.EAST:
			return "tablekick_right";
		case DungeonData.Direction.WEST:
			shouldBeFlipped = true;
			return "tablekick_right";
		case DungeonData.Direction.NORTH:
			return "tablekick_up";
		case DungeonData.Direction.SOUTH:
			return "tablekick_down";
		default:
			return "error";
		}
	}

	private void MakePerpendicularOnFlipped(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		base.sprite.IsPerpendicular = true;
		if (m_flipDirection == DungeonData.Direction.NORTH || m_flipDirection == DungeonData.Direction.SOUTH)
		{
			float num = ((m_flipDirection != 0) ? CustomSouthFlippedHeightOffGround : CustomNorthFlippedHeightOffGround);
			base.sprite.HeightOffGround = ((!UsesCustomHeightsOffGround) ? (-1.5f) : num);
			if (shadowSprite != null)
			{
				shadowSprite.HeightOffGround = ((!UsesCustomHeightsOffGround) ? (-1.5f) : (-1.75f));
			}
		}
		else
		{
			float num2 = ((m_flipDirection != DungeonData.Direction.EAST) ? CustomWestFlippedHeightOffGround : CustomEastFlippedHeightOffGround);
			base.sprite.HeightOffGround = ((!UsesCustomHeightsOffGround) ? (-1f) : num2);
			if (shadowSprite != null)
			{
				shadowSprite.HeightOffGround = -1.5f;
			}
		}
		base.sprite.UpdateZDepth();
		if (shadowSprite != null)
		{
			shadowSprite.UpdateZDepth();
		}
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(MakePerpendicularOnFlipped));
	}

	private IEnumerator DelayedMakePerpendicular(float time)
	{
		yield return new WaitForSeconds(time);
		MakePerpendicularOnFlipped(null, null);
	}

	private IEnumerator DelayedBreakBreakables(float time)
	{
		yield return new WaitForSeconds(time);
		BreakBreakablesFlippedUpon(m_flipDirection);
	}

	public void Flip(DungeonData.Direction flipDirection)
	{
		if (IsFlipped)
		{
			return;
		}
		if (GameManager.Instance.InTutorial)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerFlippedTable");
		}
		AkSoundEngine.PostEvent("Play_OBJ_table_flip_01", base.gameObject);
		GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY()).DeregisterInteractable(this);
		if (m_breakable != null)
		{
			m_breakable.TriggerTemporaryDestructibleVFXClear();
		}
		m_flipDirection = flipDirection;
		if (!string.IsNullOrEmpty(flipAnimation))
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(FlipCompleted));
			base.spriteAnimator.Play(GetAnimName(flipAnimation, m_flipDirection));
			if (m_flipDirection == DungeonData.Direction.NORTH)
			{
				MakePerpendicularOnFlipped(null, null);
			}
			else
			{
				StartCoroutine(DelayedMakePerpendicular(base.spriteAnimator.CurrentClip.BaseClipLength / 2.25f));
			}
			StartCoroutine(DelayedBreakBreakables(base.spriteAnimator.CurrentClip.BaseClipLength / 2f));
			if (m_flipDirection == DungeonData.Direction.SOUTH)
			{
				base.sprite.IsPerpendicular = true;
			}
		}
		else
		{
			base.sprite.IsPerpendicular = true;
			if (m_flipDirection == DungeonData.Direction.NORTH || m_flipDirection == DungeonData.Direction.SOUTH)
			{
				float num = ((m_flipDirection != 0) ? CustomSouthFlippedHeightOffGround : CustomNorthFlippedHeightOffGround);
				base.sprite.HeightOffGround = ((!UsesCustomHeightsOffGround) ? (-1.5f) : num);
			}
			BreakBreakablesFlippedUpon(m_flipDirection);
			FlipCompleted(null, null);
		}
		if ((bool)m_flipperPlayer && m_flipperPlayer.OnTableFlipped != null)
		{
			m_flipperPlayer.OnTableFlipped(this);
		}
		if (!string.IsNullOrEmpty(shadowFlipAnimation) && m_shadowSpriteAnimator != null)
		{
			m_shadowSpriteAnimator.Play(GetAnimName(shadowFlipAnimation, m_flipDirection));
		}
		bool flag = false;
		for (int i = 0; i < flipSubElements.Count; i++)
		{
			if ((!flipSubElements[i].isMandatory && !(UnityEngine.Random.value < flipSubElements[i].spawnChance)) || (flipSubElements[i].requiresDirection && flipSubElements[i].requiredDirection != flipDirection))
			{
				continue;
			}
			if (flipSubElements[i].onlyOneOfThese)
			{
				if (flag)
				{
					continue;
				}
				flag = true;
			}
			StartCoroutine(ProcessSubElement(flipSubElements[i], flipDirection));
		}
		m_occupiedCells.UpdateCells();
		if (DelayMoveable)
		{
			StartCoroutine(HandleDelayedMoveability());
		}
		else
		{
			base.specRigidbody.CanBePushed = true;
		}
		if ((bool)m_flipperPlayer)
		{
			StartCoroutine(HandleDelayedVibration(m_flipperPlayer));
		}
		if (base.specRigidbody.PixelColliders.Count >= 2)
		{
			base.specRigidbody.PixelColliders[1].Enabled = true;
		}
		m_flipped = true;
		base.sprite.UpdateZDepth();
		if ((bool)shadowSprite)
		{
			shadowSprite.UpdateZDepth();
		}
		SurfaceDecorator component = GetComponent<SurfaceDecorator>();
		if (component != null)
		{
			component.Destabilize(DungeonData.GetIntVector2FromDirection(m_flipDirection).ToVector2());
		}
	}

	private IEnumerator HandleDelayedMoveability()
	{
		yield return new WaitForSeconds(MoveableDelay);
		base.specRigidbody.CanBePushed = true;
	}

	private IEnumerator HandleDelayedVibration(PlayerController player)
	{
		yield return new WaitForSeconds(VibrationDelay);
		if ((bool)player)
		{
			player.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
		}
	}

	private IEnumerator ProcessSubElement(FlippableSubElement element, DungeonData.Direction flipDirection)
	{
		yield return new WaitForSeconds(element.flipDelay);
		element.Trigger(flipDirection, base.sprite);
	}

	public void ForceSetFlipper(PlayerController flipper)
	{
		m_flipperPlayer = flipper;
	}

	public void Flip(SpeculativeRigidbody flipperRigidbody)
	{
		if (IsFlipped)
		{
			return;
		}
		base.specRigidbody.PixelColliders[1].Enabled = true;
		RemoveFromRoomHierarchy();
		DungeonData.Direction flipDirection = GetFlipDirection(flipperRigidbody);
		if (flipStyle == FlipStyle.NO_FLIPS)
		{
			return;
		}
		if (flipStyle == FlipStyle.ONLY_FLIPS_LEFT_RIGHT)
		{
			if (flipDirection == DungeonData.Direction.NORTH || flipDirection == DungeonData.Direction.SOUTH)
			{
				return;
			}
		}
		else if (flipStyle == FlipStyle.ONLY_FLIPS_UP_DOWN && (flipDirection == DungeonData.Direction.EAST || flipDirection == DungeonData.Direction.WEST))
		{
			return;
		}
		AkSoundEngine.PostEvent("Play_OBJ_table_flip_01", base.gameObject);
		if (m_breakable != null)
		{
			m_breakable.TriggerTemporaryDestructibleVFXClear();
		}
		if ((bool)flipperRigidbody.gameActor && flipperRigidbody.gameActor is PlayerController)
		{
			m_flipperPlayer = flipperRigidbody.gameActor as PlayerController;
			ForceBlank(2f);
			m_flipperPlayer.healthHaver.TriggerInvulnerabilityPeriod();
		}
		Flip(flipDirection);
		GameActor gameActor = flipperRigidbody.gameActor;
		if (gameActor is PlayerController)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.TABLES_FLIPPED, 1f);
		}
	}

	private void FlipCompleted(tk2dSpriteAnimator tk2DSpriteAnimator, tk2dSpriteAnimationClip tk2DSpriteAnimationClip)
	{
		m_occupiedCells.UpdateCells();
		base.sprite.UpdateZDepth();
		if ((bool)m_flipperPlayer && m_flipperPlayer.OnTableFlipCompleted != null)
		{
			m_flipperPlayer.OnTableFlipCompleted(this);
		}
		if (m_isGilded)
		{
			RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
			if (absoluteRoom != null)
			{
				List<AIActor> activeEnemies = absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if ((bool)activeEnemies[i] && activeEnemies[i].IsNormalEnemy)
					{
						activeEnemies[i].AssignedCurrencyToDrop += UnityEngine.Random.Range(2, 6);
					}
				}
			}
			m_isGilded = false;
		}
		tk2DSpriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(tk2DSpriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(FlipCompleted));
	}

	private void BreakBreakablesFlippedUpon(DungeonData.Direction flipDirection)
	{
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		for (int i = 0; i < StaticReferenceManager.AllMinorBreakables.Count; i++)
		{
			if (!StaticReferenceManager.AllMinorBreakables[i].IsBroken && !StaticReferenceManager.AllMinorBreakables[i].debris && StaticReferenceManager.AllMinorBreakables[i].transform.position.GetAbsoluteRoom() == absoluteRoom)
			{
				SpeculativeRigidbody speculativeRigidbody = StaticReferenceManager.AllMinorBreakables[i].specRigidbody;
				if ((bool)speculativeRigidbody && (bool)base.specRigidbody && BraveMathCollege.DistBetweenRectangles(speculativeRigidbody.UnitBottomLeft, speculativeRigidbody.UnitDimensions, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions) < 0.5f)
				{
					StaticReferenceManager.AllMinorBreakables[i].Break();
				}
			}
		}
	}

	private void RemoveFromRoomHierarchy()
	{
		Transform hierarchyParent = base.transform.position.GetAbsoluteRoom().hierarchyParent;
		Transform parent = base.transform;
		while (parent.parent != null)
		{
			if (parent.parent == hierarchyParent)
			{
				parent.parent = null;
				break;
			}
			parent = parent.parent;
		}
	}

	public void Damaged(float damage)
	{
		if (m_flipped || prebreakFramesUnflipped == null || prebreakFramesUnflipped.Length == 0)
		{
			for (int num = prebreakFrames.Length - 1; num >= 0; num--)
			{
				if (m_breakable.GetCurrentHealthPercentage() <= prebreakFrames[num].healthPercentage / 100f)
				{
					if (m_flipped)
					{
						base.sprite.SetSprite(GetAnimName(prebreakFrames[num].sprite, m_flipDirection));
					}
					if (num == prebreakFrames.Length - 1 && m_makeBreakableTimer <= 0f)
					{
						m_makeBreakableTimer = 0.5f;
					}
					break;
				}
			}
			return;
		}
		for (int num2 = prebreakFramesUnflipped.Length - 1; num2 >= 0; num2--)
		{
			if (m_breakable.GetCurrentHealthPercentage() <= prebreakFramesUnflipped[num2].healthPercentage / 100f)
			{
				string text = prebreakFramesUnflipped[num2].sprite;
				base.sprite.SetSprite(text);
				if (num2 == prebreakFramesUnflipped.Length - 1 && m_makeBreakableTimer <= 0f)
				{
					m_makeBreakableTimer = 0.5f;
				}
				break;
			}
		}
	}

	public void DestroyCover()
	{
		DestroyCover(false, null);
	}

	public void DestroyCover(bool fellInPit, IntVector2? pushDirection)
	{
		if (!m_flipped)
		{
			SurfaceDecorator component = GetComponent<SurfaceDecorator>();
			if (component != null)
			{
				component.Destabilize(Vector2.zero);
			}
			ClearOutlines();
			GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY()).DeregisterInteractable(this);
		}
		m_occupiedCells.Clear();
		if (fellInPit && pushDirection.HasValue)
		{
			StartCoroutine(StartFallAnimation(pushDirection.Value));
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.PUSH_TABLE_INTO_PIT);
		}
		else if (!m_flipped && base.spriteAnimator.GetClipByName(unflippedBreakAnimation) == null)
		{
			LootEngine.DoDefaultPurplePoof(base.sprite.WorldCenter);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			base.spriteAnimator.Play(m_flipped ? GetAnimName(breakAnimation, m_flipDirection) : unflippedBreakAnimation);
			if (BreaksOnBreakAnimation)
			{
				tk2dSpriteAnimator obj = base.spriteAnimator;
				obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DestroyBrokenTable));
			}
		}
		if ((bool)shadowSprite)
		{
			shadowSprite.renderer.enabled = false;
		}
		base.sprite.IsPerpendicular = false;
		base.sprite.HeightOffGround = -1.25f;
		base.sprite.UpdateZDepth();
		ForceBlank(2f);
	}

	private void DestroyBrokenTable(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void ForceBlank(float overrideRadius = 25f, float overrideTimeAtMaxRadius = 0.5f)
	{
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		silencerInstance.ForceNoDamage = true;
		silencerInstance.TriggerSilencer(base.specRigidbody.UnitCenter, 50f, overrideRadius, null, 0f, 0f, 0f, 0f, 0f, 0f, overrideTimeAtMaxRadius, null, false, true);
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		base.specRigidbody.Initialize();
		m_occupiedCells = new OccupiedCells(base.specRigidbody, room);
	}

	protected virtual void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		PlayerController component = rigidbodyCollision.OtherRigidbody.GetComponent<PlayerController>();
		if (component != null && rigidbodyCollision.Overlap)
		{
			component.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
		}
	}

	protected virtual void OnPostMovement(SpeculativeRigidbody rigidbody, Vector2 unitOffset, IntVector2 pixelOffset)
	{
		if (pixelOffset != IntVector2.Zero)
		{
			CheckForPitDeath(pixelOffset);
		}
		if (rigidbody.CanBePushed && (bool)base.sprite)
		{
			base.sprite.UpdateZDepth();
		}
		if ((bool)shadowSprite)
		{
			shadowSprite.transform.localPosition = base.sprite.transform.localPosition;
			shadowSprite.UpdateZDepth();
		}
		if (unitOffset != Vector2.zero)
		{
			m_occupiedCells.UpdateCells();
		}
	}

	private string GetAnimName(string name, DungeonData.Direction dir)
	{
		if (name.Contains("{0}"))
		{
			return string.Format(name, dir.ToString().ToLower());
		}
		return name;
	}

	private void CheckForPitDeath(IntVector2 dir)
	{
		if (base.specRigidbody.PixelColliders.Count != 0 && !PreventPitFalls)
		{
			Rect rect = default(Rect);
			rect.min = base.specRigidbody.PixelColliders[0].UnitBottomLeft;
			rect.max = base.specRigidbody.PixelColliders[0].UnitTopRight;
			for (int i = 1; i < base.specRigidbody.PixelColliders.Count; i++)
			{
				rect.min = Vector2.Min(rect.min, base.specRigidbody.PixelColliders[i].UnitBottomLeft);
				rect.max = Vector2.Max(rect.max, base.specRigidbody.PixelColliders[i].UnitTopRight);
			}
			Dungeon dungeon = GameManager.Instance.Dungeon;
			List<IntVector2> list = new List<IntVector2>();
			if (dungeon.CellSupportsFalling(new Vector2(rect.xMin, rect.yMin)) && dungeon.CellSupportsFalling(new Vector2(rect.xMin, rect.yMax)) && dungeon.CellSupportsFalling(new Vector2(rect.center.x, rect.yMin)) && dungeon.CellSupportsFalling(new Vector2(rect.center.x, rect.yMax)))
			{
				list.Add(IntVector2.Left);
			}
			else if (dungeon.CellSupportsFalling(new Vector2(rect.xMax, rect.yMin)) && dungeon.CellSupportsFalling(new Vector2(rect.xMax, rect.yMax)) && dungeon.CellSupportsFalling(new Vector2(rect.center.x, rect.yMin)) && dungeon.CellSupportsFalling(new Vector2(rect.center.x, rect.yMax)))
			{
				list.Add(IntVector2.Right);
			}
			else if (dungeon.CellSupportsFalling(new Vector2(rect.xMin, rect.yMax)) && dungeon.CellSupportsFalling(new Vector2(rect.xMax, rect.yMax)) && dungeon.CellSupportsFalling(new Vector2(rect.xMin, rect.center.y)) && dungeon.CellSupportsFalling(new Vector2(rect.xMax, rect.center.y)))
			{
				list.Add(IntVector2.Up);
			}
			else if (dungeon.CellSupportsFalling(new Vector2(rect.xMin, rect.yMin)) && dungeon.CellSupportsFalling(new Vector2(rect.xMax, rect.yMin)) && dungeon.CellSupportsFalling(new Vector2(rect.xMin, rect.center.y)) && dungeon.CellSupportsFalling(new Vector2(rect.xMax, rect.center.y)))
			{
				list.Add(IntVector2.Down);
			}
			if (list.Count > 0)
			{
				IntVector2 value = ((!list.Contains(dir.MajorAxis)) ? list[0] : dir.MajorAxis);
				DestroyCover(true, value);
			}
		}
	}

	private IEnumerator StartFallAnimation(IntVector2 dir)
	{
		base.specRigidbody.enabled = false;
		base.sprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
		float duration = 0.5f;
		float rotation = ((dir.x == 0) ? 0f : ((0f - Mathf.Sign(dir.x)) * 135f));
		Vector3 velocity = dir.ToVector3() * 1.25f / duration;
		Vector3 acceleration = new Vector3(0f, -10f, 0f);
		Vector3 cachedVector = base.sprite.transform.position;
		base.transform.position = base.sprite.WorldCenter;
		base.sprite.transform.position = cachedVector;
		float timer = 0f;
		while (timer < duration)
		{
			base.transform.position += velocity * BraveTime.DeltaTime;
			base.transform.eulerAngles = base.transform.eulerAngles.WithZ(Mathf.Lerp(0f, rotation, timer / duration));
			base.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, timer / duration);
			yield return null;
			timer += BraveTime.DeltaTime;
			velocity += acceleration * BraveTime.DeltaTime;
		}
		GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(base.transform.position);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
