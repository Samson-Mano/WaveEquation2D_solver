using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store.fe_objects;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace WaveEquation2D_solver.src.events_handler
{
    public class file_events
    {


        private static double RoundToSixDigits(double value) => Math.Round(value, 6);


        public static void import_txt_mesh(string fileContent,
                    ref fedata_store fedata,
                    ref List<Vector3> nodePtsList,
                    ref bool isModelLoadSuccess)
        {

            // Clear the data
            fedata = new fedata_store();

            var dataLines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int j = 0;

            // Node point list to capture the bounding geometry
            nodePtsList = new List<Vector3>();
            bool is_material_inpt_exists = false;

            while (j < dataLines.Length)
            {
                var line = dataLines[j].Trim();

                if (line == "*NODE")
                {

                    while (j < dataLines.Length)
                    {
                        var nodeLine = dataLines[j + 1].Trim();
                        var splitValues = nodeLine.Split(',');

                        if (splitValues.Length != 4)
                            break;

                        try
                        {
                            int nodeId = int.Parse(splitValues[0]);
                            double x = RoundToSixDigits(double.Parse(splitValues[1]));
                            double y = RoundToSixDigits(double.Parse(splitValues[2]));
                            double z = RoundToSixDigits(double.Parse(splitValues[3]));

                            var nodePt = new Vector3((float)x, (float)y, (float)z);
                            nodePtsList.Add(nodePt);

                            // node added to the node list
                            fedata.fe_nodes.add_node(nodeId, x, y);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing node line: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }

                        j++;
                    }

                    // Console.WriteLine($"Nodes read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }

                // Set the text 
                // Set the mesh boundaries
                Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);

                // update the global static value
                gvariables_static.geom_size = (geom_extremes.Item2 - geom_extremes.Item1).Length;


                if (line == "*ELEMENT,TYPE=S3")
                {

                    while (j < dataLines.Length)
                    {
                        var elementLine = dataLines[j + 1].Trim();
                        var splitValues = elementLine.Split(',');

                        if (splitValues.Length != 4)
                            break;

                        try
                        {
                            int triId = int.Parse(splitValues[0]);
                            int nd1 = int.Parse(splitValues[1]);
                            int nd2 = int.Parse(splitValues[2]);
                            int nd3 = int.Parse(splitValues[3]);

                            // Tirangle mesh added to the list
                            fedata.fe_tris.add_elementtriangle(triId, nd1, nd2, nd3);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing triangle element: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing triangle element: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Console.WriteLine($"Triangle Elements read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }

                if (line == "*ELEMENT,TYPE=S4")
                {

                    while (j < dataLines.Length)
                    {
                        var elementLine = dataLines[j + 1].Trim();
                        var splitValues = elementLine.Split(',');

                        if (splitValues.Length != 5)
                            break;

                        try
                        {
                            int quadId = int.Parse(splitValues[0]);
                            int nd1 = int.Parse(splitValues[1]);
                            int nd2 = int.Parse(splitValues[2]);
                            int nd3 = int.Parse(splitValues[3]);
                            int nd4 = int.Parse(splitValues[4]);

                            // Quadrilateral mesh added to the list
                            fedata.fe_quads.add_elementquadrilateral(quadId, nd1, nd2, nd3, nd4);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing quadrilateral element: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing quadrilateral element: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Console.WriteLine($"Quadrilateral Elements read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }


                if (line == "*MATERIAL_DATA")
                {
                    is_material_inpt_exists = true;
                    fedata.fe_materials.Clear(); // Clear existing materials
                    fedata.materialids.Clear(); // Clear the material ids

                    var triMaterialMap = new Dictionary<int, List<int>>();
                    var quadMaterialMap = new Dictionary<int, List<int>>();

                    while (j < dataLines.Length)
                    {
                        var materialLine = dataLines[j + 1].Trim();
                        var splitValues = materialLine.Split(',');

                        int numValues = splitValues.Length;

                        if (numValues == 7)
                        {
                            // Create a temporary materials
                            var tempMaterial = new material_data
                            {

                                material_id = int.Parse(splitValues[0]),
                                material_name = splitValues[1].Trim(),
                                youngs_modulus = double.Parse(splitValues[2]),
                                poissons_ratio = double.Parse(splitValues[3]),
                                material_density = double.Parse(splitValues[4]),
                                yield_point = double.Parse(splitValues[5]),
                                thickness = double.Parse(splitValues[6]),
                            };

                            // Add to material list
                            if (!fedata.fe_materials.ContainsKey(tempMaterial.material_id))
                            {
                                fedata.fe_materials[tempMaterial.material_id] = tempMaterial; // Add the material to the list
                                fedata.materialids.Add(tempMaterial.material_id); // Add the material id to the list
                            }
                        }
                        else if (numValues == 3)
                        {
                            int elmId = int.Parse(splitValues[0]); // Element ID
                            int elmType = int.Parse(splitValues[1]); // Element type 1 = Triangle, 2 = Quadrilateral
                            int materialId = int.Parse(splitValues[2]); // Material ID

                            if (elmType == 1)
                            {
                                // Add the Triangle element id to material ID map 
                                if (!triMaterialMap.ContainsKey(materialId))
                                    triMaterialMap[materialId] = new List<int>();

                                triMaterialMap[materialId].Add(elmId);
                            }
                            else if (elmType == 2)
                            {
                                // Add the Quadrilateral element id to material ID map 
                                if (!quadMaterialMap.ContainsKey(materialId))
                                    quadMaterialMap[materialId] = new List<int>();

                                quadMaterialMap[materialId].Add(elmId);
                            }
                        }
                        else
                        {
                            // Apply all materials at once
                            foreach (var kvp in triMaterialMap)
                            {
                                // value = list of tri element id, Key = material id
                                fedata.fe_tris.update_material(kvp.Value, kvp.Key);
                            }

                            foreach (var kvp in quadMaterialMap)
                            {
                                // value = list of quad element id, Key = material id
                                fedata.fe_quads.update_material(kvp.Value, kvp.Key);
                            }

                            // Console.WriteLine($"Material data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                            break;
                        }

                        j++;
                    }

                }


                if (line == "*NODE_CONSTRAINT_DATA")
                {
                    Dictionary<int, nodecnst_data> NodeConstraintSetData = new Dictionary<int, nodecnst_data>();

                    while (j < dataLines.Length)
                    {
                        var NodeConstraintLine = dataLines[j + 1].Trim();
                        var splitValues = NodeConstraintLine.Split(',');

                        if (splitValues.Length != 7)
                            break;

                        try
                        {
                            int NodeConstraintSetId = int.Parse(splitValues[0]);
                            int nodeId = int.Parse(splitValues[1]);
                            double NodeConstraint_fieldvalue = double.Parse(splitValues[2]);
                            double NodeConstraint_sourcevalue = double.Parse(splitValues[3]);
                            double NodeConstraint_sourcefrequency = double.Parse(splitValues[4]);
                            int NodeConstraint_sourcetype = int.Parse(splitValues[5]);
                            int NodeConstraint_isField = int.Parse(splitValues[6]);

                            if (!NodeConstraintSetData.ContainsKey(NodeConstraintSetId))
                                NodeConstraintSetData[NodeConstraintSetId] = new nodecnst_data();

                            var NodeconstraintEntry = NodeConstraintSetData[NodeConstraintSetId];
                            NodeconstraintEntry.constraint_node_ids.Add(nodeId); // Add the multiple nodes where the particular load set is applied

                            // Add the load amplitude when the first node is added (all the nodes have same load values)
                            if (NodeconstraintEntry.constraint_node_ids.Count == 1)
                            {
                                NodeconstraintEntry.field_value = NodeConstraint_fieldvalue; // Field value (Dirichlet boundary condition)
                                NodeconstraintEntry.source_value = NodeConstraint_sourcevalue; // Source term, Excitation source
                                NodeconstraintEntry.source_frequency = NodeConstraint_sourcefrequency; // Source term frequency

                                // Source type (0: Half sine pulse, 1: Rectangular pulse, 2: Triangle pulse, 3: Step force with finite rise,
                                // 4. Full sine pulse, 5. Harmonic/ periodic exictation)
                                NodeconstraintEntry.source_type = NodeConstraint_sourcetype;        

                                NodeconstraintEntry.isField = NodeConstraint_isField == 1 ? true : false;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing Node Constraint data: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing constraint data: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Add to main constraint storage
                    foreach (var kvp in NodeConstraintSetData)
                    {
                        var cnst = kvp.Value;

                        // Get the point locations
                        List<Vector2> constraint_node_pts = new List<Vector2>();

                        foreach (int ptid in cnst.constraint_node_ids)
                        {
                            node_store nd = fedata.fe_nodes.nodeMap[ptid];

                            constraint_node_pts.Add(new Vector2((float)nd.node_pt_x_coord,
                                (float)nd.node_pt_y_coord));
                        }



                        // Add the node constraint to the list
                       fedata.fe_nodeconstraints.add_nodeconstraint(cnst.constraint_node_ids, constraint_node_pts,
                                            cnst.field_value, cnst.source_value, cnst.source_frequency, cnst.source_type, cnst.isField);

                    }
                    // Console.WriteLine($"Constraint data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");

                }



                if (line == "*EDGE_CONSTRAINT_DATA")
                {
                    Dictionary<int, edgecnst_store> EdgeConstraintSetData = new Dictionary<int, edgecnst_store>();

                    while (j < dataLines.Length)
                    {
                        var EdgeConstraintLine = dataLines[j + 1].Trim();
                        var splitValues = EdgeConstraintLine.Split(',');

                        if (splitValues.Length != 9)
                            break;

                        try
                        {
                            int EdgeConstraintSetId = int.Parse(splitValues[0]);
                            int edgeId = int.Parse(splitValues[1]);
                            int edgeStartptID = int.Parse(splitValues[2]);
                            int edgeEndptID = int.Parse(splitValues[3]);
                            double EdgeConstraint_fieldvalue = double.Parse(splitValues[4]);
                            double EdgeConstraint_derivfieldvalue = double.Parse(splitValues[5]);
                            int EdgeConstraint_isFieldValue = int.Parse(splitValues[6]);
                            int EdgeConstraint_isDerivFieldValue = int.Parse(splitValues[7]);
                            int EdgeConstraint_isSommerfield = int.Parse(splitValues[8]);

                            if (!EdgeConstraintSetData.ContainsKey(EdgeConstraintSetId))
                                EdgeConstraintSetData[EdgeConstraintSetId] = new edgecnst_store();

                            var constraintEntry = EdgeConstraintSetData[EdgeConstraintSetId];
                            constraintEntry.constraint_edge_ids.Add(edgeId); // Add the multiple nodes where the particular load set is applied
                            constraintEntry.constraint_edge_startpt_ids.Add(edgeStartptID);
                            constraintEntry.constraint_edge_endpt_ids.Add(edgeEndptID);

                            // Add the load amplitude when the first edge is added (all the edges have same load values)
                            if (constraintEntry.constraint_edge_ids.Count == 1)
                            {
                                constraintEntry.field_value = EdgeConstraint_fieldvalue; // Field value (Dirichlet boundary condition)
                                constraintEntry.normalderivfield_value = EdgeConstraint_derivfieldvalue; // Derivative field value (Neumann boundary condition)
                                constraintEntry.isfieldvalue = EdgeConstraint_isFieldValue == 1 ? true : false;
                                constraintEntry.isnormalderivfieldvalue = EdgeConstraint_isDerivFieldValue == 1 ? true : false;
                                constraintEntry.isSommerfieldBC = EdgeConstraint_isSommerfield == 1 ? true : false;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing Edge Boundary Condition data: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing load data: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Add to main constraint storage
                    foreach (var kvp in EdgeConstraintSetData)
                    {
                        var cnst = kvp.Value;

                        // Get the start and end point locations
                        List<Vector2> constraint_edge_startpts = new List<Vector2>();
                        List<Vector2> constraint_edge_endpts = new List<Vector2>();

                        int i = 0;

                        foreach (int edgeid in cnst.constraint_edge_ids)
                        {
                            node_store nd1 = fedata.fe_nodes.nodeMap[cnst.constraint_edge_startpt_ids[i]];
                            node_store nd2 = fedata.fe_nodes.nodeMap[cnst.constraint_edge_endpt_ids[i]];

                            constraint_edge_startpts.Add(new Vector2((float)nd1.node_pt_x_coord,
                                (float)nd1.node_pt_y_coord));

                            constraint_edge_endpts.Add(new Vector2((float)nd2.node_pt_x_coord,
                                (float)nd2.node_pt_y_coord));

                            constraint_edge_endpts.Add(new Vector2((float)nd2.node_pt_x_coord,
                                (float)nd2.node_pt_y_coord));

                            i++;
                        }



                        // Add the edge constraint to the list
                       fedata.fe_edgeconstraints.add_edgeconstraint(cnst.constraint_edge_ids,
                            cnst.constraint_edge_startpt_ids, cnst.constraint_edge_endpt_ids,
                            cnst.constraint_edge_startpts, cnst.constraint_edge_endpts,
                            cnst.field_value, cnst.normalderivfield_value,
                            cnst.isfieldvalue, cnst.isnormalderivfieldvalue, cnst.isSommerfieldBC);

                    }

                    // Console.WriteLine($"Constraint data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");

                }



                // Iterate to next line
                j++;

            }


            // Check the model
            if (fedata.fe_nodes.node_count < 2 || (fedata.fe_tris.elementtri_count + fedata.fe_quads.elementquad_count) < 1)
            {
                isModelLoadSuccess = false;
                MessageBox.Show("Input error!! ", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }

            // No material is assigned in the model (Add a default material)
            if (is_material_inpt_exists == false || fedata.fe_materials.Count == 0)
            {
                var tempMaterial = new material_data
                {
                    material_id = 0, // material id
                    material_name = "Aluminum", // Default material name
                    material_density = 2.9e-9, // Density
                    youngs_modulus = 69.5e+5, // Youngs modulus
                    poissons_ratio = 0.33,
                    yield_point = 110.0,
                    thickness = 10.0,
                };

                // Add to the material list
                fedata.fe_materials.Clear();
                fedata.materialids.Clear();

                fedata.fe_materials[tempMaterial.material_id] = tempMaterial; // Add the material to the list
                fedata.materialids.Add(tempMaterial.material_id); // Add the material id to the list

                // Add default material id to all the elements
                List<int> selected_tri_elm_ids = new List<int>();
                List<int> selected_quad_elm_ids = new List<int>();

                foreach (var tri in fedata.fe_tris.elementtriMap)
                {
                    selected_tri_elm_ids.Add(tri.Key);
                }

                // Add default material to triangle mesh
                fedata.fe_tris.update_material(selected_tri_elm_ids, tempMaterial.material_id);


                foreach (var quad in fedata.fe_quads.elementquadMap)
                {
                    selected_quad_elm_ids.Add(quad.Key);
                }

                // Add default material to quadrilateral mesh
                fedata.fe_quads.update_material(selected_quad_elm_ids, tempMaterial.material_id);

            }


            // Model load is successful
            isModelLoadSuccess = true;

        }


        public static void export_binary_mesh(string filePath,
                    fedata_store fedata)
        {

            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {

                // Nodes
                writer.Write(fedata.fe_nodes.nodeMap.Count);
                foreach (var node in fedata.fe_nodes.nodeMap.Values)
                {
                    writer.Write(node.node_id);
                    writer.Write(node.node_pt_x_coord);
                    writer.Write(node.node_pt_y_coord);
                }

                // Tri elements
                writer.Write(fedata.fe_tris.elementtriMap.Count);
                foreach (var tri in fedata.fe_tris.elementtriMap.Values)
                {
                    writer.Write(tri.tri_id);
                    writer.Write(tri.nodeid1);
                    writer.Write(tri.nodeid2);
                    writer.Write(tri.nodeid3);
                    writer.Write(tri.material_id);
                }

                // Quad elements
                writer.Write(fedata.fe_quads.elementquadMap.Count);
                foreach (var quad in fedata.fe_quads.elementquadMap.Values)
                {
                    writer.Write(quad.quad_id);
                    writer.Write(quad.nodeid1);
                    writer.Write(quad.nodeid2);
                    writer.Write(quad.nodeid3);
                    writer.Write(quad.nodeid4);
                    writer.Write(quad.material_id);
                }

                // Materials
                writer.Write(fedata.fe_materials.Count);
                foreach (var mat in fedata.fe_materials.Values)
                {
                    writer.Write(mat.material_id);

                    // write the string length explicitly as a 4-byte integer followed by the raw bytes
                    var bytes = System.Text.Encoding.UTF8.GetBytes(mat.material_name);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);

                    writer.Write(mat.material_density);
                    writer.Write(mat.youngs_modulus);
                    writer.Write(mat.poissons_ratio);
                    writer.Write(mat.yield_point);
                    writer.Write(mat.thickness);

                }

                // Node constraints
                writer.Write(fedata.fe_nodeconstraints.ndcnstMap.Count);
                foreach (var cnst in fedata.fe_nodeconstraints.ndcnstMap.Values)
                {
                    writer.Write(cnst.ndcnst_set_id);
                    writer.Write(cnst.field_value);
                    writer.Write(cnst.source_value);
                    writer.Write(cnst.source_frequency);
                    writer.Write(cnst.source_type);
                    writer.Write(cnst.isField);

                    writer.Write(cnst.constraint_node_ids.Count);
                    foreach (int nid in cnst.constraint_node_ids)
                        writer.Write(nid);
                }

                // Edge constraints
                writer.Write(fedata.fe_edgeconstraints.edgecnstMap.Count);
                foreach (var cnst in fedata.fe_edgeconstraints.edgecnstMap.Values)
                {
                    writer.Write(cnst.edgecnst_set_id);
                    writer.Write(cnst.field_value);
                    writer.Write(cnst.normalderivfield_value);
                    writer.Write(cnst.isfieldvalue);
                    writer.Write(cnst.isnormalderivfieldvalue);
                    writer.Write(cnst.isSommerfieldBC);

                    writer.Write(cnst.constraint_edge_ids.Count);
                    for (int i = 0; i < cnst.constraint_edge_ids.Count; i++)
                    {
                        writer.Write(cnst.constraint_edge_ids[i]);
                        writer.Write(cnst.constraint_edge_startpt_ids[i]);
                        writer.Write(cnst.constraint_edge_endpt_ids[i]);
                    }

                }

            }



        }




        public static void import_binary_mesh(string filePath,
                    ref fedata_store fedata,
                    ref List<Vector3> nodePtsList,
                    ref bool isModelLoadSuccess)
        {

            // Clear the data
            fedata = new fedata_store();

            // Node point list to capture the bounding geometry
            nodePtsList = new List<Vector3>();

            isModelLoadSuccess = false;

            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                //// Read the p order
                //fedata.p_order = reader.ReadInt32();

                // --- NODES ---
                int nodeCount = reader.ReadInt32();
                for (int i = 0; i < nodeCount; i++)
                {

                    int nodeId = reader.ReadInt32();
                    double x = reader.ReadDouble();
                    double y = reader.ReadDouble();

                    var nodePt = new Vector3((float)x, (float)y, 0.0f);
                    nodePtsList.Add(nodePt);

                    // node added to the node list
                    fedata.fe_nodes.add_node(nodeId, x, y);
                }


                // Set the text 
                // Set the mesh boundaries
                Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);

                // update the global static value
                gvariables_static.geom_size = (geom_extremes.Item2 - geom_extremes.Item1).Length;


                //// --- Edges --- (Ignore the edges for reading the model)
                //int edgecount = reader.ReadInt32();
                //for (int i = 0; i < edgecount; i++)
                //{
                //    int edgeId = reader.ReadInt32();
                //    int startptID = reader.ReadInt32();
                //    int endptID = reader.ReadInt32();

                //}


                // --- TRI ELEMENTS ---
                var triMaterialMap = new Dictionary<int, List<int>>();

                int triCount = reader.ReadInt32();
                for (int i = 0; i < triCount; i++)
                {
                    int triId = reader.ReadInt32();
                    int nd1 = reader.ReadInt32();
                    int nd2 = reader.ReadInt32();
                    int nd3 = reader.ReadInt32();
                    int matid = reader.ReadInt32();

                    // Tirangle mesh added to the list
                    fedata.fe_tris.add_elementtriangle(triId, nd1, nd2, nd3);

                    // Add the Triangle element id to material ID map 
                    if (!triMaterialMap.ContainsKey(matid))
                        triMaterialMap[matid] = new List<int>();

                    triMaterialMap[matid].Add(triId);

                }

                // --- QUAD ELEMENTS ---
                var quadMaterialMap = new Dictionary<int, List<int>>();

                int quadCount = reader.ReadInt32();
                for (int i = 0; i < quadCount; i++)
                {
                    int quadId = reader.ReadInt32();
                    int nd1 = reader.ReadInt32();
                    int nd2 = reader.ReadInt32();
                    int nd3 = reader.ReadInt32();
                    int nd4 = reader.ReadInt32();
                    int matid = reader.ReadInt32();

                    // Quadrilateral mesh added to the list
                    fedata.fe_quads.add_elementquadrilateral(quadId, nd1, nd2, nd3, nd4);

                    // Add the Quadrilateral element id to material ID map 
                    if (!quadMaterialMap.ContainsKey(matid))
                        quadMaterialMap[matid] = new List<int>();

                    quadMaterialMap[matid].Add(quadId);

                }

                // --- MATERIALS ---
                int matCount = reader.ReadInt32();
                fedata.fe_materials.Clear();
                fedata.materialids.Clear(); // Clear the material ids
                for (int i = 0; i < matCount; i++)
                {
                    material_data mat = new material_data();
                    mat.material_id = reader.ReadInt32();

                    // Read string length explicitly (4 bytes) and then the raw bytes
                    int nameLength = reader.ReadInt32();
                    byte[] nameBytes = reader.ReadBytes(nameLength);
                    mat.material_name = System.Text.Encoding.UTF8.GetString(nameBytes);

                    mat.material_density = reader.ReadDouble();
                    mat.youngs_modulus = reader.ReadDouble();
                    mat.poissons_ratio = reader.ReadDouble();
                    mat.yield_point = reader.ReadDouble();
                    mat.thickness = reader.ReadDouble();

                    fedata.fe_materials[mat.material_id] = mat;
                    fedata.materialids.Add(mat.material_id);

                }

                // Apply all materials at once
                foreach (var kvp in triMaterialMap)
                {
                    // value = list of tri element id, Key = material id
                    fedata.fe_tris.update_material(kvp.Value, kvp.Key);
                }

                foreach (var kvp in quadMaterialMap)
                {
                    // value = list of quad element id, Key = material id
                    fedata.fe_quads.update_material(kvp.Value, kvp.Key);
                }


                // --- NODE CONSTRAINTS ---
                int nodeCnstCount = reader.ReadInt32();
                // fe_nodeconstraints.set_shader();

                for (int i = 0; i < nodeCnstCount; i++)
                {
                    nodecnst_data cnst = new nodecnst_data();
                    cnst.ndcnst_set_id = reader.ReadInt32();
                    cnst.field_value = reader.ReadDouble();
                    cnst.source_value = reader.ReadDouble();
                    cnst.source_frequency = reader.ReadDouble();
                    cnst.source_type = reader.ReadInt32();  
                    cnst.isField = reader.ReadBoolean();


                    int nidCount = reader.ReadInt32();
                    cnst.constraint_node_ids = new List<int>();
                    for (int j = 0; j < nidCount; j++)
                        cnst.constraint_node_ids.Add(reader.ReadInt32());

                    // Get the point locations
                    List<Vector2> constraint_node_pts = new List<Vector2>();

                    foreach (int ptid in cnst.constraint_node_ids)
                    {
                        node_store nd = fedata.fe_nodes.nodeMap[ptid];

                        constraint_node_pts.Add(new Vector2((float)nd.node_pt_x_coord,
                            (float)nd.node_pt_y_coord));
                    }


                    // Add the node constraint to the list
                    fedata.fe_nodeconstraints.add_nodeconstraint(cnst.constraint_node_ids, constraint_node_pts,
                        cnst.field_value, cnst.source_value, cnst.source_frequency, cnst.source_type, cnst.isField);

                }


                // --- EDGE CONSTRAINTS ---
                int edgeCnstCount = reader.ReadInt32();
                // fe_edgeconstraints.set_shader();

                for (int i = 0; i < edgeCnstCount; i++)
                {
                    edgecnst_store cnst = new edgecnst_store();
                    cnst.edgecnst_set_id = reader.ReadInt32();
                    cnst.field_value = reader.ReadDouble();
                    cnst.normalderivfield_value = reader.ReadDouble();
                    cnst.isfieldvalue = reader.ReadBoolean();
                    cnst.isnormalderivfieldvalue = reader.ReadBoolean();
                    cnst.isSommerfieldBC = reader.ReadBoolean();

                    int edgeCount = reader.ReadInt32();
                    cnst.constraint_edge_ids = new List<int>();
                    cnst.constraint_edge_startpt_ids = new List<int>();
                    cnst.constraint_edge_endpt_ids = new List<int>();

                    for (int j = 0; j < edgeCount; j++)
                    {
                        cnst.constraint_edge_ids.Add(reader.ReadInt32());
                        cnst.constraint_edge_startpt_ids.Add(reader.ReadInt32());
                        cnst.constraint_edge_endpt_ids.Add(reader.ReadInt32());
                    }

                    // Get the start and end point locations
                    List<Vector2> constraint_edge_startpts = new List<Vector2>();
                    List<Vector2> constraint_edge_endpts = new List<Vector2>();

                    int k = 0;

                    foreach (int edgeid in cnst.constraint_edge_ids)
                    {
                        node_store nd1 = fedata.fe_nodes.nodeMap[cnst.constraint_edge_startpt_ids[k]];
                        node_store nd2 = fedata.fe_nodes.nodeMap[cnst.constraint_edge_endpt_ids[k]];

                        constraint_edge_startpts.Add(new Vector2((float)nd1.node_pt_x_coord,
                            (float)nd1.node_pt_y_coord));

                        constraint_edge_endpts.Add(new Vector2((float)nd2.node_pt_x_coord,
                            (float)nd2.node_pt_y_coord));
   

                        constraint_edge_endpts.Add(new Vector2((float)nd2.node_pt_x_coord,
                            (float)nd2.node_pt_y_coord));

                        k++;
                    }



                    // Add the edge constraint to the list
                   fedata.fe_edgeconstraints.add_edgeconstraint(cnst.constraint_edge_ids,
                        cnst.constraint_edge_startpt_ids, cnst.constraint_edge_endpt_ids,
                        constraint_edge_startpts, constraint_edge_endpts,
                        cnst.field_value, cnst.normalderivfield_value,
                        cnst.isfieldvalue, cnst.isnormalderivfieldvalue, cnst.isSommerfieldBC);


                }




                // Check the model
                if (fedata.fe_nodes.node_count < 2 || (fedata.fe_tris.elementtri_count + fedata.fe_quads.elementquad_count) < 1)
                {
                    isModelLoadSuccess = false;
                    MessageBox.Show("Input error!! ", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }


                // Set that model load is success
                isModelLoadSuccess = true;

            }

        }




        //_____






    }
}
