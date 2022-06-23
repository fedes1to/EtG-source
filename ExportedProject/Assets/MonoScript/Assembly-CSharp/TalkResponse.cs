using System;
using System.Collections.Generic;

[Serializable]
public class TalkResponse
{
	public string response;

	public string followupModuleID;

	public List<TalkResult> resultActions;
}
