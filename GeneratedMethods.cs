using System.Text.RegularExpressions;

/// <summary>auto generated sqlite error message patterns</summary>
public partial class SqliteErrorMessages 
{
    /// <summary>
    /// src/alter.c -> L:40
    /// </summary>
    [GeneratedRegex(@"^table\ (.+?)\ may\ not\ be\ altered$", RegexOptions.IgnoreCase)]
    private static partial Regex tablemaynotbealtered();
    /// <summary>
    /// src/alter.c -> L:176
    /// </summary>
    [GeneratedRegex(@"^view\ (.+?)\ may\ not\ be\ altered$", RegexOptions.IgnoreCase)]
    private static partial Regex viewmaynotbealtered();
    /// <summary>
    /// src/alter.c -> L:321
    /// </summary>
    [GeneratedRegex(@"^cannot\ (.+?)\ rowid\ alias:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotrowidalias();
    /// <summary>
    /// src/alter.c -> L:425
    /// </summary>
    [GeneratedRegex(@"^Cannot\ add\ a\ PRIMARY\ KEY\ column$", RegexOptions.IgnoreCase)]
    private static partial Regex CannotaddaPRIMARYKEYcolumn();
    /// <summary>
    /// src/alter.c -> L:566
    /// </summary>
    [GeneratedRegex(@"^virtual\ tables\ may\ not\ be\ altered$", RegexOptions.IgnoreCase)]
    private static partial Regex virtualtablesmaynotbealtered();
    /// <summary>
    /// src/alter.c -> L:573
    /// </summary>
    [GeneratedRegex(@"^Cannot\ add\ a\ column\ to\ a\ view$", RegexOptions.IgnoreCase)]
    private static partial Regex Cannotaddacolumntoaview();
    /// <summary>
    /// src/alter.c -> L:649
    /// </summary>
    [GeneratedRegex(@"^cannot\ (.+?)\ (.+?)\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex cannot();
    /// <summary>
    /// src/alter.c -> L:706
    /// </summary>
    [GeneratedRegex(@"^no\ such\ column:\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchcolumn();
    /// <summary>
    /// src/alter.c -> L:2385
    /// </summary>
    [GeneratedRegex(@"^cannot\ drop\ (.+?)\ column:\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotdropcolumn();
    /// <summary>
    /// src/alter.c -> L:2394
    /// </summary>
    [GeneratedRegex(@"^cannot\ drop\ column\ ""(.+?)"":\ no\ other\ columns\ exist$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotdropcolumnnoothercolumnsexist();
    /// <summary>
    /// src/attach.c -> L:487
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ cannot\ use\ variables$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotusevariables();
    /// <summary>
    /// src/auth.c -> L:91
    /// </summary>
    [GeneratedRegex(@"^authorizer\ malfunction$", RegexOptions.IgnoreCase)]
    private static partial Regex authorizermalfunction();
    /// <summary>
    /// src/auth.c -> L:119
    /// </summary>
    [GeneratedRegex(@"^access\ to\ (.+?)\ is\ prohibited$", RegexOptions.IgnoreCase)]
    private static partial Regex accesstoisprohibited();
    /// <summary>
    /// src/auth.c -> L:230
    /// </summary>
    [GeneratedRegex(@"^not\ authorized$", RegexOptions.IgnoreCase)]
    private static partial Regex notauthorized();
    /// <summary>
    /// src/build.c -> L:737
    /// </summary>
    [GeneratedRegex(@"^not\ authorized\ to\ use\ function:\ %\#T$", RegexOptions.IgnoreCase)]
    private static partial Regex notauthorizedtousefunction();
    /// <summary>
    /// src/build.c -> L:1069
    /// </summary>
    [GeneratedRegex(@"^corrupt\ database$", RegexOptions.IgnoreCase)]
    private static partial Regex corruptdatabase();
    /// <summary>
    /// src/build.c -> L:1075
    /// </summary>
    [GeneratedRegex(@"^unknown\ database\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex unknowndatabase();
    /// <summary>
    /// src/build.c -> L:1143
    /// </summary>
    [GeneratedRegex(@"^object\ name\ reserved\ for\ internal\ use:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex objectnamereservedforinternaluse();
    /// <summary>
    /// src/build.c -> L:1320
    /// </summary>
    [GeneratedRegex(@"^temporary\ table\ name\ must\ be\ unqualified$", RegexOptions.IgnoreCase)]
    private static partial Regex temporarytablenamemustbeunqualified();
    /// <summary>
    /// src/build.c -> L:1372
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ (.+?)\ already\ exists$", RegexOptions.IgnoreCase)]
    private static partial Regex alreadyexists();
    /// <summary>
    /// src/build.c -> L:1382
    /// </summary>
    [GeneratedRegex(@"^there\ is\ already\ an\ index\ named\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex thereisalreadyanindexnamed();
    /// <summary>
    /// src/build.c -> L:1531
    /// </summary>
    [GeneratedRegex(@"^cannot\ use\ RETURNING\ in\ a\ trigger$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotuseRETURNINGinatrigger();
    /// <summary>
    /// src/build.c -> L:1591
    /// </summary>
    [GeneratedRegex(@"^too\ many\ columns\ on\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanycolumnson();
    /// <summary>
    /// src/build.c -> L:1638
    /// </summary>
    [GeneratedRegex(@"^duplicate\ column\ name:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex duplicatecolumnname();
    /// <summary>
    /// src/build.c -> L:1830
    /// </summary>
    [GeneratedRegex(@"^default\ value\ of\ column\ \[(.+?)]\ is\ not\ constant$", RegexOptions.IgnoreCase)]
    private static partial Regex defaultvalueofcolumnisnotconstant();
    /// <summary>
    /// src/build.c -> L:1836
    /// </summary>
    [GeneratedRegex(@"^cannot\ use\ DEFAULT\ on\ a\ generated\ column$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotuseDEFAULTonageneratedcolumn();
    /// <summary>
    /// src/build.c -> L:1979
    /// </summary>
    [GeneratedRegex(@"^AUTOINCREMENT\ is\ only\ allowed\ on\ an\ $", RegexOptions.IgnoreCase)]
    private static partial Regex AUTOINCREMENTisonlyallowedonan();
    /// <summary>
    /// src/build.c -> L:2076
    /// </summary>
    [GeneratedRegex(@"^virtual\ tables\ cannot\ use\ computed\ columns$", RegexOptions.IgnoreCase)]
    private static partial Regex virtualtablescannotusecomputedcolumns();
    /// <summary>
    /// src/build.c -> L:2110
    /// </summary>
    [GeneratedRegex(@"^error\ in\ generated\ column\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex erroringeneratedcolumn();
    /// <summary>
    /// src/build.c -> L:2117
    /// </summary>
    [GeneratedRegex(@"^generated\ columns\ not\ supported$", RegexOptions.IgnoreCase)]
    private static partial Regex generatedcolumnsnotsupported();
    /// <summary>
    /// src/build.c -> L:2821
    /// </summary>
    [GeneratedRegex(@"^missing\ datatype\ for\ (.+?)\.(.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex missingdatatypefor();
    /// <summary>
    /// src/build.c -> L:2851
    /// </summary>
    [GeneratedRegex(@"^PRIMARY\ KEY\ missing\ on\ table\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex PRIMARYKEYmissingontable();
    /// <summary>
    /// src/build.c -> L:2901
    /// </summary>
    [GeneratedRegex(@"^must\ have\ at\ least\ one\ non-generated\ column$", RegexOptions.IgnoreCase)]
    private static partial Regex musthaveatleastonenongeneratedcolumn();
    /// <summary>
    /// src/build.c -> L:3133
    /// </summary>
    [GeneratedRegex(@"^parameters\ are\ not\ allowed\ in\ views$", RegexOptions.IgnoreCase)]
    private static partial Regex parametersarenotallowedinviews();
    /// <summary>
    /// src/build.c -> L:3256
    /// </summary>
    [GeneratedRegex(@"^view\ (.+?)\ is\ circularly\ defined$", RegexOptions.IgnoreCase)]
    private static partial Regex viewiscircularlydefined();
    /// <summary>
    /// src/build.c -> L:3689
    /// </summary>
    [GeneratedRegex(@"^table\ (.+?)\ may\ not\ be\ dropped$", RegexOptions.IgnoreCase)]
    private static partial Regex tablemaynotbedropped();
    /// <summary>
    /// src/build.c -> L:3698
    /// </summary>
    [GeneratedRegex(@"^use\ DROP\ TABLE\ to\ delete\ table\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex useDROPTABLEtodeletetable();
    /// <summary>
    /// src/build.c -> L:3702
    /// </summary>
    [GeneratedRegex(@"^use\ DROP\ VIEW\ to\ delete\ view\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex useDROPVIEWtodeleteview();
    /// <summary>
    /// src/build.c -> L:3763
    /// </summary>
    [GeneratedRegex(@"^foreign\ key\ on\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex foreignkeyon();
    /// <summary>
    /// src/build.c -> L:4042
    /// </summary>
    [GeneratedRegex(@"^unsupported\ use\ of\ NULLS\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex unsupporteduseofNULLS();
    /// <summary>
    /// src/build.c -> L:4170
    /// </summary>
    [GeneratedRegex(@"^table\ (.+?)\ may\ not\ be\ indexed$", RegexOptions.IgnoreCase)]
    private static partial Regex tablemaynotbeindexed();
    /// <summary>
    /// src/build.c -> L:4175
    /// </summary>
    [GeneratedRegex(@"^views\ may\ not\ be\ indexed$", RegexOptions.IgnoreCase)]
    private static partial Regex viewsmaynotbeindexed();
    /// <summary>
    /// src/build.c -> L:4181
    /// </summary>
    [GeneratedRegex(@"^virtual\ tables\ may\ not\ be\ indexed$", RegexOptions.IgnoreCase)]
    private static partial Regex virtualtablesmaynotbeindexed();
    /// <summary>
    /// src/build.c -> L:4211
    /// </summary>
    [GeneratedRegex(@"^there\ is\ already\ a\ table\ named\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex thereisalreadyatablenamed();
    /// <summary>
    /// src/build.c -> L:4217
    /// </summary>
    [GeneratedRegex(@"^index\ (.+?)\ already\ exists$", RegexOptions.IgnoreCase)]
    private static partial Regex indexalreadyexists();
    /// <summary>
    /// src/build.c -> L:4353
    /// </summary>
    [GeneratedRegex(@"^expressions\ prohibited\ in\ PRIMARY\ KEY\ and\ $", RegexOptions.IgnoreCase)]
    private static partial Regex expressionsprohibitedinPRIMARYKEYand();
    /// <summary>
    /// src/build.c -> L:4525
    /// </summary>
    [GeneratedRegex(@"^invalid\ rootpage$", RegexOptions.IgnoreCase)]
    private static partial Regex invalidrootpage();
    /// <summary>
    /// src/build.c -> L:4744
    /// </summary>
    [GeneratedRegex(@"^no\ such\ index:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchindex();
    /// <summary>
    /// src/build.c -> L:4753
    /// </summary>
    [GeneratedRegex(@"^index\ associated\ with\ UNIQUE\ $", RegexOptions.IgnoreCase)]
    private static partial Regex indexassociatedwithUNIQUE();
    /// <summary>
    /// src/build.c -> L:4942
    /// </summary>
    [GeneratedRegex(@"^too\ many\ FROM\ clause\ terms,\ max:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanyFROMclausetermsmax();
    /// <summary>
    /// src/build.c -> L:5210
    /// </summary>
    [GeneratedRegex(@"^a\ JOIN\ clause\ is\ required\ before\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex aJOINclauseisrequiredbefore();
    /// <summary>
    /// src/build.c -> L:5467
    /// </summary>
    [GeneratedRegex(@"^unable\ to\ open\ a\ temporary\ database\ $", RegexOptions.IgnoreCase)]
    private static partial Regex unabletoopenatemporarydatabase();
    /// <summary>
    /// src/build.c -> L:5770
    /// </summary>
    [GeneratedRegex(@"^unable\ to\ identify\ the\ object\ to\ be\ reindexed$", RegexOptions.IgnoreCase)]
    private static partial Regex unabletoidentifytheobjecttobereindexed();
    /// <summary>
    /// src/build.c -> L:5903
    /// </summary>
    [GeneratedRegex(@"^duplicate\ WITH\ table\ name:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex duplicateWITHtablename();
    /// <summary>
    /// src/callback.c -> L:230
    /// </summary>
    [GeneratedRegex(@"^no\ such\ collation\ sequence:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchcollationsequence();
    /// <summary>
    /// src/delete.c -> L:93
    /// </summary>
    [GeneratedRegex(@"^unsafe\ use\ of\ virtual\ table\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex unsafeuseofvirtualtable();
    /// <summary>
    /// src/delete.c -> L:121
    /// </summary>
    [GeneratedRegex(@"^table\ (.+?)\ may\ not\ be\ modified$", RegexOptions.IgnoreCase)]
    private static partial Regex tablemaynotbemodified();
    /// <summary>
    /// src/delete.c -> L:128
    /// </summary>
    [GeneratedRegex(@"^cannot\ modify\ (.+?)\ because\ it\ is\ a\ view$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotmodifybecauseitisaview();
    /// <summary>
    /// src/delete.c -> L:201
    /// </summary>
    [GeneratedRegex(@"^ORDER\ BY\ without\ LIMIT\ on\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex ORDERBYwithoutLIMITon();
    /// <summary>
    /// src/expr.c -> L:736
    /// </summary>
    [GeneratedRegex(@"^row\ value\ misused$", RegexOptions.IgnoreCase)]
    private static partial Regex rowvaluemisused();
    /// <summary>
    /// src/expr.c -> L:1131
    /// </summary>
    [GeneratedRegex(@"^IN\(\.\.\.\)\ element\ has\ (.+?)\ term(.+?)\ -\ expected\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex INelementhastermexpected();
    /// <summary>
    /// src/expr.c -> L:1210
    /// </summary>
    [GeneratedRegex(@"^too\ many\ arguments\ on\ function\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanyargumentsonfunction();
    /// <summary>
    /// src/expr.c -> L:1270
    /// </summary>
    [GeneratedRegex(@"^too\ many\ terms\ in\ ORDER\ BY\ clause$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanytermsinORDERBYclause();
    /// <summary>
    /// src/expr.c -> L:1317
    /// </summary>
    [GeneratedRegex(@"^unsafe\ use\ of\ %\#T\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex unsafeuseof();
    /// <summary>
    /// src/expr.c -> L:1371
    /// </summary>
    [GeneratedRegex(@"^variable\ number\ must\ be\ between\ \?1\ and\ \?(.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex variablenumbermustbebetween1and();
    /// <summary>
    /// src/expr.c -> L:1405
    /// </summary>
    [GeneratedRegex(@"^too\ many\ SQL\ variables$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanySQLvariables();
    /// <summary>
    /// src/expr.c -> L:2140
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ columns\ assigned\ (.+?)\ values$", RegexOptions.IgnoreCase)]
    private static partial Regex columnsassignedvalues();
    /// <summary>
    /// src/expr.c -> L:2285
    /// </summary>
    [GeneratedRegex(@"^too\ many\ columns\ in\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanycolumnsin();
    /// <summary>
    /// src/expr.c -> L:4478
    /// </summary>
    [GeneratedRegex(@"^oversized\ integer:\ (.+?)%\#T$", RegexOptions.IgnoreCase)]
    private static partial Regex oversizedinteger();
    /// <summary>
    /// src/expr.c -> L:4482
    /// </summary>
    [GeneratedRegex(@"^hex\ literal\ too\ big:\ (.+?)%\#T$", RegexOptions.IgnoreCase)]
    private static partial Regex hexliteraltoobig();
    /// <summary>
    /// src/expr.c -> L:4583
    /// </summary>
    [GeneratedRegex(@"^generated\ column\ loop\ on\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex generatedcolumnloopon();
    /// <summary>
    /// src/expr.c -> L:5487
    /// </summary>
    [GeneratedRegex(@"^misuse\ of\ aggregate:\ %\#T\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex misuseofaggregate();
    /// <summary>
    /// src/expr.c -> L:5535
    /// </summary>
    [GeneratedRegex(@"^unknown\ function:\ %\#T\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex unknownfunction();
    /// <summary>
    /// src/expr.c -> L:7509
    /// </summary>
    [GeneratedRegex(@"^more\ than\ (.+?)\ aggregate\ terms$", RegexOptions.IgnoreCase)]
    private static partial Regex morethanaggregateterms();
    /// <summary>
    /// src/insert.c -> L:1095
    /// </summary>
    [GeneratedRegex(@"^table\ (.+?)\ has\ no\ column\ named\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex tablehasnocolumnnamed();
    /// <summary>
    /// src/insert.c -> L:1251
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ values\ for\ (.+?)\ columns$", RegexOptions.IgnoreCase)]
    private static partial Regex valuesforcolumns();
    /// <summary>
    /// src/insert.c -> L:1286
    /// </summary>
    [GeneratedRegex(@"^UPSERT\ not\ implemented\ for\ virtual\ table\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex UPSERTnotimplementedforvirtualtable();
    /// <summary>
    /// src/insert.c -> L:1291
    /// </summary>
    [GeneratedRegex(@"^cannot\ UPSERT\ a\ view$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotUPSERTaview();
    /// <summary>
    /// src/malloc.c -> L:836
    /// </summary>
    [GeneratedRegex(@"^out\ of\ memory$", RegexOptions.IgnoreCase)]
    private static partial Regex outofmemory();
    /// <summary>
    /// src/parse.y -> L:50
    /// </summary>
    [GeneratedRegex(@"^incomplete\ input$", RegexOptions.IgnoreCase)]
    private static partial Regex incompleteinput();
    /// <summary>
    /// src/parse.y -> L:125
    /// </summary>
    [GeneratedRegex(@"^near\ ""(.+?)"":\ syntax\ error$", RegexOptions.IgnoreCase)]
    private static partial Regex nearsyntaxerror();
    /// <summary>
    /// src/parse.y -> L:154
    /// </summary>
    [GeneratedRegex(@"^syntax\ error\ near\ ""ORDER\ BY""$", RegexOptions.IgnoreCase)]
    private static partial Regex syntaxerrornearORDERBY();
    /// <summary>
    /// src/parse.y -> L:156
    /// </summary>
    [GeneratedRegex(@"^syntax\ error\ near\ ""LIMIT""$", RegexOptions.IgnoreCase)]
    private static partial Regex syntaxerrornearLIMIT();
    /// <summary>
    /// src/parse.y -> L:240
    /// </summary>
    [GeneratedRegex(@"^unknown\ table\ option:\ %\.\*s$", RegexOptions.IgnoreCase)]
    private static partial Regex unknowntableoption();
    /// <summary>
    /// src/parse.y -> L:556
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ clause\ should\ come\ after\ (.+?)\ not\ before$", RegexOptions.IgnoreCase)]
    private static partial Regex clauseshouldcomeafternotbefore();
    /// <summary>
    /// src/parse.y -> L:566
    /// </summary>
    [GeneratedRegex(@"^too\ many\ terms\ in\ compound\ SELECT$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanytermsincompoundSELECT();
    /// <summary>
    /// src/parse.y -> L:1295
    /// </summary>
    [GeneratedRegex(@"^%\#T\(\)\ is\ not\ an\ ordered-set\ aggregate$", RegexOptions.IgnoreCase)]
    private static partial Regex isnotanorderedsetaggregate();
    /// <summary>
    /// src/parse.y -> L:1297
    /// </summary>
    [GeneratedRegex(@"^DISTINCT\ not\ allowed\ on\ ordered-set\ aggregate\ (.+?)\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex DISTINCTnotallowedonorderedsetaggregate();
    /// <summary>
    /// src/parse.y -> L:1687
    /// </summary>
    [GeneratedRegex(@"^syntax\ error\ after\ column\ name\ ""%\.\*s""$", RegexOptions.IgnoreCase)]
    private static partial Regex syntaxerroraftercolumnname();
    /// <summary>
    /// src/pragma.c -> L:165
    /// </summary>
    [GeneratedRegex(@"^temporary\ storage\ cannot\ be\ changed\ $", RegexOptions.IgnoreCase)]
    private static partial Regex temporarystoragecannotbechanged();
    /// <summary>
    /// src/pragma.c -> L:1021
    /// </summary>
    [GeneratedRegex(@"^not\ a\ writable\ directory$", RegexOptions.IgnoreCase)]
    private static partial Regex notawritabledirectory();
    /// <summary>
    /// src/pragma.c -> L:1115
    /// </summary>
    [GeneratedRegex(@"^failed\ to\ set\ lock\ proxy\ file$", RegexOptions.IgnoreCase)]
    private static partial Regex failedtosetlockproxyfile();
    /// <summary>
    /// src/pragma.c -> L:2282
    /// </summary>
    [GeneratedRegex(@"^unsupported\ encoding:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex unsupportedencoding();
    /// <summary>
    /// src/resolve.c -> L:685
    /// </summary>
    [GeneratedRegex(@"^misuse\ of\ aliased\ aggregate\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex misuseofaliasedaggregate();
    /// <summary>
    /// src/resolve.c -> L:691
    /// </summary>
    [GeneratedRegex(@"^misuse\ of\ aliased\ window\ function\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex misuseofaliasedwindowfunction();
    /// <summary>
    /// src/resolve.c -> L:801
    /// </summary>
    [GeneratedRegex(@"^(.+?):\ ""(.+?)""\ -\ should\ this\ be\ a$", RegexOptions.IgnoreCase)]
    private static partial Regex shouldthisbea();
    /// <summary>
    /// src/resolve.c -> L:942
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ prohibited\ in\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex prohibitedin();
    /// <summary>
    /// src/resolve.c -> L:1307
    /// </summary>
    [GeneratedRegex(@"^misuse\ of\ (.+?)\ function\ %\#T\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex misuseoffunction();
    /// <summary>
    /// src/resolve.c -> L:1313
    /// </summary>
    [GeneratedRegex(@"^misuse\ of\ aggregate\ function\ %\#T\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex misuseofaggregatefunction();
    /// <summary>
    /// src/resolve.c -> L:1330
    /// </summary>
    [GeneratedRegex(@"^no\ such\ function:\ %\#T$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchfunction();
    /// <summary>
    /// src/resolve.c -> L:1333
    /// </summary>
    [GeneratedRegex(@"^wrong\ number\ of\ arguments\ to\ function\ %\#T\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex wrongnumberofargumentstofunction();
    /// <summary>
    /// src/resolve.c -> L:1765
    /// </summary>
    [GeneratedRegex(@"^(.+?)\ ORDER\ BY\ term\ does\ not\ match\ any\ $", RegexOptions.IgnoreCase)]
    private static partial Regex ORDERBYtermdoesnotmatchany();
    /// <summary>
    /// src/resolve.c -> L:1796
    /// </summary>
    [GeneratedRegex(@"^too\ many\ terms\ in\ (.+?)\ BY\ clause$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanytermsinBYclause();
    /// <summary>
    /// src/resolve.c -> L:2066
    /// </summary>
    [GeneratedRegex(@"^HAVING\ clause\ on\ a\ non-aggregate\ query$", RegexOptions.IgnoreCase)]
    private static partial Regex HAVINGclauseonanonaggregatequery();
    /// <summary>
    /// src/resolve.c -> L:2146
    /// </summary>
    [GeneratedRegex(@"^aggregate\ functions\ are\ not\ allowed\ in\ $", RegexOptions.IgnoreCase)]
    private static partial Regex aggregatefunctionsarenotallowedin();
    /// <summary>
    /// src/select.c -> L:307
    /// </summary>
    [GeneratedRegex(@"^unknown\ join\ type:\ $", RegexOptions.IgnoreCase)]
    private static partial Regex unknownjointype();
    /// <summary>
    /// src/select.c -> L:539
    /// </summary>
    [GeneratedRegex(@"^a\ NATURAL\ join\ may\ not\ have\ $", RegexOptions.IgnoreCase)]
    private static partial Regex aNATURALjoinmaynothave();
    /// <summary>
    /// src/select.c -> L:591
    /// </summary>
    [GeneratedRegex(@"^cannot\ join\ using\ column\ (.+?)\ -\ column\ $", RegexOptions.IgnoreCase)]
    private static partial Regex cannotjoinusingcolumncolumn();
    /// <summary>
    /// src/select.c -> L:620
    /// </summary>
    [GeneratedRegex(@"^ambiguous\ reference\ to\ (.+?)\ in\ USING\(\)$", RegexOptions.IgnoreCase)]
    private static partial Regex ambiguousreferencetoinUSING();
    /// <summary>
    /// src/select.c -> L:2500
    /// </summary>
    [GeneratedRegex(@"^VIEWs\ and/or\ subqueries\ nested\ too\ deep$", RegexOptions.IgnoreCase)]
    private static partial Regex VIEWsandorsubqueriesnestedtoodeep();
    /// <summary>
    /// src/select.c -> L:2759
    /// </summary>
    [GeneratedRegex(@"^cannot\ use\ window\ functions\ in\ recursive\ queries$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotusewindowfunctionsinrecursivequeries();
    /// <summary>
    /// src/select.c -> L:2847
    /// </summary>
    [GeneratedRegex(@"^recursive\ aggregate\ queries\ not\ supported$", RegexOptions.IgnoreCase)]
    private static partial Regex recursiveaggregatequeriesnotsupported();
    /// <summary>
    /// src/select.c -> L:3179
    /// </summary>
    [GeneratedRegex(@"^all\ VALUES\ must\ have\ the\ same\ number\ of\ terms$", RegexOptions.IgnoreCase)]
    private static partial Regex allVALUESmusthavethesamenumberofterms();
    /// <summary>
    /// src/select.c -> L:3181
    /// </summary>
    [GeneratedRegex(@"^SELECTs\ to\ the\ left\ and\ right\ of\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex SELECTstotheleftandrightof();
    /// <summary>
    /// src/select.c -> L:5714
    /// </summary>
    [GeneratedRegex(@"^'(.+?)'\ is\ not\ a\ function$", RegexOptions.IgnoreCase)]
    private static partial Regex isnotafunction();
    /// <summary>
    /// src/select.c -> L:5927
    /// </summary>
    [GeneratedRegex(@"^too\ many\ CTEs$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanyCTEs();
    /// <summary>
    /// src/select.c -> L:5967
    /// </summary>
    [GeneratedRegex(@"^table\ (.+?)\ has\ (.+?)\ values\ for\ (.+?)\ columns$", RegexOptions.IgnoreCase)]
    private static partial Regex tablehasvaluesforcolumns();
    /// <summary>
    /// src/select.c -> L:6175
    /// </summary>
    [GeneratedRegex(@"^too\ many\ references\ to\ ""(.+?)"":\ max\ 65535$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanyreferencestomax65535();
    /// <summary>
    /// src/select.c -> L:6194
    /// </summary>
    [GeneratedRegex(@"^access\ to\ view\ ""(.+?)""\ prohibited$", RegexOptions.IgnoreCase)]
    private static partial Regex accesstoviewprohibited();
    /// <summary>
    /// src/select.c -> L:6470
    /// </summary>
    [GeneratedRegex(@"^no\ such\ table:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchtable();
    /// <summary>
    /// src/select.c -> L:6472
    /// </summary>
    [GeneratedRegex(@"^no\ tables\ specified$", RegexOptions.IgnoreCase)]
    private static partial Regex notablesspecified();
    /// <summary>
    /// src/select.c -> L:6482
    /// </summary>
    [GeneratedRegex(@"^too\ many\ columns\ in\ result\ set$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanycolumnsinresultset();
    /// <summary>
    /// src/select.c -> L:6841
    /// </summary>
    [GeneratedRegex(@"^DISTINCT\ aggregates\ must\ have\ exactly\ one\ $", RegexOptions.IgnoreCase)]
    private static partial Regex DISTINCTaggregatesmusthaveexactlyone();
    /// <summary>
    /// src/select.c -> L:7979
    /// </summary>
    [GeneratedRegex(@"^expected\ (.+?)\ columns\ for\ '(.+?)'\ but\ got\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex expectedcolumnsforbutgot();
    /// <summary>
    /// src/tokenize.c -> L:707
    /// </summary>
    [GeneratedRegex(@"^unrecognized\ token:\ ""(.+?)""$", RegexOptions.IgnoreCase)]
    private static partial Regex unrecognizedtoken();
    /// <summary>
    /// src/trigger.c -> L:131
    /// </summary>
    [GeneratedRegex(@"^temporary\ trigger\ may\ not\ have\ qualified\ name$", RegexOptions.IgnoreCase)]
    private static partial Regex temporarytriggermaynothavequalifiedname();
    /// <summary>
    /// src/trigger.c -> L:190
    /// </summary>
    [GeneratedRegex(@"^cannot\ create\ triggers\ on\ virtual\ tables$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotcreatetriggersonvirtualtables();
    /// <summary>
    /// src/trigger.c -> L:194
    /// </summary>
    [GeneratedRegex(@"^cannot\ create\ triggers\ on\ shadow\ tables$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotcreatetriggersonshadowtables();
    /// <summary>
    /// src/trigger.c -> L:213
    /// </summary>
    [GeneratedRegex(@"^trigger\ (.+?)\ already\ exists$", RegexOptions.IgnoreCase)]
    private static partial Regex triggeralreadyexists();
    /// <summary>
    /// src/trigger.c -> L:230
    /// </summary>
    [GeneratedRegex(@"^cannot\ create\ trigger\ on\ system\ table$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotcreatetriggeronsystemtable();
    /// <summary>
    /// src/trigger.c -> L:239
    /// </summary>
    [GeneratedRegex(@"^cannot\ create\ (.+?)\ trigger\ on\ view:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotcreatetriggeronview();
    /// <summary>
    /// src/trigger.c -> L:244
    /// </summary>
    [GeneratedRegex(@"^cannot\ create\ INSTEAD\ OF$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotcreateINSTEADOF();
    /// <summary>
    /// src/trigger.c -> L:353
    /// </summary>
    [GeneratedRegex(@"^trigger\ ""(.+?)""\ contains\ too\ many\ steps$", RegexOptions.IgnoreCase)]
    private static partial Regex triggercontainstoomanysteps();
    /// <summary>
    /// src/trigger.c -> L:698
    /// </summary>
    [GeneratedRegex(@"^no\ such\ trigger:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchtrigger();
    /// <summary>
    /// src/trigger.c -> L:915
    /// </summary>
    [GeneratedRegex(@"^RETURNING\ may\ not\ use\ ""TABLE\.\*""\ wildcards$", RegexOptions.IgnoreCase)]
    private static partial Regex RETURNINGmaynotuseTABLEwildcards();
    /// <summary>
    /// src/trigger.c -> L:1269
    /// </summary>
    [GeneratedRegex(@"^triggers\ nested\ too\ deep$", RegexOptions.IgnoreCase)]
    private static partial Regex triggersnestedtoodeep();
    /// <summary>
    /// src/update.c -> L:223
    /// </summary>
    [GeneratedRegex(@"^ORDER\ BY\ without\ LIMIT\ on\ UPDATE$", RegexOptions.IgnoreCase)]
    private static partial Regex ORDERBYwithoutLIMITonUPDATE();
    /// <summary>
    /// src/upsert.c -> L:214
    /// </summary>
    [GeneratedRegex(@"^(.+?)ON\ CONFLICT\ clause\ does\ not\ match\ any\ $", RegexOptions.IgnoreCase)]
    private static partial Regex ONCONFLICTclausedoesnotmatchany();
    /// <summary>
    /// src/vdbeblob.c -> L:165
    /// </summary>
    [GeneratedRegex(@"^cannot\ open\ virtual\ table:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotopenvirtualtable();
    /// <summary>
    /// src/vdbeblob.c -> L:169
    /// </summary>
    [GeneratedRegex(@"^cannot\ open\ table\ without\ rowid:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotopentablewithoutrowid();
    /// <summary>
    /// src/vdbeblob.c -> L:173
    /// </summary>
    [GeneratedRegex(@"^cannot\ open\ table\ with\ generated\ columns:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotopentablewithgeneratedcolumns();
    /// <summary>
    /// src/vdbeblob.c -> L:179
    /// </summary>
    [GeneratedRegex(@"^cannot\ open\ view:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex cannotopenview();
    /// <summary>
    /// src/vtab.c -> L:717
    /// </summary>
    [GeneratedRegex(@"^no\ such\ module:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchmodule();
    /// <summary>
    /// src/where.c -> L:4549
    /// </summary>
    [GeneratedRegex(@"^(.+?)\.xBestIndex\ malfunction$", RegexOptions.IgnoreCase)]
    private static partial Regex xBestIndexmalfunction();
    /// <summary>
    /// src/where.c -> L:6296
    /// </summary>
    [GeneratedRegex(@"^no\ query\ solution$", RegexOptions.IgnoreCase)]
    private static partial Regex noquerysolution();
    /// <summary>
    /// src/where.c -> L:7005
    /// </summary>
    [GeneratedRegex(@"^at\ most\ (.+?)\ tables\ in\ a\ join$", RegexOptions.IgnoreCase)]
    private static partial Regex atmosttablesinajoin();
    /// <summary>
    /// src/where.c -> L:8016
    /// </summary>
    [GeneratedRegex(@"^internal\ query\ planner\ error$", RegexOptions.IgnoreCase)]
    private static partial Regex internalqueryplannererror();
    /// <summary>
    /// src/wherecode.c -> L:3115
    /// </summary>
    [GeneratedRegex(@"^too\ many\ RIGHT\ JOINs$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanyRIGHTJOINs();
    /// <summary>
    /// src/whereexpr.c -> L:1999
    /// </summary>
    [GeneratedRegex(@"^too\ many\ arguments\ on\ (.+?)\(\)\ -\ max\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex toomanyargumentsonmax();
    /// <summary>
    /// src/window.c -> L:640
    /// </summary>
    [GeneratedRegex(@"^no\ such\ window:\ (.+?)$", RegexOptions.IgnoreCase)]
    private static partial Regex nosuchwindow();
    /// <summary>
    /// src/window.c -> L:1240
    /// </summary>
    [GeneratedRegex(@"^unsupported\ frame\ specification$", RegexOptions.IgnoreCase)]
    private static partial Regex unsupportedframespecification();
}
