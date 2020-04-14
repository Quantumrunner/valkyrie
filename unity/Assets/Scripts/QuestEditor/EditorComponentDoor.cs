using Assets.Scripts;
using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

public class EditorComponentDoor : EditorComponentEvent
{
    private readonly StringKey COLOR = new StringKey("val", "COLOR");

    DoorQuestComponent DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;
    // List to select door colour

    public EditorComponentDoor(string nameIn) : base(nameIn)
    {
    }

    override public float AddPosition(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        return offset + 2;
    }

    override public float AddSubEventComponents(float offset)
    {
        DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT = component as DoorQuestComponent;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation.ToString() + "˚");
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(COLOR);
        ui.SetButton(delegate { Colour(); });
        new UIElementBorder(ui);
        offset += 2;

        game.quest.ChangeAlpha(DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName, 1f);

        return offset;
    }

    override public float AddEventTrigger(float offset)
    {
        return offset;
    }

    public void Rotate()
    {
        if (DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation == 0)
        {
            DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 90;
        }
        else
        {
            DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 0;
        }
        Game.Get().quest.Remove(DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Game.Get().quest.Add(DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Update();
    }

    public void Colour()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(SelectColour, CommonStringKeys.SELECT_ITEM);

        foreach (string s in ColorUtil.LookUp().Keys)
        {
            select.AddItem(s);
        }

        select.Draw();
    }

    public void SelectColour(string color)
    {
        DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.colourName = color;
        Game.Get().quest.Remove(DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Game.Get().quest.Add(DOOR_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Update();
    }

}
