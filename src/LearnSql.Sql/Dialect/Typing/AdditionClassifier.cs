namespace LearnSql.Sql.Dialect.Typing;

public enum AdditionMeaning
{
    Unknown,
    Numeric,
    Concatenation,
    DateArithmetic,
}

public static class AdditionClassifier
{
    public static AdditionMeaning Classify(ValueKind left, ValueKind right)
    {
        if (left == ValueKind.DateTime || right == ValueKind.DateTime)
        {
            bool otherIsNumeric = left == ValueKind.Numeric || right == ValueKind.Numeric;
            return otherIsNumeric ? AdditionMeaning.DateArithmetic : AdditionMeaning.Unknown;
        }

        if (left == ValueKind.Text || right == ValueKind.Text)
        {
            ValueKind other = left == ValueKind.Text ? right : left;
            if (other == ValueKind.Text || other == ValueKind.Unknown || other == ValueKind.Null)
                return AdditionMeaning.Concatenation;
            if (other == ValueKind.Numeric)
                return AdditionMeaning.Numeric;
            return AdditionMeaning.Unknown;
        }

        if (left == ValueKind.Numeric || right == ValueKind.Numeric)
            return AdditionMeaning.Numeric;

        return AdditionMeaning.Unknown;
    }
}
