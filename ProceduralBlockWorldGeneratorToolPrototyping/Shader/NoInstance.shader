// POMAW shader extended to fit texture atlas and unclean trigonal planar attempt
Shader "ProceduralBlockLandscape/OtherNoInstanceTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		  _AtlasRes("AtlasSteps",Range(1,16)) = 4
        _HeightMap ("HeightMap", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 1.0
        _Fix ("BorderFix", Range(-0.1,0.1)) = 0
        _FixBias ("BorderFixBias", Range(-0.1,0.1)) = 0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = .8
        _Mul ("Mul", Range(0,3)) = 1.0
        _ComputeMul ("ComputeMul", Range(0,1)) = 1.0
        _AO ("AO", Range(0,1)) = .99
        _ReNormal ("ReNormal", Range(0,1)) = 0
        
		_Height("Height", float) = .1
		_NormInts("Normal Intensity",float) = 1
		 _Smoothness("Specular", float) = .5
		 _Discard("Discard", float) = 0
		 _sD("Step Distance",float) = 0.1
		 _Steps("Steps",int) = 30
		 _HeightDeltaInfluence("Height Delta Influence",float) = 1
		 _HDI2("Shadows Height Delta Influence",float) = 1
		  _SsD("Shadows Step Distance",float) = 0.03
		  _Steps2("Steps (Soft Shadows)",int) = 30
		  _FirstOffset("_FirstOffset",float) = 0.1
		  _Shadows("Shadows",float) = .5
		  _ReRange("Fine Tune Shadows",float) = 1
		  _Shine("Shine Bright",float) = 0
		  _fLOAT("_fLOAT",float) = 99999.99999
		  _Empty("CullAlpha",Range(0,1)) = 0.5
		   _Scroll("Scroll",float) = 0
    }


    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                half3 normal : TEXCOORD2;
                //float3 coords : TEXCOORD7;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                fixed3 ambient : COLOR1;
                fixed3 diff : COLOR2;
                float4 position : SV_POSITION;
                
                float3 wPos : TEXCOORD3;
                half3 tspace0 : TEXCOORD4;
                half3 tspace1 : TEXCOORD5;
                half3 tspace2 : TEXCOORD6;
                
                float3 tangentViewDir : TEXCOORD8;
                float3 tangentLightDir : TEXCOORD9;
            };
 
            float Mod(float x, float y)
            {
                return x - y * floor(x/y);
            }
            
            int _AtlasRes;
		float _Empty,_Height,_Smoothness,_Discard,_sD,_Steps,_NormInts,_HeightDeltaInfluence,_Steps2,_FirstOffset,_Shadows,_ReRange,_Shine,_fLOAT,_Scroll,_HDI2,_SsD;
            float _Tiling,_FixBias,_Fix;
            float _ComputeMul;
            float _ReNormal;

            v2f vert (uint id : SV_VertexID,uint instance : SV_INSTANCEID,float4 position : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT, float2 uv : TEXCOORD0, float4 color : COLOR)
            {
                v2f o;
                o.color = color;
                //*
               o.normal = normal;
                
                o.position = UnityObjectToClipPos(position);
                
                o.wPos = mul(unity_ObjectToWorld, position).xyz;
                half3 wNormal = UnityObjectToWorldNormal(normal);
                half3 wTangent = UnityObjectToWorldDir(tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);



			float3 worldViewDir = o.wPos - _WorldSpaceCameraPos;

			o.tangentViewDir = float3(
				dot(worldViewDir, wTangent),
				dot(worldViewDir, wNormal),
				dot(worldViewDir, wBitangent)
				);

			worldViewDir = o.wPos - _WorldSpaceLightPos0.xyz*_fLOAT;

            o.tangentLightDir = float3(
				dot(worldViewDir, wTangent),
				dot(worldViewDir, wNormal),
				dot(worldViewDir, wBitangent)
				);


                //o.coords = o.wPos.xyz * _Tiling;

                o.uv = uv;
                half nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(wNormal,1));
                return o;
            }

            sampler2D _MainTex;
            sampler2D _HeightMap;
            sampler2D _NormalMap;
            sampler2D _OcclusionMap;
            float _Metallic;
            float _Mul;
            float _AO;

            fixed4 frag (v2f i,uint instance : SV_INSTANCEID) : SV_Target
            {
                // use absolute value of normal as texture weights
                half3 blend = abs(i.normal);
                // make sure the weights sum up to 1 (divide by sum of x+y+z)
                blend /= dot(blend,1.0);

                float3 coords = i.wPos*_Tiling;

                float2 coordsxy,coordsyz,coordsxz;


                _Steps =_Steps/clamp(distance(_WorldSpaceCameraPos, i.wPos)*0.1,1,1000);

                float uvd = 1.0/_AtlasRes;
            uvd*=(1-_Fix);

    //COORDSXY
             float3 rayPos = float3(coords.x, 0, coords.y);

			float3 rayDir = normalize(i.tangentViewDir);//from screen center x,z coordinates

			float3 oldPos = rayPos - _sD * rayDir;


			bool b = true;
			float d = 1.0;
			float e = 0.0;
			float height = 1;
			float oldHeight = 0;

            int li= 0;
			for (li = 0; li < _Steps; li++)
			{
			if(height < -_Height || height > 0){
			rayPos = float3(coords.x,0, coords.y);
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
			 height = (1 - tex2Dlod(_HeightMap, float4(i.uv+(rayPos.xz+float2(uvd,uvd)*1000)%uvd , 0, 0)).r) * -1 * _Height ;

			 			 e = abs(rayPos.y-height);
			}

			float aa = abs(oldHeight - oldPos.y);float bb = abs(height - rayPos.y);
			float3 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

            coordsxy=rayPos.xz;

            //COORDSYZ
              rayPos = float3(coords.y, 0, coords.z);

			 rayDir = normalize(i.tangentViewDir);

			 oldPos = rayPos - _sD * rayDir;


			 b = true;
			 d = 1.0;
			 e = 0.0;
			 height = 1;
			 oldHeight = 0;
             
            li= 0;
			for (li = 0; li < _Steps; li++)
			{
			if(height < -_Height || height > 0){
			rayPos = float3(coords.y,0, coords.z);
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
			 height = (1 - tex2Dlod(_HeightMap, float4(i.uv+(rayPos.xz+float2(uvd,uvd)*1000)%uvd , 0, 0)).r) * -1 * _Height ;

			 			 e = abs(rayPos.y-height);
			}

			 aa = abs(oldHeight - oldPos.y); bb = abs(height - rayPos.y);
			 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

            coordsyz=rayPos.xz;

            //COORDSXZ
              rayPos = float3(coords.x, 0, coords.z); ////// CHANGE

			 rayDir = normalize(i.tangentViewDir);

			 oldPos = rayPos - _sD * rayDir;


			 b = true;
			 d = 1.0;
			 e = 0.0;
			 height = 1;
			 oldHeight = 0;
             
            li= 0;
			for (li = 0; li < _Steps; li++) 
			{
			if(height < -_Height || height > 0){
			rayPos = float3(coords.x,0, coords.z);////// CHANGE
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
			 height = (1 - tex2Dlod(_HeightMap, float4(i.uv+(rayPos.xz+float2(uvd,uvd)*1000)%uvd , 0, 0)).r) * -1 * _Height ;

			 			 e = abs(rayPos.y-height);
			}

			 aa = abs(oldHeight - oldPos.y); bb = abs(height - rayPos.y);
			 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

            coordsxz=rayPos.xz;

            coordsxy= _FixBias+abs(i.uv+abs(coordsxy+_Fix+uvd*10)%uvd)*(1-_FixBias*2);
            coordsyz= _FixBias+abs(i.uv+abs(coordsyz+_Fix+uvd*10)%uvd)*(1-_FixBias*2);
            coordsxz= _FixBias+abs(i.uv+abs(coordsxz+_Fix+uvd*10)%uvd)*(1-_FixBias*2);


                float hx = tex2D(_HeightMap, coordsyz).r*blend.x;
                float hy = tex2D(_HeightMap, coordsxz).r*blend.y;
                float hz = tex2D(_HeightMap, coordsxy).r*blend.z;

                // read the three texture projections, for x,y,z axes
                fixed4 cx = tex2D(_MainTex, coordsyz);
                fixed4 cy = tex2D(_MainTex, coordsxz);
                fixed4 cz = tex2D(_MainTex, coordsxy);
                // blend the textures based on weights
                fixed4 color = cx * blend.x + cy * blend.y + cz * blend.z;
                color = (hx>hy)?
                ((hx>hz)?cx:cz):
                 ((hy>hz)?cy:cz);

                
                // read the three texture projections, for x,y,z axes
                half3 nx = UnpackNormal(tex2D(_NormalMap, coordsyz));
                half3 ny =UnpackNormal(tex2D(_NormalMap, coordsxz));
                half3 nz = UnpackNormal(tex2D(_NormalMap, coordsxy));
                // blend the textures based on weights
                half3 trinormal = nx * blend.x + ny * blend.y + nz * blend.z;
                trinormal = (hx>hy)?
                ((hx>hz)?nx:nz):
                 ((hy>hz)?ny:nz);
                i.normal = trinormal;

                half3 wNormal;
                wNormal.x = dot(i.tspace0, trinormal);
                wNormal.y = dot(i.tspace1, trinormal);
                wNormal.z = dot(i.tspace2, trinormal);
                // rest the same as in previous shader
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.wPos));
                half3 worldRefl = reflect(-worldViewDir, wNormal);
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);

                fixed4 ox = tex2D(_OcclusionMap, coordsyz);
                fixed4 oy = tex2D(_OcclusionMap, coordsxz);
                fixed4 oz = tex2D(_OcclusionMap, coordsxy);
                // blend the textures based on weights
                fixed4 o = ox * blend.x + oy * blend.y + oz * blend.z;
               o= (hx>hy)?
                ((hx>hz)?ox:oz):
                 ((hy>hz)?oy:oz);

                half nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
                fixed3  diff = nl * _LightColor0.rgb;
                fixed3 ambient = ShadeSH9(half4(wNormal,1));

                ///fixed shadow = clamp(dot(wNormal,_WorldSpaceLightPos0),0,1);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff + i.ambient;

                fixed4 c = 0;
                c.rgb = skyColor*_Metallic+lighting*(1.0-_Metallic);
                //c.rgb = skyColor*lighting;
                c*=1+_Mul;
                c.rgb *= color;
                c.rgb *= o*_AO+(1.0-_AO);
                c.rgb *= i.color;
                //return fixed4(i.tangentViewDir.xyz,1);
                return c;
            }
            ENDCG
        }
    }
}