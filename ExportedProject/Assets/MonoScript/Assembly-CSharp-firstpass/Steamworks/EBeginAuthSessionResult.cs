namespace Steamworks
{
	public enum EBeginAuthSessionResult
	{
		k_EBeginAuthSessionResultOK = 0,
		k_EBeginAuthSessionResultInvalidTicket = 1,
		k_EBeginAuthSessionResultDuplicateRequest = 2,
		k_EBeginAuthSessionResultInvalidVersion = 3,
		k_EBeginAuthSessionResultGameMismatch = 4,
		k_EBeginAuthSessionResultExpiredTicket = 5
	}
}
