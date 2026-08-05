using System.Text.Json.Serialization;

namespace ShipSharp.Application.Common.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }

    [JsonPropertyName("meta")]
    public ApiMeta Meta { get; set; } = new();

    public static ApiResponse<T> Success(T data, ApiMeta? meta = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Error = null,
            Meta = meta ?? ApiMeta.Create()
        };
    }

    public static ApiResponse<T> Fail(string code, string message, IEnumerable<ApiErrorDetail>? details = null, ApiMeta? meta = null)
    {
        return new ApiResponse<T>
        {
            Data = default,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details?.ToList()
            },
            Meta = meta ?? ApiMeta.Create()
        };
    }
}

public class ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public List<ApiErrorDetail>? Details { get; set; }
}

public class ApiErrorDetail
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class ApiMeta
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    [JsonPropertyName("pagination")]
    public PaginationMeta? Pagination { get; set; }

    public static ApiMeta Create(string? requestId = null, PaginationMeta? pagination = null)
    {
        return new ApiMeta
        {
            RequestId = requestId ?? Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("o"),
            Pagination = pagination
        };
    }
}

public class PaginationMeta
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("links")]
    public PaginationLinks? Links { get; set; }

    public static PaginationMeta Create(int currentPage, int perPage, int totalItems, string baseUrl)
    {
        var totalPages = (int)Math.Ceiling((double)totalItems / perPage);
        return new PaginationMeta
        {
            CurrentPage = currentPage,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = new PaginationLinks
            {
                Self = $"{baseUrl}?page={currentPage}&per_page={perPage}",
                First = $"{baseUrl}?page=1&per_page={perPage}",
                Last = totalPages > 0 ? $"{baseUrl}?page={totalPages}&per_page={perPage}" : null,
                Next = currentPage < totalPages ? $"{baseUrl}?page={currentPage + 1}&per_page={perPage}" : null,
                Prev = currentPage > 1 ? $"{baseUrl}?page={currentPage - 1}&per_page={perPage}" : null
            }
        };
    }
}

public class PaginationLinks
{
    [JsonPropertyName("self")]
    public string? Self { get; set; }

    [JsonPropertyName("first")]
    public string? First { get; set; }

    [JsonPropertyName("prev")]
    public string? Prev { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("last")]
    public string? Last { get; set; }
}
