using Assets.Scripts.Content;
using Assets.Scripts.Content.ContentData;
using Assets.Scripts.Content.QuestComponents;

namespace Assets.Scripts.Quest
{
    // Class for Quest defined activations
    public class QuestActivation : ActivationData
    {
        public QuestActivation(ActivationQuestComponent qa) : base()
        {
            // Read data from activation
            ability = qa.ability;
            if (!ability.KeyExists())
            {
                ability = StringKey.NULL;
            }

            masterActions = qa.masterActions;
            if (!masterActions.KeyExists())
            {
                masterActions = StringKey.NULL;
            }

            minionActions = qa.minionActions;
            if (!minionActions.KeyExists())
            {
                minionActions = StringKey.NULL;
            }

            minionFirst = qa.minionFirst;
            masterFirst = qa.masterFirst;
            move = qa.move;
            if (!move.KeyExists())
            {
                move = StringKey.NULL;
            }

            moveButton = qa.moveButton;
            if (!moveButton.KeyExists())
            {
                moveButton = StringKey.NULL;
            }

            sectionName = qa.sectionName;
        }
    }
}
