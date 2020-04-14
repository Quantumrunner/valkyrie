using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Quest
{
    public class BlockSlider : MonoBehaviour
    {
        public bool sliding = false;
        public Vector2 mouseStart;
        public Vector2 transStart;
        public PuzzleSlide.Block block;
        public PuzzleSlideWindow win;
        RectTransform trans;

        // Use this for initialization (called at creation)
        void Start()
        {
            trans = gameObject.GetComponent<RectTransform>();
            // Get the image attached to this game object
        }

        // Update is called once per frame
        void Update()
        {
            if (!sliding && !Input.GetMouseButtonDown(0))
            {
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.mousePosition.x < trans.position.x) return;
                if (Input.mousePosition.y < trans.position.y - trans.rect.height) return;
                if (Input.mousePosition.x > trans.position.x + trans.rect.width) return;
                if (Input.mousePosition.y > trans.position.y) return;
                sliding = true;
                mouseStart = Input.mousePosition;
                transStart = trans.anchoredPosition;
            }

            if (!sliding)
            {
                return;
            }

            if (block.rotation)
            {
                float yTarget = -transStart.y + mouseStart.y - Input.mousePosition.y;
                float yTargetSq = yTarget / (3f * UIScaler.GetPixelsPerUnit());
                int yLimit = GetNegativeLimit();
                if (yTargetSq < yLimit)
                {
                    yTargetSq = yLimit;
                }
                yLimit = GetPositiveLimit();
                if (yTargetSq > yLimit)
                {
                    yTargetSq = yLimit;
                }
                yTarget = (yTargetSq * 3f * UIScaler.GetPixelsPerUnit());
                float nearestFit = (yTargetSq * 3f * UIScaler.GetPixelsPerUnit());
                if (Mathf.Abs(yTarget - nearestFit) < (UIScaler.GetPixelsPerUnit() * 1f))
                {
                    yTarget = nearestFit;
                }
                Vector3 pos = trans.anchoredPosition;
                pos.y = -yTarget;
                trans.anchoredPosition = pos;
            }
            else
            {
                float xTarget = transStart.x + Input.mousePosition.x - mouseStart.x;
                float xTargetSq = xTarget / (3f * UIScaler.GetPixelsPerUnit());
                int xLimit = GetNegativeLimit();
                if (xTargetSq < xLimit)
                {
                    xTargetSq = xLimit;
                }
                xLimit = GetPositiveLimit();
                if (xTargetSq > xLimit)
                {
                    xTargetSq = xLimit;
                }
                xTarget = xTargetSq * 3f * UIScaler.GetPixelsPerUnit();
                float nearestFit = Mathf.Round(xTargetSq) * 3f * UIScaler.GetPixelsPerUnit();
                if (Mathf.Abs(xTarget - nearestFit) < (UIScaler.GetPixelsPerUnit() * 1f))
                {
                    xTarget = nearestFit;
                }
                Vector3 pos = trans.anchoredPosition;
                pos.x = xTarget;
                trans.anchoredPosition = pos;
            }

            if (!Input.GetMouseButton(0))
            {
                sliding = false;
                int newXPos = Mathf.RoundToInt(trans.anchoredPosition.x / (3f * UIScaler.GetPixelsPerUnit()));
                int newYPos = Mathf.RoundToInt(-trans.anchoredPosition.y / (3f * UIScaler.GetPixelsPerUnit()));
                if (newXPos != block.xpos || newYPos != block.ypos)
                {
                    win.puzzle.moves++;
                    block.xpos = newXPos;
                    block.ypos = newYPos;
                }
                // Update
                win.CreateWindow();
            }
        }

        public int GetNegativeLimit()
        {
            int posx = block.xpos;
            int posy = block.ypos;

            do
            {
                if (block.rotation)
                {
                    posy--;
                }
                else
                {
                    posx--;
                }
            } while (PuzzleSlide.Empty(win.puzzle.puzzle, posx, posy));

            if (block.rotation)
            {
                return posy + 1;
            }
            if (block.target && posx == 4)
            {
                return 6;
            }
            return posx + 1;
        }

        public int GetPositiveLimit()
        {
            int posx = block.xpos;
            int posy = block.ypos;

            if (block.rotation)
            {
                posy += block.ylen + 1;
            }
            else
            {
                posx += block.xlen + 1;
            }

            while (PuzzleSlide.Empty(win.puzzle.puzzle, posx, posy))
            {
                if (block.rotation)
                {
                    posy++;
                }
                else
                {
                    posx++;
                }
            }

            if (block.rotation)
            {
                return posy - (1 + block.ylen);
            }
            return posx - (1 + block.xlen);
        }
    }
}
