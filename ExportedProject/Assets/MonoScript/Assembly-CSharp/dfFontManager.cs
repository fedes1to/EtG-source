public static class dfFontManager
{
	private static dfList<dfFontBase> dirty = new dfList<dfFontBase>();

	private static dfList<dfFontBase> rebuildList = new dfList<dfFontBase>();

	public static void FlagPendingRequests(dfFontBase font)
	{
		dfDynamicFont dfDynamicFont2 = font as dfDynamicFont;
		if (dfDynamicFont2 != null && !rebuildList.Contains(dfDynamicFont2))
		{
			rebuildList.Add(dfDynamicFont2);
		}
	}

	public static void Invalidate(dfFontBase font)
	{
		if (!(font == null) && font is dfDynamicFont && !dirty.Contains(font))
		{
			dirty.Add(font);
		}
	}

	public static bool IsDirty(dfFontBase font)
	{
		return dirty.Contains(font);
	}

	public static bool RebuildDynamicFonts()
	{
		bool flag = false;
		rebuildList.Clear();
		dfList<dfControl> activeInstances = dfControl.ActiveInstances;
		for (int i = 0; i < activeInstances.Count; i++)
		{
			IRendersText rendersText = activeInstances[i] as IRendersText;
			if (rendersText != null)
			{
				rendersText.UpdateFontInfo();
			}
		}
		flag = rebuildList.Count > 0;
		for (int j = 0; j < rebuildList.Count; j++)
		{
			dfDynamicFont dfDynamicFont2 = rebuildList[j] as dfDynamicFont;
			if (dfDynamicFont2 != null)
			{
				dfDynamicFont2.FlushCharacterRequests();
			}
		}
		rebuildList.Clear();
		dirty.Clear();
		return flag;
	}
}
