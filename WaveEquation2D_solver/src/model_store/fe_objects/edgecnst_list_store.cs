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

namespace WaveEquation2D_solver.src.model_store.fe_objects
{

    public class edgecnst_store
    {
        public int edgecnst_set_id { get; set; } // constraint id

        public List<int> constraint_edge_startpt_ids { get; set; }

        public List<int> constraint_edge_endpt_ids { get; set; }

        public List<Vector2> constraint_edge_startpts { get; set; }

        public List<Vector2> constraint_edge_endpts { get; set; }

        public List<int> constraint_edge_ids { get; set; }

        public double field_value { get; set; } // Dirichlet boundary condition

        public double normalderivfield_value { get; set; } // neumann boundary condition (normal derivative field value)


        public bool isfieldvalue { get; set; } // is prescribed field value

        public bool isnormalderivfieldvalue { get; set; } // is prescribed normal derivative field value

        public bool isSommerfieldBC { get; set; } // is Sommerfield absorbing boundary condition



    }



    public class edgecnst_list_store
    {
        public Dictionary<int, edgecnst_store> edgecnstMap = new Dictionary<int, edgecnst_store>();
        public int edgecnst_count = 0;

        private List<int> all_edgeconstraintset_ids = new List<int>();

        // Add labels for the constraint
        private label_list_store edgeconstraint_label;


        // Edge Constraint visualization
        private Shader constraintShader;

        // Vertex Buffer object and Vertex Array object 
        private VertexBuffer constraint_vbo;
        private VertexArray constraint_vao;
        private IndexBuffer constraint_ibo;



        public edgecnst_list_store()
        {
            // (Re)Initialize the data
            edgecnstMap = new Dictionary<int, edgecnst_store>();
            edgecnst_count = 0;

            InitializeShader();
            InitializeBuffers();

            // edgecnst_meshdata = new meshdata_store(false);
            edgeconstraint_label = new label_list_store();

        }


