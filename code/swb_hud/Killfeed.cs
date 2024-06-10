using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Base;
using SWB.Player;
using SWB.Shared;
using System;
using System.Linq;

namespace SWB.HUD;

public class Killfeed : Panel
{
	IPlayerBase player;

	public Killfeed( IPlayerBase player )
	{
		this.player = player;
		StyleSheet.Load( "/swb_hud/Killfeed.cs.scss" );
	}

	public void AddKillEntry( Guid attackerId, Guid victimId, string inflictor )
	{
		if ( inflictor is null ) return;

		var attackerGO = Scene.Directory.FindByGuid( attackerId );
		var attacker = attackerGO?.Components.Get<PlayerBase>();
		var victimGO = Scene.Directory.FindByGuid( victimId );
		var victim = victimGO?.Components.Get<PlayerBase>();
		var weapon = WeaponRegistry.Instance?.GetWeapon( inflictor );
		if ( attacker is null || victim is null || weapon is null ) return;

		if ( ChildrenCount >= 10 )
		{
			Children.First().Delete();
		}

		var entryP = Add.Panel( "entry" );

		var attackerName = attacker.IsBot ? attackerGO.Name : attacker.Network.OwnerConnection.DisplayName;
		entryP.Add.Label( attackerName, "name " + (!attackerGO.IsProxy && !attacker.IsBot ? "self" : "") );
		entryP.Add.Image( weapon.Icon, "icon" );

		var victimName = victim.IsBot ? victimGO.Name : victim.Network.OwnerConnection.DisplayName;
		entryP.Add.Label( victimName, "name " + (!victimGO.IsProxy && !victim.IsBot ? "self" : "") );
		DeleteAsync( entryP );
	}

	async void DeleteAsync( Panel panel )
	{
		await GameTask.DelaySeconds( 5 );
		panel.Delete();
	}
}
