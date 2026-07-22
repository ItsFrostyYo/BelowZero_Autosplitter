using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Timers;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.DropPodLocation
{
    public sealed class Component : IComponent, IDisposable
    {
        private readonly InfoTextComponent internalComponent;
        private readonly DropPodLogReader locationReader;
        private readonly System.Timers.Timer updateTimer;

        public Settings Settings { get; }

        public string ComponentName => "Below Zero Drop Pod Location";
        public float HorizontalWidth => internalComponent.HorizontalWidth;
        public float MinimumHeight => internalComponent.MinimumHeight;
        public float VerticalHeight => internalComponent.VerticalHeight;
        public float MinimumWidth => internalComponent.MinimumWidth;
        public float PaddingTop => internalComponent.PaddingTop;
        public float PaddingBottom => internalComponent.PaddingBottom;
        public float PaddingLeft => internalComponent.PaddingLeft;
        public float PaddingRight => internalComponent.PaddingRight;
        public IDictionary<string, Action> ContextMenuControls => null;

        public Component(LiveSplitState state)
        {
            internalComponent = new InfoTextComponent("Drop Pod Location", "Unknown");
            Settings = new Settings();
            locationReader = new DropPodLogReader();
            updateTimer = new System.Timers.Timer(100.0)
            {
                AutoReset = true
            };
            updateTimer.Elapsed += UpdateLocation;
            updateTimer.Start();
        }

        public void Dispose()
        {
            updateTimer.Stop();
            updateTimer.Elapsed -= UpdateLocation;
            updateTimer.Dispose();
            locationReader.Dispose();
            internalComponent.Dispose();
        }

        public void DrawHorizontal(Graphics graphics, LiveSplitState state, float height, Region clipRegion)
        {
            DrawBackground(graphics, HorizontalWidth, height);
            PrepareDraw(state);
            internalComponent.DrawHorizontal(graphics, state, height, clipRegion);
        }

        public void DrawVertical(Graphics graphics, LiveSplitState state, float width, Region clipRegion)
        {
            DrawBackground(graphics, width, VerticalHeight);
            PrepareDraw(state);
            internalComponent.DrawVertical(graphics, state, width, clipRegion);
        }

        public XmlNode GetSettings(XmlDocument document) => Settings.GetSettings(document);

        public Control GetSettingsControl(LayoutMode mode) => Settings;

        public void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            internalComponent.Update(invalidator, state, width, height, mode);
        }

        private void PrepareDraw(LiveSplitState state)
        {
            bool dropShadows = state.LayoutSettings.DropShadows;
            internalComponent.NameLabel.HasShadow = dropShadows;
            internalComponent.ValueLabel.HasShadow = dropShadows;
            internalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            internalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
        }

        private void DrawBackground(Graphics graphics, float width, float height)
        {
            if (Settings.BackgroundColor.A == 0
                && (Settings.BackgroundGradient == GradientType.Plain || Settings.BackgroundColor2.A == 0))
            {
                return;
            }

            PointF endPoint = Settings.BackgroundGradient == GradientType.Horizontal
                ? new PointF(width, 0f)
                : new PointF(0f, height);
            Color endColor = Settings.BackgroundGradient == GradientType.Plain
                ? Settings.BackgroundColor
                : Settings.BackgroundColor2;

            using (var brush = new LinearGradientBrush(
                new PointF(0f, 0f),
                endPoint,
                Settings.BackgroundColor,
                endColor))
            {
                graphics.FillRectangle(brush, 0f, 0f, width, height);
            }
        }

        private void UpdateLocation(object sender, ElapsedEventArgs e)
        {
            internalComponent.InformationValue = locationReader.Update();
        }
    }
}
