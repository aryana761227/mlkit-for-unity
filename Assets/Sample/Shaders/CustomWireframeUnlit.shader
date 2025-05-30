Shader "Custom/WireframeUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (0,1,0,1)
        _WireframeColor ("Wireframe Color", Color) = (0,1,0,1)
        _WireframeWidth ("Wireframe Width", Range(0, 10)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2g
            {
                float4 pos : SV_POSITION;
            };
            
            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 barycentricCoords : TEXCOORD0;
            };
            
            float4 _Color;
            float4 _WireframeColor;
            float _WireframeWidth;
            
            v2g vert(appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> output)
            {
                g2f o;
                
                o.pos = input[0].pos;
                o.barycentricCoords = float3(1, 0, 0);
                output.Append(o);
                
                o.pos = input[1].pos;
                o.barycentricCoords = float3(0, 1, 0);
                output.Append(o);
                
                o.pos = input[2].pos;
                o.barycentricCoords = float3(0, 0, 1);
                output.Append(o);
            }
            
            fixed4 frag(g2f i) : SV_Target
            {
                // Calculate distance to edge
                float3 deltas = fwidth(i.barycentricCoords);
                float3 smoothing = deltas * _WireframeWidth;
                float3 thickness = smoothing + 0.001;
                
                i.barycentricCoords = smoothstep(thickness, thickness + smoothing, i.barycentricCoords);
                float minBary = min(i.barycentricCoords.x, min(i.barycentricCoords.y, i.barycentricCoords.z));
                
                return lerp(_WireframeColor, _Color, minBary);
            }
            ENDCG
        }
    }
    
    // Fallback for devices that don't support geometry shaders
    Fallback "Unlit/Color"
}