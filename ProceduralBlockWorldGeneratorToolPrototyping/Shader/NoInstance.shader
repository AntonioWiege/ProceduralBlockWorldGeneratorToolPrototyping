// POMAW shader extended to fit texture atlas and unclean trigonal planar attempt
Shader "ProceduralBlockLandscape/OtherNoInstanceTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AtlasRes("AtlasSteps",Range(1,16)) = 4
        _HeightMap ("HeightMap", 2D) = "white" {}
        _Tiling ("Tiling Worldspace", Float) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = .8
        _Mul ("Multiply Color", Range(0,3)) = 1.0
        _AO ("AO", Range(0,1)) = .99
        
		_Height("Depth Multiply", float) = .1
		_HeightOffset("Depth Offset", float) = .1
		 _sD("Step Distance",float) = 0.1
		 _Steps("Steps",int) = 30
		 _HeightDeltaInfluence("Height Delta Influence",float) = 1
		  _Shadows("Shadow Strength",Range(0,1)) = .5
		  _fLOAT("_fLOAT",float) = 99999.99999
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
		float _Height,_HeightOffset,_sD,_Steps,_HeightDeltaInfluence,_Shadows,_fLOAT,_Tiling, _Metallic,_Mul, _AO;




            v2f vert (uint id : SV_VertexID,uint instance : SV_INSTANCEID,float4 position : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT, float2 uv : TEXCOORD0, float4 color : COLOR)
            {
                v2f o;
                o.color = color;

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

                //camera view calculation
			float3 worldViewDir = o.wPos - _WorldSpaceCameraPos;

			o.tangentViewDir = float3(
				dot(worldViewDir, wTangent),
				dot(worldViewDir, wNormal),
				dot(worldViewDir, wBitangent)
				);
                 //sun view calculation
			worldViewDir = o.wPos - _WorldSpaceLightPos0.xyz*_fLOAT;

            o.tangentLightDir = float3(
				dot(worldViewDir, wTangent),
				dot(worldViewDir, wNormal),
				dot(worldViewDir, wBitangent)
				);

                o.uv = uv;
                //regular lighting
                half nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(wNormal,1));
                return o;
            }



            sampler2D _MainTex;
            sampler2D _HeightMap;
            sampler2D _NormalMap;
            sampler2D _OcclusionMap;





            fixed4 frag (v2f i,uint instance : SV_INSTANCEID) : SV_Target
            {
                // use absolute value of normal as texture weights
                half3 blend = abs(i.normal);
                // make sure the weights sum up to 1 (divide by sum of x+y+z)
                blend /= dot(blend,1.0);
                //scale textureCoordinates in worldSpace
                float3 coords = i.wPos*_Tiling;

                float2 coordsxy,coordsyz,coordsxz;

                //dynamically add or remove resolution by distance to camera
                _Steps =_Steps/clamp(distance(_WorldSpaceCameraPos, i.wPos)*0.1,1,1000);

                float uvd = 1.0/_AtlasRes;

                //more details on raymarching in POMAW.shader

            //COORDS-XY
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
			 height = (1 - tex2Dlod(_HeightMap, float4(i.uv+(rayPos.xz+float2(uvd,uvd)*1000)%uvd , 0, 0)).r-_HeightOffset) * -1 * _Height ;

			 			 e = abs(rayPos.y-height);
			}

			float aa = abs(oldHeight - oldPos.y);float bb = abs(height - rayPos.y);
			float3 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

            coordsxy=rayPos.xz;

            //COORDS-YZ
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
			 height = (1 - tex2Dlod(_HeightMap, float4(i.uv+(rayPos.xz+float2(uvd,uvd)*1000)%uvd , 0, 0)).r-_HeightOffset) * -1 * _Height ;

			 			 e = abs(rayPos.y-height);
			}

			 aa = abs(oldHeight - oldPos.y); bb = abs(height - rayPos.y);
			 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

            coordsyz=rayPos.xz;

            //COORDS-XZ
             rayPos = float3(coords.x, 0, coords.z); 

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
			rayPos = float3(coords.x,0, coords.z);
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
			 height = (1 - tex2Dlod(_HeightMap, float4(i.uv+(rayPos.xz+float2(uvd,uvd)*1000)%uvd , 0, 0)).r-_HeightOffset) * -1 * _Height ;

			 			 e = abs(rayPos.y-height);
			}

			 aa = abs(oldHeight - oldPos.y); bb = abs(height - rayPos.y);
			 weightedTex = (oldPos*aa+rayPos*bb)/(aa+bb);

            coordsxz=rayPos.xz;



            coordsxy= abs(i.uv+abs(coordsxy+uvd*10)%uvd);
            coordsyz= abs(i.uv+abs(coordsyz+uvd*10)%uvd);
            coordsxz= abs(i.uv+abs(coordsxz+uvd*10)%uvd);

                //blend heights together
                float hx = tex2D(_HeightMap, coordsyz).r*blend.x;
                float hy = tex2D(_HeightMap, coordsxz).r*blend.y;
                float hz = tex2D(_HeightMap, coordsxy).r*blend.z;

                // read the three texture projections, for x,y,z axes
                fixed4 cx = tex2D(_MainTex, coordsyz);
                fixed4 cy = tex2D(_MainTex, coordsxz);
                fixed4 cz = tex2D(_MainTex, coordsxy);

                // blend the textures based on weights
                fixed4 color = cx * blend.x + cy * blend.y + cz * blend.z;
                /*height based overwriting
                color = (hx>hy)?
                ((hx>hz)?cx:cz):
                 ((hy>hz)?cy:cz);
                */

                
                // read the three texture projections, for x,y,z axes
                half3 nx = UnpackNormal(tex2D(_NormalMap, coordsyz));
                half3 ny =UnpackNormal(tex2D(_NormalMap, coordsxz));
                half3 nz = UnpackNormal(tex2D(_NormalMap, coordsxy));
                // blend the textures based on weights
                half3 trinormal = nx * blend.x + ny * blend.y + nz * blend.z;
                /*height based overwriting
                trinormal = (hx>hy)?
                ((hx>hz)?nx:nz):
                 ((hy>hz)?ny:nz);
                */
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
               /*height based overwriting
               o= (hx>hy)?
                ((hx>hz)?ox:oz):
                 ((hy>hz)?oy:oz);
               */

                half nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
                fixed3  diff = nl * _LightColor0.rgb;
                fixed3 ambient = ShadeSH9(half4(wNormal,1));

                fixed shadow = clamp(dot(wNormal,_WorldSpaceLightPos0),0,1);
                // darken light's illumination with shadow, keep ambient intact

                fixed4 c = 0;
                c.rgb = i.diff*(1-_Shadows)+ i.diff*shadow*_Shadows
                            +skyColor*_Metallic+i.ambient*(1.0-_Metallic);

                c*=1+_Mul;
                c.rgb *= color;
                c.rgb *= o*_AO+(1.0-_AO);
                c.rgb *= i.color;

                return c;
            }
            ENDCG
        }
    }
}