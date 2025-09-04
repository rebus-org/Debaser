using Npgsql;

namespace Postgredebaser.Internals.Query;

class Parameter(string name, object value)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public object Value { get; } = value;

    public void AddTo(NpgsqlCommand command)
    {
        command.Parameters.AddWithValue(Name, Value ?? DBNull.Value);
    }
}