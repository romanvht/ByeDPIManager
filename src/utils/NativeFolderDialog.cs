using System;
using System.IO;
using System.Runtime.InteropServices;

namespace bdmanager {
  internal static class NativeFolderDialog {
    private const int ErrorCancelled = unchecked((int)0x800704C7);
    private static readonly Guid ShellItemGuid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");

    public static bool TryShow(IntPtr ownerHandle, string title, string initialPath, out string selectedPath) {
      selectedPath = null;
      object dialogObject = new FileOpenDialog();
      IShellItem result = null;

      try {
        IFileDialog dialog = (IFileDialog)dialogObject;
        dialog.GetOptions(out FileOpenOptions options);
        dialog.SetOptions(options | FileOpenOptions.PickFolders | FileOpenOptions.ForceFileSystem |
          FileOpenOptions.PathMustExist);
        dialog.SetTitle(title ?? string.Empty);
        SetInitialFolder(dialog, initialPath);

        int showResult = dialog.Show(ownerHandle);
        if (showResult == ErrorCancelled) return false;
        Marshal.ThrowExceptionForHR(showResult);

        dialog.GetResult(out result);
        result.GetDisplayName(DisplayName.FileSystemPath, out IntPtr pathPointer);
        try {
          selectedPath = Marshal.PtrToStringUni(pathPointer);
        }
        finally {
          Marshal.FreeCoTaskMem(pathPointer);
        }

        return !string.IsNullOrWhiteSpace(selectedPath);
      }
      finally {
        if (result != null) Marshal.FinalReleaseComObject(result);
        Marshal.FinalReleaseComObject(dialogObject);
      }
    }

    private static void SetInitialFolder(IFileDialog dialog, string initialPath) {
      string folderPath = null;
      if (Directory.Exists(initialPath)) folderPath = initialPath;
      else if (File.Exists(initialPath)) folderPath = Path.GetDirectoryName(initialPath);
      if (string.IsNullOrWhiteSpace(folderPath)) return;

      IShellItem folder = null;
      try {
        Guid shellItemGuid = ShellItemGuid;
        if (SHCreateItemFromParsingName(folderPath, IntPtr.Zero, ref shellItemGuid, out folder) >= 0) {
          dialog.SetFolder(folder);
        }
      }
      finally {
        if (folder != null) Marshal.FinalReleaseComObject(folder);
      }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHCreateItemFromParsingName(
      [MarshalAs(UnmanagedType.LPWStr)] string path,
      IntPtr bindingContext,
      ref Guid interfaceId,
      [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

    [ComImport]
    [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
    private class FileOpenDialog {
    }

    [ComImport]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileDialog {
      [PreserveSig]
      int Show(IntPtr owner);

      void SetFileTypes(uint fileTypeCount, IntPtr filterSpecifications);
      void SetFileTypeIndex(uint fileTypeIndex);
      void GetFileTypeIndex(out uint fileTypeIndex);
      void Advise(IntPtr events, out uint cookie);
      void Unadvise(uint cookie);
      void SetOptions(FileOpenOptions options);
      void GetOptions(out FileOpenOptions options);
      void SetDefaultFolder(IShellItem shellItem);
      void SetFolder(IShellItem shellItem);
      void GetFolder(out IShellItem shellItem);
      void GetCurrentSelection(out IShellItem shellItem);
      void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
      void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
      void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
      void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
      void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
      void GetResult(out IShellItem shellItem);
      void AddPlace(IShellItem shellItem, int alignment);
      void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);
      void Close(int result);
      void SetClientGuid(ref Guid clientGuid);
      void ClearClientData();
      void SetFilter(IntPtr filter);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem {
      void BindToHandler(IntPtr bindingContext, ref Guid handlerId, ref Guid interfaceId, out IntPtr result);
      void GetParent(out IShellItem shellItem);
      void GetDisplayName(DisplayName displayName, out IntPtr name);
      void GetAttributes(uint mask, out uint attributes);
      void Compare(IShellItem shellItem, uint hint, out int order);
    }

    [Flags]
    private enum FileOpenOptions : uint {
      PickFolders = 0x00000020,
      ForceFileSystem = 0x00000040,
      PathMustExist = 0x00000800
    }

    private enum DisplayName : uint {
      FileSystemPath = 0x80058000
    }
  }
}
