using System.Net.Http.Headers;
using System.Net.Http.Json;
using Gemz.Api.Auth.Service.Auth.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public class TwitchClient : ITwitchClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwitchClient> _logger;
    private readonly IOptions<AuthConfig> _config;

    public TwitchClient(HttpClient httpClient, ILogger<TwitchClient> logger, IOptions<AuthConfig> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }
    
    public async Task<TwitchUserData> GetTwitchAccessTokens(string code)
    {
        _logger.LogDebug("Entered GetTwitchAccessToken function");

        var request = CreateAccessTokenRequest(code);

        if (request is null)
        {
            _logger.LogWarning("Missing essential AppSettings for Twitch Token call. Leaving Function");
            return new TwitchUserData()
            {
                Error = "AU000008"
            };
        }

        _logger.LogDebug("Calling Twitch End Point for getting TwitchAccessToken data");
        var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        TwitchAuthCodeResponse twitchAuthCodeResponse = null;
        try
        {
            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                twitchAuthCodeResponse = await response.Content.ReadFromJsonAsync<TwitchAuthCodeResponse>();
            }
        }
        catch(Exception e)
        {
            _logger.LogWarning(e,$"Twitch returned status code: {response.StatusCode}. Leaving function.");
            return new TwitchUserData()
            {
                Error = "AU000009"
            };
        }
        finally
        {
            _logger.LogDebug("Calling Dispose of response in GetTwitchAccessTokens");
            response.Dispose();
        }

        _logger.LogDebug("Converted JSON returned from Twitch");
        var twitchUserData = new TwitchUserData()
        {
            UserAccessCode = twitchAuthCodeResponse.AccessToken,
            UserRefreshCode = twitchAuthCodeResponse.RefreshToken
        };

        return twitchUserData;
    }

    public async Task<TwitchUserInfoResponse> GetUserInfoFromTwitch(string twitchUserAccessCode)
    {
        _logger.LogDebug("Entered GetTwitchAccessToken function");

        var request = CreateUserInfoRequest(twitchUserAccessCode);

        if (request is null)
        {
            _logger.LogWarning("Config setting for UserInfoEndpoint is empty. Leaving Function.");
            return new TwitchUserInfoResponse()
            {
                ErrorCode = "AU000010"
            };
        }
        _logger.LogInformation($"Calling Twitch end point for user info");
        
        var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        TwitchUserInfoResponse twitchUserInfoResponse = null;
        try
        {
            response.EnsureSuccessStatusCode();
            if (response.Content is object)
            {
                twitchUserInfoResponse = await response.Content.ReadFromJsonAsync<TwitchUserInfoResponse>();
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Problem with Twitch response. Leaving Function.");
            return new TwitchUserInfoResponse()
            {
                ErrorCode = "AU000011"
            };
        }
        finally
        {
            _logger.LogDebug("Calling Dispose of response in GetUserInfoFromTwitch");
            response.Dispose();
        }
        
        _logger.LogDebug("Reading JSON returned by Twitch");
        return twitchUserInfoResponse;

    }

    public async Task<bool> ValidateTwitchAccessToken(string accessToken)
    {
        _logger.LogDebug("Entered ValidateTwitchAccessToken function");

        var request = CreateValidateTokenRequest(accessToken);

        var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        _logger.LogDebug("Returned from Twitch call");
        var responseStatusCode = false;
        _logger.LogInformation($"Twitch call status code: {response.StatusCode}");
    
        responseStatusCode = response.IsSuccessStatusCode;
        
        _logger.LogDebug("Calling Dispose of response in ValidateTwitchAccessToken");
        response.Dispose();
        return responseStatusCode;
    }

    public async Task<TwitchAuthCodeResponse> RefreshTokenFromTwitch(string twitchRefreshCode)
    {
        _logger.LogDebug("Entered RefreshTokenFromTwitch function");

        var request = CreateRefreshTokenRequest(twitchRefreshCode);

        var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        _logger.LogDebug("Returned from Twitch call");
        _logger.LogInformation($"Twitch call status code: {response.StatusCode}");
        
        TwitchAuthCodeResponse twitchAuthCodeResponse = null;
        try
        {
            response.EnsureSuccessStatusCode();
            if (response.Content is object)
            {
                twitchAuthCodeResponse = await response.Content.ReadFromJsonAsync<TwitchAuthCodeResponse>();
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Problem with Twitch response. Leaving Function.");
            return new TwitchAuthCodeResponse()
            {
                ErrorCode = "BadRequest"
            };
        }
        finally
        {
            _logger.LogDebug("Calling Dispose of response in RefreshTokenFromTwitch");
            response.Dispose();
        }

        _logger.LogDebug("Reading JSON returned by Twitch");
        return twitchAuthCodeResponse;
    }

    private HttpRequestMessage CreateRefreshTokenRequest(string twitchRefreshCode)
    {
        _logger.LogDebug("Entered CreateRefreshTokenRequest function");
        
        var twitchConfig = _config.Value.Twitch;
        var reqUri = $"{twitchConfig.TokenEndpoint}";
        var request = new HttpRequestMessage(HttpMethod.Post, reqUri);
        var body = new FormUrlEncodedContent(new []
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", twitchRefreshCode),
            new KeyValuePair<string, string>("client_id", twitchConfig.ClientId),
            new KeyValuePair<string, string>("client_secret", twitchConfig.ClientSecret),
        });
        request.Content = body;

        return request;
    }
    
    private HttpRequestMessage CreateValidateTokenRequest(string accessToken)
    {
        var twitchConfig = _config.Value.Twitch;
        var reqUri = $"{twitchConfig.TokenValidateEndpoint}";
        _logger.LogInformation($"Calling Twitch with uri: {reqUri}");
        
        var request = new HttpRequestMessage(HttpMethod.Get, reqUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }


    private HttpRequestMessage CreateUserInfoRequest(string userAccessCode)
    {
        _logger.LogDebug("Entered CreateUserInfoRequest function");
        
        var twitchConfig = _config.Value.Twitch;
        if (string.IsNullOrEmpty(twitchConfig.UserInfoEndpoint))
        {
            return null;
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, twitchConfig.UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessCode);

        return request;
    }
    private HttpRequestMessage CreateAccessTokenRequest(string code)
    {
        var twitchConfig = _config.Value.Twitch;
        if (string.IsNullOrEmpty(twitchConfig.TokenEndpoint)
            || string.IsNullOrEmpty(twitchConfig.ClientId)
            || string.IsNullOrEmpty(twitchConfig.ClientSecret)
            || string.IsNullOrEmpty(twitchConfig.RedirectUrl))
        {
            _logger.LogWarning("Missing essential AppSettings for Twitch Token call. Leaving Function");
            return null;
        }
        
        var reqUri = $"{twitchConfig.TokenEndpoint}";
        var request = new HttpRequestMessage(HttpMethod.Post, reqUri);
        var body = new FormUrlEncodedContent(new []
        {
            new KeyValuePair<string, string>("client_id", twitchConfig.ClientId),
            new KeyValuePair<string, string>("client_secret", twitchConfig.ClientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", twitchConfig.RedirectUrl)
        });
        request.Content = body;

        return request;
    }
}