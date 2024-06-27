using Sandbox.UI;
using Sandbox.UI.Construct;
using SWB.Base.Attachments;
using System.Collections.Generic;

namespace SWB.Base.UI;

public class CustomizationMenu : Panel
{
	Weapon weapon;

	Panel categoryWrapper;
	Panel attachmentWrapper;
	Panel descriptionWrapper;
	Panel activeCategoryP;
	Panel activeAttachmentP;

	Attachment selectedAttachment;
	Attachment hoveredAttachment;

	Dictionary<AttachmentCategory, List<Attachment>> attachmentsPerCategory = new();

	public CustomizationMenu( Weapon weapon )
	{
		this.weapon = weapon;
		FillAttachmentsPerCategory();
		StyleSheet.Load( "/swb_base/ui/CustomizationMenu.cs.scss" );

		categoryWrapper = Add.Panel( "categoryWrapper" );
		categoryWrapper.Add.Label( weapon.DisplayName, "weaponName" );
		attachmentWrapper = Add.Panel( "attachmentWrapper" );
		descriptionWrapper = Add.Panel( "descriptionWrapper" );

		CreateCategoryPanels();
	}

	void FillAttachmentsPerCategory()
	{
		weapon.Attachments.ForEach( attachment =>
		{
			if ( attachment.Hide ) return;
			if ( attachmentsPerCategory.TryGetValue( attachment.Category, out var attachments ) )
				attachments.Add( attachment );
			else
				attachmentsPerCategory.Add( attachment.Category, new List<Attachment>() { attachment } );
		} );
	}

	void CreateCategoryPanels()
	{
		foreach ( var entry in attachmentsPerCategory )
		{
			var categoryP = categoryWrapper.Add.Panel( "category" );
			categoryP.Add.Label( entry.Key.ToString(), "name" );
			categoryP.Add.Label( "", "attName" );

			var catActiveAttachP = categoryP.Add.Panel( "activeAttachment" );
			catActiveAttachP.Add.Label( "", "name" );

			var iconWrapperP = catActiveAttachP.Add.Panel( "iconWrapper" );
			iconWrapperP.Add.Image( "", "icon" );

			var activeCatAttachment = weapon.GetActiveAttachmentForCategory( entry.Key );
			if ( activeCatAttachment is not null )
				SetAttachmentOnCategoryPanel( categoryP, activeCatAttachment );


			categoryP.AddEventListener( "onmousedown", () =>
			{
				if ( activeCategoryP != categoryP )
				{
					PlaySound( "swb_click" );
					activeCategoryP?.SetClass( "active", false );
					categoryP.SetClass( "active", true );
					activeCategoryP = categoryP;
					selectedAttachment = null;
					descriptionWrapper.DeleteChildren();
					CreateAttachmentPanels( entry.Key );
				}
			} );

			categoryP.AddEventListener( "onmouseover", () =>
			{
				PlaySound( "ui.button.over" );
			} );
		}
	}

	void SetAttachmentOnCategoryPanel( Panel categoryP, Attachment attachment )
	{
		categoryP.SetClass( "hasAttachment", true );

		var attachP = categoryP.GetChild( 2 );
		var nameL = (Label)attachP.GetChild( 0 );
		var iconWrapper = attachP.GetChild( 1 );
		var iconP = (Image)iconWrapper.GetChild( 0 );

		nameL.Text = attachment.Name;
		iconP.SetTexture( attachment.IconPath );
	}

	void CreateAttachmentPanels( AttachmentCategory category )
	{
		attachmentWrapper.DeleteChildren();
		activeAttachmentP = null;

		if ( !attachmentsPerCategory.TryGetValue( category, out var attachments ) ) return;

		var activeAttachment = weapon.GetActiveAttachmentForCategory( category );

		attachments.ForEach( attachment =>
		{
			var attachmentP = attachmentWrapper.Add.Panel( "attachment" );
			attachmentP.Add.Label( attachment.Name, "name" );
			var iconWrapperP = attachmentP.Add.Panel( "iconWrapper" );
			iconWrapperP.Add.Image( attachment.IconPath, "icon" );

			var isActiveAttachment = attachment == activeAttachment;
			attachmentP.SetClass( "active", isActiveAttachment );

			if ( isActiveAttachment )
			{
				activeAttachmentP = attachmentP;
				selectedAttachment = attachment;
				CreateDescriptionPanel( attachment );
			}

			attachmentP.AddEventListener( "onmousedown", () =>
			{
				activeAttachmentP?.SetClass( "active", false );

				if ( activeAttachmentP != attachmentP )
				{
					PlaySound( "swb_equip" );
					attachment.EquipBroadCast();
					attachmentP.SetClass( "active", true );
					activeAttachmentP = attachmentP;
					selectedAttachment = attachment;
					SetAttachmentOnCategoryPanel( activeCategoryP, attachment );
				}
				else
				{
					PlaySound( "swb_unequip" );
					attachment.UnEquipBroadCast();
					activeCategoryP.SetClass( "hasAttachment", false );
					activeAttachmentP = null;
					selectedAttachment = null;
				}
			} );

			attachmentP.AddEventListener( "onmouseover", () =>
			{
				hoveredAttachment = attachment;
				PlaySound( "ui.button.over" );
				CreateDescriptionPanel( attachment );
			} );

			attachmentP.AddEventListener( "onmouseout", () =>
			{
				if ( hoveredAttachment != attachment ) return;
				if ( selectedAttachment is not null )
					CreateDescriptionPanel( this.selectedAttachment );
				else
					descriptionWrapper.DeleteChildren();
			} );
		} );
	}

	private void CreateDescriptionPanel( Attachment attach )
	{
		descriptionWrapper.DeleteChildren();
		descriptionWrapper.Add.Label( attach.Description, "description" );

		var posWrapper = descriptionWrapper.Add.Panel( "posWrapper" );
		foreach ( var pos in attach.Positives )
		{
			posWrapper.Add.Label( "> " + pos, "label" );
		}

		var negWrapper = descriptionWrapper.Add.Panel( "negWrapper" );
		foreach ( var neg in attach.Negatives )
		{
			negWrapper.Add.Label( "> " + neg, "label" );
		}
	}
}
