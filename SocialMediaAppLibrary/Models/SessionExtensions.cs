using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SocialMediaAppLibrary.Models
{
    public class VerificationCode
    {
        public string Code { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
    
    public static class SessionKeys
    {
        public const string SignupAuth = "SignupAuth";
        public const string SignupStudent = "SignupStudent";
        public const string SignupPartner = "SignupPartner";
        public const string Onboarding = "Onboarding";
        public const string VerificationCode = "VerificationCode";
        public const string ImageSessionKey = "ImageSessionKey";
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

        public static void SetImage(this ISession session, Image image) => session.Set(SessionKeys.ImageSessionKey, image);
        public static Image GetImage(this ISession session) => session.Get<Image>(SessionKeys.ImageSessionKey)??new Image();

        public static void SetPartner(this ISession session, Partner partner) =>
            session.Set(SessionKeys.SignupPartner, partner);

        public static Partner GetPartner(this ISession session) =>
            session.Get<Partner>(SessionKeys.SignupPartner) ?? new Partner();
        
        public static void SetVerificationCode(this ISession session, VerificationCode code) =>
            session.Set(SessionKeys.VerificationCode, code);

        public static VerificationCode? GetVerificationCode(this ISession session) =>
            session.Get<VerificationCode>(SessionKeys.VerificationCode);
    }
}
