using System.Net;
using System.Text;
using System.Xml.Linq;

namespace WebApplicationMVC.Helpers;

public static class TableOfContentsHelper
{
    public static string? FromXml(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        try
        {
            var document = XDocument.Parse(xml);

            if (document.Root == null)
            {
                return null;
            }

            // Если XML уже прошёл через редактор,
            // возвращаем сохранённый HTML.
            if (document.Root.Name.LocalName == "TableOfContents")
            {
                var htmlElement = document.Root
                    .Elements()
                    .FirstOrDefault(x => x.Name.LocalName == "Html");

                if (htmlElement != null)
                {
                    return htmlElement.Value;
                }

                var sourceElement = document.Root
                    .Elements()
                    .FirstOrDefault(x => x.Name.LocalName == "Source");

                var sourceRoot = sourceElement?
                    .Elements()
                    .FirstOrDefault();

                if (sourceRoot != null)
                {
                    return BuildHtml(sourceRoot);
                }
            }

            // Обычный загруженный XML.
            return BuildHtml(document.Root);
        }
        catch
        {
            return null;
        }
    }

    public static string? SaveEditedHtml(
        string? originalXml,
        string? html)
    {
        if (string.IsNullOrWhiteSpace(originalXml))
        {
            return null;
        }

        var document = XDocument.Parse(originalXml);

        if (document.Root == null)
        {
            return null;
        }

        // XML уже находится в нашей служебной обёртке.
        if (document.Root.Name.LocalName == "TableOfContents")
        {
            var htmlElement = document.Root
                .Elements()
                .FirstOrDefault(x => x.Name.LocalName == "Html");

            if (htmlElement == null)
            {
                htmlElement = new XElement("Html");
                document.Root.Add(htmlElement);
            }

            htmlElement.RemoveNodes();

            htmlElement.Add(
                new XCData(html ?? string.Empty)
            );

            return document.ToString();
        }

        // Первый раз редактируем загруженный XML.
        // Исходную структуру переносим целиком в Source.
        var originalRoot = new XElement(document.Root);

        var result = new XDocument(
            new XElement(
                "TableOfContents",

                new XElement(
                    "Source",
                    originalRoot
                ),

                new XElement(
                    "Html",
                    new XCData(html ?? string.Empty)
                )
            )
        );

        return result.ToString();
    }

    private static string BuildHtml(XElement root)
    {
        var html = new StringBuilder();

        RenderElement(root, html, 0);

        return html.ToString();
    }

    private static void RenderElement(
        XElement element,
        StringBuilder html,
        int level)
    {
        var values = new List<string>();

        // Namespace xmlns:* в оглавление не выводим.
        foreach (var attribute in element.Attributes())
        {
            if (attribute.IsNamespaceDeclaration)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Value))
            {
                values.Add(attribute.Value.Trim());
            }
        }

        var directText = element
            .Nodes()
            .OfType<XText>()
            .Select(x => x.Value.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x));

        values.AddRange(directText);

        if (values.Count > 0)
        {
            html.Append(
                $"<div class=\"toc-line toc-level-{Math.Min(level, 5)}\">"
            );

            html.Append(
                string.Join(
                    " ",
                    values.Select(WebUtility.HtmlEncode)
                )
            );

            html.Append("</div>");
        }

        foreach (var child in element.Elements())
        {
            RenderElement(
                child,
                html,
                level + 1
            );
        }
    }
}