// Guids.cs
// MUST match guids.h
using System;

namespace DualNotion.VegaChartWindow
{
    static class GuidList
    {
        public const string guidVegaChartWindowPkgString = "7f9d4220-19d3-4869-bcc6-4f356b1248c8";
        public const string guidVegaChartWindowCmdSetString = "30ef6cf8-4d5e-4371-a010-e4f74d60584f";
        public const string guidToolWindowPersistanceString = "f1a825e5-9dce-4854-8d1a-fe694ad83a70";

        public static readonly Guid guidVegaChartWindowCmdSet = new Guid(guidVegaChartWindowCmdSetString);
    };
}