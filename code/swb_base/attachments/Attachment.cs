using SWB.Shared;
using System;

namespace SWB.Base.Attachments;

public enum AttachmentCategory
{
	Barrel,
	Sight,
	Grip,
	Rail,
	Magazine,
	Muzzle,
	Stock,
	Other,
	Special,
	Tactical,
	Laser,
	None,
}

/*
 * Attachment base that allows for weapon bone parenting as well as bodygroup changes simultaneously
 */

[Group( "SWB Attachments" )]
public abstract class Attachment : Component, IComparable<Attachment>
{
	/// <summary>Display name (needs to be unique)</summary>
	public virtual string Name => "";

	/// <summary>Display description</summary>
	public virtual string Description => "";

	/// <summary>Only 1 active attachment per category, this is also used to determine what effect attachment to overide</summary>
	public virtual AttachmentCategory Category => AttachmentCategory.None;

	/// <summary>List of positive attributes</summary>
	public virtual string[] Positives => Array.Empty<string>();

	/// <summary>List of negative attributes</summary>
	public virtual string[] Negatives => Array.Empty<string>();

	/// <summary>Weapon stats changer</summary>
	public virtual StatsModifier StatsModifier { get; set; }

	/// <summary>Path to an image that represent the attachment on the HUD</summary>
	public virtual string IconPath => "";

	/// <summary>Path to the attachment model</summary>
	public virtual string ModelPath => "";

	/// <summary>Name of the model attachment used for new effect origins</summary>
	public virtual string EffectAttachmentOrBone { get; set; } = "";

	/// <summary>Hide this attachment in menus</summary>
	public virtual bool Hide { get; set; } = false;

	/// <summary>Depends on another attachment (e.g. rail/mount)</summary>
	[Property] public Attachment RequiresAttachment { get; set; }

	/// <summary>Name of the bone you want to attach the model to</summary>
	[Property, Group( "Model Parenting" )] public virtual string Bone { get; set; }

	/// <summary>Viewmodel scale</summary>
	[Property, Group( "Model Parenting" )] public virtual Vector3 ViewModelScale { get; set; } = Vector3.One;

	/// <summary>Worldmodel scale</summary>
	[Property, Group( "Model Parenting" )] public virtual Vector3 WorldModelScale { get; set; } = Vector3.One;

	/// <summary>The name of the body group</summary>
	[Property, Group( "BodyGroup" )] public virtual string BodyGroup { get; set; }

	/// <summary>The name of the body group choice</summary>
	[Property, Group( "BodyGroup" )] public virtual int BodyGroupChoice { get; set; } = 0;

	/// <summary>The default target body group value</summary>
	[Property, Group( "BodyGroup" )] public virtual int BodyGroupDefault { get; set; } = 0;

	/// <summary>If already equipped</summary>
	[Sync] public bool Equipped { get; private set; }

	public Weapon Weapon { get; private set; }
	public SkinnedModelRenderer ViewModelRenderer { get; private set; }
	public SkinnedModelRenderer WorldModelRenderer { get; private set; }

	private int equipTries = 0;
	private bool equippedOnClient = false;

	protected override void OnAwake()
	{
		Weapon = Components.Get<Weapon>();
	}

	private void SetBodyGroup( int choice )
	{
		if ( string.IsNullOrEmpty( BodyGroup ) ) return;

		if ( !IsProxy )
			Weapon.ViewModelRenderer.SetBodyGroup( BodyGroup, choice );

		Weapon.WorldModelRenderer.SetBodyGroup( BodyGroup, choice );
	}

	private void CreateModel( bool isViewModel = false )
	{
		if ( string.IsNullOrEmpty( ModelPath ) || string.IsNullOrEmpty( Bone ) ) return;

		var attachmentGO = new GameObject( true, "Attachment" );
		attachmentGO.Tags.Add( TagsHelper.Attachment );

		var attachmentRenderer = attachmentGO.Components.Create<SkinnedModelRenderer>();
		attachmentRenderer.Model = Model.Load( ModelPath );
		attachmentRenderer.Enabled = true;

		if ( isViewModel )
		{
			attachmentRenderer.WorldScale = ViewModelScale;
			ViewModelRenderer = attachmentRenderer;
			attachmentGO.Flags |= GameObjectFlags.NotNetworked;
			ModelUtil.ParentToBone( attachmentGO, Weapon.ViewModelRenderer, Bone );
		}
		else
		{
			attachmentRenderer.WorldScale = WorldModelScale;
			WorldModelRenderer = attachmentRenderer;
			ModelUtil.ParentToBone( attachmentGO, Weapon.WorldModelRenderer, Bone );
		}
	}

	private void CreateModels()
	{
		if ( !IsProxy )
			CreateModel( true );

		CreateModel();
	}

	/// <summary>Equips the attachment for everyone</summary>
	[Broadcast]
	public virtual void EquipBroadCast()
	{
		if ( !IsValid ) return;
		Equip();
	}

	/// <summary>Equips the attachment</summary>
	public virtual void Equip()
	{
		// Log.Info( "Trying to equip -> " + Name + ", info -> equippedOnClient: " + equippedOnClient + " equipTries: " + equipTries );
		if ( equippedOnClient || !IsValid || Weapon is null ) return;
		if ( (!IsProxy && Weapon.ViewModelRenderer is null) || Weapon.WorldModelRenderer is null )
		{
			if ( equipTries > 10 ) return;
			equipTries += 1;

			async void retry()
			{
				await GameTask.Delay( 1 );
				Equip();
			}
			retry();

			return;
		}

		// Make sure there is no other active attachment in same category
		foreach ( var att in Weapon.Attachments )
		{
			if ( att.Category == Category && att.Equipped )
			{
				att.Unequip();
				break;
			}
		}

		equippedOnClient = true;

		if ( !IsProxy )
			Equipped = true;

		// Equip dependent attachment
		RequiresAttachment?.Equip();

		// BodyGroup
		SetBodyGroup( BodyGroupChoice );

		// Models
		CreateModels();

		// Stats
		StatsModifier?.Apply( Weapon );

		if ( !IsProxy )
			CreateHudElements();

		OnEquip();
	}

	/// <summary>Unequips the attachment for everyone</summary>
	[Broadcast]
	public virtual void UnEquipBroadCast()
	{
		if ( !IsValid ) return;
		Unequip();
	}

	/// <summary>Unequips the attachment</summary>
	public virtual void Unequip()
	{
		if ( !equippedOnClient ) return;
		equippedOnClient = false;

		if ( !IsProxy )
			Equipped = false;

		// Unequip dependent attachment
		RequiresAttachment?.Unequip();

		// BodyGroup
		SetBodyGroup( BodyGroupDefault );

		// Model
		ViewModelRenderer?.GameObject.Destroy();
		WorldModelRenderer?.GameObject.Destroy();

		// Stats
		StatsModifier?.Remove( Weapon );

		if ( !IsProxy )
			DestroyHudElements();

		OnUnequip();
	}

	/// <summary>Gets called after the attachment is equipped</summary>
	public abstract void OnEquip();

	/// <summary>Gets called after the attachment is unequipped</summary>
	public abstract void OnUnequip();

	/// <summary>Gets called when the weapon is creating its HUD elements</summary>
	public virtual void CreateHudElements() { }

	/// <summary>Gets called when the weapon is destroying its HUD elements</summary>
	public virtual void DestroyHudElements() { }

	public int CompareTo( Attachment obj )
	{
		if ( obj == null )
			return 1;

		else
			return Name.CompareTo( obj.Name );
	}
}
