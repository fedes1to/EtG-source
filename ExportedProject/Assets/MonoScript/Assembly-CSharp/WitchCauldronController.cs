using System;
using System.Collections;
using UnityEngine;

public class WitchCauldronController : MonoBehaviour
{
	public tk2dBaseSprite cauldronSprite;

	public GenericLootTable lootTableToUse;

	public float baseChanceOfImprovingItem = 0.5f;

	public float CurseToGive = 2f;

	public string[] cauldronIns;

	public string[] cauldronIdles;

	public string[] cauldronOuts;

	public bool IsGunInPot { get; private set; }

	public void Start()
	{
		StartCoroutine(HandleBackgroundBubblin());
	}

	private IEnumerator HandleBackgroundBubblin()
	{
		tk2dSpriteAnimationClip mainIdleClip = cauldronSprite.spriteAnimator.GetClipByName("cauldron_idle");
		while (true)
		{
			if (cauldronSprite.spriteAnimator.IsPlaying("cauldron_splash"))
			{
				yield return null;
				continue;
			}
			int idleMultiplex = UnityEngine.Random.Range(4, 12);
			float timeToIdle = (float)idleMultiplex * ((float)mainIdleClip.frames.Length / mainIdleClip.fps);
			cauldronSprite.spriteAnimator.Play(mainIdleClip);
			yield return new WaitForSeconds(timeToIdle);
			int randomIndex = UnityEngine.Random.Range(0, cauldronIns.Length);
			tk2dSpriteAnimationClip inClip = cauldronSprite.spriteAnimator.GetClipByName(cauldronIns[randomIndex]);
			tk2dSpriteAnimationClip idleClip = cauldronSprite.spriteAnimator.GetClipByName(cauldronIdles[randomIndex]);
			tk2dSpriteAnimationClip outClip = cauldronSprite.spriteAnimator.GetClipByName(cauldronOuts[randomIndex]);
			if (cauldronSprite.spriteAnimator.IsPlaying("cauldron_splash"))
			{
				continue;
			}
			cauldronSprite.spriteAnimator.Play(inClip);
			yield return new WaitForSeconds((float)inClip.frames.Length / inClip.fps);
			if (!cauldronSprite.spriteAnimator.IsPlaying("cauldron_splash"))
			{
				cauldronSprite.spriteAnimator.Play(idleClip);
				yield return new WaitForSeconds((float)idleClip.frames.Length / idleClip.fps);
				if (!cauldronSprite.spriteAnimator.IsPlaying("cauldron_splash"))
				{
					cauldronSprite.spriteAnimator.Play(outClip);
					yield return new WaitForSeconds((float)outClip.frames.Length / outClip.fps);
				}
			}
		}
	}

	public bool TossPlayerEquippedGun(PlayerController player)
	{
		if (player.CurrentGun != null && player.CurrentGun.CanActuallyBeDropped(player) && !player.CurrentGun.InfiniteAmmo)
		{
			IsGunInPot = true;
			Gun currentGun = player.CurrentGun;
			TossObjectIntoPot(currentGun.GetSprite(), player.CenterPosition);
			player.inventory.RemoveGunFromInventory(currentGun);
			PickupObject.ItemQuality itemQuality = currentGun.quality;
			if (itemQuality < PickupObject.ItemQuality.S && UnityEngine.Random.value < baseChanceOfImprovingItem)
			{
				itemQuality++;
			}
			Gun itemOfTypeAndQuality = LootEngine.GetItemOfTypeAndQuality<Gun>(itemQuality, lootTableToUse);
			if (itemOfTypeAndQuality != null)
			{
				StartCoroutine(DelayedItemSpawn(itemOfTypeAndQuality.gameObject, 3f));
			}
			else
			{
				StartCoroutine(DelayedItemSpawn(currentGun.gameObject, 3f));
			}
			UnityEngine.Object.Destroy(currentGun.gameObject);
			return true;
		}
		return false;
	}

	private IEnumerator DelayedItemSpawn(GameObject item, float delay)
	{
		yield return new WaitForSeconds(delay);
		Vector3 spawnPoint = cauldronSprite.WorldCenter - item.GetComponent<tk2dBaseSprite>().GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
		cauldronSprite.spriteAnimator.Play("cauldron_splash");
		AkSoundEngine.PostEvent("Play_OBJ_cauldron_splash_01", base.gameObject);
		tk2dSpriteAnimator spriteAnimator = cauldronSprite.spriteAnimator;
		spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimComplete));
		LootEngine.SpawnItem(item, spawnPoint, new Vector2(0f, -1f), 2f);
		if (CurseToGive > 0f)
		{
			StatModifier statModifier = new StatModifier();
			statModifier.statToBoost = PlayerStats.StatType.Curse;
			statModifier.amount = CurseToGive;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			if ((bool)bestActivePlayer)
			{
				bestActivePlayer.ownerlessStatModifiers.Add(statModifier);
				bestActivePlayer.stats.RecalculateStats(bestActivePlayer);
			}
		}
		IsGunInPot = false;
	}

	public void TossObjectIntoPot(tk2dBaseSprite spriteSource, Vector3 startPosition)
	{
		StartCoroutine(HandleObjectPotToss(spriteSource, startPosition));
	}

	private IEnumerator HandleObjectPotToss(tk2dBaseSprite spriteSource, Vector3 startPosition)
	{
		GameObject fakeObject = new GameObject("cauldron temp object");
		tk2dSprite sprite = tk2dBaseSprite.AddComponent<tk2dSprite>(fakeObject, spriteSource.Collection, spriteSource.spriteId);
		sprite.HeightOffGround = 2f;
		sprite.PlaceAtPositionByAnchor(startPosition, tk2dBaseSprite.Anchor.MiddleCenter);
		Vector3 endPosition = cauldronSprite.WorldCenter.ToVector3ZUp();
		float duration2 = 0.4f;
		float elapsed2 = 0f;
		while (elapsed2 < duration2)
		{
			elapsed2 += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed2 / duration2);
			Vector3 targetPosition = Vector3.Lerp(startPosition, endPosition, t);
			sprite.PlaceAtPositionByAnchor(targetPosition, tk2dBaseSprite.Anchor.MiddleCenter);
			sprite.UpdateZDepth();
			yield return null;
		}
		AkSoundEngine.PostEvent("Play_OBJ_cauldron_use_01", base.gameObject);
		cauldronSprite.spriteAnimator.Play("cauldron_splash");
		tk2dSpriteAnimator spriteAnimator = cauldronSprite.spriteAnimator;
		spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimComplete));
		elapsed2 = 0f;
		duration2 = 0.25f;
		while (elapsed2 < duration2)
		{
			elapsed2 += BraveTime.DeltaTime;
			float num = 1f - elapsed2 / duration2;
			sprite.scale = Vector3.one * num;
		}
		UnityEngine.Object.Destroy(fakeObject);
	}

	private void OnAnimComplete(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		arg1.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(arg1.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimComplete));
		arg1.Play("cauldron_idle");
	}
}
