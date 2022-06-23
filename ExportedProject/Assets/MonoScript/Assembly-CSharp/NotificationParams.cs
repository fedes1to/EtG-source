public class NotificationParams
{
	public bool isSingleLine;

	public string EncounterGuid;

	public int pickupId = -1;

	public string PrimaryTitleString;

	public string SecondaryDescriptionString;

	public tk2dSpriteCollectionData SpriteCollection;

	public int SpriteID;

	public UINotificationController.NotificationColor forcedColor;

	public bool OnlyIfSynergy;

	public bool HasAttachedSynergy;

	public AdvancedSynergyEntry AttachedSynergy;
}
