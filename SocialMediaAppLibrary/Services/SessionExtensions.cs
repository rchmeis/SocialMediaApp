using Microsoft.AspNetCore.Http;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public static class SessionKeys
{
    public const string SignupAuth = "SignupAuth";
    public const string SignupStudent = "SignupStudent";
    public const string Onboarding = "Onboarding";
}

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        string? value = session.GetString(key);

        if (string.IsNullOrEmpty(value))
        {
            return default;
        }
       
        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}