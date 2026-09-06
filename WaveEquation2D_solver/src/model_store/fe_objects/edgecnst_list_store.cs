using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEquation2D_solver.src.events_handler;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store.geom_objects;

namespace WaveEquation2D_solver.src.model_store.fe_objects
{

    public class edgecnst_store
    {
        public int edgecnst_id { get; set; } // constraint id

        public List<int> constraint_edge_startpt_ids { get; set; }

        public List<int> constraint_edge_endpt_ids { get; set; }

        public List<Vector3> constraint_edge_startpts { get; set; }

        public List<Vector3> constraint_edge_endpts { get; set; }

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

        // Constraint visualization
        public meshdata_store edgecnst_meshdata;
        // Add labels for the constraint
        private label_list_store edgecnst_label;


        public edgecnst_list_store()
        {
            // (Re)Initialize the data
            edgecnstMap = new Dictionary<int, edgecnst_store>();
            edgecnst_count = 0;

            edgecnst_meshdata = new meshdata_store(false);
            edgecnst_label = new label_list_store();

        }


        public void add_edgeconstraint(List<int> constraint_edge_ids,
            List<int> constraint_edge_startpt_ids, List<int> constraint_edge_endpt_ids,
            List<Vector3> constraint_edge_startpts, List<Vector3> constraint_edge_endpts,
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
            List<Vector3> startnodePtsCopy = new List<Vector3>(constraint_edge_startpts);
            List<Vector3> endnodePtsCopy = new List<Vector3>(constraint_edge_endpts);


            // Add the constraint to the particular edge
            edgecnst_store temp_edge_cnst = new edgecnst_store
            {
                edgecnst_id = unique_constraintset_id,
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

            // Set the constraint data visualization
            set_constraint_visualization(unique_constraintset_id, true);

            // Add the constraint set id to list to track the unique constraint set id
            all_edgeconstraintset_ids.Add(unique_constraintset_id);

        }

        public void delete_edgeconstraint(int edgecnst_id)
        {
            // Remove the constraint set ID from all_constraintset_ids
            all_edgeconstraintset_ids.Remove(edgecnst_id);

            // Set the constraint data visualization
            set_constraint_visualization(edgecnst_id, false);

            // Remove the constraint data based on the key (constraint id)
            edgecnstMap.Remove(edgecnst_id);

            // adjust the constraint data count
            edgecnst_count--;

        }


        private void set_constraint_visualization(int edgecnst_id, bool isAdd)
        {
            // Get the constraint
            edgecnst_store cnstraint = edgecnstMap[edgecnst_id];

            if (isAdd == true)
            {
                // Add visualization for this constraint id
                int i = 0;
                int color_id = cnstraint.isSommerfieldBC == true ? -3 : -5;

                foreach (int edge_id in cnstraint.constraint_edge_ids)
                {
                    // Get the start point and end point
                    Vector2 edgestart_pt = new Vector2(cnstraint.constraint_edge_startpts[i].X,
                        cnstraint.constraint_edge_startpts[i].Y);
                    Vector2 edgeend_pt = new Vector2(cnstraint.constraint_edge_endpts[i].X,
                        cnstraint.constraint_edge_endpts[i].Y);

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

                    int cnst_edge_id = edge_id;

                    int ndid1 = (cnst_edge_id * 4) + 0;
                    int ndid2 = (cnst_edge_id * 4) + 1;
                    int ndid3 = (cnst_edge_id * 4) + 2;
                    int ndid4 = (cnst_edge_id * 4) + 3;

                    int lnid1 = (cnst_edge_id * 2) + 0;
                    int lnid2 = (cnst_edge_id * 2) + 1;


                    edgecnst_meshdata.add_mesh_point(ndid1, p1.X, p1.Y, 0.0, -1);
                    edgecnst_meshdata.add_mesh_point(ndid2, p2.X, p2.Y, 0.0, -1);
                    edgecnst_meshdata.add_mesh_point(ndid3, p3.X, p3.Y, 0.0, -1);
                    edgecnst_meshdata.add_mesh_point(ndid4, p4.X, p4.Y, 0.0, -1);

                    edgecnst_meshdata.add_mesh_lines(lnid1, ndid1, ndid3, color_id);
                    edgecnst_meshdata.add_mesh_lines(lnid2, ndid2, ndid4, color_id);

                    i++;

                }

                // Add labels
                int mid_index = cnstraint.constraint_edge_ids.Count / 2;
                string label_string1 = $"Edge Constraint {edgecnst_id}";
                string label_string2 = "";


                if (cnstraint.isSommerfieldBC == true)
                {
                    label_string2 = "Absorbtion Boundary Condition";
                }
                else
                {
                    if (cnstraint.isfieldvalue == true && cnstraint.isnormalderivfieldvalue == true)
                    {
                        label_string2 = $"Field value = {cnstraint.field_value}, " +
                                $"Normal derivative value = {cnstraint.normalderivfield_value}";
                    }
                    else if (cnstraint.isfieldvalue == true)
                    {
                        label_string2 = $"Field value = {cnstraint.field_value}";
                    }
                    else if (cnstraint.isnormalderivfieldvalue == true)
                    {
                        label_string2 = $"Normal derivative value = {cnstraint.normalderivfield_value}";
                    }

                }

                float label_ht = gvariables_static.get_text_height(12.0f) * 1.25f;

                Vector2 edgemid_pt = new Vector2((cnstraint.constraint_edge_startpts[mid_index].X + cnstraint.constraint_edge_endpts[mid_index].X) * 0.5f,
                    (cnstraint.constraint_edge_startpts[mid_index].Y + cnstraint.constraint_edge_endpts[mid_index].Y) * 0.5f);

                Vector2 label_loc1 = new Vector2(edgemid_pt.X, edgemid_pt.Y);
                Vector2 label_loc2 = new Vector2(edgemid_pt.X, edgemid_pt.Y - label_ht);

                edgecnst_label.add_label((edgecnst_id * 2) + 0, label_string1, label_loc1, color_id);
                edgecnst_label.add_label((edgecnst_id * 2) + 1, label_string2, label_loc2, color_id);

            }
            else
            {
                // Delete visualization for this constraint if
                foreach (int edge_id in cnstraint.constraint_edge_ids)
                {
                    int cnst_edge_id = edge_id;

                    int ndid1 = (cnst_edge_id * 4) + 0;
                    int ndid2 = (cnst_edge_id * 4) + 1;
                    int ndid3 = (cnst_edge_id * 4) + 2;
                    int ndid4 = (cnst_edge_id * 4) + 3;

                    int lnid1 = (cnst_edge_id * 2) + 0;
                    int lnid2 = (cnst_edge_id * 2) + 1;


                    // Delete mesh line
                    edgecnst_meshdata.delete_mesh_line(lnid1);
                    edgecnst_meshdata.delete_mesh_line(lnid2);

                    // Delete mesh point
                    edgecnst_meshdata.delete_mesh_point(ndid1);
                    edgecnst_meshdata.delete_mesh_point(ndid2);
                    edgecnst_meshdata.delete_mesh_point(ndid3);
                    edgecnst_meshdata.delete_mesh_point(ndid4);

                }

                // Delete labels
                edgecnst_label.delete_label((edgecnst_id * 2) + 0);
                edgecnst_label.delete_label((edgecnst_id * 2) + 1);

            }

            //cnst_meshdata.set_shader();
            edgecnst_meshdata.set_buffer();
            edgecnst_label.set_buffer();

        }

        public void set_shader()
        {
            // Set the shader 
            edgecnst_meshdata.set_shader();
            edgecnst_label.set_shader();

        }


        public void paint_edge_constraint()
        {
            // edge constraint count check
            if (edgecnst_count == 0)
                return;

            // Paint the constraint label
            gvariables_static.LineWidth = 3.0f;
            edgecnst_meshdata.paint_static_mesh_lines();
            gvariables_static.LineWidth = 1.0f;

        }


        public void paint_edge_constraint_label()
        {
            // edge constraint count check
            if (edgecnst_count == 0)
                return;

            edgecnst_label.paint_static_labels();

        }


        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            if (edgecnst_count == 0)
                return;


            edgecnst_meshdata.update_openTK_uniforms(graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            edgecnst_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);


        }




    }
}
