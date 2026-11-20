using System.Text.RegularExpressions;
using LearnSql.Sql.Models;

namespace LearnSql.Sql.Generated;

[System.CodeDom.Compiler.GeneratedCode("Generate-SqliteErrors", "1.0")]
public static class SqliteErrorMapper
{
    private static readonly (SqliteErrorType Type, Regex Regex)[] Rules =
    [
        (SqliteErrorType.QualifiedTableNamesAreNotAllowedOnINSERTUPDATEAndDELETEStatementsWithinTriggers, SqliteErrorMessages.QualifiedTableNamesAreNotAllowedOnINSERTUPDATEAndDELETEStatementsWithinTriggers()),
        (SqliteErrorType.NumberOfColumnsInForeignKeyDoesNotMatchTheNumberOfColumnsInTheReferencedTable, SqliteErrorMessages.NumberOfColumnsInForeignKeyDoesNotMatchTheNumberOfColumnsInTheReferencedTable()),
        (SqliteErrorType.TheNOTINDEXEDClauseIsNotAllowedOnUPDATEOrDELETEStatementsWithinTriggers, SqliteErrorMessages.TheNOTINDEXEDClauseIsNotAllowedOnUPDATEOrDELETEStatementsWithinTriggers()),
        (SqliteErrorType.TheINDEXEDBYClauseIsNotAllowedOnUPDATEOrDELETEStatementsWithinTriggers, SqliteErrorMessages.TheINDEXEDBYClauseIsNotAllowedOnUPDATEOrDELETEStatementsWithinTriggers()),
        (SqliteErrorType.SELECTsToTheLeftAndRightOfDoNotHaveTheSameNumberOfResultColumns, SqliteErrorMessages.SELECTsToTheLeftAndRightOfDoNotHaveTheSameNumberOfResultColumns()),
        (SqliteErrorType.AmbiguousColumnName, SqliteErrorMessages.AmbiguousColumnName()),
        (SqliteErrorType.IndexAssociatedWithUNIQUEOrPRIMARYKEYConstraintCannotBeDropped, SqliteErrorMessages.IndexAssociatedWithUNIQUEOrPRIMARYKEYConstraintCannotBeDropped()),
        (SqliteErrorType.NoSuchColumn, SqliteErrorMessages.NoSuchColumn()),
        (SqliteErrorType.ONCONFLICTClauseDoesNotMatchAnyPRIMARYKEYOrUNIQUEConstraint, SqliteErrorMessages.ONCONFLICTClauseDoesNotMatchAnyPRIMARYKEYOrUNIQUEConstraint()),
        (SqliteErrorType.RANGEWithOffsetPRECEDINGFOLLOWINGRequiresOneORDERBYExpression, SqliteErrorMessages.RANGEWithOffsetPRECEDINGFOLLOWINGRequiresOneORDERBYExpression()),
        (SqliteErrorType.UnableToOpenATemporaryDatabaseFileForStoringTemporaryTables, SqliteErrorMessages.UnableToOpenATemporaryDatabaseFileForStoringTemporaryTables()),
        (SqliteErrorType.FILTERClauseMayOnlyBeUsedWithAggregateWindowFunctions, SqliteErrorMessages.FILTERClauseMayOnlyBeUsedWithAggregateWindowFunctions()),
        (SqliteErrorType.CannotJoinUsingColumnColumnNotPresentInBothTables, SqliteErrorMessages.CannotJoinUsingColumnColumnNotPresentInBothTables()),
        (SqliteErrorType.TemporaryStorageCannotBeChangedFromWithinATransaction, SqliteErrorMessages.TemporaryStorageCannotBeChangedFromWithinATransaction()),
        (SqliteErrorType.ExpressionsProhibitedInPRIMARYKEYAndUNIQUEConstraints, SqliteErrorMessages.ExpressionsProhibitedInPRIMARYKEYAndUNIQUEConstraints()),
        (SqliteErrorType.SecondArgumentToMustBeAConstantBetween00And10, SqliteErrorMessages.SecondArgumentToMustBeAConstantBetween00And10()),
        (SqliteErrorType.AggregateFunctionsAreNotAllowedInTheGROUPBYClause, SqliteErrorMessages.AggregateFunctionsAreNotAllowedInTheGROUPBYClause()),
        (SqliteErrorType.ForeignKeyOnShouldReferenceOnlyOneColumnOfTable, SqliteErrorMessages.ForeignKeyOnShouldReferenceOnlyOneColumnOfTable()),
        (SqliteErrorType.ORDERBYTermDoesNotMatchAnyColumnInTheResultSet, SqliteErrorMessages.ORDERBYTermDoesNotMatchAnyColumnInTheResultSet()),
        (SqliteErrorType.ThereIsAlreadyAnotherTableOrIndexWithThisName, SqliteErrorMessages.ThereIsAlreadyAnotherTableOrIndexWithThisName()),
        (SqliteErrorType.AUTOINCREMENTIsOnlyAllowedOnAnINTEGERPRIMARYKEY, SqliteErrorMessages.AUTOINCREMENTIsOnlyAllowedOnAnINTEGERPRIMARYKEY()),
        (SqliteErrorType.SafetyLevelMayNotBeChangedInsideATransaction, SqliteErrorMessages.SafetyLevelMayNotBeChangedInsideATransaction()),
        (SqliteErrorType.GeneratedColumnsCannotBePartOfThePRIMARYKEY, SqliteErrorMessages.GeneratedColumnsCannotBePartOfThePRIMARYKEY()),
        (SqliteErrorType.TargetObjectAliasMayNotAppearInFROMClause, SqliteErrorMessages.TargetObjectAliasMayNotAppearInFROMClause()),
        (SqliteErrorType.DISTINCTAggregatesMustHaveExactlyOneArgument, SqliteErrorMessages.DISTINCTAggregatesMustHaveExactlyOneArgument()),
        (SqliteErrorType.ANATURALJoinMayNotHaveAnONOrUSINGClause, SqliteErrorMessages.ANATURALJoinMayNotHaveAnONOrUSINGClause()),
        (SqliteErrorType.AUTOINCREMENTNotAllowedOnWITHOUTROWIDTables, SqliteErrorMessages.AUTOINCREMENTNotAllowedOnWITHOUTROWIDTables()),
        (SqliteErrorType.BYTermOutOfRangeShouldBeBetween1And, SqliteErrorMessages.BYTermOutOfRangeShouldBeBetween1And()),
        (SqliteErrorType.RAISEMayOnlyBeUsedWithinATriggerProgram, SqliteErrorMessages.RAISEMayOnlyBeUsedWithinATriggerProgram()),
        (SqliteErrorType.CannotUseWindowFunctionsInRecursiveQueries, SqliteErrorMessages.CannotUseWindowFunctionsInRecursiveQueries()),
        (SqliteErrorType.DISTINCTNotAllowedOnOrderedSetAggregate, SqliteErrorMessages.DISTINCTNotAllowedOnOrderedSetAggregate()),
        (SqliteErrorType.CannotCreateATEMPIndexOnNonTEMPTable, SqliteErrorMessages.CannotCreateATEMPIndexOnNonTEMPTable()),
        (SqliteErrorType.DISTINCTIsNotSupportedForWindowFunctions, SqliteErrorMessages.DISTINCTIsNotSupportedForWindowFunctions()),
        (SqliteErrorType.ORDERBYMayNotBeUsedWithNonAggregate, SqliteErrorMessages.ORDERBYMayNotBeUsedWithNonAggregate()),
        (SqliteErrorType.AllVALUESMustHaveTheSameNumberOfTerms, SqliteErrorMessages.AllVALUESMustHaveTheSameNumberOfTerms()),
        (SqliteErrorType.CannotDropColumnNoOtherColumnsExist, SqliteErrorMessages.CannotDropColumnNoOtherColumnsExist()),
        (SqliteErrorType.ExpressionTreeIsTooLargeMaximumDepth, SqliteErrorMessages.ExpressionTreeIsTooLargeMaximumDepth()),
        (SqliteErrorType.RETURNINGIsNotAvailableOnVirtualTables, SqliteErrorMessages.RETURNINGIsNotAvailableOnVirtualTables()),
        (SqliteErrorType.TableHasColumnsButValuesWereSupplied, SqliteErrorMessages.TableHasColumnsButValuesWereSupplied()),
        (SqliteErrorType.TemporaryTriggerMayNotHaveQualifiedName, SqliteErrorMessages.TemporaryTriggerMayNotHaveQualifiedName()),
        (SqliteErrorType.UnableToIdentifyTheObjectToBeReindexed, SqliteErrorMessages.UnableToIdentifyTheObjectToBeReindexed()),
        (SqliteErrorType.FILTERMayNotBeUsedWithNonAggregate, SqliteErrorMessages.FILTERMayNotBeUsedWithNonAggregate()),
        (SqliteErrorType.CannotCreateINSTEADOFTriggerOnTable, SqliteErrorMessages.CannotCreateINSTEADOFTriggerOnTable()),
        (SqliteErrorType.MustHaveAtLeastOneNonGeneratedColumn, SqliteErrorMessages.MustHaveAtLeastOneNonGeneratedColumn()),
        (SqliteErrorType.TriggerMayNotWriteToShadowTable, SqliteErrorMessages.TriggerMayNotWriteToShadowTable()),
        (SqliteErrorType.UPSERTNotImplementedForVirtualTable, SqliteErrorMessages.UPSERTNotImplementedForVirtualTable()),
        (SqliteErrorType.UnknownColumnInForeignKeyDefinition, SqliteErrorMessages.UnknownColumnInForeignKeyDefinition()),
        (SqliteErrorType.CannotOpenTableWithGeneratedColumns, SqliteErrorMessages.CannotOpenTableWithGeneratedColumns()),
        (SqliteErrorType.DefaultValueOfColumnIsNotConstant, SqliteErrorMessages.DefaultValueOfColumnIsNotConstant()),
        (SqliteErrorType.VirtualTablesCannotUseComputedColumns, SqliteErrorMessages.VirtualTablesCannotUseComputedColumns()),
        (SqliteErrorType.ConflictingONCONFLICTClausesSpecified, SqliteErrorMessages.ConflictingONCONFLICTClausesSpecified()),
        (SqliteErrorType.RETURNINGMayNotUseTABLEWildcards, SqliteErrorMessages.RETURNINGMayNotUseTABLEWildcards()),
        (SqliteErrorType.RecursiveAggregateQueriesNotSupported, SqliteErrorMessages.RecursiveAggregateQueriesNotSupported()),
        (SqliteErrorType.CannotCreateTriggersOnVirtualTables, SqliteErrorMessages.CannotCreateTriggersOnVirtualTables()),
        (SqliteErrorType.CannotUseDEFAULTOnAGeneratedColumn, SqliteErrorMessages.CannotUseDEFAULTOnAGeneratedColumn()),
        (SqliteErrorType.ForeignKeyMismatchReferencing, SqliteErrorMessages.ForeignKeyMismatchReferencing()),
        (SqliteErrorType.MultipleReferencesToRecursiveTable, SqliteErrorMessages.MultipleReferencesToRecursiveTable()),
        (SqliteErrorType.TemporaryTableNameMustBeUnqualified, SqliteErrorMessages.TemporaryTableNameMustBeUnqualified()),
        (SqliteErrorType.VariableNumberMustBeBetween1And, SqliteErrorMessages.VariableNumberMustBeBetween1And()),
        (SqliteErrorType.WrongNumberOfArgumentsToFunction, SqliteErrorMessages.WrongNumberOfArgumentsToFunction()),
        (SqliteErrorType.CannotCreateTriggersOnShadowTables, SqliteErrorMessages.CannotCreateTriggersOnShadowTables()),
        (SqliteErrorType.CannotReferenceObjectsInDatabase, SqliteErrorMessages.CannotReferenceObjectsInDatabase()),
        (SqliteErrorType.MayNotBeUsedAsAWindowFunction, SqliteErrorMessages.MayNotBeUsedAsAWindowFunction()),
        (SqliteErrorType.ObjectNameReservedForInternalUse, SqliteErrorMessages.ObjectNameReservedForInternalUse()),
        (SqliteErrorType.VIEWsAndOrSubqueriesNestedTooDeep, SqliteErrorMessages.VIEWsAndOrSubqueriesNestedTooDeep()),
        (SqliteErrorType.CannotINSERTIntoGeneratedColumn, SqliteErrorMessages.CannotINSERTIntoGeneratedColumn()),
        (SqliteErrorType.HAVINGClauseOnANonAggregateQuery, SqliteErrorMessages.HAVINGClauseOnANonAggregateQuery()),
        (SqliteErrorType.TableHasMoreThanOnePrimaryKey, SqliteErrorMessages.TableHasMoreThanOnePrimaryKey()),
        (SqliteErrorType.CannotCreateTriggerOnSystemTable, SqliteErrorMessages.CannotCreateTriggerOnSystemTable()),
        (SqliteErrorType.ClauseShouldComeAfterNotBefore, SqliteErrorMessages.ClauseShouldComeAfterNotBefore()),
        (SqliteErrorType.INElementHasTermExpected, SqliteErrorMessages.INElementHasTermExpected()),
        (SqliteErrorType.TooManyReferencesToMax65535, SqliteErrorMessages.TooManyReferencesToMax65535()),
        (SqliteErrorType.CannotModifyBecauseItIsAView, SqliteErrorMessages.CannotModifyBecauseItIsAView()),
        (SqliteErrorType.ParametersAreNotAllowedInViews, SqliteErrorMessages.ParametersAreNotAllowedInViews()),
        (SqliteErrorType.AmbiguousReferenceToInUSING, SqliteErrorMessages.AmbiguousReferenceToInUSING()),
        (SqliteErrorType.IsNotAnOrderedSetAggregate, SqliteErrorMessages.IsNotAnOrderedSetAggregate()),
        (SqliteErrorType.MisuseOfAliasedWindowFunction, SqliteErrorMessages.MisuseOfAliasedWindowFunction()),
        (SqliteErrorType.TriggerContainsTooManySteps, SqliteErrorMessages.TriggerContainsTooManySteps()),
        (SqliteErrorType.AJOINClauseIsRequiredBefore, SqliteErrorMessages.AJOINClauseIsRequiredBefore()),
        (SqliteErrorType.CannotOpenTableWithoutRowid, SqliteErrorMessages.CannotOpenTableWithoutRowid()),
        (SqliteErrorType.CannotUPDATEGeneratedColumn, SqliteErrorMessages.CannotUPDATEGeneratedColumn()),
        (SqliteErrorType.CannotUseRETURNINGInATrigger, SqliteErrorMessages.CannotUseRETURNINGInATrigger()),
        (SqliteErrorType.ExpectedColumnsForButGot, SqliteErrorMessages.ExpectedColumnsForButGot()),
        (SqliteErrorType.SyntaxErrorAfterColumnName, SqliteErrorMessages.SyntaxErrorAfterColumnName()),
        (SqliteErrorType.TooManyFROMClauseTermsMax, SqliteErrorMessages.TooManyFROMClauseTermsMax()),
        (SqliteErrorType.TooManyTermsInCompoundSELECT, SqliteErrorMessages.TooManyTermsInCompoundSELECT()),
        (SqliteErrorType.TooManyTermsInORDERBYClause, SqliteErrorMessages.TooManyTermsInORDERBYClause()),
        (SqliteErrorType.VirtualTablesMayNotBeAltered, SqliteErrorMessages.VirtualTablesMayNotBeAltered()),
        (SqliteErrorType.VirtualTablesMayNotBeIndexed, SqliteErrorMessages.VirtualTablesMayNotBeIndexed()),
        (SqliteErrorType.CannotCreateTriggerOnView, SqliteErrorMessages.CannotCreateTriggerOnView()),
        (SqliteErrorType.IndexAlreadyNamed, SqliteErrorMessages.IndexAlreadyNamed()),
        (SqliteErrorType.NotAuthorizedToUseFunction, SqliteErrorMessages.NotAuthorizedToUseFunction()),
        (SqliteErrorType.ORDERBYWithoutLIMITOnUPDATE, SqliteErrorMessages.ORDERBYWithoutLIMITOnUPDATE()),
        (SqliteErrorType.CannotAddAPRIMARYKEYColumn, SqliteErrorMessages.CannotAddAPRIMARYKEYColumn()),
        (SqliteErrorType.GeneratedColumnsNotSupported, SqliteErrorMessages.GeneratedColumnsNotSupported()),
        (SqliteErrorType.MisuseOfAggregateFunction, SqliteErrorMessages.MisuseOfAggregateFunction()),
        (SqliteErrorType.ReferencesTablesToItsRight, SqliteErrorMessages.ReferencesTablesToItsRight()),
        (SqliteErrorType.TableAlreadyNamed, SqliteErrorMessages.TableAlreadyNamed()),
        (SqliteErrorType.TableHasValuesForColumns, SqliteErrorMessages.TableHasValuesForColumns()),
        (SqliteErrorType.TooManyArgumentsOnFunction, SqliteErrorMessages.TooManyArgumentsOnFunction()),
        (SqliteErrorType.TooManyArgumentsOnMax, SqliteErrorMessages.TooManyArgumentsOnMax()),
        (SqliteErrorType.UnsupportedFrameSpecification, SqliteErrorMessages.UnsupportedFrameSpecification()),
        (SqliteErrorType.UseDROPTABLEToDeleteTable, SqliteErrorMessages.UseDROPTABLEToDeleteTable()),
        (SqliteErrorType.TooManyColumnsInResultSet, SqliteErrorMessages.TooManyColumnsInResultSet()),
        (SqliteErrorType.UnsafeUseOfVirtualTable, SqliteErrorMessages.UnsafeUseOfVirtualTable()),
        (SqliteErrorType.CannotAddAColumnToAView, SqliteErrorMessages.CannotAddAColumnToAView()),
        (SqliteErrorType.FailedToSetLockProxyFile, SqliteErrorMessages.FailedToSetLockProxyFile()),
        (SqliteErrorType.ForeignKeyConstraintFailed, SqliteErrorMessages.ForeignKeyConstraintFailed()),
        (SqliteErrorType.PRIMARYKEYMissingOnTable, SqliteErrorMessages.PRIMARYKEYMissingOnTable()),
        (SqliteErrorType.UseDROPVIEWToDeleteView, SqliteErrorMessages.UseDROPVIEWToDeleteView()),
        (SqliteErrorType.AccessToViewProhibited, SqliteErrorMessages.AccessToViewProhibited()),
        (SqliteErrorType.CannotOverrideOfWindow, SqliteErrorMessages.CannotOverrideOfWindow()),
        (SqliteErrorType.ErrorInGeneratedColumn, SqliteErrorMessages.ErrorInGeneratedColumn()),
        (SqliteErrorType.InternalQueryPlannerError, SqliteErrorMessages.InternalQueryPlannerError()),
        (SqliteErrorType.MisuseOfAliasedAggregate, SqliteErrorMessages.MisuseOfAliasedAggregate()),
        (SqliteErrorType.NoSuchCollationSequence, SqliteErrorMessages.NoSuchCollationSequence()),
        (SqliteErrorType.NotNullConstraintFailed, SqliteErrorMessages.NotNullConstraintFailed()),
        (SqliteErrorType.SyntaxErrorNearORDERBY, SqliteErrorMessages.SyntaxErrorNearORDERBY()),
        (SqliteErrorType.TooManyTermsInBYClause, SqliteErrorMessages.TooManyTermsInBYClause()),
        (SqliteErrorType.CannotOpenVirtualTable, SqliteErrorMessages.CannotOpenVirtualTable()),
        (SqliteErrorType.DuplicateWITHTableName, SqliteErrorMessages.DuplicateWITHTableName()),
        (SqliteErrorType.GeneratedColumnLoopOn, SqliteErrorMessages.GeneratedColumnLoopOn()),
        (SqliteErrorType.TableHasNoColumnNamed, SqliteErrorMessages.TableHasNoColumnNamed()),
        (SqliteErrorType.ViewIsCircularlyDefined, SqliteErrorMessages.ViewIsCircularlyDefined()),
        (SqliteErrorType.CannotAddAUNIQUEColumn, SqliteErrorMessages.CannotAddAUNIQUEColumn()),
        (SqliteErrorType.MoreThanAggregateTerms, SqliteErrorMessages.MoreThanAggregateTerms()),
        (SqliteErrorType.ORDERBYWithoutLIMITOn, SqliteErrorMessages.ORDERBYWithoutLIMITOn()),
        (SqliteErrorType.TableMayNotBeModified, SqliteErrorMessages.TableMayNotBeModified()),
        (SqliteErrorType.UniqueConstraintFailed, SqliteErrorMessages.UniqueConstraintFailed()),
        (SqliteErrorType.UnknownDatatypeFor, SqliteErrorMessages.UnknownDatatypeFor()),
        (SqliteErrorType.AtMostTablesInAJoin, SqliteErrorMessages.AtMostTablesInAJoin()),
        (SqliteErrorType.CheckConstraintFailed, SqliteErrorMessages.CheckConstraintFailed()),
        (SqliteErrorType.ColumnsAssignedValues, SqliteErrorMessages.ColumnsAssignedValues()),
        (SqliteErrorType.SyntaxErrorNearLIMIT, SqliteErrorMessages.SyntaxErrorNearLIMIT()),
        (SqliteErrorType.TableMayNotBeAltered, SqliteErrorMessages.TableMayNotBeAltered()),
        (SqliteErrorType.TableMayNotBeDropped, SqliteErrorMessages.TableMayNotBeDropped()),
        (SqliteErrorType.TableMayNotBeIndexed, SqliteErrorMessages.TableMayNotBeIndexed()),
        (SqliteErrorType.UnsupportedUseOfNULLS, SqliteErrorMessages.UnsupportedUseOfNULLS()),
        (SqliteErrorType.AccessToIsProhibited, SqliteErrorMessages.AccessToIsProhibited()),
        (SqliteErrorType.NotAWritableDirectory, SqliteErrorMessages.NotAWritableDirectory()),
        (SqliteErrorType.TriggersNestedTooDeep, SqliteErrorMessages.TriggersNestedTooDeep()),
        (SqliteErrorType.ViewMayNotBeAltered, SqliteErrorMessages.ViewMayNotBeAltered()),
        (SqliteErrorType.ViewsMayNotBeIndexed, SqliteErrorMessages.ViewsMayNotBeIndexed()),
        (SqliteErrorType.CannotDropColumn, SqliteErrorMessages.CannotDropColumn()),
        (SqliteErrorType.DuplicateColumnName, SqliteErrorMessages.DuplicateColumnName()),
        (SqliteErrorType.MisuseOfAggregate, SqliteErrorMessages.MisuseOfAggregate()),
        (SqliteErrorType.TriggerAlreadyExists, SqliteErrorMessages.TriggerAlreadyExists()),
        (SqliteErrorType.XBestIndexMalfunction, SqliteErrorMessages.XBestIndexMalfunction()),
        (SqliteErrorType.AuthorizerMalfunction, SqliteErrorMessages.AuthorizerMalfunction()),
        (SqliteErrorType.MissingDatatypeFor, SqliteErrorMessages.MissingDatatypeFor()),
        (SqliteErrorType.MisuseOfFunction, SqliteErrorMessages.MisuseOfFunction()),
        (SqliteErrorType.TooManySQLVariables, SqliteErrorMessages.TooManySQLVariables()),
        (SqliteErrorType.UnknownTableOption, SqliteErrorMessages.UnknownTableOption()),
        (SqliteErrorType.UnrecognizedToken, SqliteErrorMessages.UnrecognizedToken()),
        (SqliteErrorType.UnsupportedEncoding, SqliteErrorMessages.UnsupportedEncoding()),
        (SqliteErrorType.CannotRowidAlias, SqliteErrorMessages.CannotRowidAlias()),
        (SqliteErrorType.CannotUseVariables, SqliteErrorMessages.CannotUseVariables()),
        (SqliteErrorType.HexLiteralTooBig, SqliteErrorMessages.HexLiteralTooBig()),
        (SqliteErrorType.IndexAlreadyExists, SqliteErrorMessages.IndexAlreadyExists()),
        (SqliteErrorType.NearSyntaxError, SqliteErrorMessages.NearSyntaxError()),
        (SqliteErrorType.CannotUPSERTAView, SqliteErrorMessages.CannotUPSERTAView()),
        (SqliteErrorType.IsNotAFunction, SqliteErrorMessages.IsNotAFunction()),
        (SqliteErrorType.TooManyColumnsIn, SqliteErrorMessages.TooManyColumnsIn()),
        (SqliteErrorType.TooManyColumnsOn, SqliteErrorMessages.TooManyColumnsOn()),
        (SqliteErrorType.TooManyRIGHTJOINs, SqliteErrorMessages.TooManyRIGHTJOINs()),
        (SqliteErrorType.UnknownFunction, SqliteErrorMessages.UnknownFunction()),
        (SqliteErrorType.ValuesForColumns, SqliteErrorMessages.ValuesForColumns()),
        (SqliteErrorType.NoTablesSpecified, SqliteErrorMessages.NoTablesSpecified()),
        (SqliteErrorType.OversizedInteger, SqliteErrorMessages.OversizedInteger()),
        (SqliteErrorType.UnknownJoinType, SqliteErrorMessages.UnknownJoinType()),
        (SqliteErrorType.CannotOpenView, SqliteErrorMessages.CannotOpenView()),
        (SqliteErrorType.NoSuchFunction, SqliteErrorMessages.NoSuchFunction()),
        (SqliteErrorType.NoQuerySolution, SqliteErrorMessages.NoQuerySolution()),
        (SqliteErrorType.NoSuchIndex, SqliteErrorMessages.NoSuchIndex()),
        (SqliteErrorType.NoSuchTrigger, SqliteErrorMessages.NoSuchTrigger()),
        (SqliteErrorType.RowValueMisused, SqliteErrorMessages.RowValueMisused()),
        (SqliteErrorType.UnknownDatabase, SqliteErrorMessages.UnknownDatabase()),
        (SqliteErrorType.AlreadyExists, SqliteErrorMessages.AlreadyExists()),
        (SqliteErrorType.CorruptDatabase, SqliteErrorMessages.CorruptDatabase()),
        (SqliteErrorType.IncompleteInput, SqliteErrorMessages.IncompleteInput()),
        (SqliteErrorType.InvalidRootpage, SqliteErrorMessages.InvalidRootpage()),
        (SqliteErrorType.NoSuchModule, SqliteErrorMessages.NoSuchModule()),
        (SqliteErrorType.NoSuchWindow, SqliteErrorMessages.NoSuchWindow()),
        (SqliteErrorType.UnsafeUseOf, SqliteErrorMessages.UnsafeUseOf()),
        (SqliteErrorType.ConstructProhibitedInContext, SqliteErrorMessages.ConstructProhibitedInContext()),
        (SqliteErrorType.NoSuchTable, SqliteErrorMessages.NoSuchTable()),
        (SqliteErrorType.NotAuthorized, SqliteErrorMessages.NotAuthorized()),
        (SqliteErrorType.OutOfMemory, SqliteErrorMessages.OutOfMemory()),
        (SqliteErrorType.TooManyCTEs, SqliteErrorMessages.TooManyCTEs()),
        (SqliteErrorType.CannotAlterObject, SqliteErrorMessages.CannotAlterObject()),
    ];

    public static IReadOnlyList<SqliteErrorType> MatchableTypes { get; } = [.. Rules.Select(((SqliteErrorType Type, Regex Regex) rule) => rule.Type)];

    public static SqliteError Match(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        string text = message;
        Match wrapper = SqliteErrorMessages.Wrapper().Match(message);
        if (wrapper.Success)
        {
            text = wrapper.Groups["Message"].Value;
        }

        foreach ((SqliteErrorType type, Regex regex) in Rules)
        {
            Match match = regex.Match(text);
            if (!match.Success)
            {
                continue;
            }

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (string groupName in regex.GetGroupNames())
            {
                if (!int.TryParse(groupName, out _))
                {
                    arguments[groupName] = match.Groups[groupName].Value;
                }
            }

            return new SqliteError(type, text, arguments);
        }

        return new SqliteError(SqliteErrorType.Unknown, text, new Dictionary<string, string>());
    }
}
