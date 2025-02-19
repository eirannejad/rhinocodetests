#! python 3

import rhinoscriptsyntax as rs
import Rhino

pt_ids = rs.GetObjects("Select points", 1, preselect=True)

if pt_ids:
    mesh_id = rs.GetObject("Select mesh", 32)
    if mesh_id:
        dir_vec = Rhino.Geometry.Vector3d(0, 0, -1)
        result = rs.ProjectPointToMesh(pt_ids, mesh_id, dir_vec)
        print(result[0])
        print(type(result))
