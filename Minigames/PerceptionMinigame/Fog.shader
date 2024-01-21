Shader "Abyss/Fog"
{
    Properties
    {
        _MainTex ("Texture2D display name", 2D) = "" {}
        _NoiseTex ("Noise", 2D) = "" {}
    }

    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4  _Torches[32];

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float2 noise_uv           : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float2 noise_uv           : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)

            float4 _MainTex_ST;
            float4 _NoiseTex_ST;

            CBUFFER_END
            

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.noise_uv = TRANSFORM_TEX(IN.noise_uv, _NoiseTex);
                return OUT;
            }

            float2 Unity_Remap_float2(float2 In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float4 Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                // need full precision, otherwise half overflows when p > 1
                float x = float(34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float Unity_GradientNoise_float(float2 UV, float Scale)
            {
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float light = 0;

                [unroll(32)] for (int i=0; i<32; i++)
                {
                    if (_Torches[i].z > 0.05)
                    {
                        float torchTime = _Time.y + .43 * i;

                        float scroll = fmod(torchTime * -0.1, 1);
                        float2 offset = float2(0, scroll);
                        float4 perlin = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.noise_uv + offset);

                        float flickerX = fmod(torchTime / 4, 1);
                        float gradientX = Unity_GradientNoise_float(float2(flickerX, flickerX), 5);

                        float flickerY = flickerX + 0.33;
                        float gradientY = Unity_GradientNoise_float(float2(flickerY, flickerY), 5);

                        float2 flickerGradient =
                                            Unity_Remap_float2(
                                                float2(gradientX, gradientY),
                                                float2(0, 1),
                                                float2(0.95, 1.05));

                        float2 pos = (_Torches[i].xy - IN.uv) * flickerGradient;
                        float d = length(pos);

                        float radius = _Torches[i].z;
                        float smoothCircle = smoothstep(radius + 0.05, radius - 0.05, d);

                        float4 smoothCircleA = mul(perlin, smoothCircle);
                        float cube = pow(smoothCircle, 3);
                        float4 smoothCircleB = float4(cube, cube, cube, cube);

                        float4 smoothCircleRes =
                                            Unity_Remap_float4(
                                                smoothCircleA + smoothCircleB,
                                                float2(0, 1),
                                                float2(0, 0.67));


                        light = max(light, smoothCircleRes.r);
                    }
                }

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                col.a = light;

                return col;
            }
            ENDHLSL
        }
    }
}
