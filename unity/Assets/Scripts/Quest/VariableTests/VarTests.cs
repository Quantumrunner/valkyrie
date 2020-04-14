using System.Collections.Generic;
using ValkyrieTools;

namespace Assets.Scripts.Quest.VariableTests
{
    public class VarTests
    {
        public List<VarTestsComponent> VarTestsComponents = null;

        public VarTests()
        {
            VarTestsComponents = new List<VarTestsComponent>();
        }

        public VarTests(List<VarTestsComponent> inTC)
        {
            VarTestsComponents = inTC;
        }

        override public string ToString()
        {
            string result = "";
            foreach (var v in VarTestsComponents)
            {
                result += v.GetClassVarTestsComponentType() + ":" + v.ToString() + " ";
            }

            return result;
        }

        public void Add(string str)
        {
            string[] part = str.Split(':');

            if (part[0].Equals(VarTestsLogicalOperator.GetVarTestsComponentType()))
            {
                VarTestsComponents.Add(new VarTestsLogicalOperator(part[1]));
            }

            if (part[0].Equals(VarTestsParenthesis.GetVarTestsComponentType()))
            {
                VarTestsComponents.Add(new VarTestsParenthesis(part[1]));
            }

            if (part[0].Equals(VarOperation.GetVarTestsComponentType()))
            {
                VarTestsComponents.Add(new VarOperation(part[1]));
            }
        }

        public void Add(VarTestsComponent tc)
        {
            if (tc.GetClassVarTestsComponentType().Equals("VarTestsParenthesis"))
            {
                VarTestsComponents.Insert(0, tc);
            }
            else
            {
                VarTestsComponents.Add(tc);
            }
        }


        /// <summary> Seach for the corresponding parenthesis of specified opening parenthesis </summary>
        /// <param name="index_open">index of opening parenthesis</param>
        /// <returns> index of closing parenthesis</returns>
        public int FindClosingParenthesis(int index_open)
        {
            VarTestsParenthesis tmp;
            int count = 0;

            for (int i = index_open; i < VarTestsComponents.Count; i++)
            {
                if (VarTestsComponents[i].GetClassVarTestsComponentType() ==
                    VarTestsParenthesis.GetVarTestsComponentType())
                {
                    tmp = (VarTestsParenthesis) VarTestsComponents[i];

                    if (tmp.parenthesis == "(")
                        count++;
                    else if (tmp.parenthesis == ")" && count == 0)
                        return i;
                    else
                        count--;
                }
            }

            // not found
            return -1;
        }

        /// <summary> Seach for the opening parenthesis of specified closing parenthesis </summary>
        /// <param name="index_close">index of closing parenthesis</param>
        /// <returns> index of opening parenthesis</returns>
        public int FindOpeningParenthesis(int index_close)
        {
            VarTestsParenthesis tmp;
            int count = 0;

            for (int i = index_close; i >= 0; i--)
            {
                if (VarTestsComponents[i].GetClassVarTestsComponentType() ==
                    VarTestsParenthesis.GetVarTestsComponentType())
                {
                    tmp = (VarTestsParenthesis) VarTestsComponents[i];

                    if (tmp.parenthesis == ")")
                        count++;
                    else if (tmp.parenthesis == "(" && count == 0)
                        return i;
                    else
                        count--;
                }
            }

            // not found
            return -1;
        }

