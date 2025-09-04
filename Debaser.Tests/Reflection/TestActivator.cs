using Debaser.Internals.Values;
using Activator = Debaser.Internals.Reflection.Activator;

namespace Debaser.Tests.Reflection;

[TestFixture]
public class TestActivator : FixtureBase
{
    [Test]
    public void SkipsPropertyWhenNotIncluded_Constructor()
    {
        var activator = new Activator(typeof(SomeClassWithConstructor), [nameof(SomeClassWithConstructor.Number)]);

        var values = new TestValueLookup(new Dictionary<string, object>
        {
            {nameof(SomeClassWithConstructor.Number), 123 },
            {nameof(SomeClassWithConstructor.Text), "HELLO" },
        });

        var instance = (SomeClassWithConstructor)activator.CreateInstance(values);

        Assert.That(instance.Number, Is.EqualTo(123));
        Assert.That(instance.Text, Is.EqualTo(null));
    }

    [Test]
    public void CanCreateClassFromConstructor()
    {
        var activator = new Activator(typeof(SomeClassWithConstructor), [nameof(SomeClassWithConstructor.Number), nameof(SomeClassWithConstructor.Text)
        ]);

        var values = new TestValueLookup(new Dictionary<string, object>
        {
            {nameof(SomeClassWithConstructor.Number), 123 },
            {nameof(SomeClassWithConstructor.Text), "HELLO" },
        });

        var instance = (SomeClassWithConstructor)activator.CreateInstance(values);

        Assert.That(instance.Number, Is.EqualTo(123));
        Assert.That(instance.Text, Is.EqualTo("HELLO"));
    }

    class SomeClassWithConstructor(decimal number, string text)
    {
        public decimal Number { get; } = number;
        public string Text { get; } = text;
    }

    [Test]
    public void SkipsPropertyWhenNotIncluded_Properties()
    {
        var activator = new Activator(typeof(SomeClassWithProperties), [nameof(SomeClassWithProperties.Number)]);

        var values = new TestValueLookup(new Dictionary<string, object>
        {
            {nameof(SomeClassWithProperties.Number), 123 },
            {nameof(SomeClassWithProperties.Text), "HELLO" },
        });

        var instance = (SomeClassWithProperties)activator.CreateInstance(values);

        Assert.That(instance.Number, Is.EqualTo(123));
        Assert.That(instance.Text, Is.EqualTo(null));
    }

    [Test]
    public void CanCreateClassFromProperties()
    {
        var activator = new Activator(typeof(SomeClassWithProperties), [nameof(SomeClassWithProperties.Number), nameof(SomeClassWithProperties.Text)
        ]);

        var values = new TestValueLookup(new Dictionary<string, object>
        {
            {nameof(SomeClassWithProperties.Number), 123 },
            {nameof(SomeClassWithProperties.Text), "HELLO" },
        });

        var instance = (SomeClassWithProperties)activator.CreateInstance(values);

        Assert.That(instance.Number, Is.EqualTo(123));
        Assert.That(instance.Text, Is.EqualTo("HELLO"));
    }

    class SomeClassWithProperties
    {
        public decimal Number { get; set; }
        public string Text { get; set; }
    }

    class TestValueLookup(Dictionary<string, object> values) : IValueLookup
    {
        readonly Dictionary<string, object> _values = new(values, StringComparer.InvariantCultureIgnoreCase);

        public object GetValue(string name, Type desiredType)
        {
            try
            {
                var value = _values[name];

                return Convert.ChangeType(value, desiredType);
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Could not find value with key '{name}' in the values dictionary", exception);
            }
        }
    }
}