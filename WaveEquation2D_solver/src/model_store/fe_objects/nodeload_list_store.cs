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

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace WaveEquation2D_solver.src.model_store.fe_objects
{

    public class nodeload_data
    {
        public int load_set_id { get; set; }

        public List<Vector2> load_node_pts { get; set; }

        public List<int> load_node_ids { get; set; }

        public double load_amplitude { get; set; }

        public double load_angle { get; set; }

    }




    public class nodeload_list_store
    {


        public Dictionary<int, nodeload_data> loadMap = new Dictionary<int, nodeload_data>();
        public int load_set_count = 0;

        private List<int> all_loadset_ids = new List<int>();

        // Load labels
        private label_list_store load_label;

        // Load visualization
        private Shader loadShader;

        // Vertex Buffer object and Vertex Array object 
        private VertexBuffer load_vbo;
        private VertexArray load_vao;
        private IndexBuffer load_ibo;


        public nodeload_list_store()
        {
            // (Re)Initialize the data
            loadMap = new Dictionary<int, nodeload_data>();
            load_set_count = 0;

            InitializeShader();
            InitializeBuffers();

            load_label = new label_list_store();

        }


        private void InitializeShader()
        {
            // Initialize the Shader 
            loadShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.LoadShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.LoadShader)
                );

        }


        private void InitializeBuffers()
        {
            // Initialize the Buffer
            load_vao = new VertexArray();
            load_vbo = new VertexBuffer(10);
            load_ibo = new IndexBuffer(10);

            VertexBufferLayout loadLayout = new VertexBufferLayout();
            loadLayout.AddFloat(2);
            loadLayout.AddFloat(2);

            load_vao.Add_vertexBuffer(load_vbo, loadLayout);

        }


        public void add_loads(List<int> load_node_ids, List<Vector2> load_node_pts,
            double t_load_amplitude, double t_load_angle)
        {
            // Get an unique load set id
            int unique_loadset_id = gvariables_static.get_unique_id(all_loadset_ids);

            // Make a copy of the list
            List<int> idsCopy = new List<int>(load_node_ids);
            List<Vector2> nodePtsCopy = new List<Vector2>(load_node_pts);


            // Add the Load to the list
            nodeload_data temp_load = new nodeload_data
            {
                load_set_id = unique_loadset_id,
                load_node_ids = idsCopy,
                load_node_pts = nodePtsCopy,
                load_amplitude = t_load_amplitude,
                load_angle = t_load_angle
            };


            loadMap[unique_loadset_id] = temp_load;
            load_set_count++;

            // Update the load data visualization
            update_buffer_data();

            // Add the load set id to list to track the unique load set id
            all_loadset_ids.Add(unique_loadset_id);

        }




        public void delete_nodeload(int load_set_id)
        {
            // Remove the load set ID from all_loadset_ids
            all_loadset_ids.Remove(load_set_id);

            // Remove the load data based on the key (load set id)
            loadMap.Remove(load_set_id);

            // adjust the load data count
            load_set_count--;

            // Update the load data visualization
            update_buffer_data();

        }


        public void paint_node_load()
        {
            // node load count check
            if (load_set_count == 0 || gvariables_static.is_paint_loads == false)
                return;

            loadShader.Bind();

            load_vao.Bind();
            load_ibo.Bind();

            // Paint the Load Line
            GL.LineWidth(3.0f);
            GL.DrawElements(PrimitiveType.Lines, load_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);
            GL.LineWidth(1.0f);

            load_vao.UnBind();
            load_ibo.UnBind();

            loadShader.UnBind();

            // Paint the load label
            load_label.paint_static_labels();

        }



        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            if (load_set_count == 0)
                return;

            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                                     graphic_events_control.viewMatrix *
                                     graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;

            loadShader.SetMatrix4("uMVP", uMVP);
            loadShader.SetFloat("zoomscale", zoomscale);

            Vector4 LoadColor = new Vector4(gvariables_static.ColorUtils.get_LoadColor(),
        gvariables_static.geom_transparency * 0.8f);


            loadShader.SetVector4("vertexColor", LoadColor);

            // Update the label uniforms
            load_label.update_openTK_uniforms(uMVP, zoomscale, gvariables_static.geom_transparency);

        }


        private void update_buffer_data()
        {
            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> loadVertexData = new List<float>();
            List<int> loadIndexData = new List<int>();

            // Get the load size
            float load_size = gvariables_static.get_font_scale(8.0f);


            // Get the load max
            float load_max = 0.0f;

            foreach (nodeload_data load_data in loadMap.Values)
            {
                load_max = (float)Math.Max(load_max, Math.Abs(load_data.load_amplitude));
            }


            int t_id = 0;
            int label_id = 0;

            load_label.clear_labels();

            foreach (nodeload_data load_data in loadMap.Values)
            {

                int load_sign = load_data.load_amplitude > 0 ? 1 : -1;
                float ld_visualization_factor = load_sign * load_size;
                float ld_scale = (float)(load_data.load_amplitude / load_max) * load_size;

                float arrowLength = -20.0f * ld_scale;
                float arrowheadSize = -4.0f * ld_visualization_factor;
                float arrowheadAngle = 10.0f;


                Vector2 startpt = gvariables_static.RotatePoint(new Vector2(0, 0), new Vector2(-0.1f * ld_visualization_factor, 0.0f), load_data.load_angle);
                Vector2 tailpt = gvariables_static.RotatePoint(new Vector2(0, 0), new Vector2(arrowLength, 0.0f), load_data.load_angle);
                Vector2 arrowheadpt1 = gvariables_static.RotatePoint(new Vector2(0, 0), new Vector2(arrowheadSize, 0.0f), load_data.load_angle + arrowheadAngle);
                Vector2 arrowheadpt2 = gvariables_static.RotatePoint(new Vector2(0, 0), new Vector2(arrowheadSize, 0.0f), load_data.load_angle - arrowheadAngle);


                foreach (Vector2 load_node_pt in load_data.load_node_pts)
                {

                    // Load arrow start point
                    loadVertexData.Add(load_node_pt.X + startpt.X);
                    loadVertexData.Add(load_node_pt.Y + startpt.Y);
                    loadVertexData.Add(load_node_pt.X);
                    loadVertexData.Add(load_node_pt.Y);


                    // Load arrow tail point
                    loadVertexData.Add(load_node_pt.X + tailpt.X);
                    loadVertexData.Add(load_node_pt.Y + tailpt.Y);
                    loadVertexData.Add(load_node_pt.X);
                    loadVertexData.Add(load_node_pt.Y);


                    // Load arrow pt 1
                    loadVertexData.Add(load_node_pt.X + arrowheadpt1.X);
                    loadVertexData.Add(load_node_pt.Y + arrowheadpt1.Y);
                    loadVertexData.Add(load_node_pt.X);
                    loadVertexData.Add(load_node_pt.Y);


                    // Load arrow pt 2
                    loadVertexData.Add(load_node_pt.X + arrowheadpt2.X);
                    loadVertexData.Add(load_node_pt.Y + arrowheadpt2.Y);
                    loadVertexData.Add(load_node_pt.X);
                    loadVertexData.Add(load_node_pt.Y);


                    // Set the node indices
                    // Load Line 1
                    loadIndexData.Add(t_id + 0);
                    loadIndexData.Add(t_id + 1);

                    // Arrow head line 1
                    loadIndexData.Add(t_id + 0);
                    loadIndexData.Add(t_id + 2);

                    // Arrow head line 2
                    loadIndexData.Add(t_id + 0);
                    loadIndexData.Add(t_id + 3);

                    t_id = t_id + 4;

                }


                // Create the load label
                // Add labels
                int mid_index = load_data.load_node_pts.Count / 2;
                string label_string1 = $"Load Set {load_data.load_set_id}";
                // string label_string2 = $"Amplitude = {load_data.load_amplitude}";

                // float label_ht = gvariables_static.get_text_height(12.0f) * 0.0125f;

                Vector2 label_loc1 = new Vector2(load_data.load_node_pts[mid_index].X,
                        load_data.load_node_pts[mid_index].Y);
                //Vector2 label_loc2 = new Vector2(load_data.load_node_pts[mid_index].X,
                //        load_data.load_node_pts[mid_index].Y - label_ht);

                load_label.add_label(label_id + 0, label_string1, label_loc1, gvariables_static.ColorUtils.get_LoadColor());
                // load_label.add_label(label_id + 1, label_string2, label_loc2, gvariables_static.ColorUtils.get_LoadColor());

                // label_id = label_id + 2;
                label_id++;

            }

            // Update the label buffer
            load_label.update_buffer(gvariables_static.geom_size);


            // Clear and update buffers
            if (loadVertexData.Count > 0)
            {
                // Convert to array and upload
                float[] vertexArray = loadVertexData.ToArray();
                int[] indexArray = loadIndexData.ToArray();

                // Clear existing data
                load_vbo.ClearVertexBuffer();
                load_ibo.ClearIndexBuffer();

                // Upload new data
                load_vbo.AppendVertexBuffer(vertexArray);
                load_ibo.AppendIndexBuffer(indexArray);

            }
            else
            {

                // Clear buffers if no data
                load_vbo.ClearVertexBuffer();
                load_ibo.ClearIndexBuffer();

            }

        }

        //_______________________________________________________________


    }
}