        private void InitializeShader()
        {
            // Initialize the Shader 
            constraintShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.NodeConstraintShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.NodeConstraintShader)
                );

        }


        private void InitializeBuffers()
        {
            // Initialize the Buffer
            constraint_vao = new VertexArray();
            constraint_vbo = new VertexBuffer(10);
            constraint_ibo = new IndexBuffer(10);

            VertexBufferLayout constraintLayout = new VertexBufferLayout();
            constraintLayout.AddFloat(2); // Location
            constraintLayout.AddFloat(1); // Constraint Type

            constraint_vao.Add_vertexBuffer(constraint_vbo, constraintLayout);

        }


        public void add_edgeconstraint(List<int> constraint_edge_ids,
            List<int> constraint_edge_startpt_ids, List<int> constraint_edge_endpt_ids,
            List<Vector2> constraint_edge_startpts, List<Vector2> constraint_edge_endpts,
            double field_value, double normalderivfield_value,
            bool isfieldvalue, bool isnormalderivfieldvalue,
            bool isSommerfieldBC)
        {
            // Get an unique constraint set id
            int unique_constraintset_id = gvariables_static.get_unique_id(all_edgeconstraintset_ids);

            // Make a copy of the list
            List<int> idsCopy = new List<int>(constraint_edge_ids);
            List<int> startnodePtIDsCopy = new List<int>(constraint_edge_startpt_ids);
            List<int> endnodePtIDsCopy = new List<int>(constraint_edge_endpt_ids);
            List<Vector2> startnodePtsCopy = new List<Vector2>(constraint_edge_startpts);
            List<Vector2> endnodePtsCopy = new List<Vector2>(constraint_edge_endpts);


            // Add the constraint to the particular edge
            edgecnst_store temp_edge_cnst = new edgecnst_store
            {
                edgecnst_set_id = unique_constraintset_id,
                constraint_edge_startpt_ids = startnodePtIDsCopy,
                constraint_edge_endpt_ids = endnodePtIDsCopy,
                constraint_edge_startpts = startnodePtsCopy,
                constraint_edge_endpts = endnodePtsCopy,
                constraint_edge_ids = idsCopy,
                field_value = isSommerfieldBC == true ? 0.0 : field_value,
                normalderivfield_value = isSommerfieldBC == true ? 0.0 : normalderivfield_value,
                isfieldvalue = isfieldvalue,
                isnormalderivfieldvalue = isnormalderivfieldvalue,
                isSommerfieldBC = isSommerfieldBC
            };

            // Insert the constraint to edges
            edgecnstMap[unique_constraintset_id] = temp_edge_cnst;
            edgecnst_count++;

            // Update the constraint data visualization
            update_buffer_data();

            // Add the constraint set id to list to track the unique constraint set id
            all_edgeconstraintset_ids.Add(unique_constraintset_id);

        }


        public void delete_edgeconstraint(int edgecnst_id)
        {
            // Remove the constraint set ID from all_constraintset_ids
            all_edgeconstraintset_ids.Remove(edgecnst_id);

            // Remove the constraint data based on the key (constraint id)
            edgecnstMap.Remove(edgecnst_id);

            // adjust the constraint data count
            edgecnst_count--;

            // Update the constraint data visualization
            update_buffer_data();

        }

        public void paint_edge_constraint()
        {
            // edge constraint count check
            if (edgecnst_count == 0 || gvariables_static.is_paint_constraints == false)
                return;


            constraintShader.Bind();

            constraint_vao.Bind();
            constraint_ibo.Bind();

            // Paint the constraint
            GL.LineWidth(3.0f);
            GL.DrawElements(PrimitiveType.Lines, constraint_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);
            GL.LineWidth(1.0f);

            constraint_ibo.UnBind();
            constraint_vao.UnBind();

            constraintShader.UnBind();


            edgeconstraint_label.paint_static_labels();

        }



        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            if (edgecnst_count == 0)
                return;

            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                                     graphic_events_control.viewMatrix *
                                     graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;

            constraintShader.SetMatrix4("uMVP", uMVP);
            //constraintShader.SetFloat("zoomscale", zoomscale);

            Vector4 ConstraintColor = new Vector4(gvariables_static.ColorUtils.get_ConstraintColor(),
        gvariables_static.geom_transparency * 0.8f);


            //constraintShader.SetVector4("vertexColor", ConstraintColor);


            // Update the label uniforms
            edgeconstraint_label.update_openTK_uniforms(uMVP, zoomscale, gvariables_static.geom_transparency);

        }




        private void update_buffer_data()
        {
            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> constraintVertexData = new List<float>();
            List<int> constraintIndexData = new List<int>();

            // Get the constraint size
            float constraint_size = gvariables_static.geom_size * 0.0025f; // gvariables_static.get_font_scale(18.0f);

            // Rotate the corner points
            Vector2 bot_left = new Vector2(-constraint_size, -constraint_size); // 0 0
            Vector2 bot_right = new Vector2(constraint_size, -constraint_size); // 1 0
            Vector2 top_right = new Vector2(constraint_size, constraint_size); // 1 1
            Vector2 top_left = new Vector2(-constraint_size, constraint_size); // 0 1

            int t_id = 0;
            int label_id = 0;

            edgeconstraint_label.clear_labels();

            foreach (edgecnst_store cnst_data in edgecnstMap.Values)
            {
                int edge_count = cnst_data.constraint_edge_ids.Count;

                int constraint_type = (cnst_data.isfieldvalue || cnst_data.isnormalderivfieldvalue) ? 0 : 1; // 0 for field, 1 for source


                for (int i = 0; i < edge_count; i++)
                {
                    // Get the start and end points of the edge
                    Vector2 edgestart_pt = cnst_data.constraint_edge_startpts[i];
                    Vector2 edgeend_pt = cnst_data.constraint_edge_endpts[i];

                    float rectangle_width = gvariables_static.geom_size * 0.0025f;

                    // Direction vector from start to end
                    Vector2 dir = edgeend_pt - edgestart_pt;
                    dir.Normalize();

                    // Perpendicular (normal) vector
                    Vector2 normal = new Vector2(-dir.Y, dir.X);

                    // Half width offset
                    float halfWidth = rectangle_width / 2.0f;

                    // Four corners of the rectangle
                    Vector2 p1 = edgestart_pt + normal * halfWidth;
                    Vector2 p2 = edgestart_pt - normal * halfWidth;
                    Vector2 p3 = edgeend_pt - normal * halfWidth;
                    Vector2 p4 = edgeend_pt + normal * halfWidth;

                    // Corner 1
                    // Set the constraint vertices start point corner 1
                    constraintVertexData.Add(p1.X);
                    constraintVertexData.Add(p1.Y);
                    constraintVertexData.Add((float)constraint_type);

                    // Corner 2
                    // Set the constraint vertices start point corner 2
                    constraintVertexData.Add(p2.X);
                    constraintVertexData.Add(p2.Y);
                    constraintVertexData.Add((float)constraint_type);

                    // Corner 3
                    // Set the constraint vertices end point corner 3
                    constraintVertexData.Add(p3.X);
                    constraintVertexData.Add(p3.Y);
                    constraintVertexData.Add((float)constraint_type);

                    // Corner 4
                    // Set the constraint vertices end point corner 4
                    constraintVertexData.Add(p4.X);
                    constraintVertexData.Add(p4.Y);
                    constraintVertexData.Add((float)constraint_type);

                    // Set the node indices
                    // Line 0, 1 
                    constraintIndexData.Add(t_id + 0);
                    constraintIndexData.Add(t_id + 2);

                    // Line 2, 3
                    constraintIndexData.Add(t_id + 1);
                    constraintIndexData.Add(t_id + 3);

                    t_id = t_id + 4;

                }


                // Create the constraint label
                // Add labels
                int mid_index = edge_count / 2;

                string label_string1 = $"[CSet_{cnst_data.edgecnst_set_id}]";
                Vector3 cnst_color = new Vector3(0);

                if (cnst_data.isfieldvalue || cnst_data.isnormalderivfieldvalue)
                {
                    if (cnst_data.isfieldvalue == true)
                        label_string1 += $" u = {cnst_data.field_value}";
                    else
                        label_string1 += $" du/dn = {cnst_data.normalderivfield_value}";

                    cnst_color = new Vector3(0.5412f, 0.1686f, 0.8863f);
                }
                else
                {
                    label_string1 += $" ABC";
                    cnst_color = new Vector3(1.0f, 0.0f, 1.0f);
                }


                // string label_string2 = $"Amplitude = {load_data.load_amplitude}";

                // float label_ht = gvariables_static.get_text_height(12.0f) * 0.0125f;
                double label_mid_x = (cnst_data.constraint_edge_startpts[mid_index].X + cnst_data.constraint_edge_endpts[mid_index].X) * 0.5f;
                double label_mid_y = (cnst_data.constraint_edge_startpts[mid_index].Y + cnst_data.constraint_edge_endpts[mid_index].Y) * 0.5f;

                Vector2 label_loc1 = new Vector2((float)label_mid_x, (float)label_mid_y );
                //Vector2 label_loc2 = new Vector2(load_data.load_node_pts[mid_index].X,
                //        load_data.load_node_pts[mid_index].Y - label_ht);

                edgeconstraint_label.add_label(label_id + 0, label_string1, label_loc1, cnst_color);
                // load_label.add_label(label_id + 1, label_string2, label_loc2, gvariables_static.ColorUtils.get_LoadColor());

                // label_id = label_id + 2;
                label_id++;


            }

            // Update the label buffer
            edgeconstraint_label.update_buffer(gvariables_static.geom_size);


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
