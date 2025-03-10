using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace bdmanager {
  public class RoundButton : Button {
    private Color _borderColor = Color.FromArgb(80, 80, 80);
    private Color _hoverColor = Color.FromArgb(70, 70, 70);
    private Color _pressedColor = Color.FromArgb(50, 50, 50);
    private int _borderSize = 1;
    private int _borderRadius = 15;

    public RoundButton() {
      FlatStyle = FlatStyle.Flat;
      FlatAppearance.BorderSize = 0;
      BackColor = Color.FromArgb(50, 50, 50);
      ForeColor = Color.White;
      Cursor = Cursors.Hand;
      Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    public Color BorderColor {
      get => _borderColor;
      set { _borderColor = value; Invalidate(); }
    }

    public Color HoverColor {
      get => _hoverColor;
      set { _hoverColor = value; Invalidate(); }
    }

    public Color PressedColor {
      get => _pressedColor;
      set { _pressedColor = value; Invalidate(); }
    }

    public int BorderSize {
      get => _borderSize;
      set { _borderSize = value; Invalidate(); }
    }

    public int BorderRadius {
      get => _borderRadius;
      set { _borderRadius = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);

      Rectangle rectSurface = this.ClientRectangle;
      Rectangle rectBorder = Rectangle.Inflate(rectSurface, -_borderSize, -_borderSize);
      int smoothSize = _borderRadius > 0 ? _borderRadius : 1;

      using (GraphicsPath pathSurface = GetFigurePath(rectSurface, smoothSize))
      using (GraphicsPath pathBorder = GetFigurePath(rectBorder, smoothSize - _borderSize))
      using (Pen penSurface = new Pen(Parent.BackColor, _borderSize))
      using (Pen penBorder = new Pen(_borderColor, _borderSize)) {
        penBorder.Alignment = PenAlignment.Inset;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Region = new Region(pathSurface);
        e.Graphics.DrawPath(penSurface, pathSurface);
        if (_borderSize > 0) e.Graphics.DrawPath(penBorder, pathBorder);
      }
    }

    private GraphicsPath GetFigurePath(Rectangle rect, int radius) {
      GraphicsPath path = new GraphicsPath();
      float curveSize = radius * 2F;

      path.StartFigure();
      path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
      path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
      path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
      path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
      path.CloseFigure();

      return path;
    }

    protected override void OnMouseEnter(EventArgs e) {
      base.OnMouseEnter(e);
      BackColor = _hoverColor;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      base.OnMouseDown(e);
      BackColor = _pressedColor;
    }

    protected override void OnMouseUp(MouseEventArgs e) {
      base.OnMouseUp(e);
      BackColor = _hoverColor;
    }
  }
}
