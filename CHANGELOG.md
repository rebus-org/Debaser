# Changelog

## 0.0.1
* First version

## 0.1.0
* Ability to `LoadWhere` and specify criteria on the form `[SomeProperty] = @someValue` accompanied by arguments on the form `new { someValue = "hej" }`
* Ability to `DeleteWhere` in the same way as `LoadWhere`
* Use the excellent [FastMember] to extract property values from objects to be even faster

## 0.2.0
* Comments!

## 0.3.0
* Add `[DebaserUpdateCriteria]` which can be used to add extra criteria to an upsert type, which must be satisfied for a potential update to be carried out (can be used for avoiding overwriting with old data)

## 0.4.0
* Fix bug that would make the upsert helper unable to handle single-column tables (i.e. a table that simply consists of PKs)
* Fix bug that would result in an exception when upserting an empty sequence
* Parameterize the `DropSchema` method so it can be used to e.g. drop only the data type and the procedure
* Parameterize the `CreateSchema` method so it can be used to e.g. create only the table

## 0.5.0
* Handle `double` (== float) and `float` (== real) properly in automapperino

## 0.6.0
* Add mechanism to allow for configuring command timeout and transaction isolation level

## 0.7.0
* Add ability to completely ignore a property

## 0.8.0
* Make `CreateSchema` default to actually creating it

## 0.9.0
* Target .NET Standard 2.0

## 0.10.0
* Update to FastMember 1.5.0
* Update to System.Data.SqlClient 4.8.1

## 0.11.0
* Fix roundtrip bug

## 0.12.0
* Add support for Azure integrated auth
* Also target .NET Standard 2.1
* Add `LoadAllAsync` that returns `IAsyncEnumerable` when targeting .NET Standard 2.1
* Remove methods to be consistent with regards to the use of "Async"

## 0.13.0
* Add ability to easily specify SQL types with precision

## 0.14.0
* Enable customization of data type and sproc names
* Even faster extraction of query parameter values
* Fix bug that would sometimes result in truncating string values

## 0.15.0
* Fix nullable-value-type-in-ctor edge case

## 0.16.0
* Replace System.Data.SqlClient dep with Microsoft.Data.SqlClient
* Add overloads of all methods on `UpsertHelper` that accept an `SqlConnection` and an optional `SqlTransaction` from the outside, enabling enlisting multiple operations in the same transaction
* Remove .NET Standard 2.0 target because it was in the way

## 0.17.0
* Cuddle NuGet package manifest a little bit

[FastMember]: https://github.com/mgravell/fast-member