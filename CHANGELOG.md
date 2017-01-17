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

[FastMember]: https://github.com/mgravell/fast-member