using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.UI;

public class EditorComponentSpawn : EditorComponentEvent
{
    // Not used yet
    //private readonly StringKey MONSTER_NORMAL = new StringKey("val", "MONSTER_NORMAL");

    private readonly StringKey POSITION_TYPE_UNUSED = new StringKey("val", "POSITION_TYPE_UNUSED");
    private readonly StringKey POSITION_TYPE_HIGHLIGHT = new StringKey("val", "POSITION_TYPE_HIGHLIGHT");
    private readonly StringKey MONSTER_UNIQUE = new StringKey("val", "MONSTER_UNIQUE");

    private readonly StringKey UNIQUE_TITLE = new StringKey("val", "UNIQUE_TITLE");
    private readonly StringKey UNIQUE_INFO = new StringKey("val", "UNIQUE_INFO");
    private readonly StringKey HEALTH = new StringKey("val", "HEALTH");
    private readonly StringKey HEALTH_HERO = new StringKey("val", "HEALTH_HERO");
    private readonly StringKey TYPES = new StringKey("val", "TYPES");
    
    private readonly StringKey REQ_TRAITS = new StringKey("val", "REQ_TRAITS");
    private readonly StringKey POOL_TRAITS = new StringKey("val", "POOL_TRAITS");
    
    
    Spawn SPAWN_QUEST_COMPONENT_COMPONENT;

    UIElementEditable uniqueTitleUIE;
    UIElementEditablePaneled uniqueTextUIE;
    UIElementEditable healthUIE;
    UIElementEditable healthHeroUIE;

    public EditorComponentSpawn(string nameIn) : base(nameIn)
    {
    }

    override public void AddLocationType(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(14, offset, 4, 1);
        ui.SetButton(delegate { PositionTypeCycle(); });
        new UIElementBorder(ui);
        if (!component.locationSpecified)
        {
            ui.SetText(POSITION_TYPE_UNUSED);
        }
        else
        {
            ui.SetText(POSITION_TYPE_HIGHLIGHT);
        }
    }
    
    override public float AddSubEventComponents(float offset)
    {
        SPAWN_QUEST_COMPONENT_COMPONENT = component as Spawn;

        UIElement ui = null;

        if (game.gameType is D2EGameType)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 6, 1);
            ui.SetText(new StringKey("val", "X_COLON", MONSTER_UNIQUE));

