using System.Text.RegularExpressions;

namespace LearnSql.Sql.Generated;

[System.CodeDom.Compiler.GeneratedCode("Generate-SqliteErrors", "1.0")]
internal static partial class SqliteErrorMessages
{
    [GeneratedRegex(@"^SQLite Error (?<Code>\d+): '(?<Message>.*)'\.$", RegexOptions.Singleline)]
    internal static partial Regex Wrapper();

    [GeneratedRegex(@"^qualified table names are not allowed on INSERT, UPDATE, and DELETE statements within triggers$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex QualifiedTableNamesAreNotAllowedOnINSERTUPDATEAndDELETEStatementsWithinTriggers();

    [GeneratedRegex(@"^number of columns in foreign key does not match the number of columns in the referenced table$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NumberOfColumnsInForeignKeyDoesNotMatchTheNumberOfColumnsInTheReferencedTable();

    [GeneratedRegex(@"^the NOT INDEXED clause is not allowed on UPDATE or DELETE statements within triggers$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TheNOTINDEXEDClauseIsNotAllowedOnUPDATEOrDELETEStatementsWithinTriggers();

    [GeneratedRegex(@"^the INDEXED BY clause is not allowed on UPDATE or DELETE statements within triggers$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TheINDEXEDBYClauseIsNotAllowedOnUPDATEOrDELETEStatementsWithinTriggers();

    [GeneratedRegex(@"^SELECTs to the left and right of (?<Operator>.+?) do not have the same number of result columns$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex SELECTsToTheLeftAndRightOfDoNotHaveTheSameNumberOfResultColumns();

    [GeneratedRegex(@"^(?:ambiguous column name: ""(?<Column>.+?)"" - should this be a string literal in single-quotes\?|ambiguous column name: (?<Column>.+?))$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AmbiguousColumnName();

    [GeneratedRegex(@"^index associated with UNIQUE or PRIMARY KEY constraint cannot be dropped$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex IndexAssociatedWithUNIQUEOrPRIMARYKEYConstraintCannotBeDropped();

    [GeneratedRegex(@"^(?:no such column: ""(?<Column>.+?)"" - should this be a string literal in single-quotes\?|no such column: ""(?<Column>.+?)""|no such column: (?<Column>.+?))$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchColumn();

    [GeneratedRegex(@"^(?<Prefix>.*?)ON CONFLICT clause does not match any PRIMARY KEY or UNIQUE constraint$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ONCONFLICTClauseDoesNotMatchAnyPRIMARYKEYOrUNIQUEConstraint();

    [GeneratedRegex(@"^RANGE with offset PRECEDING/FOLLOWING requires one ORDER BY expression$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex RANGEWithOffsetPRECEDINGFOLLOWINGRequiresOneORDERBYExpression();

    [GeneratedRegex(@"^unable to open a temporary database file for storing temporary tables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnableToOpenATemporaryDatabaseFileForStoringTemporaryTables();

    [GeneratedRegex(@"^FILTER clause may only be used with aggregate window functions$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex FILTERClauseMayOnlyBeUsedWithAggregateWindowFunctions();

    [GeneratedRegex(@"^cannot join using column (?<Column>.+?) - column not present in both tables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotJoinUsingColumnColumnNotPresentInBothTables();

    [GeneratedRegex(@"^temporary storage cannot be changed from within a transaction$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TemporaryStorageCannotBeChangedFromWithinATransaction();

    [GeneratedRegex(@"^expressions prohibited in PRIMARY KEY and UNIQUE constraints$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ExpressionsProhibitedInPRIMARYKEYAndUNIQUEConstraints();

    [GeneratedRegex(@"^second argument to (?<Function>.+?)\(\) must be a constant between 0\.0 and 1\.0$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex SecondArgumentToMustBeAConstantBetween00And10();

    [GeneratedRegex(@"^aggregate functions are not allowed in the GROUP BY clause$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AggregateFunctionsAreNotAllowedInTheGROUPBYClause();

    [GeneratedRegex(@"^foreign key on (?<Column>.+?) should reference only one column of table (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ForeignKeyOnShouldReferenceOnlyOneColumnOfTable();

    [GeneratedRegex(@"^(?<Position>\d+(?:st|nd|rd|th)) ORDER BY term does not match any column in the result set$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ORDERBYTermDoesNotMatchAnyColumnInTheResultSet();

    [GeneratedRegex(@"^there is already another table or index with this name: (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ThereIsAlreadyAnotherTableOrIndexWithThisName();

    [GeneratedRegex(@"^AUTOINCREMENT is only allowed on an INTEGER PRIMARY KEY$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AUTOINCREMENTIsOnlyAllowedOnAnINTEGERPRIMARYKEY();

    [GeneratedRegex(@"^Safety level may not be changed inside a transaction$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex SafetyLevelMayNotBeChangedInsideATransaction();

    [GeneratedRegex(@"^generated columns cannot be part of the PRIMARY KEY$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex GeneratedColumnsCannotBePartOfThePRIMARYKEY();

    [GeneratedRegex(@"^target object/alias may not appear in FROM clause: (?<Clause>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TargetObjectAliasMayNotAppearInFROMClause();

    [GeneratedRegex(@"^DISTINCT aggregates must have exactly one argument$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex DISTINCTAggregatesMustHaveExactlyOneArgument();

    [GeneratedRegex(@"^a NATURAL join may not have an ON or USING clause$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ANATURALJoinMayNotHaveAnONOrUSINGClause();

    [GeneratedRegex(@"^AUTOINCREMENT not allowed on WITHOUT ROWID tables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AUTOINCREMENTNotAllowedOnWITHOUTROWIDTables();

    [GeneratedRegex(@"^(?<Position>\d+(?:st|nd|rd|th)) (?<Clause>.+?) BY term out of range - should be between 1 and (?<Max>-?\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex BYTermOutOfRangeShouldBeBetween1And();

    [GeneratedRegex(@"^RAISE\(\) may only be used within a trigger-program$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex RAISEMayOnlyBeUsedWithinATriggerProgram();

    [GeneratedRegex(@"^cannot use window functions in recursive queries$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotUseWindowFunctionsInRecursiveQueries();

    [GeneratedRegex(@"^DISTINCT not allowed on ordered-set aggregate (?<Aggregate>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex DISTINCTNotAllowedOnOrderedSetAggregate();

    [GeneratedRegex(@"^cannot create a TEMP index on non-TEMP table ""(?<Table>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotCreateATEMPIndexOnNonTEMPTable();

    [GeneratedRegex(@"^DISTINCT is not supported for window functions$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex DISTINCTIsNotSupportedForWindowFunctions();

    [GeneratedRegex(@"^ORDER BY may not be used with non-aggregate (?<Aggregate>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ORDERBYMayNotBeUsedWithNonAggregate();

    [GeneratedRegex(@"^all VALUES must have the same number of terms$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AllVALUESMustHaveTheSameNumberOfTerms();

    [GeneratedRegex(@"^cannot drop column ""(?<Column>.+?)"": no other columns exist$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotDropColumnNoOtherColumnsExist();

    [GeneratedRegex(@"^Expression tree is too large \(maximum depth (?<Depth>-?\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ExpressionTreeIsTooLargeMaximumDepth();

    [GeneratedRegex(@"^(?<Statement>.+?) RETURNING is not available on virtual tables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex RETURNINGIsNotAvailableOnVirtualTables();

    [GeneratedRegex(@"^table (?<Table>.+?) has (?<ColumnCount>-?\d+) columns but (?<ValueCount>-?\d+) values were supplied$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableHasColumnsButValuesWereSupplied();

    [GeneratedRegex(@"^temporary trigger may not have qualified name$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TemporaryTriggerMayNotHaveQualifiedName();

    [GeneratedRegex(@"^unable to identify the object to be reindexed$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnableToIdentifyTheObjectToBeReindexed();

    [GeneratedRegex(@"^FILTER may not be used with non-aggregate (?<Aggregate>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex FILTERMayNotBeUsedWithNonAggregate();

    [GeneratedRegex(@"^cannot create INSTEAD OF trigger on table: (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotCreateINSTEADOFTriggerOnTable();

    [GeneratedRegex(@"^must have at least one non-generated column$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MustHaveAtLeastOneNonGeneratedColumn();

    [GeneratedRegex(@"^trigger ""(?<Trigger>.+?)"" may not write to shadow table ""(?<Table>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TriggerMayNotWriteToShadowTable();

    [GeneratedRegex(@"^UPSERT not implemented for virtual table ""(?<Table>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UPSERTNotImplementedForVirtualTable();

    [GeneratedRegex(@"^unknown column ""(?<Column>.+?)"" in foreign key definition$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnknownColumnInForeignKeyDefinition();

    [GeneratedRegex(@"^cannot open table with generated columns: (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotOpenTableWithGeneratedColumns();

    [GeneratedRegex(@"^default value of column \[(?<Column>.+?)] is not constant$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex DefaultValueOfColumnIsNotConstant();

    [GeneratedRegex(@"^virtual tables cannot use computed columns$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex VirtualTablesCannotUseComputedColumns();

    [GeneratedRegex(@"^conflicting ON CONFLICT clauses specified$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ConflictingONCONFLICTClausesSpecified();

    [GeneratedRegex(@"^RETURNING may not use ""TABLE\.\*"" wildcards$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex RETURNINGMayNotUseTABLEWildcards();

    [GeneratedRegex(@"^recursive aggregate queries not supported$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex RecursiveAggregateQueriesNotSupported();

    [GeneratedRegex(@"^cannot create triggers on virtual tables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotCreateTriggersOnVirtualTables();

    [GeneratedRegex(@"^cannot use DEFAULT on a generated column$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotUseDEFAULTOnAGeneratedColumn();

    [GeneratedRegex(@"^foreign key mismatch - ""(?<Mismatch>.+?)"" referencing ""(?<Referencing>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ForeignKeyMismatchReferencing();

    [GeneratedRegex(@"^multiple references to recursive table: (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MultipleReferencesToRecursiveTable();

    [GeneratedRegex(@"^temporary table name must be unqualified$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TemporaryTableNameMustBeUnqualified();

    [GeneratedRegex(@"^variable number must be between \?1 and \?(?<Max>-?\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex VariableNumberMustBeBetween1And();

    [GeneratedRegex(@"^wrong number of arguments to function (?<Function>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex WrongNumberOfArgumentsToFunction();

    [GeneratedRegex(@"^cannot create triggers on shadow tables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotCreateTriggersOnShadowTables();

    [GeneratedRegex(@"^(?<ObjectType>.+?) (?<Name>.+?) cannot reference objects in database (?<Database>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotReferenceObjectsInDatabase();

    [GeneratedRegex(@"^(?<Function>.+?)\(\) may not be used as a window function$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MayNotBeUsedAsAWindowFunction();

    [GeneratedRegex(@"^object name reserved for internal use: (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ObjectNameReservedForInternalUse();

    [GeneratedRegex(@"^VIEWs and/or subqueries nested too deep$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex VIEWsAndOrSubqueriesNestedTooDeep();

    [GeneratedRegex(@"^cannot INSERT into generated column ""(?<Column>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotINSERTIntoGeneratedColumn();

    [GeneratedRegex(@"^HAVING clause on a non-aggregate query$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex HAVINGClauseOnANonAggregateQuery();

    [GeneratedRegex(@"^table ""(?<Table>.+?)"" has more than one primary key$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableHasMoreThanOnePrimaryKey();

    [GeneratedRegex(@"^cannot create trigger on system table$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotCreateTriggerOnSystemTable();

    [GeneratedRegex(@"^(?<Clause>.+?) clause should come after (?<After>.+?) not before$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ClauseShouldComeAfterNotBefore();

    [GeneratedRegex(@"^IN\(\.\.\.\) element has (?<TermCount>-?\d+) term(?<Plural>.*?) - expected (?<Expected>-?\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex INElementHasTermExpected();

    [GeneratedRegex(@"^too many references to ""(?<Name>.+?)"": max 65535$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyReferencesToMax65535();

    [GeneratedRegex(@"^cannot modify (?<Name>.+?) because it is a view$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotModifyBecauseItIsAView();

    [GeneratedRegex(@"^parameters are not allowed in views$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ParametersAreNotAllowedInViews();

    [GeneratedRegex(@"^ambiguous reference to (?<Name>.+?) in USING\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AmbiguousReferenceToInUSING();

    [GeneratedRegex(@"^(?<Aggregate>.+?)\(\) is not an ordered-set aggregate$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex IsNotAnOrderedSetAggregate();

    [GeneratedRegex(@"^misuse of aliased window function (?<Function>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MisuseOfAliasedWindowFunction();

    [GeneratedRegex(@"^trigger ""(?<Trigger>.+?)"" contains too many steps$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TriggerContainsTooManySteps();

    [GeneratedRegex(@"^a JOIN clause is required before (?<Before>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AJOINClauseIsRequiredBefore();

    [GeneratedRegex(@"^cannot open table without rowid: (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotOpenTableWithoutRowid();

    [GeneratedRegex(@"^cannot UPDATE generated column ""(?<Column>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotUPDATEGeneratedColumn();

    [GeneratedRegex(@"^cannot use RETURNING in a trigger$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotUseRETURNINGInATrigger();

    [GeneratedRegex(@"^expected (?<Expected>-?\d+) columns for '(?<Name>.+?)' but got (?<Got>-?\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ExpectedColumnsForButGot();

    [GeneratedRegex(@"^syntax error after column name ""(?<Name>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex SyntaxErrorAfterColumnName();

    [GeneratedRegex(@"^too many FROM clause terms, max: (?<Max>-?\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyFROMClauseTermsMax();

    [GeneratedRegex(@"^too many terms in compound SELECT$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyTermsInCompoundSELECT();

    [GeneratedRegex(@"^too many terms in ORDER BY clause$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyTermsInORDERBYClause();

    [GeneratedRegex(@"^virtual tables may not be altered$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex VirtualTablesMayNotBeAltered();

    [GeneratedRegex(@"^virtual tables may not be indexed$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex VirtualTablesMayNotBeIndexed();

    [GeneratedRegex(@"^cannot create (?<Timing>.+?) trigger on view: (?<View>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotCreateTriggerOnView();

    [GeneratedRegex(@"^there is already an index named (?<Index>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex IndexAlreadyNamed();

    [GeneratedRegex(@"^not authorized to use function: (?<Function>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NotAuthorizedToUseFunction();

    [GeneratedRegex(@"^ORDER BY without LIMIT on UPDATE$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ORDERBYWithoutLIMITOnUPDATE();

    [GeneratedRegex(@"^Cannot add a PRIMARY KEY column$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotAddAPRIMARYKEYColumn();

    [GeneratedRegex(@"^generated columns not supported$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex GeneratedColumnsNotSupported();

    [GeneratedRegex(@"^misuse of aggregate function (?<Function>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MisuseOfAggregateFunction();

    [GeneratedRegex(@"^(?<Context>.+?) references tables to its right$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ReferencesTablesToItsRight();

    [GeneratedRegex(@"^there is already a table named (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableAlreadyNamed();

    [GeneratedRegex(@"^table (?<Table>.+?) has (?<ValueCount>-?\d+) values for (?<ColumnCount>-?\d+) columns$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableHasValuesForColumns();

    [GeneratedRegex(@"^too many arguments on function (?<Function>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyArgumentsOnFunction();

    [GeneratedRegex(@"^too many arguments on (?<Function>.+?)\(\) - max (?<Max>-?\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyArgumentsOnMax();

    [GeneratedRegex(@"^unsupported frame specification$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnsupportedFrameSpecification();

    [GeneratedRegex(@"^use DROP TABLE to delete table (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UseDROPTABLEToDeleteTable();

    [GeneratedRegex(@"^too many columns in result set$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyColumnsInResultSet();

    [GeneratedRegex(@"^unsafe use of virtual table ""(?<Table>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnsafeUseOfVirtualTable();

    [GeneratedRegex(@"^Cannot add a column to a view$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotAddAColumnToAView();

    [GeneratedRegex(@"^failed to set lock proxy file$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex FailedToSetLockProxyFile();

    [GeneratedRegex(@"^FOREIGN KEY constraint failed$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ForeignKeyConstraintFailed();

    [GeneratedRegex(@"^PRIMARY KEY missing on table (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex PRIMARYKEYMissingOnTable();

    [GeneratedRegex(@"^use DROP VIEW to delete view (?<View>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UseDROPVIEWToDeleteView();

    [GeneratedRegex(@"^access to view ""(?<View>.+?)"" prohibited$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AccessToViewProhibited();

    [GeneratedRegex(@"^cannot override (?<Override>.+?) of window: (?<Window>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotOverrideOfWindow();

    [GeneratedRegex(@"^error in generated column ""(?<Column>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ErrorInGeneratedColumn();

    [GeneratedRegex(@"^internal query planner error$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex InternalQueryPlannerError();

    [GeneratedRegex(@"^misuse of aliased aggregate (?<Aggregate>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MisuseOfAliasedAggregate();

    [GeneratedRegex(@"^no such collation sequence: (?<Sequence>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchCollationSequence();

    [GeneratedRegex(@"^NOT NULL constraint failed: (?<Table>[^.,]+)\.(?<Column>.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NotNullConstraintFailed();

    [GeneratedRegex(@"^syntax error near ""ORDER BY""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex SyntaxErrorNearORDERBY();

    [GeneratedRegex(@"^too many terms in (?<Clause>.+?) BY clause$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyTermsInBYClause();

    [GeneratedRegex(@"^cannot open virtual table: (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotOpenVirtualTable();

    [GeneratedRegex(@"^duplicate WITH table name: (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex DuplicateWITHTableName();

    [GeneratedRegex(@"^generated column loop on ""(?<Column>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex GeneratedColumnLoopOn();

    [GeneratedRegex(@"^table (?<Table>.+?) has no column named (?<Column>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableHasNoColumnNamed();

    [GeneratedRegex(@"^view (?<View>.+?) is circularly defined$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ViewIsCircularlyDefined();

    [GeneratedRegex(@"^Cannot add a UNIQUE column$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotAddAUNIQUEColumn();

    [GeneratedRegex(@"^more than (?<Max>-?\d+) aggregate terms$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MoreThanAggregateTerms();

    [GeneratedRegex(@"^ORDER BY without LIMIT on (?<Statement>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ORDERBYWithoutLIMITOn();

    [GeneratedRegex(@"^table (?<Table>.+?) may not be modified$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableMayNotBeModified();

    [GeneratedRegex(@"^UNIQUE constraint failed: (?<Table>[^.,]+)\.(?<Columns>.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UniqueConstraintFailed();

    [GeneratedRegex(@"^unknown datatype for (?<Table>.+?)\.(?<Column>.+?): ""(?<DataType>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnknownDatatypeFor();

    [GeneratedRegex(@"^at most (?<Max>-?\d+) tables in a join$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AtMostTablesInAJoin();

    [GeneratedRegex(@"^CHECK constraint failed: (?<Constraint>.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CheckConstraintFailed();

    [GeneratedRegex(@"^(?<ColumnCount>-?\d+) columns assigned (?<ValueCount>-?\d+) values$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ColumnsAssignedValues();

    [GeneratedRegex(@"^syntax error near ""LIMIT""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex SyntaxErrorNearLIMIT();

    [GeneratedRegex(@"^table (?<Table>.+?) may not be altered$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableMayNotBeAltered();

    [GeneratedRegex(@"^table (?<Table>.+?) may not be dropped$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableMayNotBeDropped();

    [GeneratedRegex(@"^table (?<Table>.+?) may not be indexed$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TableMayNotBeIndexed();

    [GeneratedRegex(@"^unsupported use of NULLS (?<Keyword>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnsupportedUseOfNULLS();

    [GeneratedRegex(@"^access to (?<Name>.+?) is prohibited$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AccessToIsProhibited();

    [GeneratedRegex(@"^not a writable directory$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NotAWritableDirectory();

    [GeneratedRegex(@"^triggers nested too deep$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TriggersNestedTooDeep();

    [GeneratedRegex(@"^view (?<View>.+?) may not be altered$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ViewMayNotBeAltered();

    [GeneratedRegex(@"^views may not be indexed$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ViewsMayNotBeIndexed();

    [GeneratedRegex(@"^cannot drop (?<Kind>.+?) column: ""(?<Column>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotDropColumn();

    [GeneratedRegex(@"^duplicate column name: (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex DuplicateColumnName();

    [GeneratedRegex(@"^misuse of aggregate: (?<Aggregate>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MisuseOfAggregate();

    [GeneratedRegex(@"^trigger (?<Trigger>.+?) already exists$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TriggerAlreadyExists();

    [GeneratedRegex(@"^(?<Table>.+?)\.xBestIndex malfunction$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex XBestIndexMalfunction();

    [GeneratedRegex(@"^authorizer malfunction$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AuthorizerMalfunction();

    [GeneratedRegex(@"^missing datatype for (?<Table>.+?)\.(?<Column>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MissingDatatypeFor();

    [GeneratedRegex(@"^misuse of (?<Kind>.+?) function (?<Function>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex MisuseOfFunction();

    [GeneratedRegex(@"^too many SQL variables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManySQLVariables();

    [GeneratedRegex(@"^unknown table option: (?<Option>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnknownTableOption();

    [GeneratedRegex(@"^unrecognized token: ""(?<Token>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnrecognizedToken();

    [GeneratedRegex(@"^unsupported encoding: (?<Encoding>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnsupportedEncoding();

    [GeneratedRegex(@"^cannot (?<Action>.+?) rowid alias: (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotRowidAlias();

    [GeneratedRegex(@"^(?<Statement>.+?) cannot use variables$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotUseVariables();

    [GeneratedRegex(@"^hex literal too big: (?<Sign>.+?)(?<Literal>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex HexLiteralTooBig();

    [GeneratedRegex(@"^index (?<Index>.+?) already exists$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex IndexAlreadyExists();

    [GeneratedRegex(@"^near ""(?<Near>.+?)"": syntax error$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NearSyntaxError();

    [GeneratedRegex(@"^cannot UPSERT a view$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotUPSERTAView();

    [GeneratedRegex(@"^'(?<Name>.+?)' is not a function$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex IsNotAFunction();

    [GeneratedRegex(@"^too many columns in (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyColumnsIn();

    [GeneratedRegex(@"^too many columns on (?<Name>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyColumnsOn();

    [GeneratedRegex(@"^too many RIGHT JOINs$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyRIGHTJOINs();

    [GeneratedRegex(@"^unknown function: (?<Function>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnknownFunction();

    [GeneratedRegex(@"^(?<ValueCount>-?\d+) values for (?<ColumnCount>-?\d+) columns$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ValuesForColumns();

    [GeneratedRegex(@"^no tables specified$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoTablesSpecified();

    [GeneratedRegex(@"^oversized integer: (?<Sign>.+?)(?<Integer>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex OversizedInteger();

    [GeneratedRegex(@"^unknown join type: (?<First>.+?)(?<FirstSeparator>[ ]?)(?<Second>.*?)(?<SecondSeparator>[ ]?)(?<Third>.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnknownJoinType();

    [GeneratedRegex(@"^cannot open view: (?<View>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotOpenView();

    [GeneratedRegex(@"^no such function: (?<Function>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchFunction();

    [GeneratedRegex(@"^no query solution$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoQuerySolution();

    [GeneratedRegex(@"^(?:no such index: ""(?<Index>.+?)""|no such index: (?<Index>.+?))$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchIndex();

    [GeneratedRegex(@"^no such trigger: (?<Trigger>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchTrigger();

    [GeneratedRegex(@"^row value misused$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex RowValueMisused();

    [GeneratedRegex(@"^unknown database (?<Database>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnknownDatabase();

    [GeneratedRegex(@"^(?<ObjectType>table|view) (?<Name>.+?) already exists$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex AlreadyExists();

    [GeneratedRegex(@"^corrupt database$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CorruptDatabase();

    [GeneratedRegex(@"^incomplete input$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex IncompleteInput();

    [GeneratedRegex(@"^invalid rootpage$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex InvalidRootpage();

    [GeneratedRegex(@"^no such module: (?<Module>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchModule();

    [GeneratedRegex(@"^no such window: (?<Window>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchWindow();

    [GeneratedRegex(@"^unsafe use of (?<Function>.+?)\(\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex UnsafeUseOf();

    [GeneratedRegex(@"^(?<Construct>.+?) prohibited in (?<Context>partial index WHERE clauses|index expressions|CHECK constraints|generated columns)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex ConstructProhibitedInContext();

    [GeneratedRegex(@"^no such table: (?<Table>.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NoSuchTable();

    [GeneratedRegex(@"^not authorized$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex NotAuthorized();

    [GeneratedRegex(@"^out of memory$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex OutOfMemory();

    [GeneratedRegex(@"^too many CTEs$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex TooManyCTEs();

    [GeneratedRegex(@"^cannot (?<Action>rename columns of|drop column from|edit constraints of) (?<ObjectType>view|virtual table) ""(?<Name>.+?)""$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    internal static partial Regex CannotAlterObject();
}
