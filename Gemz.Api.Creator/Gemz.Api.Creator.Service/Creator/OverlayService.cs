using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Gemz.ServiceBus.Factory;
using Gemz.ServiceBus.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Creator.Service.Creator;
public class OverlayService : IOverlayService
{
    private readonly ISendEndpointProvider _serviceBusFactory;
    private readonly IOptions<ServiceBusConfig> _config;
    private readonly IOverlayKeyRepository _keyRepo;
    private readonly ILogger<OverlayService> _logger;

    public OverlayService(
        ISendEndpointProvider serviceBusFactory,
        IOptions<ServiceBusConfig> config,
        IOverlayKeyRepository keyRepo,
        ILogger<OverlayService> logger)
    {
        _serviceBusFactory = serviceBusFactory;
        _config = config;
        _keyRepo = keyRepo;
        _logger = logger;
    }
    public async Task<GenericResponse<OverlayTestResponseModel>> SendOverlayTestOrder(string creatorId)
    {
        _logger.LogDebug("Entered SendOverlayTestOrder function");
        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("creatorId is null or empty. Exiting function");
            return new GenericResponse<OverlayTestResponseModel>()
            {
                Error = "CR001500"
            };
        }

        var client = await _serviceBusFactory.GetSendEndpoint(new Uri($"queue:{_config.Value.NotifyOrderQueueName}"));
        var model = new NotifyOrderModel()
        {
            CollectorName = "viewer123",
            CreatorId = creatorId,
            Packs = 4
        };

        await client.Send(model);
        return new GenericResponse<OverlayTestResponseModel>()
        {
            Data = new OverlayTestResponseModel()
            {
                DidSend = true
            }
        };
    }

    public async Task<GenericResponse<OverlayKeyModel>> CreateOverlayKey(string creatorId)
    {
        _logger.LogDebug("Entered CreateOverlayKey function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("creatorId is null or empty. Exiting function");
            return new GenericResponse<OverlayKeyModel>()
            {
                Error = "CR001601"
            };
        }

        var count = await _keyRepo.CountKeysAsync(creatorId);

        if (count >= 6)
        {
            _logger.LogWarning("Creator has too many active keys. Exiting function");
            return new GenericResponse<OverlayKeyModel>()
            {
                Error = "CR001600"
            };
        }

        var res = await _keyRepo.CreateKeyAsync(new OverlayKey()
        {
            CreatedAt =  DateTime.UtcNow,
            CreatorId = creatorId,
            Id = Guid.NewGuid().ToString(),
            KeyContent = Guid.NewGuid().ToString()
        });

        if (res is null)
        {
            _logger.LogError($"repo error creating overlay key for creator id: {creatorId}");
            return new GenericResponse<OverlayKeyModel>()
            {
                Error = "CR001602"
            };
        }

        return new GenericResponse<OverlayKeyModel>()
        {
            Data = new OverlayKeyModel()
            {
                KeyContent = res.KeyContent,
                CreatorId = res.CreatorId,
                CreatedAt = res.CreatedAt,
                Id = res.Id
            }
        };
    }

    public async Task<GenericResponse<List<OverlayKeyModel>>> GetOverlayKeysForCreator(string creatorId)
    {
        _logger.LogDebug("Entered GetOverlayKeysForCreator in Overlay Service.");
        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("creatorId is null or empty. Exiting function");
            return new GenericResponse<List<OverlayKeyModel>>()
            {
                Error = "CR001700"
            };
        }
        var resp = await _keyRepo.GetAllForCreatorAsync(creatorId);

        if (resp is null)
        {
            _logger.LogError("repo error fetching overlay keys for creator");
            return new GenericResponse<List<OverlayKeyModel>>()
            {
                Error = "CR001701"
            };
        }

        return new GenericResponse<List<OverlayKeyModel>>()
        {
            Data = resp.Select(x => new OverlayKeyModel()
            {
                CreatorId = x.CreatorId,
                KeyContent = x.KeyContent,
                CreatedAt = x.CreatedAt,
                Id = x.Id
            }).ToList()
        };
    }

    public async Task<GenericResponse<string>> RevokeKey(string creatorId, string keyId)
    {
        _logger.LogDebug("Entered revokeKey in Overlay Service.");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("creatorId is null or empty. Exiting function");
            return new GenericResponse<string>
            {
                Error = "CR001800"
            };
        }

        if (string.IsNullOrEmpty(keyId))
        {
            _logger.LogError("keyId is null or empty. Exiting function");
            return new GenericResponse<string>
            {
                Error = "CR001801"
            };
        }

        var resp = await _keyRepo.RevokeKey(creatorId, keyId);

        if (resp is null)
        {
            _logger.LogError("repo error revoking overlay key");
            return new GenericResponse<string>()
            {
                Error = "CR001802"
            };
        }

        return new GenericResponse<string>()
        {
            Data = "Success"
        };
    }

    public async Task<ValidatedOverlayKeyModel> ValidateKey(string key)
    {
        _logger.LogDebug("Entered ValidateKey in Overlay Service.");

        if (string.IsNullOrEmpty(key))
        {
            _logger.LogError("key is null or empty. Exiting function");
            return null;
        }

        var model = await  _keyRepo.GetKeyByContent(key);
        if (model is null)
        {
            _logger.LogError("key not found");
            return null;
        }
        _logger.LogDebug("key validated");
        return new ValidatedOverlayKeyModel()
        {
            CreatorId = model.CreatorId,
            Key = model.KeyContent
        };
    }
}
