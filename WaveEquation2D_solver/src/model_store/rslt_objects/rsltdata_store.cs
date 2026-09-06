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

            public double displ_x;
            public double displ_y;
            public double displ_magnitude;

            public double sigma_x;
            public double sigma_y;
            public double tau_xy;

            public double sigma_1;
            public double sigma_2;

            public double von_mises;
            public double max_shear;
            public double theta_p; // principal stress angle

            public double streamfunction_tension;
            public double streamfunction_compression;

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


        private struct reaction_store
        {
            public int point_id;
            public double x_coord;
            public double y_coord;

            public int constraint_type; // 0 = free, 1 = pinned, 2 = roller
            public double constraint_angle;

            public double reaction_x;
            public double reaction_y;
        }


        public struct result_extremes
        {
            // Displacement (option = 1)
            public double max_displacement;

            // Stress extremes in X direction (option = 2)
            public double max_stressX;
            public double min_stressX;

            // Stress extremes in Y direction (option = 3)
            public double max_stressY;
            public double min_stressY;

            // Shear stress extremes (option = 4)
            public double max_tauXY;
            public double min_tauXY;

            // Von Mises stress extremes (option = 5)
            public double max_vonMises;
            public double min_vonMises;

            // Principal stress 1 extremes (option = 6)
            public double max_principalStress1;
            public double min_principalStress1;

            // Principal stress 2 extremes (option = 7)
            public double max_principalStress2;
            public double min_principalStress2;

            // Max shear stress extremes (option = 8)
            public double max_shearStress;
            public double min_shearStress;

            // PSL Lines (option = 9)

        }



        private Dictionary<int, point_store> points = new Dictionary<int, point_store>();
        private List<line_store> wireframe_lines = new List<line_store>();
        private List<tri_store> tris = new List<tri_store>();
        private List<reaction_store> reactions = new List<reaction_store>();


        public result_extremes rslt_extremes { get { return _rslt_extremes; } }
        private result_extremes _rslt_extremes;

        // public bool isResultSet = false;


        private Shader rsltmeshShader;
        private Shader rsltmeshwireframeShader;
        private Shader rsltPSLShader;
        // private Shader rsltPSLType2Shader;
        private Shader rsltPSLType2StreamFunctionShader;

        // Vertex Buffer object and Vertex Array object 
        private VertexBuffer point_vbo;
        private VertexArray point_vao;


        // PSL lines vertex buffer and vertex array object
        private VertexBuffer psl_point_vbo;
        private VertexArray psl_point_vao;

        //// PSL lines vertex buffer and vertex array object
        //private VertexBuffer psl2_point_vbo;
        //private VertexArray psl2_point_vao;
        //private IndexBuffer psl2_lines_ibo;

        private VertexBuffer psl2_streamfunction_point_vbo;
        private VertexArray psl2_streamfunction_point_vao;
        private IndexBuffer psl2_streamfunction_tri_ibo;




        // Index buffer for the points, wireframe lines, and triangles (EBO)
        private IndexBuffer point_ibo;
        private IndexBuffer wireframe_ibo;
        private IndexBuffer triangle_ibo;

        // Result point selection index buffer for selected points
        private IndexBuffer selected_resultpoint_ibo;

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

            rsltPSLShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.RsltPSLShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.RsltPSLShader)
                );

            //rsltPSLType2Shader = new Shader(
            //    ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.RsltPSLType2Shader),
            //    ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.RsltPSLType2Shader)
            //    );

            rsltPSLType2StreamFunctionShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.RsltPSLType2StreamFunctionShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.RsltPSLType2StreamFunctionShader)
                );

        }



        public void add_point(int point_id, double x_coord, double y_coord,
            double displ_x, double displ_y,
            int constraint_type,
            double constraint_angle,
            double reaction_x, double reaction_y,
            double sigma_x, double sigma_y,
            double tau_xy,
            double sigma_1, double sigma_2,
            double von_mises,
            double max_shear,
            double theta_p,
            double streamfunction_tension,
            double streamfunction_compression)
        {

            if (constraint_type != 0)
            {
                reactions.Add(new reaction_store()
                {
                    point_id = point_id,
                    x_coord = x_coord,
                    y_coord = y_coord,
                    constraint_type = constraint_type,
                    constraint_angle = constraint_angle,
                    reaction_x = reaction_x,
                    reaction_y = reaction_y
                });
            }

            double displ_magnitude = Math.Sqrt(displ_x * displ_x + displ_y * displ_y);

            points.Add(point_id, new point_store()
            {
                point_id = point_id,
                x_coord = x_coord,
                y_coord = y_coord,
                displ_x = displ_x,
                displ_y = displ_y,
                displ_magnitude = displ_magnitude,
                sigma_x = sigma_x,
                sigma_y = sigma_y,
                tau_xy = tau_xy,
                sigma_1 = sigma_1,
                sigma_2 = sigma_2,
                von_mises = von_mises,
                max_shear = max_shear,
                theta_p = theta_p,
                streamfunction_tension = streamfunction_tension,
                streamfunction_compression = streamfunction_compression
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
            _rslt_extremes.max_displacement = 0.0;
            _rslt_extremes.max_stressX = double.MinValue;
            _rslt_extremes.min_stressX = double.MaxValue;
            _rslt_extremes.max_stressY = double.MinValue;
            _rslt_extremes.min_stressY = double.MaxValue;
            _rslt_extremes.max_tauXY = double.MinValue;
            _rslt_extremes.min_tauXY = double.MaxValue;
            _rslt_extremes.max_principalStress1 = double.MinValue;
            _rslt_extremes.min_principalStress1 = double.MaxValue;
            _rslt_extremes.max_principalStress2 = double.MinValue;
            _rslt_extremes.min_principalStress2 = double.MaxValue;
            _rslt_extremes.max_vonMises = double.MinValue;
            _rslt_extremes.min_vonMises = double.MaxValue;
            _rslt_extremes.max_shearStress = double.MinValue;
            _rslt_extremes.min_shearStress = double.MaxValue;



            foreach (var pt in points.Values)
            {
                // Maximum displacement magnitude
                _rslt_extremes.max_displacement = Math.Max(_rslt_extremes.max_displacement, pt.displ_magnitude);

                // Maximum and minimum stress in X and Y directions
                _rslt_extremes.max_stressX = Math.Max(_rslt_extremes.max_stressX, pt.sigma_x);
                _rslt_extremes.min_stressX = Math.Min(_rslt_extremes.min_stressX, pt.sigma_x);
                _rslt_extremes.max_stressY = Math.Max(_rslt_extremes.max_stressY, pt.sigma_y);
                _rslt_extremes.min_stressY = Math.Min(_rslt_extremes.min_stressY, pt.sigma_y);

                // Maximum and minimum shear stress
                _rslt_extremes.max_tauXY = Math.Max(_rslt_extremes.max_tauXY, pt.tau_xy);
                _rslt_extremes.min_tauXY = Math.Min(_rslt_extremes.min_tauXY, pt.tau_xy);

                // Maximum and minimum principal stresses
                _rslt_extremes.max_principalStress1 = Math.Max(_rslt_extremes.max_principalStress1, pt.sigma_1);
                _rslt_extremes.min_principalStress1 = Math.Min(_rslt_extremes.min_principalStress1, pt.sigma_1);
                _rslt_extremes.max_principalStress2 = Math.Max(_rslt_extremes.max_principalStress2, pt.sigma_2);
                _rslt_extremes.min_principalStress2 = Math.Min(_rslt_extremes.min_principalStress2, pt.sigma_2);

                // Maximum and minimum von Mises stress
                _rslt_extremes.max_vonMises = Math.Max(_rslt_extremes.max_vonMises, pt.von_mises);
                _rslt_extremes.min_vonMises = Math.Min(_rslt_extremes.min_vonMises, pt.von_mises);

                // Maximum and minimum shear stress
                _rslt_extremes.max_shearStress = Math.Max(_rslt_extremes.max_shearStress, pt.max_shear);
                _rslt_extremes.min_shearStress = Math.Min(_rslt_extremes.min_shearStress, pt.max_shear);

            }


            // Validate the result extremes to ensure they are meaningful
            if (_rslt_extremes.max_displacement <= 0 || !check_double(_rslt_extremes.max_displacement))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_stressX) || !check_double(_rslt_extremes.min_stressX))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_stressY) || !check_double(_rslt_extremes.min_stressY))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_tauXY) || !check_double(_rslt_extremes.min_tauXY))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_vonMises) || !check_double(_rslt_extremes.min_vonMises))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_principalStress1) || !check_double(_rslt_extremes.min_principalStress1))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_principalStress2) || !check_double(_rslt_extremes.min_principalStress2))
            {
                return false;
            }
            if (!check_double(_rslt_extremes.max_shearStress) || !check_double(_rslt_extremes.min_shearStress))
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
            if (gvariables_static.result_option == 9 || gvariables_static.result_option == 10)
            {
                // Special case for PSL lines (option = 9 or 10)
                paint_PSL_lines();
                return;
            }


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


        private void paint_selected_result_points()
        {
            if (!buffersInitialized)
                return;

            if (selected_resultpoint_ibo.BufferCount > 0)
            {
                // Paint the selected result points
                rsltmeshShader.Bind();

                point_vao.Bind();
                selected_resultpoint_ibo.Bind();

                GL.PointSize(5.0f);
                GL.DrawElements(PrimitiveType.Points, selected_resultpoint_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);
                GL.PointSize(1.0f);

                rsltmeshShader.UnBind();
                point_vao.UnBind();
                selected_resultpoint_ibo.UnBind();


                // Paint the labels for the selected result points
                result_point_label.paint_static_labels();

            }
        }


        private void paint_PSL_lines()
        {
            if (!gvariables_static.is_paint_resultmesh || !buffersInitialized)
                return;


            int option = gvariables_static.result_option;

            if (option == 9)
            {

                rsltPSLShader.Bind();
                psl_point_vao.Bind();

                if (triangle_ibo.BufferCount > 0)
                {

                    // Paint the Result triangle mesh
                    triangle_ibo.Bind();
                    GL.DrawElements(PrimitiveType.Triangles, triangle_ibo.BufferCount,
                        DrawElementsType.UnsignedInt, 0);
                    triangle_ibo.UnBind();
                }

                psl_point_vao.UnBind();
                rsltPSLShader.UnBind();

            }
            else if (option == 10)
            {

                //rsltPSLType2Shader.Bind();
                //psl2_point_vao.Bind();
                //if (psl2_lines_ibo.BufferCount > 0)
                //{
                //    // Paint the Result Stress lines
                //    psl2_lines_ibo.Bind();
                //    GL.DrawElements(PrimitiveType.Lines, psl2_lines_ibo.BufferCount,
                //        DrawElementsType.UnsignedInt, 0);
                //    psl2_lines_ibo.UnBind();
                //}
                //psl2_point_vao.UnBind();
                //rsltPSLType2Shader.UnBind();


                rsltPSLType2StreamFunctionShader.Bind();
                psl2_streamfunction_point_vao.Bind();
                if (psl2_streamfunction_tri_ibo.BufferCount > 0)
                {
                    // Paint the Result Plane Stress lines triangles
                    psl2_streamfunction_tri_ibo.Bind();
                    GL.DrawElements(PrimitiveType.Triangles, psl2_streamfunction_tri_ibo.BufferCount,
                        DrawElementsType.UnsignedInt, 0);
                    psl2_streamfunction_tri_ibo.UnBind();
                }
                psl2_streamfunction_point_vao.UnBind();
                rsltPSLType2StreamFunctionShader.UnBind();



            }

        }



        public void switch_result_option()
        {
            // Switch the result option for visualization
            // 1 = Displacement, 2 = StressX, 3 = StressY, 4 = Shear stress, 
            // 5 = Von Mises stress, 6 = Principal stress 1, 7 = Principal stress 2, 
            // 8 = Max shear stress

            int option = gvariables_static.result_option;

            // Special case for PSL lines (option = 9 or 10)
            if (option == 9 || option == 10)
            {

                return;
            }


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


            // Update the result point labels for the selected points
            if (selected_resultpoint_ids.Count > 0)
            {
                add_selected_result_point_labels();
            }

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
                case 1: // Displacement
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = 0.0f + ((float)_rslt_extremes.max_displacement - 0.0f) * zoomMin;
                        float actualRangeMax = 0.0f + ((float)_rslt_extremes.max_displacement - 0.0f) * zoomMax;
                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.displ_magnitude - actualRangeMin) / actualRangeSpan;

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

                        return normalizedValue;

                    }
                case 2: // StressX
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_stressX +
                            ((_rslt_extremes.max_stressX - _rslt_extremes.min_stressX) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_stressX +
                            ((_rslt_extremes.max_stressX - _rslt_extremes.min_stressX) * zoomMax));

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
                case 3: // StressY
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_stressY +
                            ((_rslt_extremes.max_stressY - _rslt_extremes.min_stressY) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_stressY +
                            ((_rslt_extremes.max_stressY - _rslt_extremes.min_stressY) * zoomMax));

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
                case 4: // Shear stress
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_tauXY +
                            ((_rslt_extremes.max_tauXY - _rslt_extremes.min_tauXY) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_tauXY +
                            ((_rslt_extremes.max_tauXY - _rslt_extremes.min_tauXY) * zoomMax));

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
                case 5: // Von Mises stress
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_vonMises +
                            ((_rslt_extremes.max_vonMises - _rslt_extremes.min_vonMises) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_vonMises +
                            ((_rslt_extremes.max_vonMises - _rslt_extremes.min_vonMises) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.von_mises - actualRangeMin) / actualRangeSpan;

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
                case 6: // Principal stress 1
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_principalStress1 +
                            ((_rslt_extremes.max_principalStress1 - _rslt_extremes.min_principalStress1) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_principalStress1 +
                            ((_rslt_extremes.max_principalStress1 - _rslt_extremes.min_principalStress1) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.sigma_1 - actualRangeMin) / actualRangeSpan;

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
                case 7: // Principal stress 2
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_principalStress2 +
                            ((_rslt_extremes.max_principalStress2 - _rslt_extremes.min_principalStress2) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_principalStress2 +
                            ((_rslt_extremes.max_principalStress2 - _rslt_extremes.min_principalStress2) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.sigma_2 - actualRangeMin) / actualRangeSpan;

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
                case 8: // Max shear stress
                    {
                        // Calculate the actual values at zoom boundaries
                        float actualRangeMin = (float)(_rslt_extremes.min_shearStress +
                            ((_rslt_extremes.max_shearStress - _rslt_extremes.min_shearStress) * zoomMin));

                        float actualRangeMax = (float)(_rslt_extremes.min_shearStress +
                            ((_rslt_extremes.max_shearStress - _rslt_extremes.min_shearStress) * zoomMax));

                        float actualRangeSpan = actualRangeMax - actualRangeMin;

                        float normalizedValue = ((float)pt.max_shear - actualRangeMin) / actualRangeSpan;

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
                    //case 9: // PSL Lines
                    //    {
                    //        // For PSL lines, we can return a default value or handle it differently
                    //        float pi2_value = (float)Math.PI * 0.5f;


                    //        float c_value = (float)Math.Atan2(pt.tau_xy, pt.sigma_x - pt.sigma_y) / 2.0f;

                    //        float actualRangeMin = (float)(-pi2_value +
                    //          ((pi2_value + pi2_value) * zoomMin));

                    //        float actualRangeMax = (float)(-pi2_value + +
                    //            ((pi2_value + pi2_value) * zoomMax));

                    //        float actualRangeSpan = actualRangeMax - actualRangeMin;

                    //        float normalizedValue = ((float)c_value - actualRangeMin) / actualRangeSpan;

                    //        if (normalizedValue < -EPSILON)
                    //        {
                    //            normalizedValue = -1.0f;
                    //        }
                    //        else if (normalizedValue > 1.0f + EPSILON)
                    //        {
                    //            normalizedValue = 2.0f;
                    //        }
                    //        else
                    //        {
                    //            // Clamp the normalized value to [0, 1] range
                    //            normalizedValue = Math.Max(0.0f, Math.Min(1.0f, normalizedValue));
                    //        }

                    //        normalizedValue = (normalizedValue * 2.0f) - 1.0f; // Scale to [-1, 1]

                    //        return normalizedValue;
                    //    }
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


        private void generate_PSL_mesh()
        {
            // Generate PSL mesh data based on the points and triangles
            // This function should create the necessary vertex and index buffers for PSL visualization
            // Implementation depends on how PSL data is structured and visualized

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


                // Calculate the magnitude of the displacement vector for color mapping
                vertexData.Add((float)(pt.displ_magnitude / _rslt_extremes.max_displacement)); // normalized scalar value

                // Sigma XX stress in X direction
                float sigmaX_actualRangeSpan = (float)(_rslt_extremes.max_stressX - _rslt_extremes.min_stressX);

                float aSigmaX = ((float)pt.sigma_x - (float)(_rslt_extremes.min_stressX)) / sigmaX_actualRangeSpan;


                // Sigma YY stress in Y direction
                float sigmaY_actualRangeSpan = (float)(_rslt_extremes.max_stressY - _rslt_extremes.min_stressY);

                float aSigmaY = ((float)pt.sigma_y - (float)(_rslt_extremes.min_stressY)) / sigmaY_actualRangeSpan;

                // Tau XY shear stress in XY direction





                // Principal stress angle for PSL lines
                float aPrincipalStressAngle = (float)Math.Atan2(2.0 * pt.tau_xy, pt.sigma_x - pt.sigma_y) / 2.0f;

                // Principal stresses 1
                float aPrincipalStress_sigma1 = (float)((pt.sigma_x + pt.sigma_y) / 2.0f +
                                Math.Sqrt(Math.Pow((pt.sigma_x - pt.sigma_y) / 2.0f, 2) +
                                Math.Pow(pt.tau_xy, 2)));

                float sigma1_actualRangeSpan = (float)(_rslt_extremes.max_principalStress1 - _rslt_extremes.min_principalStress1);

                // Normalize principal stress 1 (pt.sigma_1)
                aPrincipalStress_sigma1 = ((float)pt.sigma_1 - (float)(_rslt_extremes.min_principalStress1)) / sigma1_actualRangeSpan;


                // Principal stress 2
                float aPrincipalStress_sigma2 = (float)((pt.sigma_x + pt.sigma_y) / 2.0f -
                                Math.Sqrt(Math.Pow((pt.sigma_x - pt.sigma_y) / 2.0f, 2) +
                                Math.Pow(pt.tau_xy, 2)));

                float sigma2_actualRangeSpan = (float)(_rslt_extremes.max_principalStress2 - _rslt_extremes.min_principalStress2);

                // Normalize principal stress 2 (pt.sigma_2)
                aPrincipalStress_sigma2 = ((float)pt.sigma_2 - (float)(_rslt_extremes.min_principalStress2)) / sigma2_actualRangeSpan;


                // vertexData.Add(aPrincipalStressAngle); // Principal stress angle for PSL lines
                vertexData.Add(aPrincipalStress_sigma1); // Principal stress 1
                vertexData.Add(aPrincipalStress_sigma2); // Principal stress 2

                // // Line length (scale with stress magnitude or fixed)
                // float lineLength = 0.5f; // or scale with magnitude

                // Principal direction 1 (major principal stress)
                Vector2 dir1 = new Vector2((float)Math.Cos(aPrincipalStressAngle), (float)Math.Sin(aPrincipalStressAngle));
                Vector2 dir2 = new Vector2((float)Math.Cos(aPrincipalStressAngle + Math.PI / 2),
                                           (float)Math.Sin(aPrincipalStressAngle + Math.PI / 2));

                dir1 = Vector2.Normalize(dir1);
                dir2 = Vector2.Normalize(dir2);

                vertexData.Add((float)dir1.X);
                vertexData.Add((float)dir1.Y);


                vertexData.Add((float)dir2.X);
                vertexData.Add((float)dir2.Y);


            }




            // Create VAO and VBO for points
            psl_point_vao = new VertexArray();
            psl_point_vbo = new VertexBuffer(Math.Max(10, vertexData.Count));


            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(2);  // x and y coordinates
            pointLayout.AddFloat(2); // displ_x and displ_y
            pointLayout.AddFloat(1); // displacement magnitude
            pointLayout.AddFloat(1); // sigma1 principal stress value 1
            pointLayout.AddFloat(1); // sigma2 principal stress value 2
            pointLayout.AddFloat(2); // direction1 of principal stress 1
            pointLayout.AddFloat(2); // direction2 of principal stress 2


            psl_point_vao.Add_vertexBuffer(psl_point_vbo, pointLayout);

            psl_point_vbo.AppendVertexBuffer(vertexData.ToArray());

        }




        private void get_PSL_streamfunction_mesh()
        {

            //____________________________________________________________________________________________________

            List<float> vertexData = new List<float>();
            List<int> triIndexData = new List<int>();

            // Create a node id map to index mapping for the streamfunction mesh
            Dictionary<int, int> nodeIdToIndexMap = new Dictionary<int, int>();

            int pt_id = 0;

            foreach (point_store pt in points.Values)
            {
                // Map the point ID to the current index
                nodeIdToIndexMap[pt.point_id] = pt_id;
                pt_id++;

                // Add vertices for stream function mesh
                vertexData.Add((float)pt.x_coord);
                vertexData.Add((float)pt.y_coord);
                vertexData.Add((float)pt.streamfunction_tension);
                vertexData.Add((float)pt.streamfunction_compression);

            }


            // Add triangle indices for the streamfunction mesh
            foreach (tri_store tri in tris)
            {
                // Map the point IDs to their corresponding indices
                int index1 = nodeIdToIndexMap[tri.pt_id1];
                int index2 = nodeIdToIndexMap[tri.pt_id2];
                int index3 = nodeIdToIndexMap[tri.pt_id3];

                triIndexData.Add(index1);
                triIndexData.Add(index2);
                triIndexData.Add(index3);
            }


            //____________________________________________________________________________________________________
            // Create VAO and VBO for points
            psl2_streamfunction_point_vao = new VertexArray();
            psl2_streamfunction_point_vbo = new VertexBuffer(Math.Max(10, vertexData.Count));
            psl2_streamfunction_tri_ibo = new IndexBuffer(Math.Max(10, triIndexData.Count));

            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(2);  // x and y coordinates
            pointLayout.AddFloat(1); // Streamfunction value for tension
            pointLayout.AddFloat(1); // Streamfunction value for compression


            psl2_streamfunction_point_vao.Add_vertexBuffer(psl2_streamfunction_point_vbo, pointLayout);

            psl2_streamfunction_point_vbo.AppendVertexBuffer(vertexData.ToArray());
            psl2_streamfunction_tri_ibo.AppendIndexBuffer(triIndexData.ToArray());

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


            //____________________________________________________________________________________________
            // Update the PSL shader uniforms
            rsltPSLShader.SetMatrix4("uMVP", uMVP);
            rsltPSLShader.SetFloat("geomscale", gvariables_static.geom_size);

            rsltPSLShader.SetFloat("modelpercent", model_percent);

            //rsltPSLShader.SetFloat("vertexTransparency", gvariables_static.rslt_transparency);

            // //____________________________________________________________________________________________
            // // Update the PSL shader uniforms type 2
            // rsltPSLType2Shader.SetMatrix4("uMVP", uMVP);
            // rsltPSLType2Shader.SetFloat("geomscale", gvariables_static.geom_size);

            // rsltPSLType2Shader.SetFloat("modelpercent", model_percent);

            // Update the PSL shader uniforms type 2 streamfunction
            rsltPSLType2StreamFunctionShader.SetMatrix4("uMVP", uMVP);
            rsltPSLType2StreamFunctionShader.SetFloat("uNumContours", gvariables_static.contourline_level);
            rsltPSLType2StreamFunctionShader.SetFloat("uLineOpacity", 1.0f);
            rsltPSLType2StreamFunctionShader.SetFloat("uLineWidth", 1.2f);

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

            rsltPSLShader.SetFloat("sinevalue", sine_oscillation);
            // rsltPSLType2Shader.SetFloat("sinevalue", sine_oscillation);
            // rsltPSLShader.SetFloat("uLineOpacity", 1.0f);


            if (sine_oscillation < 0.1)
            {
                rsltmeshShader.SetFloat("uLineOpacity", 0.0f);

                // rsltPSLShader.SetFloat("uLineOpacity", 0.0f);
            }

        }



        public void select_result_nodes(Vector2 corner_pt1, Vector2 corner_pt2, bool isRightButton, drawing_events graphic_events_control)
        {
            // Select the result nodes for load or constraint update
            List<int> selected_result_point_ids = new List<int>();

            // Pre-compute MVP matrix
            Matrix4 mvp = graphic_events_control.projectionMatrix *
                          graphic_events_control.viewMatrix *
                          graphic_events_control.modelMatrix;


            Matrix4 invMVP = Matrix4.Invert(mvp);

            // Transform rectangle corners from screen space to model space
            Vector2 modelCorner1 = TransformToModelSpace(corner_pt1, invMVP);
            Vector2 modelCorner2 = TransformToModelSpace(corner_pt2, invMVP);

            // Loop through all node in nodeMap
            foreach (point_store pt in points.Values)
            {
                //______________________________
                Vector2 pt_coord = new Vector2((float)pt.x_coord, (float)pt.y_coord);

                Vector2 aDisplacement = new Vector2((float)(pt.displ_x / pt.displ_magnitude),
                    (float)(pt.displ_y / pt.displ_magnitude));


                float model_percent = (float)(gvariables_static.displacement_scale / 1000.0);
                float aDisplacementMagnitude = (float)(pt.displ_magnitude / _rslt_extremes.max_displacement);

                float scalevalue = gvariables_static.geom_size * model_percent * aDisplacementMagnitude;
                Vector2 scaledDisplacement = aDisplacement * scalevalue; // * sinevalue; sinevalue is not used here, as it's for animation

                // Find the displaced point location in model space
                Vector2 displaced_pt_loc = pt_coord + scaledDisplacement;

                // Check whether the point inside a rectangle
                if (gvariables_static.isPointSelected(modelCorner1, modelCorner2, displaced_pt_loc) == true)
                {
                    selected_result_point_ids.Add(pt.point_id);

                }

            }

            if (selected_result_point_ids.Count > 0)
            {
                add_selected_result_points(selected_result_point_ids, isRightButton);
            }

        }


        private void add_selected_result_points(List<int> selected_result_point_ids, bool IsRemove)
        {
            bool is_selection_changed = false;

            if (IsRemove == false)
            {
                // Add to the selected result point list
                // Add all points at once
                int initialCount = this.selected_resultpoint_ids.Count;
                this.selected_resultpoint_ids.UnionWith(selected_result_point_ids);
                is_selection_changed = this.selected_resultpoint_ids.Count != initialCount;
            }
            else
            {
                // Remove from the selected result point list
                // Remove all points at once
                int initialCount = this.selected_resultpoint_ids.Count;
                this.selected_resultpoint_ids.ExceptWith(selected_result_point_ids);
                is_selection_changed = this.selected_resultpoint_ids.Count != initialCount;
            }


            if (is_selection_changed == true)
            {
                // Add the selected result points
                selected_resultpoint_ibo.ClearIndexBuffer();
                selected_resultpoint_ibo.AppendIndexBuffer(this.selected_resultpoint_ids.ToArray());

                add_selected_result_point_labels();

            }
            //
        }


        public void clear_selected_result_points()
        {
            this.selected_resultpoint_ids.Clear();
            selected_resultpoint_ibo.ClearIndexBuffer();
            result_point_label.clear_labels();

        }


        public List<string> get_selected_result_points_string()
        {
            // return the selected result points as a list of strings for data grid view display
            List<string> resultPoints = new List<string>();

            foreach (int point_id in this.selected_resultpoint_ids)
            {
                point_store rslt_pt = points[point_id];

                double displ_magnitude = rslt_pt.displ_magnitude;
                double sigma_x = rslt_pt.sigma_x;
                double sigma_y = rslt_pt.sigma_y;
                double tau_xy = rslt_pt.tau_xy;
                double principal_1 = rslt_pt.sigma_1;
                double principal_2 = rslt_pt.sigma_2;
                double von_mises = rslt_pt.von_mises;
                double max_shear = rslt_pt.max_shear;

                resultPoints.Add($"{point_id} , {displ_magnitude} , " +
                    $"{sigma_x} , {sigma_y} , {tau_xy} , " +
                    $"{principal_1} , {principal_2} , " +
                    $"{von_mises} , {max_shear}");
            }

            return resultPoints;

        }


        private void add_selected_result_point_labels()
        {
            // Add labels for the selected result points
            result_point_label.clear_labels();
            int label_id = 0;

            foreach (int point_id in this.selected_resultpoint_ids)
            {

                // Create the result label
                point_store rslt_pt = points[point_id];

                //_______________________________________________________________________________________________________________
                // result label location
                Vector2 pt_coord = new Vector2((float)rslt_pt.x_coord, (float)rslt_pt.y_coord);

                Vector2 aDisplacement = new Vector2((float)(rslt_pt.displ_x / rslt_pt.displ_magnitude),
                    (float)(rslt_pt.displ_y / rslt_pt.displ_magnitude));


                float model_percent = (float)(gvariables_static.displacement_scale / 1000.0);
                float aDisplacementMagnitude = (float)(rslt_pt.displ_magnitude / _rslt_extremes.max_displacement);

                float scalevalue = gvariables_static.geom_size * model_percent * aDisplacementMagnitude;
                Vector2 scaledDisplacement = aDisplacement * scalevalue; // * sinevalue; sinevalue is not used here, as it's for animation

                // Find the displaced point location in model space
                Vector2 displaced_pt_loc = pt_coord + scaledDisplacement;
                //_______________________________________________________________________________________________________________

                int option = gvariables_static.result_option;
                float colorValue = scaled_contourColorValue(rslt_pt, option);
                float labelValue = (float)rslt_pt.displ_magnitude;

                switch (option)
                {
                    case 1:
                        labelValue = (float)rslt_pt.displ_magnitude;
                        break;
                    case 2:
                        labelValue = (float)rslt_pt.sigma_x;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                    case 3:
                        labelValue = (float)rslt_pt.sigma_y;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                    case 4:
                        labelValue = (float)rslt_pt.tau_xy;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                    case 5:
                        labelValue = (float)rslt_pt.von_mises;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                    case 6:
                        labelValue = (float)rslt_pt.sigma_1;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                    case 7:
                        labelValue = (float)rslt_pt.sigma_2;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                    case 8:
                        labelValue = (float)rslt_pt.max_shear;
                        colorValue = (colorValue + 1.0f) * 0.5f; // Adjust color value for stressX to be in [0, 1] range
                        break;
                }

                Vector3 LabelColor = gvariables_static.GetJetColorClamped(colorValue);
                string rsltlabel_string = FormatResultLabelValue(labelValue);


                result_point_label.add_label(label_id + 0, rsltlabel_string, displaced_pt_loc, LabelColor);

                label_id++;

            }

            // Update the label buffer
            result_point_label.update_buffer(gvariables_static.geom_size * 0.5f);


        }



        private string FormatResultLabelValue(float value)
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



        // Helper method to transform screen point to model space
        private Vector2 TransformToModelSpace(Vector2 screenPoint, Matrix4 invMVP)
        {
            // Convert to homogeneous coordinates
            Vector4 clipPoint = new Vector4(screenPoint.X, screenPoint.Y, 0.0f, 1.0f);

            // Transform to model space
            Vector4 modelPoint = invMVP * clipPoint;

            // Perspective division (if using perspective projection)
            if (Math.Abs(modelPoint.W) > float.Epsilon)
            {
                modelPoint.X /= modelPoint.W;
                modelPoint.Y /= modelPoint.W;
            }

            return new Vector2(modelPoint.X, modelPoint.Y);
        }


        public void Dispose()
        {
            point_vbo?.Dispose();
            point_vao?.Dispose();
            point_ibo?.Dispose();
            psl_point_vao?.Dispose();
            psl_point_vbo?.Dispose();
            selected_resultpoint_ibo?.Dispose();
            wireframe_ibo?.Dispose();
            triangle_ibo?.Dispose();
            // meshShader?.Dispose();

        }


        //______________________________



    }
}
