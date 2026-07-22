using LiveSplit.UI;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.DropPodLocation
{
    public sealed class Settings : UserControl
    {
        private static readonly Color DefaultBackgroundColor = Color.FromArgb(unchecked((int)0xFF000000));

        private readonly Button firstColorButton;
        private readonly Button secondColorButton;
        private readonly ComboBox gradientTypeComboBox;

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }

        public string GradientString
        {
            get => BackgroundGradient.ToString();
            set
            {
                if (Enum.TryParse(value, out GradientType gradient))
                    BackgroundGradient = gradient;
            }
        }

        public Settings()
        {
            BackgroundColor = DefaultBackgroundColor;
            BackgroundColor2 = DefaultBackgroundColor;
            BackgroundGradient = GradientType.Plain;

            var label = new Label
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Text = "Background Color:"
            };

            firstColorButton = CreateColorButton();
            secondColorButton = CreateColorButton();
            gradientTypeComboBox = new ComboBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gradientTypeComboBox.Items.AddRange(new object[] { "Plain", "Vertical", "Horizontal" });

            var layout = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 4,
                Location = new Point(7, 7),
                RowCount = 1,
                Size = new Size(462, 29)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 159f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 29f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 29f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));
            layout.Controls.Add(label, 0, 0);
            layout.Controls.Add(firstColorButton, 1, 0);
            layout.Controls.Add(secondColorButton, 2, 0);
            layout.Controls.Add(gradientTypeComboBox, 3, 0);

            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Size = new Size(476, 43);
            Controls.Add(layout);

            firstColorButton.Click += ColorButtonClick;
            secondColorButton.Click += ColorButtonClick;
            gradientTypeComboBox.SelectedIndexChanged += GradientTypeChanged;

            firstColorButton.DataBindings.Add(
                "BackColor",
                this,
                nameof(BackgroundColor),
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            gradientTypeComboBox.DataBindings.Add(
                "SelectedItem",
                this,
                nameof(GradientString),
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            UpdateSecondColorBinding();
        }

        public void SetSettings(XmlNode node)
        {
            var element = node as XmlElement;
            if (element == null)
                return;

            BackgroundColor = LiveSplit.UI.SettingsHelper.ParseColor(
                element["BackgroundColor"],
                DefaultBackgroundColor);
            BackgroundColor2 = LiveSplit.UI.SettingsHelper.ParseColor(
                element["BackgroundColor2"],
                DefaultBackgroundColor);
            BackgroundGradient = LiveSplit.UI.SettingsHelper.ParseEnum(
                element["BackgroundGradient"],
                GradientType.Plain);

            RefreshBindings();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement settings = document.CreateElement("Settings");
            LiveSplit.UI.SettingsHelper.CreateSetting(document, settings, "Version", "1.0");
            LiveSplit.UI.SettingsHelper.CreateSetting(document, settings, "BackgroundColor", BackgroundColor);
            LiveSplit.UI.SettingsHelper.CreateSetting(document, settings, "BackgroundColor2", BackgroundColor2);
            LiveSplit.UI.SettingsHelper.CreateSetting(document, settings, "BackgroundGradient", BackgroundGradient);
            return settings;
        }

        private static Button CreateColorButton()
        {
            return new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                FlatStyle = FlatStyle.Popup,
                Margin = new Padding(3),
                Size = new Size(23, 23),
                UseVisualStyleBackColor = false
            };
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            LiveSplit.UI.SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void GradientTypeChanged(object sender, EventArgs e)
        {
            if (gradientTypeComboBox.SelectedItem == null)
                return;

            GradientString = gradientTypeComboBox.SelectedItem.ToString();
            UpdateSecondColorBinding();
        }

        private void UpdateSecondColorBinding()
        {
            firstColorButton.Visible = BackgroundGradient != GradientType.Plain;
            secondColorButton.DataBindings.Clear();
            secondColorButton.DataBindings.Add(
                "BackColor",
                this,
                BackgroundGradient == GradientType.Plain
                    ? nameof(BackgroundColor)
                    : nameof(BackgroundColor2),
                false,
                DataSourceUpdateMode.OnPropertyChanged);
        }

        private void RefreshBindings()
        {
            foreach (Binding binding in firstColorButton.DataBindings)
                binding.ReadValue();
            foreach (Binding binding in gradientTypeComboBox.DataBindings)
                binding.ReadValue();

            UpdateSecondColorBinding();
            foreach (Binding binding in secondColorButton.DataBindings)
                binding.ReadValue();
        }
    }
}
