﻿using System;
using System.Reflection;
using Fasterflect;
using Microsoft.Data.SqlClient.Server;

namespace Debaser.Mapping;

/// <summary>
/// Represents a single mapped property
/// </summary>
public class ClassMapProperty
{
    readonly Func<object, object> _toDatabase;
    readonly Func<object, object> _fromDatabase;

    /// <summary>
    /// Gets the name of the database column
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Gets the name of the property
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the database column information for this property
    /// </summary>
    public ColumnInfo ColumnInfo { get; }

    /// <summary>
    /// Gets whether the property is to be PK (or part of a composite PK) for the table
    /// </summary>
    public bool IsKey { get; private set; }

    /// <summary>
    /// Creates the property
    /// </summary>
    public ClassMapProperty(string propertyName, ColumnInfo columnInfo, string columnName, bool isKey, Func<object, object> toDatabase, Func<object, object> fromDatabase)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ColumnInfo = columnInfo ?? throw new ArgumentNullException(nameof(columnInfo));
        ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
        IsKey = isKey;
        _toDatabase = toDatabase ?? throw new ArgumentNullException(nameof(toDatabase));
        _fromDatabase = fromDatabase ?? throw new ArgumentNullException(nameof(fromDatabase));
    }

    /// <summary>
    /// Changes the property to be PK (or part of a composite PK) for the table
    /// </summary>
    public void MakeKey()
    {
        IsKey = true;
    }

    /// <summary>
    /// Gets the necessary SQL to make out a single column definition line in a 'CREATE TABLE (...)' statement
    /// </summary>
    public string GetColumnDefinition() => $"[{ColumnName}] {ColumnInfo.GetTypeDefinition()}";

    internal void WriteTo(SqlDataRecord record, object row)
    {
        var ordinal = record.GetOrdinal(ColumnName);
        //var value = _accessor[row, PropertyName];
        var value = row.GetPropertyValue(PropertyName);
        var valueToWrite = ToDatabase(value);

        record.SetValue(ordinal, valueToWrite);
    }

    internal object FromDatabase(object value)
    {
        var valueToSet = _fromDatabase(value);

        return valueToSet;
    }

    object ToDatabase(object target)
    {
        return _toDatabase(target);
    }

    internal SqlMetaData GetSqlMetaData()
    {
        return ColumnInfo.GetSqlMetaData(ColumnName);
    }

    /// <summary>
    /// Gets a nice string that represents the property
    /// </summary>
    public override string ToString()
    {
        return IsKey
            ? $"{PropertyName} ([{ColumnName}] PK)"
            : $"{PropertyName} ([{ColumnName}])";
    }
}