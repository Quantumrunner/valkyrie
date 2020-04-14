using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content.QuestComponent;
using UnityEngine;
using ValkyrieTools;
using Object = System.Object;

namespace Assets.Scripts.Quest.BoardComponents
{

    // Doors are like tokens but placed differently and have different defaults
    // Note that MoM Explore tokens are tokens and do not use this
    // Doors are like tokens but placed differently and have different defaults
    // Note that MoM Explore tokens are tokens and do not use this
    public class DoorBoardComponent : BoardComponent
    {
        public Assets.Scripts.Content.QuestComponent.DoorQuestComponent QDoorQuestComponent;

        // Constuct with Quest data and reference to Game
        public DoorBoardComponent(Assets.Scripts.Content.QuestComponent.DoorQuestComponent questDoorQuestComponent, Game gameObject) : base(gameObject)
        {
            QDoorQuestComponent = questDoorQuestComponent;

            // Load door texture, should be game specific
            Texture2D newTex = Resources.Load("sprites/door") as Texture2D;
            // Check load worked
            if (newTex == null)
            {
                ValkyrieDebug.Log("Error: Cannot load door image");
                Application.Quit();
            }

            // Create object
            unityObject = new GameObject("Object" + QDoorQuestComponent.sectionName);
            unityObject.tag = Game.BOARD;

            unityObject.transform.SetParent(game.tokenCanvas.transform);

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            // Set door colour
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(0.4f, 1.6f);
            // Rotate as required
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, QDoorQuestComponent.rotation);
            // Move to square
            unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0), Space.World);
            unityObject.transform.Translate(new Vector3(QDoorQuestComponent.location.x, QDoorQuestComponent.location.y, 0), Space.World);

            // Set the texture colour from Quest data
            SetColor(QDoorQuestComponent.colourName);

            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

            game.tokenBoard.Add(this);
        }

        // Clean up
        public override void Remove()
        {
            UnityEngine.Object.Destroy(unityObject);
        }

        // Doors have events that start when pressed
        public override EventQuestComponent GetEvent()
        {
            return QDoorQuestComponent;
        }
    }
}
