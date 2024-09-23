using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Tests.Extensions;
using NUnit.Framework;

namespace Debaser.Tests.Readme;

/*
   WITH FASTMEMBER:
   
   Upserting 1000000 rows took 2,2769185 - that's 439190,1 rows/s
   Upserting 10000000 rows took 25,0504995 - that's 399193,6 rows/s
   Updating 10000 random rows in dataset of 100000 took average of 0,1 s - that's 122823,5 rows/s
   Updating 10000 random rows in dataset of 1000000 took average of 0,5 s - that's 18779,5 rows/s
   
   Upserting 1000000 rows took 2,2505385 - that's 444338,1 rows/s
   Upserting 10000000 rows took 22,888406 - that's 436902,4 rows/s
   Updating 10000 random rows in dataset of 100000 took average of 0,1 s - that's 117499,2 rows/s
   Updating 10000 random rows in dataset of 1000000 took average of 0,6 s - that's 17972,9 rows/s
   
   
   WITH FASTERFLECT:
   
   Upserting 1000000 rows took 2,6661161 - that's 375077,4 rows/s
   Upserting 10000000 rows took 24,5490089 - that's 407348,4 rows/s
   Updating 10000 random rows in dataset of 100000 took average of 0,1 s - that's 119218,9 rows/s
   Updating 10000 random rows in dataset of 1000000 took average of 0,5 s - that's 18525,6 rows/s
   
   Upserting 1000000 rows took 2,5434438 - that's 393167,7 rows/s
   Upserting 10000000 rows took 25,4894612 - that's 392319,0 rows/s
   Updating 10000 random rows in dataset of 100000 took average of 0,1 s - that's 119532,7 rows/s
   Updating 10000 random rows in dataset of 1000000 took average of 0,5 s - that's 18257,8 rows/s
   
 */
[TestFixture]
public class Snippets : FixtureBase
{
    [TestCase(10000000)]
    [TestCase(1000000)]
    public async Task InsertRows(int count)
    {
        var rows = Enumerable.Range(1, count)
            .Select(i => new SomeDataRow(i, i * 2.3m, $"This is row {i}"))
            .ToList();

        var upsertHelper = new UpsertHelper<SomeDataRow>(ConnectionString);

        upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        upsertHelper.CreateSchema();

        var stopwatch = Stopwatch.StartNew();

        await upsertHelper.UpsertAsync(rows);

        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

        Console.WriteLine($"Upserting {rows.Count} rows took {elapsedSeconds} - that's {rows.Count / elapsedSeconds:0.0} rows/s");
    }

    [TestCase(1000000, 10000, 10)]
    [TestCase(100000, 10000, 10)]
    public async Task UpdateExistingRows(int count, int rowsToChange, int iterations)
    {
        var rows = Enumerable.Range(1, count)
            .Select(i => new SomeDataRow(i, i * 2.3m, $"This is row {i}"))
            .ToList();

        var upsertHelper = new UpsertHelper<SomeDataRow>(ConnectionString);

        upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        upsertHelper.CreateSchema();

        await upsertHelper.UpsertAsync(rows);

        var recordedExecutionTimes = new Queue<TimeSpan>();

        for (var counter = 0; counter < 10; counter++)
        {
            var stopwatch = Stopwatch.StartNew();

            var updatedRows = rows.InRandomOrder().Take(rowsToChange)
                .Select(row => new SomeDataRow(row.Id, row.Number + 1, string.Concat(row.Text, "-HEJ")));

            await upsertHelper.UpsertAsync(updatedRows);

            var elapsed = stopwatch.Elapsed;
            recordedExecutionTimes.Enqueue(elapsed);
        }

        var averageExecutionTimeSeconds = recordedExecutionTimes.Average(t => t.TotalSeconds);

        Console.WriteLine($"Updating {rowsToChange} random rows in dataset of {count} took average of {averageExecutionTimeSeconds:0.0} s - that's {rowsToChange / averageExecutionTimeSeconds:0.0} rows/s");
    }
}

class SomeDataRow(int id, decimal number, string text)
{
    public int Id { get; } = id;
    public decimal Number { get; } = number;
    public string Text { get; } = text;
}