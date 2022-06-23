using System.Collections;
using System.Text;
using Dungeonator;
using UnityEngine;

public class FoyerTimeHeroStatue : BraveBehaviour, IPlayerInteractable
{
	public string targetDisplayKey;

	public Transform talkPoint;

	public IEnumerator Start()
	{
		yield return null;
		if (base.gameObject.activeSelf)
		{
			RoomHandler.unassignedInteractableObjects.Add(this);
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		TextBoxManager.ClearTextBox(talkPoint);
	}

	public void Interact(PlayerController interactor)
	{
		if (TextBoxManager.HasTextBox(talkPoint))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StringTableManager.GetLongString(targetDisplayKey));
		stringBuilder.Append("\n");
		stringBuilder.Append("\n");
		stringBuilder.Append(StringTableManager.EvaluateReplacementToken("%BTCKTP_PRIMER") + " ");
		stringBuilder.Append(StringTableManager.EvaluateReplacementToken("%BTCKTP_POWDER") + " ");
		stringBuilder.Append(StringTableManager.EvaluateReplacementToken("%BTCKTP_SLUG") + " ");
		stringBuilder.Append(StringTableManager.EvaluateReplacementToken("%BTCKTP_CASING"));
		TextBoxManager.ShowStoneTablet(talkPoint.position, talkPoint, -1f, stringBuilder.ToString());
		tk2dTextMesh[] componentsInChildren = talkPoint.GetComponentsInChildren<tk2dTextMesh>();
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			foreach (tk2dTextMesh tk2dTextMesh2 in componentsInChildren)
			{
				tk2dTextMesh2.LineSpacing = -0.25f;
				tk2dTextMesh2.transform.localPosition = tk2dTextMesh2.transform.localPosition + new Vector3(0f, -0.375f, 0f);
				tk2dTextMesh2.ForceBuild();
			}
		}
		tk2dBaseSprite[] componentsInChildren2 = talkPoint.GetComponentsInChildren<tk2dSprite>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (componentsInChildren2[j].CurrentSprite.name.StartsWith("forged_bullet"))
			{
				if (componentsInChildren2[j].CurrentSprite.name.Contains("primer"))
				{
					componentsInChildren2[j].renderer.material.SetFloat("_SaturationModifier", GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT1) ? 1 : 0);
				}
				if (componentsInChildren2[j].CurrentSprite.name.Contains("powder"))
				{
					componentsInChildren2[j].renderer.material.SetFloat("_SaturationModifier", GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT2) ? 1 : 0);
				}
				if (componentsInChildren2[j].CurrentSprite.name.Contains("slug"))
				{
					componentsInChildren2[j].renderer.material.SetFloat("_SaturationModifier", GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT3) ? 1 : 0);
				}
				if (componentsInChildren2[j].CurrentSprite.name.Contains("case"))
				{
					componentsInChildren2[j].renderer.material.SetFloat("_SaturationModifier", GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT4) ? 1 : 0);
				}
			}
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}
}
