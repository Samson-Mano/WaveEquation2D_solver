using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEquation2D_solver.src.model_store.fe_objects
{

    public class node_store
    {
        public int node_id { get; set; }
        public double node_pt_x_coord { get; set; }
        public double node_pt_y_coord { get; set; }

    }

    public class node_list_store
    {

        public Dictionary<int, node_store> nodeMap = new Dictionary<int, node_store>();
        public int node_count = 0;

        public node_list_store()
        {
            // (Re)Initialize the data
            nodeMap = new Dictionary<int, node_store>();
            node_count = 0;
        }

        public void add_node(int node_id, double node_pt_x_coord, double node_pt_y_coord)
        {
            // Check whether the node_id is already there
            if (nodeMap.ContainsKey(node_id))
                return;

            // Add the Node to the list
            node_store temp_node = new node_store
            {
                node_id = node_id,
                node_pt_x_coord = node_pt_x_coord,
                node_pt_y_coord = node_pt_y_coord
                // node_pt_z_coord = node_pt_z_coord
            };

            nodeMap[node_id] = temp_node;
            node_count++;

        }


        //______________________________________

    }
}
