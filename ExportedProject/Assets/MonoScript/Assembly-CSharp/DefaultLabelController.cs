using System.Collections;
using UnityEngine;

public class DefaultLabelController : BraveBehaviour
{
	public dfLabel label;

	public dfPanel panel;

	public Transform targetObject;

	public Vector3 offset;

	private dfGUIManager m_manager;

	public void Trigger()
	{
		StartCoroutine(Expand_CR());
	}

	public void Trigger(Transform aTarget, Vector3 anOffset)
	{
		offset = anOffset;
		targetObject = aTarget;
		Trigger();
	}

	private IEnumerator Expand_CR()
	{
		panel.Width = 1f;
		float elapsed = 0f;
		float duration = 0.3f;
		float targetWidth = label.Width + 1f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			panel.Width = Mathf.Lerp(1f, targetWidth, elapsed / duration);
			yield return null;
		}
	}

	private void LateUpdate()
	{
		UpdatePosition();
		UpdateForLanguage();
	}

	public void UpdateForLanguage()
	{
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN)
		{
			label.Padding.top = 0;
		}
		else
		{
			label.Padding.top = -6;
		}
	}

	public void UpdatePosition()
	{
		if (m_manager == null)
		{
			m_manager = panel.GetManager();
		}
		if ((bool)targetObject)
		{
			base.transform.position = dfFollowObject.ConvertWorldSpaces(targetObject.transform.position + offset, GameManager.Instance.MainCameraController.Camera, m_manager.RenderCamera).WithZ(0f);
			base.transform.position = base.transform.position.QuantizeFloor(panel.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
