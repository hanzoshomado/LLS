using Bolt;
using UnityEngine;

// Bolt creates this automatically on every peer; it only does work on the host.
// Clients cannot write entity state, so WeaponDebugger sends its equip request here instead.
[BoltGlobalBehaviour]
public class WeaponDebuggerServer : GlobalEventListener
{
    public override void OnEvent(LogEvent evnt)
    {
        if (!BoltNetwork.isServer)
        {
            return;
        }

        WeaponType type;
        int ammo;
        int hitpoints;
        if (!WeaponDebugger.TryParseRequest(evnt.message, out type, out ammo, out hitpoints))
        {
            return;
        }

        // RaisedBy is the connection that asked; null would mean the host, which applies directly.
        if (evnt.RaisedBy == null)
        {
            return;
        }

        SantaCharacterController character = CharacterTracker.Instance.GetAliveCharacterWithConnection(evnt.RaisedBy);
        if (character == null)
        {
            Debug.LogWarning("WeaponDebugger: no living character for " + evnt.RaisedBy.RemoteEndPoint);
            return;
        }

        WeaponDebugger.ApplyWeapon(character, type, ammo, hitpoints);
    }
}
