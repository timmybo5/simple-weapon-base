//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
    CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
    Description = "Reflex Sight Shader"; 
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
COMMON
{
    #include "common/shared.hlsl"
    #define S_TRANSLUCENT 1
    #define BLEND_MODE_ALREADY_SET
    #define COLOR_WRITE_ALREADY_SET
	
	// Required for us to set depth
	#define DEPTH_STATE_ALREADY_SET
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
        return FinalizeVertex( o );
    }
}

//=========================================================================================================================

PS
{
    #define CUSTOM_TEXTURE_FILTERING
    SamplerState TextureFiltering < Filter( ANISOTROPIC ); AddressU( CLAMP ); AddressV( CLAMP ); AddressW( CLAMP ); MaxAniso( 8 ); >;

	RenderState( MultisampleEnable, true );
    //RenderState( BlendEnable, true );
    //RenderState( SrcBlend, SRC_ALPHA ); 
    //RenderState( DstBlend, INV_SRC_ALPHA );
    RenderState( AlphaToCoverageEnable, true );
    RenderState( ColorWriteEnable0, RGBA );
	
	//RenderState( DepthEnable, false ); // Disable depth testing
	//RenderState( DepthWriteEnable, true ); // Don't write to the depth buffer

    #include "common/pixel.hlsl"


    //=====================================================================================================================
    // Properties
    //=====================================================================================================================
    float g_flDepth < UiType( Slider ); Default( 1.0 ); Range(0.0f, 50.0f); UiGroup( "Reflex Sight,0/Depth,1/1" ); >;
    bool g_bUseInfiniteDepth < UiType( CheckBox ); UiGroup( "Reflex Sight,0/Depth,1/2" ); >;

    float g_flSightTexScale < UiType( Slider ); Default( 1.0 ); Range(0.0f, 5.0f); UiGroup( "Reflex Sight,0/Appearance,2/1" ); >;
	  float g_flSightTexGlow < UiType( Slider ); Default( 3.0 ); Range(0.0f, 20.0f); UiGroup( "Reflex Sight,0/Appearance,2/2" ); >;
    float3 g_vSightColor < UiType( Color ); Default3( 1.0, 0.0, 0.0 ); UiGroup( "Reflex Sight,0/Appearance,2/3" ); >;
	  float g_flSightAlpha < UiType( Slider ); Default( 0.75 ); Range(0.0f, 1.0f); UiGroup( "Reflex Sight,0/Appearance,2/4" ); >;
	  float3 g_vGlassColor < UiType( Color ); Default3( 0.0, 0.0, 0.0 ); UiGroup( "Reflex Sight,0/Appearance,2/5" ); >;
	  float g_flGlassAlpha < UiType( Slider ); Default( 0.7 ); Range(0.0f, 1.0f); UiGroup( "Reflex Sight,0/Appearance,2/6" ); >;
    
    CreateInputTexture2D( TextureReticleMask, Srgb, 8, "", "_color", "Reflex Sight,0/Appearance,2/7", Default3( 1.0, 1.0, 1.0 ) );
    CreateTexture2DWithoutSampler( g_tReticleMask ) < Channel( RGB, Box( TextureReticleMask ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
    //=====================================================================================================================

    float2 CalculateTextureCoords( float2 vTextureCoords, float3 vTangentViewVector ) 
    {
        float2 vCalcTextureCoords; 

        if ( g_bUseInfiniteDepth ) 
        {
            vCalcTextureCoords = vTangentViewVector.xy / g_flSightTexScale;
        } 
        else
        {
            vCalcTextureCoords = (
                vTextureCoords - float2(0.5, 0.5) + (vTangentViewVector.xy * g_flDepth)
            ) / g_flSightTexScale;
        }

        return vCalcTextureCoords + float2( 0.5, 0.5 );
    }

    PixelOutput MainPs( PixelInput i )
    {
        PixelOutput o;
        Material m = GatherMaterial( i );
        
        float3 vPositionWs = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
        float3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPositionWs.xyz );

        float3 vNormalWs = normalize( i.vNormalWs.xyz );
        float3 vTangentUWs = i.vTangentUWs.xyz;
        float3 vTangentVWs = i.vTangentVWs.xyz;
        float3 vTangentViewVector = Vec3WsToTs( vCameraToPositionDirWs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz );

        float2 vCalcTextureCoords = CalculateTextureCoords( i.vTextureCoords, vTangentViewVector );
        i.vTextureCoords = vCalcTextureCoords;

        o.vColor.rgba = float4( g_vSightColor, g_flSightAlpha );
        
        float4 vTextureSample = Tex2DS( g_tReticleMask, TextureFiltering, vCalcTextureCoords );

		if ( vTextureSample.r < 0.2 ) {
			// Glass
			o.vColor.rgba = float4( g_vGlassColor, g_flGlassAlpha );
		} else {
			// Glow
			o.vColor.rgba *= (g_flSightTexGlow+vTextureSample.r);
		}
        
        return o;
    }
}