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
    // Class for Tile components (use TileSide content data)
    public class TileBoardComponent : BoardComponent
    {
        // This is the Quest information
        public Assets.Scripts.Content.QuestComponent.TileQuestComponent QTileQuestComponent;
        // This is the component information
        public TileSideData cTile;

        // Construct with data from the Quest, pass Game for speed
        public TileBoardComponent(Assets.Scripts.Content.QuestComponent.TileQuestComponent questTileQuestComponent, Game gameObject) : base(gameObject)
        {
            QTileQuestComponent = questTileQuestComponent;

            // Search for tile in content
            if (game.cd.tileSides.ContainsKey(QTileQuestComponent.tileSideName))
            {
                cTile = game.cd.tileSides[QTileQuestComponent.tileSideName];
            }
            else if (game.cd.tileSides.ContainsKey("TileSide" + QTileQuestComponent.tileSideName))
            {
                cTile = game.cd.tileSides["TileSide" + QTileQuestComponent.tileSideName];
            }
            else
            {
                // Fatal if not found
                ValkyrieDebug.Log("Error: Failed to located TileSide: '" + QTileQuestComponent.tileSideName + "' in Quest component: '" + QTileQuestComponent.sectionName + "'");
                Application.Quit();
                return;
            }

            // Attempt to load image
            var mTile = game.cd.tileSides[QTileQuestComponent.tileSideName];
            Texture2D newTex = ContentData.FileToTexture(mTile.image);
            if (newTex == null)
            {
                // Fatal if missing
                ValkyrieDebug.Log("Error: cannot open image file for TileSide: '" + mTile.image + "' named: '" + QTileQuestComponent.tileSideName + "'");
                Application.Quit();
                return;
            }

            // Create a unity object for the tile
            unityObject = new GameObject("Object" + QTileQuestComponent.sectionName);
            unityObject.tag = Game.BOARD;
            unityObject.transform.SetParent(game.boardCanvas.transform);

            // Add image to object
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            // Create sprite from texture
            Sprite tileSprite = null;
            if (game.gameType is MoMGameType)
            {
                // This is faster
                tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            }
            else
            {
                tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            }
            // Set image sprite
            image.sprite = tileSprite;
            // Move to get the top left square corner at 0,0
            float vPPS = mTile.pxPerSquare;
            float hPPS = vPPS;
            // manual aspect control
            // We need this for the 3x2 MoM tiles because they don't have square pixels!!
            if (mTile.aspect != 0)
            {
                hPPS = (vPPS * newTex.width / newTex.height) / mTile.aspect;
            }

            // Perform alignment move
            unityObject.transform.Translate(Vector3.right * ((newTex.width / 2) - cTile.left) / hPPS, Space.World);
            unityObject.transform.Translate(Vector3.down * ((newTex.height / 2) - cTile.top) / vPPS, Space.World);
            // Move to get the middle of the top left square at 0,0
            // We don't do this for MoM because it spaces differently
            if (game.gameType.TileOnGrid())
            {
                unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0), Space.World);
            }
            // Set the size to the image size
            image.rectTransform.sizeDelta = new Vector2((float)newTex.width / hPPS, (float)newTex.height / vPPS);

            // Rotate around 0,0 rotation amount
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, QTileQuestComponent.rotation);
            // Move tile into target location (Space.World is needed because tile has been rotated)
            unityObject.transform.Translate(new Vector3(QTileQuestComponent.location.x, QTileQuestComponent.location.y, 0), Space.World);
            image.color = new Color(1, 1, 1, 0);

            if (!Game.Get().quest.firstTileDisplayed)
            {
                Game.Get().quest.firstTileDisplayed = true;

                // We wait for the first tile displayed on MoM to display the 'NextStage' button bar
                // Don't do anything if Quest is being loaded and stageUI does not exist yet
                if (game.gameType.TypeName() == "MoM" && game.stageUI != null)
                {
                    game.stageUI.Update();
                }
            }
        }

        // Remove this tile
        public override void Remove()
        {
            UnityEngine.Object.Destroy(unityObject);
        }

        // Tiles are not interactive, no event
        public override EventQuestComponent GetEvent()
        {
            return null;
        }
    }
}
