using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ProjectileImpactVFXPool
{
	public bool alwaysUseMidair;

	[HideInInspectorIf("alwaysUseMidair", false)]
	public VFXPool tileMapVertical;

	[HideInInspectorIf("alwaysUseMidair", false)]
	public VFXPool tileMapHorizontal;

	[HideInInspectorIf("alwaysUseMidair", false)]
	public VFXPool enemy;

	public bool suppressMidairDeathVfx;

	public GameObject overrideMidairDeathVFX;

	[ShowInInspectorIf("overrideMidairDeathVFX", false)]
	public bool midairInheritsRotation;

	[ShowInInspectorIf("overrideMidairDeathVFX", false)]
	public bool midairInheritsVelocity;

	[ShowInInspectorIf("overrideMidairDeathVFX", false)]
	public bool midairInheritsFlip;

	[ShowInInspectorIf("overrideMidairDeathVFX", false)]
	public int overrideMidairZHeight = -1;

	public GameObject overrideEarlyDeathVfx;

	public bool HasProjectileDeathVFX;

	[ShowInInspectorIf("HasProjectileDeathVFX", true)]
	public VFXPool deathTileMapVertical;

	[ShowInInspectorIf("HasProjectileDeathVFX", true)]
	public VFXPool deathTileMapHorizontal;

	[ShowInInspectorIf("HasProjectileDeathVFX", true)]
	public VFXPool deathEnemy;

	[ShowInInspectorIf("HasProjectileDeathVFX", true)]
	[FormerlySerializedAs("ProjectileDeathVFX")]
	public VFXPool deathAny;

	[ShowInInspectorIf("HasProjectileDeathVFX", true)]
	public bool CenterDeathVFXOnProjectile;

	[NonSerialized]
	public bool suppressHitEffectsIfOffscreen;

	public bool HasTileMapVerticalEffects
	{
		get
		{
			return tileMapVertical.type != 0 || (HasProjectileDeathVFX && deathTileMapVertical.type != VFXPoolType.None);
		}
	}

	public bool HasTileMapHorizontalEffects
	{
		get
		{
			return tileMapHorizontal.type != 0 || (HasProjectileDeathVFX && deathTileMapHorizontal.type != VFXPoolType.None);
		}
	}

	public GameObject SpawnVFXEnemy(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(prefab, position, rotation, ignoresPools);
		if (gameObject.CompareTag("DefaultEnemyHitVFX"))
		{
			tk2dSpriteAnimator component = gameObject.GetComponent<tk2dSpriteAnimator>();
			component.deferNextStartClip = true;
			component.Play("Dust_Impact_Enemy");
		}
		return gameObject;
	}

	public void HandleProjectileDeathVFX(Vector3 position, float rotation, Transform enemyTransform, Vector2 sourceNormal, Vector2 sourceVelocity, bool isObject = false)
	{
		if (!suppressHitEffectsIfOffscreen || GameManager.Instance.MainCameraController.PointIsVisible(position, 0.05f))
		{
			if (isObject)
			{
				deathAny.SpawnAtPosition(position, rotation, enemyTransform, sourceNormal, sourceVelocity, null, false, SpawnVFXEnemy);
			}
			else
			{
				deathAny.SpawnAtPosition(position, rotation, enemyTransform, sourceNormal, sourceVelocity);
			}
		}
	}

	public void HandleEnemyImpact(Vector3 position, float rotation, Transform enemyTransform, Vector2 sourceNormal, Vector2 sourceVelocity, bool playProjectileDeathVfx, bool isObject = false)
	{
		if (suppressHitEffectsIfOffscreen && !GameManager.Instance.MainCameraController.PointIsVisible(position, 0.05f))
		{
			return;
		}
		float? heightOffGround = null;
		if (Projectile.CurrentProjectileDepth != 0.8f)
		{
			heightOffGround = Projectile.CurrentProjectileDepth;
		}
		if (isObject)
		{
			enemy.SpawnAtPosition(position, rotation, enemyTransform, sourceNormal, sourceVelocity, heightOffGround, false, SpawnVFXEnemy);
			if (playProjectileDeathVfx && HasProjectileDeathVFX)
			{
				deathEnemy.SpawnAtPosition(position, rotation, enemyTransform, sourceNormal, sourceVelocity, heightOffGround, false, SpawnVFXEnemy);
			}
		}
		else
		{
			enemy.SpawnAtPosition(position, rotation, enemyTransform, sourceNormal, sourceVelocity, heightOffGround);
			if (playProjectileDeathVfx && HasProjectileDeathVFX)
			{
				deathEnemy.SpawnAtPosition(position, rotation, enemyTransform, sourceNormal, sourceVelocity, heightOffGround, false, SpawnVFXEnemy);
			}
		}
	}

	public void HandleTileMapImpactVertical(Vector3 position, float heightOffGroundOffset, float rotation, Vector2 sourceNormal, Vector2 sourceVelocity, bool playProjectileDeathVfx, Transform parent = null, VFXComplex.SpawnMethod overrideSpawnMethod = null, VFXComplex.SpawnMethod overrideDeathSpawnMethod = null)
	{
		if (suppressHitEffectsIfOffscreen && !GameManager.Instance.MainCameraController.PointIsVisible(position, 0.05f))
		{
			return;
		}
		float num = rotation + 90f;
		if (!HasTileMapVerticalEffects)
		{
			int roomVisualTypeAtPosition = GameManager.Instance.Dungeon.data.GetRoomVisualTypeAtPosition(position.XY());
			GameManager.Instance.Dungeon.roomMaterialDefinitions[roomVisualTypeAtPosition].SpawnRandomVertical(position, num, parent, sourceNormal, sourceVelocity);
			return;
		}
		float num2 = (float)Mathf.FloorToInt(position.y) - heightOffGroundOffset;
		tileMapVertical.SpawnAtPosition(position.x, num2, position.y - num2, num, parent, sourceNormal, sourceVelocity, false, overrideSpawnMethod);
		if (playProjectileDeathVfx && HasProjectileDeathVFX)
		{
			VFXPool vFXPool = deathTileMapVertical;
			float x = position.x;
			float yPositionAtGround = num2;
			float heightOffGround = position.y - num2;
			float zRotation = num;
			Vector2? sourceNormal2 = sourceNormal;
			Vector2? sourceVelocity2 = sourceVelocity;
			vFXPool.SpawnAtPosition(x, yPositionAtGround, heightOffGround, zRotation, parent, sourceNormal2, sourceVelocity2, false, overrideDeathSpawnMethod);
		}
	}

	public void HandleTileMapImpactHorizontal(Vector3 position, float rotation, Vector2 sourceNormal, Vector2 sourceVelocity, bool playProjectileDeathVfx, Transform parent = null, VFXComplex.SpawnMethod overrideSpawnMethod = null, VFXComplex.SpawnMethod overrideDeathSpawnMethod = null)
	{
		if (suppressHitEffectsIfOffscreen && !GameManager.Instance.MainCameraController.PointIsVisible(position, 0.05f))
		{
			return;
		}
		if (!HasTileMapHorizontalEffects)
		{
			int roomVisualTypeAtPosition = GameManager.Instance.Dungeon.data.GetRoomVisualTypeAtPosition(position.XY());
			GameManager.Instance.Dungeon.roomMaterialDefinitions[roomVisualTypeAtPosition].SpawnRandomHorizontal(position, rotation, parent, sourceNormal, sourceVelocity);
			return;
		}
		tileMapHorizontal.SpawnAtPosition(position, rotation, parent, sourceNormal, sourceVelocity, null, false, overrideSpawnMethod);
		if (playProjectileDeathVfx && HasProjectileDeathVFX)
		{
			VFXPool vFXPool = deathTileMapHorizontal;
			Vector2? sourceNormal2 = sourceNormal;
			Vector2? sourceVelocity2 = sourceVelocity;
			vFXPool.SpawnAtPosition(position, rotation, parent, sourceNormal2, sourceVelocity2, null, false, overrideDeathSpawnMethod);
		}
	}
}
