// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEquation2D_solver.src.events_handler;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store.geom_objects;
using WaveEquation2D_solver.src.opentk_control.opentk_buffer;
using WaveEquation2D_solver.src.opentk_control.shader_compiler;



namespace WaveEquation2D_solver.src.model_store.rslt_objects
{
    public class rsltdata_store : IDisposable
    {


        private struct point_store
        {
            public int point_id;
            public double x_coord;
            public double y_coord;

            public List<double> field_values;
            public List<double> first_derivative_field_values;
            public List<double> second_derivative_field_values;

        }

        private struct line_store
        {
            public int line_id;
            public int line_start_id;
            public int line_end_id;
        }

        private struct tri_store
        {
            public int tri_id;
            public int pt_id1;
            public int pt_id2;
            public int pt_id3;

        }



        public struct result_extremes
        {
            // Field Values (Displacement) (option = 1)
            public double max_fieldvalues;
            public double min_fieldvalues;

            // First Derivative Field Values (Velocity) (option = 2)
            public double max_firstderivativefield;
            public double min_firstderivativefield;

            // Second Derivative Field Values (Acceleration) (option = 3)
            public double max_secondderivativefield;
            public double min_secondderivativefield;

        }



        private Dictionary<int, point_store> points = new Dictionary<int, point_store>();
        private List<line_store> wireframe_lines = new List<line_store>();
        private List<tri_store> tris = new List<tri_store>();

        private List<double> timeVector = new List<double>();

        public result_extremes rslt_extremes { get { return _rslt_extremes; } }
        private result_extremes _rslt_extremes;

        // public bool isResultSet = false;


        private Shader rsltmeshShader;
        private Shader rsltmeshwireframeShader;


        // Vertex Buffer object and Vertex Array object 
        private VertexBuffer point_vbo;
        private VertexArray point_vao;



        // Index buffer for the points, wireframe lines, and triangles (EBO)
        private IndexBuffer point_ibo;
        private IndexBuffer wireframe_ibo;
        private IndexBuffer triangle_ibo;


        // Result point label
        private label_list_store result_point_label;


        // // Shrunk mesh data
        // private shrunkrsltdata_store shrunk_rsltmesh_data = new shrunkrsltdata_store();

        public HashSet<int> selected_resultpoint_ids { get; } = new HashSet<int>();


        private bool buffersInitialized = false;

        public rsltdata_store()
        {
            InitializeShader();

            result_point_label = new label_list_store();
        }


        private void InitializeShader()
        {
            // Create Shader
            rsltmeshShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.RsltMeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.RsltMeshShader)
                );

            rsltmeshwireframeShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.RsltWireframeShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.RsltWireframeShader)
                );

        }



        public void add_point(int point_id, double x_coord, double y_coord,
            List<double> field_values, 
            List<double> first_derivative_field_values, 
            List<double> second_derivative_field_values)
        {

            points.Add(point_id, new point_store()
            {
                point_id = point_id,
                x_coord = x_coord,
                y_coord = y_coord,
                field_values = field_values,
                first_derivative_field_values = first_derivative_field_values,
                second_derivative_field_values = second_derivative_field_values
            });

        }

        public void add_wireframe_line(int line_id, int line_start_id, int line_end_id)
        {
            wireframe_lines.Add(new line_store()
            {
                line_id = line_id,
                line_start_id = line_start_id,
                line_end_id = line_end_id
            });
        }


        public void add_tri(int tri_id, int pt_id1, int pt_id2, int pt_id3)
        {
            tris.Add(new tri_store()
            {
                tri_id = tri_id,
                pt_id1 = pt_id1,
                pt_id2 = pt_id2,
                pt_id3 = pt_id3,
            });

        }


        public bool set_result_extremes()
        {
            // Result extremes are calculated based on the points data
            _rslt_extremes = new result_extremes();

            _rslt_extremes.max_fieldvalues = double.MinValue;
            _rslt_extremes.min_fieldvalues = double.MaxValue;
            _rslt_extremes.max_firstderivativefield = double.MinValue;
            _rslt_extremes.min_firstderivativefield = double.MaxValue;
            _rslt_extremes.max_secondderivativefield = double.MinValue;
            _rslt_extremes.min_secondderivativefield = double.MaxValue;

            int num_time_steps = timeVector.Count;


            foreach (var pt in points.Values)
            {
                // Maximum and minimum field values magnitude
                _rslt_extremes.max_fieldvalues = Math.Max(_rslt_extremes.max_fieldvalues, pt.field_values.Max());
                _rslt_extremes.min_fieldvalues = Math.Min(_rslt_extremes.min_fieldvalues, pt.field_values.Min());

                // Maximum and minimum first derivative field values
                _rslt_extremes.max_firstderivativefield = Math.Max(_rslt_extremes.max_firstderivativefield, pt.first_derivative_field_values.Max());
                _rslt_extremes.min_firstderivativefield = Math.Min(_rslt_extremes.min_firstderivativefield, pt.first_derivative_field_values.Min());
                
                // Maximum and minimum second derivative field values   
                _rslt_extremes.max_secondderivativefield = Math.Max(_rslt_extremes.max_secondderivativefield, pt.second_derivative_field_values.Max());
                _rslt_extremes.min_secondderivativefield = Math.Min(_rslt_extremes.min_secondderivativefield, pt.second_derivative_field_values.Min());

            }


            // Validate the result extremes to ensure they are meaningful
            if (!check_double(_rslt_extremes.max_fieldvalues) || !check_double(_rslt_extremes.min_fieldvalues))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_firstderivativefield) || !check_double(_rslt_extremes.min_firstderivativefield))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_secondderivativefield) || !check_double(_rslt_extremes.min_secondderivativefield))
            {
                return false;
            }
           
            return true;
        }


        private bool check_double(double value)
        {
            // Check if the double value is valid (not NaN or Infinity)
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }


        public void paint_results()
        {

            paint_result_mesh();

            paint_result_mesh_wireframe();

            paint_result_mesh_points();

            paint_selected_result_points();

        }


        private void paint_result_mesh()
        {
            if (!gvariables_static.is_paint_resultmesh || !buffersInitialized)
                return;

            rsltmeshShader.Bind();

            if (gvariables_static.is_paint_shrunk_triangle)
            {
                // // Paint the shrunk mesh 
                // shrunk_rsltmesh_data.paint_shrunk_rsltmesh();
                rsltmeshShader.UnBind();
                return;
            }


            point_vao.Bind();

            if (triangle_ibo.BufferCount > 0)
            {
                // Paint the Result triangle mesh
                triangle_ibo.Bind();
                GL.DrawElements(PrimitiveType.Triangles, triangle_ibo.BufferCount,
                    DrawElementsType.UnsignedInt, 0);
                triangle_ibo.UnBind();
            }

            point_vao.UnBind();
            rsltmeshShader.UnBind();


        }


        private void paint_result_mesh_wireframe()
        {
            if (!gvariables_static.is_paint_resultmesh_boundaries || !buffersInitialized)
                return;


            if (wireframe_ibo.BufferCount > 0)
            {
                rsltmeshwireframeShader.Bind();

                point_vao.Bind();
                wireframe_ibo.Bind();

                GL.DrawElements(PrimitiveType.Lines, wireframe_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);

                rsltmeshwireframeShader.UnBind();
                point_vao.UnBind();
                wireframe_ibo.UnBind();
            }

        }


        private void paint_result_mesh_points()
        {
            if (!gvariables_static.is_paint_resultmeshpoints || !buffersInitialized)
                return;


            if (point_ibo.BufferCount > 0)
            {
                // Paint the result mesh points
                rsltmeshShader.Bind();

                point_vao.Bind();
                point_ibo.Bind();

                GL.PointSize(2.0f);
                GL.DrawElements(PrimitiveType.Points, point_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);
                GL.PointSize(1.0f);

                rsltmeshShader.UnBind();
                point_vao.UnBind();
                point_ibo.UnBind();

            }

        }


        public void switch_result_option()
        {
            // Switch the result option for visualization
            // 1 = Field Values (Displacement),
            // 2 = First Derivative Field Values (Velocity),
            // 3 = Second Derivative Field Values (Acceleration)

            int option = gvariables_static.result_option;



            List<float> vertexData = new List<float>();

            for (int i = 0; i < points.Count; i++)
            {
                point_store pt = points[i];
                vertexData.Add((float)pt.x_coord);
                vertexData.Add((float)pt.y_coord);

                // Normalized displacement values for plotting
                if (pt.displ_magnitude > 0)
                {
                    vertexData.Add((float)(pt.displ_x / pt.displ_magnitude));
                    vertexData.Add((float)(pt.displ_y / pt.displ_magnitude));
                }
                else
                {
                    vertexData.Add(0);
                    vertexData.Add(0);
                }

                vertexData.Add((float)(pt.displ_magnitude / _rslt_extremes.max_displacement));

                float normalized_contourValue = scaled_contourColorValue(pt, option);
                vertexData.Add(normalized_contourValue);

            }


            point_vbo.updateVertexBuffer(vertexData.ToArray());


        }


        private float scaled_contourColorValue(point_store pt, int option)
        {

            const float EPSILON = 1e-6f;


            // Get zoom range
            float zoomMin = Math.Max(0.0f, Math.Min(1.0f, gvariables_static.contourLevel_rangeMin));
            float zoomMax = Math.Max(0.0f, Math.Min(1.0f, gvariables_static.contourLevel_rangeMax));

            if (zoomMin >= zoomMax)
            {
                zoomMin = 0.0f;
                zoomMax = 1.0f;
            }

            switch (option)
            {
                case 1: // Field Values (Displacement)
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_fieldvalues +
                            ((_rslt_extremes.max_fieldvalues - _rslt_extremes.min_fieldvalues) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_fieldvalues +
                            ((_rslt_extremes.max_fieldvalues - _rslt_extremes.min_fieldvalues) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.sigma_x - actualRangeMin) / actualRangeSpan;

                        if (normalizedValue < -EPSILON)
                        {
                            normalizedValue = -1.0f;
                        }
                        else if (normalizedValue > 1.0f + EPSILON)
                        {
                            normalizedValue = 2.0f;
                        }
                        else
                        {
                            // Clamp the normalized value to [0, 1] range
                            normalizedValue = Math.Max(0.0f, Math.Min(1.0f, normalizedValue));
                        }


                        normalizedValue = (normalizedValue * 2.0f) - 1.0f; // Scale to [-1, 1]

                        return normalizedValue;

                    }
                case 2: // First Derivative Field Values (Velocity)
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_firstderivativefield +
                            ((_rslt_extremes.max_firstderivativefield - _rslt_extremes.min_firstderivativefield) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_firstderivativefield +
                            ((_rslt_extremes.max_firstderivativefield - _rslt_extremes.min_firstderivativefield) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.sigma_y - actualRangeMin) / actualRangeSpan;

                        if (normalizedValue < -EPSILON)
                        {
                            normalizedValue = -1.0f;
                        }
                        else if (normalizedValue > 1.0f + EPSILON)
                        {
                            normalizedValue = 2.0f;
                        }
                        else
                        {
                            // Clamp the normalized value to [0, 1] range
                            normalizedValue = Math.Max(0.0f, Math.Min(1.0f, normalizedValue));
                        }

                        normalizedValue = (normalizedValue * 2.0f) - 1.0f; // Scale to [-1, 1]

                        return normalizedValue;

                    }
                case 3: // Second Derivative Field Values (Acceleration)
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_secondderivativefield +
                            ((_rslt_extremes.max_secondderivativefield - _rslt_extremes.min_secondderivativefield) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_secondderivativefield +
                            ((_rslt_extremes.max_secondderivativefield - _rslt_extremes.min_secondderivativefield) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.tau_xy - actualRangeMin) / actualRangeSpan;

                        if (normalizedValue < -EPSILON)
                        {
                            normalizedValue = -1.0f;
                        }
                        else if (normalizedValue > 1.0f + EPSILON)
                        {
                            normalizedValue = 2.0f;
                        }
                        else
                        {
                            // Clamp the normalized value to [0, 1] range
                            normalizedValue = Math.Max(0.0f, Math.Min(1.0f, normalizedValue));
                        }

                        normalizedValue = (normalizedValue * 2.0f) - 1.0f; // Scale to [-1, 1]

                        return normalizedValue;

                    }
          
            }

            return 0.0f; // Default case, should not reach here
        }


        public void create_buffer_data()
        {

            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> vertexData = new List<float>();
            List<int> pointIndexData = new List<int>();

            for (int i = 0; i < points.Count; i++)
            {
                point_store pt = points[i];
                vertexData.Add((float)pt.x_coord);
                vertexData.Add((float)pt.y_coord);

                // Normalized displacement values for plotting
                if (pt.displ_magnitude > 0)
                {
                    vertexData.Add((float)(pt.displ_x / pt.displ_magnitude));
                    vertexData.Add((float)(pt.displ_y / pt.displ_magnitude));
                }
                else
                {
                    vertexData.Add(0);
                    vertexData.Add(0);
                }

                // Calculate the magnitude of the displacement vector for color mapping
                vertexData.Add((float)(pt.displ_magnitude / _rslt_extremes.max_displacement)); // normalized scalar value
                vertexData.Add((float)(pt.displ_magnitude / _rslt_extremes.max_displacement)); // Contour value for color mapping

                pointIndexData.Add(i);
            }


            // Create VAO and VBO for points
            point_vao = new VertexArray();
            point_vbo = new VertexBuffer(Math.Max(10, vertexData.Count));
            point_ibo = new IndexBuffer(Math.Max(10, pointIndexData.Count));
            selected_resultpoint_ibo = new IndexBuffer(10);


            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(2);  // x and y coordinates
            pointLayout.AddFloat(2); // displ_x and displ_y
            pointLayout.AddFloat(1); // displacement magnitude
            pointLayout.AddFloat(1); // Contour value

            point_vao.Add_vertexBuffer(point_vbo, pointLayout);


            point_vbo.AppendVertexBuffer(vertexData.ToArray());
            point_ibo.AppendIndexBuffer(pointIndexData.ToArray());

            //_______________________________________________________________
            // prepare wireframe index data for openGL
            List<int> wireframeIndexData = new List<int>();

            foreach (line_store ln in wireframe_lines)
            {

                wireframeIndexData.Add(ln.line_start_id);
                wireframeIndexData.Add(ln.line_end_id);
            }


            wireframe_ibo = new IndexBuffer(Math.Max(10, wireframeIndexData.Count));
            if (wireframeIndexData.Count > 0)
            {
                wireframe_ibo.AppendIndexBuffer(wireframeIndexData.ToArray());
            }

            //_______________________________________________________________
            // prepare triangle index data for openGL
            List<int> triangleIndexData = new List<int>();

            foreach (tri_store tri in tris)
            {

                triangleIndexData.Add(tri.pt_id1);
                triangleIndexData.Add(tri.pt_id2);
                triangleIndexData.Add(tri.pt_id3);

            }

            triangle_ibo = new IndexBuffer(Math.Max(10, triangleIndexData.Count));
            if (triangleIndexData.Count > 0)
            {
                triangle_ibo.AppendIndexBuffer(triangleIndexData.ToArray());
            }


            // PSL Mesh buffers
            generate_PSL_mesh();

            // Create the Type 2 PSL mesh buffers
            // generate_PSL_type2_line_mesh();
            // generate_PSL_type2_streamfunction_mesh();

            get_PSL_streamfunction_mesh();

            // Shrunk Mesh buffers
            generate_shrunk_mesh();


            buffersInitialized = true;

        }



        private void generate_shrunk_mesh()
        {

            // Generate shrunk vertices for triangles
            foreach (tri_store tri in tris)
            {
                var p1 = points[tri.pt_id1];
                var p2 = points[tri.pt_id2];
                var p3 = points[tri.pt_id3];

                float[] pt1_values = new float[5];
                pt1_values[0] = (float)p1.x_coord;
                pt1_values[1] = (float)p1.y_coord;
                if (p1.displ_magnitude > 0)
                {
                    pt1_values[2] = (float)(p1.displ_x / p1.displ_magnitude);
                    pt1_values[3] = (float)(p1.displ_y / p1.displ_magnitude);
                }
                else
                {
                    pt1_values[2] = 0;
                    pt1_values[3] = 0;
                }
                pt1_values[4] = (float)(p1.displ_magnitude / _rslt_extremes.max_displacement);


                float[] pt2_values = new float[5];
                pt2_values[0] = (float)p2.x_coord;
                pt2_values[1] = (float)p2.y_coord;
                if (p2.displ_magnitude > 0)
                {
                    pt2_values[2] = (float)(p2.displ_x / p2.displ_magnitude);
                    pt2_values[3] = (float)(p2.displ_y / p2.displ_magnitude);
                }
                else
                {
                    pt2_values[2] = 0;
                    pt2_values[3] = 0;
                }
                pt2_values[4] = (float)(p2.displ_magnitude / _rslt_extremes.max_displacement);


                float[] pt3_values = new float[5];
                pt3_values[0] = (float)p3.x_coord;
                pt3_values[1] = (float)p3.y_coord;
                if (p3.displ_magnitude > 0)
                {
                    pt3_values[2] = (float)(p3.displ_x / p3.displ_magnitude);
                    pt3_values[3] = (float)(p3.displ_y / p3.displ_magnitude);
                }
                else
                {
                    pt3_values[2] = 0;
                    pt3_values[3] = 0;
                }
                pt3_values[4] = (float)(p3.displ_magnitude / _rslt_extremes.max_displacement);

                // shrunk_rsltmesh_data.add_shrunk_triangle(tri.tri_id, pt1_values, pt2_values, pt3_values);
            }


            // // Initialize the buffer
            // shrunk_rsltmesh_data.create_shrunkrsltmesh_buffer_data();

        }



        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                graphic_events_control.viewMatrix * graphic_events_control.modelMatrix;

            rsltmeshShader.SetMatrix4("uMVP", uMVP);
            rsltmeshShader.SetFloat("geomscale", gvariables_static.geom_size);

            float model_percent = (float)(gvariables_static.displacement_scale / 1000.0);

            rsltmeshShader.SetFloat("modelpercent", model_percent);

            rsltmeshShader.SetFloat("vertexTransparency", gvariables_static.rslt_transparency);

            rsltmeshShader.SetFloat("rsltoption", 0);

            if (gvariables_static.result_option != 1)
            {
                rsltmeshShader.SetFloat("rsltoption", 1);
            }

            //____________________________________________________________________________________________

            rsltmeshwireframeShader.SetMatrix4("uMVP", uMVP);
            rsltmeshwireframeShader.SetFloat("geomscale", gvariables_static.geom_size);
            rsltmeshwireframeShader.SetFloat("modelpercent", model_percent);

            // rsltmeshwireframeShader.SetFloat("vertexTransparency", gvariables_static.rslt_transparency);

            Vector3 customColor = new Vector3(1.0f, 1.0f, 1.0f); // Fallback color

            // rsltmeshwireframeShader.SetVector3("wireframeColor", customColor);
            rsltmeshwireframeShader.SetFloat("wireframeAlpha", 0.5f);

            //____________________________________________________________________________________________
            // Contour data update
            rsltmeshShader.SetFloat("uNumContours", gvariables_static.contourline_level);
            rsltmeshShader.SetFloat("uLineOpacity", 0.0f);

            if (gvariables_static.is_paint_result_contourlines)
            {
                rsltmeshShader.SetFloat("uLineOpacity", 1.0f);
            }

            // Update the result label uniforms
            float zoomscale = (float)graphic_events_control.zoom_val;
            result_point_label.update_openTK_uniforms(uMVP, zoomscale, 1.0f);


        }

        public void update_animation(float sine_oscillation)
        {
            // Update the sine oscillation value in the shader for animation
            rsltmeshShader.SetFloat("sinevalue", sine_oscillation);

            rsltmeshwireframeShader.SetFloat("sinevalue", sine_oscillation);

            if (gvariables_static.is_paint_result_contourlines)
            {
                rsltmeshShader.SetFloat("uLineOpacity", 1.0f);
            }

            // rsltPSLShader.SetFloat("sinevalue", sine_oscillation);
            // rsltPSLType2Shader.SetFloat("sinevalue", sine_oscillation);
            // rsltPSLShader.SetFloat("uLineOpacity", 1.0f);


            if (sine_oscillation < 0.1)
            {
                rsltmeshShader.SetFloat("uLineOpacity", 0.0f);

                // rsltPSLShader.SetFloat("uLineOpacity", 0.0f);
            }

        }



        public void Dispose()
        {
            point_vbo?.Dispose();
            point_vao?.Dispose();
            point_ibo?.Dispose();
            wireframe_ibo?.Dispose();
            triangle_ibo?.Dispose();
            // meshShader?.Dispose();

        }


        //______________________________



    }
}
