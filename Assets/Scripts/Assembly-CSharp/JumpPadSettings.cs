using UnityEngine;

// Tuning for the bounce pads, so they can't be used to run away indefinitely.
// Lives on GameManagers. Every peer reads the same scene values, which is what lets the host
// and the bouncing client independently arrive at the same launch height without extra syncing.
public class JumpPadSettings : Singleton<JumpPadSettings>
{
	[Header("Shared cooldown")]
	// After anyone uses a pad it goes dead for this long, for every player.
	public float CooldownSeconds = 3f;

	[Header("Per-player height decay (percent of full launch)")]
	// Applies only to the player who used the pad; anyone else still gets a full first bounce.
	public float SecondUsePercent = 50f;

	public float ThirdUsePercent = 30f;

	// Every use from the fourth onwards is capped here.
	public float ClampPercent = 5f;

	[Header("Decay reset")]
	// Leave a pad alone this long and it launches you at full height again.
	public float DecayResetSeconds = 10f;

	public float GetHeightMultiplier(int previousUses)
	{
		if (previousUses <= 0)
		{
			return 1f;
		}
		if (previousUses == 1)
		{
			return Mathf.Max(0f, SecondUsePercent) * 0.01f;
		}
		if (previousUses == 2)
		{
			return Mathf.Max(0f, ThirdUsePercent) * 0.01f;
		}
		return Mathf.Max(0f, ClampPercent) * 0.01f;
	}
}
