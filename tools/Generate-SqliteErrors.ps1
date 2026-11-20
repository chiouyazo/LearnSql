param(
    [string] $SqliteRepoPath   = (Join-Path $PSScriptRoot '../../sqlite'),
    [string] $SqliteVersionTag = 'release-393-g09894b5cd2',
    [string] $OutputPath       = (Join-Path $PSScriptRoot '../src/LearnSql.Sql/Generated'),
    [string] $Namespace        = 'LearnSql.Sql.Generated',
    [string] $ModelsNamespace  = 'LearnSql.Sql.Models'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $SqliteRepoPath -PathType Container)) {
    throw "SQLite source directory not found: $SqliteRepoPath"
}
$SqliteRepoPath = (Resolve-Path -LiteralPath $SqliteRepoPath).Path

if ($SqliteVersionTag) {
    $actualVersionTag = (& git -C $SqliteRepoPath describe --tags 2>$null)
    if ($LASTEXITCODE -ne 0 -or $actualVersionTag -ne $SqliteVersionTag) {
        throw "SQLite source is at '$actualVersionTag' but '$SqliteVersionTag' was requested."
    }
}

$generatedAttribute = '[System.CodeDom.Compiler.GeneratedCode("Generate-SqliteErrors", "1.0")]'

$callPattern = '(?m)^[ \t]*sqlite3ErrorMsg\(\s*[^,"]+,\s*(?<literals>"(?:[^"\\]|\\.)*"(?:\s*"(?:[^"\\]|\\.)*")*)'
$literalPattern = '"((?:[^"\\]|\\.)*)"'
$placeholderPattern = '%%|%[-#!0-9.*l]*[a-zA-Z]'
$wrapperPattern = '^SQLite Error (?<Code>\d+): ''(?<Message>.*)''\.$'

$stopWords = @(
    'a', 'an', 'and', 'are', 'after', 'as', 'at', 'before', 'by', 'for', 'from', 'has', 'in', 'is',
    'max', 'most', 'near', 'not', 'of', 'on', 'only', 'than', 'the', 'to', 'use', 'with'
)

$excludedMessages = @(
    '%s: "%s" - should this be a string literal in single-quotes?'
)

$nameOverrides = @{
    'cannot %s %s "%s"'                      = 'CannotAlterObject'
    '%s prohibited in %s'                    = 'ConstructProhibitedInContext'
    '%d values for %d columns'               = 'ValuesForColumns'
    'there is already a table named %s'      = 'TableAlreadyNamed'
    'there is already an index named %s'     = 'IndexAlreadyNamed'
}

