using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.GameTypes;
using Assets.Scripts.UI;

namespace Assets.Scripts.Quest
{
    // Special class for the Menu button present while in a Quest
    public class SkillButton
    {
        private StringKey SKILLS = new StringKey("val", "SKILLS");

        public SkillButton()
        {
            Game game = Game.Get();
            if (game.editMode) return;

            if (game.gameType is MoMGameType) return;

            UIElement ui = new UIElement(Game.QUESTUI);
            ui.SetLocation(10.5f, UIScaler.GetBottom(-2.5f), 5, 2);
            ui.SetText(SKILLS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Skills);
            new UIElementBorder(ui);
        }

        // When pressed bring up the approriate menu
        public void Skills()
        {
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;
            if (GameObject.FindGameObjectWithTag(Game.ACTIVATION) != null) return;
            new SkillWindow();
        }
    }
}