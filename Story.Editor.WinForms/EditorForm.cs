using Story.Model;
using Story.Persistence;

namespace Story.Editor.WinForms;

public partial class EditorForm : Form
{
    private StoryDefinition _currentStory = new() { Title = "Aventura mea" };
    private StoryRepository _repo = new();

    private ListBox lbBlocks = null!;
    private TextBox txtStoryTitle = null!;
    private TextBox txtBlockId = null!;
    private TextBox txtBlockText = null!;
    private TextBox txtBgImage = null!;
    private Button btnSaveBlock = null!;
    private Button btnAddBlock = null!;
    private Button btnExportZip = null!;

    // --- ELEMENTE NOI PENTRU DECIZII ---
    private ListBox lbDecisions = null!;
    private TextBox txtDecisionText = null!;
    private ComboBox cbTargetBlock = null!;
    private Button btnAddDecision = null!;
    private Button btnDeleteDecision = null!;

    public EditorForm()
    {
        SetupEditorLayout();
        LoadBlocksToList();
    }

    private void SetupEditorLayout()
    {
        this.Text = "Story Editor - Creator de Povești Interactive";
        this.Size = new Size(1100, 750); // Am mărit puțin fereastra ca să încapă totul perfect
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(45, 45, 48);
        this.ForeColor = Color.White;

        var mainGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.Controls.Add(mainGrid);

        // --- ZONA STÂNGA ---
        var pnlLeft = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(10) };
        pnlLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
        pnlLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        pnlLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        mainGrid.Controls.Add(pnlLeft, 0, 0);