$argumentNames = @{
    'ambiguous column name: %s'                                           = @('Column')
    'ambiguous column name: "%s" - should this be a string literal in single-quotes?' = @('Column')
    'variable number must be between ?1 and ?%d'                          = @('Max')
    '%sON CONFLICT clause does not match any PRIMARY KEY or UNIQUE constraint' = @('Prefix')
    '%s clause should come after %s not before'                           = @('Clause', 'After')
    'IN(...) element has %d term%s - expected %d'                         = @('TermCount', 'Plural', 'Expected')
    'too many references to "%s": max 65535'                              = @('Name')
    'cannot modify %s because it is a view'                               = @('Name')
    'ambiguous reference to %s in USING()'                                = @('Name')
    '%#T() is not an ordered-set aggregate'                               = @('Aggregate')
    '%r ORDER BY term does not match any column in the result set'        = @('Position')
    'cannot open table without rowid: %s'                                 = @('Table')
    'expected %d columns for ''%s'' but got %d'                           = @('Expected', 'Name', 'Got')
    'SELECTs to the left and right of %s do not have the same number of result columns' = @('Operator')
    'syntax error after column name "%.*s"'                               = @('Name')
    'too many FROM clause terms, max: %d'                                 = @('Max')
    'cannot create %s trigger on view: %S'                                = @('Timing', 'View')
    'table %s has %d values for %d columns'                               = @('Table', 'ValueCount', 'ColumnCount')
    'table %S has %d columns but %d values were supplied'                 = @('Table', 'ColumnCount', 'ValueCount')
    'table %S has no column named %s'                                     = @('Table', 'Column')
    'too many arguments on %s() - max %d'                                 = @('Function', 'Max')
    '%d values for %d columns'                                            = @('ValueCount', 'ColumnCount')
    'more than %d aggregate terms'                                        = @('Max')
    'ORDER BY without LIMIT on %s'                                        = @('Statement')
    '%d columns assigned %d values'                                       = @('ColumnCount', 'ValueCount')
    'at most %d tables in a join'                                         = @('Max')
    'unsupported use of NULLS %s'                                         = @('Keyword')
    'access to %z is prohibited'                                          = @('Name')
    'cannot drop %s column: "%s"'                                         = @('Kind', 'Column')
    '%s.xBestIndex malfunction'                                           = @('Table')
    'missing datatype for %s.%s'                                          = @('Table', 'Column')
    'misuse of %s function %#T()'                                         = @('Kind', 'Function')
    'cannot %s rowid alias: %s'                                           = @('Action', 'Name')
    '%s cannot use variables'                                             = @('Statement')
    '''%s'' is not a function'                                            = @('Name')
    'too many columns in %s'                                              = @('Name')
    'too many columns on %s'                                              = @('Name')
    'unsafe use of %#T()'                                                 = @('Function')
    'foreign key on %s should reference only one column of table %T'      = @('Column', 'Table')
    '%s prohibited in %s'                                                 = @('Construct', 'Context')
    'cannot %s %s "%s"'                                                   = @('Action', 'ObjectType', 'Name')
    '%s %T already exists'                                                = @('ObjectType', 'Name')
    'cannot open table with generated columns: %s'                        = @('Table')
    'object name reserved for internal use: %s'                           = @('Name')
    'too many terms in %s BY clause'                                      = @('Clause')
    'generated column loop on "%s"'                                       = @('Column')
    'hex literal too big: %s%#T'                                          = @('Sign', 'Literal')
    'oversized integer: %s%#T'                                            = @('Sign', 'Integer')
    'cannot join using column %s - column not present in both tables'     = @('Column')
    '%s %T cannot reference objects in database %s'                       = @('ObjectType', 'Name', 'Database')
    'unknown datatype for %s.%s: "%s"'                                    = @('Table', 'Column', 'DataType')
    'a JOIN clause is required before %s'                                 = @('Before')
    'there is already a table named %s'                                   = @('Table')
    'there is already an index named %s'                                  = @('Index')
    'near "%T": syntax error'                                             = @('Near')
    'second argument to %#T() must be a constant between 0.0 and 1.0'     = @('Function')
    '%#T() may not be used as a window function'                          = @('Function')
    '%r %s BY term out of range - should be between 1 and %d'             = @('Position', 'Clause', 'Max')
    'unknown join type: %T%s%T%s%T'                                       = @('First', 'FirstSeparator', 'Second', 'SecondSeparator', 'Third')
    '%s references tables to its right'                                   = @('Context')
    '%s RETURNING is not available on virtual tables'                     = @('Statement')
}

$argumentPatterns = @{
    'unknown join type: %T%s%T%s%T' = @{
        First           = @{ Pattern = '.+?'; Sample = 'A' }
        FirstSeparator  = @{ Pattern = '[ ]?'; Sample = ' ' }
        Second          = @{ Pattern = '.*?'; Sample = 'B' }
        SecondSeparator = @{ Pattern = '[ ]?'; Sample = ' ' }
        Third           = @{ Pattern = '.*?'; Sample = 'C' }
    }
    '%s %T already exists'  = @{ ObjectType = @{ Pattern = 'table|view'; Sample = 'table' } }
    '%s prohibited in %s'   = @{
        Context = @{
            Pattern = 'partial index WHERE clauses|index expressions|CHECK constraints|generated columns'
            Sample  = 'CHECK constraints'
        }
    }
    'cannot %s %s "%s"'     = @{
        Action     = @{ Pattern = 'rename columns of|drop column from|edit constraints of'; Sample = 'drop column from' }
        ObjectType = @{ Pattern = 'view|virtual table'; Sample = 'view' }
    }
}

$manualMessages = @(
    [PSCustomObject]@{
        Name      = 'NoSuchColumn'
        Message   = 'no such column: %s'
        Pattern   = $null
        Sample    = $null
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'NoSuchColumn'
        Message   = 'no such column: "%s" - should this be a string literal in single-quotes?'
        Pattern   = $null
        Sample    = $null
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'AmbiguousColumnName'
        Message   = 'ambiguous column name: %s'
        Pattern   = $null
        Sample    = $null
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'AmbiguousColumnName'
        Message   = 'ambiguous column name: "%s" - should this be a string literal in single-quotes?'
        Pattern   = $null
        Sample    = $null
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'UniqueConstraintFailed'
        Message   = 'UNIQUE constraint failed: %s'
        Pattern   = '^UNIQUE constraint failed: (?<Table>[^.,]+)\.(?<Columns>.+)$'
        Sample    = 'UNIQUE constraint failed: Customer.Email'
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'PrimaryKeyConstraintFailed'
        Message   = 'PRIMARY KEY constraint failed: %s'
        Pattern   = '^PRIMARY KEY constraint failed: (?<Table>[^.,]+)\.(?<Columns>.+)$'
        Sample    = $null
        Matchable = $false
    }
    [PSCustomObject]@{
        Name      = 'NotNullConstraintFailed'
        Message   = 'NOT NULL constraint failed: %s'
        Pattern   = '^NOT NULL constraint failed: (?<Table>[^.,]+)\.(?<Column>.+)$'
        Sample    = 'NOT NULL constraint failed: Customer.Name'
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'CheckConstraintFailed'
        Message   = 'CHECK constraint failed: %s'
        Pattern   = '^CHECK constraint failed: (?<Constraint>.+)$'
        Sample    = 'CHECK constraint failed: Age>0'
        Matchable = $true
    }
    [PSCustomObject]@{
        Name      = 'ForeignKeyConstraintFailed'
        Message   = 'FOREIGN KEY constraint failed'
        Pattern   = '^FOREIGN KEY constraint failed$'
        Sample    = 'FOREIGN KEY constraint failed'
        Matchable = $true
    }
)

function ConvertTo-PascalCase([string] $text) {
    $words = $text -split '[^A-Za-z0-9]+' | Where-Object { $_ }
    return ($words | ForEach-Object { $_.Substring(0, 1).ToUpper() + $_.Substring(1) }) -join ''
}

function ConvertTo-RegexLiteral([string] $text) {
    return [regex]::Escape($text).Replace('\ ', ' ')
}

function Get-DefaultArgumentPattern([string] $specifier, [string] $before, [string] $after) {
    $kind = $specifier.Substring($specifier.Length - 1)
    if ('d', 'i', 'u' -contains $kind) { return '-?\d+' }
    if ('x', 'X' -contains $kind) { return '[0-9a-fA-F]+' }
    if ($kind -eq 'r') { return '\d+(?:st|nd|rd|th)' }
    if ($kind -eq 'c') { return '.' }
    $adjacent = ($before -match '[A-Za-z0-9]$') -or ($after -match '^[A-Za-z0-9]')
    if ($adjacent) { return '.*?' }
    return '.+?'
}

function Get-DefaultArgumentSample([string] $specifier) {
    $kind = $specifier.Substring($specifier.Length - 1)
    if ('d', 'i', 'u', 'x', 'X' -contains $kind) { return '1' }
    if ($kind -eq 'r') { return '1st' }
    return 'Sample'
}

function New-MessagePattern([string] $message) {
    $placeholders = [regex]::Matches($message, $placeholderPattern) | Where-Object { $_.Value -ne '%%' }
    $names = $null
    if ($argumentNames.ContainsKey($message)) {
        $names = $argumentNames[$message]
        if ($names.Count -ne @($placeholders).Count) {
            throw "Message '$message' has $(@($placeholders).Count) placeholders but $($names.Count) argument names."
        }
    }
    $patterns = $argumentPatterns[$message]

    $body      = [System.Text.StringBuilder]::new()
    $sample    = [System.Text.StringBuilder]::new()
    $usedNames = [System.Collections.Generic.List[string]]::new()
    $position  = 0
    $argIndex  = 0

    foreach ($placeholder in [regex]::Matches($message, $placeholderPattern)) {
        $literal = $message.Substring($position, $placeholder.Index - $position)
        [void] $body.Append((ConvertTo-RegexLiteral $literal))
        [void] $sample.Append($literal)
        $position = $placeholder.Index + $placeholder.Length

        if ($placeholder.Value -eq '%%') {
            [void] $body.Append('%')
            [void] $sample.Append('%')
            continue
        }

        if ($names) {
            $groupName = $names[$argIndex]
        }
        else {
            $wordBefore = [regex]::Match($literal, '([A-Za-z]+)[^A-Za-z]*$')
            if (-not $wordBefore.Success -or $stopWords -contains $wordBefore.Groups[1].Value.ToLowerInvariant()) {
                $script:problems.Add("No argument name derivable for placeholder $($argIndex + 1) of '$message'. Add an entry to argumentNames.")
                $groupName = "Unresolved$($argIndex + 1)"
            }
            else {
                $groupName = ConvertTo-PascalCase $wordBefore.Groups[1].Value
            }
        }
        $argIndex++

        if ($usedNames -contains $groupName) {
            $script:problems.Add("Duplicate argument name '$groupName' in '$message'.")
            $groupName = "$groupName$argIndex"
        }
        $usedNames.Add($groupName)

        $after = $message.Substring($position)
        $argumentPattern = Get-DefaultArgumentPattern $placeholder.Value $literal $after
        $argumentSample = Get-DefaultArgumentSample $placeholder.Value
        if ($patterns -and $patterns.ContainsKey($groupName)) {
            $argumentPattern = $patterns[$groupName].Pattern
            $argumentSample = $patterns[$groupName].Sample
        }

        [void] $body.Append("(?<$groupName>$argumentPattern)")
        [void] $sample.Append($argumentSample)
    }

    [void] $body.Append((ConvertTo-RegexLiteral $message.Substring($position)))
    [void] $sample.Append($message.Substring($position))

    return [PSCustomObject]@{
        Body  = $body.ToString()
        Names = $usedNames.ToArray()
        Sample = $sample.ToString()
    }
}

function Get-ArgumentNames([string] $pattern) {
    return @([regex]::Matches($pattern, '\(\?<([A-Za-z][A-Za-z0-9]*)>') | ForEach-Object { $_.Groups[1].Value })
}

$sourceFiles = [string[]] (Get-ChildItem -LiteralPath $SqliteRepoPath -File -Recurse -Include *.c, *.h, *.y | ForEach-Object { $_.FullName })
[System.Array]::Sort($sourceFiles, [System.StringComparer]::Ordinal)

$sourceMessages = [System.Collections.Generic.List[string]]::new()
foreach ($file in $sourceFiles) {
    $text = [System.IO.File]::ReadAllText($file)
    foreach ($call in [regex]::Matches($text, $callPattern)) {
        $joined = ([regex]::Matches($call.Groups['literals'].Value, $literalPattern) | ForEach-Object { $_.Groups[1].Value }) -join ''
        $sourceMessages.Add(($joined -replace '\\(.)', '$1'))
    }
}

$problems = [System.Collections.Generic.List[string]]::new()
$entries = [System.Collections.Generic.List[object]]::new()
$seenMessages = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

foreach ($manual in $manualMessages) {
    [void] $seenMessages.Add($manual.Message)
    $entries.Add($manual)
}

foreach ($message in $sourceMessages) {
    if ($excludedMessages -contains $message) { continue }
    if (-not $seenMessages.Add($message)) { continue }

    $baseName = ConvertTo-PascalCase ($message -replace $placeholderPattern, ' ')
    if ($nameOverrides.ContainsKey($message)) { $baseName = $nameOverrides[$message] }
    if ($baseName.Length -lt 3 -or $baseName -match '^\d') { continue }

    $entries.Add([PSCustomObject]@{
        Name      = $baseName
        Message   = $message
        Pattern   = $null
        Sample    = $null
        Matchable = $true
    })
}

$variants = foreach ($entry in $entries) {
    if ($entry.Pattern) {
        $body = $entry.Pattern.Substring(1, $entry.Pattern.Length - 2)
        $names = Get-ArgumentNames $entry.Pattern
        $sample = $entry.Sample
    }
    else {
        $built = New-MessagePattern $entry.Message
        $body = $built.Body
        $names = $built.Names
        $sample = $built.Sample
    }
    [PSCustomObject]@{
        Name          = $entry.Name
        Message       = $entry.Message
        Body          = $body
        Names         = @($names)
        Sample        = $sample
        Matchable     = $entry.Matchable
        LiteralLength = ($entry.Message -replace $placeholderPattern, '').Length
    }
}

if ($problems.Count -gt 0) {
    foreach ($problem in $problems) {
        [System.Console]::Error.WriteLine($problem)
    }
    throw "Unresolved messages: $($problems.Count)"
}

$errors = [System.Collections.Generic.List[object]]::new()
foreach ($group in ($variants | Group-Object -Property Name)) {
    $ordered = @($group.Group | Sort-Object -Property @{ Expression = 'LiteralLength'; Descending = $true }, Message)
    $reference = ($ordered[0].Names | Sort-Object) -join ','
    foreach ($variant in $ordered) {
        if ((($variant.Names | Sort-Object) -join ',') -ne $reference) {
            throw "Variants of '$($group.Name)' disagree on argument names: '$($variant.Message)'."
        }
    }
    if ($group.Name -eq 'Unknown') { throw 'A message maps to the reserved name Unknown.' }

    $bodies = @($ordered | ForEach-Object { $_.Body } | Select-Object -Unique)
    $pattern = if ($bodies.Count -eq 1) { "^$($bodies[0])$" } else { "^(?:$($bodies -join '|'))$" }

    $errors.Add([PSCustomObject]@{
        Name          = $group.Name
        Pattern       = $pattern
        Names         = @($ordered[0].Names)
        Samples       = @($ordered | Where-Object { $_.Sample } | ForEach-Object { $_.Sample })
        Matchable     = $ordered[0].Matchable
        LiteralLength = ($ordered | Measure-Object -Property LiteralLength -Maximum).Maximum
    })
}

$errors.Sort([System.Comparison[object]] {
    param($left, $right)
    [System.StringComparer]::Ordinal.Compare($left.Name, $right.Name)
})
$enumOrder = @($errors)

$ruleOrder = @($errors | Where-Object { $_.Matchable })
$ruleOrder = [System.Collections.Generic.List[object]]::new($ruleOrder)
$ruleOrder.Sort([System.Comparison[object]] {
    param($left, $right)
    if ($left.LiteralLength -ne $right.LiteralLength) { return $right.LiteralLength.CompareTo($left.LiteralLength) }
    return [System.StringComparer]::Ordinal.Compare($left.Name, $right.Name)
})

$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
$compiled = @{}
foreach ($rule in $ruleOrder) {
    $compiled[$rule.Name] = [System.Text.RegularExpressions.Regex]::new($rule.Pattern, $regexOptions)
}

$conflicts = [System.Collections.Generic.List[string]]::new()
foreach ($candidate in $ruleOrder) {
    foreach ($sample in $candidate.Samples) {
        $winner = $null
        foreach ($rule in $ruleOrder) {
            if ($compiled[$rule.Name].IsMatch($sample)) { $winner = $rule.Name; break }
        }
        if ($winner -ne $candidate.Name) {
            $conflicts.Add("Sample '$sample' of $($candidate.Name) is matched by '$winner'.")
        }
    }
}
if ($conflicts.Count -gt 0) {
    throw ("Pattern conflicts:`n" + ($conflicts -join "`n"))
}

$enumMembers = foreach ($e in $enumOrder) { "    $($e.Name)," }

$regexMethods = foreach ($e in $ruleOrder) {
    $verbatim = $e.Pattern.Replace('"', '""')
    "    [GeneratedRegex(@`"$verbatim`", RegexOptions.IgnoreCase | RegexOptions.Singleline)]`n" +
    "    internal static partial Regex $($e.Name)();"
}

$rules = foreach ($e in $ruleOrder) {
    "        (SqliteErrorType.$($e.Name), SqliteErrorMessages.$($e.Name)()),"
}

$argumentCases = foreach ($e in $enumOrder) {
    if ($e.Names.Count -gt 0) {
        $items = ($e.Names | ForEach-Object { "`"$_`"" }) -join ', '
        "        SqliteErrorType.$($e.Name) => [$items],"
    }
}

$wrapperVerbatim = $wrapperPattern.Replace('"', '""')

$enumFile = @'
namespace __NAMESPACE__;

__ATTRIBUTE__
public enum SqliteErrorType
{
    Unknown,
__MEMBERS__
}
'@

$messagesFile = @'
using System.Text.RegularExpressions;

namespace __NAMESPACE__;

__ATTRIBUTE__
internal static partial class SqliteErrorMessages
{
    [GeneratedRegex(@"__WRAPPER__", RegexOptions.Singleline)]
    internal static partial Regex Wrapper();

__REGEXES__
}
'@

$mapperFile = @'
using System.Text.RegularExpressions;
using __MODELS__;

namespace __NAMESPACE__;

__ATTRIBUTE__
public static class SqliteErrorMapper
{
    private static readonly (SqliteErrorType Type, Regex Regex)[] Rules =
    [
__RULES__
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
'@

$argumentsFile = @'
namespace __NAMESPACE__;

__ATTRIBUTE__
public static class SqliteErrorArguments
{
    public static IReadOnlyList<string> GetNames(SqliteErrorType type) => type switch
    {
__CASES__
        _ => [],
    };
}
'@

function Expand-Template([string] $template, [hashtable] $values) {
    $result = $template
    foreach ($key in $values.Keys) {
        $result = $result.Replace("__${key}__", $values[$key])
    }
    return $result.Replace("`r`n", "`n").TrimEnd() + "`n"
}

$common = @{ NAMESPACE = $Namespace; MODELS = $ModelsNamespace; ATTRIBUTE = $generatedAttribute }

$files = [ordered]@{
    'SqliteErrorType.cs'      = Expand-Template $enumFile ($common + @{ MEMBERS = ($enumMembers -join "`n") })
    'SqliteErrorMessages.cs'  = Expand-Template $messagesFile ($common + @{ WRAPPER = $wrapperVerbatim; REGEXES = ($regexMethods -join "`n`n") })
    'SqliteErrorMapper.cs'    = Expand-Template $mapperFile ($common + @{ RULES = ($rules -join "`n") })
    'SqliteErrorArguments.cs' = Expand-Template $argumentsFile ($common + @{ CASES = ($argumentCases -join "`n") })
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$encoding = [System.Text.UTF8Encoding]::new($false)
foreach ($name in $files.Keys) {
    [System.IO.File]::WriteAllText((Join-Path $OutputPath $name), $files[$name], $encoding)
}

Write-Host "Generated $($enumOrder.Count) SQLite error types ($($ruleOrder.Count) matchable) into $OutputPath"
