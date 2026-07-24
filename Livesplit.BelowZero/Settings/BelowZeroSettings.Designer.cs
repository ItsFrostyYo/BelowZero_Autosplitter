namespace LiveSplit.BelowZero
{
    partial class BelowZeroSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.flowMain = new System.Windows.Forms.FlowLayoutPanel();
            this.flowOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.Options_GroupBox = new System.Windows.Forms.GroupBox();
            this.ButtonSplitGenerator = new System.Windows.Forms.Button();
            this.Other_GroupBox = new System.Windows.Forms.GroupBox();
            this.chkDiscordStatus = new System.Windows.Forms.CheckBox();
            this.cbOrderedAutoSplits = new System.Windows.Forms.CheckBox();
            this.cbOrderedLiveSplit = new System.Windows.Forms.CheckBox();
            this.chkAskForGoldSave = new System.Windows.Forms.CheckBox();
            this.btnAddSplit = new System.Windows.Forms.Button();
            this.StartReset_GroupBox = new System.Windows.Forms.GroupBox();
            this.chkReset = new System.Windows.Forms.CheckBox();
            this.chkCreativeStart = new System.Windows.Forms.CheckBox();
            this.chkIntroStart = new System.Windows.Forms.CheckBox();
            this.SortBy_GroupBox = new System.Windows.Forms.GroupBox();
            this.RdAlpha = new System.Windows.Forms.RadioButton();
            this.RdType = new System.Windows.Forms.RadioButton();
            this.ToolTips = new System.Windows.Forms.ToolTip(this.components);
            this.flowMain.SuspendLayout();
            this.flowOptions.SuspendLayout();
            this.Options_GroupBox.SuspendLayout();
            this.Other_GroupBox.SuspendLayout();
            this.StartReset_GroupBox.SuspendLayout();
            this.SortBy_GroupBox.SuspendLayout();
            this.SuspendLayout();
            // flowMain
            // 
            this.flowMain.AllowDrop = true;
            this.flowMain.AutoSize = true;
            this.flowMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowMain.Controls.Add(this.flowOptions);
            this.flowMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowMain.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowMain.Location = new System.Drawing.Point(0, 0);
            this.flowMain.Margin = new System.Windows.Forms.Padding(0);
            this.flowMain.Name = "flowMain";
            this.flowMain.Size = new System.Drawing.Size(472, 152);
            this.flowMain.TabIndex = 0;
            this.flowMain.WrapContents = false;
            this.flowMain.DragDrop += new System.Windows.Forms.DragEventHandler(this.flowMain_DragDrop);
            this.flowMain.DragEnter += new System.Windows.Forms.DragEventHandler(this.flowMain_DragEnter);
            this.flowMain.DragOver += new System.Windows.Forms.DragEventHandler(this.flowMain_DragOver);
            this.flowMain.DragLeave += new System.EventHandler(this.flowMain_DragLeave);
            this.flowMain.Paint += new System.Windows.Forms.PaintEventHandler(this.flowMain_Paint);
            // 
            // flowOptions
            // 
            this.flowOptions.AutoSize = true;
            this.flowOptions.Controls.Add(this.Options_GroupBox);
            this.flowOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowOptions.Location = new System.Drawing.Point(0, 0);
            this.flowOptions.Margin = new System.Windows.Forms.Padding(0);
            this.flowOptions.Name = "flowOptions";
            this.flowOptions.Size = new System.Drawing.Size(472, 152);
            this.flowOptions.TabIndex = 2;
            // 
            // Options_GroupBox
            // 
            this.Options_GroupBox.Controls.Add(this.ButtonSplitGenerator);
            this.Options_GroupBox.Controls.Add(this.Other_GroupBox);
            this.Options_GroupBox.Controls.Add(this.btnAddSplit);
            this.Options_GroupBox.Controls.Add(this.StartReset_GroupBox);
            this.Options_GroupBox.Controls.Add(this.SortBy_GroupBox);
            this.Options_GroupBox.Location = new System.Drawing.Point(3, 3);
            this.Options_GroupBox.Name = "Options_GroupBox";
            this.Options_GroupBox.Size = new System.Drawing.Size(466, 146);
            this.Options_GroupBox.TabIndex = 6;
            this.Options_GroupBox.TabStop = false;
            this.Options_GroupBox.Text = "Options";
            // 
            // ButtonSplitGenerator
            // 
            this.ButtonSplitGenerator.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonSplitGenerator.Location = new System.Drawing.Point(128, 118);
            this.ButtonSplitGenerator.Name = "ButtonSplitGenerator";
            this.ButtonSplitGenerator.Size = new System.Drawing.Size(332, 23);
            this.ButtonSplitGenerator.TabIndex = 10;
            this.ButtonSplitGenerator.Text = "Generate Splits";
            this.ButtonSplitGenerator.UseVisualStyleBackColor = true;
            this.ButtonSplitGenerator.Click += new System.EventHandler(this.ButtonSplitGenerator_Click);
            // 
            // Other_GroupBox
            // 
            this.Other_GroupBox.Controls.Add(this.chkDiscordStatus);
            this.Other_GroupBox.Controls.Add(this.cbOrderedAutoSplits);
            this.Other_GroupBox.Controls.Add(this.cbOrderedLiveSplit);
            this.Other_GroupBox.Controls.Add(this.chkAskForGoldSave);
            this.Other_GroupBox.Location = new System.Drawing.Point(296, 15);
            this.Other_GroupBox.Name = "Other_GroupBox";
            this.Other_GroupBox.Size = new System.Drawing.Size(164, 98);
            this.Other_GroupBox.TabIndex = 8;
            this.Other_GroupBox.TabStop = false;
            this.Other_GroupBox.Text = "Others";
            // 
            // chkDiscordStatus
            // 
            this.chkDiscordStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDiscordStatus.AutoSize = true;
            this.chkDiscordStatus.Checked = true;
            this.chkDiscordStatus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDiscordStatus.Location = new System.Drawing.Point(5, 19);
            this.chkDiscordStatus.Margin = new System.Windows.Forms.Padding(2);
            this.chkDiscordStatus.Name = "chkDiscordStatus";
            this.chkDiscordStatus.Size = new System.Drawing.Size(96, 17);
            this.chkDiscordStatus.TabIndex = 0;
            this.chkDiscordStatus.Text = "Discord Status";
            this.ToolTips.SetToolTip(this.chkDiscordStatus, "Enables Discord Rich Presence to Display your Discord Game Status with the Autosplitter, this will display Main Menu| Loading| Running| Finished| Unknown with Time and Split Delta's.");
            this.chkDiscordStatus.UseVisualStyleBackColor = true;
            this.chkDiscordStatus.CheckedChanged += new System.EventHandler(this.chkDiscordStatus_CheckedChanged);
            // 
            // cbOrderedAutoSplits
            // 
            this.cbOrderedAutoSplits.AutoSize = true;
            this.cbOrderedAutoSplits.Location = new System.Drawing.Point(5, 79);
            this.cbOrderedAutoSplits.Name = "cbOrderedAutoSplits";
            this.cbOrderedAutoSplits.Size = new System.Drawing.Size(151, 17);
            this.cbOrderedAutoSplits.TabIndex = 3;
            this.cbOrderedAutoSplits.Text = "Ordered Splits (Auto-Splits)";
            this.ToolTips.SetToolTip(this.cbOrderedAutoSplits, "Auto-splits trigger one after another in their own sequence.");
            this.cbOrderedAutoSplits.UseVisualStyleBackColor = true;
            this.cbOrderedAutoSplits.CheckedChanged += new System.EventHandler(this.cbOrderedAutoSplits_CheckedChanged);
            // 
            // cbOrderedLiveSplit
            // 
            this.cbOrderedLiveSplit.AutoSize = true;
            this.cbOrderedLiveSplit.Location = new System.Drawing.Point(5, 59);
            this.cbOrderedLiveSplit.Name = "cbOrderedLiveSplit";
            this.cbOrderedLiveSplit.Size = new System.Drawing.Size(141, 17);
            this.cbOrderedLiveSplit.TabIndex = 2;
            this.cbOrderedLiveSplit.Text = "Ordered Splits (LiveSplit)";
            this.ToolTips.SetToolTip(this.cbOrderedLiveSplit, "Auto-splits are triggered in the same order as the splits in LiveSplit.\nIf you sk" +
        "ip a split in LiveSplit, the corresponding auto-split will also be skipped.");
            this.cbOrderedLiveSplit.UseVisualStyleBackColor = true;
            this.cbOrderedLiveSplit.CheckedChanged += new System.EventHandler(this.cbOrderedLiveSplit_CheckedChanged);
            // 
            // chkAskForGoldSave
            // 
            this.chkAskForGoldSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAskForGoldSave.AutoSize = true;
            this.chkAskForGoldSave.Location = new System.Drawing.Point(5, 39);
            this.chkAskForGoldSave.Margin = new System.Windows.Forms.Padding(2);
            this.chkAskForGoldSave.Name = "chkAskForGoldSave";
            this.chkAskForGoldSave.Size = new System.Drawing.Size(157, 17);
            this.chkAskForGoldSave.TabIndex = 1;
            this.chkAskForGoldSave.Text = "Warn On Reset If Gold Split";
            this.ToolTips.SetToolTip(this.chkAskForGoldSave, "Ask to save golds when the timer auto resets");
            this.chkAskForGoldSave.UseVisualStyleBackColor = true;
            this.chkAskForGoldSave.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // btnAddSplit
            // 
            this.btnAddSplit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddSplit.Location = new System.Drawing.Point(6, 118);
            this.btnAddSplit.Name = "btnAddSplit";
            this.btnAddSplit.Size = new System.Drawing.Size(116, 23);
            this.btnAddSplit.TabIndex = 0;
            this.btnAddSplit.Text = "Add Split";
            this.btnAddSplit.UseVisualStyleBackColor = true;
            this.btnAddSplit.Click += new System.EventHandler(this.BtnAddSplit_Click);
            // 
            // StartReset_GroupBox
            // 
            this.StartReset_GroupBox.Controls.Add(this.chkReset);
            this.StartReset_GroupBox.Controls.Add(this.chkIntroStart);
            this.StartReset_GroupBox.Controls.Add(this.chkCreativeStart);
            this.StartReset_GroupBox.Location = new System.Drawing.Point(128, 15);
            this.StartReset_GroupBox.Name = "StartReset_GroupBox";
            this.StartReset_GroupBox.Size = new System.Drawing.Size(162, 97);
            this.StartReset_GroupBox.TabIndex = 7;
            this.StartReset_GroupBox.TabStop = false;
            this.StartReset_GroupBox.Text = "Start / Reset";
            // 
            // chkReset
            // 
            this.chkReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkReset.AutoSize = true;
            this.chkReset.Location = new System.Drawing.Point(106, 31);
            this.chkReset.Margin = new System.Windows.Forms.Padding(2);
            this.chkReset.Name = "chkReset";
            this.chkReset.Size = new System.Drawing.Size(54, 17);
            this.chkReset.TabIndex = 2;
            this.chkReset.Text = "Reset";
            this.ToolTips.SetToolTip(this.chkReset, "Resets the timer when you come back to the main menu");
            this.chkReset.UseVisualStyleBackColor = true;
            this.chkReset.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // chkCreativeStart
            // 
            this.chkCreativeStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCreativeStart.AutoSize = true;
            this.chkCreativeStart.Checked = true;
            this.chkCreativeStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreativeStart.Location = new System.Drawing.Point(5, 55);
            this.chkCreativeStart.Margin = new System.Windows.Forms.Padding(2);
            this.chkCreativeStart.Name = "chkCreativeStart";
            this.chkCreativeStart.Size = new System.Drawing.Size(90, 17);
            this.chkCreativeStart.TabIndex = 1;
            this.chkCreativeStart.Text = "Start on Input";
            this.ToolTips.SetToolTip(this.chkCreativeStart, "Starts on movement input, jumping, PDA use, a snowball pickup, or opening a builder or crafting menu.");
            this.chkCreativeStart.UseVisualStyleBackColor = true;
            this.chkCreativeStart.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // chkIntroStart
            // 
            this.chkIntroStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkIntroStart.AutoSize = true;
            this.chkIntroStart.Checked = true;
            this.chkIntroStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIntroStart.Location = new System.Drawing.Point(5, 31);
            this.chkIntroStart.Margin = new System.Windows.Forms.Padding(2);
            this.chkIntroStart.Name = "chkIntroStart";
            this.chkIntroStart.Size = new System.Drawing.Size(48, 17);
            this.chkIntroStart.TabIndex = 0;
            this.chkIntroStart.Text = "Survival Start";
            this.ToolTips.SetToolTip(this.chkIntroStart, "Arms during the opening cinematic, starts on actual horizontal player movement, and falls back to intro completion.");
            this.chkIntroStart.UseVisualStyleBackColor = true;
            this.chkIntroStart.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // SortBy_GroupBox
            // 
            this.SortBy_GroupBox.Controls.Add(this.RdAlpha);
            this.SortBy_GroupBox.Controls.Add(this.RdType);
            this.SortBy_GroupBox.Location = new System.Drawing.Point(6, 15);
            this.SortBy_GroupBox.Name = "SortBy_GroupBox";
            this.SortBy_GroupBox.Size = new System.Drawing.Size(116, 97);
            this.SortBy_GroupBox.TabIndex = 6;
            this.SortBy_GroupBox.TabStop = false;
            this.SortBy_GroupBox.Text = "Sort Split Selects By";
            // 
            // RdAlpha
            // 
            this.RdAlpha.AutoSize = true;
            this.RdAlpha.Checked = true;
            this.RdAlpha.Location = new System.Drawing.Point(6, 42);
            this.RdAlpha.Name = "RdAlpha";
            this.RdAlpha.Size = new System.Drawing.Size(83, 17);
            this.RdAlpha.TabIndex = 3;
            this.RdAlpha.TabStop = true;
            this.RdAlpha.Text = "Alphabetical";
            this.RdAlpha.UseVisualStyleBackColor = true;
            this.RdAlpha.CheckedChanged += new System.EventHandler(this.RdSort_CheckedChanged);
            // 
            // RdType
            // 
            this.RdType.AutoSize = true;
            this.RdType.Location = new System.Drawing.Point(6, 19);
            this.RdType.Name = "RdType";
            this.RdType.Size = new System.Drawing.Size(49, 17);
            this.RdType.TabIndex = 2;
            this.RdType.Text = "Type";
            this.RdType.UseVisualStyleBackColor = true;
            this.RdType.CheckedChanged += new System.EventHandler(this.RdSort_CheckedChanged);
            // 
            // ToolTips
            // 
            this.ToolTips.AutoPopDelay = 9000;
            this.ToolTips.InitialDelay = 300;
            this.ToolTips.ReshowDelay = 180;
            this.ToolTips.ShowAlways = true;
            // 
            // BelowZeroSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flowMain);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "BelowZeroSettings";
            this.Size = new System.Drawing.Size(472, 152);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.flowMain.ResumeLayout(false);
            this.flowMain.PerformLayout();
            this.flowOptions.ResumeLayout(false);
            this.Options_GroupBox.ResumeLayout(false);
            this.Other_GroupBox.ResumeLayout(false);
            this.Other_GroupBox.PerformLayout();
            this.StartReset_GroupBox.ResumeLayout(false);
            this.StartReset_GroupBox.PerformLayout();
            this.SortBy_GroupBox.ResumeLayout(false);
            this.SortBy_GroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowMain;
        private System.Windows.Forms.FlowLayoutPanel flowOptions;
        private System.Windows.Forms.GroupBox Options_GroupBox;
        private System.Windows.Forms.Button btnAddSplit;
        private System.Windows.Forms.GroupBox StartReset_GroupBox;
        private System.Windows.Forms.GroupBox SortBy_GroupBox;
        private System.Windows.Forms.RadioButton RdAlpha;
        private System.Windows.Forms.RadioButton RdType;
        private System.Windows.Forms.CheckBox chkCreativeStart;
        private System.Windows.Forms.CheckBox chkIntroStart;
        private System.Windows.Forms.ToolTip ToolTips;
        private System.Windows.Forms.GroupBox Other_GroupBox;
        private System.Windows.Forms.CheckBox chkAskForGoldSave;
        private System.Windows.Forms.CheckBox chkReset;
        private System.Windows.Forms.Button ButtonSplitGenerator;
        private System.Windows.Forms.CheckBox cbOrderedLiveSplit;
        private System.Windows.Forms.CheckBox cbOrderedAutoSplits;
        private System.Windows.Forms.CheckBox chkDiscordStatus;
    }
}