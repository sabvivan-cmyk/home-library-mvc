using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using WebApplicationMVC.Data;
using WebApplicationMVC.Helpers;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers;

public class BooksController : Controller
{
    private readonly BookRepository _repository;

    public BooksController(
        BookRepository repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        var books =
            await _repository.GetAllAsync();

        return View(books);
    }

    public async Task<IActionResult> Details(int id)
    {
        var book =
            await _repository.GetByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        book.TableOfContentsHtml =
            TableOfContentsHelper.FromXml(
                book.TableOfContents
            );

        return View(book);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Book book,
        IFormFile? tableOfContentsFile)
    {
        if (!ModelState.IsValid)
        {
            return View(book);
        }

        if (tableOfContentsFile is not null &&
            tableOfContentsFile.Length > 0)
        {
            using var reader = new StreamReader(
                tableOfContentsFile.OpenReadStream());

            var xml = await reader.ReadToEndAsync();

            var document = XDocument.Parse(xml);

            document.Declaration = null;

            book.TableOfContents =
                document.ToString();
        }

        var id =
            await _repository.InsertAsync(book);

        return RedirectToAction(
            nameof(Details),
            new { id }
        );
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var book =
            await _repository.GetByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        book.TableOfContentsHtml =
            TableOfContentsHelper.FromXml(
                book.TableOfContents
            );

        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Book book)
    {
        if (!ModelState.IsValid)
        {
            return View(book);
        }

        var existingBook =
            await _repository.GetByIdAsync(book.Id);

        if (existingBook is null)
        {
            return NotFound();
        }

        book.TableOfContents =
            TableOfContentsHelper.SaveEditedHtml(
                existingBook.TableOfContents,
                book.TableOfContentsHtml
            );

        await _repository.UpdateAsync(book);

        return RedirectToAction(
            nameof(Details),
            new { id = book.Id }
        );
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var book =
            await _repository.GetByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        await _repository.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> XmlData(int id)
    {
        var book = await _repository.GetByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        ViewBag.Book = book;

        var values =
            await _repository.GetXmlValuesAsync(id);

        return View(values);
    }

    [HttpGet]
    public async Task<IActionResult> XmlSearch(
    string? searchText)
    {
        var books = new List<Book>();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            books =
                await _repository.FindByXmlTextAsync(
                    searchText
                );
        }

        ViewBag.SearchText = searchText;

        return View(books);
    }
}