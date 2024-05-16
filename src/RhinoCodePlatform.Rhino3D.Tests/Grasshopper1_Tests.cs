using System;
using System.Collections.Generic;

using NUnit.Framework;

using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Execution;
using Rhino.Runtime.Code.Languages;
using Rhino.Runtime.Code.Text;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace RhinoCodePlatform.Rhino3D.Tests
{
    [TestFixture]
    public class Grasshopper1_Tests : ScriptFixture
    {
        const string GHDOC_PARAM = "__ghdoc__";
        static readonly Guid s_assertTrue = new("0890a32c-4e30-4f06-a98f-ed62b45838cf");

        [Test]
        public void TestGH1_Error()
        {
            const string source =
                "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiIHN0YW5kYWxvbmU9InllcyI/Pgo8QXJjaGl2ZSBuYW1lPSJSb290Ij4KICA8IS0tR3Jhc3Nob3BwZXIgYXJjaGl2ZS0tPgogIDwhLS1HcmFzc2hvcHBlciBhbmQgR0hfSU8uZGxsIGFyZSBjb3B5cmlnaHRlZCBieSBSb2JlcnQgTWNOZWVsICYgQXNzb2NpYXRlcy0tPgogIDwhLS1BcmNoaXZlIGdlbmVyYXRlZCBieSBHSF9JTy5kbGwgZmlsZSB1dGlsaXR5IGxpYnJhcnkgezAuMi4wMDAyfS0tPgogIDxpdGVtcyBjb3VudD0iMSI+CiAgICA8aXRlbSBuYW1lPSJBcmNoaXZlVmVyc2lvbiIgdHlwZV9uYW1lPSJnaF92ZXJzaW9uIiB0eXBlX2NvZGU9IjgwIj4KICAgICAgPE1ham9yPjA8L01ham9yPgogICAgICA8TWlub3I+MjwvTWlub3I+CiAgICAgIDxSZXZpc2lvbj4yPC9SZXZpc2lvbj4KICAgIDwvaXRlbT4KICA8L2l0ZW1zPgogIDxjaHVua3MgY291bnQ9IjIiPgogICAgPGNodW5rIG5hbWU9IkRlZmluaXRpb24iPgogICAgICA8aXRlbXMgY291bnQ9IjEiPgogICAgICAgIDxpdGVtIG5hbWU9InBsdWdpbl92ZXJzaW9uIiB0eXBlX25hbWU9ImdoX3ZlcnNpb24iIHR5cGVfY29kZT0iODAiPgogICAgICAgICAgPE1ham9yPjE8L01ham9yPgogICAgICAgICAgPE1pbm9yPjA8L01pbm9yPgogICAgICAgICAgPFJldmlzaW9uPjc8L1JldmlzaW9uPgogICAgICAgIDwvaXRlbT4KICAgICAgPC9pdGVtcz4KICAgICAgPGNodW5rcyBjb3VudD0iNCI+CiAgICAgICAgPGNodW5rIG5hbWU9IkRvY3VtZW50SGVhZGVyIj4KICAgICAgICAgIDxpdGVtcyBjb3VudD0iNSI+CiAgICAgICAgICAgIDxpdGVtIG5hbWU9IkRvY3VtZW50SUQiIHR5cGVfbmFtZT0iZ2hfZ3VpZCIgdHlwZV9jb2RlPSI5Ij5lYTE4MjhhZS0zNGIyLTRmMjUtYjhhOC03NzU3NjdlYjE3OTU8L2l0ZW0+CiAgICAgICAgICAgIDxpdGVtIG5hbWU9IlByZXZpZXciIHR5cGVfbmFtZT0iZ2hfc3RyaW5nIiB0eXBlX2NvZGU9IjEwIj5TaGFkZWQ8L2l0ZW0+CiAgICAgICAgICAgIDxpdGVtIG5hbWU9IlByZXZpZXdNZXNoVHlwZSIgdHlwZV9uYW1lPSJnaF9pbnQzMiIgdHlwZV9jb2RlPSIzIj4xPC9pdGVtPgogICAgICAgICAgICA8aXRlbSBuYW1lPSJQcmV2aWV3Tm9ybWFsIiB0eXBlX25hbWU9ImdoX2RyYXdpbmdfY29sb3IiIHR5cGVfY29kZT0iMzYiPgogICAgICAgICAgICAgIDxBUkdCPjEwMDsxNTA7MDswPC9BUkdCPgogICAgICAgICAgICA8L2l0ZW0+CiAgICAgICAgICAgIDxpdGVtIG5hbWU9IlByZXZpZXdTZWxlY3RlZCIgdHlwZV9uYW1lPSJnaF9kcmF3aW5nX2NvbG9yIiB0eXBlX2NvZGU9IjM2Ij4KICAgICAgICAgICAgICA8QVJHQj4xMDA7MDsxNTA7MDwvQVJHQj4KICAgICAgICAgICAgPC9pdGVtPgogICAgICAgICAgPC9pdGVtcz4KICAgICAgICA8L2NodW5rPgogICAgICAgIDxjaHVuayBuYW1lPSJEZWZpbml0aW9uUHJvcGVydGllcyI+CiAgICAgICAgICA8aXRlbXMgY291bnQ9IjQiPgogICAgICAgICAgICA8aXRlbSBuYW1lPSJEYXRlIiB0eXBlX25hbWU9ImdoX2RhdGUiIHR5cGVfY29kZT0iOCI+NjM4MzAyOTMyODg0NDY5NDg1PC9pdGVtPgogICAgICAgICAgICA8aXRlbSBuYW1lPSJEZXNjcmlwdGlvbiIgdHlwZV9uYW1lPSJnaF9zdHJpbmciIHR5cGVfY29kZT0iMTAiPjwvaXRlbT4KICAgICAgICAgICAgPGl0ZW0gbmFtZT0iS2VlcE9wZW4iIHR5cGVfbmFtZT0iZ2hfYm9vbCIgdHlwZV9jb2RlPSIxIj5mYWxzZTwvaXRlbT4KICAgICAgICAgICAgPGl0ZW0gbmFtZT0iTmFtZSIgdHlwZV9uYW1lPSJnaF9zdHJpbmciIHR5cGVfY29kZT0iMTAiPnRlc3Rfc2VsZl9lcnJvci5naHg8L2l0ZW0+CiAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgPGNodW5rcyBjb3VudD0iMyI+CiAgICAgICAgICAgIDxjaHVuayBuYW1lPSJSZXZpc2lvbnMiPgogICAgICAgICAgICAgIDxpdGVtcyBjb3VudD0iMSI+CiAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJSZXZpc2lvbkNvdW50IiB0eXBlX25hbWU9ImdoX2ludDMyIiB0eXBlX2NvZGU9IjMiPjA8L2l0ZW0+CiAgICAgICAgICAgICAgPC9pdGVtcz4KICAgICAgICAgICAgPC9jaHVuaz4KICAgICAgICAgICAgPGNodW5rIG5hbWU9IlByb2plY3Rpb24iPgogICAgICAgICAgICAgIDxpdGVtcyBjb3VudD0iMiI+CiAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJUYXJnZXQiIHR5cGVfbmFtZT0iZ2hfZHJhd2luZ19wb2ludCIgdHlwZV9jb2RlPSIzMCI+CiAgICAgICAgICAgICAgICAgIDxYPjExMzwvWD4KICAgICAgICAgICAgICAgICAgPFk+Nzg8L1k+CiAgICAgICAgICAgICAgICA8L2l0ZW0+CiAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJab29tIiB0eXBlX25hbWU9ImdoX3NpbmdsZSIgdHlwZV9jb2RlPSI1Ij4xLjYyODMzMjc8L2l0ZW0+CiAgICAgICAgICAgICAgPC9pdGVtcz4KICAgICAgICAgICAgPC9jaHVuaz4KICAgICAgICAgICAgPGNodW5rIG5hbWU9IlZpZXdzIj4KICAgICAgICAgICAgICA8aXRlbXMgY291bnQ9IjEiPgogICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iVmlld0NvdW50IiB0eXBlX25hbWU9ImdoX2ludDMyIiB0eXBlX2NvZGU9IjMiPjA8L2l0ZW0+CiAgICAgICAgICAgICAgPC9pdGVtcz4KICAgICAgICAgICAgPC9jaHVuaz4KICAgICAgICAgIDwvY2h1bmtzPgogICAgICAgIDwvY2h1bms+CiAgICAgICAgPGNodW5rIG5hbWU9IlJjcExheW91dCI+CiAgICAgICAgICA8aXRlbXMgY291bnQ9IjEiPgogICAgICAgICAgICA8aXRlbSBuYW1lPSJHcm91cENvdW50IiB0eXBlX25hbWU9ImdoX2ludDMyIiB0eXBlX2NvZGU9IjMiPjA8L2l0ZW0+CiAgICAgICAgICA8L2l0ZW1zPgogICAgICAgIDwvY2h1bms+CiAgICAgICAgPGNodW5rIG5hbWU9IkRlZmluaXRpb25PYmplY3RzIj4KICAgICAgICAgIDxpdGVtcyBjb3VudD0iMSI+CiAgICAgICAgICAgIDxpdGVtIG5hbWU9Ik9iamVjdENvdW50IiB0eXBlX25hbWU9ImdoX2ludDMyIiB0eXBlX2NvZGU9IjMiPjI8L2l0ZW0+CiAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgPGNodW5rcyBjb3VudD0iMiI+CiAgICAgICAgICAgIDxjaHVuayBuYW1lPSJPYmplY3QiIGluZGV4PSIwIj4KICAgICAgICAgICAgICA8aXRlbXMgY291bnQ9IjIiPgogICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iR1VJRCIgdHlwZV9uYW1lPSJnaF9ndWlkIiB0eXBlX2NvZGU9IjkiPmZiYWMzZTMyLWYxMDAtNDI5Mi04NjkyLTc3MjQwYTQyZmQxYTwvaXRlbT4KICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9Ik5hbWUiIHR5cGVfbmFtZT0iZ2hfc3RyaW5nIiB0eXBlX2NvZGU9IjEwIj5Qb2ludDwvaXRlbT4KICAgICAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgICAgIDxjaHVua3MgY291bnQ9IjEiPgogICAgICAgICAgICAgICAgPGNodW5rIG5hbWU9IkNvbnRhaW5lciI+CiAgICAgICAgICAgICAgICAgIDxpdGVtcyBjb3VudD0iNyI+CiAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iRGVzY3JpcHRpb24iIHR5cGVfbmFtZT0iZ2hfc3RyaW5nIiB0eXBlX2NvZGU9IjEwIj5Db250YWlucyBhIGNvbGxlY3Rpb24gb2YgdGhyZWUtZGltZW5zaW9uYWwgcG9pbnRzPC9pdGVtPgogICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9Ikluc3RhbmNlR3VpZCIgdHlwZV9uYW1lPSJnaF9ndWlkIiB0eXBlX2NvZGU9IjkiPjU5N2FmZDcwLTFiNTktNDg0Zi05ODlmLTExYmYyNDk3ZmFlMDwvaXRlbT4KICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJOYW1lIiB0eXBlX25hbWU9ImdoX3N0cmluZyIgdHlwZV9jb2RlPSIxMCI+UG9pbnQ8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iTmlja05hbWUiIHR5cGVfbmFtZT0iZ2hfc3RyaW5nIiB0eXBlX2NvZGU9IjEwIj5QdDwvaXRlbT4KICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJPcHRpb25hbCIgdHlwZV9uYW1lPSJnaF9ib29sIiB0eXBlX2NvZGU9IjEiPmZhbHNlPC9pdGVtPgogICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9IlNvdXJjZSIgaW5kZXg9IjAiIHR5cGVfbmFtZT0iZ2hfZ3VpZCIgdHlwZV9jb2RlPSI5Ij40ZjFiZTNjNy1jYWZiLTRlMWUtODdiOC1iMWNjZTNjNTU1NWM8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iU291cmNlQ291bnQiIHR5cGVfbmFtZT0iZ2hfaW50MzIiIHR5cGVfY29kZT0iMyI+MTwvaXRlbT4KICAgICAgICAgICAgICAgICAgPC9pdGVtcz4KICAgICAgICAgICAgICAgICAgPGNodW5rcyBjb3VudD0iMSI+CiAgICAgICAgICAgICAgICAgICAgPGNodW5rIG5hbWU9IkF0dHJpYnV0ZXMiPgogICAgICAgICAgICAgICAgICAgICAgPGl0ZW1zIGNvdW50PSIyIj4KICAgICAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iQm91bmRzIiB0eXBlX25hbWU9ImdoX2RyYXdpbmdfcmVjdGFuZ2xlZiIgdHlwZV9jb2RlPSIzNSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgPFg+MTQ1PC9YPgogICAgICAgICAgICAgICAgICAgICAgICAgIDxZPjUyPC9ZPgogICAgICAgICAgICAgICAgICAgICAgICAgIDxXPjUwPC9XPgogICAgICAgICAgICAgICAgICAgICAgICAgIDxIPjIwPC9IPgogICAgICAgICAgICAgICAgICAgICAgICA8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9IlBpdm90IiB0eXBlX25hbWU9ImdoX2RyYXdpbmdfcG9pbnRmIiB0eXBlX2NvZGU9IjMxIj4KICAgICAgICAgICAgICAgICAgICAgICAgICA8WD4xNzAuMDgzMjU8L1g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgPFk+NjIuMjIzMTE4PC9ZPgogICAgICAgICAgICAgICAgICAgICAgICA8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgICAgICAgICAgIDwvY2h1bms+CiAgICAgICAgICAgICAgICAgIDwvY2h1bmtzPgogICAgICAgICAgICAgICAgPC9jaHVuaz4KICAgICAgICAgICAgICA8L2NodW5rcz4KICAgICAgICAgICAgPC9jaHVuaz4KICAgICAgICAgICAgPGNodW5rIG5hbWU9Ik9iamVjdCIgaW5kZXg9IjEiPgogICAgICAgICAgICAgIDxpdGVtcyBjb3VudD0iMiI+CiAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJHVUlEIiB0eXBlX25hbWU9ImdoX2d1aWQiIHR5cGVfY29kZT0iOSI+Y2I5NWRiODktNjE2NS00M2I2LTljNDEtNTcwMmJjNWJmMTM3PC9pdGVtPgogICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iTmFtZSIgdHlwZV9uYW1lPSJnaF9zdHJpbmciIHR5cGVfY29kZT0iMTAiPkJvb2xlYW48L2l0ZW0+CiAgICAgICAgICAgICAgPC9pdGVtcz4KICAgICAgICAgICAgICA8Y2h1bmtzIGNvdW50PSIxIj4KICAgICAgICAgICAgICAgIDxjaHVuayBuYW1lPSJDb250YWluZXIiPgogICAgICAgICAgICAgICAgICA8aXRlbXMgY291bnQ9IjYiPgogICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9IkRlc2NyaXB0aW9uIiB0eXBlX25hbWU9ImdoX3N0cmluZyIgdHlwZV9jb2RlPSIxMCI+Q29udGFpbnMgYSBjb2xsZWN0aW9uIG9mIGJvb2xlYW4gdmFsdWVzPC9pdGVtPgogICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9Ikluc3RhbmNlR3VpZCIgdHlwZV9uYW1lPSJnaF9ndWlkIiB0eXBlX2NvZGU9IjkiPjRmMWJlM2M3LWNhZmItNGUxZS04N2I4LWIxY2NlM2M1NTU1YzwvaXRlbT4KICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJOYW1lIiB0eXBlX25hbWU9ImdoX3N0cmluZyIgdHlwZV9jb2RlPSIxMCI+Qm9vbGVhbjwvaXRlbT4KICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJOaWNrTmFtZSIgdHlwZV9uYW1lPSJnaF9zdHJpbmciIHR5cGVfY29kZT0iMTAiPkJvb2w8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iT3B0aW9uYWwiIHR5cGVfbmFtZT0iZ2hfYm9vbCIgdHlwZV9jb2RlPSIxIj5mYWxzZTwvaXRlbT4KICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJTb3VyY2VDb3VudCIgdHlwZV9uYW1lPSJnaF9pbnQzMiIgdHlwZV9jb2RlPSIzIj4wPC9pdGVtPgogICAgICAgICAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgICAgICAgICA8Y2h1bmtzIGNvdW50PSIyIj4KICAgICAgICAgICAgICAgICAgICA8Y2h1bmsgbmFtZT0iQXR0cmlidXRlcyI+CiAgICAgICAgICAgICAgICAgICAgICA8aXRlbXMgY291bnQ9IjIiPgogICAgICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJCb3VuZHMiIHR5cGVfbmFtZT0iZ2hfZHJhd2luZ19yZWN0YW5nbGVmIiB0eXBlX2NvZGU9IjM1Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICA8WD42MTwvWD4KICAgICAgICAgICAgICAgICAgICAgICAgICA8WT41MjwvWT4KICAgICAgICAgICAgICAgICAgICAgICAgICA8Vz41MDwvVz4KICAgICAgICAgICAgICAgICAgICAgICAgICA8SD4yMDwvSD4KICAgICAgICAgICAgICAgICAgICAgICAgPC9pdGVtPgogICAgICAgICAgICAgICAgICAgICAgICA8aXRlbSBuYW1lPSJQaXZvdCIgdHlwZV9uYW1lPSJnaF9kcmF3aW5nX3BvaW50ZiIgdHlwZV9jb2RlPSIzMSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgPFg+ODYuNDgzMjQ2PC9YPgogICAgICAgICAgICAgICAgICAgICAgICAgIDxZPjYyLjI3MzEyPC9ZPgogICAgICAgICAgICAgICAgICAgICAgICA8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgICAgICAgICAgIDwvY2h1bms+CiAgICAgICAgICAgICAgICAgICAgPGNodW5rIG5hbWU9IlBlcnNpc3RlbnREYXRhIj4KICAgICAgICAgICAgICAgICAgICAgIDxpdGVtcyBjb3VudD0iMSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9IkNvdW50IiB0eXBlX25hbWU9ImdoX2ludDMyIiB0eXBlX2NvZGU9IjMiPjE8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgICA8L2l0ZW1zPgogICAgICAgICAgICAgICAgICAgICAgPGNodW5rcyBjb3VudD0iMSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDxjaHVuayBuYW1lPSJCcmFuY2giIGluZGV4PSIwIj4KICAgICAgICAgICAgICAgICAgICAgICAgICA8aXRlbXMgY291bnQ9IjIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iQ291bnQiIHR5cGVfbmFtZT0iZ2hfaW50MzIiIHR5cGVfY29kZT0iMyI+MTwvaXRlbT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxpdGVtIG5hbWU9IlBhdGgiIHR5cGVfbmFtZT0iZ2hfc3RyaW5nIiB0eXBlX2NvZGU9IjEwIj57MH08L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgICAgICAgPC9pdGVtcz4KICAgICAgICAgICAgICAgICAgICAgICAgICA8Y2h1bmtzIGNvdW50PSIxIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxjaHVuayBuYW1lPSJJdGVtIiBpbmRleD0iMCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxpdGVtcyBjb3VudD0iMSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGl0ZW0gbmFtZT0iYm9vbGVhbiIgdHlwZV9uYW1lPSJnaF9ib29sIiB0eXBlX2NvZGU9IjEiPnRydWU8L2l0ZW0+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvaXRlbXM+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2NodW5rPgogICAgICAgICAgICAgICAgICAgICAgICAgIDwvY2h1bmtzPgogICAgICAgICAgICAgICAgICAgICAgICA8L2NodW5rPgogICAgICAgICAgICAgICAgICAgICAgPC9jaHVua3M+CiAgICAgICAgICAgICAgICAgICAgPC9jaHVuaz4KICAgICAgICAgICAgICAgICAgPC9jaHVua3M+CiAgICAgICAgICAgICAgICA8L2NodW5rPgogICAgICAgICAgICAgIDwvY2h1bmtzPgogICAgICAgICAgICA8L2NodW5rPgogICAgICAgICAgPC9jaHVua3M+CiAgICAgICAgPC9jaHVuaz4KICAgICAgPC9jaHVua3M+CiAgICA8L2NodW5rPgogICAgPGNodW5rIG5hbWU9IlRodW1ibmFpbCI+CiAgICAgIDxpdGVtcyBjb3VudD0iMSI+CiAgICAgICAgPGl0ZW0gbmFtZT0iVGh1bWJuYWlsIiB0eXBlX25hbWU9ImdoX2RyYXdpbmdfYml0bWFwIiB0eXBlX2NvZGU9IjM3Ij4KICAgICAgICAgIDxiaXRtYXAgbGVuZ3RoPSIxMzM1Ij5pVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBSllBQUFCa0NBSUFBQURyT1Y2bkFBQUFBWE5TUjBJQXJzNGM2UUFBQUFSblFVMUJBQUN4and2OFlRVUFBQUFKY0VoWmN3QUFEc01BQUE3REFjZHZxR1FBQUFUTVNVUkJWSGhlN2RmZFMxdDNITWZ4dENTOTZIMTdXK2lEdmVrL1VlaVZiV0hRQ3k5NmEyOG1CV09sYkZRR2dpMVNYUjdGa0VhcVlwN3NzTG82WjBGMGJsSHluQ2E2bUJpVGFGTVQ4MlFleU54MnRlMmI1S3p6NkhSdTV1WTdQaTkrL0RqbmwzTU93YmM1T1pIQS84SHZ3TlpmQ1Q4QVEwaklIaEt5aDRUc0lTRjdTTWdlRXJLSGhPd2hJWHRJeUI0U3NvZUU3Q0VoZTBqSUhoS3loNFRzSVNGN1NNZ2VFcktIaE95SkVtNXZid3ZMTlFkMi81V1A1NTdtSW5BU0J4TW1rOGwwT3IxVGswZ2s2dHMwRTNwSk9PbWYxTFBSdGRMWkxGMGttVXFsTTVuVXprNzlWV2dzVWNLSmlZbTV1Ym5WR3AvUHQ3Q3c0UEY0QW9FQWJidGNybzJOalJOV3BIaXpGc3Y4NjlkQnY5L3Zjbm1XbGp3MjI5TGJ0NmwwV2pnQ0drZVUwR0F3akkyTldTd1dvOUZvdFZxSGhvWkdSMGRwZDJSa2hGYW9ZaWFURWM0N0dtV09ySzE5WXpSK056MzlsVTVINCt2aFlhdFdhNXVkemVSeXdrSFFPS0tFZ1lDZlBtcWhVQ2hjRXd3R285SG9XazA4SHFmYjQvRUo2M2ZPZXFmcUhUbVZXZ3NFMW9QQkdGMDBITjdhM016azgzVEF6Z24rRCtEa1JBbTlYby9iN2ZaVXA2cmFob3RtR3ZRUnROdnRYcS8zcUhzcHJXOXViVG1YbDJtNDdYYTN3MEd6eiszMjBvM1U2YXpPRGtmOVZhL1RpV2VjQmhJbHJQM1pxZGs3dDl0SEd3NEhaYU4yUHFmVFU5dndUazI5b1crMzNkMWRla2dSTGxCRHU4Vks1ZnZ4OFhjR1E5aHNEcHRNSVpPSjVyalZHck5ZMXMzbStpNk5kYXQxV2F2MUxpNFd5dVVERjRIL1JwUXdITjZNUk40dkx0TG56VWNia1VpQ2hzUGhDd1JDNit0YjBlZzJiWGQwUElyRll1VnlPYi9QM3Q3ZTFNek1sNjJ0dituMWUycjFua2J6czBiemsxb2Q3dTZPUFh1V1Z5aCswV3Bwa2NhdkdrMVJyZTU2OE1DN3NrSlB1Y0s3Z0ZNUUpUU1pwc2ZIWjl2YU90dmFIajE5cXVydTd1dnBVY3Jsbjc5NFliRmF2N1ZZWm96R3FaNmVucGFXRnAxT3AveVRTcVhxN2UzOTVQNzlOKzN0OGN1WG85ZXVSWnVhNGsxTnExZXY5bDI0MEhmeDR2U2xTOG5yMTJteHVuN2xTdmpHRFpWYzN2bmtTYWxVRXQ0Rm5JSW80Y0NBVWFjemQzUjg4ZkRoWnpUTDVWMDB0N2QzS1pVdkJ3ZE5nNE5taldha3RiVzF2Ny9mWnJQUno0K1A1dWZuUDMzOFdIbnpadmpNbVJXcGRFVW1XNVhKWEZMcHBFejJ3N2x6TTFKcFNDYWpSUm8vbmozclAzOWVmdmV1U3E4dkZvdkN1NEJURUNWVUtGN1doMW85cWxBTTErYjZ0ckRZMWRXbjErc3JsVW91bDh2dVV5Z1VZdG5zNEsxYlBvbGsvd2hJSlA1REs0c1NpYUd6TTF1cDRLR21JVVFKKy92VnRhRTVOS3JyejUrcmxNcUJqWTNJNGU4d2VqREpsVXIyeVVsVGMvT3IyN2RmM2JsejVHaHVOdCs3dCtiMTRqZGlvNGdTSnBQMGMrN0lRWitaVENhZFNxWCsva2t5a2RqSjV4T2wwdnRpOFppeFZTaHNsOHVwVEFhUG80MGlTa2lWamllY2RBemh3T01JUjBLRGlCSUthOEFLRXJLSGhPd2hJWHRJeUI0U3NvZUU3Q0VoZTBqSUhoS3loNFRzSVNGN1NNZ2VFcktIaE93aElYdEl5QjRTc29lRTdDRWhlMGpJSGhLeWg0VHNJU0Y3U01nZUVyS0hoT3doSVh0SXlCNFNzb2VFN0NFaGUwaklIaEt5aDRUc0lTRjdTTWdlRXJLSGhPd2hJWHRJeUI0U3NvZUU3Q0VoZTBqSUhoS3loNFRzSVNGN1NNZ2VFcktIaE93aElYdEl5QjRTc29lRTdDRWhlMGpJSGhLeWg0VHNJU0Y3U01nZUVySW5TZ2hNQ1FtQk1ZbmtENkozaTlNL2dUSTlBQUFBQUVsRlRrU3VRbUNDPC9iaXRtYXA+CiAgICAgICAgPC9pdGVtPgogICAgICA8L2l0ZW1zPgogICAgPC9jaHVuaz4KICA8L2NodW5rcz4KPC9BcmNoaXZlPg==";

            Code code = GetGrasshopper().CreateCode(source);

            var ctx = new RunContext
            {
                Options = {
                    ["grasshopper.runAsCommand"] = false
                }
            };

            try
            {
                code.Run(ctx);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message.StartsWith("Data conversion failed from Boolean to Point"));
                return;
            }

            throw new Exception("Failed collecting erorr from Grasshopper definition");
        }

        [Test, TestCaseSource(nameof(GetTestDefinitions))]
        public void TestGH1_Script(ScriptInfo scriptInfo)
        {
            TestSkip(scriptInfo);

            Code code = GetGrasshopper().CreateCode(scriptInfo.Uri);

            var ctx = new RunContext
            {
                AutoApplyParams = true,
                Outputs = {
                    ["result"] = default,
                },
                Options = {
                    ["grasshopper.runner.asCommand"] = false,
                    ["grasshopper.runner.extractDoc"] = GHDOC_PARAM,
                }
            };

            if (scriptInfo.ExpectsWarning)
            {
                ctx.Options["grasshopper.runner.warningsAreErrors"] = true;
            }

            if (scriptInfo.IsProfileTest)
            {
                ctx.Options["grasshopper.runner.expireAll"] = true;
            }

            if (TryRunCode(scriptInfo, code, ctx, out string errorMessage))
            {
                // NOTE:
                // definition with no errors, either need to have a 'result' collector
                if (ctx.Outputs.TryGet("result", out IGH_Structure data))
                {
                    foreach (GH_Path p in data.Paths)
                    {
                        foreach (var d in data.get_Branch(p))
                            if (d is GH_Boolean result)
                                Assert.True(result.Value);
                    }
                }
                // or have at least one assert component
                else
                {
                    bool hasAssertComponent = false;
                    GH_Document ghdoc = ctx.Outputs.Get<GH_Document>(GHDOC_PARAM);
                    foreach (IGH_DocumentObject docObj in ghdoc.Objects)
                    {
                        hasAssertComponent |= docObj.ComponentGuid == s_assertTrue;
                    }

                    Assert.True(hasAssertComponent);
                }
            }
            else
            {
                foreach (var line in errorMessage.ToLinesLazy())
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    Assert.True(scriptInfo.MatchesError(line));
                }
            }

        }

        ILanguage GetGrasshopper() => GetLanguage(this, new LanguageSpec("*.*.grasshopper", "1"));

        static IEnumerable<object[]> GetTestDefinitions() => GetTestScripts(@"gh1\", "test_*.gh?");
    }
}
