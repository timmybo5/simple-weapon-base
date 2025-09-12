namespace SWB.Base.Attachments;

public abstract class SightAttachment : Attachment
{
	public override string Name => "Sight";
	public override AttachmentCategory Category => AttachmentCategory.Sight;
	public override string Description => "An optical sight that allows the user to look through a partially reflecting glass element and see an illuminated projection of an aiming point or some other image superimposed on the field of view.";
	public override string[] Positives => new string[]
	{
		"Precision sight picture",
		"Increases accuracy by 5%"
	};

	public override string[] Negatives => new string[]
	{
	};

	public override StatsModifier StatsModifier { get; set; } = new()
	{
		Spread = -0.05f,
	};

	[Property]
	public virtual AimInfo AimInfo { get; set; } = new()
	{
		// By default don't modify anything
		Sensitivity = -1f,
		SpreadModifier = -1f,
		AimInFOVSpeed = -1f,
		AimOutFOVSpeed = -1f,
	};
	// Used to restore the weapon's AimInfo on unequip
	AimInfo oldAimInfo;


	/// <summary>The new aim position offset</summary>
	[Property, Group( "Sight" )] public AngPos AimAnimData { get; set; }
	AngPos oldAimAnimData;

	public override void OnEquip()
	{
		oldAimAnimData = Weapon.AimAnimData;
		oldAimInfo = Weapon.AimInfo;

		Weapon.AimAnimData = AimAnimData;

		// Don't modify unset values
		var aimInfo = AimInfo.Clone();
		aimInfo.FillDefaults( oldAimInfo );

		Weapon.AimInfo = aimInfo;
	}

	public override void OnUnequip()
	{
		Weapon.AimAnimData = oldAimAnimData;
		Weapon.AimInfo = oldAimInfo;
	}
}
