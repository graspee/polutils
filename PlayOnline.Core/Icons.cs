using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

namespace PlayOnline.Core {

  public class Icons {

    [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
    private static extern IntPtr LoadLibraryEx(string FileName, IntPtr hFile, ulong Flags);

    private static IntPtr LoadLibrary(string FileName) {
      return Icons.LoadLibraryEx(FileName, IntPtr.Zero, 0x2 /* 0x2 = LOAD_LIBRARY_AS_DATAFILE */);
    }

    [DllImport("kernel32.dll", SetLastError=true)]
    private static extern bool FreeLibrary(IntPtr Library);

    [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
    private static extern IntPtr LoadIcon(IntPtr Library, IntPtr ResourceID);

    private static Icon GetIcon(string FileName, ushort ResourceID) {
    IntPtr hDLL = Icons.LoadLibrary(FileName);
    Icon Result = null;
      if (hDLL != IntPtr.Zero) {
      IntPtr hIcon = Icons.LoadIcon(hDLL, new IntPtr(ResourceID));
	if (hIcon != IntPtr.Zero)
	  Result = Icon.FromHandle(hIcon);
	Icons.FreeLibrary(hDLL);
      }
      return Result;
    }

    private static Icon GetPOLIcon(ushort ResourceID) {
      return Icons.GetIcon(Path.Combine(POL.GetApplicationPath("1000"), "pol.exe"), ResourceID);
    }

    private static Icon GetShell32Icon(ushort ResourceID) {
      return Icons.GetIcon("shell32.dll", ResourceID);
    }

    public static Icon POLViewer    { get { return Icons.GetPOLIcon(3); } }
    public static Icon TetraMaster  { get { return Icons.GetPOLIcon(5); } }

    public static Icon Joystick     { get { return Icons.GetIcon("joy.cpl", 102); } }

    public static Icon AudioFile    { get { return Icons.GetShell32Icon(225); } }
    public static Icon AudioStuff   { get { return Icons.GetShell32Icon(277); } }
    public static Icon AudioFolder  { get { return Icons.GetShell32Icon(237); } }
    public static Icon CheckedPage  { get { return Icons.GetShell32Icon(137); } }
    public static Icon ConfigFile   { get { return Icons.GetShell32Icon(151); } }
    public static Icon DocFolder    { get { return Icons.GetShell32Icon(235); } }
    public static Icon FolderOpen   { get { return Icons.GetShell32Icon(  5); } }
    public static Icon FolderClosed { get { return Icons.GetShell32Icon(  4); } }
    public static Icon People       { get { return Icons.GetShell32Icon(269); } }
    public static Icon TextFile     { get { return Icons.GetShell32Icon(152); } }

  }

}
