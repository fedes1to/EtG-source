using System.Collections;
using Dungeonator;
using UnityEngine;

public class DarknessChallengeModifier : ChallengeModifier
{
	public Shader DarknessEffectShader;

	public float FlashlightAngle = 25f;

	private AdditionalBraveLight[] flashlights;

	private int m_valueMinID;

	private RoomHandler m_room;

	private Material m_material;

	private void Start()
	{
		m_material = new Material(DarknessEffectShader);
		Pixelator.Instance.AdditionalCoreStackRenderPass = m_material;
	}

	private Vector4 GetCenterPointInScreenUV(Vector2 centerPoint)
	{
		Vector3 vector = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(centerPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, 0f, 0f);
	}

	private void LateUpdate()
	{
		if (!(m_material != null))
		{
			return;
		}
		float num = GameManager.Instance.PrimaryPlayer.FacingDirection;
		if (num > 270f)
		{
			num -= 360f;
		}
		if (num < -270f)
		{
			num += 360f;
		}
		m_material.SetFloat("_ConeAngle", FlashlightAngle);
		Vector4 centerPointInScreenUV = GetCenterPointInScreenUV(GameManager.Instance.PrimaryPlayer.CenterPosition);
		centerPointInScreenUV.z = num;
		Vector4 value = centerPointInScreenUV;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			num = GameManager.Instance.SecondaryPlayer.FacingDirection;
			if (num > 270f)
			{
				num -= 360f;
			}
			if (num < -270f)
			{
				num += 360f;
			}
			value = GetCenterPointInScreenUV(GameManager.Instance.SecondaryPlayer.CenterPosition);
			value.z = num;
		}
		m_material.SetVector("_Player1ScreenPosition", centerPointInScreenUV);
		m_material.SetVector("_Player2ScreenPosition", value);
	}

	private IEnumerator LerpFlashlight(AdditionalBraveLight abl, bool turnOff)
	{
		float elapsed = 0f;
		float duration = 1f;
		float startPower = abl.LightIntensity;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = ((!turnOff) ? (elapsed / duration) : (1f - elapsed / duration));
			abl.LightIntensity = Mathf.Lerp(1f, startPower, t);
			yield return null;
		}
		if (turnOff)
		{
			Object.Destroy(abl.gameObject);
		}
	}

	private void OnDestroy()
	{
		if ((bool)Pixelator.Instance)
		{
			Pixelator.Instance.AdditionalCoreStackRenderPass = null;
		}
	}
}
