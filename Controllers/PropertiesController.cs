using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Flat.Data;
using Flat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flat.Controllers;

public class PropertiesController(ApplicationDbContext database, IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 50;
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? listingType, int page = 1)
    {
        var query = database.PropertyListings.AsNoTracking().Include(item => item.Photos).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(item => item.Title.Contains(searchTerm) || item.City.Contains(searchTerm)
                || item.Locality.Contains(searchTerm) || item.Address.Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(listingType))
            query = query.Where(item => item.ListingType == listingType);
        query = query.OrderByDescending(item => item.CreatedAtUtc);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var model = new PropertySearchViewModel
        {
            Listings = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            SearchTerm = searchTerm,
            ListingType = listingType,
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View("Browse", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var listing = await database.PropertyListings.AsNoTracking().Include(item => item.Photos)
            .FirstOrDefaultAsync(item => item.Id == id);
        return listing is null ? NotFound() : View(listing);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var listings = await database.PropertyListings.AsNoTracking()
            .Include(item => item.Photos)
            .Where(item => item.OwnerId == ownerId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync();
        return View(listings);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create() => View(new PropertyListing());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,PropertyType,ListingType,City,Locality,Address,MapLink,Price,Deposit,Bedrooms,Bathrooms,AreaSqFt,Furnishing,ContactPhone")] PropertyListing listing,
        IFormFile[]? photos, CancellationToken cancellationToken)
    {
        var selectedPhotos = photos ?? Array.Empty<IFormFile>();
        await ValidatePhotos(selectedPhotos, 0, cancellationToken);
        if (!ModelState.IsValid)
            return View(listing);

        listing.OwnerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        listing.CreatedAtUtc = DateTime.UtcNow;
        await SavePhotos(listing, selectedPhotos, cancellationToken);
        database.PropertyListings.Add(listing);
        await database.SaveChangesAsync();
        TempData["SuccessMessage"] = "Your property is posted and visible on the homepage.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var listing = await OwnedListing(id, includePhotos: true);
        return listing is null ? NotFound() : View(listing);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Title,Description,PropertyType,ListingType,City,Locality,Address,MapLink,Price,Deposit,Bedrooms,Bathrooms,AreaSqFt,Furnishing,ContactPhone")] PropertyListing input,
        IFormFile[]? photos, CancellationToken cancellationToken)
    {
        var listing = await OwnedListing(id, includePhotos: true);
        if (listing is null)
            return NotFound();
        var selectedPhotos = photos ?? Array.Empty<IFormFile>();
        await ValidatePhotos(selectedPhotos, listing.Photos.Count, cancellationToken);
        if (!ModelState.IsValid)
            return View(input);

        listing.Title = input.Title;
        listing.Description = input.Description;
        listing.PropertyType = input.PropertyType;
        listing.ListingType = input.ListingType;
        listing.City = input.City;
        listing.Locality = input.Locality;
        listing.Address = input.Address;
        listing.MapLink = input.MapLink;
        listing.Price = input.Price;
        listing.Deposit = input.Deposit;
        listing.Bedrooms = input.Bedrooms;
        listing.Bathrooms = input.Bathrooms;
        listing.AreaSqFt = input.AreaSqFt;
        listing.Furnishing = input.Furnishing;
        listing.ImageUrl = input.ImageUrl;
        listing.ContactPhone = input.ContactPhone;
        await SavePhotos(listing, selectedPhotos, cancellationToken);
        await database.SaveChangesAsync();
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await OwnedListing(id, includePhotos: true);
        if (listing is not null)
        {
            foreach (var photo in listing.Photos)
                DeletePhotoFile(photo.ImagePath);
            database.PropertyListings.Remove(listing);
            await database.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Mine));
    }

    private Task<PropertyListing?> OwnedListing(int id, bool includePhotos = false)
    {
        var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var query = database.PropertyListings.AsQueryable();
        if (includePhotos)
            query = query.Include(item => item.Photos);
        return query.FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);
    }

    private async Task ValidatePhotos(IFormFile[] photos, int existingCount, CancellationToken cancellationToken)
    {
        if (photos.Length + existingCount > 5)
            ModelState.AddModelError("photos", "You can add up to five photos per property.");

        foreach (var photo in photos)
        {
            if (photo.Length > MaxPhotoBytes || await GetPhotoExtension(photo, cancellationToken) is null)
                ModelState.AddModelError("photos", "Choose JPEG, PNG or WebP photo files under 5 MB each. Videos are not accepted.");
        }
    }

    private async Task SavePhotos(PropertyListing listing, IFormFile[] photos, CancellationToken cancellationToken)
    {
        if (photos.Length == 0)
            return;

        var uploadDirectory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadDirectory);

        foreach (var photo in photos)
        {
            var extension = await GetPhotoExtension(photo, cancellationToken);
            if (extension is null)
                continue;

            var fileName = $"{Guid.NewGuid():N}{extension}";
            await using var destination = System.IO.File.Create(Path.Combine(uploadDirectory, fileName));
            await photo.CopyToAsync(destination, cancellationToken);
            listing.Photos.Add(new PropertyPhoto { ImagePath = $"/uploads/{fileName}" });
        }
    }

    private static async Task<string?> GetPhotoExtension(IFormFile photo, CancellationToken cancellationToken)
    {
        if (photo.Length == 0 || photo.Length > MaxPhotoBytes)
            return null;

        var header = new byte[12];
        await using var stream = photo.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ".jpg";
        if (bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            return ".png";
        if (bytesRead >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return ".webp";
        return null;
    }

    private void DeletePhotoFile(string imagePath)
    {
        if (!imagePath.StartsWith("/uploads/", StringComparison.Ordinal))
            return;
        var path = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", Path.GetFileName(imagePath));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }
}