/// <summary>auto generated sqlite error types</summary>
public enum SqliteErrorTypes 
{
    Unknown,
    /// <summary> src/alter.c -> L:40 </summary>
    tablemaynotbealtered,
    /// <summary> src/alter.c -> L:176 </summary>
    viewmaynotbealtered,
    /// <summary> src/alter.c -> L:321 </summary>
    cannotrowidalias,
    /// <summary> src/alter.c -> L:425 </summary>
    CannotaddaPRIMARYKEYcolumn,
    /// <summary> src/alter.c -> L:566 </summary>
    virtualtablesmaynotbealtered,
    /// <summary> src/alter.c -> L:573 </summary>
    Cannotaddacolumntoaview,
    /// <summary> src/alter.c -> L:649 </summary>
    cannot,
    /// <summary> src/alter.c -> L:706 </summary>
    nosuchcolumn,
    /// <summary> src/alter.c -> L:2385 </summary>
    cannotdropcolumn,
    /// <summary> src/alter.c -> L:2394 </summary>
    cannotdropcolumnnoothercolumnsexist,
    /// <summary> src/attach.c -> L:487 </summary>
    cannotusevariables,
    /// <summary> src/auth.c -> L:91 </summary>
    authorizermalfunction,
    /// <summary> src/auth.c -> L:119 </summary>
    accesstoisprohibited,
    /// <summary> src/auth.c -> L:230 </summary>
    notauthorized,
    /// <summary> src/build.c -> L:737 </summary>
    notauthorizedtousefunction,
    /// <summary> src/build.c -> L:1069 </summary>
    corruptdatabase,
    /// <summary> src/build.c -> L:1075 </summary>
    unknowndatabase,
    /// <summary> src/build.c -> L:1143 </summary>
    objectnamereservedforinternaluse,
    /// <summary> src/build.c -> L:1320 </summary>
    temporarytablenamemustbeunqualified,
    /// <summary> src/build.c -> L:1372 </summary>
    alreadyexists,
    /// <summary> src/build.c -> L:1382 </summary>
    thereisalreadyanindexnamed,
    /// <summary> src/build.c -> L:1531 </summary>
    cannotuseRETURNINGinatrigger,
    /// <summary> src/build.c -> L:1591 </summary>
    toomanycolumnson,
    /// <summary> src/build.c -> L:1638 </summary>
    duplicatecolumnname,
    /// <summary> src/build.c -> L:1830 </summary>
    defaultvalueofcolumnisnotconstant,
    /// <summary> src/build.c -> L:1836 </summary>
    cannotuseDEFAULTonageneratedcolumn,
    /// <summary> src/build.c -> L:1979 </summary>
    AUTOINCREMENTisonlyallowedonan,
    /// <summary> src/build.c -> L:2076 </summary>
    virtualtablescannotusecomputedcolumns,
    /// <summary> src/build.c -> L:2110 </summary>
    erroringeneratedcolumn,
    /// <summary> src/build.c -> L:2117 </summary>
    generatedcolumnsnotsupported,
    /// <summary> src/build.c -> L:2821 </summary>
    missingdatatypefor,
    /// <summary> src/build.c -> L:2851 </summary>
    PRIMARYKEYmissingontable,
    /// <summary> src/build.c -> L:2901 </summary>
    musthaveatleastonenongeneratedcolumn,
    /// <summary> src/build.c -> L:3133 </summary>
    parametersarenotallowedinviews,
    /// <summary> src/build.c -> L:3256 </summary>
    viewiscircularlydefined,
    /// <summary> src/build.c -> L:3689 </summary>
    tablemaynotbedropped,
    /// <summary> src/build.c -> L:3698 </summary>
    useDROPTABLEtodeletetable,
    /// <summary> src/build.c -> L:3702 </summary>
    useDROPVIEWtodeleteview,
    /// <summary> src/build.c -> L:3763 </summary>
    foreignkeyon,
    /// <summary> src/build.c -> L:4042 </summary>
    unsupporteduseofNULLS,
    /// <summary> src/build.c -> L:4170 </summary>
    tablemaynotbeindexed,
    /// <summary> src/build.c -> L:4175 </summary>
    viewsmaynotbeindexed,
    /// <summary> src/build.c -> L:4181 </summary>
    virtualtablesmaynotbeindexed,
    /// <summary> src/build.c -> L:4211 </summary>
    thereisalreadyatablenamed,
    /// <summary> src/build.c -> L:4217 </summary>
    indexalreadyexists,
    /// <summary> src/build.c -> L:4353 </summary>
    expressionsprohibitedinPRIMARYKEYand,
    /// <summary> src/build.c -> L:4525 </summary>
    invalidrootpage,
    /// <summary> src/build.c -> L:4744 </summary>
    nosuchindex,
    /// <summary> src/build.c -> L:4753 </summary>
    indexassociatedwithUNIQUE,
    /// <summary> src/build.c -> L:4942 </summary>
    toomanyFROMclausetermsmax,
    /// <summary> src/build.c -> L:5210 </summary>
    aJOINclauseisrequiredbefore,
    /// <summary> src/build.c -> L:5467 </summary>
    unabletoopenatemporarydatabase,
    /// <summary> src/build.c -> L:5770 </summary>
    unabletoidentifytheobjecttobereindexed,
    /// <summary> src/build.c -> L:5903 </summary>
    duplicateWITHtablename,
    /// <summary> src/callback.c -> L:230 </summary>
    nosuchcollationsequence,
    /// <summary> src/delete.c -> L:93 </summary>
    unsafeuseofvirtualtable,
    /// <summary> src/delete.c -> L:121 </summary>
    tablemaynotbemodified,
    /// <summary> src/delete.c -> L:128 </summary>
    cannotmodifybecauseitisaview,
    /// <summary> src/delete.c -> L:201 </summary>
    ORDERBYwithoutLIMITon,
    /// <summary> src/expr.c -> L:736 </summary>
    rowvaluemisused,
    /// <summary> src/expr.c -> L:1131 </summary>
    INelementhastermexpected,
    /// <summary> src/expr.c -> L:1210 </summary>
    toomanyargumentsonfunction,
    /// <summary> src/expr.c -> L:1270 </summary>
    toomanytermsinORDERBYclause,
    /// <summary> src/expr.c -> L:1317 </summary>
    unsafeuseof,
    /// <summary> src/expr.c -> L:1371 </summary>
    variablenumbermustbebetween1and,
    /// <summary> src/expr.c -> L:1405 </summary>
    toomanySQLvariables,
    /// <summary> src/expr.c -> L:2140 </summary>
    columnsassignedvalues,
    /// <summary> src/expr.c -> L:2285 </summary>
    toomanycolumnsin,
    /// <summary> src/expr.c -> L:4478 </summary>
    oversizedinteger,
    /// <summary> src/expr.c -> L:4482 </summary>
    hexliteraltoobig,
    /// <summary> src/expr.c -> L:4583 </summary>
    generatedcolumnloopon,
    /// <summary> src/expr.c -> L:5487 </summary>
    misuseofaggregate,
    /// <summary> src/expr.c -> L:5535 </summary>
    unknownfunction,
    /// <summary> src/expr.c -> L:7509 </summary>
    morethanaggregateterms,
    /// <summary> src/insert.c -> L:1095 </summary>
    tablehasnocolumnnamed,
    /// <summary> src/insert.c -> L:1251 </summary>
    valuesforcolumns,
    /// <summary> src/insert.c -> L:1286 </summary>
    UPSERTnotimplementedforvirtualtable,
    /// <summary> src/insert.c -> L:1291 </summary>
    cannotUPSERTaview,
    /// <summary> src/malloc.c -> L:836 </summary>
    outofmemory,
    /// <summary> src/parse.y -> L:50 </summary>
    incompleteinput,
    /// <summary> src/parse.y -> L:125 </summary>
    nearsyntaxerror,
    /// <summary> src/parse.y -> L:154 </summary>
    syntaxerrornearORDERBY,
    /// <summary> src/parse.y -> L:156 </summary>
    syntaxerrornearLIMIT,
    /// <summary> src/parse.y -> L:240 </summary>
    unknowntableoption,
    /// <summary> src/parse.y -> L:556 </summary>
    clauseshouldcomeafternotbefore,
    /// <summary> src/parse.y -> L:566 </summary>
    toomanytermsincompoundSELECT,
    /// <summary> src/parse.y -> L:1295 </summary>
    isnotanorderedsetaggregate,
    /// <summary> src/parse.y -> L:1297 </summary>
    DISTINCTnotallowedonorderedsetaggregate,
    /// <summary> src/parse.y -> L:1687 </summary>
    syntaxerroraftercolumnname,
    /// <summary> src/pragma.c -> L:165 </summary>
    temporarystoragecannotbechanged,
    /// <summary> src/pragma.c -> L:1021 </summary>
    notawritabledirectory,
    /// <summary> src/pragma.c -> L:1115 </summary>
    failedtosetlockproxyfile,
    /// <summary> src/pragma.c -> L:2282 </summary>
    unsupportedencoding,
    /// <summary> src/resolve.c -> L:685 </summary>
    misuseofaliasedaggregate,
    /// <summary> src/resolve.c -> L:691 </summary>
    misuseofaliasedwindowfunction,
    /// <summary> src/resolve.c -> L:801 </summary>
    shouldthisbea,
    /// <summary> src/resolve.c -> L:942 </summary>
    prohibitedin,
    /// <summary> src/resolve.c -> L:1307 </summary>
    misuseoffunction,
    /// <summary> src/resolve.c -> L:1313 </summary>
    misuseofaggregatefunction,
    /// <summary> src/resolve.c -> L:1330 </summary>
    nosuchfunction,
    /// <summary> src/resolve.c -> L:1333 </summary>
    wrongnumberofargumentstofunction,
    /// <summary> src/resolve.c -> L:1765 </summary>
    ORDERBYtermdoesnotmatchany,
    /// <summary> src/resolve.c -> L:1796 </summary>
    toomanytermsinBYclause,
    /// <summary> src/resolve.c -> L:2066 </summary>
    HAVINGclauseonanonaggregatequery,
    /// <summary> src/resolve.c -> L:2146 </summary>
    aggregatefunctionsarenotallowedin,
    /// <summary> src/select.c -> L:307 </summary>
    unknownjointype,
    /// <summary> src/select.c -> L:539 </summary>
    aNATURALjoinmaynothave,
    /// <summary> src/select.c -> L:591 </summary>
    cannotjoinusingcolumncolumn,
    /// <summary> src/select.c -> L:620 </summary>
    ambiguousreferencetoinUSING,
    /// <summary> src/select.c -> L:2500 </summary>
    VIEWsandorsubqueriesnestedtoodeep,
    /// <summary> src/select.c -> L:2759 </summary>
    cannotusewindowfunctionsinrecursivequeries,
    /// <summary> src/select.c -> L:2847 </summary>
    recursiveaggregatequeriesnotsupported,
    /// <summary> src/select.c -> L:3179 </summary>
    allVALUESmusthavethesamenumberofterms,
    /// <summary> src/select.c -> L:3181 </summary>
    SELECTstotheleftandrightof,
    /// <summary> src/select.c -> L:5714 </summary>
    isnotafunction,
    /// <summary> src/select.c -> L:5927 </summary>
    toomanyCTEs,
    /// <summary> src/select.c -> L:5967 </summary>
    tablehasvaluesforcolumns,
    /// <summary> src/select.c -> L:6175 </summary>
    toomanyreferencestomax65535,
    /// <summary> src/select.c -> L:6194 </summary>
    accesstoviewprohibited,
    /// <summary> src/select.c -> L:6470 </summary>
    nosuchtable,
    /// <summary> src/select.c -> L:6472 </summary>
    notablesspecified,
    /// <summary> src/select.c -> L:6482 </summary>
    toomanycolumnsinresultset,
    /// <summary> src/select.c -> L:6841 </summary>
    DISTINCTaggregatesmusthaveexactlyone,
    /// <summary> src/select.c -> L:7979 </summary>
    expectedcolumnsforbutgot,
    /// <summary> src/tokenize.c -> L:707 </summary>
    unrecognizedtoken,
    /// <summary> src/trigger.c -> L:131 </summary>
    temporarytriggermaynothavequalifiedname,
    /// <summary> src/trigger.c -> L:190 </summary>
    cannotcreatetriggersonvirtualtables,
    /// <summary> src/trigger.c -> L:194 </summary>
    cannotcreatetriggersonshadowtables,
    /// <summary> src/trigger.c -> L:213 </summary>
    triggeralreadyexists,
    /// <summary> src/trigger.c -> L:230 </summary>
    cannotcreatetriggeronsystemtable,
    /// <summary> src/trigger.c -> L:239 </summary>
    cannotcreatetriggeronview,
    /// <summary> src/trigger.c -> L:244 </summary>
    cannotcreateINSTEADOF,
    /// <summary> src/trigger.c -> L:353 </summary>
    triggercontainstoomanysteps,
    /// <summary> src/trigger.c -> L:698 </summary>
    nosuchtrigger,
    /// <summary> src/trigger.c -> L:915 </summary>
    RETURNINGmaynotuseTABLEwildcards,
    /// <summary> src/trigger.c -> L:1269 </summary>
    triggersnestedtoodeep,
    /// <summary> src/update.c -> L:223 </summary>
    ORDERBYwithoutLIMITonUPDATE,
    /// <summary> src/upsert.c -> L:214 </summary>
    ONCONFLICTclausedoesnotmatchany,
    /// <summary> src/vdbeblob.c -> L:165 </summary>
    cannotopenvirtualtable,
    /// <summary> src/vdbeblob.c -> L:169 </summary>
    cannotopentablewithoutrowid,
    /// <summary> src/vdbeblob.c -> L:173 </summary>
    cannotopentablewithgeneratedcolumns,
    /// <summary> src/vdbeblob.c -> L:179 </summary>
    cannotopenview,
    /// <summary> src/vtab.c -> L:717 </summary>
    nosuchmodule,
    /// <summary> src/where.c -> L:4549 </summary>
    xBestIndexmalfunction,
    /// <summary> src/where.c -> L:6296 </summary>
    noquerysolution,
    /// <summary> src/where.c -> L:7005 </summary>
    atmosttablesinajoin,
    /// <summary> src/where.c -> L:8016 </summary>
    internalqueryplannererror,
    /// <summary> src/wherecode.c -> L:3115 </summary>
    toomanyRIGHTJOINs,
    /// <summary> src/whereexpr.c -> L:1999 </summary>
    toomanyargumentsonmax,
    /// <summary> src/window.c -> L:640 </summary>
    nosuchwindow,
    /// <summary> src/window.c -> L:1240 </summary>
    unsupportedframespecification,
}
