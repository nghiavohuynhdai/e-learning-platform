using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Catalog;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly IRedisService _cacheService;

    public CatalogController(ICatalogService catalogService, IRedisService cacheService)
    {
        _catalogService = catalogService;
        _cacheService = cacheService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCatalog()
    {
        var cache = await _cacheService.GetDataAsync<IEnumerable<CatalogDTO>>($"catalogs");
        if (cache is not null)
            return StatusCode(200, ResultDTO.Success(cache));
        var result = await _catalogService.GetAll();
        if (result._isSuccess)
            await _cacheService.SetDataAsync($"catalogs", result._data);
        return StatusCode(result._statusCode, result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Post([FromBody] CatalogCreateDTO catalogRequest)
    {
        if (catalogRequest == null)
        {
            return NoContent();
        }

        var result = await _catalogService.AddCatalog(catalogRequest);
        if (result._isSuccess)
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.Catalogs);
        return StatusCode(result._statusCode, result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCatalog([FromBody] CatalogDTO catalog)
    {
        var result = await _catalogService.UpdateCatalog(catalog);
        if (result._isSuccess)
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.Catalogs);
        return StatusCode(result._statusCode, result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCatalog(Guid ID)
    {
        var result = await _catalogService.DeleteCatalog(ID);
        if (result._isSuccess)
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.Catalogs);
        return StatusCode(result._statusCode, result);
    }
}