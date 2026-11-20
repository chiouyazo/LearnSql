using LearnSql.Core.Services.Guided;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Core;

public sealed class GuidedRunMachineTests
{
    private static GuidedStep Step(string id, GuidedStepKind kind, int? rows = null)
    {
        return new GuidedStep
        {
            Id = id,
            Title = id,
            Sql = "SELECT " + id + ";",
            Kind = kind,
            QueryExplanation = new QueryExplanation { Summary = "s", Parts = [] },
            ResultExplanation = "r",
            DatabaseEffect = "e",
            ExpectedResultRows = rows,
        };
    }

    private static GuidedScenarioDefinition Definition(params GuidedStep[] steps)
    {
        return new GuidedScenarioDefinition
        {
            Id = "g1",
            Title = "Titel",
            Description = "Beschreibung",
            Topic = ExerciseTopic.Select,
            RequiresFreshDatabase = false,
            Steps = steps,
        };
    }

    private static ExecutionResult Rows(int count)
    {
        object?[][] data = [.. Enumerable.Range(0, count).Select((int i) => new object?[] { i })];
        StatementResult statement = new StatementResult(
            0,
            0,
            StatementKind.Select,
            default,
            [new ResultSet([], data, false)],
            null,
            TimeSpan.Zero,
            []
        );
        return new ExecutionResult([statement], [], null, TimeSpan.Zero, false);
    }

    private static ExecutionResult Cancelled()
    {
        return new ExecutionResult(
            [],
            [],
            new ExecutionError(ExecutionErrorKind.Cancelled, string.Empty),
            TimeSpan.Zero,
            false
        );
    }

    [Fact]
    public void Constructor_NoSteps_Throws()
    {
        Assert.Throws<ArgumentException>(() => new GuidedRunMachine(Definition()));
    }

    [Fact]
    public void NewRun_StartsAtFirstStepWithoutProgress()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ReadOnly), Step("b", GuidedStepKind.ReadOnly))
        );

        Assert.Equal(0, machine.CurrentIndex);
        Assert.Equal(0, machine.CompletedCount);
        Assert.True(machine.IsFirst);
        Assert.False(machine.IsLast);
        Assert.False(machine.CanMoveBack);
        Assert.False(machine.IsFinished);
    }

    [Theory]
    [InlineData(GuidedStepKind.ReadOnly)]
    [InlineData(GuidedStepKind.ChangesData)]
    public void MoveNext_BlockedUntilTheStepRan(GuidedStepKind kind)
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", kind), Step("b", GuidedStepKind.ReadOnly))
        );

        Assert.False(machine.CanMoveNext);
        Assert.False(machine.MoveNext());
        machine.Record(Rows(1));
        Assert.True(machine.CanMoveNext);
        Assert.True(machine.MoveNext());
        Assert.Equal(1, machine.CurrentIndex);
    }

    [Fact]
    public void RunsOnEnter_NewStep_IsTrue()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ChangesData))
        );

        Assert.True(machine.RunsOnEnter);
    }

    [Fact]
    public void RunsOnEnter_CompletedReadOnlyStep_RunsAgain()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ReadOnly), Step("b", GuidedStepKind.ReadOnly))
        );
        machine.Record(Rows(1));
        machine.MoveNext();
        machine.MovePrevious();

        Assert.True(machine.CurrentCompleted);
        Assert.True(machine.RunsOnEnter);
    }

    [Fact]
    public void RunsOnEnter_CompletedChangeStep_DoesNotRunAgain()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ChangesData), Step("b", GuidedStepKind.ReadOnly))
        );
        machine.Record(Rows(1));
        machine.MoveNext();
        machine.MovePrevious();

        Assert.False(machine.RunsOnEnter);
    }

    [Fact]
    public void Record_Interrupted_DoesNotComplete()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ReadOnly))
        );

        GuidedCheck check = machine.Record(Cancelled());

        Assert.Equal(GuidedCheckKind.Interrupted, check.Kind);
        Assert.False(machine.CurrentCompleted);
        Assert.Equal(check, machine.CurrentCheck);
    }

    [Fact]
    public void Record_AllSteps_Finishes()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ReadOnly, 1), Step("b", GuidedStepKind.ReadOnly, 2))
        );

        machine.Record(Rows(1));
        machine.MoveNext();
        machine.Record(Rows(2));

        Assert.True(machine.IsFinished);
        Assert.Equal(1.0, machine.Fraction);
        Assert.True(machine.IsLast);
        Assert.False(machine.CanMoveNext);
    }

    [Fact]
    public void Record_Deviation_CountsAsDoneButKeepsCheck()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ReadOnly, 3))
        );

        GuidedCheck check = machine.Record(Rows(4));

        Assert.Equal(GuidedCheckKind.RowCountDiffers, check.Kind);
        Assert.True(machine.CurrentCompleted);
    }

    [Fact]
    public void MovePrevious_FromSecondStep_Works()
    {
        GuidedRunMachine machine = new GuidedRunMachine(
            Definition(Step("a", GuidedStepKind.ReadOnly), Step("b", GuidedStepKind.ReadOnly))
        );
        machine.Record(Rows(1));
        machine.MoveNext();

        Assert.True(machine.MovePrevious());
        Assert.False(machine.MovePrevious());
        Assert.Equal(0, machine.CurrentIndex);
    }
}
