using System.ComponentModel.DataAnnotations;

namespace WebApplicationMVC.Models;

public class XmlValue
{
    public string NodeName { get; set; } = string.Empty;
    public string? NodeValue { get; set; }
}