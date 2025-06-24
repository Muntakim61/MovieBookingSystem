using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MovieBookingSystem.Services
{
    public class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!context.Actors.Any())
            {
                var actors = new List<Actor>
                {
                    new Actor { Name = "Robert Downey Jr.", Biography = "Famous for Iron Man.", DateOfBirth = new DateTime(1965, 4, 4), ImageUrl = "/images/robert.jpg" },
                    new Actor { Name = "Scarlett Johansson", Biography = "Famous for Black Widow.", DateOfBirth = new DateTime(1984, 11, 22), ImageUrl = "/images/scarlett.jpg" },
                    new Actor { Name = "Hugh Jackman", Biography = "Famous for Wolverine.", DateOfBirth = new DateTime(1968, 10, 12), ImageUrl = "/images/hugh.jpg" },
                    new Actor { Name = "Sam Worthington", Biography = "Lead in Avatar.", DateOfBirth = new DateTime(1976, 8, 2), ImageUrl = "/images/sam.jpg" }
                };
                context.Actors.AddRange(actors);
                await context.SaveChangesAsync();
            }

            if (!context.Directors.Any())
            {
                var directors = new List<Director>
                {
                    new Director { Name = "Christopher Nolan", Biography = "Known for Inception.", DateOfBirth = new DateTime(1970, 7, 30), ImageUrl = "/images/nolan.jpg" },
                    new Director { Name = "James Cameron", Biography = "Known for Avatar.", DateOfBirth = new DateTime(1954, 8, 16), ImageUrl = "/images/cameron.jpg" },
                    new Director { Name = "Anthony Russo", Biography = "Known for Marvel films.", DateOfBirth = new DateTime(1970, 2, 3), ImageUrl = "/images/russo.jpg" }
                };
                context.Directors.AddRange(directors);
                await context.SaveChangesAsync();
            }

            if (!context.Halls.Any())
            {
                var halls = new List<Hall>
                {
                    new Hall { Name = "Main Hall", Capacity = 200, Location = "Building A" },
                    new Hall { Name = "VIP Hall", Capacity = 100, Location = "Building B" }
                };
                context.Halls.AddRange(halls);
                await context.SaveChangesAsync();
            }

            if (!context.Movies.Any())
            {
                var nolan = context.Directors.FirstOrDefault(d => d.Name == "Christopher Nolan");
                var cameron = context.Directors.FirstOrDefault(d => d.Name == "James Cameron");
                var russo = context.Directors.FirstOrDefault(d => d.Name == "Anthony Russo");

                var movies = new List<Movie>
                {
                    new Movie
                    {
                        Title = "Inception",
                        Genre = "Sci-Fi",
                        ReleaseDate = new DateTime(2010, 7, 16),
                        Duration = 148,
                        Price = 12.99m,
                        ImageUrl = "/images/inception.jpg",
                        DirectorId = nolan?.DirectorId
                    },
                    new Movie
                    {
                        Title = "The Prestige",
                        Genre = "Drama",
                        ReleaseDate = new DateTime(2006, 10, 20),
                        Duration = 130,
                        Price = 10.99m,
                        ImageUrl = "/images/prestige.jpg",
                        DirectorId = nolan?.DirectorId
                    },
                    new Movie
                    {
                        Title = "Avatar",
                        Genre = "Sci-Fi",
                        ReleaseDate = new DateTime(2009, 12, 18),
                        Duration = 162,
                        Price = 13.99m,
                        ImageUrl = "/images/avatar.jpg",
                        DirectorId = cameron?.DirectorId
                    },
                    new Movie
                    {
                        Title = "Avengers: Endgame",
                        Genre = "Action",
                        ReleaseDate = new DateTime(2019, 4, 26),
                        Duration = 181,
                        Price = 14.99m,
                        ImageUrl = "/images/endgame.jpg",
                        DirectorId = russo?.DirectorId
                    }
                };

                context.Movies.AddRange(movies);
                await context.SaveChangesAsync();
            }

            if (!context.Set<MovieActor>().Any())
            {
                var inception = context.Movies.FirstOrDefault(m => m.Title == "Inception");
                var prestige = context.Movies.FirstOrDefault(m => m.Title == "The Prestige");
                var avatar = context.Movies.FirstOrDefault(m => m.Title == "Avatar");
                var endgame = context.Movies.FirstOrDefault(m => m.Title == "Avengers: Endgame");

                var robert = context.Actors.FirstOrDefault(a => a.Name == "Robert Downey Jr.");
                var scarlett = context.Actors.FirstOrDefault(a => a.Name == "Scarlett Johansson");
                var hugh = context.Actors.FirstOrDefault(a => a.Name == "Hugh Jackman");
                var sam = context.Actors.FirstOrDefault(a => a.Name == "Sam Worthington");

                var movieActors = new List<MovieActor>
                {
                    new MovieActor { MovieId = inception!.MovieId, ActorId = robert!.ActorId },
                    new MovieActor { MovieId = endgame!.MovieId, ActorId = robert!.ActorId },
                    new MovieActor { MovieId = endgame!.MovieId, ActorId = scarlett!.ActorId },
                    new MovieActor { MovieId = prestige!.MovieId, ActorId = hugh!.ActorId },
                    new MovieActor { MovieId = avatar!.MovieId, ActorId = sam!.ActorId }
                };

                context.AddRange(movieActors);
                await context.SaveChangesAsync();
            }
        }
    }
}
