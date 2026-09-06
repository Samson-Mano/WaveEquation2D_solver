// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEquation2D_solver.Properties;
using WaveEquation2D_solver.src.events_handler;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store.geom_objects;
using WaveEquation2D_solver.src.opentk_control.opentk_buffer;
using WaveEquation2D_solver.src.opentk_control.shader_compiler;



namespace WaveEquation2D_solver.src.model_store.fe_objects
{


    public class nodecnst_data
    {
        public int ndcnst_set_id { get; set; } // constraint id

        public List<Vector2> constraint_node_pts { get; set; }

        public List<int> constraint_node_ids { get; set; }

        public double field_value { get; set; } // Dirichlet boundary condition

        public double source_value { get; set; } // Source/ External excitation 

        public bool isField { get; set; } // is Field value


    }


    public class nodecnst_list_store
    {



        public Dictionary<int, nodecnst_data> ndcnstMap = new Dictionary<int, nodecnst_data>();
        public int ndcnst_set_count = 0;

        private List<int> all_constraintset_ids = new List<int>();

        // Load labels
        private label_list_store constraint_label;


        // Constraint visualization
        private Shader constraintShader;
        private Texture constraintTexture_Pin;
        private Texture constraintTexture_Roller;

        // Vertex Buffer object and Vertex Array object 
        private VertexBuffer constraint_vbo;
        private VertexArray constraint_vao;
        private IndexBuffer constraint_ibo;


        public nodecnst_list_store()
        {
            // (Re)Initialize the data
            ndcnstMap = new Dictionary<int, nodecnst_data>();
            ndcnst_set_count = 0;

            InitializeShader();
            InitializeBuffers();

            constraint_label = new label_list_store();
        }


        private void InitializeShader()
        {
            // Initialize the Shader 
            constraintShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.ConstraintShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.ConstraintShader)
                );


            System.Drawing.Bitmap pin_support = Resources.Resource_pics.pic_pin_support;
            constraintTexture_Pin = new Texture();
            constraintTexture_Pin.LoadTexture(pin_support);

            System.Drawing.Bitmap roller_support = Resources.Resource_pics.pic_roller_support;
            constraintTexture_Roller = new Texture();
            constraintTexture_Roller.LoadTexture(roller_support);

