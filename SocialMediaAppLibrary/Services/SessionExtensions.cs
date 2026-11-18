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
    public const string SignupPartner = "SignupPartner";
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
    
    public static void SetUserAuthentication(this ISession session, UserAuthentication userAuth) => session.Set(SessionKeys.SignupAuth, userAuth);
    public static UserAuthentication GetUserAuthentication(this ISession session) => session.Get<UserAuthentication>(SessionKeys.SignupAuth) ?? new UserAuthentication();

    public static void SetStudent(this ISession session, Student student) => session.Set(SessionKeys.SignupStudent, student);
    public static Student GetStudent(this ISession session) => session.Get<Student>(SessionKeys.SignupStudent) ?? new Student();

    public static void SetPartner(this ISession session, Partner partner) =>
        session.Set(SessionKeys.SignupPartner, partner);

    public static Partner GetPartner(this ISession session) =>
        session.Get<Partner>(SessionKeys.SignupPartner) ?? new Partner();
}