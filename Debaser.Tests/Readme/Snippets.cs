using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Tests.Extensions;
using NUnit.Framework;

namespace Debaser.Tests.Readme;

[TestFixture]
public class Snippets : FixtureBase
{
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
}

class SomeDataRow(int id, decimal number, string text)
{
    public int Id { get; } = id;
    public decimal Number { get; } = number;
    public string Text { get; } = text;
}