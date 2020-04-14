using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.Quest.Events;
using Assets.Scripts.UI;
using UnityEngine;
using Object = System.Object;

namespace Assets.Scripts.Quest.BoardComponents
{
    // Tokens are events that are tied to a token placed on the board
    public class UIBoardComponent : BoardComponent
    {
        // Quest info on the token
        public UiQuestComponent QUiQuestComponent;
        public UIElementBorder border;

        GameObject unityObject_text = null;
        UnityEngine.UI.Text uiText;
        UnityEngine.UI.Image uiTextBG;

        // Construct with Quest info and reference to Game
        public UIBoardComponent(UiQuestComponent questUiQuestComponent, Game gameObject) : base(gameObject)
        {
            QUiQuestComponent = questUiQuestComponent;

            // Find Quest Ui panel
            GameObject panel = GameObject.Find("QuestUICanvas");
            if (panel == null)
            {
                // Create Ui Panel
                panel = new GameObject("QuestUICanvas");
                panel.tag = Game.BOARD;
                panel.transform.SetParent(game.uICanvas.transform);
                panel.transform.SetAsFirstSibling();
                panel.AddComponent<RectTransform>();
                panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
                panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, Screen.width);
            }

            Texture2D newTex = null;
            if (game.cd.images.ContainsKey(QUiQuestComponent.imageName))
            {
                Vector2 texPos = new Vector2(game.cd.images[QUiQuestComponent.imageName].x, game.cd.images[QUiQuestComponent.imageName].y);
                Vector2 texSize = new Vector2(game.cd.images[QUiQuestComponent.imageName].width, game.cd.images[QUiQuestComponent.imageName].height);
                newTex = ContentData.FileToTexture(game.cd.images[QUiQuestComponent.imageName].image, texPos, texSize);
            }
            else if (QUiQuestComponent.imageName.Length > 0)
            {
                newTex = ContentData.FileToTexture(Quest.FindLocalisedMultimediaFile(QUiQuestComponent.imageName, Path.GetDirectoryName(game.quest.qd.questPath)));
            }

            // Create object
            unityObject = new GameObject("Object" + QUiQuestComponent.sectionName);
            unityObject.tag = Game.BOARD;

            unityObject.transform.SetParent(panel.transform);

            float aspect = 1;
            RectTransform rectTransform = unityObject.AddComponent<RectTransform>();
            RectTransform rectTransform_text = null;

            if (QUiQuestComponent.imageName.Length == 0)
            {
                uiTextBG = unityObject.AddComponent<UnityEngine.UI.Image>();
                uiTextBG.color = ColorUtil.ColorFromName(QUiQuestComponent.textBackgroundColor);

                unityObject_text = new GameObject("Object" + QUiQuestComponent.sectionName + "text");
                unityObject_text.tag = Game.BOARD;
                unityObject_text.transform.SetParent(unityObject.transform);
                rectTransform_text = unityObject_text.AddComponent<RectTransform>();

                uiText = unityObject_text.AddComponent<UnityEngine.UI.Text>();
                uiText.text = GetText();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.font = game.gameType.GetFont();
                uiText.material = uiText.font.material;
                uiText.fontSize = Mathf.RoundToInt(UIScaler.GetPixelsPerUnit() * QUiQuestComponent.textSize);
                SetColor(QUiQuestComponent.textColor);
                aspect = QUiQuestComponent.aspect;
            }
            else
            {
                // Create the image
                image = unityObject.AddComponent<UnityEngine.UI.Image>();
                Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
                image.color = new Color(1, 1, 1, 0);
                image.sprite = tileSprite;
                aspect = (float)newTex.width / (float)newTex.height;
            }

            float unitScale = Screen.width;
            float hSize = QUiQuestComponent.size * unitScale;
            float vSize = hSize / aspect;
            if (QUiQuestComponent.verticalUnits)
            {
                unitScale = Screen.height;
                vSize = QUiQuestComponent.size * unitScale;
                hSize = vSize * aspect;
            }

            float hOffset = QUiQuestComponent.location.x * unitScale;
            float vOffset = QUiQuestComponent.location.y * unitScale;

            if (QUiQuestComponent.hAlign < 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, hOffset, hSize);
                rectTransform.ForceUpdateRectTransforms();
                if (rectTransform_text != null)
                    rectTransform_text.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, hSize);
            }
            else if (QUiQuestComponent.hAlign > 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, hOffset, hSize);
                rectTransform.ForceUpdateRectTransforms();
                if (rectTransform_text != null)
                    rectTransform_text.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, hSize);
            }
            else
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, hOffset + ((Screen.width - hSize) / 2f), hSize);
                rectTransform.ForceUpdateRectTransforms();
                if (rectTransform_text != null)
                    rectTransform_text.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, hSize);
            }

            if (QUiQuestComponent.vAlign < 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, vOffset, vSize);
                rectTransform.ForceUpdateRectTransforms();
                if (rectTransform_text != null)
                    rectTransform_text.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, vSize);
            }
            else if (QUiQuestComponent.vAlign > 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, vOffset, vSize);
                rectTransform.ForceUpdateRectTransforms();
                if (rectTransform_text != null)
                    rectTransform_text.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, vSize);
            }
            else
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, vOffset + ((Screen.height - vSize) / 2f), vSize);
                rectTransform.ForceUpdateRectTransforms();
                if (rectTransform_text != null)
                    rectTransform_text.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, vSize);
            }

            if (QUiQuestComponent.border)
            {
                border = new UIElementBorder(unityObject.transform, rectTransform, Game.BOARD, uiText.color);
            }

            game.tokenBoard.Add(this);
        }

        // Tokens have an associated event to start on press
        public override EventQuestComponent GetEvent()
        {
            return QUiQuestComponent;
        }

        override public void SetColor(Color c)
        {
            if (image != null && image.gameObject != null) image.color = c;
            if (uiText != null && uiText.gameObject != null) uiText.color = c;
            if (border != null) border.SetColor(c);
            // Text BG has its own color, only alpha is changing
            if (uiTextBG != null && uiTextBG.gameObject != null && QUiQuestComponent.textBackgroundColor != "transparent")
            {
                Color tmp = uiTextBG.color;
                tmp.a = c.a;
                uiTextBG.color = tmp;
            }
        }

        override public Color GetColor()
        {
            if (image != null) return image.color;
            if (uiText != null) return uiText.color;
            return Color.clear;
        }

        // Set visible can control the transparency level of the component
        public override void SetVisible(float alpha)
        {
            targetAlpha = alpha;
            // Hide in editor
            if (targetAlpha < 0.5f)
            {
                targetAlpha = 0;
            }
        }

        // Get the text to display on the Ui
        virtual public string GetText()
        {
            string text = QUiQuestComponent.uiText.Translate(true);

            // Find and replace {q:element with the name of the
            // element
            text = ValkyrieEvent.ReplaceComponentText(text);

            // Fix new lines and replace symbol text with special characters
            return EventManager.OutputSymbolReplace(text).Replace("\\n", "\n");
        }

        // Clean up
        public override void Remove()
        {
            if (unityObject_text != null)
                UnityEngine.Object.Destroy(unityObject_text);
            UnityEngine.Object.Destroy(unityObject);
        }
    }

}
