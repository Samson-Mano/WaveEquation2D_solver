using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.opentk_control.opentk_buffer;
using WaveEquation2D_solver.src.opentk_control.shader_compiler;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace WaveEquation2D_solver.src.model_store.geom_objects
{
    public class dynamiclabel_store
    {

        label_store label = new label_store();

        private int char_count = 0;
        private float font_size = 14.0f;
        private float geom_size = 1.0f;



        private VertexArray _dynlabelVAO;
        private VertexBuffer _dynlabelVBO;
        private IndexBuffer _dynlabelIBO;

        public Shader _dynlabelshader;



        public dynamiclabel_store(float geom_size, int char_count,
                                Vector2 label_loc, Vector3 label_color,
                                double label_angle, bool label_above_loc)
        {
            // (Re)Initialize the data
            this.geom_size = geom_size;
            this.char_count = char_count;

            string label_value = "";

            for (int i = 0; i < char_count; i++)
            {
                // Initialize the label data
                label_value += " ";
            }

            label.label_id = 0;
            label.label = label_value;
            label.label_loc = label_loc;
            label.label_color = label_color;
            label.label_angle = label_angle;
            label.label_above_loc = label_above_loc;
            label.label_char_count = char_count;


            InitializeShader();
            InitializeBuffers();

        }



        private void InitializeShader()
        {

            // Create Shader
            _dynlabelshader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.TextShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.TextShader));

        }


        private void InitializeBuffers()
        {
            // Label buffer
            _dynlabelVAO = new VertexArray();
            _dynlabelVBO = new VertexBuffer(10);
            _dynlabelIBO = new IndexBuffer(10);

            var labelLayout = new VertexBufferLayout();
            labelLayout.AddFloat(2);  // Character Position
            labelLayout.AddFloat(2);  // Text location
            labelLayout.AddFloat(2);  // Texture coordinate
            labelLayout.AddFloat(3);  // Text color


            _dynlabelVAO.Add_vertexBuffer(_dynlabelVBO, labelLayout);

            //_______________________________________________________________________
            // Set the buffer for the label

            // Set the buffer for index (6 indices to form a two triangle, quadrilateral )
            List<int> label_indices = new List<int>();

            // Set the label index buffers
            int t_id = 0;
            get_label_index_buffer(label, ref label_indices, ref t_id);

            // Update the index buffer
            _dynlabelIBO.ClearIndexBuffer();
            _dynlabelIBO.AppendIndexBuffer(label_indices.ToArray());


            // Define the vertex buffer size for a character
            List<float> label_vertices = new List<float>();

            // Set the label vertex buffers
            // Add vertex buffers
            get_label_vertex_buffer(label, ref label_vertices);

            // Update the vertex buffer
            _dynlabelVBO.ClearVertexBuffer();
            _dynlabelVBO.AppendVertexBuffer(label_vertices.ToArray());

        }

        public void update_label(string label_value, Vector2 label_loc)
        {
            // Update the label data
            this.label.label = label_value;
            this.label.label_loc = label_loc;

            // Update the vertex buffer for the label
            List<float> label_vertices = new List<float>();

            // Set the label vertex buffers
            get_label_vertex_buffer(label, ref label_vertices);

            // Update the vertex buffer
            _dynlabelVBO.updateVertexBuffer(label_vertices.ToArray());

        }



        public void paint_dynamic_labels()
        {
            // Paint all the dynamic points
            _dynlabelshader.Bind();
            _dynlabelVAO.Bind();
            _dynlabelIBO.Bind();

            // Activate texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture to the active texture unit
            GL.BindTexture(TextureTarget.Texture2D, gvariables_static.main_font.TextureID);

            // Draw the elements
            GL.DrawElements(PrimitiveType.Triangles, _dynlabelIBO.BufferCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);


            _dynlabelshader.UnBind();
            _dynlabelVAO.UnBind();
            _dynlabelIBO.UnBind();

        }



        public void update_openTK_uniforms(Matrix4 uMVP, float zoomscale, float transparency)
        {
            // Update the openGl uniform matrices
            _dynlabelshader.SetMatrix4("uMVP", uMVP);

            _dynlabelshader.SetFloat("zoomscale", zoomscale);

            // Set the transparency float
            _dynlabelshader.SetFloat("vertexTransparency", transparency);

        }




        private void get_label_vertex_buffer(label_store lb, ref List<float> label_vertices)
        {

            float font_scale = this.geom_size * 0.01f * (this.font_size / 12.0f) * 0.025f;

            // Find the label total width and total height of the label
            float total_label_width = 0.0f;
            float total_label_height = 0.0f;

            // lb.label[i] != '\0'

            for (int i = 0; i < lb.label_char_count; ++i)
            {
                // get the atlas information
                char ch = lb.label[i];
                Character ch_data = gvariables_static.main_font.Glyphs[ch];

                total_label_width += ch_data.Advance * font_scale;
                total_label_height = Math.Max(total_label_height, ch_data.Size.Y * font_scale);
            }


            // Get the x,y location
            Vector2 loc = lb.label_loc;
            float x = loc.X - (total_label_width * 0.5f);

            // Whether paint above the location or not
            float y = 0.0f;
            if (lb.label_above_loc == true)
            {
                y = loc.Y + (total_label_height * 0.5f);
            }
            else
            {
                y = loc.Y - (total_label_height + (total_label_height * 0.5f));
            }


            Vector2 rotated_pt = new Vector2(0, 0);

            for (int i = 0; i < lb.label_char_count; ++i)
            {
                // get the atlas information
                char ch = lb.label[i];

                Character ch_data = gvariables_static.main_font.Glyphs[ch];

                float xpos = x + (ch_data.Bearing.X * font_scale);
                float ypos = y - (ch_data.Size.Y - ch_data.Bearing.Y) * font_scale;

                float w = ch_data.Size.X * font_scale;
                float h = ch_data.Size.Y * font_scale;

                float margin = 0.00022f; // This value prevents the minor overlap with the next char when rendering

                // Point 1
                // Vertices [0,0] // 0th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos, ypos + h), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.TopLeft.X + margin);
                label_vertices.Add(ch_data.TopLeft.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________

                // Point 2
                // Vertices [0,1] // 1th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos, ypos), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.TopLeft.X + margin);
                label_vertices.Add(ch_data.BottomRight.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________

                // Point 3
                // Vertices [1,1] // 2th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos + w, ypos), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.BottomRight.X - margin);
                label_vertices.Add(ch_data.BottomRight.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________

                // Point 4
                // Vertices [1,0] // 3th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos + w, ypos + h), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.BottomRight.X - margin);
                label_vertices.Add(ch_data.TopLeft.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________
                x += ch_data.Advance * font_scale;

            }

        }


        private void get_label_index_buffer(label_store lb, ref List<int> label_indices, ref int t_id)
        {

            for (int i = 0; i < lb.label_char_count; ++i)
            {
                // Set the index buffers
                // Triangle 0,1,2
                label_indices.Add(t_id + 0);
                label_indices.Add(t_id + 1);
                label_indices.Add(t_id + 2);

                // Triangle 2,3,0
                label_indices.Add(t_id + 2);
                label_indices.Add(t_id + 3);
                label_indices.Add(t_id + 0);

                // Increment
                t_id = t_id + 4;
            }

        }


    }
}
