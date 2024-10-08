﻿using System;

namespace Debaser.Attributes;

/// <summary>
/// Include as an update criteria the requirement that this particular property has a value that is incremented compared to the previous value
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DebaserRevisionCriteriaAttribute(string propertyName) : DebaserUpdateCriteriaAttribute($"[S].[{propertyName}] > [T].[{propertyName}]");