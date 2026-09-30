using System.ComponentModel.DataAnnotations;

namespace Flat.Models;

public class PropertyPhoto
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }

    [Required, StringLength(300)]
    public string ImagePath { get; set; } = string.Empty;

    public PropertyListing PropertyListing { get; set; } = null!;
}