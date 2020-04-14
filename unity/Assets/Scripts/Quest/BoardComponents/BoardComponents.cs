using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content.QuestComponent;
using UnityEngine;

namespace Assets.Scripts.Quest.BoardComponents
{
    // Super class for all Quest components
    abstract public class BoardComponent
    {
        // image for display
        public UnityEngine.UI.Image image;

        // Game object
        public Game game;

        public GameObject unityObject;

        // Target alpha
        public float targetAlpha = 1f;

        public BoardComponent(Game gameObject)
        {
            game = gameObject;
        }

        abstract public void Remove();

        abstract public EventQuestComponent GetEvent();

        // Set visible can control the transparency level of the component
        virtual public void SetVisible(float alpha)
        {
            targetAlpha = alpha;
        }

        // Set visible can control the transparency level of the component
        virtual public void UpdateAlpha(float time)
        {
            float alpha = GetColor().a;
            float distUpdate = time;
            float distRemain = targetAlpha - alpha;
            if (distRemain > distUpdate)
            {
                alpha += distUpdate;
            }
            else if (-distRemain > distUpdate)
            {
                alpha -= distUpdate;
            }
            else
            {
                alpha = targetAlpha;
            }
            SetColor(new Color(GetColor().r, GetColor().g, GetColor().b, alpha));
        }

        virtual public void SetColor(Color c)
        {
            if (image == null)
                return;
            if (image.gameObject == null)
                return;
            image.color = c;
        }

        virtual public Color GetColor()
        {
            if (image != null) return image.color;
            return Color.clear;
        }

        // Function to set color from string
        public void SetColor(string colorName)
        {
            // Translate from name to #RRGGBB, will return input if already #RRGGBB
            string colorRGB = ColorUtil.FromName(colorName);
            // Check format is valid
            if ((colorRGB.Length != 7) || (colorRGB[0] != '#'))
            {
                game.quest.log.Add(new global::Assets.Scripts.Quest.LogEntry("Warning: Color must be in #RRGGBB format or a known name: " + colorName, true));
            }

            // State with white (used for alpha)
            Color colour = Color.white;
            // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
            colour[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
            colour[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
            colour[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;
            SetColor(colour);
        }
    }
}
