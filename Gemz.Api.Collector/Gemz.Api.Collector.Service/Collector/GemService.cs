using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Service.Collector;

public class GemService : IGemService
{
    private readonly IGemRepository _gemRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly ILogger<GemService> _logger;

    public GemService(IGemRepository gemRepo, ICollectionRepository collectionRepo, ILogger<GemService> logger)
    {
        _gemRepo = gemRepo;
        _collectionRepo = collectionRepo;
        _logger = logger;
    }

    public async Task<GenericResponse<GemSampleWithCountModel>> FetchFixedQuantityOfGems(GemFixedQtyInputModel gemFixedQtyInputModel)
    {
        _logger.LogDebug("Entered FetchFixedQuantityOfGems function");

        if (gemFixedQtyInputModel is null)
        {
            _logger.LogError("Missing Params");
            return new GenericResponse<GemSampleWithCountModel>()
            {
                Error = "CL000800"
            };
        }

        if (string.IsNullOrEmpty(gemFixedQtyInputModel.CollectionId))
        {
            _logger.LogError("No Collection Id passed in");
            return new GenericResponse<GemSampleWithCountModel>()
            {
                Error = "CL000801"
            };
        }

        if (gemFixedQtyInputModel.QtyOfGemsToFetch < 1)
        {
            _logger.LogError("QtyOfGems parameter is set to < 1");
            return new GenericResponse<GemSampleWithCountModel>()
            {
                Error = "CL000802"
            };
        }

        var creatorCollection = await _collectionRepo.GetSingleCollection(gemFixedQtyInputModel.CollectionId);

        if (creatorCollection is null)
        {
            _logger.LogError("Collection does not exist");
            return new GenericResponse<GemSampleWithCountModel>()
            {
                Error = "CL000803"
            };
        }

        var collectionGems =
            await _gemRepo.FetchAllGemsForCollection(gemFixedQtyInputModel.CollectionId);

        if (collectionGems is null)
        {
            _logger.LogError("Problem fetching gems for the collection");
            return new GenericResponse<GemSampleWithCountModel>()
            {
                Error = "CL000804"
            };
        }

        var returnModel = new GemSampleWithCountModel()
        {
            TotalGemsInCollection = collectionGems.Count,
            Gems = new List<GemModel>()
        };
        
        foreach (var gem in collectionGems.Take(gemFixedQtyInputModel.QtyOfGemsToFetch))
        {
            returnModel.Gems.Add(MapGemToGemModel(gem));
        }

        return new GenericResponse<GemSampleWithCountModel>()
        {
            Data = returnModel
        };
    }


    public async Task<GenericResponse<GemsPagedModel>> GetPagedGemsForCollection(GemPagingModel gemPagingModel)
    {
        _logger.LogDebug("Entered GetPagedGemsForCollection function");

        if (gemPagingModel is null)
        {
            _logger.LogError("No input params passed into method");
            return new GenericResponse<GemsPagedModel>()
            {
                Error = "CL000900"
            };
        }

        if (string.IsNullOrEmpty(gemPagingModel.CollectionId))
        {
            _logger.LogError("No CollectionId Detected. Leaving function.");
            return new GenericResponse<GemsPagedModel>()
            {
                Error = "CL000901"
            };
        }     
        
        
        if (gemPagingModel.PageSize <= 0)
        {
            _logger.LogError("pageSize passed in as <= zero. Leaving function.");
            return new GenericResponse<GemsPagedModel>()
            {
                Error = "CL000903"
            };
        }

        if (gemPagingModel.CurrentPage < 0)
        {
            _logger.LogError("CurentPage param passed in as < zero. Leaving function.");
            return new GenericResponse<GemsPagedModel>()
            {
                Error = "CL000906"
            };
        }

        var existingCollection = await _collectionRepo.GetSingleCollection(gemPagingModel.CollectionId);

        if (existingCollection is null)
        {
            _logger.LogError("Collection passed in does not exist. Exiting.");
            return new GenericResponse<GemsPagedModel>()
            {
                Error = "CL000904"
            };
        }

        _logger.LogDebug("Calling Gems Repo GetGemsPageByCollectionId");
        var gemsPage = await _gemRepo.GetGemsPageByCollectionId(gemPagingModel.CollectionId,gemPagingModel.CurrentPage, gemPagingModel.PageSize);

        if (gemsPage == null)
        {
            _logger.LogError(
                "null returned from repo function GetGemsPageByCollectionId. Leaving function, returning null.");
            return new GenericResponse<GemsPagedModel>()
            {
                Error = "CL000905"
            };
        }
        
        _logger.LogInformation($"Repo returned {gemsPage.Gems.Count} Gems records.");
        var gemsPagedModel = new GemsPagedModel()
        {
            Gems = new List<GemModel>(),
            ThisPage = gemsPage.ThisPage,
            TotalPages = gemsPage.TotalPages
        };

        _logger.LogDebug("Building GemsPagedModel return type from repo data.");
        foreach (var gem in gemsPage.Gems)
        {
            var gemModel = MapGemToGemModel(gem);
            gemsPagedModel.Gems.Add(gemModel);
        }
        
        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<GemsPagedModel>()
        {
            Data = gemsPagedModel
        };
    }

    public async Task<GenericResponse<SingleGemOutputModel>> GetSingleGemById(SingleGemInputModel singleGemInputModel)
    {
        _logger.LogDebug("Entered GetSingleGemIdId function.");
        _logger.LogInformation($"GemId: {singleGemInputModel.GemId}");

        if (string.IsNullOrEmpty(singleGemInputModel.GemId))
        {
            _logger.LogError("Missing or Empty Gem Id. Leaving Function.");
            return new GenericResponse<SingleGemOutputModel>()
            {
                Error = "CL002100"
            };
        }

        var gem = await _gemRepo.FetchSingleGem(singleGemInputModel.GemId);

        if (gem == null)
        {
            _logger.LogError("Unable to find Gem for given Id. Leaving Function.");
            return new GenericResponse<SingleGemOutputModel>
            {
                Error = "CL002101"
            };
        }

        var gemOutputModel = MapGemToSingleGemOutputModel(gem);

        return new GenericResponse<SingleGemOutputModel> 
        { 
            Data = gemOutputModel 
        };
    }

    private static SingleGemOutputModel MapGemToSingleGemOutputModel(Gem gem)
    {
        return new SingleGemOutputModel()
        {
            GemId = gem.Id,
            GemName = gem.Name,
            ImageId = gem.ImageId,
            Rarity = gem.Rarity,
            SizePercentage = gem.SizePercentage,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage
        };
    }

    private static GemModel MapGemToGemModel(Gem gem)    {
        return new GemModel()
        {
            Id = gem.Id,
            Name = gem.Name,
            Rarity = gem.Rarity,
            ImageId = gem.ImageId,
            SizePercentage = gem.SizePercentage,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage
        };
    }

}