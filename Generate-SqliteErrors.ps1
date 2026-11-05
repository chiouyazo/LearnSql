param(
    [string] $SqliteRepoPath = "/home/chio/sqlite/",
    [string] $OutputPath     = "./src/LearnSql.Sql/Generated",
    [string] $Namespace      = "LearnSql.Sql.Generated"
)

$ErrorActionPreference = 'Stop'

# Finds the message string (2nd argument) of every sqlite3ErrorMsg(...) call.
$sourcePattern = '(?<=^\s*sqlite3ErrorMsg\(\s*[^,"]+,\s*")(?:[^"\\]|\\.)*(?=")'

# printf-style placeholders incl. SQLite-specific ones (%T, %S, %z, %q, %Q, %w, %lld, %.*s, ...).
$placeholderPattern = '%%|%[-#!0-9.*l]*[a-zA-Z]'

$wrapperPattern = '^SQLite Error \d+: ''(.*)''\.$'

$indent = '    '

function ConvertTo-PascalCase([string] $text) {
    $words = $text -split '[^A-Za-z0-9]+' | Where-Object { $_ }
    return ($words | ForEach-Object { $_.Substring(0, 1).ToUpper() + $_.Substring(1) }) -join ''
}

function ConvertTo-RegexLiteral([string] $text) {
    return [regex]::Escape($text).Replace('\ ', ' ')
}

function ConvertTo-CSharpComment([string] $text) {
    return $text.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;')
}

function New-MessagePattern([string] $message) {
    $pattern   = [System.Text.StringBuilder]::new('^')
    $template  = [System.Text.StringBuilder]::new()
    $usedNames = [System.Collections.Generic.HashSet[string]]::new()
    $position  = 0
    $argIndex  = 0

    foreach ($placeholder in [regex]::Matches($message, $placeholderPattern)) {
        $literal = $message.Substring($position, $placeholder.Index - $position)
        [void] $pattern.Append((ConvertTo-RegexLiteral $literal))
        [void] $template.Append($literal)
        $position = $placeholder.Index + $placeholder.Length

        if ($placeholder.Value -eq '%%') {
            [void] $pattern.Append('%')
            [void] $template.Append('%')
            continue
        }

        $argIndex++
        $groupName = "Arg$argIndex"
        $wordBefore = [regex]::Match($literal, '([A-Za-z]+)[^A-Za-z]*$')
        if ($wordBefore.Success) {
            $groupName = ConvertTo-PascalCase $wordBefore.Groups[1].Value
        }
        if (-not $usedNames.Add($groupName)) {
            $groupName = "$groupName$argIndex"
            [void] $usedNames.Add($groupName)
        }

        [void] $pattern.Append("(?<$groupName>.+?)")
        [void] $template.Append("{$groupName}")
    }

    [void] $pattern.Append((ConvertTo-RegexLiteral $message.Substring($position)))
    [void] $pattern.Append('$')
    [void] $template.Append($message.Substring($position))

    return [PSCustomObject]@{
        Pattern  = $pattern.ToString()
        Template = $template.ToString()
    }
}

$manualMessages = @(
    [PSCustomObject]@{ Message = 'no such column: %s';        Source = 'src/resolve.c (composed)' }
    [PSCustomObject]@{ Message = 'ambiguous column name: %s'; Source = 'src/resolve.c (composed)' }
)

$sourceMessages = Get-ChildItem $SqliteRepoPath -File -Recurse -Include *.c, *.h, *.y |
    Select-String -CaseSensitive -Pattern $sourcePattern |
    ForEach-Object {
        [PSCustomObject]@{
            Message = $_.Matches[0].Value -replace '\\(.)', '$1'
            Source  = [System.IO.Path]::GetRelativePath($SqliteRepoPath, $_.Path) + ":" + $_.LineNumber
        }
    }

$errors       = [System.Collections.Generic.List[object]]::new()
$seenNames    = [System.Collections.Generic.HashSet[string]]::new()
$seenMessages = [System.Collections.Generic.HashSet[string]]::new()

foreach ($entry in @($manualMessages) + @($sourceMessages)) {
    if (-not $seenMessages.Add($entry.Message)) { continue }

    $baseName = ConvertTo-PascalCase ($entry.Message -replace $placeholderPattern, ' ')

    if ($baseName.Length -lt 3 -or $baseName -match '^\d') { continue }

    $name = $baseName
    $suffix = 2
    while (-not $seenNames.Add($name)) { $name = "$baseName$suffix"; $suffix++ }

    $built = New-MessagePattern $entry.Message

    $errors.Add([PSCustomObject]@{
        Name          = $name
        Message       = $entry.Message
        Pattern       = $built.Pattern
        Template      = $built.Template
        Source        = $entry.Source
        LiteralLength = ($entry.Message -replace $placeholderPattern, '').Length
    })
}

$errors = $errors | Sort-Object -Property @{ Expression = 'LiteralLength'; Descending = $true }, Name

$enumMembers = foreach ($e in $errors) {
    "$indent/// <summary>$(ConvertTo-CSharpComment $e.Message) ($($e.Source))</summary>`n$indent$($e.Name),"
}

