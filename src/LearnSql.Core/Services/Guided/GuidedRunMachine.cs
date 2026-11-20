namespace LearnSql.Core.Services.Guided;

public sealed class GuidedRunMachine
{
    private readonly bool[] _completed;
    private readonly Dictionary<int, GuidedCheck> _checks = new Dictionary<int, GuidedCheck>();

    public GuidedRunMachine(GuidedScenarioDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Steps.Count == 0)
        {
            throw new ArgumentException(
                "A guided scenario needs at least one step.",
                nameof(definition)
            );
        }

        Definition = definition;
        _completed = new bool[definition.Steps.Count];
    }

    public GuidedScenarioDefinition Definition { get; }

    public int CurrentIndex { get; private set; }

    public int StepCount => Definition.Steps.Count;

    public GuidedStep CurrentStep => Definition.Steps[CurrentIndex];

    public int CompletedCount => _completed.Count((bool done) => done);

    public double Fraction => (double)CompletedCount / StepCount;

    public bool IsFinished => CompletedCount == StepCount;

    public bool IsFirst => CurrentIndex == 0;

    public bool IsLast => CurrentIndex == StepCount - 1;

    public bool CurrentCompleted => _completed[CurrentIndex];

    public GuidedCheck? CurrentCheck => _checks.GetValueOrDefault(CurrentIndex);

    public bool CanMoveBack => !IsFirst;

    public bool CanMoveNext => !IsLast && CurrentCompleted;

    public bool RunsOnEnter => !CurrentCompleted || CurrentStep.Kind == GuidedStepKind.ReadOnly;

    public bool MoveNext()
    {
        if (!CanMoveNext)
        {
            return false;
        }

        CurrentIndex++;
        return true;
    }

    public bool MovePrevious()
    {
        if (!CanMoveBack)
        {
            return false;
        }

        CurrentIndex--;
        return true;
    }

    public GuidedCheck Record(ExecutionResult result)
    {
        GuidedCheck check = GuidedStepChecker.Check(CurrentStep, result);
        _checks[CurrentIndex] = check;
        if (check.CountsAsDone)
        {
            _completed[CurrentIndex] = true;
        }

        return check;
    }
}
