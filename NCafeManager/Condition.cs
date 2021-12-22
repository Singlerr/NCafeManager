using System;

namespace Conditions
{
    internal class Condition
    {
        public string Author;
        public string MenuName;
        public DateTime Time;
        public string Title;

        public Condition(string title, DateTime time, string author, string menuName)
        {
            Title = title;
            Time = time;
            Author = author;
            MenuName = menuName;
        }

        public Condition()
        {
        }

        public override string ToString()
        {
            return "Author=" + Author + "&MenuName=" + MenuName + "&Title=" + Title;
        }

        public static ConditionOperationType Parse(string str)
        {
            switch (str.ToUpper())
            {
                case "OR":
                    return ConditionOperationType.Or;
                case "AND":
                    return ConditionOperationType.And;
                case "AFTER":
                    return ConditionOperationType.After;
                case "BEFORE":
                    return ConditionOperationType.Before;
                default:
                    return ConditionOperationType.None;
            }
        }
    }

    internal enum ConditionOperationType
    {
        And,
        Or,
        After,
        Before,
        None
    }
}