using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	[Sync] public string clothingJSON { get; set; }
	ClothingContainer clothingContainer;
	List<SkinnedModelRenderer> clothingRenderers = new();

	void ApplyClothes( Connection connection )
	{
		clothingJSON = connection.GetUserData( "avatar" );
		clothingContainer = ClothingContainer.CreateFromJson( clothingJSON );
		clothingContainer.Apply( BodyRenderer );
		UpdateClothingRenderers();
	}

	void UpdateClothingRenderers()
	{
		BodyRenderer.GameObject.Children.ForEach( c =>
		{
			if ( c.Name.StartsWith( "Clothing" ) )
			{
				var renderer = c.Components.Get<SkinnedModelRenderer>();
				clothingRenderers.Add( renderer );
			}
		} );
	}

	void UpdateClothes()
	{
		// Can take a while to spawn on clients so we check here until they are spawned in
		if ( clothingRenderers.Count == 0 )
			UpdateClothingRenderers();

		if ( !IsProxy && !IsBot && IsAlive && IsFirstPerson )
		{
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
		else
		{
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
		}

		// Fix for body teleporting from death pos and being visible onEnabled
		if ( !IsAlive )
		{
			BodyRenderer.Tint = Color.Transparent;
		}
		else
		{
			BodyRenderer.Tint = Color.White;
		}

		clothingRenderers.ForEach( c =>
		{
			c.RenderType = BodyRenderer.RenderType;
			c.Tint = BodyRenderer.Tint;
		} );
	}
}
