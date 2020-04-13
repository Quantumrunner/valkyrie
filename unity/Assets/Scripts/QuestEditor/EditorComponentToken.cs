using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

public class EditorComponentToken : EditorComponentEvent
{
    TokenQuestComponent TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;

    public EditorComponentToken(string nameIn) : base(nameIn)
    {
    }

    override public void Highlight()
    {
        CameraController.SetCamera(component.location);
    }

    override public void AddLocationType(float offset)
    {
    }
    
    override public float AddSubEventComponents(float offset)
    {
        TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT = component as TokenQuestComponent;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation.ToString());
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TYPE));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 12, 1);
        ui.SetText(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.tokenName);
        ui.SetButton(delegate { Type(); });
        new UIElementBorder(ui);
        offset += 2;

        game.quest.ChangeAlpha(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName, 1f);

        return offset;
    }

    override public float AddEventTrigger(float offset)
    {
        return offset;
    }

    override public float AddEventVarConditionComponents(float offset)
    {
        return offset;
    }

    public void Rotate()
    {
        TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation += 90;
        if (TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation > 300)
        {
            TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 0;
        }
        Game.Get().quest.Remove(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Game.Get().quest.Add(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Update();
    }

    public void Type()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListImage(SelectType, new StringKey("val", "SELECT", CommonStringKeys.TOKEN));

        select.AddItem(CommonStringKeys.NONE.Translate(), "{NONE}");

        foreach (KeyValuePair<string, TokenData> kv in game.cd.tokens)
        {
            select.AddItem(kv.Value);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectType(string token)
    {
        TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.tokenName = token.Split(" ".ToCharArray())[0];
        Game.Get().quest.Remove(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Game.Get().quest.Add(TOKEN_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Update();
    }
}
