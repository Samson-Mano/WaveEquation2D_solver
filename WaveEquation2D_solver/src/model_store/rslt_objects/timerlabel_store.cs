using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEquation2D_solver.src.model_store.geom_objects;
using WaveEquation2D_solver.src.opentk_control.opentk_buffer;
using WaveEquation2D_solver.src.opentk_control.shader_compiler;

namespace WaveEquation2D_solver.src.model_store.rslt_objects
{
    public class timerlabel_store : IDisposable
    {

        // Timer Label labels
        private dynamiclabel_store timer_label; // = new label_list_store();

        // Drawing area rectangle point
        private Vector2 _lefttopCornerPoint = Vector2.Zero;


        private bool _disposed = false;

        private bool IsInitialized = false;

        public timerlabel_store()
        {
            // Empty constructor
        }


        public void InitializeTimerLabelData(int window_width, int window_height)
        {

            // Character count for timer label is 4 (e.g., "00:00")
            // 12345678901234567
            // Time = 00:00:00 s

            timer_label = new dynamiclabel_store(1.4f, 18, new Vector2(0), new Vector3(0.862745f, 0.078431f, 0.23529f), 0.0, true);

            // Initialize axis data here
            IsInitialized = true;


            UpdateTimerLabelPosition(window_width, window_height);
        }

    

        public void UpdateTimerLabelPosition(int window_width, int window_height)
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

            float leftX = -(float)normalizedScreenWidth * 0.5f;
            float leftY = (float)normalizedScreenHeight * 0.5f;

            // Assign to private rectangle points
            this._lefttopCornerPoint = new Vector2(leftX, leftY);


            UpdateVertexBuffers("Time = 00:00.000 s");

            // Update the label shader uniforms for the new window size
            Matrix4 uMVP = Matrix4.Identity;

            timer_label.update_openTK_uniforms(uMVP, 1.0f, 1.0f);

        }


        public void UpdateVertexBuffers(string label)
        {
          
            float OriginX = _lefttopCornerPoint.X + 0.06f;
            float OriginY = _lefttopCornerPoint.Y - 0.06f;

            // Add the labels for the X and Y axes
            timer_label.update_label(label, new Vector2(OriginX + 0.2f, OriginY - 0.001f));

        }



        public void paint_timer_label()
        {

            timer_label.paint_dynamic_labels();

        }



        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }


        //__________________________________________________


    }
}
