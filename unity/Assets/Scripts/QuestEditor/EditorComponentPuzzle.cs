using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.IO;
using Assets.Scripts.Content.QuestComponent;
using ValkyrieTools;

public class EditorComponentPuzzle : EditorComponentEvent
{
    // Not used yet
    //private readonly StringKey PUZZLE = new StringKey("val", "PUZZLE");

    private readonly StringKey PUZZLE_CLASS = new StringKey("val", "PUZZLE_CLASS");
    private readonly StringKey PUZZLE_CLASS_SELECT = new StringKey("val", "PUZZLE_CLASS_SELECT");
    private readonly StringKey PUZZLE_LEVEL = new StringKey("val", "PUZZLE_LEVEL");
    private readonly StringKey IMAGE = new StringKey("val", "IMAGE");
    private readonly StringKey PUZZLE_ALT_LEVEL = new StringKey("val", "PUZZLE_ALT_LEVEL");
    private readonly StringKey PUZZLE_SOLUTION = new StringKey("val", "PUZZLE_SOLUTION");

    private readonly StringKey PUZZLE_SELECT_SKILL = new StringKey("val", "PUZZLE_SELECT_SKILL");
    private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");

    Assets.Scripts.Content.QuestComponent.Puzzle PUZZLE_QUEST_COMPONENT_COMPONENT;

    UIElementEditable levelUIE;
    UIElementEditable altLevelUIE;
    UIElementEditable puzzleSolutionUIE;

    public EditorComponentPuzzle(string nameIn) : base(nameIn)
    {
    }

    override public float AddPosition(float offset)
    {
        return offset;
    }

    /* Write an example puzzle solution with dim grey color:
     * If puzzlelevel=5 and puzzlelevel=4, we give example "1 2 3 4 1". */
    private void ProvidePuzzleSolutionExample()
    {
        string example_str = "";
        // build string
        for (int i=0; i < PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleLevel ; i++) {
            example_str += ((i%PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleAltLevel)+1).ToString() + " ";
        }
        example_str = example_str.TrimEnd(); // kill that last space
        // Set the text field and put it in grey color
        puzzleSolutionUIE.SetText(example_str);
        puzzleSolutionUIE.SetColor(Color.grey);
    }
    
    override public float AddSubEventComponents(float offset)
    {
        PUZZLE_QUEST_COMPONENT_COMPONENT = component as Assets.Scripts.Content.QuestComponent.Puzzle;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", PUZZLE_CLASS));

        // Translate puzzle type trait
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 8, 1);
        ui.SetText(new StringKey("val", PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass));
        ui.SetButton(delegate { Class(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.SKILL));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 6, 1);
        ui.SetText(PUZZLE_QUEST_COMPONENT_COMPONENT.skill);
        ui.SetButton(delegate { Skill(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", PUZZLE_LEVEL));

        levelUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        levelUIE.SetLocation(5, offset, 2, 1);
        levelUIE.SetText(PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleLevel.ToString());
        levelUIE.SetSingleLine();
        levelUIE.SetButton(delegate { UpdateLevel(); });
        new UIElementBorder(levelUIE);
        offset += 2;

        if (PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass.Equals("image") || PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass.Equals("code"))
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", PUZZLE_ALT_LEVEL));

            altLevelUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            altLevelUIE.SetLocation(5, offset, 2, 1);
            altLevelUIE.SetText(PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleAltLevel.ToString());
            altLevelUIE.SetSingleLine();
            altLevelUIE.SetButton(delegate { UpdateAltLevel(); });
            new UIElementBorder(altLevelUIE);
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 3, 1);
            ui.SetText(new StringKey("val", "X_COLON", IMAGE));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(5, offset, 8, 1);
            ui.SetText(PUZZLE_QUEST_COMPONENT_COMPONENT.imageType);
            ui.SetButton(delegate { Image(); });
            new UIElementBorder(ui);
            offset += 2;
        }
        
        if (PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass.Equals("code")) 
        {
            // Initialize the puzzle solution Ui element
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", PUZZLE_SOLUTION));

            puzzleSolutionUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            puzzleSolutionUIE.SetLocation(5, offset, 8, 1);
            /* If there is no set puzzlesolution give an example with gray letters */
            if (PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleSolution.Length == 0) {
                ProvidePuzzleSolutionExample();
            } else { /* otherwise display the solution  */
                puzzleSolutionUIE.SetText(PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleSolution);
            }
            puzzleSolutionUIE.SetSingleLine();
            puzzleSolutionUIE.SetButton(delegate { UpdatePuzzleSolution(); });
            new UIElementBorder(puzzleSolutionUIE);
            offset += 2;
        }

        return offset;
    }
    
