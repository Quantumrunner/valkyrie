using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

namespace Assets.Scripts.QuestEditor
{
    public class ReorderComponents
    {
        string source = "";
        List<UIElement> names;

        public ReorderComponents()
        {
            Game game = Game.Get();

            HashSet<string> sources = new HashSet<string>();
            foreach (QuestComponent c in game.quest.qd.components.Values)
            {
                if (!(c is PerilData)) sources.Add(c.source);
            }

            UIWindowSelectionList select =
                new UIWindowSelectionList(ReorderSource, new StringKey("val", "SELECT", CommonStringKeys.FILE));
            foreach (string s in sources)
            {
                select.AddItem(s);
            }

            select.Draw();
        }

        public void ReorderSource(string s)
        {
            source = s;
            Draw();
        }

        public void Draw()
        {
            Game game = Game.Get();
            Destroyer.Dialog();

            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-12.5f), 0, 25, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-12), 0, 24, 1);
            ui.SetText(source);

            // Scroll BG
            UIElementScrollVertical scrollArea = new UIElementScrollVertical();
            scrollArea.SetLocation(UIScaler.GetHCenter(-11.5f), 2, 23, 25);
            new UIElementBorder(scrollArea);

            bool first = true;
            float offset = 0;
            names = new List<UIElement>();
            int index = 0;
            foreach (QuestComponent c in game.quest.qd.components.Values)
            {
                if (!c.source.Equals(source)) continue;

                int tmp = index++;
                if (!first)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(21f, offset, 1, 1);
                    ui.SetBGColor(Color.green);
                    ui.SetText("▽", Color.black);
                    ui.SetButton(delegate { IncComponent(tmp); });
                    offset += 1.05f;

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(0, offset, 1, 1);
                    ui.SetBGColor(Color.green);
                    ui.SetText("△", Color.black);
                    ui.SetButton(delegate { IncComponent(tmp); });
                }

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(1.05f, offset, 19.9f, 1);
                ui.SetBGColor(Color.white);
                ui.SetText(c.sectionName, Color.black);
                names.Add(ui);
                first = false;
            }

            offset += 1.05f;

            if (offset < 25) offset = 25;

            scrollArea.SetScrollSize(offset);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(CommonStringKeys.FINISHED);
            ui.SetButton(delegate { Destroyer.Dialog(); });
            new UIElementBorder(ui);
        }

        public void Update()
        {
            int i = 0;
            foreach (QuestComponent c in Game.Get().quest.qd.components.Values)
            {
                if (c.source.Equals(source))
                {
                    names[i++].SetText(c.sectionName, Color.black);
                }
            }
        }

        public void IncComponent(int index)
        {
            string name = names[index].GetText();
            Game game = Game.Get();
            Dictionary<string, QuestComponent> preDict = new Dictionary<string, QuestComponent>();
            List<QuestComponent> postList = new List<QuestComponent>();
            foreach (QuestComponent c in game.quest.qd.components.Values)
            {
                if (c.sectionName.Equals(name))
                {
                    preDict.Add(c.sectionName, c);
                }
                else
                {
                    if (c.source.Equals(game.quest.qd.components[name].source))
                    {
                        foreach (QuestComponent post in postList)
                        {
                            preDict.Add(post.sectionName, post);
                        }

                        postList = new List<QuestComponent>();
                    }

                    postList.Add(c);
                }
            }

            foreach (QuestComponent post in postList)
            {
                preDict.Add(post.sectionName, post);
            }

            game.quest.qd.components = preDict;
            Update();
        }
    }
}