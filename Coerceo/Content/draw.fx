float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;

float4 DiffuseColor;
float Alpha;

float3 LightPosition[16];
float LightFalloff[16];
float3 LightColor[16];
float ActiveLights;

texture Texture;
texture NormalMap;
float4x4 UVTransform;

sampler normalMapSampler = sampler_state
{
	Texture = (NormalMap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};


struct TexturedVertexShaderInput
{
	float4 Position : SV_Position0;
	float3 Normal : NORMAL0;
	float2 Texcoord : TEXCOORD0;
	float3 Tangent : TEXCOORD1;
	float3 BiNormal : TEXCOORD2;
};

struct TexturedVertexShaderOutput
{
	float4 Position : SV_Position0;
	float2 Texcoord : TEXCOORD0;
	float3 Normal : TEXCOORD2;
	float3 WorldPos : TEXCOORD3;
	float3 Tangent : TEXCOORD4;
	float3 BiNormal : TEXCOORD5;
};


TexturedVertexShaderOutput TexturedVertexShaderFunction(TexturedVertexShaderInput input)
{
    TexturedVertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.WorldPos = worldPosition;

	output.Normal = normalize(mul(input.Normal, World));
	output.Tangent = normalize(mul(input.Tangent, World));
	output.BiNormal = normalize(mul(input.BiNormal, World));

	float4 tTexcoord = float4(input.Texcoord[0], input.Texcoord[1], 1, 1);
	tTexcoord = mul(tTexcoord, UVTransform);

	output.Texcoord = float2(tTexcoord[0], tTexcoord[1]);
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSTexturedColor(TexturedVertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	float4 texColor = tex2D(diffuseSampler, input.Texcoord);
	float4 normalMap = tex2D(normalMapSampler, input.Texcoord);
	output.Color = float4(0,0,0,1);

	normalMap = (normalMap * 2.0f) - 1.0f;

    // Calculate the normal from the data in the bump map.
    float3 bumpNormal = (normalMap.x * input.Tangent) + (normalMap.y * input.BiNormal) + (normalMap.z * input.Normal);
	
    // Normalize the resulting bump normal.
    bumpNormal = normalize(bumpNormal);

	for (int i = 0; i < ActiveLights; ++i)
	{
		float lightAttenuation = 1 - (length(input.WorldPos - LightPosition[i]) / LightFalloff[i]);
		float lightIntensity = clamp(dot(bumpNormal, input.WorldPos - LightPosition[i]), 0, 1);
		output.Color += saturate(texColor * DiffuseColor * float4(LightColor[i],1) * lightAttenuation * lightIntensity);
	}

	output.Color.a = texColor.a * Alpha;
	clip(texColor.a < 0.1f ? -1:1);

	output.Color.r = floor(output.Color.r * 16) / 16;
	output.Color.g = floor(output.Color.g * 16) / 16;
	output.Color.b = floor(output.Color.b * 16) / 16;

    return output;
}

PixelShaderOutput PSTexturedColorNoLight(TexturedVertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	float4 texColor = tex2D(diffuseSampler, input.Texcoord);
    output.Color = texColor * DiffuseColor;
	output.Color.a = texColor.a * Alpha;
	clip(texColor.a < 0.1f ? -1 : 1);

    return output;
}

technique DrawTextured
{
    pass Pass1
    {
		AlphaBlendEnable = false;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		//////ZEnable = false;
		//////ZWriteEnable = true;
		//////ZFunc = LessEqual;
		//CullMode = None;
		
        VertexShader = compile vs_4_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_4_0 PSTexturedColor();
    }
}

technique DrawNoLight
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 TexturedVertexShaderFunction();
		PixelShader = compile ps_4_0 PSTexturedColorNoLight();
	}
}

