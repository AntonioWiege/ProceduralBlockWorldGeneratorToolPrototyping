/*Antonio Wiege POM like implementation based on Catlike Coding Rendering Tutorial catlikecoding.com rendering part 20 */
//& more via https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
Shader "ProceduralBlockLandscape/AntonioWiegeParallaxOcclusionMapping"
{
	Properties
	{
		_HeightTex("Height texture", 2D) = "white" {}
		_AlbedoTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color Tint",Color) = (1,1,1,1)
		_Height("Height", float) = 1.0
		 _NormalMap ("Normal", 2D) = "bump" {}
		_NormInts("Normal Intensity",float) = 1
		 _Metallic("Metallic", float) = 1.0
		 _Smoothness("Specular", float) = 1.0
		 _Discard("Discard", float) = 1.0
		 _sD("Step Distance",float) = 0.1
		 _Steps("Steps",int) = 100
		 _HeightDeltaInfluence("Height Delta Influence",float) = 0.1
		 _HDI2("Shadows Height Delta Influence",float) = 0.1
		  _SsD("Shadows Step Distance",float) = 0.1
		  _Steps2("Steps (Soft Shadows)",int) = 70
		  _FirstOffset("_FirstOffset",float) = 1
		  _Shadows("Shadows",float) = 0
		  _ReRange("Fine Tune Shadows",float) = 1
		  _AO("Ambient Occlusion",float) = 1
		  _Shine("Shine Bright",float) = 0.1
		  _fLOAT("_fLOAT",float) = 99999.99999
		  _Empty("CullAlpha",Range(0,1)) = 0.5
		   _Scroll("Scroll",float) = 0.1
		   _QPB("Quality Performance Break",float) = 0.01
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0



		sampler2D _HeightTex;
		sampler2D _AlbedoTex;
		  sampler2D _NormalMap;
		float _Empty,_Height,_Metallic,_Smoothness,_Discard,_sD,_Steps,_NormInts,_HeightDeltaInfluence,_Steps2,_FirstOffset,_Shadows,_ReRange,_AO,_Shine,_fLOAT,_Scroll,_HDI2,_SsD,_QPB;
		float4 _Color;

		struct Input
		{

			float2 uv_HeightTex;
			float2 uv_NormalMap;

			float3 tangentViewDir;
			float3 tangentLightDir;
		};


		void vert(inout appdata_full i, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float3 worldVertexPos = mul(unity_ObjectToWorld, i.vertex).xyz;
			float3 worldViewDir = worldVertexPos - _WorldSpaceCameraPos;

			float3 worldNormal = UnityObjectToWorldNormal(i.normal);
			float3 worldTangent = UnityObjectToWorldDir(i.tangent.xyz);
			float3 worldBitangent = cross(worldNormal, worldTangent) * i.tangent.w * unity_WorldTransformParams.w;

			o.tangentViewDir = float3(
				dot(worldViewDir, worldTangent),
				dot(worldViewDir, worldNormal),
				dot(worldViewDir, worldBitangent)
				);

			worldViewDir = worldVertexPos - _WorldSpaceLightPos0.xyz*_fLOAT;

			o.tangentLightDir = float3(
				dot(worldViewDir, worldTangent),
				dot(worldViewDir, worldNormal),
				dot(worldViewDir, worldBitangent)
				);
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
		IN.uv_HeightTex+=_Time.x*_Scroll;
		IN.uv_NormalMap+=_Time.x*_Scroll;

			float3 rayPos = float3(IN.uv_HeightTex.x, 0, IN.uv_HeightTex.y);

			float3 rayDir = normalize(IN.tangentViewDir);
	
			float3 oldPos = rayPos - _sD * rayDir;


			float4 finalColor = 1;
			float3 finalCo = 0;


			bool b = true;//bellow or above surface
			float d = 1.0;//for decreasing step size as approaching goal
			float e = 0.0;//early break to improve performance
			float height = 1;
			float oldHeight = 0;

			for (int i = 0; i < _Steps; i++)
			{
			if(height < -_Height || height > 0){
			rayPos = float3(IN.uv_HeightTex.x,0, IN.uv_HeightTex.y);
			}
			oldPos = rayPos;
			if(b){
			if(rayPos.y < height){
			d/=2;
			rayPos-=rayDir*_sD*(d+e*_HeightDeltaInfluence);
			b = false;
			}else{
			rayPos+=rayDir*_sD*(d+e*_HeightDeltaInfluence);
			}
			}else{
			if(rayPos.y > height){
			d/=2;
			rayPos+=rayDir*_sD*(d+e*_HeightDeltaInfluence);
			b = true;
			}else{
			rayPos-=rayDir*_sD*(d+e*_HeightDeltaInfluence);
			}
			}
						oldHeight = height;
			 height = (1 - tex2Dlod(_HeightTex, float4(rayPos.xz , 0, 0)).r) * -1 * _Height ;
			 			 e = abs(rayPos.y-height);
						 if(e < _QPB){
						 break;
						 }
			}

			float aa = abs(oldHeight - oldPos.y);float bb = abs(height - rayPos.y);
			float3 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);


					finalColor =			 tex2Dlod(_AlbedoTex, float4(weightedTex.xz, 0, 0));

					 finalCo = UnpackNormal (tex2Dlod(_NormalMap, float4(weightedTex.xz, 0, 0)));


//Repeat for shadows
			d = 1;e= 0;b = true;
			float3 originalPos = weightedTex;
			rayPos = originalPos;
			rayPos-=rayDir*_SsD*_FirstOffset;
			rayDir = -normalize(IN.tangentLightDir);
			rayPos+=rayDir*_SsD*_FirstOffset;
			float3 resetPos = rayPos;
			oldPos = rayPos - _SsD * rayDir;
			bool hit = false;
			for (int i = 0; i < _Steps2; i++)
			{
			if(height < -_Height || height > 0){
			rayPos = resetPos;
			}
			oldPos = rayPos;
			if(b){
			if(rayPos.y < height){
			d/=2;
			rayPos-=rayDir*_SsD*(d+e*_HDI2);
			b = false;
			hit = true;
			}else{
			rayPos+=rayDir*_SsD*(d+e*_HDI2);
			}
			}else{
			if(rayPos.y > height){
			d/=2;
			rayPos+=rayDir*_SsD*(d+e*_HDI2);
			b = true;
			hit = true;
			}else{
			rayPos-=rayDir*_SsD*(d+e*_HDI2);
			}
			}
						oldHeight = height;
			 height = (1 - tex2Dlod(_HeightTex, float4(rayPos.xz , 0, 0)).r) * -1 * _Height;
			 			 e = abs(rayPos.y-height);
						  if(e < _QPB){
						 break;
						 }
			}

			aa = abs(oldHeight - oldPos.y); bb = abs(height - rayPos.y);
			weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

			float3 delta = originalPos-weightedTex;
			float distance = sqrt(delta.x*delta.x+delta.y*delta.y+delta.z*delta.z);

			o.Normal = (_NormInts > 1)?finalCo*_NormInts:(_NormInts < 0)?-finalCo*_NormInts:finalCo*_NormInts+fixed3(0,0,1)*(1.0-_NormInts);
			if(hit){
			o.Albedo = finalColor*_Color*(1.0-clamp(1.0-pow(distance,_ReRange),0,1)*_Shadows);
			}else{
				o.Albedo = finalColor*_Color ;
			}
			o.Emission = o.Albedo*_Shine;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}