using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Flat.Models;

public class PropertyListing
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string PropertyType { get; set; } = "Flat";

    [Required, StringLength(12)]
    public string ListingType { get; set; } = "Rent";

    [Required, StringLength(80)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Locality { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(500), Url]
    public string? MapLink { get; set; }

    [Precision(18, 2), Range(1, 1000000000)]
    public decimal Price { get; set; }

    [Precision(18, 2), Range(0, 1000000000)]
    public decimal Deposit { get; set; }

    [Range(0, 30)]
    public int Bedrooms { get; set; }

    [Range(0, 30)]
    public int Bathrooms { get; set; }

    [Range(0, 100000)]
    public int AreaSqFt { get; set; }

    [Required, StringLength(30)]
    public string Furnishing { get; set; } = "Unfurnished";

    [StringLength(500), Url]
    public string? ImageUrl { get; set; }

    [Required, Phone, StringLength(30)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    [ValidateNever]
    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ValidateNever]
    public ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
}