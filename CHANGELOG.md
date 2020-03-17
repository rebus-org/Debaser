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


[FastMember]: https://github.com/mgravell/fast-member