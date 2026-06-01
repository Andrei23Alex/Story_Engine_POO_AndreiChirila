using Story.Engine;
using Story.Model;
using Story.Persistence;

namespace Story.Player.WinForms;

public partial class PlayerForm : Form
{
    private StoryRepository _repo = new();
    private GameEngine? _engine;
    private ImageRepository? _imgRepo;
    private string? _extractPath;

    // Elemente vizuale create din cod
    private Label lblText = null!;
    private Panel pnlTextContainer = null!; // Container nou elastic pentru textul de sus
    private FlowLayoutPanel pnlDecisions = null!; // Transformat în FlowLayoutPanel pentru butoane flexibile
    private FlowLayoutPanel pnlHud = null!;
    private PictureBox pbBackground = null!;
    private MenuStrip menuStrip = null!;

    public PlayerForm()
    {
        SetupCustomLayout();
    }

    private void SetupCustomLayout()
    {
        this.Text = "Story Player - Interactive Adventure";
        this.Size = new Size(900, 650);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Meniu de Sus
        menuStrip = new MenuStrip();
        var menuFile = new ToolStripMenuItem("File");
        var btnOpen = new ToolStripMenuItem("Deschide Poveste (.zip)", null, OpenStoryClick);
        var btnLoad = new ToolStripMenuItem("Încarcă Salvare", null, LoadStateClick);
        var btnSave = new ToolStripMenuItem("Salvează Joc", null, SaveStateClick);
        menuFile.DropDownItems.AddRange(new ToolStripItem[] { btnOpen, btnLoad, btnSave });
        menuStrip.Items.Add(menuFile);
        this.Controls.Add(menuStrip);

        // Imaginea de Fundal (umple toată fereastra)
        pbBackground = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.FromArgb(30, 30, 30)
        };
        this.Controls.Add(pbBackground);