            // Set texture uniform variables
            constraintShader.SetInt("u_TexturePin", 0);      // Texture unit 0
            constraintShader.SetInt("u_TextureRoller", 1);   // Texture unit 1

        }


        private void InitializeBuffers()
        {
            // Initialize the Buffer
            constraint_vao = new VertexArray();
            constraint_vbo = new VertexBuffer(10);
            constraint_ibo = new IndexBuffer(10);

            VertexBufferLayout constraintLayout = new VertexBufferLayout();
            constraintLayout.AddFloat(2);
            constraintLayout.AddFloat(2);
            constraintLayout.AddFloat(2);
            constraintLayout.AddFloat(1);

            constraint_vao.Add_vertexBuffer(constraint_vbo, constraintLayout);

        }


        public void add_nodeconstraint(List<int> constraint_node_ids, List<Vector2> constraint_node_pts,
            double field_value, double source_value, bool isField)
        {
            // Get an unique constraint set id
            int unique_constraintset_id = gvariables_static.get_unique_id(all_constraintset_ids);

            // Make a copy of the list
            List<int> idsCopy = new List<int>(constraint_node_ids);
            List<Vector2> nodePtsCopy = new List<Vector2>(constraint_node_pts);

            // Add the constraint to the particular node
            nodecnst_data temp_cnst = new nodecnst_data
            {
                ndcnst_set_id = unique_constraintset_id,
                constraint_node_pts = nodePtsCopy,
                constraint_node_ids = idsCopy,
                field_value = isField == true ? field_value : 0.0,
                source_value = isField == true ? 0.0 : source_value,
                isField = isField
            };

            // Insert the constraint to nodes
            ndcnstMap[unique_constraintset_id] = temp_cnst;
            ndcnst_set_count++;

            // Update the constraint data visualization
            update_buffer_data();

            // Add the constraint set id to list to track the unique constraint set id
            all_constraintset_ids.Add(unique_constraintset_id);

        }

        public void delete_nodeconstraint(int cnst_set_id)
        {
            // Remove the constraint set ID from all_constraintset_ids
            all_constraintset_ids.Remove(cnst_set_id);

            // Remove the constraint data based on the key (constraint set id)
            ndcnstMap.Remove(cnst_set_id);

            // adjust the constraint data count
            ndcnst_set_count--;

            // Update the constraint data visualization
            update_buffer_data();

        }


        public void paint_node_constraint()
        {
            // node constraint count check
            if (ndcnst_set_count == 0 || gvariables_static.is_paint_constraints == false)
                return;

            constraintShader.Bind();

            constraintTexture_Pin.Bind(0);
            constraintTexture_Roller.Bind(1);

            constraint_vao.Bind();
            constraint_ibo.Bind();

            // Paint the constraint
            GL.DrawElements(PrimitiveType.Triangles, constraint_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);

            constraint_ibo.UnBind();
            constraint_vao.UnBind();

            constraintTexture_Pin.UnBind();
            constraintTexture_Roller.UnBind();

            constraintShader.UnBind();

        }



        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            if (ndcnst_set_count == 0)
                return;

            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                            graphic_events_control.viewMatrix *
                            graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;

            constraintShader.SetMatrix4("uMVP", uMVP);
            constraintShader.SetFloat("zoomscale", zoomscale);

            Vector4 ConstraintColor = new Vector4(gvariables_static.ColorUtils.get_ConstraintColor(),
        gvariables_static.geom_transparency * 0.8f);


            constraintShader.SetVector4("vertexColor", ConstraintColor);

        }


        private void update_buffer_data()
        {
            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> constraintVertexData = new List<float>();
            List<int> constraintIndexData = new List<int>();

            // Get the constraint size
            float constraint_size = gvariables_static.get_font_scale(18.0f);

            // Rotate the corner points
            Vector2 bot_left = new Vector2(-constraint_size, -constraint_size); // 0 0
            Vector2 bot_right = new Vector2(constraint_size, -constraint_size); // 1 0
            Vector2 top_right = new Vector2(constraint_size, constraint_size); // 1 1
            Vector2 top_left = new Vector2(-constraint_size, constraint_size); // 0 1

            int t_id = 0;

            foreach (nodecnst_data cnst_data in ndcnstMap.Values)
            {
                float radians = (((float)cnst_data.constraint_angle + 90.0f) * 3.14159365f) / 180.0f; // convert degrees to radians
                float cos_theta = (float)Math.Cos(radians);
                float sin_theta = (float)Math.Sin(radians);

                // Rotated point of the corners
                Vector2 rotated_pt_bot_left = new Vector2((bot_left.X * cos_theta) + (bot_left.Y * sin_theta),
                     -(bot_left.X * sin_theta) + (bot_left.Y * cos_theta));

                Vector2 rotated_pt_bot_right = new Vector2((bot_right.X * cos_theta) + (bot_right.Y * sin_theta),
                    -(bot_right.X * sin_theta) + (bot_right.Y * cos_theta));

                Vector2 rotated_pt_top_right = new Vector2((top_right.X * cos_theta) + (top_right.Y * sin_theta),
                    -(top_right.X * sin_theta) + (top_right.Y * cos_theta));

                Vector2 rotated_pt_top_left = new Vector2((top_left.X * cos_theta) + (top_left.Y * sin_theta),
                    -(top_left.X * sin_theta) + (top_left.Y * cos_theta));


                foreach (Vector2 cnst_node_pt in cnst_data.constraint_node_pts)
                {

                    // Corner 1
                    // Set the constraint vertices corner 1
                    constraintVertexData.Add(cnst_node_pt.X + rotated_pt_bot_left.X);
                    constraintVertexData.Add(cnst_node_pt.Y + rotated_pt_bot_left.Y);
                    constraintVertexData.Add(cnst_node_pt.X);
                    constraintVertexData.Add(cnst_node_pt.Y);
                    constraintVertexData.Add(0.0f);
                    constraintVertexData.Add(0.0f);
                    constraintVertexData.Add(cnst_data.constraint_type);

                    // Corner 2
                    // Set the constraint vertices corner 2
                    constraintVertexData.Add(cnst_node_pt.X + rotated_pt_bot_right.X);
                    constraintVertexData.Add(cnst_node_pt.Y + rotated_pt_bot_right.Y);
                    constraintVertexData.Add(cnst_node_pt.X);
                    constraintVertexData.Add(cnst_node_pt.Y);
                    constraintVertexData.Add(1.0f);
                    constraintVertexData.Add(0.0f);
                    constraintVertexData.Add(cnst_data.constraint_type);

                    // Corner 3
                    // Set the constraint vertices corner 3
                    constraintVertexData.Add(cnst_node_pt.X + rotated_pt_top_right.X);
                    constraintVertexData.Add(cnst_node_pt.Y + rotated_pt_top_right.Y);
                    constraintVertexData.Add(cnst_node_pt.X);
                    constraintVertexData.Add(cnst_node_pt.Y);
                    constraintVertexData.Add(1.0f);
                    constraintVertexData.Add(1.0f);
                    constraintVertexData.Add(cnst_data.constraint_type);

                    // Corner 4
                    // Set the constraint vertices corner 4
                    constraintVertexData.Add(cnst_node_pt.X + rotated_pt_top_left.X);
                    constraintVertexData.Add(cnst_node_pt.Y + rotated_pt_top_left.Y);
                    constraintVertexData.Add(cnst_node_pt.X);
                    constraintVertexData.Add(cnst_node_pt.Y);
                    constraintVertexData.Add(0.0f);
                    constraintVertexData.Add(1.0f);
                    constraintVertexData.Add(cnst_data.constraint_type);

                    // Set the node indices
                    // Triangle 0, 1, 2
                    constraintIndexData.Add(t_id + 0);
                    constraintIndexData.Add(t_id + 1);
                    constraintIndexData.Add(t_id + 2);

                    // Triangle 2, 3, 0
                    constraintIndexData.Add(t_id + 2);
                    constraintIndexData.Add(t_id + 3);
                    constraintIndexData.Add(t_id + 0);

                    t_id = t_id + 4;

                }
            }


            // Clear and update buffers
            if (constraintVertexData.Count > 0)
            {
                // Convert to array and upload
                float[] vertexArray = constraintVertexData.ToArray();
                int[] indexArray = constraintIndexData.ToArray();

                // Clear existing data
                constraint_vbo.ClearVertexBuffer();
                constraint_ibo.ClearIndexBuffer();

                // Upload new data
                constraint_vbo.AppendVertexBuffer(vertexArray);
                constraint_ibo.AppendIndexBuffer(indexArray);

            }
            else
            {

                // Clear buffers if no data
                constraint_vbo.ClearVertexBuffer();
                constraint_ibo.ClearIndexBuffer();

            }

        }

        //___________________________________________________________



    }
}
