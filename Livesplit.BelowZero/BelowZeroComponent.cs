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
        private readonly HashSet<BelowZeroSplit> alreadySplit = new HashSet<BelowZeroSplit>();
        private bool survivalStartArmed;

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
            memory = new BelowZeroMemory(logger, settings);
            timerModel = new TimerModel() { CurrentState = state };
        }

        private void LogUpdateState(string state)
        {
            if (lastUpdateState == state)
                return;

            lastUpdateState = state;
            logger.Log(state);
        }

        private bool MarkStarted(string reason)
        {
            logger.Log(reason);
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

        private bool TryInputStart()
        {
            bool inputEnabled = memory.PlayerControllerInputEnabled?.New ?? false;

            if (memory.HorizontalMovementStarted())
                return MarkStarted("Start of Move");

            if (inputEnabled && memory.PlayerJumped())
                return MarkStarted("Start of Jump");

            if (memory.PDAOpenedOrClosed())
                return MarkStarted("Start of PDA Open/Close");

            if (inputEnabled && memory.PickedUpSnowball())
                return MarkStarted("Start of Snowball Pickup");

            if (inputEnabled && memory.BuilderMenuOpened())
                return MarkStarted("Start of Builder Menu");

            if (memory.CraftingMenuOpened())
                return MarkStarted("Start of Crafting Menu");

            return false;
        }

        private void UpdateSurvivalStartState()
        {
            if (!settings.IntroStart)
            {
                ResetSurvivalStartState();
                return;
            }

            if (!survivalStartArmed && (memory.IsIntroCinematicActive?.New ?? false))
            {
                survivalStartArmed = true;
                logger.Log("Survival Start armed: intro cinematic started.");
            }
        }

        private bool TrySurvivalStart()
        {
            if (!survivalStartArmed)
                return false;

            bool cinematicEnded = !(memory.IsIntroCinematicActive?.New ?? true)
                && !(memory.IsLoadingScreenShowing?.New ?? true);

            if (cinematicEnded && memory.PlayerMovedHorizontally())
                return MarkStarted("Survival Start: first actual horizontal player movement");

            if (memory.IntroCompleted())
                return MarkStarted("Survival Start fallback: intro completed and gameplay resumed");

            return false;
        }

        private void ResetSurvivalStartState()
        {
            survivalStartArmed = false;
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

            if (memory.isInMainMenu)
            {
                ResetSurvivalStartState();
                return false;
            }

            UpdateSurvivalStartState();

            if (settings.IntroStart && survivalStartArmed)
                return TrySurvivalStart();

            if (!settings.CreativeStart
                || memory.PlayerMain?.New == IntPtr.Zero
                || (memory.IsLoadingScreenShowing?.New ?? true)
                || (memory.IsIntroCinematicActive?.New ?? true)
                || !(memory.PlayerControllerInputEnabled?.New ?? false))
                return false;

            return TryInputStart();
        }

        public override bool Split()
        {
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

        public override bool Loading() => false;

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
            ResetSurvivalStartState();
            memory.ResetRunState();
            logger.Log("Run state cleared on timer reset.");
        }
    }
}

