using Assets.Scripts.Quest.BoardComponents;
using UnityEngine;

namespace Assets.Scripts.Quest.Tokens
{
    // Class for tokens and doors that will get the onClick event
    public class TokenControl
    {
        BoardComponent c;

        // Initialise from a door
        public TokenControl(BoardComponent component)
        {
            // If we are in the editor we don't add the buttons
            if (Game.Get().editMode) return;

            c = component;
            UnityEngine.UI.Button button = c.unityObject.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { startEvent(); });
        }

        // On click the tokens start an event
        public void startEvent()
        {
            Game game = Game.Get();

            // If in horror phase ignore token, accept Ui element (items)
            if (c.GetEvent().typeDynamic == "Token" && game.quest.phase != MoMPhase.investigator) return;

            // If a dialog is open ignore
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
                return;
            // Spawn a window with the door/token info
            game.quest.eManager.QueueEvent(c.GetEvent().sectionName);
        }

    }
}
