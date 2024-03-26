using System;
using System.Xml.Serialization;

namespace RhinoCodePlatform.Rhino3D.Tests
{
    [Serializable]
    [XmlRoot("Settings")]
    public sealed class MxTestSettings
    {
        [XmlElement]
        public string TestFilesDirectory { get; set; } = string.Empty;
    }
}
