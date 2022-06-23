using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Pauses a Movie Texture.")]
	[ActionCategory(ActionCategory.Movie)]
	public class PauseMovieTexture : FsmStateAction
	{
		[ObjectType(typeof(MovieTexture))]
		[RequiredField]
		public FsmObject movieTexture;

		public override void Reset()
		{
			movieTexture = null;
		}

		public override void OnEnter()
		{
			MovieTexture movieTexture = this.movieTexture.Value as MovieTexture;
			if (movieTexture != null)
			{
				movieTexture.Pause();
			}
			Finish();
		}
	}
}
