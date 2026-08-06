using Microsoft.AspNetCore.Mvc;
using ShipSharp.Application.Common.Models;

namespace ShipSharp.API.Common.Extensions;

public static class ControllerExtensions
{
    public static OkObjectResult OkPaged<T>(
        this ControllerBase controller,
        IReadOnlyList<T> items,
        int page,
        int perPage,
        int totalCount)
    {
        var baseUrl = $"{controller.Request.Scheme}://{controller.Request.Host}{controller.Request.Path}";
        var pagination = PaginationMeta.Create(page, perPage, totalCount, baseUrl);
        var meta = ApiMeta.Create(pagination: pagination);
        return controller.Ok(ApiResponse<IReadOnlyList<T>>.Success(items, meta));
    }
}
