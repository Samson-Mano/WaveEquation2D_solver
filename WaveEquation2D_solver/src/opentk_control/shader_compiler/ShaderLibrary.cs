using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEquation2D_solver.src.opentk_control.shader_compiler
{
    public class ShaderLibrary
    {


        public enum ShaderType
        {
            MeshShader,
            TextShader,
            NodeConstraintShader,
            LoadShader,
            RsltMeshShader,
            RsltWireframeShader,
            SelectionShader,
            DrawingAxisShader,
            ContourBarShader
        }


        #region "Mesh Shaders"

        private static string mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform vec4 vertexColor;
                    
            layout(location = 0) in vec2 aPosition;
                    

            out vec4 vColor;
                    
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                vColor = vertexColor;
            }


                    ";

        }




        private static string mesh_frag_shader()
        {

            return @"

            #version 330 core

            in vec4 vColor;
            out vec4 fColor;
    
            void main()
            {
                // Simple color output without lighting
                fColor = vColor;
            }


                    ";

        }

        #endregion




        #region "Result Mesh Shaders"

        private static string rslt_mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix

            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in float aScalarValue;
                    
            out float v_deflscale;
                    
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                
                v_deflscale = aScalarValue;

            }


                    ";

        }

        private static string rslt_mesh_frag_shader()
        {

            return @"

            #version 330 core

            uniform int CmapOption; // User selected color map option
            uniform float vertexTransparency;

            in float v_deflscale;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            const float TWO_PI = 6.28318530717958;
            const float PI = 3.141592653589793;

            vec3 rainbowHeatmap(float x)
            {   
                // Option 1: Rainbow heatmap using sine functions for smooth transitions
                float r = 0.5 + 0.5 * sin(TWO_PI * x);
                float g = 0.5 + 0.5 * sin(TWO_PI * x + 2.09439); // 120 degrees phase shift
                float b = 0.5 + 0.5 * sin(TWO_PI * x + 4.18879); // 240 degrees phase shift
                return vec3(r, g, b);
            }

            vec3 jetHeatmap(float x)
            {
                // Option 2: Jet heatmap using piecewise linear functions
                float r = clamp(1.5 - abs(4.0 * x - 3.0), 0.0, 1.0);
                float g = clamp(1.5 - abs(4.0 * x - 2.0), 0.0, 1.0);
                float b = clamp(1.5 - abs(4.0 * x - 1.0), 0.0, 1.0);
                return vec3(r, g, b);
            }

            vec3 turboHeatmap(float x)
            {
                // Option 3: Turbo heatmap using a polynomial approximation
                 x = clamp(x, 0.0, 1.0);

                const vec4 kRedVec4   = vec4(0.13572138, 4.61539260, -42.66032258, 132.13108234);
                const vec4 kGreenVec4 = vec4(0.09140261, 2.19418839,   4.84296658, -14.18503333);
                const vec4 kBlueVec4  = vec4(0.10667330,12.64194608, -60.58204836, 110.36276771);

                const vec2 kRedVec2   = vec2(-152.94239396, 59.28637943);
                const vec2 kGreenVec2 = vec2(  4.27729857,   2.82956604);
                const vec2 kBlueVec2  = vec2(-89.90310912, 27.34824973);

                float r = dot(vec4(1.0, x, x*x, x*x*x), kRedVec4) + dot(vec2(x*x*x*x, x*x*x*x*x), kRedVec2);
                float g = dot(vec4(1.0, x, x*x, x*x*x), kGreenVec4) + dot(vec2(x*x*x*x, x*x*x*x*x), kGreenVec2);
                float b = dot(vec4(1.0, x, x*x, x*x*x), kBlueVec4) + dot(vec2(x*x*x*x, x*x*x*x*x), kBlueVec2);

                return clamp(vec3(r, g, b), 0.0, 1.0);
            }


            vec3 viridisHeatmap(float x)
            {
                // Option 4: Viridis heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                const vec4 c0 = vec4(0.277727, 0.005407, 0.334099, 0.0);
                const vec4 c1 = vec4(0.105093, 1.404613, 1.384590, 0.0);
                const vec4 c2 = vec4(-0.330861, 0.214847, 0.095095, 0.0);
                const vec4 c3 = vec4(-4.634230, -5.799100, -19.332440, 0.0);
                const vec4 c4 = vec4(6.228269, 14.179933, 56.690552, 0.0);
                const vec4 c5 = vec4(4.776384, -13.745145, -65.353032, 0.0);
                const vec4 c6 = vec4(-5.435455, 4.645852, 26.312435, 0.0);

                vec3 result =
                      c0.rgb
                    + x * (c1.rgb
                    + x * (c2.rgb
                    + x * (c3.rgb
                    + x * (c4.rgb
                    + x * (c5.rgb
                    + x *  c6.rgb)))));

                return clamp(result, 0.0, 1.0);
            }

            vec3 plasmaHeatmap(float x)
            {
                // Option 5: Plasma heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                const vec4 c0 = vec4(0.050383, 0.029803, 0.527975, 0.0);
                const vec4 c1 = vec4(2.404045, 0.801052, -0.538516, 0.0);
                const vec4 c2 = vec4(-2.364640, 2.067370, 1.772104, 0.0);
                const vec4 c3 = vec4(1.448550, -2.165510, 0.985241, 0.0);
                const vec4 c4 = vec4(-0.619128, 1.318690, -0.427321, 0.0);

                vec3 result =
                      c0.rgb
                    + x * (c1.rgb
                    + x * (c2.rgb
                    + x * (c3.rgb
                    + x *  c4.rgb)));

                return clamp(result, 0.0, 1.0);
            }

            vec3 infernoHeatmap(float x)
            {
                // Option 6: Inferno heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                const vec4 c0 = vec4(0.000218, 0.001651, 0.019331, 0.0);
                const vec4 c1 = vec4(0.106513, 0.563956, 1.204423, 0.0);
                const vec4 c2 = vec4(11.602493, -3.972853, -3.071330, 0.0);
                const vec4 c3 = vec4(-41.703996, 17.436398, 7.177935, 0.0);
                const vec4 c4 = vec4(59.966167, -33.872990, -10.180971, 0.0);
                const vec4 c5 = vec4(-35.595260, 20.926208, 6.070863, 0.0);

                vec3 result =
                      c0.rgb
                    + x * (c1.rgb
                    + x * (c2.rgb
                    + x * (c3.rgb
                    + x * (c4.rgb
                    + x *  c5.rgb))));

                return clamp(result, 0.0, 1.0);
            }

            vec3 magmaHeatmap(float x)
            {
                // Option 7: Magma heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                const vec4 c0 = vec4(0.001462, 0.000466, 0.013866, 0.0);
                const vec4 c1 = vec4(0.152070, 0.386306, 0.529958, 0.0);
                const vec4 c2 = vec4(2.792526, -1.113869, -0.897369, 0.0);
                const vec4 c3 = vec4(-11.365472, 5.007153, 2.183972, 0.0);
                const vec4 c4 = vec4(17.436398, -10.147660, -3.309513, 0.0);
                const vec4 c5 = vec4(-10.300000, 6.000000, 1.900000, 0.0);

                vec3 result =
                      c0.rgb
                    + x * (c1.rgb
                    + x * (c2.rgb
                    + x * (c3.rgb
                    + x * (c4.rgb
                    + x *  c5.rgb))));

                return clamp(result, 0.0, 1.0);
            }

            vec3 cividisHeatmap(float x)
            {
                // Option 8: Cividis heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                const vec4 c0 = vec4(0.0,       0.135112, 0.304751, 0.0);
                const vec4 c1 = vec4(0.0,       1.168321, 1.595000, 0.0);
                const vec4 c2 = vec4(2.340000, -2.000000, -1.200000, 0.0);
                const vec4 c3 = vec4(-4.000000, 2.500000, 0.800000, 0.0);
                const vec4 c4 = vec4(2.500000, -1.200000, -0.200000, 0.0);

                vec3 result =
                      c0.rgb
                    + x * (c1.rgb
                    + x * (c2.rgb
                    + x * (c3.rgb
                    + x *  c4.rgb)));

                return clamp(result, 0.0, 1.0);
            }

            vec3 twilightHeatmap(float x)
            {
                // Option 9: Twilight heatmap using a polynomial approximation
                // Wrap instead of clamp (important for cyclic maps)
                x = fract(x);

                const vec4 c0 = vec4(0.885750, 0.850009, 0.887973, 0.0);
                const vec4 c1 = vec4(-2.000000, -1.500000, -2.200000, 0.0);
                const vec4 c2 = vec4(2.500000, 2.000000, 2.800000, 0.0);
                const vec4 c3 = vec4(-1.500000, -1.200000, -1.600000, 0.0);

                vec3 result =
                      c0.rgb
                    + x * (c1.rgb
                    + x * (c2.rgb
                    + x *  c3.rgb));

                return clamp(result, 0.0, 1.0);
            }

            vec3 twilightShiftedHeatmap(float x)
            {
                // Option 10: Twilight Shifted heatmap using a polynomial approximation
                // Shift by half cycle (0.5)
                return twilightHeatmap(fract(x + 0.5));
            }

            vec3 sinebowHeatmap(float x)
            {
                // Option 11: Sinebow heatmap using sine functions for smooth transitions
                // Wrap for cyclic behavior
                x = fract(x);

                // Phase offsets for RGB
                float r = sin(PI * x);
                float g = sin(PI * (x + 1.0/3.0));
                float b = sin(PI * (x + 2.0/3.0));

                // Square to keep values positive and smooth
                return vec3(r*r, g*g, b*b);
            }

            vec3 hotHeatmap(float x)
            {
                // Option 12: Hot heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                float r = min(1.0, 3.0 * x);
                float g = min(1.0, max(0.0, 3.0 * x - 1.0));
                float b = min(1.0, max(0.0, 3.0 * x - 2.0));

                return vec3(r, g, b);
            }

            vec3 coolHeatmap(float x)
            {
                // Option 13: Cool heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                // Cool = linear interpolation between cyan and magenta
                vec3 c1 = vec3(0.0, 1.0, 1.0); // cyan
                vec3 c2 = vec3(1.0, 0.0, 1.0); // magenta

                return mix(c1, c2, x);
            }

            vec3 springHeatmap(float x)
            {
                // Option 14: Spring heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                float r = 1.0;
                float g = x;
                float b = 1.0;

                return vec3(r, g, b);
            }

            vec3 summerHeatmap(float x)
            {
                // Option 15: Summer heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                float r = x;
                float g = 0.5 + 0.5 * x;
                float b = 0.4 + 0.6 * x;

                return vec3(r, g, b);
            }

            vec3 autumnHeatmap(float x)
            {
                // Option 16: Autumn heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                float r = 1.0;
                float g = x;
                float b = 0.0;

                return vec3(r, g, b);
            }

            vec3 winterHeatmap(float x)
            {
                // Option 17: Winter heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                float r = 0.0;
                float g = x;
                float b = 1.0;

                return vec3(r, g, b);
            }

            vec3 grayHeatmap(float x)
            {
                // Option 18: Gray heatmap using a polynomial approximation
                float intensity = clamp(1.0 - abs(4.0 * x - 2.0), 0.0, 1.0);
                return vec3(intensity);
            }

            vec3 boneHeatmap(float x)
            {
                // Option 19: Bone heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                // Slight gamma-like shaping for better contrast
                x = 0.5 * x + 0.5 * x * x;

                float r = 0.6 * x + 0.1;
                float g = 0.6 * x + 0.3;
                float b = 0.6 * x + 0.6;

                return clamp(vec3(r, g, b), 0.0, 1.0);
            }

            vec3 copperHeatmap(float x)
            {
                // Option 20: Copper heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                // Slight nonlinear shaping for “metallic” feel
                x = sqrt(x);

                float r = clamp(1.25 * x, 0.0, 1.0);
                float g = clamp(0.78 * x, 0.0, 1.0);
                float b = clamp(0.50 * x, 0.0, 1.0);

                return vec3(r, g, b);
            }

            vec3 pinkHeatmap(float x)
            {
                // Option 21: Pink heatmap using a polynomial approximation
                x = clamp(x, 0.0, 1.0);

                // Gentle contrast shaping (avoids flat look)
                x = sqrt(x);

                float r = 0.96 * x + 0.5 * (1.0 - x);
                float g = 0.6  * x + 0.3 * (1.0 - x);
                float b = 0.8  * x + 0.6 * (1.0 - x);

                return clamp(vec3(r, g, b), 0.0, 1.0);
            }

            vec3 getColor(int CmapOption, float x)
            {

                vec3 color;
                if (CmapOption < 11)
                {
                    if(CmapOption <6)
                    {
                        // For options 0-5
                        switch (CmapOption)
                        {
                            case 0: color = rainbowHeatmap(x); break;
                            case 1: color = jetHeatmap(x); break;
                            case 2: color = turboHeatmap(x); break;
                            case 3: color = viridisHeatmap(x); break;
                            case 4: color = plasmaHeatmap(x); break;
                            case 5: color = infernoHeatmap(x); break;
                        }
                        //
                    }
                    else
                    {
                        // For options 6-10
                        switch (CmapOption)
                        {
                            case 6: color = magmaHeatmap(x); break;
                            case 7: color = cividisHeatmap(x); break;
                            case 8: color = twilightHeatmap(x); break;
                            case 9: color = twilightShiftedHeatmap(x); break;
                            case 10: color = sinebowHeatmap(x); break;
                        }
                        //
                    }
                    //
                }
                else
                {
                    if(CmapOption < 16)
                    {
                        // For options 11-15
                        switch (CmapOption)
                        {
                            case 11: color = hotHeatmap(x); break;
                            case 12: color = coolHeatmap(x); break;
                            case 13: color = springHeatmap(x); break;
                            case 14: color = summerHeatmap(x); break;
                            case 15: color = autumnHeatmap(x); break;
                        }
                        //
                    }
                    else
                    {
                        // For options 16-20
                        switch (CmapOption)
                        {
                            case 16: color = winterHeatmap(x); break;
                            case 17: color = grayHeatmap(x); break;
                            case 18: color = boneHeatmap(x); break;
                            case 19: color = copperHeatmap(x); break;
                            case 20: color = pinkHeatmap(x); break;
                        }
                        //
                    }
                }

                // Default to rainbow heatmap
                return color;
            }


            void main() 
            {
                // Need to implement 1D color mapping texture lookup for better performance, 
                // but for now we can use the function directly in the shader

                vec3 vertexColor = getColor(CmapOption, v_deflscale);
    
                f_Color = vec4(vertexColor, vertexTransparency); // Set the final color
            }


                    ";

        }

        #endregion


        #region "Result WireFrame Mesh Shaders"

        private static string rslt_wireframe_vert_shader()
        {
            return @"
            #version 330 core

            uniform mat4 uMVP;
            uniform float geomscale = 1.0f;
            uniform float sinevalue = 1.0f;                    
            uniform float modelpercent = 0.01;
    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aDisplacement;
            layout(location = 2) in float aDisplacementMagnitude;
            layout(location = 3) in float aScalarValue;
    
            out float v_deflscale;
        
            void main()
            {
                float scalevalue = geomscale * modelpercent * aDisplacementMagnitude;
                vec2 scaledDisplacement = aDisplacement * scalevalue * sinevalue;
                gl_Position = uMVP * vec4(aPosition + scaledDisplacement, 0.0, 1.0);

                v_deflscale = aScalarValue * sinevalue;
            }
            ";
        }


        private static string rslt_wireframe_frag_shader()
        {
            return @"
            #version 330 core
    
            uniform float wireframeAlpha;
    
            in float v_deflscale;
    
            out vec4 fColor;
    
            vec3 jetHeatmap(float value) 
            {
                float t = value; // (value + 1.0) * 0.5;
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }
    
            void main()
            {
                vec3 contourColor = jetHeatmap(v_deflscale);
                vec3 finalColor;
        
         
                // Use complementary color with some brightness enhancement
                finalColor = vec3(1.0) - contourColor;

                // Make it brighter for visibility
                finalColor = mix(finalColor, vec3(1.0), 0.1);
              
        
                fColor = vec4(finalColor, wireframeAlpha);
            }
            ";
        }

        #endregion



        #region "Text shaders"

        public static string text_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform float zoomscale = 1.0f;

            uniform float vertexTransparency = 1.0f; // Transparency of the mesh

            layout(location = 0) in vec2 position;
            layout(location = 1) in vec2 origin;
            layout(location = 2) in vec2 textureCoord;
            layout(location = 3) in vec3 textColor;

            out vec4 v_textureColor;
            out vec2 v_textureCoord;

            void main()
            {

	            // apply Translation to the final position 
	            vec4 finalPosition =  uMVP * vec4(position,0.0f,1.0f);

	            // apply Translation to the text origin
	            vec4 finalTextorigin =  uMVP * vec4(origin,0.0f,1.0f);
    
	            // Remove the zoom scale
	            vec2 scaled_pt = vec2(finalPosition.x - finalTextorigin.x,finalPosition.y - finalTextorigin.y) / zoomscale;
		
	            // Set the final position of the vertex
	            gl_Position = vec4(scaled_pt.x + finalTextorigin.x, scaled_pt.y + finalTextorigin.y, 0.0f, 1.0f);

	            // Calculate texture coordinates for the glyph
	            v_textureCoord = textureCoord;
	
	            // Pass the texture color to the fragment shader
	            v_textureColor = vec4(textColor, vertexTransparency);
            }

                    ";

        }


        public static string text_frag_shader()
        {
            return @"

            #version 330 core
            uniform sampler2D u_Texture;

            in vec4 v_textureColor;
            in vec2 v_textureCoord;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
	            vec4 texColor = vec4(1.0, 1.0, 1.0, texture(u_Texture, v_textureCoord).r);
	            f_Color = v_textureColor * texColor;
            }

                    ";

        }

        #endregion


        #region "Node Constraint Shader"

        public static string nodeconstraint_vert_shader()
        {
            return @"

            #version 330 core

          
            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
                    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in float constraintType;        

            out vec4 vColor;
                    
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);

                if (constraintType == 0.0)
                    vColor = vec4(0.5412, 0.1686, 0.8863, 1.0); // BlueViolet for field value
                else
                    vColor = vec4(1.0, 0.0, 1.0, 1.0); // Magenta for source value

            }


                    ";

        }


        public static string nodeconstraint_frag_shader()
        {
            return @"

            #version 330 core

            in vec4 vColor;
            out vec4 fColor;
    
            void main()
            {
                // Simple color output without lighting
                fColor = vColor;
            }


                    ";

        }



        #endregion


        #region "Load Shader"

        public static string load_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;
            uniform vec4 vertexColor;
            uniform float zoomscale = 1.0f;
    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aOrigin;
    
            out vec4 vColor;
    
            void main()
            {
                // Transform to clip space
                vec4 clipPos = uMVP * vec4(aPosition, 0.0, 1.0);
                vec4 clipOrigin = uMVP * vec4(aOrigin, 0.0, 1.0);
        
                // Calculate NDC coordinates
                vec3 ndcPos = clipPos.xyz / clipPos.w;
                vec3 ndcOrigin = clipOrigin.xyz / clipOrigin.w;
        
                // Scale offset in NDC space
                vec2 scaledOffset = (ndcPos.xy - ndcOrigin.xy) / zoomscale;
        
                // Final position (back to clip space)
                gl_Position = vec4(ndcOrigin.xy + scaledOffset, 0.0, 1.0);
        
                vColor = vertexColor;
            }

                    ";

        }


        public static string load_frag_shader()
        {
            return @"

            #version 330 core

            in vec4 vColor;
            out vec4 fColor;
    
            void main()
            {
                // Simple color output without lighting
                fColor = vColor;
            }

                    ";

        }



        #endregion


        #region "Selection Shader"

        private static string selrect_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;

            out vec4 v_Color;

            void main()
            {
	            v_Color = vec4(0.8039f,0.3608f,0.3608f,0.5f);

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string selrect_frag_shader()
        {
            return @"

            #version 330 core

            in vec4 v_Color;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
	            f_Color = v_Color;
            }

                    ";

        }


        #endregion


        #region "Drawing Axis Shader"

        private static string drawingaxis_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in vec3 node_color;

            out vec4 v_Color;

            void main()
            {
	            v_Color = vec4(node_color, 1.0f);

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string drawingaxis_frag_shader()
        {
            return @"

            #version 330 core

            in vec4 v_Color;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
	            f_Color = v_Color;
            }

                    ";

        }


        #endregion


        #region "Contour Bar Shader"

        private static string contourbar_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in float node_value; // Value for the contour level between 0.0 and 1.0

            out float v_node_value;

            void main()
            {
	            // Map the node_value to a color (e.g., from blue to red)
	            v_node_value = node_value;

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string contourbar_frag_shader()
        {
            return @"

            #version 330 core

            in float v_node_value;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)
            
             vec3 jetHeatmap(float value) 
            {
                float t = value; // (value + 1.0) * 0.5;
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            void main()
            {
                vec3 contourColor = vec3(0.0); 
                
                if(v_node_value < 0.0f)
                    contourColor = vec3(0.4f, 0.4f, 0.4f); // Dark gray for negative values
                else if(v_node_value > 1.0f)
                    contourColor = vec3(0.8f, 0.8f, 0.8f); // Light gray for values greater than 1
                else
                    contourColor = jetHeatmap(v_node_value); // Use the heatmap for values between 0 and 1


	            f_Color = vec4(contourColor, 1.0f);
            }

                    ";

        }


        #endregion




        public static string get_vertex_shader(ShaderType type)
        {
            // Returns the vertex shader
            switch (type)
            {
                case ShaderType.MeshShader:
                    return mesh_vert_shader();
                case ShaderType.RsltMeshShader:
                    return rslt_mesh_vert_shader();
                case ShaderType.RsltWireframeShader:
                    return rslt_wireframe_vert_shader();
                case ShaderType.SelectionShader:
                    return selrect_vert_shader();
                case ShaderType.NodeConstraintShader:
                    return nodeconstraint_vert_shader();
                case ShaderType.LoadShader:
                    return load_vert_shader();
                case ShaderType.TextShader:
                    return text_vert_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_vert_shader();
                case ShaderType.ContourBarShader:
                    return contourbar_vert_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        public static string get_fragment_shader(ShaderType type)
        {
            // Returns the fragment shader
            switch (type)
            {
                case ShaderType.MeshShader:
                    return mesh_frag_shader();
                case ShaderType.RsltMeshShader:
                    return rslt_mesh_frag_shader();
                case ShaderType.RsltWireframeShader:
                    return rslt_wireframe_frag_shader();
                case ShaderType.SelectionShader:
                    return selrect_frag_shader();
                case ShaderType.NodeConstraintShader:
                    return nodeconstraint_frag_shader();
                case ShaderType.LoadShader:
                    return load_frag_shader();
                case ShaderType.TextShader:
                    return text_frag_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_frag_shader();
                case ShaderType.ContourBarShader:
                    return contourbar_frag_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        //___________________


    }
}