        /// <summary> Search for the next valid position for parenthesis or varOperation </summary>
        /// <param name="index">index of current item to move</param>
        /// <param name="up">direction of requested movement (visually on Ui)</param>
        /// <returns> index of next position</returns>
        public int FindNextValidPosition(int index, bool up)
        {
            if (VarTestsComponents[index].GetClassVarTestsComponentType() !=
                VarTestsParenthesis.GetVarTestsComponentType()
                && VarTestsComponents[index].GetClassVarTestsComponentType() != VarOperation.GetVarTestsComponentType())
            {
                return -1;
            }

            int i = index;

            if (up) i--;
            else i++;

            if (VarTestsComponents[index].GetClassVarTestsComponentType() ==
                VarTestsParenthesis.GetVarTestsComponentType())
            {
                VarTestsParenthesis tmp = (VarTestsParenthesis) VarTestsComponents[index];
                while (i >= 0 && i <= VarTestsComponents.Count - 1)
                {

                    if ( // "(" should be before a varOperation or at the beginning a&(b  a&((b  (a&b 
                        (tmp.parenthesis == "("
                         && VarTestsComponents[i].GetClassVarTestsComponentType() ==
                         VarOperation.GetVarTestsComponentType())
                        ||
                        // ")" should be before a LogicalOperator or at the end  a)&b  a))&b  a&b)
                        (tmp.parenthesis == ")"
                         && VarTestsComponents[i].GetClassVarTestsComponentType() ==
                         VarTestsLogicalOperator.GetVarTestsComponentType())
                    )
                    {
                        if (up)
                            return i;
                        else if (i != index + 1) // if going down, ignore first item found
                            return i - 1;
                    }

                    if (up) i--;
                    else i++;
                }

                if (i >= VarTestsComponents.Count && tmp.parenthesis == ")")
                    return VarTestsComponents.Count - 1;
                else if (i <= 0 && tmp.parenthesis == "(")
                    return 0;
            }
            else if (VarTestsComponents[index].GetClassVarTestsComponentType() ==
                     VarOperation.GetVarTestsComponentType())
            {
                while (i >= 0 && i <= VarTestsComponents.Count - 1)
                {
                    // varOperation should take the place of another varOperation
                    if (VarTestsComponents[i].GetClassVarTestsComponentType() ==
                        VarOperation.GetVarTestsComponentType())
                    {
                        return i;
                    }

                    if (up) i--;
                    else i++;
                }
            }

            if (i < 0 || i >= VarTestsComponents.Count) return -1;

            // this should not happen
            ValkyrieDebug.Log("Invalid test position");

            return -1;
        }

        public void Remove(int index)
        {
            if (VarTestsComponents[index].GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType())
            {
                if (index > 0 && VarTestsComponents[index - 1].GetClassVarTestsComponentType() ==
                    VarTestsLogicalOperator.GetVarTestsComponentType())
                {
                    // remove TestOperator then VarOperation
                    VarTestsComponents.RemoveAt(index - 1);
                    VarTestsComponents.RemoveAt(index - 1);
                }
                else if (index < VarTestsComponents.Count - 1 &&
                         VarTestsComponents[index + 1].GetClassVarTestsComponentType() ==
                         VarTestsLogicalOperator.GetVarTestsComponentType())
                {
                    // remove VarOperation then TestOperator
                    VarTestsComponents.RemoveAt(index);
                    VarTestsComponents.RemoveAt(index);
                }
                else if (VarTestsComponents.Count == 1)
                {
                    VarTestsComponents.RemoveAt(0);
                }
                else
                {
                    // no operator before and after: items must be between parenthesis, we delete and run again
                    VarTestsComponents.RemoveAt(index + 1);
                    VarTestsComponents.RemoveAt(index - 1);
                    Remove(index - 1);
                }
            }
            else if (VarTestsComponents[index].GetClassVarTestsComponentType() ==
                     VarTestsParenthesis.GetVarTestsComponentType())
            {
                VarTestsParenthesis tmp = (VarTestsParenthesis) VarTestsComponents[index];
                int other_parenthesis_index = -1;

                if (tmp.parenthesis == "(")
                {
                    other_parenthesis_index = FindClosingParenthesis(index + 1);
                    VarTestsComponents.RemoveAt(other_parenthesis_index);
                    VarTestsComponents.RemoveAt(index);
                }
                else if (tmp.parenthesis == ")")
                {
                    other_parenthesis_index = FindOpeningParenthesis(index - 1);
                    VarTestsComponents.RemoveAt(index);
                    VarTestsComponents.RemoveAt(other_parenthesis_index);
                }
            }

        }

        public void moveComponent(int index, bool up)
        {
            int next_position = FindNextValidPosition(index, up);

            if (up) // up arrow means down in the list
            {
                for (int i = index; i > next_position; i--)
                {
                    VarTestsComponents.Reverse(i - 1, 2);
                }

                if (VarTestsComponents[next_position].GetClassVarTestsComponentType() ==
                    VarOperation.GetVarTestsComponentType())
                {
                    for (int i = next_position + 1; i < index; i++)
                    {
                        VarTestsComponents.Reverse(i, 2);
                    }
                }
            }
            else
            {
                for (int i = index; i < next_position; i++)
                {
                    VarTestsComponents.Reverse(i, 2);
                }

                if (VarTestsComponents[next_position].GetClassVarTestsComponentType() ==
                    VarOperation.GetVarTestsComponentType())
                {
                    for (int i = next_position - 1; i > index; i--)
                    {
                        VarTestsComponents.Reverse(i - 1, 2);
                    }
                }
            }
        }
    }
}