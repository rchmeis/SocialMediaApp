using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IInstitutionRepository:ICrud<Institute, int>
    {
        public Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken);
        public Task<List<Institute>> GetByDomainAsync(string domain_name, CancellationToken cancellationToken);
        public Task<List<Course>> GetCoursesByInstituteIdAsync(int instituteId, CancellationToken cancellationToken);
        public Task<Course> GetCourseForInstituteAsync(int courseId, int instituteId, CancellationToken cancellationToken);
        
    }
}
