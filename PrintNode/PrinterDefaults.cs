using System.Runtime.InteropServices;

namespace PrintNode
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct PrinterDefaults
    {
        public IntPtr pDatatype;     // LPTSTR
        public IntPtr pDevMode;      // LPDEVMODE
        public int DesiredAccess; // ACCESS_MASK
    };
}
