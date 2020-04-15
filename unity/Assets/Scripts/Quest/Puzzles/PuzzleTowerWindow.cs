using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponents;
using Assets.Scripts.Quest.Events;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Quest.Puzzles
{
    public class PuzzleTowerWindow : IUpdateListener
    {
        public ValkyrieEvent ValkyrieEventData;
        PuzzleQuestComponent QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT;
        public PuzzleTower puzzle;
        public int lastMoves = 0;
        protected bool windowClosed = false;
        protected List<UIElement> towerBoxes = new List<UIElement>();
        protected int startDragTower = -1;
        protected int hoverTower = -1;
        protected List<List<int>> puzzleDisplay;

        public PuzzleTowerWindow(ValkyrieEvent e)
        {
            ValkyrieEventData = e;
            Game game = Game.Get();

            game.AddUpdateListener(this as IUpdateListener);
            QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT =
                e.QEventQuestComponent as PuzzleQuestComponent;

            if (game.quest.puzzle.ContainsKey(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName))
            {
                puzzle = game.quest.puzzle[QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName] as PuzzleTower;
                lastMoves = puzzle.moves;
            }
            else
            {
                puzzle = new PuzzleTower(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.puzzleLevel);
            }

            puzzleDisplay = puzzle.CopyState(puzzle.puzzle);

            CreateWindow();
        }

        public void CreateWindow()
        {
            Destroyer.Dialog();
            UIElement ui = new UIElement();
            towerBoxes = new List<UIElement>();

            ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, 22.5f);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-13.5f), 1, 9, 16.5f);
            new UIElementBorder(ui);
            towerBoxes.Add(ui);
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 1, 9, 16.5f);
            new UIElementBorder(ui);
            towerBoxes.Add(ui);
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(4.5f), 1, 9, 16.5f);
            new UIElementBorder(ui);
            towerBoxes.Add(ui);

            for (int i = 0; i < puzzleDisplay.Count; i++)
            {
                CreateTower(UIScaler.GetHCenter(-9) + (i * 9), 17f, puzzleDisplay[i]);
            }

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-5), 18, 7, 2);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.SKILL));
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(2), 18, 3, 2);
            ui.SetText(EventManager.OutputSymbolReplace(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.skill));
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-11f), 20.5f, 6, 2);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.MOVES));
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-5), 20.5f, 3, 2);
            ui.SetText((puzzle.moves - lastMoves).ToString());
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-2f), 20.5f, 10, 2);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TOTAL_MOVES));
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(8), 20.5f, 3, 2);
            ui.SetText(puzzle.moves.ToString());
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-13), 24, 8, 2);
            if (puzzle.Solved())
            {
                ui.SetText(CommonStringKeys.CLOSE, Color.grey);
                new UIElementBorder(ui, Color.grey);
            }
            else
            {
                ui.SetText(CommonStringKeys.CLOSE);
                new UIElementBorder(ui);
                ui.SetButton(Close);
            }

            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(5), 24, 8, 2);
            if (!puzzle.Solved())
            {
                ui.SetText(ValkyrieEventData.GetButtons()[0].GetLabel(), Color.grey);
                new UIElementBorder(ui, Color.grey);
            }
            else
            {
                ui.SetText(ValkyrieEventData.GetButtons()[0].GetLabel());
                new UIElementBorder(ui);
                ui.SetButton(Finished);
            }

            ui.SetFontSize(UIScaler.GetMediumFont());
        }

        public void Close()
        {
            Destroyer.Dialog();
            Game game = Game.Get();
            if (game.quest.puzzle.ContainsKey(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName))
            {
                game.quest.puzzle.Remove(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName);
            }

            game.quest.puzzle.Add(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName, puzzle);

            game.quest.eManager.CurrentValkyrieEvent = null;
            game.quest.eManager.CurrentValkyrieEvent = null;

            windowClosed = true;
            game.quest.eManager.TriggerEvent();
        }

        public void Finished()
        {
            Destroyer.Dialog();
            Game game = Game.Get();
            if (game.quest.puzzle.ContainsKey(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName))
            {
                game.quest.puzzle.Remove(QUEST_PUZZLE_QUEST_COMPONENT_QUEST_COMPONENT.sectionName);
            }

            windowClosed = true;
            game.quest.eManager.EndEvent();
        }

        /// <summary>
        /// Draw a tower
        /// </summary>
        /// <param name="hCentre">Horizontal centre of the tower in UIScaler units</param>
        /// <param name="vBottom">Vertiacal bottom of the tower in UIScaler units</param>
        /// <param name="blocks">Block sizes to draw</param>
        public void CreateTower(float hCentre, float vBottom, List<int> blocks)
        {
            foreach (int size in blocks)
            {
                CreateBlock(hCentre, vBottom, size);
                vBottom -= 2;
            }
        }

        /// <summary>
        /// Draw a tower
        /// </summary>
        /// <param name="hCentre">Horizontal centre of the block in UIScaler units</param>
        /// <param name="vBottom">Vertiacal bottom of the block in UIScaler units</param>
        /// <param name="blocks">Block size to draw</param>
        public void CreateBlock(float hCentre, float vBottom, int size)
        {
            UIElement ui = new UIElement();
            ui.SetLocation(hCentre - ((size + 1.5f) / 2), vBottom - 1.5f, size + 1.5f, 1.5f);
            ui.SetBGColor(new Color(0.6f, 0.6f, 0f, 1f));
            new UIElementBorder(ui, Color.yellow);
        }

        /// <summary>
        /// This method is called on click.
        /// </summary>
        void IUpdateListener.Click()
        {
            if (puzzle.Solved()) return;

            for (int i = 0; i < towerBoxes.Count; i++)
            {
                if (towerBoxes[i].AtLocationPixels(Input.mousePosition.x, Screen.height - Input.mousePosition.y))
                {
                    startDragTower = i;
                    hoverTower = i;
                }
            }
        }

        /// <summary>
        /// This method is called on Unity Update.  Must return false to allow garbage collection.
        /// </summary>
        /// <returns>True to keep this in the update list, false to remove.</returns>
        bool IUpdateListener.Update()
        {
            if (startDragTower < 0) return !windowClosed;

            int newHover = -1;
            for (int i = 0; i < towerBoxes.Count; i++)
            {
                if (towerBoxes[i].AtLocationPixels(Input.mousePosition.x, Screen.height - Input.mousePosition.y))
                {
                    newHover = i;
                }
            }

            if (hoverTower != newHover)
            {
                hoverTower = newHover;
                puzzleDisplay = puzzle.CopyState(puzzle.puzzle);
                if (hoverTower >= 0)
                {
                    puzzle.Move(startDragTower, hoverTower, puzzleDisplay);
                }

                CreateWindow();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (puzzle.MoveOK(startDragTower, hoverTower))
                {
                    puzzle.moves++;
                    puzzle.Move(startDragTower, hoverTower);
                }

                puzzleDisplay = puzzle.CopyState(puzzle.puzzle);
                startDragTower = -1;
                CreateWindow();
            }

            return true;
        }
    }
}