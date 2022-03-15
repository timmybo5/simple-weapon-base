//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Sniper scope shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
	#include "common/features.hlsl"
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
	VrForward();													// Indicates this shader will be used for main rendering
	Depth( "vr_depth_only.vfx" ); 									// Shader that will be used for shadowing and depth prepass
	ToolsVis( S_MODE_TOOLS_VIS ); 									// Ability to see in the editor
	ToolsWireframe( "vr_tools_wireframe.vfx" ); 					// Allows for mat_wireframe to work
	ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" ); 	// Shows how expensive drawing is in debug view
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
	#define S_TRANSLUCENT 1
	#define BLEND_MODE_ALREADY_SET
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
  	#include "common/pixel.hlsl"
	#include "common.fxc"
	  
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );
	RenderState(AlphaToCoverageEnable, true);

	CreateInputTexture2D( ReticleTexture, Srgb, 8, "", "",  "Textures", Default3( 1, 1, 1 ) );
	CreateTexture2D( g_tReticle ) < Channel( RGBA, None( ReticleTexture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	TextureAttribute( g_tReticle, g_tReticle );

	CreateInputTexture2D( MaskTexture, Srgb, 8, "", "",  "Textures", Default3( 1, 1, 1 ) );
	CreateTexture2D( g_tMask ) < Channel( RGBA, None( MaskTexture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	TextureAttribute( g_tMask, g_tMask );

	CreateTexture2D( g_tScopeRT ) < Attribute( "ScopeRT" ); SrgbRead( false ); >;

	float g_flReticleScale < UiType( Slider ); Default( 2.5f ); UiGroup( "Reticle" ); >;
	float2 g_vReticleOffset < UiType( Slider ); Default2( 0.5f, 0.5f ); UiGroup( "Reticle" ); Range2( 0.0f, 0.0f, 1.0f, 1.0f ); >;
	float g_flReticleCircleScale < UiType( Slider ); Default( 2.0f ); UiGroup( "Reticle" ); >;
	float g_flReticleLiveOffsetMul < UiType( Slider ); Default( 0.25f ); Range( 0.0f, 2.0f ); UiGroup( "Reticle" ); >;
	
	float g_flMaskLiveOffsetMul < UiType( Slider ); Default( 1.5f ); Range( 0.0f, 2.0f ); UiGroup( "Mask" ); >;
	float g_flMaskScale < UiType( Slider ); Default( 1.0f ); UiGroup( "Mask" ); >;
	float2 g_vMaskOffset < UiType( Slider ); Default2( 0.0f, 0.0f ); UiGroup( "Mask" ); Range2( -1.0f, -1.0f, 1.0f, 1.0f ); >;
	
	float g_flScreenLiveOffsetMul < UiType( Slider ); Default( 0.25f ); Range( 0.0f, 2.0f ); UiGroup( "Misc" ); >;

	float2 CalcSquareUvs( float2 uvs ) 
	{
		float fAspectRatio = g_vViewportSize.y / g_vViewportSize.x;

		float2 location = float2( 0.5f, 0.5f );
		float width = 0.25f;
		float height = 0.25f;

		uvs.y *= fAspectRatio;
		location.y *= fAspectRatio;
		float x = location.x - uvs.x;
		float y = location.y - uvs.y;
		
		return float2( -x, -y );
	}

	//
	// Main
	//
	PixelOutput MainPs( PixelInput i )
	{
		Material material = GatherMaterial( i );
		PixelOutput o = FinalizePixelMaterial( i, material );

		float2 vPositionUvWithOffset = ( ( i.vPositionSs.xy ) - g_vViewportOffset ) / g_vRenderTargetSize;
		float fAspectRatio = g_vViewportSize.y / g_vViewportSize.x;

		// TODO: UVs are fixed but scaling is still wacky
		float2 squareUvs = CalcSquareUvs( vPositionUvWithOffset );

		//
		// Circle mask
		//
		float2 vCircleMaskUvs = float2( squareUvs.x, squareUvs.y ) * g_flMaskScale;
		vCircleMaskUvs += g_vReticleOffset + ( g_vMaskOffset * g_flMaskLiveOffsetMul );
		float fMaskMix = Tex2D( g_tMask, vCircleMaskUvs );

		//
		// Reticle texture
		//
		float2 vReticleUvs = float2( squareUvs.x, squareUvs.y ) * g_flReticleScale;
		vReticleUvs += g_vReticleOffset - ( g_vMaskOffset * g_flReticleLiveOffsetMul );
		float4 vReticleColor = Tex2D( g_tReticle, vReticleUvs );

		//
		// Put it all together
		//
		float2 vScreenUvs = vPositionUvWithOffset - ( g_vMaskOffset * g_flScreenLiveOffsetMul );
		o.vColor = Tex2D( g_tScopeRT, vScreenUvs );
		o.vColor.xyz = SrgbGammaToLinear( o.vColor.xyz * vReticleColor.xyz );
		o.vColor.xyz *= fMaskMix;
		o.vColor.xy *= sqrt( fMaskMix );

		o.vColor.a = 1.0;

		return o;
	}
}