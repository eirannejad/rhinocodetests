#! python3
import os
import sys

from os import path as op

import rhinoscriptsyntax as rs
import scriptcontext as sc

import Rhino.Geometry
from Rhino.Geometry import Point3d

import numpy

class Test:
    def __init__(self):
        pass

    def do_something(self, value):
        pass


j = Test()

j.do_something(12)

p = Point3d(1,2,3)

print(rs.GetPoint())
