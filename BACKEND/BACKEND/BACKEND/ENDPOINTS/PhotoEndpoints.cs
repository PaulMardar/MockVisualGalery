using BACKEND.DOMAIN;
using BACKEND.DOMAIN.Objects;
using BACKEND.SERVICES;
using BACKEND.SERVICES.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BACKEND.ENDPOINTS
{
    public static class PhotoEndpoints
    {
        // authEnabled comes from the "Auth:Enabled" config switch (see Program.cs).
        // Unlike UserEndpoints, nothing in this group needs to stay anonymous -
        // photos are only reachable once you're logged in.
        public static IEndpointRouteBuilder MapPhotoEndpoints(this IEndpointRouteBuilder app, bool authEnabled)
        {
            var group = app.MapGroup("/api/photos").WithTags("Photos");

            if (authEnabled)
                group.RequireAuthorization();

            // POST /api/photos/upload -> PhotoService.Upload
            // "Content" in the request body is a base64 string that System.Text.Json
            // decodes into a byte[] automatically.
            group.MapPost("/upload", async ([FromForm] UploadPhotoForm form, IPhotoService photoService) =>
            {
                if (form.File is null || form.File.Length == 0)
                    return Results.BadRequest(new { error = "A file must be provided." });

                try
                {
                    using var ms = new MemoryStream();
                    await form.File.CopyToAsync(ms);
                    var content = ms.ToArray();

                    var tags = string.IsNullOrWhiteSpace(form.Tags)
                        ? new List<string>()
                        : form.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                   .ToList();

                    var photo = photoService.Upload(form.File.FileName, (form.Length, form.Width), tags, content);
                    return Results.Created($"/api/photos/{photo.Id}", ToDto(photo));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .DisableAntiforgery()   // this is a JSON/multipart API, not a browser form post
            .WithName("UploadPhoto")
            .WithSummary("Uploads a new photo as multipart/form-data")
            .Accepts<UploadPhotoForm>("multipart/form-data")
            .Produces<PhotoDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

            // POST /api/photos -> PhotoService.Add (raw passthrough, no validation)
            group.MapPost("/", (CreatePhotoRequest request, IPhotoService photoService) =>
            {
                var photo = new Photo
                {
                    Id = request.Id,
                    Name = request.Name,
                    Extension = request.Extension,
                    Dimensions = new Dimensions { Length = request.Length, Width = request.Width },
                    Tags = request.Tags,
                    Content = request.Content,
                    OwnerId = request.OwnerId
                };
                var created = photoService.Add(photo);
                return Results.Created($"/api/photos/{created.Id}", ToDto(created));
            })
            .WithName("AddPhoto")
            .WithSummary("Adds a photo directly, bypassing Upload's validation")
            .Produces<PhotoDto>(StatusCodes.Status201Created);

            // GET /api/photos -> PhotoService.GetAll
            group.MapGet("/", (IPhotoService photoService) =>
            {
                var photos = photoService.GetAll().Select(ToDto);
                return Results.Ok(photos);
            })
            .WithName("GetAllPhotos")
            .WithSummary("Returns every photo")
            .Produces<IEnumerable<PhotoDto>>(StatusCodes.Status200OK);

            // GET /api/photos/{id} -> PhotoService.GetById
            group.MapGet("/{id:int}", (int id, IPhotoService photoService) =>
            {
                var photo = photoService.GetById(id);
                return photo is null ? Results.NotFound() : Results.Ok(ToDto(photo));
            })
            .WithName("GetPhotoById")
            .WithSummary("Returns a single photo's metadata by id")
            .Produces<PhotoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // GET /api/photos/{id}/content -> PhotoService.GetById (raw bytes)
            group.MapGet("/{id:int}/content", (int id, IPhotoService photoService) =>
            {
                var photo = photoService.GetById(id);
                if (photo is null)
                    return Results.NotFound();

                var contentType = photo.Extension.ToLowerInvariant() switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    "gif" => "image/gif",
                    "webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                return Results.File(photo.Content, contentType, $"{photo.Name}.{photo.Extension}");
            })
            .WithName("GetPhotoContent")
            .WithSummary("Downloads the raw bytes of a photo")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST /api/photos/{id}/tags -> PhotoService.AddTag
            group.MapPost("/{id:int}/tags", (int id, TagRequest request, IPhotoService photoService) =>
            {
                var success = photoService.AddTag(id, request.Tag);
                return success ? Results.NoContent() : Results.NotFound();
            })
            .WithName("AddPhotoTag")
            .WithSummary("Adds a tag to a photo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE /api/photos/{id}/tags/{tag} -> PhotoService.RemoveTag
            group.MapDelete("/{id:int}/tags/{tag}", (int id, string tag, IPhotoService photoService) =>
            {
                var success = photoService.RemoveTag(id, tag);
                return success ? Results.NoContent() : Results.NotFound();
            })
            .WithName("RemovePhotoTag")
            .WithSummary("Removes a tag from a photo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE /api/photos/{id} -> PhotoService.Delete
            group.MapDelete("/{id:int}", (int id, IPhotoService photoService) =>
            {
                var deleted = photoService.Delete(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeletePhoto")
            .WithSummary("Deletes a photo by id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }

        private static PhotoDto ToDto(Photo photo) => new()
        {
            Id = photo.Id,
            Name = photo.Name,
            Extension = photo.Extension,
            Length = photo.Dimensions.Length,
            Width = photo.Dimensions.Width,
            Tags = photo.Tags,
            CreatedAt = photo.CreatedAt,
            OwnerId = photo.OwnerId
        };
    }
}