// Flavour text for the kill feed, two per weapon. Picked with the attack id rather than
// Random so every machine formats the same line for the same kill.
//
// Safe to reword freely - nothing parses these, they're only ever displayed.
public static class KillFeedVerbs
{
	private static readonly string[] Fists = new string[] { "decked", "knocked out" };

	private static readonly string[] Sword = new string[] { "carved", "sliced up" };

	private static readonly string[] Crossbow = new string[] { "skewered", "pinned" };

	private static readonly string[] Rifle = new string[] { "picked off", "drilled" };

	private static readonly string[] Reindeer = new string[] { "trampled", "gored" };

	private static readonly string[] Pistol = new string[] { "popped", "tagged" };

	private static readonly string[] LightningGun = new string[] { "zapped", "fried" };

	private static readonly string[] SnowballLauncher = new string[] { "snowballed", "frosted" };

	private static readonly string[] Grenade = new string[] { "'sploded", "blew up" };

	private static readonly string[] BoxingGloves = new string[] { "clobbered", "socked" };

	private static readonly string[] ShockRifle = new string[] { "railed", "vaporised" };

	public static string Get(WeaponType weapon, int seed)
	{
		string[] options = getOptions(weapon);
		// Negative ids would index backwards out of the array.
		int index = ((seed < 0) ? (-seed) : seed) % options.Length;
		return options[index];
	}

	private static string[] getOptions(WeaponType weapon)
	{
		switch (weapon)
		{
		case WeaponType.Sword:
			return Sword;
		case WeaponType.Crossbow:
			return Crossbow;
		case WeaponType.Rifle:
			return Rifle;
		case WeaponType.Reindeer:
			return Reindeer;
		case WeaponType.Pistol:
			return Pistol;
		case WeaponType.LightningGun:
			return LightningGun;
		case WeaponType.SnowballLauncher:
			return SnowballLauncher;
		case WeaponType.Grenade:
			return Grenade;
		case WeaponType.BoxingGloves:
			return BoxingGloves;
		case WeaponType.ShockRifle:
			return ShockRifle;
		default:
			return Fists;
		}
	}
}
