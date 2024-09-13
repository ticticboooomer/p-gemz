using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure;
using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Linq;
using SharpCompress.Common;

namespace Gemz.Api.Creator.Data.Repository;
public class OverlayKeyRepository : IOverlayKeyRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<OverlayKeyRepository> _logger;
    private readonly IMongoCollection<OverlayKey> _container;

    public OverlayKeyRepository(MongoFactory factory, ILogger<OverlayKeyRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<OverlayKey>("overlay_keys");
    }

    public async Task<List<OverlayKey>> GetAllForCreatorAsync(string creatorId)
    {
        _logger.LogDebug("Entered GetAllForCreatorAsync function");
        _logger.LogInformation($"Getting overlay keys for creator with id: {creatorId}");

        try
        {
            var filter = Builders<OverlayKey>.Filter.Eq(x => x.CreatorId, creatorId);
            var response = await  _container.FindAsync(filter);

            _logger.LogDebug("Returned from Queryable Where call");

            if (response is null)
            {
                _logger.LogWarning("FirstOrDefaultAsync did not return a gem");
            }

            return await response.ToListAsync();

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repo error during fetch of overlay keys by id of creator");
            return null;
        }
    }

    public async Task<OverlayKey> CreateKeyAsync(OverlayKey model)
    {
        _logger.LogDebug("Entered CreateKeyAsync function");
        _logger.LogInformation($"Creating overlay key for creator id: {model.CreatorId}");

        try
        {
            await _container.InsertOneAsync(model);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"repo error creating overlay key for creator id: {model.CreatorId}");
            return null;
        }
    }

    public async Task<string> RevokeKey(string creatorId, string keyId)
    {
        _logger.LogDebug("Entered RevokeKey function");
        _logger.LogInformation($"Revoking overlay key with id: {keyId}");

        try
        {
            var filter = Builders<OverlayKey>.Filter.Where(x => x.CreatorId == creatorId && x.Id == keyId);
            await _container.FindOneAndDeleteAsync(filter);
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to delete key");
            return null;
        }
    }

    public async Task<OverlayKey> GetKeyByContent(string keyContent)
    {
        _logger.LogDebug("Entered GetKeyByContent function");
        _logger.LogInformation($"Getting overlay key with content: {keyContent}");

        try
        {
            var filter = Builders<OverlayKey>.Filter.Where(x => x.KeyContent == keyContent);
            return await _container.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get key");
            return null;
        }
    }

    public async Task<int> CountKeysAsync(string creatorId)
    {
        _logger.LogDebug("Entered CountKeysAsync function");
        _logger.LogInformation($"Getting document count of overlay keys where creator is: {creatorId}");

        var filter = Builders<OverlayKey>.Filter.Eq(x => x.CreatorId, creatorId);

        var resp = await _container.CountDocumentsAsync(filter);

        return (int)resp;
    }
}
