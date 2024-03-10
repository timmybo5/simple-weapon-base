﻿using SWB.Base.UI;

namespace SWB.Base;

public partial class Weapon
{
	public ScreenPanel ScreenPanel { get; set; }
	public PanelComponent RootPanel { get; set; }

	public virtual void CreateUI()
	{
		ScreenPanel = Components.Create<ScreenPanel>();
		ScreenPanel.Opacity = 1;
		ScreenPanel.ZIndex = 1;

		var rootPanel = Components.Create<RootWeaponDisplay>();
		rootPanel.Player = Owner;
		rootPanel.Weapon = this;
		RootPanel = rootPanel;
	}

	public virtual void DestroyUI()
	{
		ScreenPanel?.Destroy();
		RootPanel?.Destroy();
	}

	void BroadcastUIEvent( string name, object value )
	{
		if ( RootPanel is null ) return;

		foreach ( var panel in RootPanel.Panel.Children )
		{
			panel.CreateEvent( name, value );
		}
	}
}