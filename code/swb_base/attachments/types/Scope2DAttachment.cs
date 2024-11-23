using SWB.Base.UI;

namespace SWB.Base.Attachments;

public abstract class Scope2DAttachment : Attachment
{
	public override string Name => "2D Scope";
	public override AttachmentCategory Category => AttachmentCategory.Sight;
	public override string Description => "A high magnification scope that is utilized for firing at long ranges.";
	public override string[] Positives => new string[]
	{
		"x12 magnification",
		"100% accurate while scoped in"
	};

	public override string[] Negatives => new string[]
	{
	};

	/// <summary>The new aim position offset</summary>
	[Property, Group( "Scope" )] public AngPos AimAnimData { get; set; }
	AngPos oldAimAnimData;

	/// <summary>Scope Information</summary>
	[Property, Group( "Scope" )] public virtual ScopeInfo ScopeInfo { get; set; } = new();
	ScopeInfo oldScopeInfo = new();

	SniperScope sniperScope;

	public override void OnEquip()
	{
		oldAimAnimData = Weapon.AimAnimData;
		oldScopeInfo = Weapon.ScopeInfo;

		Weapon.Scoping = true;
		Weapon.AimAnimData = AimAnimData;
		Weapon.ScopeInfo = ScopeInfo;
	}

	public override void OnUnequip()
	{
		Weapon.Scoping = false;
		Weapon.AimAnimData = oldAimAnimData;
		Weapon.ScopeInfo = oldScopeInfo;
	}

	public override void CreateHudElements()
	{
		base.CreateHudElements();
		sniperScope = new SniperScope( Weapon, Weapon.ScopeInfo.LensTexture, Weapon.ScopeInfo.ScopeTexture );
		Weapon.RootPanel.Panel.AddChild( sniperScope );
	}

	public override void DestroyHudElements()
	{
		base.DestroyHudElements();
		sniperScope?.Delete();
	}
}
