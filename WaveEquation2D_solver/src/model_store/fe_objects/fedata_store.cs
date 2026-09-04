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


    public class material_data
    {
        public int material_id = 0;
        public string material_name = "";

        public double youngs_modulus = 0.0; // E
        public double poissons_ratio = 0.0; // G
        public double material_density = 0.0; // Rho
        public double yield_point = 0.0; // Y
        public double thickness = 0.0; // tk

        // public int number_of_elements_appliedto = 0;

    }

    public class fedata_store
    {


        public int p_order = 4; // higher order p - refinement

        public node_list_store fe_nodes;
        public elementtri_list_store fe_tris;
        public elementquad_list_store fe_quads;

        public nodecnst_list_store fe_constraints;
        public nodeload_list_store fe_loads;

        public Dictionary<int, material_data> fe_materials;
        public List<int> materialids;
        // public label_list_store materiallabels;

        public int number_of_edges = 0;

        // Drawing data
        private meshdata_store meshdrawingdata;
        private bool IsMeshDrawingDataSet = false;


        public HashSet<int> selected_tri_ids { get; } = new HashSet<int>();
        public HashSet<int> selected_quad_ids { get; } = new HashSet<int>();
        public HashSet<int> selected_node_ids { get; } = new HashSet<int>();


        public fedata_store()
        {
            // (Re)Initialize the data
            p_order = 4;

            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_constraints = new nodecnst_list_store();
            fe_loads = new nodeload_list_store();

            fe_materials = new Dictionary<int, material_data>();
            materialids = new List<int>();

            IsMeshDrawingDataSet = false;
        }

        public void set_meshdrawing_data()
        {
            meshdrawingdata = new meshdata_store();

            // Add the mesh points
            foreach (node_store nd in fe_nodes.nodeMap.Values)
            {
                meshdrawingdata.add_point(nd.node_id, (float)nd.node_pt_x_coord, (float)nd.node_pt_y_coord);

            }

            // Add the mesh tris
            foreach (elementtri_store tri in fe_tris.elementtriMap.Values)
            {
                meshdrawingdata.add_tri(tri.tri_id, tri.nodeid1, tri.nodeid2, tri.nodeid3, tri.material_id);

            }

            // Add the mesh quads
            foreach (elementquad_store quad in fe_quads.elementquadMap.Values)
            {
                meshdrawingdata.add_quad(quad.quad_id, quad.nodeid1, quad.nodeid2, quad.nodeid3, quad.nodeid4, quad.material_id);

            }

            // Create the mesh boundaries
            meshdrawingdata.create_wireframe();

            number_of_edges = meshdrawingdata.get_wireframe_line_count;

            // Create the mesh buffer
            meshdrawingdata.create_buffer_data();


            IsMeshDrawingDataSet = true;
        }


        public void paint_model()
        {
            if (!IsMeshDrawingDataSet)
                return;

            meshdrawingdata.paint_mesh();
            meshdrawingdata.paint_mesh_wireframe();
            meshdrawingdata.paint_mesh_points();

            meshdrawingdata.paint_selected_mesh_points();

            meshdrawingdata.paint_selected_mesh();

            // Paint the constraints
            fe_constraints.paint_node_constraint();

            // Paint the loads
            fe_loads.paint_node_load();

        }



        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            if (!IsMeshDrawingDataSet)
                return;

            meshdrawingdata.update_openTK_uniforms(graphic_events_control);
            fe_constraints.update_openTK_uniforms(graphic_events_control);
            fe_loads.update_openTK_uniforms(graphic_events_control);

        }


        public void select_nodes(Vector2 corner_pt1, Vector2 corner_pt2, bool isRightButton, drawing_events graphic_events_control)
        {
            // Select the nodes for load or constraint update
            List<int> selected_node_ids = new List<int>();

            // Pre-compute MVP matrix
            Matrix4 mvp = graphic_events_control.projectionMatrix *
                          graphic_events_control.viewMatrix *
                          graphic_events_control.modelMatrix;


            Matrix4 invMVP = Matrix4.Invert(mvp);

            // Transform rectangle corners from screen space to model space
            Vector2 modelCorner1 = TransformToModelSpace(corner_pt1, invMVP);
            Vector2 modelCorner2 = TransformToModelSpace(corner_pt2, invMVP);

            // Loop through all node in nodeMap
            foreach (node_store nd in fe_nodes.nodeMap.Values)
            {
                //______________________________
                Vector2 node_pt = new Vector2((float)nd.node_pt_x_coord, (float)nd.node_pt_y_coord);

                // Check whether the point inside a rectangle
                if (gvariables_static.isPointSelected(modelCorner1, modelCorner2, node_pt) == true)
                {
                    selected_node_ids.Add(nd.node_id);

                }

            }

            if (selected_node_ids.Count > 0)
            {
                add_selected_nodes(selected_node_ids, isRightButton);
            }

        }


        private void add_selected_nodes(List<int> selected_node_ids, bool IsRemove)
        {
            bool is_selection_changed = false;

            if (IsRemove == false)
            {
                // Add to the selected node list
                // Add all nodes at once
                int initialCount = this.selected_node_ids.Count;
                this.selected_node_ids.UnionWith(selected_node_ids);
                is_selection_changed = this.selected_node_ids.Count != initialCount;
            }
            else
            {
                // Remove from the selected node list
                // Remove all nodes at once
                int initialCount = this.selected_node_ids.Count;
                this.selected_node_ids.ExceptWith(selected_node_ids);
                is_selection_changed = this.selected_node_ids.Count != initialCount;
            }


            if (is_selection_changed == true)
            {
                // Add the selected nodes
                meshdrawingdata.add_selected_points(this.selected_node_ids.ToList());
            }
            //
        }


        public void clear_selected_nodes()
        {
            this.selected_node_ids.Clear();
            meshdrawingdata.clear_selected_points();

        }



        public void select_mesh(Vector2 corner_pt1, Vector2 corner_pt2, bool isRightButton, drawing_events graphic_events_control)
        {
            // Select the mesh for material properties update
            List<int> selected_tri_ids = new List<int>();
            List<int> selected_quad_ids = new List<int>();

            // Pre-compute MVP matrix
            Matrix4 mvp = graphic_events_control.projectionMatrix *
                          graphic_events_control.viewMatrix *
                          graphic_events_control.modelMatrix;


            Matrix4 invMVP = Matrix4.Invert(mvp);

            // Transform rectangle corners from screen space to model space
            Vector2 modelCorner1 = TransformToModelSpace(corner_pt1, invMVP);
            Vector2 modelCorner2 = TransformToModelSpace(corner_pt2, invMVP);


            // Select tri element for mesh
            foreach (elementtri_store tri in fe_tris.elementtriMap.Values)
            {
                Vector2 node_pt1 = new Vector2((float)fe_nodes.nodeMap[tri.nodeid1].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[tri.nodeid1].node_pt_y_coord);
                Vector2 node_pt2 = new Vector2((float)fe_nodes.nodeMap[tri.nodeid2].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[tri.nodeid2].node_pt_y_coord);
                Vector2 node_pt3 = new Vector2((float)fe_nodes.nodeMap[tri.nodeid3].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[tri.nodeid3].node_pt_y_coord);


                if (gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt1, node_pt2) == true ||
                    gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt2, node_pt3) == true ||
                    gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt3, node_pt1) == true)
                {
                    selected_tri_ids.Add(tri.tri_id);
                }

            }


            // Select quad element for mesh
            foreach (elementquad_store quad in fe_quads.elementquadMap.Values)
            {

                Vector2 node_pt1 = new Vector2((float)fe_nodes.nodeMap[quad.nodeid1].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[quad.nodeid1].node_pt_y_coord);
                Vector2 node_pt2 = new Vector2((float)fe_nodes.nodeMap[quad.nodeid2].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[quad.nodeid2].node_pt_y_coord);
                Vector2 node_pt3 = new Vector2((float)fe_nodes.nodeMap[quad.nodeid3].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[quad.nodeid3].node_pt_y_coord);
                Vector2 node_pt4 = new Vector2((float)fe_nodes.nodeMap[quad.nodeid4].node_pt_x_coord,
                    (float)fe_nodes.nodeMap[quad.nodeid4].node_pt_y_coord);

                if (gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt1, node_pt2) == true ||
                    gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt2, node_pt3) == true ||
                    gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt3, node_pt4) == true ||
                    gvariables_static.isEdgeSelected(modelCorner1, modelCorner2, node_pt4, node_pt1) == true)
                {
                    selected_quad_ids.Add(quad.quad_id);
                }

            }


            if ((selected_tri_ids.Count + selected_quad_ids.Count) > 0)
            {
                add_selected_mesh(selected_tri_ids, selected_quad_ids, isRightButton);
            }

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

        private void add_selected_mesh(List<int> selected_tri_ids, List<int> selected_quad_ids, bool IsRemove)
        {
            bool is_selection_changed = false;

            if (IsRemove == false)
            {
                // Add to the selected tri or selected quad list
                // Add all tri and all quad at once
                int initialCount = this.selected_tri_ids.Count + this.selected_quad_ids.Count;
                this.selected_tri_ids.UnionWith(selected_tri_ids);
                this.selected_quad_ids.UnionWith(selected_quad_ids);

                is_selection_changed = (this.selected_tri_ids.Count + this.selected_quad_ids.Count) != initialCount;

            }
            else
            {
                // Remove from the selected node list
                // Remove all tri and all quad at once
                int initialCount = this.selected_tri_ids.Count + this.selected_quad_ids.Count;
                this.selected_tri_ids.ExceptWith(selected_tri_ids);
                this.selected_quad_ids.ExceptWith(selected_quad_ids);

                is_selection_changed = (this.selected_tri_ids.Count + this.selected_quad_ids.Count) != initialCount;

            }


            if (is_selection_changed == true)
            {
                // Add the selected meshes
                meshdrawingdata.add_selected_mesh(this.selected_tri_ids.ToList(), this.selected_quad_ids.ToList());
            }
            //
        }


        public void clear_selected_mesh()
        {
            this.selected_tri_ids.Clear();
            this.selected_quad_ids.Clear();

            meshdrawingdata.clear_selected_mesh();

        }

        public void updateMaterial(int material_id)
        {
            if ((this.selected_tri_ids.Count + this.selected_quad_ids.Count) > 0)
            {
                fe_tris.update_material(this.selected_tri_ids.ToList(), material_id);
                fe_quads.update_material(this.selected_quad_ids.ToList(), material_id);

                meshdrawingdata.update_material(this.selected_tri_ids, this.selected_quad_ids, material_id);
            }

        }


        public void execute_delete_material(int material_id)
        {
            fe_tris.execute_delete_material(material_id);
            fe_quads.execute_delete_material(material_id);

            meshdrawingdata.deletematerial(material_id);

        }
        //________________________________




    }
}