        // Panou HUD (Statistici) - Plasat Sus
        pnlHud = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(180, 0, 0, 0),
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };
        pbBackground.Controls.Add(pnlHud);

        // Panou Decizii - Transformat în FlowLayoutPanel pentru aliniere și auto-size automat
        pnlDecisions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 130,
            BackColor = Color.FromArgb(200, 20, 20, 20),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(20, 40, 20, 20)
        };
        pbBackground.Controls.Add(pnlDecisions);

        // Textul Poveștii – Poziționat direct ca etichetă inteligentă sus
        lblText = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true, // Permite creșterea pe verticală în jos
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Regular),
            BackColor = Color.FromArgb(150, 0, 0, 0), // Bandă semi-transparentă cinematografică
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(25, 20, 25, 20)
        };
        pbBackground.Controls.Add(lblText);

        // Forțăm ordinea corectă a straturilor
        lblText.BringToFront();
        pnlHud.BringToFront();

        // Adăugăm un eveniment de redimensionare pentru ca textul să știe mereu când se schimbă lățimea ferestrei
        this.SizeChanged += (s, e) => { SetLabelMaxWidth(); };
    }

    // Funcție ajutătoare care forțează textul să facă Word-Wrap perfect în funcție de fereastră
    private void SetLabelMaxWidth()
    {
        if (lblText != null && pbBackground != null)
        {
            lblText.MaximumSize = new Size(pbBackground.Width, 0); // 0 înseamnă înălțime infinită (crește oricât în jos)
        }
    }

    private void OpenStoryClick(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "Story Archive (*.zip)|*.zip" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            if (_extractPath != null && Directory.Exists(_extractPath))
            {
                _imgRepo?.Dispose();
                try { Directory.Delete(_extractPath, true); } catch { }
            }

            var (story, path) = _repo.LoadFromZip(ofd.FileName);
            _extractPath = path;

            _imgRepo = new ImageRepository(path);
            _engine = new GameEngine(story);

            this.Text = "Story Player - " + (story.Title ?? "Interactive Adventure");
            UpdateUi();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Eroare critică la încărcare: " + ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateUi()
    {
        if (_engine == null) return;

        var block = _engine.CurrentBlock;
        if (block == null)
        {
            lblText.Text = "Eroare: Blocul curent nu a fost găsit în structura citită de Motorul de Joc.";
            return;
        }

        // Actualizăm limita de lățime chiar înainte de a pune textul, asigurând un wrap perfect
        SetLabelMaxWidth();
        lblText.Text = block.Text;

        // Schimbăm imaginea de fundal din arhivă
        if (_imgRepo != null && !string.IsNullOrEmpty(block.BackgroundImage))
            pbBackground.Image = _imgRepo.GetImage(block.BackgroundImage);
        else
            pbBackground.Image = null;

        // Actualizare HUD (Statistici jucător)
        pnlHud.Controls.Clear();
        if (_engine.Story != null && _engine.Story.Properties != null)
        {
            var visibleProps = _engine.Story.Properties
                .Where(p => p != null && p.VisibleInHud)
                .OrderBy(p => p.HudOrder);

            foreach (var prop in visibleProps)
            {
                double val = _engine.State.Properties.GetValueOrDefault(prop.Key, prop.Initial);
                var lbl = new Label
                {
                    Text = $"{prop.HudLabel}: {val}",
                    ForeColor = Color.Gold,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(10, 0, 10, 0)
                };
                pnlHud.Controls.Add(lbl);
            }
        }

        // Actualizare Butoane Decizii
        pnlDecisions.Controls.Clear();
        if (_engine.IsGameOver)
        {
            var lblGameOver = new Label
            {
                Text = "SFÂRȘIT",
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding((pnlDecisions.Width - 100) / 2, 10, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDecisions.Controls.Add(lblGameOver);
            return;
        }

        var available = _engine.GetAvailableDecisions();

        if (available == null || available.Count == 0)
        {
            var lblEnd = new Label
            {
                Text = "Sfârșitul acestei ramuri sau decizii neconfigurate.",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                AutoSize = true,
                Margin = new Padding((pnlDecisions.Width - 320) / 2, 15, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDecisions.Controls.Add(lblEnd);
            return;
        }

        // Generare butoane elastice (Soluția ta excelentă care a funcționat!)
        List<Button> buttonsList = new List<Button>();
        int totalWidthNeeded = 0;

        for (int i = 0; i < available.Count; i++)
        {
            var dec = available[i];
            var btn = new Button
            {
                Text = dec.Text,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Tag = dec,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20, 10, 20, 10),
                Margin = new Padding(10, 0, 10, 0)
            };
            btn.FlatAppearance.BorderColor = Color.White;
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += DecisionClick;

            buttonsList.Add(btn);

            using (Graphics g = btn.CreateGraphics())
            {
                Size size = TextRenderer.MeasureText(g, dec.Text, btn.Font);
                totalWidthNeeded += size.Width + 60; // Compensare completă pentru padding și margini
            }
        }

        // Centrare dinamică în FlowLayout
        if (buttonsList.Count > 0)
        {
            int leftMargin = (pnlDecisions.Width - totalWidthNeeded) / 2;
            if (leftMargin < 15) leftMargin = 15;
            buttonsList[0].Margin = new Padding(leftMargin, 0, 10, 0);
        }

        foreach (var btn in buttonsList)
        {
            pnlDecisions.Controls.Add(btn);
        }
    }

    private void DecisionClick(object? sender, EventArgs e)
    {
        if (_engine == null || sender is not Button btn || btn.Tag is not DecisionDefinition dec) return;

        _engine.MakeDecision(dec);
        UpdateUi();
    }

    private void SaveStateClick(object? sender, EventArgs e)
    {
        if (_engine == null) return;
        using var sfd = new SaveFileDialog { Filter = "Save State (*.json)|*.json" };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            _repo.SaveGameState(_engine.SaveState(), sfd.FileName);
            MessageBox.Show("Joc salvat cu succes!", "Salvare", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void LoadStateClick(object? sender, EventArgs e)
    {
        if (_engine == null)
        {
            MessageBox.Show("Încarcă mai întâi povestea (.zip) înainte de a încărca o salvare!", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var ofd = new OpenFileDialog { Filter = "Save State (*.json)|*.json" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var state = _repo.LoadGameState(ofd.FileName);
            _engine.LoadState(state);
            UpdateUi();
        }
    }
}