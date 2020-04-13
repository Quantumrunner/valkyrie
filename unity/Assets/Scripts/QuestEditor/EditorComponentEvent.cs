using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

public class EditorComponentEvent : EditorComponent
{
    //  Not used yet
    //private readonly StringKey NEXT_EVENTS = new StringKey("val", "NEXT_EVENTS");
    //private readonly StringKey VARS = new StringKey("val", "VARS");
    private readonly StringKey VAR = new StringKey("val", "VAR");
    private readonly StringKey VAR_NAME = new StringKey("val", "VAR_NAME");
    private readonly StringKey MIN_CAM = new StringKey("val", "MIN_CAM");
    private readonly StringKey MAX_CAM = new StringKey("val", "MAX_CAM");
    private readonly StringKey UNUSED = new StringKey("val", "UNUSED");
    private readonly StringKey HIGHLIGHT = new StringKey("val", "HIGHLIGHT");
    private readonly StringKey CAMERA = new StringKey("val", "CAMERA");
    private readonly StringKey REMOVE_COMPONENTS = new StringKey("val", "REMOVE_COMPONENTS");
    private readonly StringKey ADD_COMPONENTS = new StringKey("val", "ADD_COMPONENTS");
    private readonly StringKey MAX = new StringKey("val", "MAX");
    private readonly StringKey MIN = new StringKey("val", "MIN");
    private readonly StringKey DIALOG = new StringKey("val", "DIALOG");
    private readonly StringKey SELECTION = new StringKey("val", "SELECTION");
    private readonly StringKey AUDIO = new StringKey("val", "AUDIO");
    private readonly StringKey MUSIC = new StringKey("val", "MUSIC");
    private readonly StringKey CONTINUE = new StringKey("val", "CONTINUE");
    private readonly StringKey QUOTA = new StringKey("val","QUOTA");
    private readonly StringKey BUTTONS = new StringKey("val","BUTTONS");
    private readonly StringKey BUTTON = new StringKey("val", "BUTTON");
    
    EventQuestComponent EVENT_QUEST_COMPONENT_COMPONENT;

    UIElementEditablePaneled eventTextUIE;
    UIElementEditable quotaUIE;
    List<UIElementEditable> buttonUIE;

    public EditorComponentEvent(string nameIn) : base()
    {
        Game game = Game.Get();
        EVENT_QUEST_COMPONENT_COMPONENT = game.quest.qd.components[nameIn] as EventQuestComponent;
        component = EVENT_QUEST_COMPONENT_COMPONENT;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        EVENT_QUEST_COMPONENT_COMPONENT = component as EventQuestComponent;
    }

