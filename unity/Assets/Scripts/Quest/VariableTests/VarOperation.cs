using ValkyrieTools;

namespace Assets.Scripts.Quest.VariableTests
{
    public class VarOperation : VarTestsComponent
    {
        public string var = "";
        public string operation = "";
        public string value = "";

        public VarOperation()
        {
        }

        public VarOperation(string inOp)
        {
            string[] splitted_string = inOp.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);

            if (splitted_string.Length != 3)
            {
                ValkyrieDebug.Log("Invalid var operation: " + inOp);
            }

            var = splitted_string[0];
            operation = splitted_string[1];
            value = splitted_string[2];

            // Support old internal var names (depreciated, format 3)
            var = UpdateVarName(var);
            value = UpdateVarName(value);
        }

        override public string ToString()
        {
            return var + ',' + operation + ',' + value;
        }

        private string UpdateVarName(string s)
        {
            if (s.Equals("#fire")) return "$fire";
            return s;
        }

        public static string GetVarTestsComponentType()
        {
            return "VarOperation";
        }

        override public string GetClassVarTestsComponentType()
        {
            return GetVarTestsComponentType();
        }

    }
}
