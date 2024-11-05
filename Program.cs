using System.Collections.ObjectModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using MicrosoftExtenstionsVectorData;


#pragma warning disable
var movieData = new List<Movie>()
{
    new Movie
        {
            Key=0,
            Title="Lion King",
            Description="The Lion King is a classic Disney animated film that tells the story of a young lion named Simba who embarks on a journey to reclaim his throne as the king of the Pride Lands after the tragic death of his father."
        },
    new Movie
        {
            Key=1,
            Title="Inception",
            Description="Inception is a science fiction film directed by Christopher Nolan that follows a group of thieves who enter the dreams of their targets to steal information."
        },
    new Movie
        {
            Key=2,
            Title="The Matrix",
            Description="The Matrix is a science fiction film directed by the Wachowskis that follows a computer hacker named Neo who discovers that the world he lives in is a simulated reality created by machines."
        },
    new Movie
        {
            Key=3,
            Title="Shrek",
            Description="Shrek is an animated film that tells the story of an ogre named Shrek who embarks on a quest to rescue Princess Fiona from a dragon and bring her back to the kingdom of Duloc."
        }
};

var vectorStore = new InMemoryVectorStore();

var movies = vectorStore.GetCollection<int, Movie>("movies");

await movies.CreateCollectionIfNotExistsAsync();

IEmbeddingGenerator<string, Embedding<float>> generator =
    new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "all-minilm");

foreach (var movie in movieData)
{
    movie.Vector = await generator.GenerateEmbeddingVectorAsync(movie.Description);
    await movies.UpsertAsync(movie);
}

var query = "A family friendly movie";
var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);

var searchOptions = new VectorSearchOptions()
{
    Top = 2,
    VectorPropertyName = "Vector"
};

var results = await movies.VectorizedSearchAsync(queryEmbedding, searchOptions);

await foreach (var result in results.Results)
{
    Console.WriteLine($"Title: {result.Record.Title}");
    Console.WriteLine($"Description: {result.Record.Description}");
    Console.WriteLine($"Score: {result.Score}");
    Console.WriteLine();
}
