using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

// This class manages the Quest editor Interface
// FIXME: Rename, not a good name any more
public class QuestEditorData {

    // Not used yet
    //private readonly StringKey COMPONENT_TO_DELETE = new StringKey("val", "COMPONENT_TO_DELETE");

    // This is the currently selected component
    public EditorComponent selection;
    // The selection stack is used for the 'back' button
    public Stack<EditorComponent> selectionStack;

    // Start the editor
    public QuestEditorData()
    {
        Game.Get().qed = this;       
        selectionStack = new Stack<EditorComponent>();
        // Start at the Quest component
        SelectQuest();
    }

    // Start the editor from old version
    public QuestEditorData(QuestEditorData old)
    {
        Game.Get().qed = this;
        selectionStack = new Stack<EditorComponent>();
        if (old == null || old.selection == null)
        {
            // Start at the Quest component
            SelectQuest();
        }
        else
        {
            selectionStack = old.selectionStack;
            selectionStack.Push(old.selection);
            Back();
        }
    }

    // Update component selection
    // Used to save selection history
    public void NewSelection(EditorComponent c)
    {
        // selection starts at null
        if (selection != null)
        {
            selectionStack.Push(selection);
        }
        selection = c;
    }

    // Go back in the selection stack
    public static void Back()
    {
        Game game = Game.Get();
        // Check if there is anything to go back to
        if (game.qed.selectionStack.Count == 0)
        {
            // Reset on Quest selection
            game.qed.selection = new EditorComponentQuest();
            return;
        }
        game.qed.selection = game.qed.selectionStack.Pop();
        // Quest is OK
        if (game.qed.selection is EditorComponentQuest)
        {
            game.qed.selection.Update();
        }
        else if (game.quest.qd.components.ContainsKey(game.qed.selection.name))
        {
            // Existing component
            game.qed.selection.Update();
        }
        else
        {
            // History item has been deleted/renamed, go back further
            Back();
        }
    }