    override public float AddSubComponents(float offset)
    {
        offset = AddPosition(offset);

        offset = AddSubEventComponents(offset);

        offset = AddEventDialog(offset);

        offset = AddEventTrigger(offset);

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", AUDIO));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset++, 10, 1);
        ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.audio);
        ui.SetButton(delegate { SetAudio(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(1.5f, offset, 10, 1);
        ui.SetText(new StringKey("val", "X_COLON", MUSIC));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(11.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddMusic(0); });
        new UIElementBorder(ui, Color.green);

        int index;
        for (index = 0; index < EVENT_QUEST_COMPONENT_COMPONENT.music.Count; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveMusic(i); });
            new UIElementBorder(ui, Color.red);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(1.5f, offset, 10, 1);
            ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.music[index]);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(11.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddMusic(i + 1); });
            new UIElementBorder(ui, Color.green);
        }
        offset++;

        if (game.gameType is D2EGameType || game.gameType is IAGameType)
        {
            offset = AddHeroSelection(offset);
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(ADD_COMPONENTS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddVisibility(true); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < EVENT_QUEST_COMPONENT_COMPONENT.addComponents.Length; index++)
        {
            int i = index;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.addComponents[index]);
            ui.SetButton(delegate { AddVisibility(true, i); });
            if (game.quest.qd.components.ContainsKey(EVENT_QUEST_COMPONENT_COMPONENT.addComponents[i]))
            {
                ui.SetLocation(0.5f, offset, 17, 1);
                UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                link.SetLocation(17.5f, offset, 1, 1);
                link.SetText("<b>⇨</b>", Color.cyan);
                link.SetTextAlignment(TextAnchor.LowerCenter);
                link.SetButton(delegate { QuestEditorData.SelectComponent(EVENT_QUEST_COMPONENT_COMPONENT.addComponents[i]); });
                new UIElementBorder(link, Color.cyan);
            }
            else
            {
                ui.SetLocation(0.5f, offset, 18, 1);
            }
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveVisibility(i, true); });
            new UIElementBorder(ui, Color.red);
        }
        offset++;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(REMOVE_COMPONENTS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddVisibility(false); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < EVENT_QUEST_COMPONENT_COMPONENT.removeComponents.Length; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.removeComponents[index]);
            ui.SetButton(delegate { AddVisibility(false, i); });
            if (game.quest.qd.components.ContainsKey(EVENT_QUEST_COMPONENT_COMPONENT.removeComponents[i]))
            {
                ui.SetLocation(0.5f, offset, 17, 1);
                UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                link.SetLocation(17.5f, offset, 1, 1);
                link.SetText("<b>⇨</b>", Color.cyan);
                link.SetTextAlignment(TextAnchor.LowerCenter);
                link.SetButton(delegate { QuestEditorData.SelectComponent(EVENT_QUEST_COMPONENT_COMPONENT.removeComponents[i]); });
                new UIElementBorder(link, Color.cyan);
            }
            else
            {
                ui.SetLocation(0.5f, offset, 18, 1);
            }
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveVisibility(i, false); });
            new UIElementBorder(ui, Color.red);
        }
        offset++;

        offset = AddNextEventComponents(offset);

        offset = AddEventVarConditionComponents(offset);

        offset = AddEventVarOperationComponents(offset);

        Highlight();
        return offset;
    }

    virtual public float AddPosition(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_FREE);
        ui.SetButton(delegate { GetPosition(false); });
        new UIElementBorder(ui);

        AddLocationType(offset);

        return offset + 2;
    }

    virtual public void AddLocationType(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(14, offset, 4, 1);
        ui.SetButton(delegate { PositionTypeCycle(); });
        new UIElementBorder(ui);
        if (EVENT_QUEST_COMPONENT_COMPONENT.minCam)
        {
            ui.SetText(MIN_CAM);
        }
        else if (EVENT_QUEST_COMPONENT_COMPONENT.maxCam)
        {
            ui.SetText(MAX_CAM);
        }
        else if (!EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified)
        {
            ui.SetText(UNUSED);
        }
        else if (EVENT_QUEST_COMPONENT_COMPONENT.highlight)
        {
            ui.SetText(HIGHLIGHT);
        }
        else
        {
            ui.SetText(CAMERA);
        }
    }

    virtual public float AddSubEventComponents(float offset)
    {
        return offset;
    }

    virtual public float AddEventDialog(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 20, 1);
        ui.SetText(new StringKey("val", "X_COLON", DIALOG));

        eventTextUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        eventTextUIE.SetLocation(0.5f, offset, 19, 18);
        eventTextUIE.SetText(EVENT_QUEST_COMPONENT_COMPONENT.text.Translate(true));
        offset += eventTextUIE.HeightToTextPadding(1);
        eventTextUIE.SetButton(delegate { UpdateText(); });
        new UIElementBorder(eventTextUIE);
        return offset + 1;
    }

    virtual public float AddEventTrigger(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TRIGGER));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 10, 1);
        ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.trigger);
        ui.SetButton(delegate { SetTrigger(); });
        new UIElementBorder(ui);
        return offset + 2;
    }

    virtual public float AddHeroSelection(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", SELECTION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 12.5f, 1);
        ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.heroListName);
        ui.SetButton(delegate { SetHighlight(); });
        new UIElementBorder(ui);

        if (game.quest.qd.components.ContainsKey(EVENT_QUEST_COMPONENT_COMPONENT.heroListName))
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset, 1, 1);
            ui.SetText("<b>⇨</b>", Color.cyan);
            ui.SetTextAlignment(TextAnchor.LowerCenter);
            ui.SetButton(delegate { QuestEditorData.SelectComponent(EVENT_QUEST_COMPONENT_COMPONENT.heroListName); });
            new UIElementBorder(ui, Color.cyan);
        }
        offset++;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", MIN));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 2, 1);
        ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.minHeroes.ToString());
        ui.SetButton(delegate { SetHeroCount(false); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(11, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", MAX));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(14, offset, 2, 1);
        ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.maxHeroes.ToString());
        ui.SetButton(delegate { SetHeroCount(true); });
        new UIElementBorder(ui);
        return offset + 2;
    }

    virtual public float AddNextEventComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", QUOTA));

        if (EVENT_QUEST_COMPONENT_COMPONENT.quotaVar.Length == 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(new StringKey("val", "NUMBER"));
            ui.SetButton(delegate { SetQuotaVar(); });
            new UIElementBorder(ui);

            // Quota dont need translation
            quotaUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            quotaUIE.SetLocation(9, offset, 2, 1);
            quotaUIE.SetText(EVENT_QUEST_COMPONENT_COMPONENT.quota.ToString());
            quotaUIE.SetButton(delegate { SetQuota(); });
            quotaUIE.SetSingleLine();
            new UIElementBorder(quotaUIE);
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(new StringKey("val", "VAR"));
            ui.SetButton(delegate { SetQuotaInt(); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(9, offset, 9, 1);
            ui.SetText(EVENT_QUEST_COMPONENT_COMPONENT.quotaVar);
            ui.SetButton(delegate { SetQuotaVar(); });
            new UIElementBorder(ui);
        }
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "NEXT_EVENTS")));

        string randomButton = "Ordered";
        if (EVENT_QUEST_COMPONENT_COMPONENT.randomEvents) randomButton = "Random";
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        ui.SetText(new StringKey("val", randomButton));
        ui.SetButton(delegate { ToggleRandom(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(1.5f, offset, 17, 1);
        ui.SetText(new StringKey("val", "X_COLON", BUTTONS));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddButton(); });
        new UIElementBorder(ui, Color.green);

        int button = 1;
        int index = 0;
        float lastButtonOffset = 0;
        buttonUIE = new List<UIElementEditable>();
        foreach (List<string> l in EVENT_QUEST_COMPONENT_COMPONENT.nextEvent)
        {
            lastButtonOffset = offset;
            int buttonTmp = button++;

            StringKey buttonLabel = EVENT_QUEST_COMPONENT_COMPONENT.buttons[buttonTmp - 1];
            string colorRGB = ColorUtil.FromName(EVENT_QUEST_COMPONENT_COMPONENT.buttonColors[buttonTmp - 1]);
            Color32 c = Color.white;
            c.r = (byte)System.Convert.ToByte(colorRGB.Substring(1, 2), 16);
            c.g = (byte)System.Convert.ToByte(colorRGB.Substring(3, 2), 16);
            c.b = (byte)System.Convert.ToByte(colorRGB.Substring(5, 2), 16);
            if (colorRGB.Length == 9)
                c.a = (byte)System.Convert.ToByte(colorRGB.Substring(7, 2), 16);
            else
                c.a = 255; // opaque by default

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 3, 1);
            ui.SetText(new StringKey("val", "COLOR"), c);
            ui.SetButton(delegate { SetButtonColor(buttonTmp); });
            new UIElementBorder(ui, c);

            UIElementEditable buttonEdit = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            buttonEdit.SetLocation(3.5f, offset++, 15, 1);
            buttonEdit.SetText(buttonLabel);
            buttonEdit.SetButton(delegate { UpdateButtonLabel(buttonTmp); });
            buttonEdit.SetSingleLine();
            new UIElementBorder(buttonEdit);
            buttonUIE.Add(buttonEdit);

            index = 0;
            foreach (string s in l)
            {
                int i = index++;
                string tmpName = s;
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.PLUS, Color.green);
                ui.SetButton(delegate { AddEvent(i, buttonTmp); });
                new UIElementBorder(ui, Color.green);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(1.5f, offset, 16, 1);
                ui.SetText(s);
                ui.SetButton(delegate { SetEvent(i, buttonTmp); });
                new UIElementBorder(ui);

                if (game.quest.qd.components.ContainsKey(tmpName))
                {
                    UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    link.SetLocation(17.5f, offset, 1, 1);
                    link.SetText("<b>⇨</b>", Color.cyan);
                    link.SetTextAlignment(TextAnchor.LowerCenter);
                    link.SetButton(delegate { QuestEditorData.SelectComponent(tmpName); });
                    new UIElementBorder(link, Color.cyan);
                }

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveEvent(i, buttonTmp); });
                new UIElementBorder(ui, Color.red);
            }

            int tmp = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddEvent(tmp, buttonTmp); });
            new UIElementBorder(ui, Color.green);
        }

        if (lastButtonOffset != 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, lastButtonOffset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveButton(); });
            new UIElementBorder(ui, Color.red);
        }

        return offset + 1;
    }



    virtual public void Highlight()
    {
        if (EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified || EVENT_QUEST_COMPONENT_COMPONENT.maxCam || EVENT_QUEST_COMPONENT_COMPONENT.minCam)
        {
            CameraController.SetCamera(EVENT_QUEST_COMPONENT_COMPONENT.location);
            game.tokenBoard.AddHighlight(EVENT_QUEST_COMPONENT_COMPONENT.location, "EventLoc", Game.EDITOR);
        }
    }

    virtual public void PositionTypeCycle()
    {
        if (EVENT_QUEST_COMPONENT_COMPONENT.minCam)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified = false;
            EVENT_QUEST_COMPONENT_COMPONENT.highlight = false;
            EVENT_QUEST_COMPONENT_COMPONENT.maxCam = true;
            EVENT_QUEST_COMPONENT_COMPONENT.minCam = false;
        }
        else if (EVENT_QUEST_COMPONENT_COMPONENT.maxCam)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified = false;
            EVENT_QUEST_COMPONENT_COMPONENT.highlight = false;
            EVENT_QUEST_COMPONENT_COMPONENT.maxCam = false;
            EVENT_QUEST_COMPONENT_COMPONENT.minCam = false;
        }
        else if (EVENT_QUEST_COMPONENT_COMPONENT.highlight)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified = false;
            EVENT_QUEST_COMPONENT_COMPONENT.highlight = false;
            EVENT_QUEST_COMPONENT_COMPONENT.maxCam = false;
            EVENT_QUEST_COMPONENT_COMPONENT.minCam = true;
        }
        else if (EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified = true;
            EVENT_QUEST_COMPONENT_COMPONENT.highlight = true;
            EVENT_QUEST_COMPONENT_COMPONENT.maxCam = false;
            EVENT_QUEST_COMPONENT_COMPONENT.minCam = false;
        }
        else
        {
            EVENT_QUEST_COMPONENT_COMPONENT.locationSpecified = true;
            EVENT_QUEST_COMPONENT_COMPONENT.highlight = false;
            EVENT_QUEST_COMPONENT_COMPONENT.maxCam = false;
            EVENT_QUEST_COMPONENT_COMPONENT.minCam = false;
        }
        Update();
    }

    public void UpdateText()
    {
        if (eventTextUIE.Changed())
        {
            if (eventTextUIE.Empty())
            {
                LocalizationRead.dicts["qst"].Remove(EVENT_QUEST_COMPONENT_COMPONENT.text_key);
                EVENT_QUEST_COMPONENT_COMPONENT.display = false;
            }
            else
            {
                LocalizationRead.updateScenarioText(EVENT_QUEST_COMPONENT_COMPONENT.text_key, eventTextUIE.GetText());
                if (EVENT_QUEST_COMPONENT_COMPONENT.buttons.Count == 0)
                {
                    EVENT_QUEST_COMPONENT_COMPONENT.buttons.Add(EVENT_QUEST_COMPONENT_COMPONENT.genQuery("button1"));
                    EVENT_QUEST_COMPONENT_COMPONENT.nextEvent.Add(new List<string>());
                    EVENT_QUEST_COMPONENT_COMPONENT.buttonColors.Add("white");
                    LocalizationRead.updateScenarioText(EVENT_QUEST_COMPONENT_COMPONENT.genKey("button1"),
                        CONTINUE.Translate());
                }
                if (!EVENT_QUEST_COMPONENT_COMPONENT.display)
                {
                    EVENT_QUEST_COMPONENT_COMPONENT.display = true;
                    Update();
                    return;
                }
            }
            if (!eventTextUIE.HeightAtTextPadding(1))
            {
                Update();
            }
        }
    }

    public void SetTrigger()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectEventTrigger, new StringKey("val", "SELECT", CommonStringKeys.TRIGGER));


        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { new StringKey("val", "GENERAL").Translate() });
        select.AddItem("{NONE}", "", traits);

        bool startPresent = false;
        bool noMorale = false;
        bool eliminated = false;
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            EventQuestComponent e = kv.Value as EventQuestComponent;
            if (e != null)
            {
                if (e.trigger.Equals("EventStart"))
                {
                    startPresent = true;
                }
                if (e.trigger.Equals("NoMorale"))
                {
                    noMorale = true;
                }
                if (e.trigger.Equals("Eliminated"))
                {
                    eliminated = true;
                }
            }
        }

        if (startPresent)
        {
            select.AddItem("EventStart", traits, Color.gray);
        }
        else
        {
            select.AddItem("EventStart", traits);
        }

        //Morale exists only in descent
        if (game.gameType is D2EGameType)
        {
            if (noMorale)
            {
                select.AddItem("NoMorale", traits, Color.gray);
            }
            else
            {
                select.AddItem("NoMorale", traits);
            }
        }

        //Eliminated only exists in MoM
        if (game.gameType is MoMGameType)
        {
            if (eliminated)
            {
                select.AddItem("Eliminated", traits, Color.gray);
            }
            else
            {
                select.AddItem("Eliminated", traits);
            }
        }

        //Mythos phase only exists in MoM
        if (game.gameType is MoMGameType)
        {
            select.AddItem("Mythos", traits);
        }

        select.AddItem("EndRound", traits);
        select.AddItem("StartRound", traits);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { CommonStringKeys.MONSTER.Translate() });

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            select.AddItem("Defeated" + kv.Key, traits);
            //Add defeated unique triggers only for descent because Unique Monster do not exist in MoM
            if (game.gameType is D2EGameType)
            {
                select.AddItem("DefeatedUnique" + kv.Key, traits);
            }
        }

        HashSet<string> vars = new HashSet<string>();
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is EventQuestComponent)
            {
                EventQuestComponent e = kv.Value as EventQuestComponent;
                foreach (string s in ExtractVarsFromEvent(e))
                {
                    if (s[0] == '@')
                    {
                        vars.Add(s);
                    }
                }
            }
        }

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { CommonStringKeys.CUSTOMMONSTER.Translate() });
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is CustomMonsterQuestComponent)
            {
                select.AddItem("Defeated" + kv.Key, traits);
                //Add defeated unique triggers only for descent because Unique Monster do not exist in MoM
                if (game.gameType is D2EGameType)
                {
                    select.AddItem("DefeatedUnique" + kv.Key, traits);
                }
            }
        }

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { CommonStringKeys.SPAWN.Translate() });

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is SpawnQuestComponent)
            {
                select.AddItem("Defeated" + kv.Key, traits);
                //Add defeated unique triggers only for descent because Unique Monster do not exist in MoM
                if (game.gameType is D2EGameType)
                {
                    select.AddItem("DefeatedUnique" + kv.Key, traits);
                }
                
            }
        }

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { new StringKey("val", "VARS").Translate() });

        foreach (string s in vars)
        {
            select.AddItem("Var" + s.Substring(1), traits);
        }

        select.Draw();
    }

    public void SelectEventTrigger(string trigger)
    {
        EVENT_QUEST_COMPONENT_COMPONENT.trigger = trigger;
        Update();
    }

    public void SetAudio()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListAudio(SelectEventAudio, new StringKey("val", "SELECT", new StringKey("val", "AUDIO")));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;

        select.AddItem("{NONE}", "");

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { CommonStringKeys.FILE.Translate() });

        foreach (string s in Directory.GetFiles(relativePath, "*.ogg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }

        foreach (KeyValuePair<string, AudioData> kv in game.cd.audio)
        {
            traits = new Dictionary<string, IEnumerable<string>>();
            traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "FFG" });
            traits.Add(CommonStringKeys.TRAITS.Translate(), kv.Value.traits);

            select.AddItem(kv.Key, traits);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectEventAudio(string audio)
    {
        Game game = Game.Get();
        EVENT_QUEST_COMPONENT_COMPONENT.audio = audio;
        if (game.cd.audio.ContainsKey(EVENT_QUEST_COMPONENT_COMPONENT.audio))
        {
            game.audioControl.Play(game.cd.audio[EVENT_QUEST_COMPONENT_COMPONENT.audio].file);
        }
        else
        {
            string path = Path.GetDirectoryName(Game.Get().quest.qd.questPath) + Path.DirectorySeparatorChar + EVENT_QUEST_COMPONENT_COMPONENT.audio;
            game.audioControl.Play(path);
        }
        Update();
    }

    public void AddMusic(int index)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListAudio(delegate(string s) { SelectMusic(index, s); }, new StringKey("val", "SELECT", new StringKey("val", "AUDIO")));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;

        select.AddItem("{NONE}", "");

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { CommonStringKeys.FILE.Translate() });

        foreach (string s in Directory.GetFiles(relativePath, "*.ogg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }

        foreach (KeyValuePair<string, AudioData> kv in game.cd.audio)
        {
            traits = new Dictionary<string, IEnumerable<string>>();
            traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "FFG" });
            traits.Add(CommonStringKeys.TRAITS.Translate(), kv.Value.traits);

            select.AddItem(kv.Key, traits);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectMusic(int index, string music)
    {
        EVENT_QUEST_COMPONENT_COMPONENT.music.Insert(index, music);
        Update();
    }

    public void RemoveMusic(int index)
    {
        EVENT_QUEST_COMPONENT_COMPONENT.music.RemoveAt(index);
        Update();
    }

    public void SetHighlight()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectEventHighlight, new StringKey("val", "SELECT", CommonStringKeys.EVENT));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;

        select.AddItem("{NONE}", "");

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is EventQuestComponent)
            {
                select.AddItem(kv.Value);
            }
        }
        select.Draw();
    }

    public void SelectEventHighlight(string eventName)
    {
        EVENT_QUEST_COMPONENT_COMPONENT.heroListName = eventName;
        Update();
    }

    public void SetHeroCount(bool max)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionList select = new UIWindowSelectionList(delegate(string s) { SelectEventHeroCount(max, s); }, new StringKey("val", "SELECT", CommonStringKeys.NUMBER));
        for (int i = 0; i <= game.gameType.MaxHeroes(); i++)
        {
            select.AddItem(i.ToString());
        }
        select.Draw();
    }

    public void SelectEventHeroCount(bool max, string number)
    {
        if (max)
        {
            int.TryParse(number, out EVENT_QUEST_COMPONENT_COMPONENT.maxHeroes);
        }
        else
        {
            int.TryParse(number, out EVENT_QUEST_COMPONENT_COMPONENT.minHeroes);
        }
        Update();
    }

    public void AddVisibility(bool add, int index = -1)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectAddVisibility(add, index, s); }, new StringKey("val", "SELECT", new StringKey("val", "COMPONENT")));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Special" });

        select.AddItem("#boardcomponents", traits);
        select.AddItem("#monsters", traits);
        select.AddItem("#shop", traits);

        if (game.gameType is D2EGameType || game.gameType is IAGameType)
        {
            select.AddNewComponentItem("DoorQuestComponent");
        }
        select.AddNewComponentItem("TileQuestComponent");
        select.AddNewComponentItem("TokenQuestComponent");
        select.AddNewComponentItem("UiQuestComponent");
        select.AddNewComponentItem("QItemQuestComponent");

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is DoorQuestComponent || kv.Value is TileQuestComponent || kv.Value is TokenQuestComponent || kv.Value is UiQuestComponent)
            {
                select.AddItem(kv.Value);
            }
            if (kv.Value is SpawnQuestComponent)
            {
                select.AddItem(kv.Value);
            }
            if (kv.Value is QItemQuestComponent)
            {
                select.AddItem(kv.Value);
            }
        }
        select.Draw();
    }

    public void SelectAddVisibility(bool add, int index, string component)
    {
        string target = component;
        int i;
        if (component.Equals("{NEW:DoorQuestComponent}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("DoorQuestComponent" + i))
            {
                i++;
            }
            target = "DoorQuestComponent" + i;
            DoorQuestComponent doorQuestComponent = new DoorQuestComponent(target);
            Game.Get().quest.qd.components.Add(target, doorQuestComponent);

            CameraController cc = GameObject.FindObjectOfType<CameraController>();
            doorQuestComponent.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
            doorQuestComponent.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

            game.quest.Add(target);
        }
        if (component.Equals("{NEW:TileQuestComponent}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("TileQuestComponent" + i))
            {
                i++;
            }
            target = "TileQuestComponent" + i;
            TileQuestComponent tileQuestComponent = new TileQuestComponent(target);
            Game.Get().quest.qd.components.Add(target, tileQuestComponent);

            CameraController cc = GameObject.FindObjectOfType<CameraController>();
            tileQuestComponent.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
            tileQuestComponent.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

            game.quest.Add(target);
        }
        if (component.Equals("{NEW:TokenQuestComponent}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("TokenQuestComponent" + i))
            {
                i++;
            }
            target = "TokenQuestComponent" + i;
            TokenQuestComponent tokenQuestComponent = new TokenQuestComponent(target);
            Game.Get().quest.qd.components.Add(target, tokenQuestComponent);

            CameraController cc = GameObject.FindObjectOfType<CameraController>();
            tokenQuestComponent.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
            tokenQuestComponent.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

            game.quest.Add(target);
        }
        if (component.Equals("{NEW:UiQuestComponent}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("UiQuestComponent" + i))
            {
                i++;
            }
            target = "UiQuestComponent" + i;
            Game.Get().quest.qd.components.Add(target, new UiQuestComponent(target));
        }
        if (component.Equals("{NEW:QItemQuestComponent}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("QItemQuestComponent" + i))
            {
                i++;
            }
            target = "QItemQuestComponent" + i;
            Game.Get().quest.qd.components.Add(target, new QItemQuestComponent(target));
        }

        if (index != -1)
        {
            if (add)
            {
                EVENT_QUEST_COMPONENT_COMPONENT.addComponents[index] = target;
            }
            else
            {
                EVENT_QUEST_COMPONENT_COMPONENT.removeComponents[index] = target;
            }
            Update();
            return;
        }
        string[] oldC = null;

        if(add)
        {
            oldC = EVENT_QUEST_COMPONENT_COMPONENT.addComponents;
        }
        else
        {
            oldC = EVENT_QUEST_COMPONENT_COMPONENT.removeComponents;
        }
        string[] newC = new string[oldC.Length + 1];
        for (i = 0; i < oldC.Length; i++)
        {
            newC[i] = oldC[i];
        }

        newC[i] = target;

        if (add)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.addComponents = newC;
        }
        else
        {
            EVENT_QUEST_COMPONENT_COMPONENT.removeComponents = newC;
        }
        Update();
    }

    public void RemoveVisibility(int index, bool add)
    {
        string[] oldC = null;

        if (add)
        {
            oldC = EVENT_QUEST_COMPONENT_COMPONENT.addComponents;
        }
        else
        {
            oldC = EVENT_QUEST_COMPONENT_COMPONENT.removeComponents;
        }

        string[] newC = new string[oldC.Length - 1];

        int j = 0;
        for (int i = 0; i < oldC.Length; i++)
        {
            if (i != index)
            {
                newC[j++] = oldC[i];
            }
        }

        if (add)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.addComponents = newC;
        }
        else
        {
            EVENT_QUEST_COMPONENT_COMPONENT.removeComponents = newC;
        }
        Update();
    }

    public void ToggleRandom()
    {
        EVENT_QUEST_COMPONENT_COMPONENT.randomEvents = !EVENT_QUEST_COMPONENT_COMPONENT.randomEvents;
        Update();
    }

    public void SetQuota()
    {
        int.TryParse(quotaUIE.GetText(), out EVENT_QUEST_COMPONENT_COMPONENT.quota);
        Update();
    }

    public void SetQuotaInt()
    {
        EVENT_QUEST_COMPONENT_COMPONENT.quotaVar = "";
        Update();
    }

    public void SetQuotaVar()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectQuotaVar, new StringKey("val", "SELECT", VAR));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits);

        AddQuestVars(select);

        select.Draw();
    }

    public void SelectQuotaVar(string var)
    {
        if (var.Equals("{NEW}"))
        {
            varText = new QuestEditorTextEdit(VAR_NAME, "", delegate { NewQuotaVar(); });
            varText.EditText();
        }
        else
        {
            EVENT_QUEST_COMPONENT_COMPONENT.quotaVar = var;
            EVENT_QUEST_COMPONENT_COMPONENT.quota = 0;
            Update();
        }
    }

    public void NewQuotaVar()
    {
        string var = System.Text.RegularExpressions.Regex.Replace(varText.value, "[^A-Za-z0-9_]", "");
        if (var.Length > 0)
        {
            if (varText.value[0] == '%')
            {
                var = '%' + var;
            }
            if (varText.value[0] == '@')
            {
                var = '@' + var;
            }
            if (char.IsNumber(var[0]) || var[0] == '-' || var[0] == '.')
            {
                var = "var" + var;
            }
            EVENT_QUEST_COMPONENT_COMPONENT.quotaVar = var;
            EVENT_QUEST_COMPONENT_COMPONENT.quota = 0;
        }
        Update();
    }

    public void AddButton()
    {
        int count = EVENT_QUEST_COMPONENT_COMPONENT.nextEvent.Count + 1;
        EVENT_QUEST_COMPONENT_COMPONENT.nextEvent.Add(new List<string>());
        EVENT_QUEST_COMPONENT_COMPONENT.buttons.Add(EVENT_QUEST_COMPONENT_COMPONENT.genQuery("button" + count));
        EVENT_QUEST_COMPONENT_COMPONENT.buttonColors.Add("white");
        LocalizationRead.updateScenarioText(EVENT_QUEST_COMPONENT_COMPONENT.genKey("button" + count), BUTTON.Translate() + count);
        Update();
    }

    public void RemoveButton()
    {
        int count = EVENT_QUEST_COMPONENT_COMPONENT.nextEvent.Count;
        EVENT_QUEST_COMPONENT_COMPONENT.nextEvent.RemoveAt(count - 1);
        EVENT_QUEST_COMPONENT_COMPONENT.buttons.RemoveAt(count - 1);
        EVENT_QUEST_COMPONENT_COMPONENT.buttonColors.RemoveAt(count - 1);
        LocalizationRead.dicts["qst"].Remove(EVENT_QUEST_COMPONENT_COMPONENT.genKey("button" + count));
        Update();
    }

    public void SetButtonColor(int number)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate(string s) { SelectButtonColour(number, s); }, CommonStringKeys.SELECT_ITEM);

        foreach (string s in ColorUtil.LookUp().Keys)
        {
            select.AddItem(s);
        }

        select.Draw();
    }

    public void SelectButtonColour(int number, string color)
    {
        EVENT_QUEST_COMPONENT_COMPONENT.buttonColors[number - 1] = color;
        Update();
    }

    public void UpdateButtonLabel(int number)
    {
        if (!buttonUIE[number - 1].Empty() && buttonUIE[number - 1].Changed())
        {
            LocalizationRead.updateScenarioText(EVENT_QUEST_COMPONENT_COMPONENT.genKey("button" + number), buttonUIE[number - 1].GetText());
        }
    }

    public void SetEvent(int index, int button)
    {
        AddEvent(index, button, true);
    }

    public void AddEvent(int index, int button, bool replace = false)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate(string s) { SelectAddEvent(index, button, replace, s); }, new StringKey("val", "SELECT", CommonStringKeys.EVENT));

        select.AddNewComponentItem("EventQuestComponent");
        select.AddNewComponentItem("SpawnQuestComponent");
        if (game.gameType is MoMGameType)
        {
            select.AddNewComponentItem("PuzzleQuestComponent");
        }

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is EventQuestComponent)
            {
                select.AddItem(kv.Value);
            }
        }

        select.Draw();
    }

    public void SelectAddEvent(int index, int button, bool replace, string eventName)
    {
        string toAdd = eventName;
        Game game = Game.Get();
        if (eventName.Equals("{NEW:EventQuestComponent}"))
        {
            int i = 0;
            while (game.quest.qd.components.ContainsKey("EventQuestComponent" + i))
            {
                i++;
            }
            toAdd = "EventQuestComponent" + i;
            Game.Get().quest.qd.components.Add(toAdd, new EventQuestComponent(toAdd));
        }

        if (eventName.Equals("{NEW:SpawnQuestComponent}"))
        {
            int i = 0;
            while (game.quest.qd.components.ContainsKey("SpawnQuestComponent" + i))
            {
                i++;
            }
            toAdd = "SpawnQuestComponent" + i;
            Game.Get().quest.qd.components.Add(toAdd, new SpawnQuestComponent(toAdd));
        }

        if (eventName.Equals("{NEW:PuzzleQuestComponent}"))
        {
            int i = 0;
            while (game.quest.qd.components.ContainsKey("PuzzleQuestComponent" + i))
            {
                i++;
            }
            toAdd = "PuzzleQuestComponent" + i;
            Game.Get().quest.qd.components.Add(toAdd, new PuzzleQuestComponent(toAdd));
        }

        if (replace)
        {
            EVENT_QUEST_COMPONENT_COMPONENT.nextEvent[button - 1][index] = toAdd;
        }
        else
        {
            EVENT_QUEST_COMPONENT_COMPONENT.nextEvent[button - 1].Insert(index, toAdd);
        }
        Update();
    }

    public void RemoveEvent(int index, int button)
    {
        EVENT_QUEST_COMPONENT_COMPONENT.nextEvent[button - 1].RemoveAt(index);
        Update();
    }


}
