using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEquation2D_solver.src.model_store.fe_objects
{

    public class elementtri_store
    {
        public int tri_id { get; set; }
        public int nodeid1 { get; set; }
        public int nodeid2 { get; set; }
        public int nodeid3 { get; set; }
        public int material_id { get; set; }

    }



    public class elementtri_list_store
    {




        public Dictionary<int, elementtri_store> elementtriMap = new Dictionary<int, elementtri_store>();
        public int elementtri_count = 0;

        public elementtri_list_store()
        {
            // (Re)Initialize the data
            elementtriMap = new Dictionary<int, elementtri_store>();
            elementtri_count = 0;

        }

        public void add_elementtriangle(int tri_id, int nodeid1, int nodeid2, int nodeid3)
        {
            // Check whether the tri_id is already there
            if (elementtriMap.ContainsKey(tri_id))
                return;


            // Add the Triangle to the list
            elementtri_store temp_elementtri = new elementtri_store
            {
                tri_id = tri_id,
                nodeid1 = nodeid1,
                nodeid2 = nodeid2,
                nodeid3 = nodeid3
            };

            elementtriMap[tri_id] = temp_elementtri;
            elementtri_count++;

        }


        public void update_material(List<int> selected_element_tris, int material_id)
        {
            // Update the material id of the tri element
            foreach (int tri_id in selected_element_tris)
            {
                elementtriMap[tri_id].material_id = material_id;
            }

        }


        public void execute_delete_material(int del_material_id)
        {
            // Material is deleted, update the element which has the deleted material with default material
            foreach (int tri_id in elementtriMap.Keys)
            {
                if (elementtriMap[tri_id].material_id == del_material_id)
                {
                    elementtriMap[tri_id].material_id = 0;
                }

            }

        }

        //_________________________________________


    }
}
