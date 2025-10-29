using Microsoft.AspNetCore.Components;
using System.Web;

namespace InstanceManager.Host.WA.Services;

public class NavigationHelper
{
    private readonly NavigationManager _navigationManager;

    public NavigationHelper(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Gets the current URI
    /// </summary>
    public Uri CurrentUri => new Uri(_navigationManager.Uri);

    /// <summary>
    /// Gets the current path (without query string)
    /// </summary>
    public string CurrentPath => CurrentUri.GetLeftPart(UriPartial.Path);

    /// <summary>
    /// Gets all current query parameters as a dictionary
    /// </summary>
    public Dictionary<string, string?> GetQueryParameters()
    {
        var query = HttpUtility.ParseQueryString(CurrentUri.Query);
        var dict = new Dictionary<string, string?>();
        
        foreach (string key in query.Keys)
        {
            dict[key] = query[key];
        }
        
        return dict;
    }

    /// <summary>
    /// Gets a specific query parameter value
    /// </summary>
    public string? GetQueryParameter(string key)
    {
        var query = HttpUtility.ParseQueryString(CurrentUri.Query);
        return query[key];
    }

    /// <summary>
    /// Navigates to the current path with new query parameters (replaces all existing parameters)
    /// </summary>
    public void NavigateWithQuery(Dictionary<string, string?> queryParams, bool forceLoad = false)
    {
        var path = CurrentPath;
        var queryString = BuildQueryString(queryParams);
        
        var newUri = string.IsNullOrEmpty(queryString) 
            ? path 
            : $"{path}?{queryString}";
        
        _navigationManager.NavigateTo(newUri, forceLoad);
    }

    /// <summary>
    /// Navigates to the current path, merging new query parameters with existing ones
    /// </summary>
    public void NavigateWithMergedQuery(Dictionary<string, string?> queryParams, bool forceLoad = false)
    {
        var currentParams = GetQueryParameters();
        
        // Merge new parameters (overwrites existing keys)
        foreach (var param in queryParams)
        {
            if (param.Value == null)
            {
                currentParams.Remove(param.Key);
            }
            else
            {
                currentParams[param.Key] = param.Value;
            }
        }
        
        NavigateWithQuery(currentParams, forceLoad);
    }

    /// <summary>
    /// Navigates to the current path with a single query parameter (replaces all existing parameters)
    /// </summary>
    public void NavigateWithQuery(string key, string? value, bool forceLoad = false)
    {
        var queryParams = new Dictionary<string, string?>();
        
        if (value != null)
        {
            queryParams[key] = value;
        }
        
        NavigateWithQuery(queryParams, forceLoad);
    }

    /// <summary>
    /// Navigates to the current path, setting or updating a single query parameter
    /// </summary>
    public void SetQueryParameter(string key, string? value, bool forceLoad = false)
    {
        var queryParams = new Dictionary<string, string?> { { key, value } };
        NavigateWithMergedQuery(queryParams, forceLoad);
    }

    /// <summary>
    /// Removes a query parameter and navigates
    /// </summary>
    public void RemoveQueryParameter(string key, bool forceLoad = false)
    {
        SetQueryParameter(key, null, forceLoad);
    }

    /// <summary>
    /// Clears all query parameters and navigates to the current path
    /// </summary>
    public void ClearQueryParameters(bool forceLoad = false)
    {
        _navigationManager.NavigateTo(CurrentPath, forceLoad);
    }

    /// <summary>
    /// Builds a URL with query parameters (does not navigate)
    /// </summary>
    public string BuildUrl(Dictionary<string, string?> queryParams)
    {
        var path = CurrentPath;
        var queryString = BuildQueryString(queryParams);
        
        return string.IsNullOrEmpty(queryString) 
            ? path 
            : $"{path}?{queryString}";
    }

    /// <summary>
    /// Builds a URL with a single query parameter (does not navigate)
    /// </summary>
    public string BuildUrl(string key, string? value)
    {
        var queryParams = new Dictionary<string, string?>();
        
        if (value != null)
        {
            queryParams[key] = value;
        }
        
        return BuildUrl(queryParams);
    }

    /// <summary>
    /// Builds a URL with merged query parameters (does not navigate)
    /// </summary>
    public string BuildUrlWithMergedQuery(Dictionary<string, string?> queryParams)
    {
        var currentParams = GetQueryParameters();
        
        // Merge new parameters (overwrites existing keys)
        foreach (var param in queryParams)
        {
            if (param.Value == null)
            {
                currentParams.Remove(param.Key);
            }
            else
            {
                currentParams[param.Key] = param.Value;
            }
        }
        
        return BuildUrl(currentParams);
    }

    private string BuildQueryString(Dictionary<string, string?> queryParams)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        
        foreach (var param in queryParams.Where(p => p.Value != null))
        {
            query[param.Key] = param.Value;
        }
        
        return query.ToString() ?? string.Empty;
    }
}
