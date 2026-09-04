using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEquation2D_solver.src.model_store.fe_objects
{

    public class elementquad_store
    {
        public int quad_id { get; set; }
        public int nodeid1 { get; set; }
        public int nodeid2 { get; set; }
        public int nodeid3 { get; set; }
        public int nodeid4 { get; set; }
        public int material_id { get; set; }

    }

    public class elementquad_list_store
    {


        public Dictionary<int, elementquad_store> elementquadMap = new Dictionary<int, elementquad_store>();
        public int elementquad_count = 0;

        public elementquad_list_store()
        {
            // (Re)Initialize the data
            elementquadMap = new Dictionary<int, elementquad_store>();
            elementquad_count = 0;

        }

        public void add_elementquadrilateral(int quad_id, int nodeid1, int nodeid2, int nodeid3, int nodeid4)
        {
            // Check whether the quad_id is already there
            if (elementquadMap.ContainsKey(quad_id))
                return;

            // Add the Quadrilateral to the list
            elementquad_store temp_elementquad = new elementquad_store
            {
                quad_id = quad_id,
                nodeid1 = nodeid1,
                nodeid2 = nodeid2,
                nodeid3 = nodeid3,
                nodeid4 = nodeid4
            };

            elementquadMap[quad_id] = temp_elementquad;
            elementquad_count++;

        }


        public void update_material(List<int> selected_element_quads, int material_id)
        {
            // Update the material id of the quad element
            foreach (int quad_id in selected_element_quads)
            {
                elementquadMap[quad_id].material_id = material_id;
            }

        }


        public void execute_delete_material(int del_material_id)
        {
            // Material is deleted, update the element which has the deleted material with default material
            foreach (int quad_id in elementquadMap.Keys)
            {
                if (elementquadMap[quad_id].material_id == del_material_id)
                {
                    elementquadMap[quad_id].material_id = 0;
                }

            }

        }

        //____________________________________________

    }
}
