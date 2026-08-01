using System.Collections.Generic;

public class AudioLibrary : Singleton<AudioLibrary>
{
	public AudioClipDefinition CountdownMusic;

	public AudioClipDefinition[] CombatMusic;

	public AudioClipDefinition[] WinMusic;

	public AudioClipDefinition[] MainMenuMusic;

	public AudioClipDefinition[] BeginPunch;

	public AudioClipDefinition[] PunchImpact;

	public AudioClipDefinition[] SwordSwing;

	public AudioClipDefinition[] SwordImpact;

	public AudioClipDefinition[] JumpPadCompress;

	public AudioClipDefinition[] JumpPadBoing;

	public AudioClipDefinition[] SnowFootstep;

	public AudioClipDefinition[] DirtFootstep;

	public AudioClipDefinition[] IceFootstep;

	public AudioClipDefinition[] StoneFootstep;

	public AudioClipDefinition[] WoodFootstep;

	public AudioClipDefinition[] TreeFootstep;

	public AudioClipDefinition[] ReindeerTrot;

	public AudioClipDefinition[] ReindeerRun;

	public AudioClipDefinition[] CrossbowFire;

	public AudioClipDefinition[] PistolFire;
	public AudioClipDefinition[] SnowballThrow;
	public AudioClipDefinition[] LightningFire;
	// Loops while the rail is charging, then the firing one-shot takes over on release.
	public AudioClipDefinition[] RailGunCharging;
	public AudioClipDefinition[] RailGunFiring;

	public AudioClipDefinition[] CrossbowFlyBy;

	public AudioClipDefinition[] CrossbowImpactEnv;

	public AudioClipDefinition[] CrossbowImpactBody;

	public AudioClipDefinition[] CrossbowImpactBodySecondary;

	public AudioClipDefinition[] FlareStart;

	public AudioClipDefinition[] FlareIdle;

	public AudioClipDefinition[] GiftParachuteIdle;

	public AudioClipDefinition[] GiftParachuteExplode;

	public AudioClipDefinition[] GiftParachuteExplodeSecondary;

	public AudioClipDefinition[] GiftBoxOpen;

	public AudioClipDefinition[] GiftBoxPostReveal;

	public AudioClipDefinition[] GiftGet;

	public Dictionary<GroundType, AudioClipDefinition[]> GroundTypeToSound;

	public AudioClipDefinition CountdownPunch;

	public AudioClipDefinition CrowdCountdown;

	public AudioClipDefinition GameStart;

	public AudioClipDefinition ReadyFightVoice;

	public AudioClipDefinition GameStartDrop;

	public AudioClipDefinition Jingle;

	public AudioClipDefinition GameOver;

	public AudioClipDefinition GameOverSong;

	public AudioClipDefinition GameOverHoHo;

 

	public void Start()
	{
		GroundTypeToSound = new Dictionary<GroundType, AudioClipDefinition[]>
		{
			{
				GroundType.Snow,
				SnowFootstep
			},
			{
				GroundType.Dirt,
				DirtFootstep
			},
			{
				GroundType.Ice,
				IceFootstep
			},
			{
				GroundType.Stone,
				StoneFootstep
			},
			{
				GroundType.Wood,
				WoodFootstep
			},
			{
				GroundType.Tree,
				TreeFootstep
			}
		};
	}
}
