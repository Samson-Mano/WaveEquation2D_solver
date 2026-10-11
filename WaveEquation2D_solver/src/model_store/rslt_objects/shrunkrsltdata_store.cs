using WaveEquation2D_solver.src.opentk_control.opentk_buffer;
using WaveEquation2D_solver.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace WaveEquation2D_solver.src.model_store.rslt_objects
{
    public class shrunkrsltdata_store
    {
        private struct point_store
        {
            public int point_id;
            public float x_coord;
            public float y_coord;

            public float scalarValue;

        }

        private struct tri_store
        {
            public int tri_id;
            public int pt_id1;
            public int pt_id2;
            public int pt_id3;
        }

        private int shrunkpt_id = 0;
        private const float SHRINKFACTOR = 0.80f;


        private VertexBuffer shrunk_point_vbo; // Pre-shrunk vertices
        private VertexArray shrunk_point_vao;
        private IndexBuffer shrunk_triangle_ibo;



        private List<point_store> shrunk_points = new List<point_store>();
        private List<tri_store> shrunk_tris = new List<tri_store>();

        private bool shrunk_buffersInitialized = false;


        public shrunkrsltdata_store()
        {
            // Empty constructor
        }


        private void add_shrunk_point(int point_id, float x, float y, float scalarValue)
        {
            shrunk_points.Add(new point_store()
            {
                point_id = point_id,
                x_coord = x,
                y_coord = y,
                scalarValue = scalarValue
            });
        }


        public void add_shrunk_triangle(int tri_id, float[] pt1_data,
            float[] pt2_data, float[] pt3_data)
        {

            // Calculate centroid
            float cx = (pt1_data[0] + pt2_data[0] + pt3_data[0]) / 3.0f;
            float cy = (pt1_data[1] + pt2_data[1] + pt3_data[1]) / 3.0f;

            // Create 3 new points
            add_shrunk_point(shrunkpt_id + 0,
                cx + (pt1_data[0] - cx) * SHRINKFACTOR,
                cy + (pt1_data[1] - cy) * SHRINKFACTOR,
                pt1_data[2]);

            add_shrunk_point(shrunkpt_id + 1,
                cx + (pt2_data[0] - cx) * SHRINKFACTOR,
                cy + (pt2_data[1] - cy) * SHRINKFACTOR,
                pt2_data[2]);


            add_shrunk_point(shrunkpt_id + 2,
                cx + (pt3_data[0] - cx) * SHRINKFACTOR,
                cy + (pt3_data[1] - cy) * SHRINKFACTOR,
                pt3_data[2]);


            shrunk_tris.Add(new tri_store()
            {
                tri_id = tri_id,
                pt_id1 = shrunkpt_id + 0,
                pt_id2 = shrunkpt_id + 1,
                pt_id3 = shrunkpt_id + 2,
            });

            shrunkpt_id += 3;

        }



        public void create_shrunkrsltmesh_buffer_data()
        {

            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> shrunk_vertexData = new List<float>();

            for (int i = 0; i < shrunk_points.Count; i++)
            {
                point_store pt = shrunk_points[i];
                shrunk_vertexData.Add(pt.x_coord);
                shrunk_vertexData.Add(pt.y_coord);
                shrunk_vertexData.Add(pt.scalarValue);
            }

            // Create VAO and VBO for points
            shrunk_point_vao = new VertexArray();
            shrunk_point_vbo = new VertexBuffer(Math.Max(10, shrunk_vertexData.Count));

            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(2);
            pointLayout.AddFloat(1);

            shrunk_point_vao.Add_vertexBuffer(shrunk_point_vbo, pointLayout);
            shrunk_point_vbo.AppendVertexBuffer(shrunk_vertexData.ToArray());


            //_______________________________________________________________
            // prepare shrunk triangle and shrunk quadrilateral index data for openGL

            List<int> shrunk_triangleIndexData = new List<int>();

            foreach (tri_store tri in shrunk_tris)
            {
                shrunk_triangleIndexData.Add(tri.pt_id1);
                shrunk_triangleIndexData.Add(tri.pt_id2);
                shrunk_triangleIndexData.Add(tri.pt_id3);

            }

            shrunk_triangle_ibo = new IndexBuffer(Math.Max(10, shrunk_triangleIndexData.Count));
            if (shrunk_triangleIndexData.Count > 0)
            {
                shrunk_triangle_ibo.AppendIndexBuffer(shrunk_triangleIndexData.ToArray());
            }


            shrunk_buffersInitialized = true;

        }


        public void paint_shrunk_rsltmesh()
        {
            if (!shrunk_buffersInitialized)
                return;

            shrunk_point_vao.Bind();

            // Paint the triangle mesh
            shrunk_triangle_ibo.Bind();
            GL.DrawElements(PrimitiveType.Triangles, shrunk_triangle_ibo.BufferCount,
                DrawElementsType.UnsignedInt, 0);
            shrunk_triangle_ibo.UnBind();

            shrunk_point_vao.UnBind();

        }



    }
}
