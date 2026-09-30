using System.Collections.Generic;

namespace Flat.Models;

public class PropertySearchViewModel
{
    public IReadOnlyList<PropertyListing> Listings { get; set; } = [];
    public string? SearchTerm { get; set; }
    public string? ListingType { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}