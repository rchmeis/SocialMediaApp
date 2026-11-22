using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IInstitutionService :ICrud<Institute,int>
    {
        public Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken);
        public Task<List<Institute>> GetByDomainAsync(string domain_name, CancellationToken cancellationToken);
        public string? ExtractDomain(string? email);
        public Task<List<Course>> GetCoursesByInstituteIdAsync(int instituteId, CancellationToken cancellationToken);
        public Task<Course?> GetCourseForInstituteAsync(int courseId, int instituteId, CancellationToken cancellationToken);
    }
}
