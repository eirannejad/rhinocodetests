import Rhino
import scriptcontext as sc
import rhinoscriptsyntax as rs
from System.Drawing import Color
import os
import os.path as op

# Variables
folder = op.join(op.dirname(__file__), "files")
paths_list = []

g = 'ghenv' in globals()
d = sc._ScriptContextModule__doc
i = scriptcontext.id

#Paths
files_list = os.listdir(path=folder)
for item in files_list:
    l_c = item[-4:]
    if '.3dm' in l_c:
        file_path = folder + '\\' + item
        paths_list.append(file_path)

for f in paths_list:

    #Open file

    rs.Command('_-Open {} _Enter'.format(f))
    print("File opened: {0}".format(sc.doc.ActiveDoc.Name))

    print("Layers in this file")
    print(len(sc.doc.Layers))

    print("Objects in this file")
    print(len(sc.doc.Objects))

    ## Save file
    rs.Command('_-Save V 7 _Enter')
