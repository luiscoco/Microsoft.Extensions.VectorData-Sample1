# Introduction to Microsoft.Extenstions.VectorData

This sample followed the instructions detailed in the following URL:

https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-vector-data/

See also this youtube video: 

https://www.youtube.com/watch?v=31JKIxV-Y0g

## 1. How to run an Ollama Docker container

```
docker run -d -v ollama:/root/.ollama -p 11434:11434 --name ollama ollama/ollama
```

## 2. How to install the in the Ollama Docker container

Access the Running Container:  First, ensure your Ollama container is running. If it's named ollama, you can access its shell using:

```
docker exec -it ollama /bin/sh
```

Now we have to intall the **all-minilm** Model inside the container. We use the ollama pull command to download the model:

```
ollama pull all-minilm
```

we verify the installation

```
ollama list
```

We also verify the Ollama docker container is running in **Docker Desktop**

![image](https://github.com/user-attachments/assets/06919925-3e42-4994-9043-88121cb664ea)

![image](https://github.com/user-attachments/assets/e0f45259-a3a8-44e7-838d-f65999a2b15c)

## 3. Create a C# .NET 9 console application



## 4. Load the Nuget packages

![image](https://github.com/user-attachments/assets/119ef35b-89d6-4e83-8693-458389463089)

## 5. We create the data model

![image](https://github.com/user-attachments/assets/7018dd25-8c3f-4233-91e7-87fd8c7a1494)

```csharp
using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftExtenstionsVectorData
{
    public class Movie
    {
        [VectorStoreRecordKey]
        public int Key { get; set; }

        [VectorStoreRecordData]
        public string Title { get; set; }

        [VectorStoreRecordData]
        public string Description { get; set; }

        [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}
```

##  6. We define the Program.cs

```csharp
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
```

## 7. We run the application and see the results

![image](https://github.com/user-attachments/assets/53598141-ed61-4998-bb6c-ed1b801cd777)
