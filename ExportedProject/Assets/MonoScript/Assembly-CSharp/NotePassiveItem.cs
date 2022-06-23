public class NotePassiveItem : PassiveItem
{
	public int ResourcefulRatNoteIdentifier = -1;

	private void Awake()
	{
		if (ResourcefulRatNoteIdentifier >= 0)
		{
			string appropriateSpriteName = GetAppropriateSpriteName(false);
			if (!string.IsNullOrEmpty(appropriateSpriteName))
			{
				base.sprite.SetSprite(appropriateSpriteName);
			}
		}
	}

	public string GetAppropriateSpriteName(bool isAmmonomicon)
	{
		return (!isAmmonomicon) ? "resourcefulrat_note_base" : "resourcefulrat_note_base_001";
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
