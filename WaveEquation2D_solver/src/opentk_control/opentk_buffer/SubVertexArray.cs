using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;



namespace WaveEquation2D_solver.src.opentk_control.opentk_buffer
{
    public class SubVertexArray : IDisposable
    {

        private int _rendererId;

        private bool _disposed = false;



        public SubVertexArray()
        {
            // Main Constructor: generates a unique vertex array object ID.
            this._rendererId = GL.GenVertexArray();
  
        }


        private void ConfigureVertexAttribPointer(VertexBufferLayout layout, int startingLocation)
        {
            int offset = 0;

            IReadOnlyList<VertexBufferElement> elements = layout.GetElements;

            for (int i = 0; i < elements.Count; i++)
            {
                VertexBufferElement element = elements[i];
                int location = startingLocation + i;

                // Enable the vertex attribute array for the specified buffer index.
                GL.EnableVertexAttribArray(location);

                // Set up the vertex attribute pointer for the specified buffer index.
                // This specifies how to interpret the vertex data in the vertex buffer object.
                GL.VertexAttribPointer(location, element.count, element.type, element.normalized, layout.GetStride, offset);

                // Offset is the previous layout count (most likely the stride will remain the same)
                offset += element.count * VertexBufferElement.GetSizeOfType(element.type);
            }
        }


        public void Add_subVertexBuffer(SubVertexBuffer vb, VertexBufferLayout staticLayout, VertexBufferLayout dynamicLayout)
        {
            // Add and Bind a vertex buffer to an appropriate layout
            // Vertex Buffer layout  contains the information about co-ordinates, normals, color etc

            // Bind the vertex array object.
            Bind();

            // Bind the static vertex buffer object.
            vb.StaticBind();

            ConfigureVertexAttribPointer(staticLayout, 0);

            // Unbind the vertex buffer object.
            vb.UnBind();

            //___________________________________________________________________________________________________________________
            // Bind the dynamic vertex buffer object.
            vb.DynamicBind();
            
            ConfigureVertexAttribPointer(dynamicLayout, staticLayout.GetElements.Count);

            // Unbind the dynamic vertex buffer object.
            vb.UnBind();

            // Unbind the dynamic vertex array object.
            UnBind();
        }




        public void Bind() => GL.BindVertexArray(_rendererId);

        public void UnBind() => GL.BindVertexArray(0);




        public void Dispose()
        {
            if (!_disposed)
            {
                // Delete this buffer (acts like a  destructor)
                GL.DeleteVertexArray(this._rendererId);
                _disposed = true;
            }
        }






    }
}
