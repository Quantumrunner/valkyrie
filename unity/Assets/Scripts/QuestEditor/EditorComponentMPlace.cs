using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponents;
using Assets.Scripts.Quest;
using Assets.Scripts.UI;

namespace Assets.Scripts.QuestEditor
{
    public class EditorComponentMPlace : EditorComponent
    {
        MPlaceQuestComponent M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;

        public EditorComponentMPlace(string nameIn) : base()
        {
            Game game = Game.Get();
            M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT =
                game.quest.qd.components[nameIn] as MPlaceQuestComponent;
            component = M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;
            name = component.sectionName;
            Update();
        }

        override protected void RefreshReference()
        {
            base.RefreshReference();
            M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT = component as MPlaceQuestComponent;
        }

        override public float AddSubComponents(float offset)
        {
            CameraController.SetCamera(M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.location);
            Game game = Game.Get();

            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 4, 1);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 4, 1);
            ui.SetText(CommonStringKeys.POSITION_SNAP);
            ui.SetButton(delegate { GetPosition(); });
            new UIElementBorder(ui);
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 6, 1);
            ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

            StringKey rotateKey = new StringKey("val", "RIGHT");
            if (M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotate)
            {
                rotateKey = new StringKey("val", "DOWN");
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(6, offset, 4, 1);
            ui.SetText(rotateKey);
            ui.SetButton(delegate { Rotate(); });
            new UIElementBorder(ui);
            offset += 2;

            StringKey mast = new StringKey("val", "MONSTER_MINION");
            if (M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.master)
            {
                mast = new StringKey("val", "MONSTER_MASTER");
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 8, 1);
            ui.SetText(mast);
            ui.SetButton(delegate { MasterToggle(); });
            new UIElementBorder(ui);
            offset += 2;

            game.tokenBoard.AddHighlight(M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.location, "MonsterLoc",
                Game.EDITOR);

            return offset;
        }

        public void Rotate()
        {
            M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotate =
                !M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.rotate;
            Update();
        }

        public void MasterToggle()
        {
            M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.master =
                !M_PLACE_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.master;
            Update();
        }
    }
}