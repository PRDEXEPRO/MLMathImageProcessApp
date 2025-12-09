using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace MLMathImageApp.UI;

/// <summary>
/// Modern Windows COM API kullanarak klasör seçici - FolderBrowserDialog sorunlarını çözmek için
/// </summary>
internal static class FolderBrowserHelper
{
    [ComImport]
    [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        [PreserveSig]
        int Show(IntPtr hwnd);

        void SetFileTypes(uint cFileTypes, [MarshalAs(UnmanagedType.LPArray)] IntPtr rgFilterSpec);
        void SetFileTypeIndex(uint iFileType);
        void GetFileTypeIndex(out uint piFileType);
        void Advise(IntPtr pfde, out uint pdwCookie);
        void Unadvise(uint dwCookie);
        void SetOptions(uint fos);
        void GetOptions(out uint pfos);
        void SetDefaultFolder(IntPtr psi);
        void SetFolder(IntPtr psi);
        void GetFolder(out IntPtr ppsi);
        void GetCurrentSelection(out IntPtr ppsi);
        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        void GetResult(out IntPtr ppsi);
        void AddPlace(IntPtr psi, uint fdap);
        void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        void Close([MarshalAs(UnmanagedType.Error)] int hr);
        void SetClientGuid(ref Guid guid);
        void ClearClientData();
        void SetFilter(IntPtr pFilter);
        void GetResults(out IntPtr ppenum);
        void GetSelectedItems(out IntPtr ppsai);
    }

    [ComImport]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    [ClassInterface(ClassInterfaceType.None)]
    private class FileOpenDialogRCW
    {
    }

    private const uint FOS_PICKFOLDERS = 0x20;
    private const uint FOS_FORCEFILESYSTEM = 0x40;

    public static string? BrowseForFolder(IWin32Window owner, string title, string initialPath = "")
    {
        try
        {
            var dialog = (IFileOpenDialog)new FileOpenDialogRCW();
            dialog.SetOptions(FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);
            dialog.SetTitle(title);

            if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
            {
                var pidl = GetPidlFromPath(initialPath);
                if (pidl != IntPtr.Zero)
                {
                    dialog.SetFolder(pidl);
                    Marshal.FreeCoTaskMem(pidl);
                }
            }

            var hwnd = owner?.Handle ?? IntPtr.Zero;
            var result = dialog.Show(hwnd);

            if (result == 0) // S_OK
            {
                dialog.GetResult(out var ppsi);
                if (ppsi != IntPtr.Zero)
                {
                    var path = GetPathFromPidl(ppsi);
                    Marshal.Release(ppsi);
                    return path;
                }
            }

            return null;
        }
        catch
        {
            // Fallback to old API if COM fails
            return BrowseForFolderLegacy(owner, title, initialPath);
        }
    }

    private static string? BrowseForFolderLegacy(IWin32Window owner, string title, string initialPath)
    {
        try
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = title,
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true
            };

            if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
            {
                dialog.SelectedPath = initialPath;
            }

            if (dialog.ShowDialog(owner) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return dialog.SelectedPath;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] System.Text.StringBuilder pszPath);

    private static IntPtr GetPidlFromPath(string path)
    {
        return ILCreateFromPath(path);
    }

    private static string? GetPathFromPidl(IntPtr pidl)
    {
        var path = new System.Text.StringBuilder(260);
        if (SHGetPathFromIDList(pidl, path))
        {
            return path.ToString();
        }
        return null;
    }
}