$regexMethods = foreach ($e in $errors) {
    $verbatim = $e.Pattern.Replace('"', '""')
    "$indent/// <summary>$(ConvertTo-CSharpComment $e.Message) ($($e.Source))</summary>`n" +
    "$indent[GeneratedRegex(@`"$verbatim`", RegexOptions.IgnoreCase | RegexOptions.Singleline)]`n" +
    "${indent}internal static partial Regex $($e.Name)();"
}

$rules = foreach ($e in $errors) {
    "$indent$indent(SqliteErrorType.$($e.Name), SqliteErrorMessages.$($e.Name)()),"
}

$wrapperVerbatim = $wrapperPattern.Replace('"', '""')

$templateMethods = foreach ($e in $errors) {
    $verbatim = $e.Template.Replace('"', '""')
    "$indent/// <summary>$(ConvertTo-CSharpComment $e.Message) ($($e.Source))</summary>`n" +
    "${indent}public virtual string $($e.Name)() => @`"$verbatim`";"
}

$templateCases = foreach ($e in $errors) {
    "${indent}${indent}SqliteErrorType.$($e.Name) => $($e.Name)(),"
}

$enumFile = @"
// <auto-generated />
namespace $Namespace;

/// <summary>Auto generated SQLite error types.</summary>
public enum SqliteErrorType
{
${indent}Unknown,

$($enumMembers -join "`n`n")
}
"@

$messagesFile = @"
// <auto-generated />
using System.Text.RegularExpressions;

namespace $Namespace;

/// <summary>Auto generated SQLite error message patterns.</summary>
internal static partial class SqliteErrorMessages
{
$indent/// <summary>Microsoft.Data.Sqlite wrapper: SQLite Error {code}: '{message}'.</summary>
$indent[GeneratedRegex(@"$wrapperVerbatim", RegexOptions.Singleline)]
${indent}internal static partial Regex Wrapper();

$($regexMethods -join "`n`n")
}
"@

$errorTypeFile = @"
// <auto-generated />
namespace $Namespace;

/// <summary>A mapped SQLite error with its named arguments (e.g. "Table", "Near").</summary>
public sealed record SqliteError(
${indent}SqliteErrorType Type,
${indent}string Message,
${indent}IReadOnlyDictionary<string, string> Arguments);
"@

$mapperFile = @"
// <auto-generated />
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace $Namespace;

/// <summary>Auto generated SQLite error message mapper.</summary>
public static class SqliteErrorMapper
{
${indent}private static readonly (SqliteErrorType Type, Regex Regex)[] Rules =
${indent}[
$($rules -join "`n")
${indent}];

${indent}/// <summary>Maps a SQLite error message (raw or wrapped by Microsoft.Data.Sqlite).</summary>
${indent}public static SqliteError Match(string message)
${indent}{
${indent}${indent}var wrapper = SqliteErrorMessages.Wrapper().Match(message);
${indent}${indent}if (wrapper.Success)
${indent}${indent}${indent}message = wrapper.Groups[1].Value;

${indent}${indent}foreach (var (type, regex) in Rules)
${indent}${indent}{
${indent}${indent}${indent}var match = regex.Match(message);
${indent}${indent}${indent}if (!match.Success)
${indent}${indent}${indent}${indent}continue;

${indent}${indent}${indent}var arguments = new Dictionary<string, string>();
${indent}${indent}${indent}foreach (var groupName in regex.GetGroupNames())
${indent}${indent}${indent}{
${indent}${indent}${indent}${indent}if (!int.TryParse(groupName, out _))
${indent}${indent}${indent}${indent}${indent}arguments[groupName] = match.Groups[groupName].Value;
${indent}${indent}${indent}}

${indent}${indent}${indent}return new SqliteError(type, message, arguments);
${indent}${indent}}

${indent}${indent}return new SqliteError(SqliteErrorType.Unknown, message, new Dictionary<string, string>());
${indent}}
}
"@

$genericFile = @"
// <auto-generated />
namespace $Namespace;

/// <summary>Auto generated message templates with the original SQLite texts as fallback.</summary>
public abstract class GenericErrorMessages
{
$($templateMethods -join "`n`n")

${indent}public string GetTemplate(SqliteErrorType type) => type switch
${indent}{
$($templateCases -join "`n")
${indent}${indent}_ => "{Message}",
${indent}};

${indent}public string Format(SqliteError error)
${indent}{
${indent}${indent}var text = GetTemplate(error.Type);
${indent}${indent}foreach (var (name, value) in error.Arguments)
${indent}${indent}${indent}text = text.Replace("{" + name + "}", value);

${indent}${indent}return text.Replace("{Message}", error.Message);
${indent}}
}
"@

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
Set-Content -Path (Join-Path $OutputPath 'SqliteErrorType.cs')     -Value $enumFile     -Encoding utf8
Set-Content -Path (Join-Path $OutputPath 'SqliteErrorMessages.cs') -Value $messagesFile -Encoding utf8
Set-Content -Path (Join-Path $OutputPath 'SqliteErrorMapper.cs')   -Value $mapperFile   -Encoding utf8
Set-Content -Path (Join-Path $OutputPath 'SqliteError.cs')   -Value $errorTypeFile   -Encoding utf8
Set-Content -Path (Join-Path $OutputPath 'GenericErrorMessages.cs') -Value $genericFile -Encoding utf8

Write-Host "Generated $($errors.Count) SQLite error types into $OutputPath"