using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Gets data from a url and store it in variables. See Unity WWW docs for more details.")]
	[ActionCategory("Web Player")]
	public class WWWObject : FsmStateAction
	{
		[Tooltip("Url to download data from.")]
		[RequiredField]
		public FsmString url;

		[ActionSection("Results")]
		[UIHint(UIHint.Variable)]
		[Tooltip("Gets text from the url.")]
		public FsmString storeText;

		[Tooltip("Gets a Texture from the url.")]
		[UIHint(UIHint.Variable)]
		public FsmTexture storeTexture;

		[Tooltip("Gets a Texture from the url.")]
		[ObjectType(typeof(MovieTexture))]
		[UIHint(UIHint.Variable)]
		public FsmObject storeMovieTexture;

		[Tooltip("Error message if there was an error during the download.")]
		[UIHint(UIHint.Variable)]
		public FsmString errorString;

		[Tooltip("How far the download progressed (0-1).")]
		[UIHint(UIHint.Variable)]
		public FsmFloat progress;

		[ActionSection("Events")]
		[Tooltip("Event to send when the data has finished loading (progress = 1).")]
		public FsmEvent isDone;

		[Tooltip("Event to send if there was an error.")]
		public FsmEvent isError;

		private WWW wwwObject;

		public override void Reset()
		{
			url = null;
			storeText = null;
			storeTexture = null;
			errorString = null;
			progress = null;
			isDone = null;
		}

		public override void OnEnter()
		{
			if (string.IsNullOrEmpty(url.Value))
			{
				Finish();
			}
			else
			{
				wwwObject = new WWW(url.Value);
			}
		}

		public override void OnUpdate()
		{
			if (wwwObject == null)
			{
				errorString.Value = "WWW Object is Null!";
				Finish();
				return;
			}
			errorString.Value = wwwObject.error;
			if (!string.IsNullOrEmpty(wwwObject.error))
			{
				Finish();
				base.Fsm.Event(isError);
				return;
			}
			progress.Value = wwwObject.progress;
			if (progress.Value.Equals(1f))
			{
				storeText.Value = wwwObject.text;
				storeTexture.Value = wwwObject.texture;
				storeMovieTexture.Value = wwwObject.GetMovieTexture();
				errorString.Value = wwwObject.error;
				base.Fsm.Event((!string.IsNullOrEmpty(errorString.Value)) ? isError : isDone);
				Finish();
			}
		}
	}
}
