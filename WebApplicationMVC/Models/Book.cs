using System.ComponentModel.DataAnnotations;

namespace WebApplicationMVC.Models;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название")]
    [Display(Name = "Название")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите автора")]
    [Display(Name = "Автор")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Год издания")]
    public int? YearPublished { get; set; }

    public string? ISBN { get; set; }

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    public string? TableOfContents { get; set; }

    [Display(Name = "Оглавление")]
    public string? TableOfContentsHtml { get; set; }
}