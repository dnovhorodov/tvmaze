using FizzWare.NBuilder;
using TvMaze.Persistence;
using TvMaze.Service.TvMaze;

namespace TvMaze.Tests;

public class TvShowModelTest
{
    private static readonly RandomGenerator _randomGen = new ();

    [Fact]
    public void GivenCollectionOfCastsTheyShouldBeOrderedByBirthdayInDescendingOrder()
    {
        var actual = CreateDbShowModel();
        var expected = actual.Casts!
            .Select(c => new Service.TvMaze.Cast()
            {
                Id = c.Id,
                Name = c.Name,
                Birthday = ToDateOnly(c.Birthday),
            })
            .OrderByDescending(c => c.Birthday);

        var result = new[] { actual }.ToApiModel();

        Assert.True(result.Single().Cast!.SequenceEqual(expected));
    }

    private static TvShowDbModel CreateDbShowModel()
    {
        return Builder<TvShowDbModel>
            .CreateNew()
            .With(m => m.Casts =
                Builder<Persistence.Cast>.CreateListOfSize(10)
                    .All()
                    .With(c => c.Birthday = GetRandomBirthday())
                    .Build()
                    .ToList()).Build();
    }

    private static DateTime? GetRandomBirthday()
        => new Random().NextDouble() > 0.3 ?
            _randomGen.Next(new DateTime(1950, 1, 1), new DateTime(2010, 12, 31)) : null;

    private static DateOnly? ToDateOnly(DateTime? dt)
        => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null;
}