    override public float AddEventDialog(float offset)
    {
        return offset;
    }

    override public void Highlight()
    {
    }

    public void Class()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(SelectClass, PUZZLE_CLASS_SELECT);
        select.AddItem(new StringKey("val", "slide"));
        select.AddItem(new StringKey("val", "code"));
        select.AddItem(new StringKey("val", "image"));
        select.AddItem(new StringKey("val", "tower"));
        select.Draw();
    }

    public void SelectClass(string className)
    {
        if (!PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass.Equals(className))
        {
            PUZZLE_QUEST_COMPONENT_COMPONENT.imageType = "";
        }
        // the selection has the key (ie:{val:PUZZLE_SLIDE_CLASS}) so we can build the StringKey.
        PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass = className;
        Update();
    }

    public void Skill()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(SelectSkill, PUZZLE_SELECT_SKILL);
        select.AddItem("{will} " + EventManager.OutputSymbolReplace("{will}"));
        select.AddItem("{strength} " + EventManager.OutputSymbolReplace("{strength}"));
        select.AddItem("{agility} " + EventManager.OutputSymbolReplace("{agility}"));
        select.AddItem("{lore} " + EventManager.OutputSymbolReplace("{lore}"));
        select.AddItem("{influence} " + EventManager.OutputSymbolReplace("{influence}"));
        select.AddItem("{observation} " + EventManager.OutputSymbolReplace("{observation}"));
        select.Draw();
    }

    public void SelectSkill(string skill)
    {
        PUZZLE_QUEST_COMPONENT_COMPONENT.skill = skill.Substring(0, skill.IndexOf(" "));
        Update();
    }

    public void UpdateLevel()
    {
        int.TryParse(levelUIE.GetText(), out PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleLevel);
        Update();
    }

    public void UpdateAltLevel()
    {
        int.TryParse(altLevelUIE.GetText(), out PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleAltLevel);
        Update();
    }

    // A pre-made puzzle solution has been specified. Validate it and update it.
    // A valid solution for a puzzle with level=5 and altlevel=4 might look like "3 2 1 4 1"
    public void UpdatePuzzleSolution()
    {
        ValkyrieDebug.Log("Setting puzzle solution");
        var solutionArray = puzzleSolutionUIE.GetText().Split(" ".ToCharArray());

        // Validate puzzle solution and mark it with red if it's not valid
        bool invalid = false;
        // Check the solution's length
        if (solutionArray.Length != PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleLevel)
        {
            ValkyrieDebug.Log("Puzzle solution must be the same length as puzzleLevel");
            invalid = true;
        }

        // Check the solution's characters to be valid and in range
        foreach (string part in solutionArray)
        {
            int j;
            if (!int.TryParse(part, out j))
            {
                ValkyrieDebug.Log("Solution needs to be a number (e.g. 1 2 3)");
                invalid = true;
            }
            if (j == 0 || j > PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleAltLevel)
            {
                ValkyrieDebug.Log("Puzzle solution " + j + " out of AltLevel " + PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleAltLevel + " range.");
                invalid = true;
            }
        }

        // Error out if it's invalid and reset the puzzle solution
        if (invalid)
        {
            puzzleSolutionUIE.SetColor(Color.red);
            PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleSolution = "";
            return;
        }

        PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleSolution = puzzleSolutionUIE.GetText();
        Update();
    }

    public void Image()
    {
        if (PUZZLE_QUEST_COMPONENT_COMPONENT.puzzleClass.Equals("code"))
        {
            UIWindowSelectionList selectType = new UIWindowSelectionList(SelectImage, SELECT_IMAGE.Translate());
            selectType.AddItem("{NUMBERS}", "");
            selectType.AddItem(new StringKey("val", "SYMBOL").Translate(), "symbol");
            selectType.AddItem(new StringKey("val", "ELEMENT").Translate(), "element");
            selectType.Draw();
            return;
        }

        UIWindowSelectionListImage select = new UIWindowSelectionListImage(SelectImage, SELECT_IMAGE.Translate());

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.SOURCE.Translate(), new string[] { CommonStringKeys.FILE.Translate() });
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (KeyValuePair<string, PuzzleData> kv in Game.Get().cd.puzzles)
        {
            select.AddItem(kv.Value);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectImage(string image)
    {
        PUZZLE_QUEST_COMPONENT_COMPONENT.imageType = image;
        Update();
    }
}
