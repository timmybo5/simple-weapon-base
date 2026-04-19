using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public Dresser Dresser { get; set; }
	List<SkinnedModelRenderer> clothingRenderers = new();
	ModelRenderer.ShadowRenderType lastBodyRenderType;
	Color lastBodyTint;
	int lastChildrenCount = -1;
	bool isDressed;

	async void ApplyClothes()
	{
		if ( Application.IsDedicatedServer ) return;
		await Dresser.Apply();
		isDressed = true;
	}

	/// <summary>Can be called multiple times</summary>
	public virtual void OnDressed( List<SkinnedModelRenderer> clothingRenderers ) { }

	void UpdateClothingRenderers()
	{
		clothingRenderers.Clear();
		lastBodyRenderType = ModelRenderer.ShadowRenderType.Off;
		lastBodyTint = Color.Black;

		BodyRenderer.GameObject.Children.ForEach( c =>
		{
			if ( c.Name.StartsWith( "Clothing", System.StringComparison.OrdinalIgnoreCase ) )
			{
				var renderer = c.Components.Get<SkinnedModelRenderer>();
				clothingRenderers.Add( renderer );
			}
		} );

		// Can take a while to spawn on clients
		OnDressed( clothingRenderers );
	}

	void UpdateClothes()
	{
		if ( !isDressed || Dresser.IsDressing ) return;

		if ( BodyRenderer.GameObject.Children.Count != lastChildrenCount )
		{
			lastChildrenCount = BodyRenderer.GameObject.Children.Count;
			UpdateClothingRenderers();
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
