// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

public class Programs
{
    public static async Task Main(string[] args)
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("books");
        var client = new ElasticClient(settings);

        // 1. Elasticsearch'de "books" indeksini oluştur
        var createIndexResponse = await client.Indices.CreateAsync("books", c => c
            .Map<Book>(m => m.AutoMap())
        );

        // 2. Kitapları ekle
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "The Catcher in the Rye", Author = "J.D. Salinger" },
            new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee" },
            new Book { Id = 3, Title = "1984", Author = "George Orwell" },
        };

        foreach (var book in books)
        {
            await client.IndexDocumentAsync(book);
        }

        Console.WriteLine("Kitaplar Elasticsearch'e eklendi!");

        // 3. Arama işlemi yap
        Console.Write("Aramak istediğiniz kitap veya yazar: ");
        var query = Console.ReadLine();

        var searchResponse = await client.SearchAsync<Book>(s => s
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f.Field(b => b.Title).Field(b => b.Author))
                    .Query(query)
                )
            )
        );

        Console.WriteLine("Arama Sonuçları:");
        foreach (var hit in searchResponse.Hits)
        {
            Console.WriteLine($"- {hit.Source.Title} ({hit.Source.Author})");
        }
    }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
}