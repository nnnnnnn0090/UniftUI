Shader "UI/RoundedCornersUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _CornerRadiusPercent ("Corner Radius Percent (0-1)", Range(0, 1)) = 0.1
        _RectWidth ("Rect Width", Float) = 100.0
        _RectHeight ("Rect Height", Float) = 100.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode] // Always for Overlay, LEqual for other canvases
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 clipRect : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _CornerRadiusPercent;
            float _RectWidth;
            float _RectHeight;


            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                o.clipRect = _ClipRect;
                return o;
            }

            float rounded_rectangle_sdf(float2 uv, float2 size, float radius)
            {
                uv = abs(uv) - size + radius; // Calculate distance from center, adjust for size and radius
                return length(max(uv, 0.0)) - radius; // Distance to the rounded box
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 小さい方の辺（幅または高さ）を取得
                float minDimension = min(_RectWidth, _RectHeight);

                // パーセント値から実際の半径を計算
                // 50%（_CornerRadiusPercent = 0.5）のとき、半径は短辺の半分（円になる）
                float radiusPixels = _CornerRadiusPercent * 0.5 * minDimension;

                // Map texcoords to be -0.5 to 0.5 relative to center for SDF
                float2 center_uv = i.texcoord - 0.5;
                
                // Convert pixel radius to UV space radius for x and y
                // Ensure _RectWidth and _RectHeight are not zero to avoid division by zero
                float2 R_uv;
                R_uv.x = (_RectWidth > 0) ? radiusPixels / _RectWidth : 0;
                R_uv.y = (_RectHeight > 0) ? radiusPixels / _RectHeight : 0;

                // The size of the rectangle in UV space is (0.5, 0.5) because center_uv is from -0.5 to 0.5
                float2 rect_half_size_uv = float2(0.5, 0.5);
                
                float sdf = rounded_rectangle_sdf(center_uv, rect_half_size_uv, min(R_uv.x, R_uv.y));

                // Antialiasing: use fwidth to determine smoothing amount based on pixel coverage
                float smooth_width = fwidth(sdf) * 0.5; // Adjust 0.5 for more or less smoothing
                float alpha = 1.0 - smoothstep(-smooth_width, smooth_width, sdf);

                fixed4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
                color.a *= alpha;
                
                // UI Clipping
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
