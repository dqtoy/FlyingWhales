Shader "AlphaMask" {
    Properties {
        _MainTex ("_MainTex", 2D) = "white" {}
        _Alpha ("_Alpha", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
       
        ZWrite Off
       
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
       
        Pass {
            SetTexture[_MainTex] {
                Combine texture
            }
            SetTexture[_Alpha] {
                Combine previous * texture
            }
        }
    }
}