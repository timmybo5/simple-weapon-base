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


	/// <summary>The new aim position offset</summary>
	[Property, Group( "Sight" )] public AngPos AimAnimData { get; set; }
	AngPos oldAimAnimData;

	/// <summary>Weapon FOV while aiming (-1 to use default)</summary>
	[Property, Group( "Sight" )] public virtual float AimFOV { get; set; } = -1f;
	float oldAimFOV;

	/// <summary>Player FOV while aiming (-1 to use default)</summary>
	[Property, Group( "Sight" )] public virtual float AimPlayerFOV { get; set; } = -1f;
	float oldAimPlayerFOV;

	/// <summary>FOV aim in speed (-1 to use default)</summary>
	[Property, Group( "Sight" ), Title( "Aim in FOV speed" )] public virtual float AimInFOVSpeed { get; set; } = -1f;
	float oldAimInFOVSpeed;

	/// <summary>FOV aim out speed (-1 to use default)</summary>
	[Property, Group( "Sight" ), Title( "Aim out FOV speed" )] public virtual float AimOutFOVSpeed { get; set; } = -1f;
	private float oldAimOutFOVSpeed;

	/// <summary>Mouse sensitivity while aiming (-1 to use default)</summary>
	[Property, Group( "Sight" )] public virtual float AimSensitivity { get; set; } = -1;
	float oldAimSensitivity;

	public override void OnEquip()
	{
		oldAimAnimData = Weapon.AimAnimData;
		oldAimFOV = Weapon.AimFOV;
		oldAimPlayerFOV = Weapon.AimPlayerFOV;
		oldAimInFOVSpeed = Weapon.AimInFOVSpeed;
		oldAimOutFOVSpeed = Weapon.AimOutFOVSpeed;
		oldAimSensitivity = Weapon.AimSensitivity;

		Weapon.AimAnimData = AimAnimData;

		if ( AimFOV > -1 ) Weapon.AimFOV = AimFOV;
		if ( AimPlayerFOV > -1 ) Weapon.AimPlayerFOV = AimPlayerFOV;
		if ( AimInFOVSpeed > -1 ) Weapon.AimInFOVSpeed = AimInFOVSpeed;
		if ( AimOutFOVSpeed > -1 ) Weapon.AimOutFOVSpeed = AimOutFOVSpeed;
		if ( AimSensitivity > -1 ) Weapon.AimSensitivity = AimSensitivity;
	}

	public override void OnUnequip()
	{
		Weapon.AimAnimData = oldAimAnimData;
		Weapon.AimFOV = oldAimFOV;
		Weapon.AimPlayerFOV = oldAimPlayerFOV;
		Weapon.AimInFOVSpeed = oldAimInFOVSpeed;
		Weapon.AimOutFOVSpeed = oldAimOutFOVSpeed;
		Weapon.AimSensitivity = oldAimSensitivity;
	}
}
