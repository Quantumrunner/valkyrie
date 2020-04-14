using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.GameTypes;
using Assets.Scripts.UI;

namespace Assets.Scripts.QuestEditor
{
    public class EditorComponentCustomMonster : EditorComponent
    {

        private readonly StringKey BASE = new StringKey("val", "BASE");
        private readonly StringKey NAME = new StringKey("val", "NAME");
        private readonly StringKey ACTIVATIONS = new StringKey("val", "ACTIVATIONS");
        private readonly StringKey INFO = new StringKey("val", "INFO");
        private readonly StringKey HEALTH = new StringKey("val", "HEALTH");
        private readonly StringKey HEALTH_HERO = new StringKey("val", "HEALTH_HERO");
        private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");
        private readonly StringKey PLACE_IMG = new StringKey("val", "PLACE_IMG");
        private readonly StringKey IMAGE = new StringKey("val", "IMAGE");

        CustomMonsterQuestComponent MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;

        UIElementEditable nameUIE;
        UIElementEditablePaneled infoUIE;
        Dictionary<string, List<UIElementEditablePaneled>> attacksUIE;
        UIElementEditable healthUIE;
        UIElementEditable healthHeroUIE;
        UIElementEditable horrorUIE;
        UIElementEditable awarenessUIE;

        // TODO: Translate expansion traits, translate base monster names.

        public EditorComponentCustomMonster(string nameIn) : base()

        {
            Game game = Game.Get();
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT =
                game.quest.qd.components[nameIn] as CustomMonsterQuestComponent;
            component = MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT;
            name = component.sectionName;
            Update();
        }

        override protected void RefreshReference()
        {
            base.RefreshReference();
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT = component as CustomMonsterQuestComponent;
        }

