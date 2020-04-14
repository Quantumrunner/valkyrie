using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using UnityEngine;
using ValkyrieTools;
using Object = System.Object;

namespace Assets.Scripts.Quest.BoardComponents
{
    // Tokens are events that are tied to a token placed on the board
    public class TokenBoardComponent : BoardComponent
    {
        // Quest info on the token
        public Assets.Scripts.Content.QuestComponent.TokenQuestComponent QTokenQuestComponent;

        // Construct with Quest info and reference to Game
        public TokenBoardComponent(Assets.Scripts.Content.QuestComponent.TokenQuestComponent questTokenQuestComponent, Game gameObject) : base(gameObject)
        {
            QTokenQuestComponent = questTokenQuestComponent;

            string tokenName = QTokenQuestComponent.tokenName;
            // Check that token exists
            if (!game.cd.tokens.ContainsKey(tokenName))
            {
                game.quest.log.Add(new LogEntry("Warning: Quest component " + QTokenQuestComponent.sectionName + " is using missing token type: " + tokenName, true));
                // Catch for older quests with different types (0.4.0 or older)
                if (game.cd.tokens.ContainsKey("TokenSearch"))
                {
                    tokenName = "TokenSearch";
                }
            }

            // Get texture for token
            Vector2 texPos = new Vector2(game.cd.tokens[tokenName].x, game.cd.tokens[tokenName].y);
            Vector2 texSize = new Vector2(game.cd.tokens[tokenName].width, game.cd.tokens[tokenName].height);
            Texture2D newTex = ContentData.FileToTexture(game.cd.tokens[tokenName].image, texPos, texSize);
            if (newTex == null)
            {
                ValkyrieDebug.Log("Error: Token " + tokenName + " does not have a valid picture");
                return;
            }

            // Create object
            unityObject = new GameObject("Object" + QTokenQuestComponent.sectionName);
            unityObject.tag = Game.BOARD;

            unityObject.transform.SetParent(game.tokenCanvas.transform);

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = new Color(1, 1, 1, 0);
            image.sprite = tileSprite;

            float PPS = game.cd.tokens[tokenName].pxPerSquare;
            if (PPS == 0)
            {
                PPS = (float)newTex.width;
            }

            // Set the size to the image size
            image.rectTransform.sizeDelta = new Vector2((float)newTex.width / PPS, (float)newTex.height / PPS);
            // Rotate around 0,0 rotation amount
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, QTokenQuestComponent.rotation);
            // Move to square
            unityObject.transform.Translate(new Vector3(QTokenQuestComponent.location.x, QTokenQuestComponent.location.y, 0), Space.World);

            game.tokenBoard.Add(this);
        }

        // Tokens have an associated event to start on press
        public override EventQuestComponent GetEvent()
        {
            return QTokenQuestComponent;
        }

        // Clean up
        public override void Remove()
        {
            UnityEngine.Object.Destroy(unityObject);
        }
    }
}
