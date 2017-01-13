# Changelog

## 0.0.1

* First version

## 0.1.0

* Ability to `LoadWhere` and specify criteria on the form `[SomeProperty] = @someValue` accompanied by arguments on the form `new { someValue = "hej" }`
* Ability to `DeleteWhere` in the same way as `LoadWhere`
* Use the excellent [FastMember] to extract property values from objects to be even faster

[FastMember]: https://github.com/mgravell/fast-member