        override public float AddSubComponents(float offset)
        {
            Game game = Game.Get();

            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 3, 1);
            ui.SetText(new StringKey("val", "X_COLON", BASE));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(3, offset, 16.5f, 1);
            ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster);
            ui.SetButton(delegate { SetBase(); });
            new UIElementBorder(ui);
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 3, 1);
            ui.SetText(new StringKey("val", "X_COLON", NAME));

            if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.monsterName.KeyExists())
            {
                nameUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
                nameUIE.SetLocation(3, offset, 13.5f, 1);
                nameUIE.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.monsterName.Translate());
                nameUIE.SetSingleLine();
                nameUIE.SetButton(delegate { UpdateName(); });
                new UIElementBorder(nameUIE);
                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(16.5f, offset, 3, 1);
                    ui.SetText(CommonStringKeys.RESET);
                    ui.SetButton(delegate { ClearName(); });
                    new UIElementBorder(ui);
                }
            }
            else
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.SET);
                ui.SetButton(delegate { SetName(); });
                new UIElementBorder(ui);
            }

            offset += 2;

            if (game.gameType is D2EGameType)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 16, 1);
                ui.SetText(new StringKey("val", "X_COLON", INFO));

                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                    MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.info.KeyExists())
                {
                    if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(16.5f, offset++, 3, 1);
                        ui.SetText(CommonStringKeys.RESET);
                        ui.SetButton(delegate { ClearInfo(); });
                        new UIElementBorder(ui);
                    }

                    infoUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
                    infoUIE.SetLocation(0.5f, offset, 19, 18);
                    infoUIE.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.info.Translate());
                    offset += infoUIE.HeightToTextPadding(1);
                    infoUIE.SetButton(delegate { UpdateInfo(); });
                    new UIElementBorder(infoUIE);
                }
                else
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(16.5f, offset++, 3, 1);
                    ui.SetText(CommonStringKeys.SET);
                    ui.SetButton(delegate { SetInfo(); });
                    new UIElementBorder(ui);
                }

                offset++;

                offset = DrawD2EActivations(offset);
            }
            else
            {
                offset = DrawMoMActivations(offset);
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(12.5f, offset, 6, 1);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TRAITS));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddTrait(); });
            new UIElementBorder(ui, Color.green);

            for (int index = 0; index < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits.Length; index++)
            {
                int i = index;
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(12.5f, offset, 6, 1);
                ui.SetText(new StringKey("val", MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits[index]));
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveTrait(i); });
                new UIElementBorder(ui, Color.red);
            }

            offset += 1;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", HEALTH));

            if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthDefined)
            {
                healthUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
                healthUIE.SetLocation(5, offset, 3, 1);
                healthUIE.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthBase.ToString());
                healthUIE.SetSingleLine();
                healthUIE.SetButton(delegate { UpdateHealth(); });
                new UIElementBorder(healthUIE);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(8, offset, 6, 1);
                ui.SetText(new StringKey("val", "X_COLON", HEALTH_HERO));

                healthHeroUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
                healthHeroUIE.SetLocation(14, offset, 2.5f, 1);
                healthHeroUIE.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthPerHero.ToString());
                healthHeroUIE.SetSingleLine();
                healthHeroUIE.SetButton(delegate { UpdateHealthHero(); });
                new UIElementBorder(healthHeroUIE);
                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(16.5f, offset, 3, 1);
                    ui.SetText(CommonStringKeys.RESET);
                    ui.SetButton(delegate { ClearHealth(); });
                    new UIElementBorder(ui);
                }
            }
            else
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.SET);
                ui.SetButton(delegate { SetHealth(); });
                new UIElementBorder(ui);
            }

            offset += 2;

            if (game.gameType is MoMGameType)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 7, 1);
                ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "horror")));

                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                    MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorDefined)
                {
                    horrorUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
                    horrorUIE.SetLocation(7, offset, 3, 1);
                    horrorUIE.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horror.ToString());
                    horrorUIE.SetSingleLine();
                    horrorUIE.SetButton(delegate { UpdateHorror(); });
                    new UIElementBorder(horrorUIE);

                    if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(10, offset, 3, 1);
                        ui.SetText(CommonStringKeys.RESET);
                        ui.SetButton(delegate { ClearHorror(); });
                        new UIElementBorder(ui);
                    }
                }
                else
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(10, offset, 3, 1);
                    ui.SetText(CommonStringKeys.SET);
                    ui.SetButton(delegate { SetHorror(); });
                    new UIElementBorder(ui);
                }

                offset += 2;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 7, 1);
                ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "AWARENESS")));

                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                    MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awarenessDefined)
                {
                    awarenessUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
                    awarenessUIE.SetLocation(7, offset, 3, 1);
                    awarenessUIE.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awareness.ToString());
                    awarenessUIE.SetSingleLine();
                    awarenessUIE.SetButton(delegate { UpdateAwareness(); });
                    new UIElementBorder(awarenessUIE);

                    if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(10, offset, 3, 1);
                        ui.SetText(CommonStringKeys.RESET);
                        ui.SetButton(delegate { ClearAwareness(); });
                        new UIElementBorder(ui);
                    }
                }
                else
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(10, offset, 3, 1);
                    ui.SetText(CommonStringKeys.SET);
                    ui.SetButton(delegate { SetAwareness(); });
                    new UIElementBorder(ui);
                }

                offset += 2;

                offset = DrawInvestigatorAttacks(offset);
            }

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 3, 1);
            ui.SetText(new StringKey("val", "X_COLON", IMAGE));
            if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePath.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(3, offset, 13.5f, 1);
                ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePath);
                ui.SetButton(delegate { SetImage(); });
                new UIElementBorder(ui);
                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(16.5f, offset, 3, 1);
                    ui.SetText(CommonStringKeys.RESET);
                    ui.SetButton(delegate { ClearImage(); });
                    new UIElementBorder(ui);
                }
            }
            else
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.SET);
                ui.SetButton(delegate { SetImage(); });
                new UIElementBorder(ui);
            }

            offset += 2;

            if (game.gameType is D2EGameType)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 4, 1);
                ui.SetText(new StringKey("val", "X_COLON", PLACE_IMG));
                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length == 0 ||
                    MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePlace.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(4, offset, 12.5f, 1);
                    ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePlace);
                    ui.SetButton(delegate { SetImagePlace(); });
                    new UIElementBorder(ui);
                    if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster.Length > 0)
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(16.5f, offset, 3, 1);
                        ui.SetText(CommonStringKeys.RESET);
                        ui.SetButton(delegate { ClearImagePlace(); });
                        new UIElementBorder(ui);
                    }
                }
                else
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(16.5f, offset, 3, 1);
                    ui.SetText(CommonStringKeys.SET);
                    ui.SetButton(delegate { SetImagePlace(); });
                    new UIElementBorder(ui);
                }

                offset += 2;
            }

            if (game.gameType is MoMGameType)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 5, 1);
                ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "EVADE")));

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(5, offset, 13.5f, 1);
                ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.evadeEvent);
                ui.SetButton(delegate { SetEvade(); });
                new UIElementBorder(ui);

                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.evadeEvent.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(18.5f, offset, 1, 1);
                    ui.SetText("<b>⇨</b>", Color.cyan);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate
                    {
                        QuestEditorData.SelectComponent(
                            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.evadeEvent);
                    });
                    new UIElementBorder(ui, Color.cyan);
                }

                offset += 2;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 5, 1);
                ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "horror")));

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(5, offset, 13.5f, 1);
                ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorEvent);
                ui.SetButton(delegate { SetHorrorEvent(); });
                new UIElementBorder(ui);

                if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorEvent.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(18.5f, offset, 1, 1);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetText("<b>⇨</b>", Color.cyan);
                    ui.SetButton(delegate
                    {
                        QuestEditorData.SelectComponent(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT
                            .horrorEvent);
                    });
                    new UIElementBorder(ui, Color.cyan);
                }

                offset += 2;
            }

            return offset;
        }

        public float DrawD2EActivations(float offset)
        {
            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 18, 1);
            ui.SetText(new StringKey("val", "X_COLON", ACTIVATIONS));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddActivation(); });
            new UIElementBorder(ui, Color.green);

            int index;
            for (index = 0; index < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length; index++)
            {
                int i = index;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 17, 1);
                ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[index]);
                ui.SetButton(delegate { AddActivation(i); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(17.5f, offset, 1, 1);
                ui.SetText("<b>⇨</b>", Color.cyan);
                ui.SetTextAlignment(TextAnchor.LowerCenter);
                ui.SetButton(delegate
                {
                    QuestEditorData.SelectComponent("Activation" +
                                                    MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT
                                                        .activations[i]);
                });
                new UIElementBorder(ui, Color.cyan);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveActivation(i); });
                new UIElementBorder(ui, Color.red);
                offset++;
            }

            return offset + 1;
        }

        public float DrawMoMActivations(float offset)
        {
            if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length > 1)
            {
                return DepreciatedMoMActivations(offset);
            }

            if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length == 1 &&
                !game.quest.qd.components.ContainsKey(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[0]))
            {
                return DepreciatedMoMActivations(offset);
            }

            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", ACTIVATIONS));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(5, offset, 13.5f, 1);
            ui.SetButton(delegate { SetActivation(); });
            new UIElementBorder(ui);
            if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length > 0)
            {
                ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[0]);
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset, 1, 1);
                ui.SetText("<b>⇨</b>", Color.cyan);
                ui.SetTextAlignment(TextAnchor.LowerCenter);
                ui.SetButton(delegate
                {
                    QuestEditorData.SelectComponent(
                        MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[0]);
                });
                new UIElementBorder(ui, Color.cyan);
            }

            return offset + 2;
        }

        public float DepreciatedMoMActivations(float offset)
        {
            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 11.5f, 1);
            ui.SetText("DEPRECIATED Activations", Color.red);

            offset += 1;
            int index;
            for (index = 0; index < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length; index++)
            {
                int i = index;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 11.5f, 1);
                ui.SetText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[index]);
                ui.SetButton(delegate
                {
                    QuestEditorData.SelectComponent(
                        MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[i]);
                });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(11.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveActivation(i); });
                new UIElementBorder(ui, Color.red);
                offset += 1;
            }

            return offset + 1;
        }

        public float DrawInvestigatorAttacks(float offset)
        {
            UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 18, 1);
            ui.SetText(new StringKey("val", "INVESTIGATOR_ATTACKS"));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(NewAttackType);
            new UIElementBorder(ui, Color.green);
            offset += 1;

            attacksUIE = new Dictionary<string, List<UIElementEditablePaneled>>();
            foreach (string attackType in MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks.Keys)
            {
                string aType = attackType;
                attacksUIE.Add(attackType, new List<UIElementEditablePaneled>());
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 18, 1);
                ui.SetText(new StringKey("val", attackType));

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(17.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.PLUS, Color.green);
                ui.SetButton(delegate { NewInvestigatorAttack(aType); });
                new UIElementBorder(ui, Color.green);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveInvestigatorAttackType(aType); });
                new UIElementBorder(ui, Color.red);
                offset += 1;

                foreach (StringKey attack in MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[
                    attackType])
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(18.5f, offset, 1, 1);
                    ui.SetText(CommonStringKeys.MINUS, Color.red);
                    ui.SetButton(delegate { RemoveInvestigatorAttack(attackType, attack); });
                    new UIElementBorder(ui, Color.red);
                    offset += 1;

                    UIElementEditablePaneled uie =
                        new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
                    uie.SetLocation(0.5f, offset, 19, 18);
                    uie.SetText(attack.Translate());
                    offset += uie.HeightToTextPadding(1);
                    uie.SetButton(UpdateAttacks);
                    new UIElementBorder(uie);
                    attacksUIE[attackType].Add(uie);
                }
            }

            return offset + 1;
        }

        public void SetBase()
        {
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
            {
                return;
            }

            Game game = Game.Get();
            UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectSetBase,
                new StringKey("val", "SELECT", CommonStringKeys.MONSTER));

            select.AddItem(CommonStringKeys.NONE.Translate(), "{NONE}");

            foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
            {
                select.AddItem(kv.Value);
            }

            select.ExcludeExpansions();
            select.Draw();
        }

        public void SelectSetBase(string type)
        {
            if (type.Equals("{NONE}"))
            {
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster = "";
                if (!MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.monsterName.KeyExists())
                {
                    SetName();
                }

                if (!MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.info.KeyExists())
                {
                    SetInfo();
                }

                if (!MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthDefined)
                {
                    SetHealth();
                }

                if (!MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorDefined)
                {
                    SetHorror();
                }

                if (!MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awarenessDefined)
                {
                    SetAwareness();
                }
            }
            else
            {
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.baseMonster = type.Split(" ".ToCharArray())[0];
            }

            Update();
        }

        public void UpdateName()
        {
            if (!nameUIE.Empty() && nameUIE.Changed())
            {
                LocalizationRead.updateScenarioText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.monstername_key,
                    nameUIE.GetText());
            }
        }

        public void ClearName()
        {
            LocalizationRead.dicts["qst"].Remove(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.monstername_key);
            Update();
        }

        public void SetName()
        {
            LocalizationRead.updateScenarioText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.monstername_key,
                NAME.Translate());
            Update();
        }

        public void UpdateInfo()
        {
            if (!infoUIE.Empty() && infoUIE.Changed())
            {
                LocalizationRead.updateScenarioText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.info_key,
                    infoUIE.GetText());
                if (!infoUIE.HeightAtTextPadding(1))
                {
                    Update();
                }
            }
        }

        public void ClearInfo()
        {
            LocalizationRead.dicts["qst"].Remove(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.info_key);
            Update();
        }

        public void SetInfo()
        {
            LocalizationRead.updateScenarioText(MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.info_key,
                INFO.Translate());
            Update();
        }

        public void AddActivation(int index = -1)
        {
            UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(
                delegate(string s) { SelectAddActivation(index, s); },
                new StringKey("val", "SELECT", CommonStringKeys.ACTIVATION));

            select.AddNewComponentItem("Activation");
            foreach (KeyValuePair<string, QuestComponent> kv in Game.Get().quest.qd.components)
            {
                if (kv.Value is ActivationQuestComponent)
                {
                    select.AddItem(kv.Value);
                }
            }

            select.Draw();
        }

        public void SelectAddActivation(int index, string key)
        {
            int i = 0;
            string toAdd = key;
            if (key.Equals("{NEW:Activation}"))
            {
                while (game.quest.qd.components.ContainsKey("Activation" + i))
                {
                    i++;
                }

                toAdd = "Activation" + i;
                Game.Get().quest.qd.components.Add(toAdd, new ActivationQuestComponent(toAdd));
            }

            if (index != -1)
            {
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[index] =
                    toAdd.Substring("Activation".Length);
                Update();
                return;
            }

            string[] newA = new string[MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length + 1];
            for (i = 0; i < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length; i++)
            {
                newA[i] = MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[i];
            }

            newA[i] = toAdd.Substring("Activation".Length);
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations = newA;
            Update();
        }

        public void SetActivation()
        {
            UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectSetActivation,
                new StringKey("val", "SELECT", CommonStringKeys.ACTIVATION));

            select.AddItem("{NONE}", "");
            select.AddNewComponentItem("Event");
            foreach (QuestComponent c in Game.Get().quest.qd.components.Values)
            {
                if (c.typeDynamic.IndexOf("Event") == 0)
                {
                    select.AddItem(c);
                }
            }

            select.Draw();
        }

        public void SelectSetActivation(string key)
        {
            if (key.Length == 0)
            {
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations = new string[0];
            }
            else
            {
                string toAdd = key;
                if (toAdd.Equals("{NEW:Event}"))
                {
                    int i = 0;
                    while (game.quest.qd.components.ContainsKey("Event" + i))
                    {
                        i++;
                    }

                    toAdd = "Event" + i;
                    Game.Get().quest.qd.components.Add(toAdd, new EventQuestComponent(toAdd));
                }

                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations = new string[1];
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[0] = toAdd;
            }

            Update();
        }

        public void RemoveActivation(int index)
        {
            string[] newA = new string[MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length - 1];

            int j = 0;
            for (int i = 0; i < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations.Length; i++)
            {
                if (i != index)
                {
                    newA[j++] = MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations[i];
                }
            }

            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.activations = newA;
            Update();
        }

        public void AddTrait()
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

            UIWindowSelectionList select = new UIWindowSelectionList(SelectAddTraits,
                new StringKey("val", "SELECT", CommonStringKeys.TRAITS));

            foreach (string s in traits)
            {
                select.AddItem(s);
            }

            select.Draw();
        }

        public void SelectAddTraits(string trait)
        {
            string[] newT = new string[MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits.Length + 1];
            int i;
            for (i = 0; i < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits.Length; i++)
            {
                newT[i] = MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits[i];
            }

            newT[i] = trait;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits = newT;
            Update();
        }

        public void RemoveTrait(int index)
        {
            string[] newT = new string[MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits.Length - 1];

            int j = 0;
            for (int i = 0; i < MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits.Length; i++)
            {
                if (i != index)
                {
                    newT[j++] = MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits[i];
                }
            }

            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.traits = newT;
            Update();
        }

        public void UpdateHealth()
        {
            float.TryParse(healthUIE.GetText(), out MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthBase);
        }

        public void UpdateHealthHero()
        {
            float.TryParse(healthHeroUIE.GetText(),
                out MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthPerHero);
        }

        public void ClearHealth()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthBase = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthPerHero = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthDefined = false;
            Update();
        }

        public void SetHealth()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthBase = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthPerHero = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.healthDefined = true;
            Update();
        }

        public void UpdateHorror()
        {
            int.TryParse(horrorUIE.GetText(), out MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horror);
        }

        public void ClearHorror()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horror = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorDefined = false;
            Update();
        }

        public void SetHorror()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horror = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorDefined = true;
            Update();
        }

        public void NewAttackType()
        {
            UIWindowSelectionList select = new UIWindowSelectionList(NewInvestigatorAttack,
                new StringKey("val", "SELECT", CommonStringKeys.TYPE));

            HashSet<string> attackTypes = new HashSet<string>();
            foreach (AttackData a in Game.Get().cd.investigatorAttacks.Values)
            {
                attackTypes.Add(a.attackType);
            }

            foreach (string s in attackTypes)
            {
                select.AddItem(new StringKey("val", s));
            }

            select.Draw();
        }

        public void NewInvestigatorAttack(string type)
        {
            if (!MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks.ContainsKey(type))
            {
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks.Add(type, new List<StringKey>());
            }

            int position = MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[type].Count + 1;
            StringKey newAttack = new StringKey("qst",
                MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.sectionName + ".Attack_" + type + "_" + position);
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[type].Add(newAttack);
            LocalizationRead.updateScenarioText(newAttack.key, "-");
            Update();
        }

        public void RemoveInvestigatorAttack(string type, StringKey attack)
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[type].Remove(attack);
            Update();
        }

        public void RemoveInvestigatorAttackType(string type)
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks.Remove(type);
            Update();
        }

        public void UpdateAttacks()
        {
            foreach (string type in MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks.Keys)
            {
                int index = 0;
                foreach (StringKey entry in MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[type])
                {
                    if (attacksUIE[type][index].Empty())
                    {
                        LocalizationRead.dicts["qst"].Remove(entry.key);
                        if (MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[type].Count == 1)
                        {
                            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks.Remove(type);
                        }
                        else
                        {
                            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.investigatorAttacks[type].RemoveAt(index);
                        }

                        Update();
                        return;
                    }

                    if (attacksUIE[type][index].Changed())
                    {
                        LocalizationRead.updateScenarioText(entry.key, attacksUIE[type][index].GetText());
                        if (!attacksUIE[type][index].HeightAtTextPadding(1))
                        {
                            Update();
                        }

                        return;
                    }

                    index++;
                }
            }
        }

        public void UpdateAwareness()
        {
            int.TryParse(awarenessUIE.GetText(), out MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awareness);
        }

        public void ClearAwareness()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awareness = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awarenessDefined = false;
            Update();
        }

        public void SetAwareness()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awareness = 0;
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.awarenessDefined = true;
            Update();
        }

        public void SetImage()
        {
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
            {
                return;
            }

            UIWindowSelectionList select = new UIWindowSelectionList(SelectImage, SELECT_IMAGE);

            string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
            foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
            {
                select.AddItem(s.Substring(relativePath.Length + 1));
            }

            foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
            {
                select.AddItem(s.Substring(relativePath.Length + 1));
            }

            select.Draw();
        }

        public void SelectImage(string image)
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePath = image;
            Update();
        }

        public void ClearImage()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePath = "";
            Update();
        }

        public void SetImagePlace()
        {
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
            {
                return;
            }

            UIWindowSelectionList select = new UIWindowSelectionList(SelectImagePlace, SELECT_IMAGE);

            string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
            foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
            {
                select.AddItem(s.Substring(relativePath.Length + 1));
            }

            foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
            {
                select.AddItem(s.Substring(relativePath.Length + 1));
            }

            select.Draw();
        }

        public void SelectImagePlace(string image)
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePlace = image;
            Update();
        }

        public void ClearImagePlace()
        {
            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.imagePlace = "";
            Update();
        }

        public void SetEvade()
        {
            UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectSetEvade,
                new StringKey("val", "SELECT", new StringKey("val", "EVADE")));

            select.AddItem("{NONE}", "");
            select.AddNewComponentItem("Event");
            foreach (KeyValuePair<string, QuestComponent> kv in Game.Get().quest.qd.components)
            {
                if (kv.Value.typeDynamic.Equals("Event"))
                {
                    select.AddItem(kv.Value);
                }
            }

            select.Draw();
        }

        public void SelectSetEvade(string evade)
        {
            string toAdd = evade;
            if (toAdd.Equals("{NEW:Event}"))
            {
                int i = 0;
                while (game.quest.qd.components.ContainsKey("Event" + i))
                {
                    i++;
                }

                toAdd = "Event" + i;
                Game.Get().quest.qd.components.Add(toAdd, new EventQuestComponent(toAdd));
            }

            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.evadeEvent = toAdd;
            Update();
        }

        public void SetHorrorEvent()
        {
            UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectSetHorror,
                new StringKey("val", "SELECT", new StringKey("val", "horror")));

            select.AddItem("{NONE}", "");
            select.AddNewComponentItem("Event");
            foreach (KeyValuePair<string, QuestComponent> kv in Game.Get().quest.qd.components)
            {
                if (kv.Value.typeDynamic.Equals("Event"))
                {
                    select.AddItem(kv.Value);
                }
            }

            select.Draw();
        }

        public void SelectSetHorror(string horror)
        {
            string toAdd = horror;
            if (toAdd.Equals("{NEW:Event}"))
            {
                int i = 0;
                while (game.quest.qd.components.ContainsKey("Event" + i))
                {
                    i++;
                }

                toAdd = "Event" + i;
                Game.Get().quest.qd.components.Add(toAdd, new EventQuestComponent(toAdd));
            }

            MONSTER_QUEST_COMPONENT_QUEST_COMPONENT_COMPONENT.horrorEvent = toAdd;
            Update();
        }
    }
}