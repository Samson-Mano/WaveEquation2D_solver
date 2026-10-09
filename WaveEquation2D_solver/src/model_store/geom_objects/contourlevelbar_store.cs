using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store.geom_objects;
using WaveEquation2D_solver.src.opentk_control.opentk_buffer;
using WaveEquation2D_solver.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace WaveEquation2D_solver.src.model_store.geom_objects
{
    public class contourlevelbar_store : IDisposable
    {
        // XY labels
        private label_list_store contourResultLabel;

        // Contour level label data
        private label_list_store contourLevelLabel;


        // Rectangle
        private Vector2 _RightBottomCornerPoint = Vector2.Zero;
        private Vector2 _RightTopCornerPoint = Vector2.Zero;

        const float CONTOUR_LEVELBAR_WIDTH = 0.04f; // Width of the contour level bar in pixels
        const float CONTOUR_LEVELTRI_WIDTH = 0.038f; // Width of the contour level triangle in pixels
        const int CONTOUR_LEVELS = 10;

        Vector3 ResultLabelColor = new Vector3(0.65f, 0.25f, 0.25f); // Brown color for the result label

        // Rendering resources
        private VertexArray _contourbarVAO;
        private VertexBuffer _contourbarVBO;
        private IndexBuffer _contourbarIBO;


        private VertexArray _contourtriVAO;
        private VertexBuffer _contourtriVBO;
        private IndexBuffer _contourtriIBO;


        private Shader _contourbarshader;
        private bool _disposed = false;

        private bool IsInitialized = false;



        public contourlevelbar_store()
        {
            // Empty constructor
        }



        public void InitializeContourLevelBarData(int window_width, int window_height)
        {
            // Initialize contour level bar data here
            InitializeShader();
            InitializeBuffers();
            SetupIndexBuffers();

            IsInitialized = true;

            contourResultLabel = new label_list_store();
            contourLevelLabel = new label_list_store();

            UpdateContourLevelBarPosition(window_width, window_height, 0.0f, 1.0f, "Dummy");
        }


        private void InitializeShader()
        {
            // Create Shader
            _contourbarshader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.ContourBarShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.ContourBarShader));


        }


        private void InitializeBuffers()
        {
            // Contour level bar
            _contourbarVAO = new VertexArray();
            _contourbarVBO = new VertexBuffer(CONTOUR_LEVELS * 2 * 3); // 2 points per level, 3 floats per point (x, y, colorValue)
            _contourbarIBO = new IndexBuffer(CONTOUR_LEVELS * 2); // 8 indices to form a rectangle (4 lines)


            _contourtriVAO = new VertexArray();
            _contourtriVBO = new VertexBuffer(CONTOUR_LEVELS * 2 * 3); // 2 points per level, 3 floats per point (x, y, colorValue)
            _contourtriIBO = new IndexBuffer(CONTOUR_LEVELS * 6); // 6 indices to form a rectangle (2 triangles)

            var contourbarLayout = new VertexBufferLayout();
            contourbarLayout.AddFloat(2);
            contourbarLayout.AddFloat(1);

            _contourbarVAO.Add_vertexBuffer(_contourbarVBO, contourbarLayout);
            _contourtriVAO.Add_vertexBuffer(_contourtriVBO, contourbarLayout);

        }


        private void SetupIndexBuffers()
        {
            // Setup contour level bar indices
            int[] lineIndices =  new int[CONTOUR_LEVELS * 2];

            lineIndices[0] = 0;
            lineIndices[1] = 1;

            for (int i = 1; i < CONTOUR_LEVELS; i++)
            {
                lineIndices[i * 2] = (i * 2);
                lineIndices[(i * 2) + 1] = (i * 2) + 1;

            }
            _contourbarIBO.AppendIndexBuffer(lineIndices);


            int[] triIndices = new int[CONTOUR_LEVELS * 6];
            
            for (int i = 0; i < CONTOUR_LEVELS - 1; i++)
            {
                int baseIndex = i * 6;
                int vertexIndex = i * 2;
                triIndices[baseIndex + 0] = vertexIndex + 0;
                triIndices[baseIndex + 1] = vertexIndex + 1;
                triIndices[baseIndex + 2] = vertexIndex + 2;
                triIndices[baseIndex + 3] = vertexIndex + 1;
                triIndices[baseIndex + 4] = vertexIndex + 3;
                triIndices[baseIndex + 5] = vertexIndex + 2;
            }

            _contourtriIBO.AppendIndexBuffer(triIndices);
        }


        public void UpdateContourLevelBarPosition(int window_width, int window_height, float contour_min, float contour_max,
            string resultLabel)
        {
            if (!IsInitialized)
                return;


            // 1. Determine max dimension
            int max_drawing_area_size = Math.Max(window_width, window_height);

            int drawing_area_center_x = (int)((window_width - max_drawing_area_size) * 0.5f);
            int drawing_area_center_y = (int)((window_height - max_drawing_area_size) * 0.5f);

            // 2. Normalize screen dimensions
            double normalizedScreenWidth = 2.0d * ((double)window_width / (double)max_drawing_area_size);
            double normalizedScreenHeight = 2.0d * ((double)window_height / (double)max_drawing_area_size);

            float rightBotX = (float)normalizedScreenWidth * 0.5f;
            float rightBotY = -(float)normalizedScreenHeight * 0.5f;

            float rightTopX = (float)normalizedScreenWidth * 0.5f;
            float rightTopY = (float)normalizedScreenHeight * 0.5f;


            // Assign to private rectangle points
            this._RightBottomCornerPoint = new Vector2(rightBotX, rightBotY);
            this._RightTopCornerPoint = new Vector2(rightTopX, rightTopY);

            UpdateVertexBuffers(contour_min, contour_max, resultLabel);

            // Update the label shader uniforms for the new window size
            Matrix4 uMVP = Matrix4.Identity;

            contourResultLabel.update_openTK_uniforms(uMVP, 1.0f, 1.0f);
            contourLevelLabel.update_openTK_uniforms(uMVP, 1.0f, 1.0f);

        }



        private void UpdateVertexBuffers(float contour_min, float contour_max, string resultLabel)
        {
            // Define the 4 corners of the rectangle
            float[] vertices = new float[CONTOUR_LEVELS * 2 * 3];

            float drawing_area_height = _RightTopCornerPoint.Y - _RightBottomCornerPoint.Y;
            float contour_bar_height = (float)(drawing_area_height * 0.8f); // 80% of the drawing area height

            float OriginX = _RightBottomCornerPoint.X - 0.1f;
            float OriginY = _RightBottomCornerPoint.Y + (float)(drawing_area_height * 0.1f);

            float levelY = 0.0f;
            float colorValue = 0.0f;

            // Bottop point of the contour bar
            colorValue = getColorAtContourLevel(0, contour_min, contour_max);

            vertices[0] = OriginX;
            vertices[1] = OriginY;
            vertices[2] = colorValue;

            vertices[3] = OriginX - CONTOUR_LEVELBAR_WIDTH;
            vertices[4] = OriginY;
            vertices[5] = colorValue;

            for (int i = 1; i< (CONTOUR_LEVELS - 1); i++)
            {
                levelY = ((contour_bar_height * i) / (CONTOUR_LEVELS - 1));
                colorValue = getColorAtContourLevel(i, contour_min, contour_max);

                vertices[(i*6) + 0] = OriginX;
                vertices[(i*6) + 1] = OriginY + levelY;
                vertices[(i*6) + 2] = colorValue;

                vertices[(i*6) + 3] = OriginX - CONTOUR_LEVELBAR_WIDTH;
                vertices[(i*6) + 4] = OriginY + levelY;
                vertices[(i*6) + 5] = colorValue;

            }


            colorValue = getColorAtContourLevel((CONTOUR_LEVELS - 1), contour_min, contour_max);

            vertices[((CONTOUR_LEVELS - 1) * 6) + 0] = OriginX;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 1] = OriginY + contour_bar_height;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 2] = colorValue;

            vertices[((CONTOUR_LEVELS - 1) * 6) + 3] = OriginX - CONTOUR_LEVELBAR_WIDTH;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 4] = OriginY + contour_bar_height;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 5] = colorValue;


            // Update both VBOs with the same vertices
            _contourbarVBO.updateVertexBuffer(vertices);



            //________________________________________________________________________________________________________
            // Bottop point of the contour bar
            colorValue = getColorAtContourLevel(0, contour_min, contour_max);

            vertices[0] = OriginX;
            vertices[1] = OriginY;
            vertices[2] = colorValue;

            vertices[3] = OriginX - CONTOUR_LEVELTRI_WIDTH;
            vertices[4] = OriginY;
            vertices[5] = colorValue;

            for (int i = 1; i < (CONTOUR_LEVELS - 1); i++)
            {
                levelY = ((contour_bar_height * i) / (CONTOUR_LEVELS - 1));
                colorValue = getColorAtContourLevel(i, contour_min, contour_max);

                vertices[(i * 6) + 0] = OriginX;
                vertices[(i * 6) + 1] = OriginY + levelY;
                vertices[(i * 6) + 2] = colorValue;

                vertices[(i * 6) + 3] = OriginX - CONTOUR_LEVELTRI_WIDTH;
                vertices[(i * 6) + 4] = OriginY + levelY;
                vertices[(i * 6) + 5] = colorValue;

            }

            colorValue = getColorAtContourLevel((CONTOUR_LEVELS - 1), contour_min, contour_max);

            vertices[((CONTOUR_LEVELS - 1) * 6) + 0] = OriginX;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 1] = OriginY + contour_bar_height;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 2] = colorValue;

            vertices[((CONTOUR_LEVELS - 1) * 6) + 3] = OriginX - CONTOUR_LEVELTRI_WIDTH;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 4] = OriginY + contour_bar_height;
            vertices[((CONTOUR_LEVELS - 1) * 6) + 5] = colorValue;


            _contourtriVBO.updateVertexBuffer(vertices);

            // Update the contour level labels
            UpdateContourLevelLabels(contour_min, contour_max);


            //_______________________________________________________________________________________________________
            // Result label
            contourResultLabel.clear_labels();

            contourResultLabel.add_label(0, resultLabel,
                 new Vector2(OriginX - (resultLabel.Length * 0.01f) * 0.5f, OriginY + contour_bar_height + 0.07f),
                 new Vector3(0.0f, 0.0f, 0.0f));

            contourResultLabel.update_buffer(1.3f);

        }


        public float getColorAtContourLevel(int level_i, float contour_min, float contour_max)
        {

            // Get zoom range
            float zoomMin = Math.Max(0.0f, Math.Min(1.0f, gvariables_static.contourLevel_rangeMin));
            float zoomMax = Math.Max(0.0f, Math.Min(1.0f, gvariables_static.contourLevel_rangeMax));

            if (zoomMin >= zoomMax)
            {
                zoomMin = 0.0f;
                zoomMax = 1.0f;
            }


            //// Calculate the actual values at zoom boundaries
            //float actualRangeMin = contour_min + (contour_max - contour_min) * zoomMin;
            //float actualRangeMax = contour_min + (contour_max - contour_min) * zoomMax;
            //float actualRangeSpan = actualRangeMax - actualRangeMin;


            bool isZoomed = (zoomMin > 0.001f || zoomMax < 0.999f);
            bool isZoomedMin = (zoomMin > 0.001f);
            bool isZoomedMax = (zoomMax < 0.999f);
            bool isZoomedBoth = (isZoomedMin && isZoomedMax);


            if (isZoomed)
            {
                if (isZoomedBoth)
                {
                    if (level_i == 0)
                    {
                        // Bottom: show actual minimum
                        return -1.0f; // Light gray
                    }
                    else if (level_i == (CONTOUR_LEVELS - 1))
                    {
                        // Top: show actual maximum
                        return 2.0f; // Light gray
                      }
                    else
                    {
                        // Middle: map zoom range to [0,1]
                        return (float)(level_i - 1.0f) / (float)(CONTOUR_LEVELS - 3);

                    }

                }
                else if (isZoomedMin)
                {
                    if (level_i == 0)
                    {
                        // Bottom: show actual minimum
                        return -1.0f; // Light gray
                    }
                    else
                    {
                        // Rest: map zoom range to [0,1]
                        return (float)(level_i - 1.0f) / (float)(CONTOUR_LEVELS - 2);
                    }

                }
                else if (isZoomedMax)
                {
                    if (level_i == (CONTOUR_LEVELS - 1))
                    {
                        // Top: show actual maximum
                        return 2.0f; // Light gray
                    }
                    else
                    {
                        // Rest: map zoom range to [0,1]
                        return (float)(level_i) / (float)(CONTOUR_LEVELS - 2);
                    }
                }
                else
                {                     
                    // Should not reach here, but fallback to standard mapping
                    return (float)(level_i) / (float)(CONTOUR_LEVELS - 1);
                }
            }
            else
            {
                // Full range - standard mapping
                return (float)(level_i) / (float)(CONTOUR_LEVELS - 1);
            }
            //
        }


        public void UpdateContourLevelLabels(float contour_min, float contour_max)
        {

            float drawing_area_height = _RightTopCornerPoint.Y - _RightBottomCornerPoint.Y;
            float contour_bar_height = drawing_area_height * 0.8f;

            float OriginX = _RightBottomCornerPoint.X - 0.1f;
            float OriginY = _RightBottomCornerPoint.Y + (drawing_area_height * 0.1f);
            float levelX = OriginX - CONTOUR_LEVELBAR_WIDTH;


            // Get zoom range
            float zoomMin = Math.Max(0.0f, Math.Min(1.0f, gvariables_static.contourLevel_rangeMin));
            float zoomMax = Math.Max(0.0f, Math.Min(1.0f, gvariables_static.contourLevel_rangeMax));

            if (zoomMin >= zoomMax)
            {
                zoomMin = 0.0f;
                zoomMax = 1.0f;
            }


            // Calculate the actual values at zoom boundaries
            float actualRangeMin = contour_min + (contour_max - contour_min) * zoomMin;
            float actualRangeMax = contour_min + (contour_max - contour_min) * zoomMax;
            float actualRangeSpan = actualRangeMax - actualRangeMin;


            bool isZoomed = (zoomMin > 0.001f || zoomMax < 0.999f);
            bool isZoomedMin = (zoomMin > 0.001f);
            bool isZoomedMax = (zoomMax < 0.999f);
            bool isZoomedBoth = (isZoomedMin && isZoomedMax);


            contourLevelLabel.clear_labels();

            for (int i = 0; i < CONTOUR_LEVELS; i++)
            {
         
                float contour_value = 0.0f;
                Vector3 color = new Vector3(0.0f, 0.0f, 0.0f);

                if (isZoomed)
                {
                    if(isZoomedBoth)
                    {
                        if (i == 0)
                        {
                            // Bottom: show actual minimum
                            contour_value = contour_min;
                            color = new Vector3(0.4f, 0.4f, 0.4f); // Dark gray
                        }  
                        else if(i == (CONTOUR_LEVELS - 1))
                        {
                            // Top: show actual maximum
                            contour_value = contour_max;
                            color = new Vector3(0.8f, 0.8f, 0.8f); // Light gray
                        }
                        else
                        {
                            // Middle: map zoom range to [0,1]
                            float remappedPos = (float)(i - 1.0f) / (float)(CONTOUR_LEVELS - 3);

                            contour_value = actualRangeMin + actualRangeSpan * remappedPos;
                            color = gvariables_static.GetJetColorClamped(remappedPos);

                        }

                    }
                    else if (isZoomedMin)
                    {
                        if( i == 0)
                        {
                            // Bottom: show actual minimum
                            contour_value = contour_min;
                            color = new Vector3(0.4f, 0.4f, 0.4f); // Dark gray
                        }
                        else
                        {
                            // Rest: map zoom range to [0,1]
                            float remappedPos = (float)(i - 1.0f) / (float)(CONTOUR_LEVELS - 2);
                            contour_value = actualRangeMin + (contour_max - actualRangeMin) * remappedPos;
                            color = gvariables_static.GetJetColorClamped(remappedPos);
                        }

                    }
                    else if(isZoomedMax)
                    {
                        if(i == (CONTOUR_LEVELS -1))
                        {
                            // Top: show actual maximum
                            contour_value = contour_max;
                            color = new Vector3(0.8f, 0.8f, 0.8f); // Light gray
                        }
                        else
                        {
                            // Rest: map zoom range to [0,1]
                            float remappedPos = (float)i / (float)(CONTOUR_LEVELS - 2);
                            contour_value = contour_min + (actualRangeMax - contour_min) * remappedPos;
                            color = gvariables_static.GetJetColorClamped(remappedPos);
                        }
                    }
                }
                else
                {
                    // Full range - standard mapping
                    float remappedPos = (float)i / (float)(CONTOUR_LEVELS - 1);
                    contour_value = contour_min + (contour_max - contour_min) * remappedPos;
                    color = gvariables_static.GetJetColorClamped(remappedPos);
                }


                float normalizedPosition = (float)i / (float)(CONTOUR_LEVELS - 1);
                float levelY = OriginY + (contour_bar_height * normalizedPosition) + 0.015f;

                string label = FormatContourValue(contour_value);
                float string_width = label.Length * 0.01f;

                contourLevelLabel.add_label(i, label,
                    new Vector2(levelX - string_width, levelY), color);
            }

            contourLevelLabel.update_buffer(1.1f);

        }


        private string FormatContourValue(float value)
        {
            // Determine precision based on value magnitude
            float absValue = Math.Abs(value);

            if (absValue < 0.001f)
                return value.ToString("F6");
            else if (absValue < 0.01f)
                return value.ToString("F5");
            else if (absValue < 0.1f)
                return value.ToString("F4");
            else if (absValue < 1.0f)
                return value.ToString("F3");
            else if (absValue < 10.0f)
                return value.ToString("F2");
            else if (absValue < 100.0f)
                return value.ToString("F1");
            else
                return value.ToString("F0");
        }



        public void draw_contour_bar()
        {

            _contourbarshader.Bind();

            //______________________________________________________________
            _contourbarVAO.Bind();
            _contourbarIBO.Bind();

            GL.LineWidth(3.0f);
            GL.DrawElements(PrimitiveType.Lines, CONTOUR_LEVELS * 2,
                           DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.LineWidth(1.0f);

            // Cleanup 1
            _contourbarIBO.UnBind();
            _contourbarVAO.UnBind();


            //______________________________________________________________
            _contourtriVAO.Bind();
            _contourtriIBO.Bind();

            GL.DrawElements(PrimitiveType.Triangles, CONTOUR_LEVELS * 6,
                           DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Cleanup 2
            _contourtriIBO.UnBind();
            _contourtriVAO.UnBind();

            
        
             _contourbarshader.UnBind();

            // Paint the contour level labels
            contourLevelLabel.paint_static_labels();

            // Paint the result label
            contourResultLabel.paint_static_labels();   
        }



        public void Dispose()
        {
            if (!_disposed)
            {
                _contourbarVAO?.Dispose();
                _contourbarVBO?.Dispose();
                _contourbarIBO?.Dispose();
                _contourtriVAO?.Dispose();
                _contourtriVBO?.Dispose();
                _contourtriIBO?.Dispose();
                // _shader?.Dispose();
                _disposed = true;
            }
        }




    }
}
