using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Quest.Events;
using Assets.Scripts.UI;
using ValkyrieTools;

namespace Assets.Scripts.Quest
{
    // Class for creation of a dialog window with buttons and handling button press
    // This is used for display of event information
    public class DialogWindow
    {
        // The even that raises this dialog
        public ValkyrieEvent ValkyrieEventData;

        // An event can have a list of selected heroes
        public List<Hero> heroList;

        public int quota = 0;

        public string text = "";

        // Create from event
        public DialogWindow(ValkyrieEvent e)
        {
            ValkyrieEventData = e;
            heroList = new List<Hero>();
            Game game = Game.Get();
            text = ValkyrieEventData.GetText();

            // hero list can be populated from another event
            if (!ValkyrieEventData.QEventQuestComponent.heroListName.Equals(""))
            {
                // Try to find the event
                if (!game.quest.heroSelection.ContainsKey(ValkyrieEventData.QEventQuestComponent.heroListName))
                {
                    ValkyrieDebug.Log("Warning: Hero selection in event: " +
                                      ValkyrieEventData.QEventQuestComponent.sectionName + " from event " +
                                      ValkyrieEventData.QEventQuestComponent.heroListName + " with no data.");
                    game.quest.log.Add(new LogEntry(
                        "Warning: Hero selection in event: " + ValkyrieEventData.QEventQuestComponent.sectionName +
                        " from event " + ValkyrieEventData.QEventQuestComponent.heroListName + " with no data.", true));
                }
                else
                {
                    // Get selection data from other event
                    foreach (Hero h in game.quest.heroSelection[ValkyrieEventData.QEventQuestComponent.heroListName])
                    {
                        h.selected = true;
                    }
                }
            }

            // Update selection status
            game.heroCanvas.UpdateStatus();

            if (ValkyrieEventData.QEventQuestComponent.quota > 0 ||
                ValkyrieEventData.QEventQuestComponent.quotaVar.Length > 0)
            {
                if (ValkyrieEventData.QEventQuestComponent.quotaVar.Length > 0)
                {
                    quota = Mathf.RoundToInt(game.quest.vars.GetValue(ValkyrieEventData.QEventQuestComponent.quotaVar));
                }

                CreateQuotaWindow();
            }
            else
            {
                CreateWindow();
            }

            DrawItem();
        }

        public void CreateWindow()
        {
            // Draw text
            UIElement ui = new UIElement();
            float offset = ui.GetStringHeight(text, 28);
            if (offset < 4)
            {
                offset = 4;
            }

            ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, offset);
            ui.SetText(text);
            new UIElementBorder(ui);
            offset += 1f;

            // Determine button size
            float buttonWidth = 8;
            float hOffset = UIScaler.GetWidthUnits() - 19f;
            float hOffsetCancel = 11;
            float offsetCancel = offset;

            List<DialogWindow.EventButton> buttons = ValkyrieEventData.GetButtons();
            foreach (EventButton eb in buttons)
            {
                float length = ui.GetStringWidth(eb.GetLabel().Translate(), UIScaler.GetMediumFont());
                if (length > buttonWidth)
                {
                    buttonWidth = length;
                    hOffset = UIScaler.GetHCenter(-length / 2);
                    hOffsetCancel = UIScaler.GetHCenter(-4);
                    offsetCancel = offset + (2.5f * buttons.Count);
                }
            }

            int num = 1;
            foreach (EventButton eb in buttons)
            {
                int numTmp = num++;
                ui = new UIElement();
                ui.SetLocation(hOffset, offset, buttonWidth, 2);
                ui.SetText(eb.GetLabel(), eb.colour);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { onButton(numTmp); });
                new UIElementBorder(ui, eb.colour);
                offset += 2.5f;
            }

