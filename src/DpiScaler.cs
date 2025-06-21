using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace bdmanager {
  public static class DpiScaler {
    private static readonly HashSet<string> _scaledForms = new HashSet<string>();

    public static void ScaleForm(Form form) {
      string formId = $"{form.Name}_{form.GetHashCode()}";

      if (_scaledForms.Contains(formId))
        return;

      float scaleFactor;
      using (Graphics g = form.CreateGraphics()) {
        scaleFactor = g.DpiX / 96f;
      }

      if (Math.Abs(scaleFactor - 1.0f) < 0.01f)
        return;

      form.AutoScaleMode = AutoScaleMode.None;

      form.SuspendLayout();

      try {
        if (form.FormBorderStyle != FormBorderStyle.None) {
          Size originalSize = form.Size;
          Size newSize = new Size(
              (int)(originalSize.Width * scaleFactor),
              (int)(originalSize.Height * scaleFactor)
          );
          form.Size = newSize;
        }

        ScaleControlAndChildren(form, scaleFactor);

        _scaledForms.Add(formId);
      }
      finally {
        form.ResumeLayout(true);
      }
    }

    private static void ScaleControlAndChildren(Control control, float scaleFactor) {
      if (control is ToolStrip || control is MenuStrip || control is StatusStrip)
        return;

      if (control is RoundButton roundButton) {
        roundButton.BorderRadius = (int)(roundButton.BorderRadius * scaleFactor);
        roundButton.BorderSize = Math.Max(1, (int)(roundButton.BorderSize * scaleFactor));
      }

      if (!(control is Form) && control.Dock == DockStyle.None) {
        control.Size = new Size(
            (int)(control.Size.Width * scaleFactor),
            (int)(control.Size.Height * scaleFactor)
        );
      }

      if (!control.MinimumSize.IsEmpty) {
        control.MinimumSize = new Size(
            (int)(control.MinimumSize.Width * scaleFactor),
            (int)(control.MinimumSize.Height * scaleFactor)
        );
      }

      if (control is TableLayoutPanel tablePanel) {
        foreach (ColumnStyle columnStyle in tablePanel.ColumnStyles) {
          if (columnStyle.SizeType == SizeType.Absolute) {
            columnStyle.Width = columnStyle.Width * scaleFactor;
          }
        }

        foreach (RowStyle rowStyle in tablePanel.RowStyles) {
          if (rowStyle.SizeType == SizeType.Absolute) {
            rowStyle.Height = rowStyle.Height * scaleFactor;
          }
        }
      }

      if (control is TextBoxBase textBox) {
        if (textBox.Multiline) {
          if (textBox.Dock == DockStyle.None) {
            textBox.Size = new Size(
                (int)(textBox.Size.Width * scaleFactor),
                (int)(textBox.Size.Height * scaleFactor)
            );
          }
        }
      }

      foreach (Control child in control.Controls) {
        ScaleControlAndChildren(child, scaleFactor);
      }
    }
  }
}