            if (!SPAWN_QUEST_COMPONENT_COMPONENT.unique)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(6, offset, 3, 1);
                ui.SetText(CommonStringKeys.FALSE);
                ui.SetButton(delegate { UniqueToggle(); });
                new UIElementBorder(ui);
                offset += 2;
            }
            else
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(6, offset, 3, 1);
                ui.SetText(CommonStringKeys.TRUE);
                ui.SetButton(delegate { UniqueToggle(); });
                new UIElementBorder(ui);
                offset += 2;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 5, 1);
                ui.SetText(new StringKey("val", "X_COLON", UNIQUE_TITLE));

                uniqueTitleUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
                uniqueTitleUIE.SetLocation(5, offset, 14.5f, 1);
                uniqueTitleUIE.SetText(SPAWN_QUEST_COMPONENT_COMPONENT.uniqueTitle.Translate());
                uniqueTitleUIE.SetSingleLine();
                uniqueTitleUIE.SetButton(delegate { UpdateUniqueTitle(); });
                new UIElementBorder(uniqueTitleUIE);
                offset += 2;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset++, 20, 1);
                ui.SetText(new StringKey("val", "X_COLON", UNIQUE_INFO));

                uniqueTextUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
                uniqueTextUIE.SetLocation(0.5f, offset, 19, 18);
                uniqueTextUIE.SetText(SPAWN_QUEST_COMPONENT_COMPONENT.uniqueText.Translate());
                offset += uniqueTextUIE.HeightToTextPadding(1);
                uniqueTextUIE.SetButton(delegate { UpdateUniqueText(); });
                new UIElementBorder(uniqueTextUIE);
                offset += 1;
            }
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", HEALTH));

        healthUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        healthUIE.SetLocation(5, offset, 3, 1);
        healthUIE.SetText(SPAWN_QUEST_COMPONENT_COMPONENT.uniqueHealthBase.ToString());
        healthUIE.SetSingleLine();
        healthUIE.SetButton(delegate { UpdateHealth(); });
        new UIElementBorder(healthUIE);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 7, 1);
        ui.SetText(new StringKey("val", "X_COLON", HEALTH_HERO));

        healthHeroUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        healthHeroUIE.SetLocation(15, offset, 3, 1);
        healthHeroUIE.SetText(SPAWN_QUEST_COMPONENT_COMPONENT.uniqueHealthHero.ToString());
        healthHeroUIE.SetSingleLine();
        healthHeroUIE.SetButton(delegate { UpdateHealthHero(); });
        new UIElementBorder(healthHeroUIE);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(1.5f, offset, 17, 1);
        ui.SetText(new StringKey("val", "X_COLON", TYPES));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { MonsterTypeAdd(0); });
        new UIElementBorder(ui, Color.green);

        int i = 0;
        for (i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length; i++)
        {
            int mSlot = i;
            string mName = SPAWN_QUEST_COMPONENT_COMPONENT.mTypes[i];
            if (mName.IndexOf("Monster") == 0)
            {
                mName = mName.Substring("Monster".Length);
            }

            if ((SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length > 1) || (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length > 0) || (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length > 0))
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { MonsterTypeRemove(mSlot); });
                new UIElementBorder(ui, Color.red);
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            if (game.quest.qd.components.ContainsKey(SPAWN_QUEST_COMPONENT_COMPONENT.mTypes[i]))
            {
                ui.SetLocation(1.5f, offset, 16, 1);
                UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                link.SetLocation(17.5f, offset, 1, 1);
                link.SetText("<b>⇨</b>", Color.cyan);
                link.SetTextAlignment(TextAnchor.LowerCenter);
                link.SetButton(delegate { QuestEditorData.SelectComponent(SPAWN_QUEST_COMPONENT_COMPONENT.mTypes[mSlot]); });
                new UIElementBorder(link, Color.cyan);
            }
            else
            {
                ui.SetLocation(1.5f, offset, 17, 1);
            }
            ui.SetText(mName);
            ui.SetButton(delegate { MonsterTypeReplace(mSlot); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { MonsterTypeAdd(mSlot + 1); });
            new UIElementBorder(ui, Color.green);
        }
        offset++;

        float traitOffset = offset;
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(REQ_TRAITS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { MonsterTraitsAdd(); });
        new UIElementBorder(ui, Color.green);

        for (i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length; i++)
        {
            int mSlot = i;
            string mName = SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired[i];

            if ((SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length > 0) || (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length > 1) || (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length > 0))
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { MonsterTraitsRemove(mSlot); });
                new UIElementBorder(ui, Color.red);
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(1.5f, offset++, 8, 1);
            ui.SetText(new StringKey("val", mName));
            ui.SetButton(delegate { MonsterTraitReplace(mSlot); });
            new UIElementBorder(ui);
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(10.5f, traitOffset, 8, 1);
        ui.SetText(POOL_TRAITS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, traitOffset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { MonsterTraitsAdd(true); });
        new UIElementBorder(ui, Color.green);

        for (int j = 0; j < SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length; j++)
        {
            int mSlot = j;
            string mName = SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool[j];

            if ((SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length > 0) || (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length > 0) || (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length > 1))
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(10.5f, traitOffset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { MonsterTraitsRemove(mSlot, true); });
                new UIElementBorder(ui, Color.red);
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(11.5f, traitOffset++, 8, 1);
            ui.SetText(new StringKey("val", mName));
            ui.SetButton(delegate { MonsterTraitReplace(mSlot, true); });
            new UIElementBorder(ui);
        }

        if (traitOffset > offset) offset = traitOffset;

        offset++;
        if (game.gameType is D2EGameType || game.gameType is IAGameType)
        {
            offset = AddPlacementComponenets(offset);
        }

        return offset;
    }

    public float AddPlacementComponenets(float offset)
    {
        for (int heroes = 2; heroes < 5; heroes++)
        {
            int h = heroes;
            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 18, 1);
            ui.SetText(new StringKey("val", "NUMBER_HEROS", heroes));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { MonsterPlaceAdd(h); });
            new UIElementBorder(ui, Color.green);

            for (int i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes].Length; i++)
            {
                int mSlot = i;
                string place = SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes][i];

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { MonsterPlaceRemove(h, mSlot); });
                new UIElementBorder(ui, Color.red);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(1.5f, offset, 17, 1);
                ui.SetText(place);
                ui.SetButton(delegate { MonsterPlaceAdd(h, mSlot); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetTextAlignment(TextAnchor.LowerCenter);
                ui.SetText("<b>⇨</b>", Color.cyan);
                ui.SetButton(delegate { QuestEditorData.SelectComponent(place); });
                new UIElementBorder(ui, Color.cyan);
            }
            offset++;
        }

        return offset;
    }

    override public void PositionTypeCycle()
    {
        SPAWN_QUEST_COMPONENT_COMPONENT.locationSpecified = !SPAWN_QUEST_COMPONENT_COMPONENT.locationSpecified;
        Update();
    }

    public void UniqueToggle()
    {
        SPAWN_QUEST_COMPONENT_COMPONENT.unique = !SPAWN_QUEST_COMPONENT_COMPONENT.unique;
        if (!SPAWN_QUEST_COMPONENT_COMPONENT.unique)
        {
            LocalizationRead.dicts["qst"].Remove(SPAWN_QUEST_COMPONENT_COMPONENT.uniquetitle_key);
            LocalizationRead.dicts["qst"].Remove(SPAWN_QUEST_COMPONENT_COMPONENT.uniquetext_key);
        }
        else
        {
            LocalizationRead.updateScenarioText(SPAWN_QUEST_COMPONENT_COMPONENT.uniquetitle_key, SPAWN_QUEST_COMPONENT_COMPONENT.sectionName);
            LocalizationRead.updateScenarioText(SPAWN_QUEST_COMPONENT_COMPONENT.uniquetext_key, "-");
        }
        Update();
    }

    public void UpdateHealth()
    {
        float.TryParse(healthUIE.GetText(), out SPAWN_QUEST_COMPONENT_COMPONENT.uniqueHealthBase);
        Update();
    }

    public void UpdateHealthHero()
    {
        float.TryParse(healthHeroUIE.GetText(), out SPAWN_QUEST_COMPONENT_COMPONENT.uniqueHealthHero);
        Update();
    }

    public void UpdateUniqueTitle()
    {
        if (!uniqueTitleUIE.Empty() && uniqueTitleUIE.Changed())
        {
            LocalizationRead.updateScenarioText(SPAWN_QUEST_COMPONENT_COMPONENT.uniquetitle_key, uniqueTitleUIE.GetText());
        }
    }

    public void UpdateUniqueText()
    {
        if (!uniqueTextUIE.Empty() && uniqueTextUIE.Changed())
        {
            LocalizationRead.updateScenarioText(SPAWN_QUEST_COMPONENT_COMPONENT.uniquetext_key, uniqueTextUIE.GetText());
            if (!uniqueTextUIE.HeightAtTextPadding(1))
            {
                Update();
            }
        }
    }

    public void MonsterTypeAdd(int pos)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate(string s) { SelectMonsterType(s, pos); }, new StringKey("val", "SELECT", CommonStringKeys.MONSTER));

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is CustomMonster)
            {
                select.AddItem(kv.Value);
            }
            if (kv.Value is Spawn)
            {
                select.AddItem(kv.Value);
            }
        }

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            select.AddItem(kv.Value);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void MonsterTypeReplace(int pos)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectMonsterType(s, pos, true); }, new StringKey("val", "SELECT", CommonStringKeys.MONSTER));

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is CustomMonster)
            {
                select.AddItem(kv.Value);
            }
            if (kv.Value is Spawn)
            {
                select.AddItem(kv.Value);
            }
        }

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            select.AddItem(kv.Value);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectMonsterType(string type, int pos, bool replace = false)
    {
        if (replace)
        {
            SPAWN_QUEST_COMPONENT_COMPONENT.mTypes[pos] = type.Split(" ".ToCharArray())[0];
        }
        else
        {
            string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length + 1];

            int j = 0;
            for (int i = 0; i < newM.Length; i++)
            {
                if (j == pos && i == j)
                {
                    newM[i] = type.Split(" ".ToCharArray())[0];
                }
                else
                {
                    newM[i] = SPAWN_QUEST_COMPONENT_COMPONENT.mTypes[j];
                    j++;
                }
            }
            SPAWN_QUEST_COMPONENT_COMPONENT.mTypes = newM;
        }
        Update();
    }

    public void MonsterTypeRemove(int pos)
    {
        if ((SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length == 1) && (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length == 0) && (SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length == 0))
        {
            return;
        }

        string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length - 1];

        int j = 0;
        for (int i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = SPAWN_QUEST_COMPONENT_COMPONENT.mTypes[i];
                j++;
            }
        }
        SPAWN_QUEST_COMPONENT_COMPONENT.mTypes = newM;
        Update();
    }

    public void MonsterTraitReplace(int pos, bool pool = false)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            foreach (string s in kv.Value.traits)
            {
                traits.Add(s);
            }
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate(string s) { SelectMonsterTraitReplace(pos, pool, s); }, new StringKey("val", "SELECT", CommonStringKeys.TRAITS));
        foreach (string s in traits)
        {
            select.AddItem(new StringKey("val", s));
        }
        select.Draw();
    }

    public void SelectMonsterTraitReplace(int pos, bool pool, string trait)
    {
        if (pool)
        {
            SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool[pos] = trait;
        }
        else
        {
            SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired[pos] = trait;
        }
        Update();
    }

    public void MonsterTraitsAdd(bool pool = false)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            foreach (string s in kv.Value.traits)
            {
                traits.Add(s);
            }
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate(string s) { SelectMonsterTrait(pool, s); }, new StringKey("val", "SELECT", CommonStringKeys.TRAITS));
        foreach (string s in traits)
        {
            select.AddItem(new StringKey("val", s));
        }
        select.Draw();
    }

    public void SelectMonsterTrait(bool pool, string trait)
    {
        if (pool)
        {
            string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length + 1];

            int i;
            for (i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length; i++)
            {
                newM[i] = SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool[i];
            }

            newM[i] = trait;
            SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool = newM;
        }
        else
        {
            string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length + 1];

            int i;
            for (i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length; i++)
            {
                newM[i] = SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired[i];
            }

            newM[i] = trait;
            SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired = newM;
        }
        Update();
    }

    public void MonsterTraitsRemove(int pos, bool pool = false)
    {
        if ((SPAWN_QUEST_COMPONENT_COMPONENT.mTypes.Length + SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length + SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length) <= 1)
        {
            return;
        }
        if (pool)
        {
            string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length - 1];

            int j = 0;
            for (int i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool.Length; i++)
            {
                if (i != pos || i != j)
                {
                    newM[j] = SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool[i];
                    j++;
                }
            }
            SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsPool = newM;
        }
        else
        {
            string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length - 1];

            int j = 0;
            for (int i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired.Length; i++)
            {
                if (i != pos || i != j)
                {
                    newM[j] = SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired[i];
                    j++;
                }
            }
            SPAWN_QUEST_COMPONENT_COMPONENT.mTraitsRequired = newM;
        }
        Update();
    }

    public void MonsterPlaceAdd(int heroes, int slot = -1)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { MonsterPlaceAddSelection(heroes, slot, s); }, CommonStringKeys.SELECT_ITEM);

        select.AddNewComponentItem("MPlace");

        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is MPlace)
            {
                select.AddItem(kv.Value);
            }
        }
        select.Draw();
    }

    public void MonsterPlaceAddSelection(int heroes, int slot, string name)
    {
        if (name.Equals("{NEW:MPlace}"))
        {
            Game game = Game.Get();
            int index = 0;

            while (game.quest.qd.components.ContainsKey("MPlace" + index))
            {
                index++;
            }
            game.quest.qd.components.Add("MPlace" + index, new MPlace("MPlace" + index));
            name = "MPlace" + index;
        }

        if (slot == -1)
        {
            string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes].Length + 1];
            int i;
            for (i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes].Length; i++)
            {
                newM[i] = SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes][i];
            }

            newM[i] = name;
            SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes] = newM;
        }
        else
        {
            SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes][slot] = name;
        }
        Update();
    }

    public void MonsterPlaceRemove(int heroes, int pos)
    {
        string[] newM = new string[SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes].Length - 1];

        int j = 0;
        for (int i = 0; i < SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes].Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes][i];
                j++;
            }
        }
        SPAWN_QUEST_COMPONENT_COMPONENT.placement[heroes] = newM;
        Update();
    }
}
