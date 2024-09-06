# Debaser

![](https://raw.githubusercontent.com/rebus-org/Debaser/master/artwork/new_logo_512.png)

Have you ever had the need to insert/update many rows in SQL Server? (i.e. "upsert" them)

Did you try to use the `MERGE INTO ...` syntax then? (and did you enjoy it?)

Did you know that ADO.NET has an API that allows for STREAMING rows to a temporary table in `tempdb` making SQL Server call a stored procedure for each row very quickly?

Did you know that Debaser can do these things for you?

## How to do it?

Create a class that looks the way you want it to look:

```csharp
class SomeDataRow(int id, decimal number, string text)
{
    public int Id { get; } = id;
    public decimal Number { get; } = number;
	public string Text { get; } = text;
}
```

Debaser supports using a default constructor and properties with setters, or using a constructor with parameters matching the properties like this example shows.

You CAN also use C# records, but Debaser's configuration attributes (like `[DebaserKey]`, `[DebaserMapper(..)]`, etc.) must be used on properties, so C# classes using the primary constructor syntax as shown above is probably the most compact and neat way to declare your Debaser types.

Then you create the `UpsertHelper` for it:

```csharp
var upsertHelper = new UpsertHelper<SomeDataRow>("db");
```

(where `db` in this case is the name of a connection string in the current app.config)

and then you do this once:

```csharp
upsertHelper.CreateSchema();
```

(which will create a table, a table data type, and a stored procedure)

and then you insert 100k rows like this:

```csharp
var rows = Enumerable.Range(1, 100000)
	.Select(i => new SomeDataRow(i, i*2.3m, $"This is row {i}"))
	.ToList();

var stopwatch = Stopwatch.StartNew();

await upsertHelper.Upsert(rows);

var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

Console.WriteLine($"Upserting {rows.Count} rows took {elapsedSeconds} - that's {rows.Count / elapsedSeconds:0.0} rows/s");
```

which on my machine yields this:

```csharp
Upserting 100000 rows took 0.753394 - that's 132732.7 rows/s
```

which I think is fair.

## Merging with existing data

By default each row is identified by its ID property. This means that any IDs matching existing rows will lead to an update instead of an insert.

Let's hurl 100k rows into the database again:

```csharp
var rows = Enumerable.Range(1, 100000)
	.Select(i => new SomeDataRow(i, i*2.3m, $"This is row {i}"))
	.ToList();

await upsertHelper.Upsert(rows);
```

and then let's perform some iterations where we pick 10k random rows, mutate them, and upsert them:

```csharp
var stopwatch = Stopwatch.StartNew();

var updatedRows = rows.InRandomOrder().Take(rowsToChange)
    .Select(row => new SomeDataRow(row.Id, row.Number + 1, string.Concat(row.Text, "-HEJ")));

await upsertHelper.Upsert(updatedRows);
```

which on my machine yields this:

```csharp
Updating 10000 random rows in dataset of 100000 took average of 0.2 s - that's 57191.1 rows/s
```

which (again) I think is OK.

## Maturity

Debaser is fairly mature. It's been used for some serious things already, but please back your Debaser-based stuff up by some nice integration tests.
