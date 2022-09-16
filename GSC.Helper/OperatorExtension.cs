using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json;

namespace GSC.Helper
{
    public static class OperatorExtension
    {


        public static bool CheckMathOperator(this Operator value)
        {
            return value == Operator.Plus ||
            value == Operator.Minus ||
            value == Operator.Divide ||
            value == Operator.SquareRoot ||
            value == Operator.Power ||
            value == Operator.Multiplication ||
            value == Operator.Percentage;
        }


        public static bool IsDropDownCollectionForEditCheck(this CollectionSources? value)
        {
            if (value == null)
                return false;
            else
                return (value == CollectionSources.ComboBox ||
                                   value == CollectionSources.RadioButton ||
                                   value == CollectionSources.CheckBox ||
                                   value == CollectionSources.MultiCheckBox);
        }

        public static bool IsDropDownCollection(this CollectionSources value)
        {
            return (value == CollectionSources.ComboBox ||
                                   value == CollectionSources.RadioButton ||
                                   value == CollectionSources.NumericScale ||
                                   value == CollectionSources.CheckBox ||
                                   value == CollectionSources.MultiCheckBox);
        }

    }
}