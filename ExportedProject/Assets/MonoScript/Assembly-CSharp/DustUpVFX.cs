using System;
using Dungeonator;
using UnityEngine;

[Serializable]
public class DustUpVFX
{
	public GameObject runDustup;

	public GameObject waterDustup;

	public GameObject additionalWaterDustup;

	public GameObject rollNorthDustup;

	public GameObject rollNorthEastDustup;

	public GameObject rollEastDustup;

	public GameObject rollSouthEastDustup;

	public GameObject rollSouthDustup;

	public GameObject rollSouthWestDustup;

	public GameObject rollWestDustup;

	public GameObject rollNorthWestDustup;

	public GameObject rollLandDustup;

	public void InstantiateLandDustup(Vector3 worldPosition)
	{
		SpawnManager.SpawnVFX(rollLandDustup, worldPosition, Quaternion.identity);
	}

	public void InstantiateDodgeDustup(Vector2 velocity, Vector3 worldPosition)
	{
		switch (DungeonData.GetDirectionFromVector2(velocity))
		{
		case DungeonData.Direction.NORTH:
			if (rollNorthDustup != null)
			{
				GameObject gameObject9 = SpawnManager.SpawnVFX(rollNorthDustup, worldPosition, Quaternion.identity);
				gameObject9.GetComponent<tk2dSprite>().FlipX = false;
				gameObject9.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject10 = SpawnManager.SpawnVFX(rollSouthDustup, worldPosition, Quaternion.identity);
				gameObject10.GetComponent<tk2dSprite>().FlipX = false;
				gameObject10.GetComponent<tk2dSprite>().FlipY = true;
			}
			break;
		case DungeonData.Direction.NORTHEAST:
			if (rollNorthEastDustup != null)
			{
				GameObject gameObject3 = SpawnManager.SpawnVFX(rollNorthEastDustup, worldPosition, Quaternion.identity);
				gameObject3.GetComponent<tk2dSprite>().FlipX = false;
				gameObject3.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject4 = SpawnManager.SpawnVFX(rollNorthWestDustup, worldPosition, Quaternion.identity);
				gameObject4.GetComponent<tk2dSprite>().FlipX = true;
				gameObject4.GetComponent<tk2dSprite>().FlipY = false;
			}
			break;
		case DungeonData.Direction.EAST:
			if (rollEastDustup != null)
			{
				GameObject gameObject11 = SpawnManager.SpawnVFX(rollEastDustup, worldPosition, Quaternion.identity);
				gameObject11.GetComponent<tk2dSprite>().FlipX = false;
				gameObject11.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject12 = SpawnManager.SpawnVFX(rollWestDustup, worldPosition, Quaternion.identity);
				gameObject12.GetComponent<tk2dSprite>().FlipX = true;
				gameObject12.GetComponent<tk2dSprite>().FlipY = false;
			}
			break;
		case DungeonData.Direction.SOUTHEAST:
			if (rollSouthEastDustup != null)
			{
				GameObject gameObject15 = SpawnManager.SpawnVFX(rollSouthEastDustup, worldPosition, Quaternion.identity);
				gameObject15.GetComponent<tk2dSprite>().FlipX = false;
				gameObject15.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject16 = SpawnManager.SpawnVFX(rollSouthWestDustup, worldPosition, Quaternion.identity);
				gameObject16.GetComponent<tk2dSprite>().FlipX = true;
				gameObject16.GetComponent<tk2dSprite>().FlipY = false;
			}
			break;
		case DungeonData.Direction.SOUTH:
			if (rollSouthDustup != null)
			{
				GameObject gameObject5 = SpawnManager.SpawnVFX(rollSouthDustup, worldPosition, Quaternion.identity);
				gameObject5.GetComponent<tk2dSprite>().FlipX = false;
				gameObject5.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject6 = SpawnManager.SpawnVFX(rollNorthDustup, worldPosition, Quaternion.identity);
				gameObject6.GetComponent<tk2dSprite>().FlipX = false;
				gameObject6.GetComponent<tk2dSprite>().FlipY = true;
			}
			break;
		case DungeonData.Direction.SOUTHWEST:
			if (rollSouthWestDustup != null)
			{
				GameObject gameObject13 = SpawnManager.SpawnVFX(rollSouthWestDustup, worldPosition, Quaternion.identity);
				gameObject13.GetComponent<tk2dSprite>().FlipX = false;
				gameObject13.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject14 = SpawnManager.SpawnVFX(rollSouthEastDustup, worldPosition, Quaternion.identity);
				gameObject14.GetComponent<tk2dSprite>().FlipX = true;
				gameObject14.GetComponent<tk2dSprite>().FlipY = false;
			}
			break;
		case DungeonData.Direction.WEST:
			if (rollWestDustup != null)
			{
				GameObject gameObject7 = SpawnManager.SpawnVFX(rollWestDustup, worldPosition, Quaternion.identity);
				gameObject7.GetComponent<tk2dSprite>().FlipX = false;
				gameObject7.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject8 = SpawnManager.SpawnVFX(rollEastDustup, worldPosition, Quaternion.identity);
				gameObject8.GetComponent<tk2dSprite>().FlipX = true;
				gameObject8.GetComponent<tk2dSprite>().FlipY = false;
			}
			break;
		case DungeonData.Direction.NORTHWEST:
			if (rollNorthWestDustup != null)
			{
				GameObject gameObject = SpawnManager.SpawnVFX(rollNorthWestDustup, worldPosition, Quaternion.identity);
				gameObject.GetComponent<tk2dSprite>().FlipX = false;
				gameObject.GetComponent<tk2dSprite>().FlipY = false;
			}
			else
			{
				GameObject gameObject2 = SpawnManager.SpawnVFX(rollNorthEastDustup, worldPosition, Quaternion.identity);
				gameObject2.GetComponent<tk2dSprite>().FlipX = true;
				gameObject2.GetComponent<tk2dSprite>().FlipY = false;
			}
			break;
		}
	}
}
