using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.BelowZero;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Voxif.AutoSplitter;
using Voxif.IO;

namespace LiveSplit.BelowZero
{
    public class BelowZeroComponent : Voxif.AutoSplitter.Component
    {
        private readonly BelowZeroMemory memory;
        private readonly LiveSplitState _state;
        private readonly TimerModel timerModel;
        private string lastUpdateState = string.Empty;
        private string lastOrderedSplitDescription = string.Empty;
        public readonly HashSet<BelowZeroSplit> alreadySplit = new HashSet<BelowZeroSplit>();

        public BelowZeroComponent(LiveSplitState state) : base(state)
        {
            string logFilePath = Path.GetFullPath("_" + Factory.ExAssembly.GetName().Name.Substring(10) + ".log");
            logger =
#if DEBUG || LOG
                new CompositeLogger(
                    new ConsoleLogger(),
                    new FileLogger(logFilePath));
#else
                new FileLogger(logFilePath);
#endif
            logger.StartLogger();
            logger.Log($"Logger started. File: {logFilePath}");
            logger.Log($"Component version: {Factory.ExAssembly.GetName().Version}");

            Localization.Load();
            _state = state;
            settings = new BelowZeroSettings(state);
            memory = new BelowZeroMemory(state, this, logger, settings);
            timerModel = new TimerModel() { CurrentState = state };
        }

        private void LogUpdateState(string state)
        {
            if (lastUpdateState == state)
                return;

            lastUpdateState = state;
            logger.Log(state);
        }

        private void LogStartStateChanges()
        {
            if (memory.PlayerControllerInputEnabled.Changed)
                logger.Log($"Input enabled changed: {memory.PlayerControllerInputEnabled.Old} -> {memory.PlayerControllerInputEnabled.New}");

            if (memory.IsLoadingScreenShowing.Changed)
                logger.Log($"Loading screen changed: {memory.IsLoadingScreenShowing.Old} -> {memory.IsLoadingScreenShowing.New}");

            if (memory.IsIntroCinematicActive.Changed)
                logger.Log($"Intro cinematic changed: {memory.IsIntroCinematicActive.Old} -> {memory.IsIntroCinematicActive.New}");

            if (memory.PDATab.Changed)
                logger.Log($"PDA tab changed: {(PDATab)memory.PDATab.Old} -> {(PDATab)memory.PDATab.New}");
        }

        private bool MarkStarted(string reason)
        {
            logger.Log(reason);
            memory.MovementStartArmed = false;
            memory.startedTimerBefore = true;
            return true;
        }

        private void LogOrderedSplit(BelowZeroSplit split)
        {
            if (!(BelowZeroSettings.OrderedAutoSplits || BelowZeroSettings.OrderedLiveSplit))
            {
                lastOrderedSplitDescription = string.Empty;
                return;
            }

            string description = split?.GetDescription() ?? split?.SplitName.ToString() ?? "Unknown Split";
            if (description == lastOrderedSplitDescription)
                return;

            lastOrderedSplitDescription = description;
            logger.Log($"Ordered split armed: {description}");
        }

        private bool IsAutomaticIntroUiStateActive()
        {
            if (memory.IsLoadingScreenShowing?.New ?? false)
                return true;

            if (memory.IsIntroCinematicPlaying())
                return true;

            return (PDATab)memory.PDATab.New == PDATab.Intro;
        }

        private bool PDAStartTriggered()
        {
            if (!memory.PDATab.Changed)
                return false;

            if (!(memory.PlayerControllerInputEnabled?.New ?? true))
                return false;

            if (memory.IsIntroCinematicPlaying())
                return false;

            if (memory.IsLoadingScreenShowing?.New ?? false)
                return false;

            PDATab oldTab = (PDATab)memory.PDATab.Old;
            PDATab tab = (PDATab)memory.PDATab.New;
            return oldTab == PDATab.None && tab != PDATab.None && tab != PDATab.Intro;
        }

        private bool TryStartFromPlayerAction(string context)
        {
            if (memory.isInMainMenu)
                return false;

            if (memory.PlayerMain?.New == IntPtr.Zero)
                return false;

            if (!(memory.PlayerControllerInputEnabled?.New ?? false))
                return false;

            if (memory.MovementStartArmed
                && !IsAutomaticIntroUiStateActive()
                && memory.IsMovementInputActive(0.001f))
            {
                return MarkStarted($"{context}: Start of Move (held after reset)");
            }

            if (memory.MovementStarted(0.35f, 0.001f))
            {
                return MarkStarted($"{context}: Start of Move");
            }

            bool becameStartEligible =
                (memory.IsLoadingScreenShowing.Changed && !memory.IsLoadingScreenShowing.New)
                || (memory.PlayerControllerInputEnabled.Changed && memory.PlayerControllerInputEnabled.New);

            if (becameStartEligible &&
                (memory.IsMovingHorizontally(0.35f)
                 || memory.IsMovementInputActive(0.001f)))
            {
                return MarkStarted($"{context}: Start of Move (held input)");
            }

            if (memory.IsPlayerJumping.New && memory.IsPlayerJumping.Changed)
            {
                return MarkStarted($"{context}: Start of Jump");
            }

            if (memory.BuilderMenuOpened())
            {
                return MarkStarted($"{context}: Start of Builder Menu");
            }

            if (memory.PickedUpSnowball())
            {
                return MarkStarted($"{context}: Start of Snowball Pickup");
            }

            if (!IsAutomaticIntroUiStateActive() &&
                memory.CraftingMenu.New != IntPtr.Zero &&
                memory.CraftingMenu.Old == IntPtr.Zero)
            {
                return MarkStarted($"{context}: Start of Crafting Menu");
            }

            if (PDAStartTriggered())
            {
                return MarkStarted($"{context}: Start of PDA");
            }

            return false;
        }

