namespace Assets.Scripts.Quest.VariableTests
{
    public class VarTestsParenthesis : VarTestsComponent
    {
        public string parenthesis;

        public VarTestsParenthesis()
        {
        }

        // can be "(" or ")"
        public VarTestsParenthesis(string inOp)
        {
            parenthesis = inOp;
        }

        override public string ToString()
        {
            return parenthesis;
        }

        public static string GetVarTestsComponentType()
        {
            return "VarTestsParenthesis";
        }

        override public string GetClassVarTestsComponentType()
        {
            return GetVarTestsComponentType();
        }

    }
}
