namespace Assets.Scripts.Quest.VariableTests
{
    public class VarTestsLogicalOperator : VarTestsComponent
    {
        public string op;

        public VarTestsLogicalOperator()
        {
            op = "AND";
        }

        public VarTestsLogicalOperator(string inOp)
        {
            op = inOp;
        }

        override public string ToString()
        {
            return op;
        }

        public void NextLogicalOperator()
        {
            if (op == "AND")
                op = "OR";
            else
                op = "AND";
        }

        public static string GetVarTestsComponentType()
        {
            return "VarTestsLogicalOperator";
        }

        override public string GetClassVarTestsComponentType()
        {
            return GetVarTestsComponentType();
        }
    }
}
