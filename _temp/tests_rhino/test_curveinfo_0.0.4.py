#! python 3
# PROBLEMS:
# file edited from vscode switches to empty
# self.Content returns empty in Eto form
# from Rhino.Geometry import * misses Point3d and probably other things
# sticky.Remove crashes
import scriptcontext as sc
from Rhino import ApplicationSettings as aps
from Rhino.Geometry import *
import Rhino.UI
import Eto.Forms as forms
import Eto.Drawing as drawing
import rhinoscriptsyntax as rs


class LineDivider(forms.Drawable):
    def __init__(self):
        # needed for python 3
        super().__init__()
        self.Size = drawing.Size(1, 1)
        self.BackgroundColor = drawing.Colors.Gray
        self.Padding = drawing.Padding(122)


class Dialog(forms.Form):
    def __init__(self):
        # needed for python 3
        super().__init__()
        self.Owner = Rhino.UI.RhinoEtoApp.MainWindow
        Rhino.UI.EtoExtensions.UseRhinoStyle(self)  # enable dark mode if set
        self.CreateEvents()
        title = "Curve Info 0.0.4"
        self.Title = title
        self.Size = drawing.Size(250, 412)
        self.Resizable = True
        self.ShowHelpButton = False
        self.Minimizable = False
        self.Maximizable = False
        self.precision = sc.doc.ModelDistanceDisplayPrecision
        self.boldfont = drawing.Font(
            "",
            9,
            drawing.FontStyle.Bold,
        )
        self.curvelabel = forms.Label()
        self.curvelabel.Text = "Curve Type"
        self.curvelabel.Font = self.boldfont
        self.curvedesc = forms.Label()
        self.curvedesc.Text = "..."
        self.curvedesc.Font = self.boldfont
        self.subdfriendly = forms.Label()
        self.subdfriendly.Text = "..."
        self.curvelength = forms.Label()
        self.curvelength.Text = "..."
        self.curvelength_label = forms.Label()
        self.curvelength_label.Text = "Length: "
        self.subdfriendly_label = forms.Label()
        self.subdfriendly_label.Text = "SubD Friendly: "
        self.area_label = forms.Label()
        self.area_label.Text = "Area: "
        self.obj = None
        self.radiuschanged = False
        self.start = forms.Label()
        self.start.Text = "..."
        self.end = forms.Label()
        self.end.Text = "..."
        self.degree = forms.Label()
        self.degree.Text = "..."
        self.spans = forms.Label()
        self.spans.Text = "..."
        self.pointcount = forms.Label()
        self.pointcount.Text = "..."
        self.area = forms.Label()
        self.area.Text = "..."
        self.arcradius = forms.NumericStepper()
        self.arcradius.Value = 0
        self.arcradius.Increment = 1
        self.arcradius.DecimalPlaces = self.precision
        self.arcradius.MinValue = 0
        self.arcradius.ValueChanged += self.OnArcRadiusChanged
        self.arccenter_x = forms.NumericStepper()
        self.arccenter_x.Value = 0
        self.arccenter_x.Increment = 1
        self.arccenter_x.DecimalPlaces = self.precision
        self.arccenter_y = forms.NumericStepper()
        self.arccenter_y.Value = 0
        self.arccenter_y.Increment = 1
        self.arccenter_y.DecimalPlaces = self.precision
        self.arccenter_z = forms.NumericStepper()
        self.arccenter_z.Value = 0
        self.arccenter_z.Increment = 1
        self.arccenter_z.DecimalPlaces = self.precision
        self.startpt_x = forms.NumericStepper()
        self.startpt_x.Value = 0
        self.startpt_x.Increment = 1
        self.startpt_x.DecimalPlaces = self.precision
        self.startpt_y = forms.NumericStepper()
        self.startpt_y.Value = 0
        self.startpt_y.Increment = 1
        self.startpt_y.DecimalPlaces = self.precision
        self.startpt_z = forms.NumericStepper()
        self.startpt_z.Value = 0
        self.startpt_z.Increment = 1
        self.startpt_z.DecimalPlaces = self.precision
        self.endpt_x = forms.NumericStepper()
        self.endpt_x.Value = 0
        self.endpt_x.Increment = 1
        self.endpt_x.DecimalPlaces = self.precision
        self.endpt_y = forms.NumericStepper()
        self.endpt_y.Value = 0
        self.endpt_y.Increment = 1
        self.endpt_y.DecimalPlaces = self.precision
        self.endpt_z = forms.NumericStepper()
        self.endpt_z.Value = 0
        self.endpt_z.Increment = 1
        self.endpt_z.DecimalPlaces = self.precision
        self.arccenter_x.ValueChanged += self.OnArcXChanged
        self.arccenter_y.ValueChanged += self.OnArcYChanged
        self.arccenter_z.ValueChanged += self.OnArcZChanged
        self.startpt_x.ValueChanged += self.OnStartPtXChanged
        self.startpt_y.ValueChanged += self.OnStartPtYChanged
        self.startpt_z.ValueChanged += self.OnStartPtZChanged
        self.endpt_x.ValueChanged += self.OnEndPtXChanged
        self.endpt_y.ValueChanged += self.OnEndPtYChanged
        self.endpt_z.ValueChanged += self.OnEndPtZChanged
        self.reset()
        self.Closed += self.OnFormClosed
        layout = forms.DynamicLayout()
        layout.Padding = drawing.Padding(11)
        layout.DefaultSpacing = drawing.Size(5, 5)
        layout.BeginVertical()
        layout.AddRow(self.curvelabel, None, self.curvedesc)
        layout.AddRow(self.curvelength_label, None, self.curvelength)
        layout.AddRow(self.subdfriendly_label, None, self.subdfriendly)
        layout.AddRow(self.area_label, None, self.area)
        layout.EndVertical()
        # divider start
        layout.BeginVertical()
        line = LineDivider()
        layout.AddRow(line)
        layout.EndVertical()
        # divider end
        ta = forms.TextAlignment.Right
        tl = forms.TextAlignment.Left
        tc = forms.TextAlignment.Center
        self.line_start_label = forms.Label()
        self.line_start_label.Text = "Line Start"
        self.line_start_label.TextAlignment = tl
        self.x_label = forms.Label()
        self.x_label.Text = "x"
        self.x_label.TextAlignment = tc
        self.y_label = forms.Label()
        self.y_label.Text = "y"
        self.y_label.TextAlignment = tc
        self.z_label = forms.Label()
        self.z_label.Text = "z"
        self.z_label.TextAlignment = tc
        self.line_end_label = forms.Label()
        self.line_end_label.Text = "Line End"
        self.line_end_label.TextAlignment = tl
        layout.BeginVertical()
        layout.AddRow(self.line_end_label, None, self.line_start_label)
        layout.AddRow(self.startpt_x, self.x_label, self.endpt_x)
        layout.AddRow(self.startpt_y, self.y_label, self.endpt_y)
        layout.AddRow(self.startpt_z, self.z_label, self.endpt_z)
        layout.EndVertical()
        # layout.BeginVertical()
        # layout.AddRow(forms.Label(Text='Start pt', TextColor = tc),None, self.start)
        # layout.AddRow(forms.Label(Text='End pt', TextColor = tc),None, self.end)
        # layout.EndVertical()
        # divider start
        layout.BeginVertical()
        line = LineDivider()
        layout.AddRow(line)
        layout.EndVertical()
        # divider end
        layout.BeginVertical()
        self.label_degree = forms.Label()
        self.label_degree.Text = "Degree"
        self.label_ptcount = forms.Label()
        self.label_ptcount.Text = "Point Count"
        self.label_spans = forms.Label()
        self.label_spans.Text = "Spans"
        layout.AddRow(self.label_degree, None, self.degree)
        layout.AddRow(self.label_ptcount, None, self.pointcount)
        layout.AddRow(self.label_spans, None, self.spans)
        layout.EndVertical()
        # divider start
        layout.BeginVertical()
        line2 = LineDivider()
        layout.AddRow(line2)
        layout.EndVertical()
        # divider end
        layout.BeginVertical()
        self.label_arcradius = forms.Label()
        self.label_arcradius.TextAlignment = tl
        self.label_arcradius.Text = "Arc Radius"
        layout.AddRow(self.label_arcradius, None, self.arcradius)
        self.label_arccenter = forms.Label()
        self.label_arccenter.Text = "Arc Center"
        self.label_arccenter.TextAlignment = tl
        self.xa_label = forms.Label()
        self.xa_label.Text = "x"
        self.xa_label.TextAlignment = ta
        self.ya_label = forms.Label()
        self.ya_label.Text = "y"
        self.ya_label.TextAlignment = ta
        self.za_label = forms.Label()
        self.za_label.Text = "z"
        self.za_label.TextAlignment = ta
        layout.AddRow(self.label_arccenter, self.xa_label, self.arccenter_x)
        layout.AddRow(None, self.ya_label, self.arccenter_y)
        layout.AddRow(None, self.za_label, self.arccenter_z)
        layout.EndVertical()
        layout.AddRow(None)
        self.Content = layout

    def reset(self):
        self.isarc = False
        self.centerpoint = Rhino.Geometry.Point3d.Unset
        self.radius = 1
        self.radiuschanged = False
        self.arc_X = 0
        self.arc_Y = 0
        self.arc_Z = 0
        self.start_X = 0
        self.start_Y = 0
        self.start_Z = 0
        self.startpoint = Rhino.Geometry.Point3d.Unset
        self.endpoint = Rhino.Geometry.Point3d.Unset
        self.arc_X_changed = False
        self.arc_Y_changed = False
        self.arc_Z_changed = False
        self.start_X_changed = False
        self.start_Y_changed = False
        self.start_Z_changed = False
        self.end_X_changed = False
        self.end_Y_changed = False
        self.end_Z_changed = False
        self.startpt_x.Enabled = False
        self.startpt_y.Enabled = False
        self.startpt_z.Enabled = False
        self.endpt_x.Enabled = False
        self.endpt_y.Enabled = False
        self.endpt_z.Enabled = False

    def CreateEvents(self):
        Rhino.RhinoDoc.SelectObjects += self.OnSelectObjects
        Rhino.RhinoDoc.DeselectAllObjects += self.OnDeselectAllObjects
        Rhino.RhinoApp.Idle += self.OnIdle

    def RemoveEvents(self):
        Rhino.RhinoDoc.SelectObjects -= self.OnSelectObjects
        Rhino.RhinoDoc.DeselectAllObjects -= self.OnDeselectAllObjects
        Rhino.RhinoApp.Idle -= self.OnIdle

    def OnSelectObjects(self, sender, e):
        try:
            self.reset()
        except Exception as ex:
            print(ex)
        self.isarc = False
        objs = sc.doc.Objects.GetSelectedObjects(False, False)
        if len(list(objs)) > 1:
            return
        if len(e.RhinoObjects) == 1:
            self.obj = e.RhinoObjects[0]
            if not type(self.obj) == Rhino.DocObjects.CurveObject:
                return
            try:
                if type(self.obj.CurveGeometry) == Rhino.Geometry.ArcCurve:
                    self.curvedesc.Text = "Arc/Circle"
                    self.isarc = True
                if type(self.obj.CurveGeometry) == Rhino.Geometry.PolyCurve:
                    self.curvedesc.Text = "PolyCurve"
                    if self.obj.CurveGeometry.IsPlanar():
                        self.curvedesc.Text = "Planar PolyCurve"
                if type(self.obj.CurveGeometry) == Rhino.Geometry.PolylineCurve:
                    self.curvedesc.Text = "PolyLine"
                    if self.obj.CurveGeometry.IsPlanar():
                        self.curvedesc.Text = "Planar PolyLine"
                    self.pointcount.Text = str(self.obj.CurveGeometry.PointCount)
                if type(self.obj.CurveGeometry) == Rhino.Geometry.LineCurve:
                    self.curvedesc.Text = "Line"
                    self.pointcount.Text = str(2)
                if type(self.obj.CurveGeometry) == Rhino.Geometry.NurbsCurve:
                    self.curvedesc.Text = "NurbsCurve"
                    if self.obj.CurveGeometry.IsPlanar():
                        self.curvedesc.Text = "Planar NurbsCurve"
                    self.pointcount.Text = str(self.obj.CurveGeometry.Points.Count)
                self.curvelength.Text = str(
                    round(self.obj.CurveGeometry.GetLength(), self.precision)
                )
                self.subdfriendly.Text = "no"
                self.arcradius.Enabled = False
                self.arccenter_x.Enabled = False
                self.arccenter_y.Enabled = False
                self.arccenter_z.Enabled = False
                if self.isarc:
                    self.arcradius.Enabled = True
                    self.arccenter_x.Enabled = True
                    self.arccenter_y.Enabled = True
                    self.arccenter_z.Enabled = True
                if self.obj.CurveGeometry.IsSubDFriendly:
                    self.subdfriendly.Text = "yes"
                if self.obj.CurveGeometry.IsClosed:
                    if self.obj.CurveGeometry.IsPlanar():
                        amp = Rhino.Geometry.AreaMassProperties.Compute(
                            self.obj.CurveGeometry
                        )
                        if amp.Area:
                            self.area.Text = str(round(amp.Area, self.precision))
                # start = self.obj.CurveGeometry.PointAtStart
                # end = self.obj.CurveGeometry.PointAtEnd
                self.startpt_x.Enabled = False
                self.startpt_y.Enabled = False
                self.startpt_z.Enabled = False
                self.endpt_x.Enabled = False
                self.endpt_y.Enabled = False
                self.endpt_z.Enabled = False
                if (type(self.obj.CurveGeometry) == Rhino.Geometry.LineCurve) or (
                    type(self.obj.CurveGeometry) == Rhino.Geometry.PolylineCurve
                    and self.obj.CurveGeometry.PointCount == 2
                ):
                    self.startpt_x.Enabled = True
                    self.startpt_y.Enabled = True
                    self.startpt_z.Enabled = True
                    self.endpt_x.Enabled = True
                    self.endpt_y.Enabled = True
                    self.endpt_z.Enabled = True
                self.startpoint = self.obj.CurveGeometry.PointAtStart
                self.endpoint = self.obj.CurveGeometry.PointAtEnd
                if self.endpoint[2] == self.startpoint[2]:
                    if (
                        self.startpoint[0] == self.endpoint[0]
                        and not self.startpoint[1] == self.endpoint[1]
                    ):
                        if (
                            self.obj.CurveGeometry.SpanCount == 1
                            and self.obj.CurveGeometry.Degree == 1
                        ):
                            self.curvedesc.Text += " (Vertical)"
                    if (
                        self.startpoint[1] == self.endpoint[1]
                        and not self.startpoint[0] == self.endpoint[0]
                    ):
                        if (
                            self.obj.CurveGeometry.SpanCount == 1
                            and self.obj.CurveGeometry.Degree == 1
                        ):
                            self.curvedesc.Text += " (Horizontal)"
                self.startpoint[0] = round(self.startpoint[0], self.precision)
                self.startpoint[1] = round(self.startpoint[1], self.precision)
                self.startpoint[2] = round(self.startpoint[2], self.precision)
                self.endpoint[0] = round(self.endpoint[0], self.precision)
                self.endpoint[1] = round(self.endpoint[1], self.precision)
                self.endpoint[2] = round(self.endpoint[2], self.precision)
                self.start_X = self.startpoint[0]
                self.start_Y = self.startpoint[1]
                self.start_Z = self.startpoint[2]
                self.end_X = self.endpoint[0]
                self.end_Y = self.endpoint[1]
                self.end_Z = self.endpoint[2]
                self.startpt_x.Value = self.start_X
                self.startpt_y.Value = self.start_Y
                self.startpt_z.Value = self.start_Z
                self.endpt_x.Value = self.end_X
                self.endpt_y.Value = self.end_Y
                self.endpt_z.Value = self.end_Z
                # self.end.Text = str(end)
                self.degree.Text = str(self.obj.CurveGeometry.Degree)
                self.spans.Text = str(self.obj.CurveGeometry.SpanCount)
                if type(self.obj.CurveGeometry) == Rhino.Geometry.ArcCurve:
                    self.arcradius.ValueChanged -= self.OnArcRadiusChanged
                    self.arcradius.Value = round(
                        self.obj.CurveGeometry.Arc.Radius, self.precision
                    )
                    self.arcradius.ValueChanged += self.OnArcRadiusChanged
                    self.centerpoint = self.obj.CurveGeometry.Arc.Center
                    self.arccenter_x.ValueChanged -= self.OnArcXChanged
                    self.arccenter_x.Value = self.centerpoint[0]
                    self.arccenter_x.ValueChanged += self.OnArcXChanged
                    self.arccenter_y.ValueChanged -= self.OnArcYChanged
                    self.arccenter_y.Value = self.centerpoint[1]
                    self.arccenter_y.ValueChanged += self.OnArcYChanged
                    self.arccenter_z.ValueChanged -= self.OnArcZChanged
                    self.arccenter_z.Value = self.centerpoint[2]
                    self.arccenter_z.ValueChanged -= self.OnArcZChanged
            except Exception as ex:
                print(ex)
            # self.Content.Invalidate()

    def OnDeselectAllObjects(self, sender, e):
        try:
            self.reset()
            self.curvedesc.Text = "..."
            self.curvelength.Text = "..."
            self.degree.Text = "..."
            self.spans.Text = "..."
            self.area.Text = "..."
            self.pointcount.Text = "..."
            self.subdfriendly.Text = "..."
        except Exception as ex:
            print(ex)
        self.Content.Invalidate()

    def OnArcXChanged(self, sender, e):
        try:
            if self.centerpoint[0] == sender.Value:
                return
            self.arc_X = sender.Value
            self.arc_X_changed = True
        except Exception as ex:
            print(ex)

    def OnArcYChanged(self, sender, e):
        try:
            if self.centerpoint[1] == sender.Value:
                return
            self.arc_Y = sender.Value
            self.arc_Y_changed = True
        except Exception as ex:
            print(ex)

    def OnArcZChanged(self, sender, e):
        try:
            if self.centerpoint[2] == sender.Value:
                return
            self.arc_Z = sender.Value
            self.arc_Z_changed = True
        except Exception as ex:
            print(ex)

    def OnStartPtXChanged(self, sender, e):
        if self.startpoint[0] == sender.Value:
            return
        self.start_X = sender.Value
        self.start_X_changed = True

    def OnStartPtYChanged(self, sender, e):
        if self.startpoint[1] == sender.Value:
            return
        self.start_Y = sender.Value
        self.start_Y_changed = True

    def OnStartPtZChanged(self, sender, e):
        if self.startpoint[2] == sender.Value:
            return
        self.start_Z = sender.Value
        self.start_Z_changed = True

    def OnEndPtXChanged(self, sender, e):
        if self.endpoint[0] == sender.Value:
            return
        self.end_X = sender.Value
        self.end_X_changed = True

    def OnEndPtYChanged(self, sender, e):
        if self.endpoint[1] == sender.Value:
            return
        self.end_Y = sender.Value
        self.end_Y_changed = True

    def OnEndPtZChanged(self, sender, e):
        if self.endpoint[2] == sender.Value:
            return
        self.end_Z = sender.Value
        self.end_Z_changed = True

    def OnArcRadiusChanged(self, sender, e):
        try:
            if sender.Value > 0:
                if self.radius == sender.Value:
                    return
                self.radius = sender.Value
                self.radiuschanged = True
        except Exception as ex:
            print(ex)

    def OnIdle(self, sender, e):
        def replace_arc():
            # result = sc.doc.BeginUndoRecord("Modify Arc")
            ar = self.obj.Geometry.Arc.Angle
            plane = self.obj.Geometry.Arc.Plane
            newarc = Arc(plane, self.centerpoint, self.radius, ar)
            sc.doc.Objects.Replace(self.obj.Id, newarc)
            # sc.doc.EndUndoRecord(result)
            sc.doc.Views.Redraw()

        def replace_line():
            # result = sc.doc.BeginUndoRecord("Modify Line")
            newline = Line(self.startpoint, self.endpoint)
            sc.doc.Objects.Replace(self.obj.Id, newline)
            # sc.doc.EndUndoRecord(result)
            sc.doc.Views.Redraw()

        try:
            objs = sc.doc.Objects.GetSelectedObjects(False, False)
            if len(list(objs)) == 0:
                self.arcradius.Enabled = False
                self.arccenter_x.Enabled = False
                self.arccenter_y.Enabled = False
                self.arccenter_z.Enabled = False
                return
            if self.radiuschanged:
                self.radiuschanged = False
                replace_arc()
            if self.arc_X_changed:
                self.arc_X_changed = False
                self.radius = self.obj.Geometry.Arc.Radius
                self.centerpoint[0] = self.arc_X
                replace_arc()
            if self.arc_Y_changed:
                self.arc_Y_changed = False
                self.radius = self.obj.Geometry.Arc.Radius
                self.centerpoint[1] = self.arc_Y
                replace_arc()
            if self.arc_Z_changed:
                self.arc_Z_changed = False
                self.radius = self.obj.Geometry.Arc.Radius
                self.centerpoint[2] = self.arc_Z
                replace_arc()
            # line start and end points changed handling
            if self.start_X_changed:
                self.start_X_changed = False
                self.startpoint[0] = self.start_X
                replace_line()
            if self.start_Y_changed:
                self.start_Y_changed = False
                self.startpoint[1] = self.start_Y
                replace_line()
            if self.start_Z_changed:
                self.start_Z_changed = False
                self.startpoint[2] = self.start_Z
                replace_line()
            if self.end_X_changed:
                self.end_X_changed = False
                self.endpoint[0] = self.end_X
                replace_line()
            if self.end_Y_changed:
                self.end_Y_changed = False
                self.endpoint[1] = self.end_Y
                replace_line()
            if self.end_Z_changed:
                self.end_Z_changed = False
                self.endpoint[2] = self.end_Z
                replace_line()
        except Exception as ex:
            print(ex)

    # Form Closed event handler
    def OnFormClosed(self, sender, e):
        # Remove the events added in the initializer
        self.RemoveEvents()
        # Dispose of the form and remove it from the sticky dictionary
        if sc.sticky.has_key("curve_info_modeless_form"):
            form = sc.sticky["curve_info_modeless_form"]
            if form:
                form.Close()
                form = None
            sc.sticky.pop("curve_info_modeless_form")
        # clean the user text from surfaces


def main():
    # See if the form is already visible
    if "curve_info_modeless_form" in sc.sticky:
        return
    # Create and show form
    form = Dialog()

    try:
        form.Show()
        # Add the form to the sticky dictionary so it
        # survives when the main function ends.
        sc.sticky["curve_info_modeless_form"] = form
    except Exception as ex:
        print(ex)


main()
