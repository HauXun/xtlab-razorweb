using System;
using Bogus;
using App.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class initdb : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "articles",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            Created = table.Column<DateTime>(type: "datetime2", nullable: false),
            Content = table.Column<string>(type: "ntext", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_articles", x => x.Id);
          });

      Randomizer.Seed = new Random(8675309);
      var fakerArticle = new Faker<Article>();
      fakerArticle.RuleFor(a => a.Title, f => f.Lorem.Sentence(5, 5));
      fakerArticle.RuleFor(a => a.Created, f => f.Date.Between(new DateTime(2022, 2, 1), new DateTime(2022, 2, 28)));
      fakerArticle.RuleFor(a => a.Content, f => f.Lorem.Paragraphs(1, 4));

      for (int i = 0; i < 150; i++)
      {
        Article article = fakerArticle.Generate();

        migrationBuilder.InsertData(
            table: "articles",
            columns: new[] { "Title", "Created", "Content" },
            values: new object[] {
              article.Title,
              article.Created,
              article.Content
            }
        );
      }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "articles");
    }
  }
}
