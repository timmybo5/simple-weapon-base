namespace SWB.Base.Attachments;

public abstract class LaserAttachment : Attachment
{
	public override string Name => "Laser";
	public override AttachmentCategory Category => AttachmentCategory.Laser;
	public override string Description => "Aids target acquisition by projecting a beam onto the target that provides a visual reference point.";

	public override bool Hide => true;

	public override string[] Positives => new string[]
	{
		"Increases accuracy by 5%"
	};

	public override string[] Negatives => new string[]
	{
		"Visibible to enemies"
	};

	public override StatsModifier StatsModifier { get; set; } = new()
	{
		Spread = -0.05f,
	};

	/*
	/// <summary>New muzzle flash effect point</summary>
	[Property, Group( "Laser" )] public override string EffectAttachmentOrBone { get; set; } = "laser_start";

	/// <summary>Laser beam particle</summary>
	[Property, Group( "Laser" )] public virtual ParticleSystem BeamParticle { get; set; } = ParticleSystem.Load( "particles/swb/laser/laser_small.vpcf" );

	/// <summary>Laser dot particle</summary>
	[Property, Group( "Laser" )] public virtual ParticleSystem DotParticle { get; set; } = ParticleSystem.Load( "particles/swb/laser/laser_dot.vpcf" );

	[Property, Group( "Laser" )] public virtual Color Color { get; set; } = Color.Red;

	SceneParticles beamParticles;
	SceneParticles dotParticles;
	*/

	public override void OnEquip()
	{
		CreateParticles();
	}

	public override void OnUnequip()
	{
		DestroyParticles();
	}

	private void CreateParticles()
	{
		/*
		DestroyParticles();

		beamParticles = new( Weapon.Scene.SceneWorld, BeamParticle );
		beamParticles.Tags.Add( TagsHelper.ViewModel );
		//beamParticles.
		//beamParticles?.SetControlPoint( 1, muzzleTransform.Value );
		//beamParticles?.SetControlPoint( 2, endPos );
		beamParticles?.SetNamedValue( "color", Color );
		// beamParticles?.SetControlPoint( 3, Color );
		beamParticles?.PlayUntilFinished( TaskSource.Create(), ( particles ) =>
		{
			//var startAttachment = Weapon.ViewModelRenderer.SceneModel.GetAttachment( EffectAttachmentOrBone );
			//var startPos = startAttachment.Value.Position;
			//var endPos = startAttachment.Value.Position + startAttachment.Value.Rotation.Forward * 9999;

			//var tr = Scene.Trace.Ray( startPos, endPos )
			//.UseHitboxes()
			//.WithoutTags( "trigger" )
			//.Size( 1.0f )
			//.IgnoreGameObjectHierarchy( Weapon.Owner.GameObject )
			//.Run();

			//var testStartPos = Weapon.GetMuzzleTransform();

			////particles?.SetControlPoint( 0, startPos );
			//particles?.SetControlPoint( 0, startPos );
			//particles?.SetControlPoint( 1, tr.EndPosition );
		} );

		//laserParticle = Particles.Create( Particle );
		//laserParticle?.SetPosition( 3, Color );

		//laserDotParticle = Particles.Create( DotParticle );
		//laserDotParticle?.SetPosition( 1, Color );
		*/
	}

	private void DestroyParticles()
	{
		/*
		beamParticles?.Delete();
		beamParticles = null;

		dotParticles?.Delete();
		dotParticles = null;
		*/
	}

	protected override void OnUpdate()
	{
		/*
		if ( !Equipped ) return;

		var startAttachment = Weapon.ViewModelRenderer.SceneModel.GetAttachment( EffectAttachmentOrBone );
		var startPos = startAttachment.Value.Position;
		var endPos = startAttachment.Value.Position + startAttachment.Value.Rotation.Forward * 9999;

		var tr = Scene.Trace.Ray( startPos, endPos )
		.UseHitboxes()
		.WithoutTags( TagsHelper.Trigger )
		.Size( 1.0f )
		.IgnoreGameObjectHierarchy( Weapon.Owner.GameObject )
		.Run();

		beamParticles?.SetControlPoint( 0, startPos );
		beamParticles?.SetControlPoint( 1, tr.EndPosition );
		*/
	}
}