    // Open component selection top level
    // Menu for selection of all component types, includes delete options
    public static void TypeSelect(string type = "")
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectComponent, CommonStringKeys.SELECT_ITEM);

        select.AddNewComponentItem("TileQuestComponent");
        select.AddNewComponentItem("TokenQuestComponent");
        select.AddNewComponentItem("SpawnQuestComponent");
        select.AddNewComponentItem("EventQuestComponent");
        select.AddNewComponentItem("CustomMonsterQuestComponent");
        select.AddNewComponentItem("UiQuestComponent");
        select.AddNewComponentItem("QItemQuestComponent");
        if (game.gameType is D2EGameType || game.gameType is IAGameType)
        {
            select.AddNewComponentItem("ActivationQuestComponent");
            select.AddNewComponentItem("DoorQuestComponent");
            select.AddNewComponentItem("MPlaceQuestComponent");
        }
        else
        {
            select.AddNewComponentItem("PuzzleQuestComponent");
        }

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        select.AddItem(CommonStringKeys.QUEST.Translate(), "Quest", traits);

        foreach (QuestComponent c in game.quest.qd.components.Values)
        {
            if (!(c is PerilData))
            {
                select.AddItem(c);
            }
        }

        select.Draw();
        if (type.Length > 0)
        {
            select.SelectTrait(CommonStringKeys.TYPE.Translate(), new StringKey("val", type.ToUpper()).Translate());
        }
    }

    // Select a component for editing
    public static void SelectComponent(string name)
    {
        Game game = Game.Get();
        QuestEditorData qed = game.qed;

        // Quest is a special component
        if (name.Equals("Quest"))
        {
            SelectQuest();
            return;
        }
        // These are special strings for creating new objects
        if (name.Equals("{NEW:TileQuestComponent}"))
        {
            qed.NewTile();
            return;
        }
        if (name.Equals("{NEW:DoorQuestComponent}"))
        {
            qed.NewDoor();
            return;
        }
        if (name.Equals("{NEW:TokenQuestComponent}"))
        {
            qed.NewToken();
            return;
        }
        if (name.Equals("{NEW:UiQuestComponent}"))
        {
            qed.NewUI();
            return;
        }
        if (name.Equals("{NEW:SpawnQuestComponent}"))
        {
            qed.NewSpawn();
            return;
        }
        if (name.Equals("{NEW:MPlaceQuestComponent}"))
        {
            qed.NewMPlace();
            return;
        }
        if (name.Equals("{NEW:QItemQuestComponent}"))
        {
            qed.NewItem();
            return;
        }
        if (name.Equals("{NEW:CustomMonsterQuestComponent}"))
        {
            qed.NewCustomMonster();
            return;
        }
        if (name.Equals("{NEW:ActivationQuestComponent}"))
        {
            qed.NewActivation();
            return;
        }
        if (name.Equals("{NEW:EventQuestComponent}"))
        {
            qed.NewEvent();
            return;
        }
        if (name.Equals("{NEW:PuzzleQuestComponent}"))
        {
            qed.NewPuzzle();
            return;
        }
        // This may happen to due rename/delete
        if (!game.quest.qd.components.ContainsKey(name))
        {
            SelectQuest();
        }

        // Determine the component type and select
        if (game.quest.qd.components[name] is TileQuestComponent)
        {
            SelectAsTile(name);
            return;
        }

        if (game.quest.qd.components[name] is DoorQuestComponent)
        {
            SelectAsDoor(name);
            return;
        }
        if (game.quest.qd.components[name] is TokenQuestComponent)
        {
            SelectAsToken(name);
            return;
        }
        if (game.quest.qd.components[name] is UiQuestComponent)
        {
            SelectAsUI(name);
            return;
        }
        if (game.quest.qd.components[name] is SpawnQuestComponent)
        {
            SelectAsSpawn(name);
            return;
        }
        if (game.quest.qd.components[name] is MPlaceQuestComponent)
        {
            SelectAsMPlace(name);
            return;
        }
        if (game.quest.qd.components[name] is PuzzleQuestComponent)
        {
            SelectAsPuzzle(name);
            return;
        }
        if (game.quest.qd.components[name] is QItemQuestComponent)
        {
            SelectAsItem(name);
            return;
        }
        if (game.quest.qd.components[name] is CustomMonsterQuestComponent)
        {
            SelectAsCustomMonster(name);
            return;
        }
        if (game.quest.qd.components[name] is ActivationQuestComponent)
        {
            SelectAsActivation(name);
            return;
        }
        if (game.quest.qd.components[name] is EventQuestComponent)
        {
            SelectAsEvent(name);
            return;
        }
    }

    public static void SelectQuest()
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentQuest());
    }

    public static void SelectAsTile(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentTile(name));
    }

    public static void SelectAsDoor(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentDoor(name));
    }

    public static void SelectAsToken(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentToken(name));
    }

    public static void SelectAsUI(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentUI(name));
    }

    public static void SelectAsEvent(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentEvent(name));
    }

    public static void SelectAsSpawn(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentSpawn(name));
    }

    public static void SelectAsMPlace(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentMPlace(name));
    }

    public static void SelectAsPuzzle(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentPuzzle(name));
    }

    public static void SelectAsItem(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentItem(name));
    }
    public static void SelectAsCustomMonster(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentCustomMonster(name));
    }
    public static void SelectAsActivation(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentActivation(name));
    }

    // Create a new tileQuestComponent, use next available number
    public void NewTile()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("TileQuestComponent" + index))
        {
            index++;
        }
        TileQuestComponent tileQuestComponent = new TileQuestComponent("TileQuestComponent" + index);
        game.quest.qd.components.Add("TileQuestComponent" + index, tileQuestComponent);

        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        tileQuestComponent.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
        tileQuestComponent.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

        game.quest.Add("TileQuestComponent" + index);
        SelectComponent("TileQuestComponent" + index);
    }

    public void NewDoor()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("DoorQuestComponent" + index))
        {
            index++;
        }
        DoorQuestComponent doorQuestComponent = new DoorQuestComponent("DoorQuestComponent" + index);
        game.quest.qd.components.Add("DoorQuestComponent" + index, doorQuestComponent);

        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        doorQuestComponent.location.x = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.SelectionRound());
        doorQuestComponent.location.y = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.SelectionRound());

        game.quest.Add("DoorQuestComponent" + index);
        SelectComponent("DoorQuestComponent" + index);
    }

    public void NewToken()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("TokenQuestComponent" + index))
        {
            index++;
        }
        TokenQuestComponent tokenQuestComponent = new TokenQuestComponent("TokenQuestComponent" + index);
        game.quest.qd.components.Add("TokenQuestComponent" + index, tokenQuestComponent);

        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        tokenQuestComponent.location.x = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.SelectionRound());
        tokenQuestComponent.location.y = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.SelectionRound());

        game.quest.Add("TokenQuestComponent" + index);
        SelectComponent("TokenQuestComponent" + index);
    }

    public void NewUI()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("UiQuestComponent" + index))
        {
            index++;
        }
        UiQuestComponent uiQuestComponent = new UiQuestComponent("UiQuestComponent" + index);
        game.quest.qd.components.Add("UiQuestComponent" + index, uiQuestComponent);
        SelectComponent("UiQuestComponent" + index);
    }

    public void NewSpawn()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("SpawnQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("SpawnQuestComponent" + index, new SpawnQuestComponent("SpawnQuestComponent" + index));
        SelectComponent("SpawnQuestComponent" + index);
    }

    public void NewMPlace()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("MPlaceQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("MPlaceQuestComponent" + index, new MPlaceQuestComponent("MPlaceQuestComponent" + index));
        SelectComponent("MPlaceQuestComponent" + index);
    }

    public void NewEvent()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("EventQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("EventQuestComponent" + index, new EventQuestComponent("EventQuestComponent" + index));
        SelectComponent("EventQuestComponent" + index);
    }

    public void NewPuzzle()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("PuzzleQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("PuzzleQuestComponent" + index, new PuzzleQuestComponent("PuzzleQuestComponent" + index));
        SelectComponent("PuzzleQuestComponent" + index);
    }
    
    public void NewItem()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("QItemQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("QItemQuestComponent" + index, new QItemQuestComponent("QItemQuestComponent" + index));
        SelectComponent("QItemQuestComponent" + index);
    }

    public void NewCustomMonster()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("CustomMonsterQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("CustomMonsterQuestComponent" + index, new CustomMonsterQuestComponent("CustomMonsterQuestComponent" + index));
        SelectComponent("CustomMonsterQuestComponent" + index);
    }

    public void NewActivation()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("ActivationQuestComponent" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("ActivationQuestComponent" + index, new ActivationQuestComponent("ActivationQuestComponent" + index));
        SelectComponent("ActivationQuestComponent" + index);
    }

    // Item selected from list for deletion
    public static void DeleteCurrentComponent()
    {
        Game game = Game.Get();
        string name = game.qed.selection.name;
        Destroyer.Dialog();

        if (name.Length == 0) return;

        // Remove all references to the deleted component
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            kv.Value.RemoveReference(name);
        }

        // Remove the component
        if (game.quest.qd.components.ContainsKey(name))
        {
            game.quest.qd.components.Remove(name);
        }

        LocalizationRead.dicts["qst"].RemoveKeyPrefix(name + ".");

        // Clean up the current Quest environment
        game.quest.Remove(name);

        Back();
    }

    // Cancel a component selection, clean up
    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);
    }

    // This is called by game
    public void MouseDown()
    {
        selection.MouseDown();
    }

    // This is called by game
    public void RightClick()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count == 0) return;

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectComponent, CommonStringKeys.SELECT_ITEM);

        int count = 0;
        string last = "";
        foreach (RaycastResult hit in raycastResults)
        {
            foreach (KeyValuePair<string, Quest.BoardComponent> kv in Game.Get().quest.boardItems)
            {
                if (kv.Value.unityObject == hit.gameObject)
                {
                    if (kv.Key.IndexOf("UiQuestComponent") != 0)
                    {
                        last = kv.Key;
                        count++;
                        select.AddItem(Game.Get().quest.qd.components[kv.Key]);
                    }
                    break;
                }
            }
        }
        if (count == 1) SelectComponent(last);
        if (count > 1) select.Draw();
    }
}
