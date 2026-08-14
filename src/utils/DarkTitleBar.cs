using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace bdmanager {
  internal static class DarkTitleBar {
    private const int UseImmersiveDarkMode = 20;
    private const int UseImmersiveDarkModeBefore20H1 = 19;
    private const int BorderColor = 34;
    private const int CaptionColor = 35;
    private const int TextColor = 36;
    private const int SystemBackdropType = 38;
    private const int MicaEffect = 1029;

    private const int DarkCaptionColor = 0x001E1E1E;
    private const int DarkCaptionTextColor = 0x00F2F2F2;
    private const int DarkCaptionBorderColor = 0x00303030;

    public static void Apply(Window window) {
      if (window == null) return;
      window.SourceInitialized += (sender, args) => ApplyToHandle(new WindowInteropHelper(window).Handle);
    }

    private static void ApplyToHandle(IntPtr handle) {
      if (handle == IntPtr.Zero) return;

      try {
        int enabled = 1;
        int result = DwmSetWindowAttribute(handle, UseImmersiveDarkMode, ref enabled, sizeof(int));
        if (result != 0) {
          DwmSetWindowAttribute(handle, UseImmersiveDarkModeBefore20H1, ref enabled, sizeof(int));
        }

        int disabled = 0;
        int noSystemBackdrop = 1;
        int captionColor = DarkCaptionColor;
        int textColor = DarkCaptionTextColor;
        int borderColor = DarkCaptionBorderColor;

        DwmSetWindowAttribute(handle, MicaEffect, ref disabled, sizeof(int));
        DwmSetWindowAttribute(handle, SystemBackdropType, ref noSystemBackdrop, sizeof(int));
        DwmSetWindowAttribute(handle, CaptionColor, ref captionColor, sizeof(int));
        DwmSetWindowAttribute(handle, TextColor, ref textColor, sizeof(int));
        DwmSetWindowAttribute(handle, BorderColor, ref borderColor, sizeof(int));
      }
      catch (DllNotFoundException) {
      }
      catch (EntryPointNotFoundException) {
      }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
      IntPtr windowHandle,
      int attribute,
      ref int attributeValue,
      int attributeSize);
  }
}
