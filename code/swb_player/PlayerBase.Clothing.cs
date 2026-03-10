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

	async void ApplyClothes()
	{
		await Dresser.Apply();
		isDressed = true;
	}

	public virtual void OnDressed( List<SkinnedModelRenderer> clothingRenderers ) { }

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

		if ( !calledOnDressed )
		{
			calledOnDressed = true;
			OnDressed( clothingRenderers );
		}
	}

	void UpdateClothes()
	{
		if ( !isDressed || Dresser.IsDressing ) return;

		// Can take a while to spawn on clients so we check here until they are spawned in
		if ( clothingRenderers.Count == 0 )
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
