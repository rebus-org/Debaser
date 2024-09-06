using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Debaser.Attributes;
using Newtonsoft.Json;
using NUnit.Framework;
// ReSharper disable MemberCanBeProtected.Local

namespace Debaser.Tests.Corners;

[TestFixture]
public class WannabeDocDb : FixtureBase
{
    [Test]
    public async Task CanDoIt()
    {
        var upsertHelper = new UpsertHelper<TeamRow>(ConnectionString);

        upsertHelper.DropSchema(dropType: true, dropTable: true, dropProcedure: true);
        upsertHelper.CreateSchema();

        await upsertHelper.UpsertAsync([
            new TeamRow(new Team("team1", "Team c0ffeebada55", ["t-team1-1", "t-team1-2"])),
            new TeamRow(new Team("team2", "Team c0ffeebada55", ["t-team2-1"])),
            new TeamRow(new Team("team3", "Team c0ffeebada55", ["t-team3-1", "t-team3-2", "t-team3-3"])),
            new TeamRow(new Team("team4", "Team c0ffeebada55", Enumerable.Range(1, 200).Select(n => $"t-team4-{n}").ToList())),
            new TeamRow(new Team("team5", "Team c0ffeebada55", ["t-team5-1", "t-team5-2"]))
        ]);

        var team = (await upsertHelper.LoadWhereAsync("[AccountIds] like '%;t-team4-200;%'")).FirstOrDefault();

        Assert.That(team, Is.Not.Null);

        Assert.That(team.Id, Is.EqualTo("team4"));
        Assert.That(team.Entity.AccountIds.Count, Is.EqualTo(200));
    }

    class Doc<TEntity>
    {
        protected Doc(TEntity entity)
        {
            Entity = entity;
        }

        [DebaserMapper(typeof(JsonMapper<Team>))]
        public TEntity Entity { get; }

        protected string GetList(List<string> values) => $";{string.Join(";", values.Select(value => value))};";
    }

    class TeamRow : Doc<Team>
    {
        public TeamRow(Team entity) : base(entity)
        {
        }

        public string Id => Entity.Id;

        [DebaserSqlType(SqlDbType.NVarChar, int.MaxValue)]
        public string AccountIds => GetList(Entity.AccountIds);
    }

    class Team
    {
        public string Id { get; }
        public string Name { get; }
        public List<string> AccountIds { get; }

        public Team(string id, string name, List<string> accountIds)
        {
            Id = id;
            Name = name;
            AccountIds = accountIds;
        }
    }

    class JsonMapper<T> : IDebaserMapper
    {
        public SqlDbType SqlDbType => SqlDbType.NVarChar;
        public int? SizeOrNull => int.MaxValue;
        public int? AdditionalSizeOrNull => null;

        public object ToDatabase(object arg) => JsonConvert.SerializeObject(arg);

        public object FromDatabase(object arg) => JsonConvert.DeserializeObject<T>((string)arg);
    }
}