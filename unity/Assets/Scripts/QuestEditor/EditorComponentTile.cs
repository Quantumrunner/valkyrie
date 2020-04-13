using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

public class EditorComponentTile : EditorComponent
{
    TileQuestComponent TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;

    public EditorComponentTile(string nameIn) : base()
    {
        Game game = Game.Get();
        TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT = game.quest.qd.components[nameIn] as TileQuestComponent;
        component = TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT = component as TileQuestComponent;
    }

    override public float AddSubComponents(float offset)
    {
        Game game = Game.Get();
        CameraController.SetCamera(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.location);

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "IMAGE")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4.5f, offset, 15, 1);
        ui.SetText(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.tileSideName);
        ui.SetButton(delegate { ChangeTileSide(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_FREE);
        ui.SetButton(delegate { GetPosition(false); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation.ToString());
        ui.SetButton(delegate { TileRotate(); });
        new UIElementBorder(ui);
        offset += 2;

        game.tokenBoard.AddHighlight(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.location, "TileAnchor", Game.EDITOR);

        game.quest.ChangeAlpha(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName, 1f);

        return offset;
    }

    public void ChangeTileSide()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListImage(SelectTileSide, new StringKey("val", "SELECT", CommonStringKeys.TILE));

        // Work out what sides are used
        HashSet<string> usedSides = new HashSet<string>();
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            TileQuestComponent t = kv.Value as TileQuestComponent;
            if (t != null)
            {
                usedSides.Add(t.tileSideName);
                usedSides.Add(game.cd.tileSides[t.tileSideName].reverse);
            }
        }

        foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
        {
            if (usedSides.Contains(kv.Key))
            {
                select.AddItem(kv.Value, new Color(0.4f, 0.4f, 1));
            }
            else
            {
                select.AddItem(kv.Value);
            }
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectTileSide(string tile)
    {
        Game game = Game.Get();
        TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.tileSideName = tile.Split(" ".ToCharArray())[0];
        game.quest.Remove(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        game.quest.Add(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        Update();
    }

    public void TileRotate()
    {
        if (TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation == 0)
        {
            TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 90;
        }
        else if (TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation > 0 && TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation <= 100)
        {
            TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 180;
        }
        else if (TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation > 100 && TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation <= 190)
        {
            TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 270;
        }
        else
        {
            TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotation = 0;
        }

        Game game = Game.Get();
        game.quest.Remove(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);
        game.quest.Add(TILE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName);

        Update();
    }
}
