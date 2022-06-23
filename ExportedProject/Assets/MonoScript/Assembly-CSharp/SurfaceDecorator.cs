using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class SurfaceDecorator : BraveBehaviour
{
	internal class ObjectPlacementData
	{
		public GameObject o;

		public bool rotated;

		public bool horizontalFlip;

		public bool verticalFlip;

		public ObjectPlacementData(GameObject obj)
		{
			o = obj;
		}
	}

	public GenericLootTable tableTable;

	public float chanceToDecorate = 1f;

	public IntVector2 localSurfaceOrigin;

	public IntVector2 localSurfaceDimensions;

	public float heightOffGround = 0.5f;

	public tk2dSprite parentSprite;

	private List<GameObject> m_surfaceObjects;

	private List<tk2dSprite> m_attachedSprites;

	private RoomHandler m_parentRoom;

	private bool m_destabilized;

	private const int PRIMARY_PIXEL_BUFFER = 1;

	public bool IsDestabilized
	{
		get
		{
			return m_destabilized;
		}
	}

	private ObjectPlacementData GetSurfaceObject(Vector2 availableSpace, out Vector2 objectDimensions, out Vector2 localOrigin)
	{
		List<GameObject> list = new List<GameObject>();
		bool flag = false;
		int num = 0;
		GenericLootTable overrideTableTable = tableTable;
		if (m_parentRoom.RoomMaterial.overrideTableTable != null)
		{
			overrideTableTable = m_parentRoom.RoomMaterial.overrideTableTable;
		}
		while (!flag && num < 1000)
		{
			GameObject gameObject = overrideTableTable.SelectByWeightWithoutDuplicates(list);
			if (gameObject == null)
			{
				break;
			}
			list.Add(gameObject);
			DebrisObject component = gameObject.GetComponent<DebrisObject>();
			MinorBreakableGroupManager component2 = gameObject.GetComponent<MinorBreakableGroupManager>();
			if (component2 != null)
			{
				objectDimensions = component2.GetDimensions();
				if (objectDimensions == Vector2.zero)
				{
					continue;
				}
				localOrigin = Vector2.zero;
			}
			else
			{
				tk2dSprite component3 = gameObject.GetComponent<tk2dSprite>();
				Bounds bounds = component3.GetBounds();
				objectDimensions = new Vector2(bounds.size.x, bounds.size.y);
				localOrigin = bounds.min.XY();
			}
			bool flag2 = objectDimensions.x <= availableSpace.x && objectDimensions.y <= availableSpace.y;
			bool flag3 = component != null && component.placementOptions.canBeRotated && objectDimensions.x <= availableSpace.y && objectDimensions.y <= availableSpace.x;
			if (flag2 || flag3)
			{
				flag = true;
				ObjectPlacementData objectPlacementData = new ObjectPlacementData(gameObject);
				if (flag2 && flag3)
				{
					objectPlacementData.rotated = Random.value > 0.5f;
				}
				else
				{
					objectPlacementData.rotated = ((!flag2) ? true : false);
				}
				if (objectPlacementData.rotated)
				{
					objectDimensions = new Vector2(objectDimensions.y, objectDimensions.x);
					localOrigin = new Vector2(localOrigin.y, localOrigin.x);
				}
				if (component != null && component.placementOptions.canBeFlippedHorizontally)
				{
					objectPlacementData.horizontalFlip = Random.value > 0.5f;
				}
				if (component != null && component.placementOptions.canBeFlippedVertically)
				{
					objectPlacementData.verticalFlip = Random.value > 0.5f;
				}
				return objectPlacementData;
			}
			num++;
		}
		objectDimensions = new Vector2(float.MaxValue, float.MaxValue);
		localOrigin = Vector2.zero;
		return null;
	}

	public void RegisterAdditionalObject(GameObject o)
	{
		if (m_surfaceObjects.Contains(o))
		{
			return;
		}
		tk2dSprite[] componentsInChildren = o.GetComponentsInChildren<tk2dSprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!m_attachedSprites.Contains(componentsInChildren[i]) && componentsInChildren[i].attachParent == null)
			{
				componentsInChildren[i].HeightOffGround = 0.1f;
				m_attachedSprites.Add(componentsInChildren[i]);
			}
		}
		m_surfaceObjects.Add(o);
	}

	private void PostProcessObject(GameObject placedObject, ObjectPlacementData placementData)
	{
		MinorBreakableGroupManager component = placedObject.GetComponent<MinorBreakableGroupManager>();
		if (component != null)
		{
			component.Initialize();
			tk2dSprite[] componentsInChildren = component.GetComponentsInChildren<tk2dSprite>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].attachParent == null)
				{
					componentsInChildren[i].HeightOffGround = 0.1f;
					parentSprite.AttachRenderer(componentsInChildren[i]);
					m_attachedSprites.Add(componentsInChildren[i]);
				}
			}
			MinorBreakable[] componentsInChildren2 = component.GetComponentsInChildren<MinorBreakable>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].IgnoredForPotShotsModifier = true;
				componentsInChildren2[j].heightOffGround = 0.75f;
				componentsInChildren2[j].isImpermeableToGameActors = true;
				componentsInChildren2[j].parentSurface = this;
				componentsInChildren2[j].specRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.BulletBreakable;
			}
			DebrisObject[] componentsInChildren3 = component.GetComponentsInChildren<DebrisObject>();
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].InitializeForCollisions();
				componentsInChildren3[k].additionalHeightBoost = 0.25f;
			}
		}
		else
		{
			tk2dSprite component2 = placedObject.GetComponent<tk2dSprite>();
			component2.HeightOffGround = 0.1f;
			if (!placementData.rotated)
			{
				if (placementData.horizontalFlip)
				{
					Vector2 vector = component2.GetBounds().min.XY();
					component2.FlipX = true;
					Vector2 vector2 = component2.GetBounds().min.XY();
					Vector2 vector3 = vector2 - vector;
					component2.transform.position = component2.transform.position - vector3.ToVector3ZUp();
				}
				if (placementData.verticalFlip)
				{
					Vector2 vector4 = component2.GetBounds().min.XY();
					component2.FlipY = true;
					Vector2 vector5 = component2.GetBounds().min.XY();
					Vector2 vector6 = vector5 - vector4;
					component2.transform.position = component2.transform.position - vector6.ToVector3ZUp();
				}
			}
			parentSprite.AttachRenderer(component2);
			m_attachedSprites.Add(component2);
			MinorBreakable component3 = placedObject.GetComponent<MinorBreakable>();
			if (component3 != null)
			{
				component3.IgnoredForPotShotsModifier = true;
				component3.heightOffGround = 0.75f;
				component3.isImpermeableToGameActors = true;
				component3.parentSurface = this;
				component3.specRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.BulletBreakable;
			}
			DebrisObject component4 = placedObject.GetComponent<DebrisObject>();
			if (component4 != null)
			{
				component4.InitializeForCollisions();
				component4.additionalHeightBoost = 0.25f;
			}
		}
		GenericDecorator[] componentsInChildren4 = placedObject.GetComponentsInChildren<GenericDecorator>();
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			componentsInChildren4[l].parentSurface = this;
			componentsInChildren4[l].ConfigureOnPlacement(m_parentRoom);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void Decorate(RoomHandler parentRoom)
	{
		if (GameManager.Options.DebrisQuantity == GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			return;
		}
		IntVector2 intVector = base.transform.position.IntXY();
		if ((GameManager.Instance.Dungeon.data.CheckInBounds(intVector) && GameManager.Instance.Dungeon.data[intVector].cellVisualData.containsObjectSpaceStamp && GameManager.Instance.Dungeon.data[intVector].cellVisualData.containsWallSpaceStamp) || !(Random.value < chanceToDecorate))
		{
			return;
		}
		m_parentRoom = parentRoom;
		if (parentSprite == null)
		{
			parentSprite = GetComponent<tk2dSprite>();
		}
		if (tableTable == null)
		{
			BraveUtility.Log("Trying to decorate a SurfaceDecorator at: " + base.gameObject.name + " and failing.", Color.red, BraveUtility.LogVerbosity.CHATTY);
			return;
		}
		m_surfaceObjects = new List<GameObject>();
		m_attachedSprites = new List<tk2dSprite>();
		bool flag = ((localSurfaceDimensions.x >= localSurfaceDimensions.y) ? true : false);
		float num = 0f;
		float num2 = 0f;
		float num3 = PhysicsEngine.PixelToUnit((!flag) ? localSurfaceDimensions.y : localSurfaceDimensions.x);
		float num4 = PhysicsEngine.PixelToUnit((!flag) ? localSurfaceDimensions.x : localSurfaceDimensions.y);
		float num5 = PhysicsEngine.PixelToUnit((!flag) ? localSurfaceOrigin.y : localSurfaceOrigin.x);
		float num6 = PhysicsEngine.PixelToUnit((!flag) ? localSurfaceOrigin.x : localSurfaceOrigin.y);
		float num8;
		for (; num < num3; num += num8)
		{
			float num7 = num3 - num;
			num8 = 0f;
			num2 = 0f;
			int num9 = 0;
			float num10 = 0f;
			List<GameObject> list = new List<GameObject>();
			while (num2 < num4)
			{
				float num11 = 1f - 1f / Mathf.Pow(2f, num9);
				float num12 = num4 - num2;
				Vector2 objectDimensions = Vector2.zero;
				Vector2 localOrigin = Vector2.zero;
				Vector2 availableSpace = ((!flag) ? new Vector2(num12, num7) : new Vector2(num7, num12));
				ObjectPlacementData objectPlacementData = null;
				if (Random.value > num11)
				{
					objectPlacementData = GetSurfaceObject(availableSpace, out objectDimensions, out localOrigin);
				}
				if (objectPlacementData == null)
				{
					num10 = num4 - num2;
					num2 = num4;
					break;
				}
				GameObject o = objectPlacementData.o;
				float num13 = ((!flag) ? objectDimensions.y : objectDimensions.x);
				float num14 = ((!flag) ? objectDimensions.x : objectDimensions.y);
				if (num13 <= num7 && num14 <= num12)
				{
					Vector3 vector = ((!flag) ? (base.transform.position + new Vector3(num6 + num2, num5 + num, -0.5f)) : (base.transform.position + new Vector3(num5 + num, num6 + num2, -0.5f)));
					Vector3 position = vector - localOrigin.ToVector3ZUp();
					if (objectPlacementData.rotated)
					{
						position += new Vector3(objectDimensions.x, 0f, 0f);
					}
					GameObject gameObject = SpawnManager.SpawnDebris(o, position, (!objectPlacementData.rotated) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f));
					PostProcessObject(gameObject, objectPlacementData);
					m_surfaceObjects.Add(gameObject);
					list.Add(gameObject);
					num9++;
				}
				num8 = Mathf.Max(num8, num13);
				num2 += num14;
				num10 = num4 - num2;
			}
			if (num8 == 0f)
			{
				num = num3;
			}
			else
			{
				num8 += 1f / (float)PhysicsEngine.Instance.PixelsPerUnit;
			}
			int num15 = Mathf.FloorToInt(num10 / 0.0625f);
			if (num15 >= 2)
			{
				int num16 = Mathf.FloorToInt((float)num15 / 2f);
				int num17 = Mathf.FloorToInt((float)num15 / 3f);
				int num18 = Random.Range(-1 * num17, num17 + 1);
				IntVector2 offset = ((!flag) ? new IntVector2(num16 + num18, 0) : new IntVector2(0, num16 + num18));
				for (int i = 0; i < list.Count; i++)
				{
					list[i].transform.MovePixelsLocal(offset);
				}
			}
		}
		parentSprite.UpdateZDepth();
	}

	private float GetForceMultiplier(Vector3 objectPosition, Vector2 forceDirection)
	{
		bool flag = ((localSurfaceDimensions.x >= localSurfaceDimensions.y) ? true : false);
		bool flag2 = ((Mathf.Abs(forceDirection.x) > Mathf.Abs(forceDirection.y)) ? true : false);
		if (flag2 != flag)
		{
			return 1f;
		}
		Vector2 vector = PhysicsEngine.PixelToUnit(localSurfaceOrigin);
		Vector2 vector2 = PhysicsEngine.PixelToUnit(localSurfaceDimensions);
		Vector2 vector3 = new Vector2((objectPosition.x - (base.transform.position.x + vector.x)) / vector2.x, (objectPosition.y - (base.transform.position.y + vector.y)) / vector2.y);
		if (forceDirection.x > 0f)
		{
			vector3.x = 1f - vector3.x;
		}
		if (forceDirection.y > 0f)
		{
			vector3.y = 1f - vector3.y;
		}
		float t = ((!flag) ? Mathf.Clamp01(vector3.y) : Mathf.Clamp01(vector3.x));
		return Mathf.Lerp(1f, 1.5f, t);
	}

	public void Destabilize(Vector2 direction)
	{
		if (m_destabilized)
		{
			return;
		}
		m_destabilized = true;
		if (parentSprite == null)
		{
			parentSprite = GetComponent<tk2dSprite>();
		}
		Vector2 zero = Vector2.zero;
		float num = 0f;
		if (direction.x > 0f)
		{
			zero += new Vector2(5f, 0f);
			num = 0.5f;
		}
		if (direction.x < 0f)
		{
			zero += new Vector2(-5f, 0f);
			num = 0.5f;
		}
		if (direction.y > 0f)
		{
			zero += new Vector2(0f, 9f);
			num = -0.25f;
		}
		if (direction.y < 0f)
		{
			zero += new Vector2(0f, -5f);
		}
		if (m_attachedSprites != null)
		{
			for (int i = 0; i < m_attachedSprites.Count; i++)
			{
				if ((bool)m_attachedSprites[i])
				{
					parentSprite.DetachRenderer(m_attachedSprites[i]);
					m_attachedSprites[i].attachParent = null;
					m_attachedSprites[i].IsPerpendicular = true;
				}
			}
		}
		if (m_surfaceObjects == null)
		{
			return;
		}
		for (int j = 0; j < m_surfaceObjects.Count; j++)
		{
			if (!m_surfaceObjects[j])
			{
				continue;
			}
			float forceMultiplier = GetForceMultiplier(m_surfaceObjects[j].transform.position, zero);
			MinorBreakableGroupManager component = m_surfaceObjects[j].GetComponent<MinorBreakableGroupManager>();
			if (component != null)
			{
				component.Destabilize(zero.ToVector3ZUp(0.5f) * forceMultiplier, 0.5f + num);
				continue;
			}
			DebrisObject component2 = m_surfaceObjects[j].gameObject.GetComponent<DebrisObject>();
			if (component2 != null)
			{
				Vector3 startingForce = Quaternion.Euler(0f, 0f, Mathf.Lerp(-30f, 30f, Random.value)) * zero.ToVector3ZUp(0.25f + num) * forceMultiplier;
				component2.Trigger(startingForce, 0.5f);
				continue;
			}
			MinorBreakable component3 = m_surfaceObjects[j].GetComponent<MinorBreakable>();
			if (component3 != null)
			{
				Vector3 vector = Quaternion.Euler(0f, 0f, Mathf.Lerp(-30f, 30f, Random.value)) * zero.ToVector3ZUp(0.25f + num) * forceMultiplier;
				component3.destroyOnBreak = true;
				component3.Break(vector.XY());
			}
		}
	}

	private IEnumerator DetachRenderersMomentarily()
	{
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < m_attachedSprites.Count; i++)
		{
			if ((bool)m_attachedSprites[i])
			{
				parentSprite.DetachRenderer(m_attachedSprites[i].GetComponent<tk2dBaseSprite>());
			}
		}
	}
}
