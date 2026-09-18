using System;
using System.Drawing;
using System.Windows.Forms;

// Original procedural pixel art; no external assets or packages.
public sealed class DesktopCat : Form
{
    readonly Timer timer = new Timer();
    readonly Random random = new Random();
    readonly ContextMenuStrip menu = new ContextMenuStrip();
    readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    readonly Brush outline = new SolidBrush(Color.FromArgb(55, 39, 43));
    readonly Brush fur = new SolidBrush(Color.FromArgb(235, 163, 81));
    readonly Brush light = new SolidBrush(Color.FromArgb(255, 224, 166));
    readonly Brush pink = new SolidBrush(Color.FromArgb(238, 137, 148));
    Rectangle area;
    double x, y, vy, previous, changeAt, happyUntil;
    int direction = 1;
    bool resting, paused, dragging;
    Point grab;
    const int PixelScale = 4;

    public DesktopCat()
    {
        Text = "Kocka na plose";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(40 * PixelScale, 32 * PixelScale);
        StartPosition = FormStartPosition.Manual;
        area = Screen.PrimaryScreen.WorkingArea;
        x = area.Left + area.Width / 2 - Width / 2;
        y = area.Bottom - Height;
        Location = new Point((int)x, (int)y);
        changeAt = 4;
        menu.Items.Add("Pohladit", null, delegate { Pet(); });
        ToolStripMenuItem pause = new ToolStripMenuItem("Pozastavit");
        pause.CheckOnClick = true;
        pause.CheckedChanged += delegate { paused = pause.Checked; };
        menu.Items.Add(pause);
        menu.Items.Add("Zpet na hlavni monitor", null, delegate {
            area = Screen.PrimaryScreen.WorkingArea;
            x = area.Left + area.Width / 2 - Width / 2;
            y = area.Bottom - Height;
            vy = 0;
            Location = new Point((int)x, (int)y);
        });
        menu.Items.Add("Ukoncit", null, delegate { Close(); });
        ContextMenuStrip = menu;
        MouseDown += delegate(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;
            grab = e.Location;
            dragging = true;
            Capture = true;
            Pet();
        };
        MouseMove += delegate {
            if (!dragging) return;
            Point p = Cursor.Position;
            x = p.X - grab.X;
            y = p.Y - grab.Y;
            Location = new Point((int)x, (int)y);
        };
        MouseUp += delegate(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;
            dragging = false;
            Capture = false;
            area = Screen.FromPoint(Cursor.Position).WorkingArea;
            vy = 0;
        };
        MouseCaptureChanged += delegate { if (!Capture) dragging = false; };
        timer.Interval = 33;
        timer.Tick += delegate { TickCat(); };
        timer.Start();
    }

    protected override bool ShowWithoutActivation { get { return true; } }

    void Pet() { happyUntil = clock.Elapsed.TotalSeconds + 2; Invalidate(); }

    void TickCat()
    {
        double now = clock.Elapsed.TotalSeconds;
        double dt = Math.Min(0.05, now - previous);
        previous = now;
        if (!dragging && !paused && !menu.Visible)
        {
            area = Screen.FromRectangle(Bounds).WorkingArea;
            double ground = area.Bottom - Height;
            if (y < ground)
            {
                vy += 700 * dt;
                y = Math.Min(ground, y + vy * dt);
            }
            else { y = ground; vy = 0; }
            if (now >= changeAt)
            {
                resting = random.Next(3) == 0;
                direction = random.Next(2) == 0 ? -1 : 1;
                changeAt = now + random.Next(3, 8);
            }
            if (!resting && y >= ground) x += direction * 62 * dt;
            if (x <= area.Left) { x = area.Left; direction = 1; }
            if (x >= area.Right - Width) { x = area.Right - Width; direction = -1; }
            Location = new Point((int)x, (int)y);
        }
        Invalidate();
    }

