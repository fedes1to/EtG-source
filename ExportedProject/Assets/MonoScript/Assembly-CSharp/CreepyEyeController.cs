using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[ExecuteInEditMode]
public class CreepyEyeController : MonoBehaviour
{
	public const float c_TimeBeforeWarp = 15f;

	public float MaxPupilRadius = 2.5f;

	public CreepyEyeLayer[] layers;

	public Transform poopil;

	public tk2dSprite baseSprite;

	private RoomHandler m_parentRoom;

	private bool m_alreadyWarpingAutomatically;

	private void Start()
	{
		m_parentRoom = base.transform.position.GetAbsoluteRoom();
		m_parentRoom.Entered += HandlePlayerEntered;
	}

	private void HandlePlayerEntered(PlayerController p)
	{
		if (!m_alreadyWarpingAutomatically)
		{
			StartCoroutine(HandleAutowarpOut());
		}
	}

	private IEnumerator HandleAutowarpOut()
	{
		m_alreadyWarpingAutomatically = true;
		yield return new WaitForSeconds(15f);
		m_alreadyWarpingAutomatically = false;
		if (GameManager.Instance.BestActivePlayer.CurrentRoom != m_parentRoom)
		{
			yield break;
		}
		for (int i = 0; i < Minimap.Instance.roomsContainingTeleporters.Count; i++)
		{
			RoomHandler roomHandler = Minimap.Instance.roomsContainingTeleporters[i];
			if (!roomHandler.TeleportersActive)
			{
				continue;
			}
			TeleporterController teleporterController = ((!roomHandler.hierarchyParent) ? null : roomHandler.hierarchyParent.GetComponentInChildren<TeleporterController>(true));
			if (!teleporterController)
			{
				List<TeleporterController> componentsInRoom = roomHandler.GetComponentsInRoom<TeleporterController>();
				if (componentsInRoom.Count > 0)
				{
					teleporterController = componentsInRoom[0];
				}
			}
			if ((bool)teleporterController)
			{
				Vector2 worldCenter = teleporterController.GetComponent<tk2dBaseSprite>().WorldCenter;
				worldCenter -= GameManager.Instance.BestActivePlayer.SpriteDimensions.XY().WithY(0f) / 2f;
				GameManager.Instance.BestActivePlayer.TeleportToPoint(worldCenter, true);
			}
			break;
		}
	}

	private void LateUpdate()
	{
		if (Application.isPlaying)
		{
			Vector2 vector = GameManager.Instance.PrimaryPlayer.CenterPosition - base.transform.position.XY();
			float num = Mathf.Lerp(0f, MaxPupilRadius, vector.magnitude / 7f);
			poopil.transform.localPosition = num * vector.normalized;
		}
		float x = baseSprite.GetBounds().extents.x;
		float x2 = poopil.GetComponent<tk2dSprite>().GetBounds().extents.x;
		for (int i = 0; i < layers.Length; i++)
		{
			if (layers[i].sprite == null)
			{
				layers[i].sprite = layers[i].xform.GetComponent<tk2dSprite>();
			}
			float x3 = layers[i].sprite.GetBounds().extents.x;
			float f = 1f - x3 / x;
			float num2 = (float)i / ((float)layers.Length - 1f);
			f = Mathf.Pow(f, Mathf.Lerp(0.75f, 1f, 1f - num2));
			float num3 = poopil.localPosition.magnitude / (x - x2);
			layers[i].xform.localPosition = poopil.localPosition * f + poopil.localPosition.normalized * x2 * num3 * f;
			layers[i].xform.localPosition = layers[i].xform.localPosition.Quantize(0.0625f);
			layers[i].sprite.HeightOffGround = (float)i * 0.1f + 0.1f;
			layers[i].sprite.UpdateZDepth();
		}
		poopil.GetComponent<tk2dSprite>().HeightOffGround = 1f;
		poopil.GetComponent<tk2dSprite>().UpdateZDepth();
	}
}
