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

## 0.18.0
* Add ability to output schema generation scripts

## 0.19.0
* Fix ability to specify precision of `DateTimeOffset` properties

## 0.20.0
* Add abilty to query by a dictionary of args and not only anonymous objects. This is useful in cases where the arguments vary, e.g. when implementing query builders and stuff
* Update Microsoft.Data.SqlClient to 2.1.1

## 0.21.0
* Update Microsoft.Data.SqlClient to 3.0.0
* Update Microsoft.Azure.Services.AppAuthentication to 1.6.1
* Tolerate `Authentication=Active Directory Integrated` in the connection string to support managed identity

## 0.22.0
* Target .NET Standard 2.0 because it's the right thing to do

## 0.23.0
* Update Microsoft.Data.SqlClient to 4.0.0

## 0.24.0
* Update Microsoft.Data.SqlClient to 5.0.1

## 0.25.0
* Update Microsoft.Data.SqlClient to 5.1.2

## 0.26.0
* Fix package metadata

## 0.27.0
* Fix bug that would choke on passwords containing an equals sign

## 0.28.0
* Update Microsoft.Data.SqlClient to 5.2.2
* Add additional compilation targets because why not


[FastMember]: https://github.com/mgravell/fast-member