    void Pixel(Graphics g, Brush brush, int px, int py, int w, int h)
    {
        if (direction < 0) px = 40 - px - w;
        g.FillRectangle(brush, px * PixelScale, py * PixelScale, w * PixelScale, h * PixelScale);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        double now = clock.Elapsed.TotalSeconds;
        bool happy = now < happyUntil;
        bool blink = now % 5.5 > 5.3;

        bool walking = !resting && !paused && !dragging
               && !menu.Visible && vy == 0;

        int walkFrame = walking ? (int)(now * 8) % 4 : 0;

        // Mřížka odečtená z předlohy:
        // . průhledné pozadí
        // # tmavý obrys a oči
        // o základní srst
        // s stín srsti
        // n oranžové pruhy
        // + světlé uši a tlapky
        string[] sprite = {
        ".........#.....#.",
        "........#o#...#o#",
        ".......#o+####o+#",
        ".......#++onon++#",
        "......#oooononoo#",
        ".#....#oooooonoo#",
        "#o#...#ooo#ooo#o#",
        "#o#...#ooo#ooo#o#",
        "#o#....#soooooos#",
        "#so######soooos#.",
        ".#soonons#nnnn#..",
        "..##soonoossss#..",
        "....#ooooooooo#..",
        "....#o#s###o#s#..",
        "....#+#+#.#+#+#..",
        ".....####..####.."
    };

        using (Brush edge = new SolidBrush(Color.FromArgb(56, 39, 2)))
        using (Brush coat = new SolidBrush(Color.FromArgb(247, 168, 96)))
        using (Brush shade = new SolidBrush(Color.FromArgb(232, 145, 69)))
        using (Brush stripe = new SolidBrush(Color.FromArgb(211, 120, 42)))
        using (Brush cream = new SolidBrush(Color.FromArgb(234, 197, 151)))
        {
            // 17 × 16 zdrojových pixelů -> 34 × 32 jednotek.
            // Při PixelScale = 4 má kresba 136 × 128 px.
            const int cell = 2;
            const int offsetX = 3;

            for (int row = 0; row < sprite.Length; row++)
            {
                for (int col = 0; col < sprite[row].Length; col++)
                {
                    char pixel = sprite[row][col];

                    // Zavření očí: zůstane jen horní řádek.
                    if ((blink || happy) &&
                        row == 7 && (col == 10 || col == 14))
                    {
                        pixel = 'o';
                    }

                    Brush brush = null;

                    switch (pixel)
                    {
                        case '#': brush = edge; break;
                        case 'o': brush = coat; break;
                        case 's': brush = shade; break;
                        case 'n': brush = stripe; break;
                        case '+': brush = cream; break;
                    }

                    if (brush == null)
                        continue;

                    int lift = 0;

                    if (walking && row >= 14)
                    {
                        // Čtyři tlapky, počítané zleva v původní kresbě.
                        bool firstPaw = col >= 4 && col <= 6;
                        bool secondPaw = col >= 7 && col <= 8;
                        bool thirdPaw = col >= 10 && col <= 12;
                        bool fourthPaw = col >= 13 && col <= 14;

                        // Mezi kroky se všechny tlapky dotknou země.
                        if (walkFrame == 1 && (firstPaw || fourthPaw))
                            lift = 1;
                        else if (walkFrame == 3 && (secondPaw || thirdPaw))
                            lift = 1;
                    }

                    Pixel(
                        e.Graphics,
                        brush,
                        offsetX + col * cell,
                        row * cell - lift,
                        cell,
                        cell
                    );
                }
            }
        }

        // Srdíčko v prázdném prostoru nad ocasem.
        if (happy)
        {
            Pixel(e.Graphics, pink, 5, 2, 2, 2);
            Pixel(e.Graphics, pink, 8, 2, 2, 2);
            Pixel(e.Graphics, pink, 5, 4, 5, 1);
            Pixel(e.Graphics, pink, 6, 5, 3, 1);
            Pixel(e.Graphics, pink, 7, 6, 1, 1);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Dispose(); menu.Dispose();
            outline.Dispose(); fur.Dispose(); light.Dispose(); pink.Dispose();
        }
        base.Dispose(disposing);
    }

    [STAThread]
    public static void Run()
    {
        Application.EnableVisualStyles();
        using (DesktopCat cat = new DesktopCat()) Application.Run(cat);
    }
}
