using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class GenericDecorator : BraveBehaviour, IPlaceConfigurable
{
	public GenericLootTable tableTable;

	public IntVector2 localPixelSurfaceOrigin;

	public IntVector2 localPixelSurfaceDimensions;

	public float heightOffGround = 0.1f;

	public bool disableRigidbodies = true;

	public DebrisObject rigidbodyTrigger;

	public tk2dSprite parentSprite;

	[HideInInspector]
	public SurfaceDecorator parentSurface;

	private List<SpeculativeRigidbody> m_srbs = new List<SpeculativeRigidbody>();

	public void ConfigureOnPlacement(RoomHandler room)
	{
		Decorate();
		if (disableRigidbodies && rigidbodyTrigger != null)
		{
			DebrisObject debrisObject = rigidbodyTrigger;
			debrisObject.OnTriggered = (Action)Delegate.Combine(debrisObject.OnTriggered, new Action(EnableRigidbodies));
		}
	}

	private GameObject GetSurfaceObject(Vector2 availableSpace, out Vector2 objectDimensions, out Vector2 localOrigin)
	{
		List<GameObject> list = new List<GameObject>();
		bool flag = false;
		int num = 0;
		while (!flag && num < 1000)
		{
			GameObject gameObject = tableTable.SelectByWeightWithoutDuplicates(list);
			if (gameObject == null)
			{
				break;
			}
			list.Add(gameObject);
			MinorBreakableGroupManager component = gameObject.GetComponent<MinorBreakableGroupManager>();
			if (component != null)
			{
				objectDimensions = component.GetDimensions();
				if (objectDimensions == Vector2.zero)
				{
					continue;
				}
				localOrigin = Vector2.zero;
			}
			else
			{
				tk2dSprite component2 = gameObject.GetComponent<tk2dSprite>();
				Bounds bounds = component2.GetBounds();
				objectDimensions = new Vector2(bounds.size.x, bounds.size.y);
				localOrigin = bounds.min.XY();
			}
			if (objectDimensions.x <= availableSpace.x && objectDimensions.y <= availableSpace.y)
			{
				flag = true;
				return gameObject;
			}
			num++;
		}
		objectDimensions = new Vector2(float.MaxValue, float.MaxValue);
		localOrigin = Vector2.zero;
		return null;
	}

	public void EnableRigidbodies()
	{
		for (int i = 0; i < m_srbs.Count; i++)
		{
			if ((bool)m_srbs[i])
			{
				m_srbs[i].enabled = true;
			}
		}
	}

	private void PostProcessObject(GameObject placedObject)
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
					componentsInChildren[i].HeightOffGround = heightOffGround + 0.1f;
					parentSprite.AttachRenderer(componentsInChildren[i]);
				}
			}
			MinorBreakable[] componentsInChildren2 = component.GetComponentsInChildren<MinorBreakable>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].heightOffGround = 0.75f;
				componentsInChildren2[j].isImpermeableToGameActors = true;
				componentsInChildren2[j].specRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.BulletBreakable;
			}
			DebrisObject[] componentsInChildren3 = component.GetComponentsInChildren<DebrisObject>();
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].InitializeForCollisions();
				componentsInChildren3[k].additionalHeightBoost = 0.25f;
			}
			if (disableRigidbodies && rigidbodyTrigger != null)
			{
				SpeculativeRigidbody[] componentsInChildren4 = component.GetComponentsInChildren<SpeculativeRigidbody>(true);
				for (int l = 0; l < componentsInChildren4.Length; l++)
				{
					m_srbs.Add(componentsInChildren4[l]);
					componentsInChildren4[l].enabled = false;
				}
			}
		}
		else
		{
			tk2dSprite component2 = placedObject.GetComponent<tk2dSprite>();
			if (component2.attachParent == null)
			{
				component2.HeightOffGround = heightOffGround + 0.1f;
				parentSprite.AttachRenderer(component2);
			}
			MinorBreakable component3 = placedObject.GetComponent<MinorBreakable>();
			if (component3 != null)
			{
				component3.heightOffGround = 0.75f;
				component3.isImpermeableToGameActors = true;
				component3.specRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.BulletBreakable;
			}
			DebrisObject component4 = placedObject.GetComponent<DebrisObject>();
			if (component4 != null)
			{
				component4.InitializeForCollisions();
				component4.additionalHeightBoost = 0.25f;
			}
			if (disableRigidbodies && rigidbodyTrigger != null)
			{
				SpeculativeRigidbody component5 = placedObject.GetComponent<SpeculativeRigidbody>();
				if (component5 != null)
				{
					m_srbs.Add(component5);
					component5.enabled = false;
				}
			}
		}
		if (parentSurface != null)
		{
			parentSurface.RegisterAdditionalObject(placedObject);
			if (parentSurface.sprite != null)
			{
				parentSurface.sprite.UpdateZDepth();
			}
		}
		else
		{
			parentSprite.UpdateZDepth();
		}
	}

	public void Decorate()
	{
		if (parentSprite == null)
		{
			parentSprite = GetComponent<tk2dSprite>();
		}
		if (tableTable == null)
		{
			BraveUtility.Log("Trying to decorate a SurfaceDecorator at: " + base.gameObject.name + " and failing.", Color.red, BraveUtility.LogVerbosity.CHATTY);
			return;
		}
		Vector2 vector = PhysicsEngine.PixelToUnit(localPixelSurfaceOrigin);
		Vector2 vector2 = PhysicsEngine.PixelToUnit(localPixelSurfaceDimensions);
		bool flag = ((vector2.x >= vector2.y) ? true : false);
		float num = 0f;
		float num2 = 0f;
		float num3 = ((!flag) ? vector2.y : vector2.x);
		float num4 = ((!flag) ? vector2.x : vector2.y);
		float num5 = ((!flag) ? vector.y : vector.x);
		float num6 = ((!flag) ? vector.x : vector.y);
		float num8;
		for (; num < num3; num += num8)
		{
			float num7 = num3 - num;
			num8 = 0f;
			float num11;
			for (num2 = 0f; num2 < num4; num2 += num11)
			{
				float num9 = num4 - num2;
				Vector2 objectDimensions = Vector2.zero;
				Vector2 localOrigin = Vector2.zero;
				Vector2 availableSpace = ((!flag) ? new Vector2(num9, num7) : new Vector2(num7, num9));
				GameObject surfaceObject = GetSurfaceObject(availableSpace, out objectDimensions, out localOrigin);
				if (surfaceObject == null)
				{
					num = num3;
					num2 = num4;
					break;
				}
				float num10 = ((!flag) ? objectDimensions.y : objectDimensions.x);
				num11 = ((!flag) ? objectDimensions.x : objectDimensions.y);
				if (num10 <= num7 && num11 <= num9)
				{
					Vector3 vector3 = ((!flag) ? (base.transform.position + new Vector3(num6 + num2, num5 + num, -0.5f)) : (base.transform.position + new Vector3(num5 + num, num6 + num2, -0.5f)));
					GameObject placedObject = UnityEngine.Object.Instantiate(surfaceObject, vector3 - localOrigin.ToVector3ZUp(), Quaternion.identity);
					PostProcessObject(placedObject);
				}
				num8 = Mathf.Max(num8, num10);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
