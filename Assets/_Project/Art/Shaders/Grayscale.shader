Shader "Hidden/MangaLegends/Grayscale"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} _Amount ("Amount", Range(0, 1)) = 0.85 }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Amount;
            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                fixed grey = dot(c.rgb, fixed3(0.299, 0.587, 0.114));
                c.rgb = lerp(c.rgb, grey.xxx * 0.8, _Amount);
                return c;
            }
            ENDCG
        }
    }
}