            // Do we have a cancel button?
            if (ValkyrieEventData.QEventQuestComponent.cancelable)
            {
                ui = new UIElement();
                ui.SetLocation(hOffsetCancel, offsetCancel, 8, 2);
                ui.SetText(CommonStringKeys.CANCEL);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(onCancel);
                new UIElementBorder(ui);
            }
        }

        public void CreateQuotaWindow()
        {
            // Draw text
            UIElement ui = new UIElement();
            float offset = ui.GetStringHeight(text, 28);
            if (offset < 4)
            {
                offset = 4;
            }

            ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, offset);
            ui.SetText(text);
            new UIElementBorder(ui);
            offset += 1;

            ui = new UIElement();
            ui.SetLocation(11, offset, 2, 2);
            new UIElementBorder(ui);
            if (quota == 0)
            {
                ui.SetText(CommonStringKeys.MINUS, Color.grey);
            }
            else
            {
                ui.SetText(CommonStringKeys.MINUS);
                ui.SetButton(quotaDec);
            }

            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(14, offset, 2, 2);
            ui.SetText(quota.ToString());
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(17, offset, 2, 2);
            new UIElementBorder(ui);
            if (quota >= 10)
            {
                ui.SetText(CommonStringKeys.PLUS, Color.grey);
            }
            else
            {
                ui.SetText(CommonStringKeys.PLUS);
                ui.SetButton(quotaInc);
            }

            ui.SetFontSize(UIScaler.GetMediumFont());

            // Only one button, action depends on quota
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetWidthUnits() - 19, offset, 8, 2);
            ui.SetText(ValkyrieEventData.GetButtons()[0].GetLabel());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(onQuota);
            new UIElementBorder(ui);

            // Do we have a cancel button?
            if (ValkyrieEventData.QEventQuestComponent.cancelable)
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-4f), offset + 2.5f, 8, 2);
                ui.SetText(CommonStringKeys.CANCEL);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(onCancel);
                new UIElementBorder(ui);
            }
        }

        public void DrawItem()
        {
            if (ValkyrieEventData.QEventQuestComponent.highlight) return;

            string item = "";
            int items = 0;
            foreach (string s in ValkyrieEventData.QEventQuestComponent.addComponents)
            {
                if (s.IndexOf("QItem") == 0)
                {
                    item = s;
                    items++;
                }
            }

            if (items != 1) return;

            Game game = Game.Get();

            if (!game.quest.itemSelect.ContainsKey(item)) return;

            Texture2D tex = ContentData.FileToTexture(game.cd.items[game.quest.itemSelect[item]].image);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1, 0,
                SpriteMeshType.FullRect);

            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-21), 0.5f, 6, 6);
            ui.SetImage(sprite);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-22.5f), 6.5f, 9, 1);
            ui.SetText(game.cd.items[game.quest.itemSelect[item]].name);
        }

        public void quotaDec()
        {
            quota--;
            Destroyer.Dialog();
            CreateQuotaWindow();
        }

        public void quotaInc()
        {
            quota++;
            Destroyer.Dialog();
            CreateQuotaWindow();
        }

        public void onQuota()
        {
            Game game = Game.Get();
            if (ValkyrieEventData.QEventQuestComponent.quotaVar.Length > 0)
            {
                game.quest.vars.SetValue(ValkyrieEventData.QEventQuestComponent.quotaVar, quota);
                onButton(1);
                return;
            }

            if (game.quest.eventQuota.ContainsKey(ValkyrieEventData.QEventQuestComponent.sectionName))
            {
                game.quest.eventQuota[ValkyrieEventData.QEventQuestComponent.sectionName] += quota;
            }
            else
            {
                game.quest.eventQuota.Add(ValkyrieEventData.QEventQuestComponent.sectionName, quota);
            }

            if (game.quest.eventQuota[ValkyrieEventData.QEventQuestComponent.sectionName] >=
                ValkyrieEventData.QEventQuestComponent.quota)
            {
                game.quest.eventQuota.Remove(ValkyrieEventData.QEventQuestComponent.sectionName);
                onButton(1);
            }
            else
            {
                onButton(2);
            }
        }

        // Cancel cleans up
        public void onCancel()
        {
            Destroyer.Dialog();
            Game.Get().quest.eManager.CurrentValkyrieEvent = null;
            // There may be a waiting event
            Game.Get().quest.eManager.TriggerEvent();
        }

        public void onButton(int num)
        {
            // Do we have correct hero selection?
            if (!checkHeroes()) return;

            Game game = Game.Get();
            // Destroy this dialog to close
            Destroyer.Dialog();

            // If the user started this event button is undoable
            if (ValkyrieEventData.QEventQuestComponent.cancelable)
            {
                game.quest.Save();
            }

            // Add this to the log
            game.quest.log.Add(new LogEntry(text.Replace("\n", "\\n")));

            // Add this to the eventList
            game.quest.eventList.Add(ValkyrieEventData.QEventQuestComponent.sectionName);

            // Event manager handles the aftermath
            game.quest.eManager.EndEvent(num - 1);
        }

        // Check that the correct number of heroes are selected
        public bool checkHeroes()
        {
            Game game = Game.Get();

            heroList = new List<Hero>();

            // List all selected heroes
            foreach (Hero h in game.quest.heroes)
            {
                if (h.selected)
                {
                    heroList.Add(h);
                }
            }

            // Check that count matches
            if (ValkyrieEventData.QEventQuestComponent.maxHeroes < heroList.Count &&
                ValkyrieEventData.QEventQuestComponent.maxHeroes != 0) return false;
            if (ValkyrieEventData.QEventQuestComponent.minHeroes > heroList.Count) return false;

            // Clear selection
            foreach (Hero h in game.quest.heroes)
            {
                h.selected = false;
            }

            // If this event has previous selected heroes clear the data
            if (game.quest.heroSelection.ContainsKey(ValkyrieEventData.QEventQuestComponent.sectionName))
            {
                game.quest.heroSelection.Remove(ValkyrieEventData.QEventQuestComponent.sectionName);
            }

            // Add this selection to the Quest
            game.quest.heroSelection.Add(ValkyrieEventData.QEventQuestComponent.sectionName, heroList);

            // Update hero image state
            game.heroCanvas.UpdateStatus();

            // Selection OK
            return true;
        }

        public class EventButton
        {
            StringKey label = StringKey.NULL;
            public Color32 colour = Color.white;

            public EventButton(StringKey newLabel, string newColour)
            {
                label = newLabel;
                string colorRGB = ColorUtil.FromName(newColour);

                // Check format is valid
                if ((colorRGB.Length != 7 && colorRGB.Length != 9) || (colorRGB[0] != '#'))
                {
                    Game.Get().quest.log
                        .Add(new LogEntry("Warning: Button color must be in #RRGGBB format or a known name", true));
                }

                // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
                colour.r = (byte) System.Convert.ToByte(colorRGB.Substring(1, 2), 16);
                colour.g = (byte) System.Convert.ToByte(colorRGB.Substring(3, 2), 16);
                colour.b = (byte) System.Convert.ToByte(colorRGB.Substring(5, 2), 16);

                if (colorRGB.Length == 9)
                    colour.a = (byte) System.Convert.ToByte(colorRGB.Substring(7, 2), 16);
                else
                    colour.a = 255; // opaque by default
            }

            public StringKey GetLabel()
            {
                return new StringKey(null,
                    EventManager.OutputSymbolReplace(ValkyrieEvent.ReplaceComponentText(label.Translate())), false);
            }
        }
    }
}