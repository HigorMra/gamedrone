Shader "Drone/Vertex Color Lit"
{
    Properties { _Tint ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 250
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Tint;
        struct Input { float4 color : COLOR; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed3 c = IN.color.rgb * _Tint.rgb;
            o.Albedo = c;
            o.Smoothness = 0.08;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