        public override bool Update()
        {
            bool ok;

            try
            {
                ok = memory.Update();
            }
            catch (Win32Exception ex)
            {
                logger.Log($"Win32Exception in memory.Update: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                logger.Log($"Unexpected exception in memory.Update: {ex}");
                return false;
            }

            if (!ok)
            {
                LogUpdateState("Waiting for Subnautica Below Zero process or pointer initialization.");
                return false;
            }

            if (!memory.pointersInitialized)
            {
                LogUpdateState("Waiting for pointer initialization.");
                return false;
            }
            LogUpdateState("Autosplitter update loop active.");

            if (memory.BiomeString.Changed)
                logger.Log($"Biome changed: {memory.BiomeString.Old} -> {memory.BiomeString.New}");

            TryResetOnMainMenu();

            return true;
        }

        public override bool Start()
        {
            if (memory.startedTimerBefore || !memory.pointersInitialized || !settings.StartEnabled)
                return false;

            LogStartStateChanges();

            if (!memory.IsLoadingScreenShowing.New &&
                !memory.isInMainMenu &&
                TryStartFromPlayerAction("Start"))
                return true;

            return false;
        }

        public override bool Split()
        {
            // TODO: fix only split once shit
            if (!memory.pointersInitialized)
                return false;

            var splits = settings.Splits;
            
            for (int i = 0; i < splits.Count; i++)
            {
                if ((BelowZeroSettings.OrderedAutoSplits && i != alreadySplit.Count) || (BelowZeroSettings.OrderedLiveSplit && i != _state.CurrentSplitIndex))
                    continue;

                var split = splits[i];
                LogOrderedSplit(split);

                IEnumerable<BelowZeroSplit> conditionsSplits = GetAllConditions(split);
                bool allConditionsMet = true;

                foreach (var conditionSplit in conditionsSplits)
                {
                    memory.CurrentSplitToCheck = conditionSplit;
                    if (memory.subConditions.TryGetValue(conditionSplit.SplitName, out var subCondition) && !subCondition())
                    {
                        allConditionsMet = false;
                        break;
                    }
                }

                memory.CurrentSplitToCheck = split;
                if (allConditionsMet 
                    && memory.splitConditions.TryGetValue(split.SplitName, out var condition) 
                    && condition()
                    && !(split.OnlySplitOnce && !BelowZeroSettings.OrderedAutoSplits && !BelowZeroSettings.OrderedLiveSplit && alreadySplit.Contains(split)))
                {
                    alreadySplit.Add(split);
                    logger.Log($"{split.GetDescription()} triggered");
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<BelowZeroSplit> GetAllConditions(BelowZeroSplit split)
        {
            if (split?.Conditions == null)
                yield break;

            foreach (var c in split.Conditions.Where(c => c.IsSubCondition))
            {
                yield return c;

                foreach (var nested in GetAllConditions(c))
                    yield return nested;
            }
        }

        public override bool Loading() => memory.ShouldPause();

        private void TryResetOnMainMenu()
        {
            if (!settings.Reset)
                return;
            if (_state.CurrentPhase == TimerPhase.NotRunning)
                return;
            if (memory.wasInMainMenu)
                return;
            if (!memory.isInMainMenu)
                return;

            Form ui = _state.Form;
            Action doReset = () =>
            {
                bool GoldSegment = false;
                for (int index = 0; index < _state.Run.Count; index++)
                {
                    if (LiveSplitStateHelper.CheckBestSegment(_state, index, _state.CurrentTimingMethod))
                    {
                        GoldSegment = true;
                        break;
                    }
                }

                bool save = true;
                if (settings.AskForGoldSave && GoldSegment)
                {
                    DialogResult r = MessageBox.Show(
                        ui,
                        "Save splits before resetting?",
                        "Reset",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (r == DialogResult.Cancel)
                        return;

                    save = (r == DialogResult.Yes);
                }

                timerModel.Reset(save);
                logger.Log("Reset at main menu");
            };

            if (ui.InvokeRequired)
                ui.BeginInvoke(doReset);
            else
                doReset();
        }

        public override void OnReset()
        {
            alreadySplit.Clear();
            lastOrderedSplitDescription = string.Empty;
            memory.ResetRunState(armHeldMovementStartAfterReset: true);
            logger.Log("Run state cleared on timer reset.");
        }
    }
}

