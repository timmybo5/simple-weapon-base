using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	[Property] public Dresser Dresser { get; set; }
	List<SkinnedModelRenderer> clothingRenderers = new();
	int lastChildrenCount = -1;

	async void ApplyClothes()
	{
		if ( Application.IsDedicatedServer ) return;
		await Dresser.Apply();
	}

	/// <summary>Can be called multiple times</summary>
	public virtual void OnDressed( List<SkinnedModelRenderer> clothingRenderers ) { }

	void UpdateClothingRenderers()
	{
		clothingRenderers.Clear();

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

	protected virtual bool CanUpdateClothes()
	{
		return true;
	}

	void UpdateClothes()
	{
		if ( Application.IsDedicatedServer || !CanUpdateClothes() || !BodyRenderer.IsValid() ) return;
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

		if ( BodyRenderer.RenderType != desiredRenderType )
			BodyRenderer.RenderType = desiredRenderType; // Performance drain

		if ( BodyRenderer.Tint != desiredTint )
			BodyRenderer.Tint = desiredTint; // Performance drain

		foreach ( var c in clothingRenderers )
		{
			if ( !c.IsValid() ) continue;

			if ( c.RenderType != desiredRenderType )
				c.RenderType = desiredRenderType; // Performance drain

			if ( c.Tint != desiredTint )
				c.Tint = BodyRenderer.Tint; // Performance drain
		}
	}
}
