using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public Dresser Dresser { get; set; }
	List<SkinnedModelRenderer> clothingRenderers = new();
	ModelRenderer.ShadowRenderType lastBodyRenderType;
	Color lastBodyTint;
	bool calledOnDressed;
	bool isDressed;
	int clothingCount;

	async void ApplyClothes()
	{
		List<ClothingContainer.ClothingEntry> wearable = new();

		if ( Dresser.Source == Dresser.ClothingSource.Manual )
		{
			// Manual (bots)
			wearable = GetWearableEntries( Dresser.Clothing );
			Dresser.Clothing = wearable;
		}
		else if ( Dresser.Source == Dresser.ClothingSource.OwnerConnection )
		{
			// Owner clothing
			var container = ClothingContainer.CreateFromJson( Network.Owner.GetUserData( "avatar" ) );
			wearable = GetWearableEntries( container?.Clothing );
		}
		else if ( Dresser.Source == Dresser.ClothingSource.LocalUser )
		{
			// Local clothing
			var container = ClothingContainer.CreateFromLocalUser();
			wearable = GetWearableEntries( container?.Clothing );
		}

		clothingCount = wearable.Count;

		await Dresser.Apply();
		isDressed = true;
	}

	public virtual void OnDressed( List<SkinnedModelRenderer> clothingRenderers ) { }

	List<ClothingContainer.ClothingEntry> GetWearableEntries( List<ClothingContainer.ClothingEntry> source )
	{
		var result = new List<ClothingContainer.ClothingEntry>();
		if ( source is null ) return result;

		foreach ( var entry in source )
		{
			var clothing = entry?.Clothing;
			if ( clothing is null ) continue;

			var compatible = true;
			foreach ( var kept in result )
			{
				var keptClothing = kept?.Clothing;
				if ( keptClothing is null ) continue;

				if ( !clothing.CanBeWornWith( keptClothing ) || !keptClothing.CanBeWornWith( clothing ) )
				{
					compatible = false;
					break;
				}
			}

			if ( compatible )
				result.Add( entry );
		}

		return result;
	}

	void UpdateClothingRenderers()
	{
		clothingRenderers.Clear();
		lastBodyRenderType = ModelRenderer.ShadowRenderType.Off;
		lastBodyTint = Color.Black;

		BodyRenderer.GameObject.Children.ForEach( c =>
		{
			if ( c.Name.StartsWith( "Clothing" ) )
			{
				var renderer = c.Components.Get<SkinnedModelRenderer>();
				clothingRenderers.Add( renderer );
			}
		} );

		// Can take a while to spawn on clients
		if ( !calledOnDressed && clothingRenderers.Count == clothingCount )
		{
			calledOnDressed = true;
			OnDressed( clothingRenderers );
		}
	}

	void UpdateClothes()
	{
		if ( !isDressed || Dresser.IsDressing ) return;

		if ( !calledOnDressed )
		{
			UpdateClothingRenderers();
			return;
		}

		var desiredRenderType = ModelRenderer.ShadowRenderType.On;
		var desiredTint = Color.White;

		if ( !IsProxy && !IsBot && IsAlive && IsFirstPerson )
			desiredRenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;

		// Fix for body teleporting from death pos and being visible onEnabled
		if ( !IsAlive )
			desiredTint = Color.Transparent;

		var updatedRenderType = false;
		var updatedTint = false;

		if ( lastBodyRenderType != desiredRenderType )
		{
			lastBodyRenderType = desiredRenderType;
			BodyRenderer.RenderType = desiredRenderType; // Performance drain
			updatedRenderType = true;
		}

		if ( lastBodyTint != desiredTint )
		{
			lastBodyTint = desiredTint;
			BodyRenderer.Tint = desiredTint; // Performance drain
			updatedTint = true;
		}

		if ( updatedRenderType || updatedTint )
		{
			clothingRenderers.ForEach( c =>
			{
				if ( c is null ) return;
				if ( updatedRenderType )
					c.RenderType = BodyRenderer.RenderType; // Performance drain
				if ( updatedTint )
					c.Tint = BodyRenderer.Tint; // Performance drain
			} );
		}
	}
}
