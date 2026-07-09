using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace bdmanager {
  public class RoundedPanel : Panel {
    public int BorderRadius { get; set; } = 8;
    public Color BorderColor { get; set; } = Color.FromArgb(100, 100, 100);

    public RoundedPanel() {
      DoubleBuffered = true;
    }

    protected override void OnSizeChanged(EventArgs e) {
      base.OnSizeChanged(e);
      ApplyRoundedRegion();
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);

      Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
      using (GraphicsPath path = GetRoundedPath(bounds, BorderRadius))
      using (Pen borderPen = new Pen(BorderColor, 1)) {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawPath(borderPen, path);
      }
    }

    private void ApplyRoundedRegion() {
      if (Width <= 0 || Height <= 0) {
        return;
      }

      Rectangle bounds = new Rectangle(0, 0, Width, Height);
      using (GraphicsPath path = GetRoundedPath(bounds, BorderRadius)) {
        Region = new Region(path);
      }
    }

    private GraphicsPath GetRoundedPath(Rectangle bounds, int radius) {
      int diameter = radius * 2;
      GraphicsPath path = new GraphicsPath();

      path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
      path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
      path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
      path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
      path.CloseFigure();

      return path;
    }
  }
}
