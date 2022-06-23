using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BooRoomChallengeModifier : ChallengeModifier
{
	public float ConeAngle = 45f;

	public Shader DarknessEffectShader;

	private Material m_material;

	private void Start()
	{
		if (Pixelator.Instance.AdditionalCoreStackRenderPass == null)
		{
			m_material = new Material(DarknessEffectShader);
			Pixelator.Instance.AdditionalCoreStackRenderPass = m_material;
		}
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
		m_material.SetFloat("_ConeAngle", ConeAngle);
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

	private void Update()
	{
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		Vector2 zero = Vector2.zero;
		float num = 0f;
		Vector2 zero2 = Vector2.zero;
		float num2 = 0f;
		num = ((!GameManager.Instance.PrimaryPlayer.CurrentGun || GameManager.Instance.PrimaryPlayer.IsGhost) ? BraveMathCollege.Atan2Degrees(GameManager.Instance.PrimaryPlayer.unadjustedAimPoint.XY()) : GameManager.Instance.PrimaryPlayer.CurrentGun.CurrentAngle);
		zero = GameManager.Instance.PrimaryPlayer.CenterPosition;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			num2 = ((!GameManager.Instance.SecondaryPlayer.CurrentGun || GameManager.Instance.SecondaryPlayer.IsGhost) ? BraveMathCollege.Atan2Degrees(GameManager.Instance.SecondaryPlayer.unadjustedAimPoint.XY()) : GameManager.Instance.SecondaryPlayer.CurrentGun.CurrentAngle);
			zero2 = GameManager.Instance.SecondaryPlayer.CenterPosition;
		}
		else
		{
			zero2 = zero;
			num2 = num;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor = activeEnemies[i];
			if (!aIActor || !aIActor.healthHaver || !aIActor.IsNormalEnemy || aIActor.healthHaver.IsBoss || aIActor.healthHaver.IsDead)
			{
				continue;
			}
			Vector2 centerPosition = aIActor.CenterPosition;
			float b = BraveMathCollege.Atan2Degrees(centerPosition - zero);
			float b2 = BraveMathCollege.Atan2Degrees(centerPosition - zero2);
			if (BraveMathCollege.AbsAngleBetween(num, b) < ConeAngle || BraveMathCollege.AbsAngleBetween(num2, b2) < ConeAngle)
			{
				if ((bool)aIActor.behaviorSpeculator)
				{
					aIActor.behaviorSpeculator.Stun(0.25f);
				}
				if (aIActor.IsBlackPhantom)
				{
					aIActor.UnbecomeBlackPhantom();
				}
			}
			else if (!aIActor.IsBlackPhantom)
			{
				aIActor.BecomeBlackPhantom();
			}
		}
	}

	private void OnDestroy()
	{
		if (m_material != null && (bool)Pixelator.Instance)
		{
			Pixelator.Instance.AdditionalCoreStackRenderPass = null;
		}
	}
}
