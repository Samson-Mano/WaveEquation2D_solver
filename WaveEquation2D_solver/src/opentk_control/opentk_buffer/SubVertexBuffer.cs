// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace WaveEquation2D_solver.src.opentk_control.opentk_buffer
{
    public class SubVertexBuffer : IDisposable
    {


        private int _static_rendererId;
        private int _dynamic_rendererId;

        private int _static_capacity;  // Current static capacity in bytes
        private int _dynamic_capacity;  // Current dynamic capacity in bytes


        private int _static_size;      // Current used static count
        private int _dynamic_size;      // Current used dynamic count

        private bool _disposed = false;
        
        
        private List<float> _staticLocalBuffer = new List<float>();  // Local copy of all vertex data
        private List<float> _dynamicLocalBuffer = new List<float>();  // Local copy of all vertex data

        public int StaticSize => _static_size;
        
        public int DynamicSize => _dynamic_size;

        public int StaticCapacity => _static_capacity;

        public int DynamicCapacity => _dynamic_capacity;


        public SubVertexBuffer(int static_vertexbuffer_count = 10, int dynamic_vertexbuffer_count = 10)  // Note: Data count is the number of float count
        {
            // Main Constructor

            // Generate renderer IDs for static buffers
            _static_rendererId = GL.GenBuffer();
            _static_capacity = static_vertexbuffer_count * sizeof(float);
            _static_size = 0;

            StaticBind();
            GL.BufferData(BufferTarget.ArrayBuffer, _static_capacity, IntPtr.Zero, BufferUsageHint.StaticDraw);
            UnBind();

            // Generate renderer IDs for dynamic buffers
            _dynamic_rendererId = GL.GenBuffer();
            _dynamic_capacity = dynamic_vertexbuffer_count * sizeof(float);
            _dynamic_size = 0;

            DynamicBind();
            GL.BufferData(BufferTarget.ArrayBuffer, _dynamic_capacity, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            UnBind();

        }


        public void AppendVertexBuffer(float[] static_vertexbuffer_data, float[] dynamic_vertexbuffer_data)
        {

            // Static buffer handling
            if (static_vertexbuffer_data != null && static_vertexbuffer_data.Length != 0)
            {

                // Add to local buffer
                _staticLocalBuffer.AddRange(static_vertexbuffer_data);

                int static_vertexbuffer_size = _staticLocalBuffer.Count * sizeof(float);

                StaticBind();

                // Grow buffer if needed
                if (static_vertexbuffer_size > _static_capacity)
                {
                    // Grow the GPU buffer to accommodate new data
                    int newStaticCapacity = Math.Max(_static_capacity * 2, static_vertexbuffer_size);

                    // Reallocate GPU buffer with new size
                    GL.BufferData(BufferTarget.ArrayBuffer, newStaticCapacity, IntPtr.Zero, BufferUsageHint.StaticDraw);

                    _static_capacity = newStaticCapacity;
                }

                // Upload ALL data to GPU (this is the key - upload everything, not just new data)
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, static_vertexbuffer_size, _staticLocalBuffer.ToArray());
                _static_size = _staticLocalBuffer.Count;

                UnBind();
            }

            // Dynamic buffer handling
            if (dynamic_vertexbuffer_data != null && dynamic_vertexbuffer_data.Length != 0)
            {

                // Add to local buffer
                _dynamicLocalBuffer.AddRange(dynamic_vertexbuffer_data);

                int dynamic_vertexbuffer_size = _dynamicLocalBuffer.Count * sizeof(float);

                DynamicBind();

                // Grow buffer if needed
                if (dynamic_vertexbuffer_size > _dynamic_capacity)
                {
                    // Grow the GPU buffer to accommodate new data
                    int newDynamicCapacity = Math.Max(_dynamic_capacity * 2, dynamic_vertexbuffer_size);

                    // Reallocate GPU buffer with new size
                    GL.BufferData(BufferTarget.ArrayBuffer, newDynamicCapacity, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                    _dynamic_capacity = newDynamicCapacity;
                }

                // Upload ALL data to GPU (this is the key - upload everything, not just new data)
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, dynamic_vertexbuffer_size, _dynamicLocalBuffer.ToArray());
                _dynamic_size = _dynamicLocalBuffer.Count;

                UnBind();
            }
        }



        public void updateDynamicVertexBuffer(float[] dynamic_vertexbuffer_data)
        {
            if (dynamic_vertexbuffer_data == null || dynamic_vertexbuffer_data.Length == 0)
                return;

            int dynamic_vertexbuffer_size = dynamic_vertexbuffer_data.Length * sizeof(float);

            // Replace entire local buffer
            _dynamicLocalBuffer.Clear();
            _dynamicLocalBuffer.AddRange(dynamic_vertexbuffer_data);

            // Important!! Call only in Dynamic Buffer case
            // Update the vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._dynamic_rendererId);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, dynamic_vertexbuffer_size, dynamic_vertexbuffer_data);

        }



        public void ClearVertexBuffer()
        {
            _staticLocalBuffer.Clear();
            _dynamicLocalBuffer.Clear();

            _static_size = 0;
            _dynamic_size = 0;

        }

        public void StaticBind() => GL.BindBuffer(BufferTarget.ArrayBuffer, _static_rendererId);
        public void DynamicBind() => GL.BindBuffer(BufferTarget.ArrayBuffer, _dynamic_rendererId);
        public void UnBind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteBuffer(_static_rendererId);
                GL.DeleteBuffer(_dynamic_rendererId);
                _disposed = true;
            }
        }


        //________________________________________________________

    }
}
