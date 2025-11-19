namespace L2D;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SpriteWindow());
    }
}

public class SpriteWindow : Form
{
    private Timer timer;
    private Bitmap sheet;
    private int frameWidth = 350;
    private int frameHeight = 350;
    private int currentFrame = 0;
    private int totalFrames;

    private bool flipHorizontal = true;
    private float scale = 0.5f;

    private bool dragging = false;
    private Point dragOffset;

    public SpriteWindow()
    {
        TopMost = true;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false; // remove from taskbar
        StartPosition = FormStartPosition.CenterScreen;
        Width = (int)(frameWidth * scale);
        Height = (int)(frameHeight * scale);
        BackColor = Color.Magenta; // transparency key
        TransparencyKey = Color.Magenta;

        DoubleBuffered = true; // reduce flicker

        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
        if (!Directory.Exists(folder))
        {
            MessageBox.Show("Missing /assets folder");
            Close(); return;
        }

        string[] pngFiles = Directory.GetFiles(folder, "*.png");
        if (pngFiles.Length == 0)
        {
            MessageBox.Show("No PNGs in /assets");
            Close(); return;
        }

        // Load first PNG as sprite sheet
        sheet = new Bitmap(pngFiles[0]);
        totalFrames = sheet.Width / frameWidth;

        timer = new Timer();
        timer.Interval = 1000 / 12;
        timer.Tick += (s, e) => { currentFrame = (currentFrame + 1) % totalFrames; Invalidate(); };
        timer.Start();

        // Mouse click event
        this.MouseClick += SpriteWindow_MouseClick;

        // Mouse events for dragging
        this.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragOffset = e.Location;
            }
        };

        this.MouseMove += (s, e) =>
        {
            if (dragging)
            {
                Location = new Point(Location.X + e.X - dragOffset.X, Location.Y + e.Y - dragOffset.Y);
            }
        };

        this.MouseUp += (s, e) => { dragging = false; };
    }

    private void SpriteWindow_MouseClick(object sender, MouseEventArgs e)
    {
        Debug.WriteLine($"Mouse clicked at: X={e.X}, Y={e.Y}");
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        int x = currentFrame * frameWidth;
        Rectangle src = new Rectangle(x, 0, frameWidth, frameHeight);

        Rectangle dest;
        if (flipHorizontal)
        {
            dest = new Rectangle((int)(frameWidth * scale), 0, -(int)(frameWidth * scale), (int)(frameHeight * scale));
        }
        else
        {
            dest = new Rectangle(0, 0, (int)(frameWidth * scale), (int)(frameHeight * scale));
        }

        e.Graphics.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
            return cp;
        }
    }
}