        var lblBlocksHeader = new Label { Text = "Lista Blocuri Povești:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        lbBlocks = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
        lbBlocks.SelectedIndexChanged += LbBlocksSelectedIndexChanged;

        btnAddBlock = new Button { Text = "➕ Adaugă Bloc Nou", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btnAddBlock.Click += BtnAddBlockClick;

        pnlLeft.Controls.Add(lblBlocksHeader, 0, 0);
        pnlLeft.Controls.Add(lbBlocks, 0, 1);
        pnlLeft.Controls.Add(btnAddBlock, 0, 2);

        // --- ZONA DREAPTA ---
        var pnlRight = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1, Padding = new Padding(20) };
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 0. Titlu
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 1. ID Bloc
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 2. Imagine Fundal
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));   // 3. Text principal (proporțional)
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));   // 4. ZONA NOUĂ: Management Decizii
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // 5. Buton Salvare
        pnlRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));  // 6. Buton Export
        mainGrid.Controls.Add(pnlRight, 1, 0);

        // 0. Rand Titlu
        var pnlTitleGroup = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
        pnlTitleGroup.Controls.Add(new Label { Text = "Titlu Poveste:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        txtStoryTitle = new TextBox { Text = _currentStory.Title, Width = 500, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
        txtStoryTitle.TextChanged += (s, e) => _currentStory.Title = txtStoryTitle.Text;
        pnlTitleGroup.Controls.Add(txtStoryTitle);
        pnlRight.Controls.Add(pnlTitleGroup, 0, 0);

        // 1. Rand ID Bloc
        var pnlIdGroup = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
        pnlIdGroup.Controls.Add(new Label { Text = "ID Bloc curent (folosit la legături):", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        txtBlockId = new TextBox { Width = 200, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
        pnlIdGroup.Controls.Add(txtBlockId);
        pnlRight.Controls.Add(pnlIdGroup, 0, 1);

        // 2. Rand Imagine Fundal
        var pnlBgGroup = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
        pnlBgGroup.Controls.Add(new Label { Text = "Imagine Fundal (ex: padure.jpg):", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        txtBgImage = new TextBox { Width = 200, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
        pnlBgGroup.Controls.Add(txtBgImage);
        pnlRight.Controls.Add(pnlBgGroup, 0, 2);

        // 3. Rand Casetă Text Mare
        var pnlTextGroup = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        pnlTextGroup.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        pnlTextGroup.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        pnlTextGroup.Controls.Add(new Label { Text = "Textul principal al poveștii în acest punct:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 0);
        txtBlockText = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
        pnlTextGroup.Controls.Add(txtBlockText, 0, 1);
        pnlRight.Controls.Add(pnlTextGroup, 0, 3);

        // --- 4. ZONA NOUĂ: CONSTRUCȚIE GRAFICĂ PENTRU DECIZII (BUTOANE) ---
        var gbDecisions = new GroupBox { Text = "Decizii / Opțiuni pentru acest Bloc (Butoane în Player)", Dock = DockStyle.Fill, ForeColor = Color.LightBlue, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var gridDecisions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(5) };
        gridDecisions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); // Partea stângă: lista de decizii
        gridDecisions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); // Partea dreaptă: editare decizie curentă
        gbDecisions.Controls.Add(gridDecisions);

        // Stânga zonă decizii: Lista + Buton Adăugare/Ștergere
        var pnlDecLeft = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
        pnlDecLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        pnlDecLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

        lbDecisions = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 25, 25), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
        lbDecisions.SelectedIndexChanged += LbDecisionsSelectedIndexChanged;

        btnAddDecision = new Button { Text = "➕ Adaugă Opțiune", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        btnAddDecision.Click += BtnAddDecisionClick;

        btnDeleteDecision = new Button { Text = "❌ Șterge Opțiune", Dock = DockStyle.Fill, BackColor = Color.DarkRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        btnDeleteDecision.Click += BtnDeleteDecisionClick;

        pnlDecLeft.Controls.Add(lbDecisions, 0, 0);
        pnlDecLeft.SetColumnSpan(lbDecisions, 2);
        pnlDecLeft.Controls.Add(btnAddDecision, 0, 1);
        pnlDecLeft.Controls.Add(btnDeleteDecision, 1, 1);
        gridDecisions.Controls.Add(pnlDecLeft, 0, 0);

        // Dreapta zonă decizii: Câmpuri de editare text și destinație
        var pnlDecRight = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10, 0, 0, 0) };

        pnlDecRight.Controls.Add(new Label { Text = "Text afișat pe buton:", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 9) });
        txtDecisionText = new TextBox { Width = 300, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
        txtDecisionText.TextChanged += TxtDecisionFieldsChanged;
        pnlDecRight.Controls.Add(txtDecisionText);

        pnlDecRight.Controls.Add(new Label { Text = "Bloc Destinație (Unde duce butonul):", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 9) });
        cbTargetBlock = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
        cbTargetBlock.SelectedIndexChanged += TxtDecisionFieldsChanged;
        pnlDecRight.Controls.Add(cbTargetBlock);

        gridDecisions.Controls.Add(pnlDecRight, 1, 0);
        pnlRight.Controls.Add(gbDecisions, 0, 4);

        // 5. Rand Buton Salvare
        btnSaveBlock = new Button { Text = "💾 Salvează Modificări Bloc Curent (Inclusiv Decizii)", Dock = DockStyle.Fill, BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btnSaveBlock.Click += BtnSaveBlockClick;
        pnlRight.Controls.Add(btnSaveBlock, 0, 5);

        // 6. Rand Buton Export
        btnExportZip = new Button { Text = "📦 Exportă Povestea Finală în Format (.zip)", Dock = DockStyle.Fill, BackColor = Color.FromArgb(140, 20, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
        btnExportZip.Click += BtnExportZipClick;
        pnlRight.Controls.Add(btnExportZip, 0, 6);

        if (_currentStory.Blocks == null) _currentStory.Blocks = new List<StoryBlock>();
        if (_currentStory.Blocks.Count == 0)
        {
            _currentStory.Blocks.Add(new StoryBlock { Id = "start", Text = "Text de pornire...", Decisions = new List<DecisionDefinition>() });
        }
    }

    private void LoadBlocksToList()
    {
        int saveIndex = lbBlocks.SelectedIndex;
        lbBlocks.Items.Clear();
        cbTargetBlock.Items.Clear();
        if (_currentStory.Blocks == null) return;

        foreach (var block in _currentStory.Blocks)
        {
            if (block != null && !string.IsNullOrEmpty(block.Id))
            {
                lbBlocks.Items.Add(block.Id);
                cbTargetBlock.Items.Add(block.Id); // Umplem și lista de selecție pentru destinații
            }
        }

        if (lbBlocks.Items.Count > 0)
        {
            if (saveIndex >= 0 && saveIndex < lbBlocks.Items.Count)
                lbBlocks.SelectedIndex = saveIndex;
            else
                lbBlocks.SelectedIndex = 0;
        }
    }

    private void LbBlocksSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lbBlocks.SelectedItem == null || _currentStory.Blocks == null) return;
        int idx = lbBlocks.SelectedIndex;
        if (idx < 0 || idx >= _currentStory.Blocks.Count) return;

        var block = _currentStory.Blocks[idx];

        txtBlockId.Text = block.Id;
        txtBlockText.Text = block.Text;
        txtBgImage.Text = block.BackgroundImage ?? "";
        txtBlockId.ReadOnly = (block.Id == "start");

        // --- ÎNCĂRCARE DECIZII PENTRU BLOCUL SELECTAT ---
        if (block.Decisions == null) block.Decisions = new List<DecisionDefinition>();

        lbDecisions.Items.Clear();
        foreach (var dec in block.Decisions)
        {
            lbDecisions.Items.Add($"{dec.Text} -> [{dec.TargetBlock}]");
        }

        txtDecisionText.Text = "";
        if (cbTargetBlock.Items.Count > 0) cbTargetBlock.SelectedIndex = 0;
    }

    private void LbDecisionsSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lbBlocks.SelectedIndex < 0 || lbDecisions.SelectedIndex < 0) return;
        var block = _currentStory.Blocks[lbBlocks.SelectedIndex];
        var dec = block.Decisions[lbDecisions.SelectedIndex];

        // Scoatere evenimente temporar ca să nu intre în buclă la scriere
        txtDecisionText.TextChanged -= TxtDecisionFieldsChanged;
        cbTargetBlock.SelectedIndexChanged -= TxtDecisionFieldsChanged;

        txtDecisionText.Text = dec.Text;
        cbTargetBlock.SelectedItem = dec.TargetBlock;

        txtDecisionText.TextChanged += TxtDecisionFieldsChanged;
        cbTargetBlock.SelectedIndexChanged += TxtDecisionFieldsChanged;
    }

    private void TxtDecisionFieldsChanged(object? sender, EventArgs e)
    {
        if (lbBlocks.SelectedIndex < 0 || lbDecisions.SelectedIndex < 0) return;
        var block = _currentStory.Blocks[lbBlocks.SelectedIndex];
        var dec = block.Decisions[lbDecisions.SelectedIndex];

        dec.Text = txtDecisionText.Text;
        dec.TargetBlock = cbTargetBlock.SelectedItem?.ToString() ?? "";

        // Actualizăm textul în lista vizuală live
        int currentDecIdx = lbDecisions.SelectedIndex;
        lbDecisions.Items[currentDecIdx] = $"{dec.Text} -> [{dec.TargetBlock}]";
    }

    private void BtnAddDecisionClick(object? sender, EventArgs e)
    {
        if (lbBlocks.SelectedIndex < 0)
        {
            MessageBox.Show("Selectează mai întâi un bloc din lista din stânga!");
            return;
        }

        var block = _currentStory.Blocks[lbBlocks.SelectedIndex];
        if (block.Decisions == null) block.Decisions = new List<DecisionDefinition>();

        string primaDestinatie = cbTargetBlock.Items.Count > 0 ? cbTargetBlock.Items[0].ToString()! : "start";
        var nouaDecizie = new DecisionDefinition { Text = "Opțiune nouă", TargetBlock = primaDestinatie };
        block.Decisions.Add(nouaDecizie);

        lbDecisions.Items.Add($"{nouaDecizie.Text} -> [{nouaDecizie.TargetBlock}]");
        lbDecisions.SelectedIndex = lbDecisions.Items.Count - 1;
    }

    private void BtnDeleteDecisionClick(object? sender, EventArgs e)
    {
        if (lbBlocks.SelectedIndex < 0 || lbDecisions.SelectedIndex < 0) return;
        var block = _currentStory.Blocks[lbBlocks.SelectedIndex];

        block.Decisions.RemoveAt(lbDecisions.SelectedIndex);
        int deSters = lbDecisions.SelectedIndex;
        lbDecisions.Items.RemoveAt(deSters);

        if (lbDecisions.Items.Count > 0)
            lbDecisions.SelectedIndex = Math.Max(0, deSters - 1);
        else
        {
            txtDecisionText.Text = "";
        }
    }

    private void BtnSaveBlockClick(object? sender, EventArgs e)
    {
        if (lbBlocks.SelectedItem == null || _currentStory.Blocks == null) return;
        int idx = lbBlocks.SelectedIndex;
        if (idx < 0 || idx >= _currentStory.Blocks.Count) return;

        var block = _currentStory.Blocks[idx];
        block.Id = txtBlockId.Text;
        block.Text = txtBlockText.Text;
        block.BackgroundImage = string.IsNullOrWhiteSpace(txtBgImage.Text) ? null : txtBgImage.Text;

        MessageBox.Show($"Blocul [{block.Id}] și deciziile sale au fost salvate în memorie!", "Salvare Reușită", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadBlocksToList();
    }

    private void BtnAddBlockClick(object? sender, EventArgs e)
    {
        if (_currentStory.Blocks == null) _currentStory.Blocks = new List<StoryBlock>();

        string newId = "bloc_" + (_currentStory.Blocks.Count + 1);
        var newBlock = new StoryBlock { Id = newId, Text = "Scrie textul noului bloc aici...", Decisions = new List<DecisionDefinition>() };
        _currentStory.Blocks.Add(newBlock);

        LoadBlocksToList();
        lbBlocks.SelectedIndex = _currentStory.Blocks.Count - 1;
    }

    private void BtnExportZipClick(object? sender, EventArgs e)
    {
        if (_currentStory.Blocks == null || !_currentStory.Blocks.Any(b => b.Id == "start"))
        {
            MessageBox.Show("Eroare: Trebuie să ai obligatoriu un bloc numit 'start'!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var sfd = new SaveFileDialog { Filter = "Story Zip Archive (*.zip)|*.zip", FileName = "povestea_mea.zip" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "StoryExport_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "images"));

            string jsonPath = Path.Combine(tempDir, "story.json");
            string jsonContent = System.Text.Json.JsonSerializer.Serialize(_currentStory, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            File.WriteAllText(jsonPath, jsonContent);

            if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
            System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, sfd.FileName);
            Directory.Delete(tempDir, true);

            MessageBox.Show("Povestea a fost exportată cu succes!", "Bravo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Eroare: " + ex.Message);
        }
    }
}