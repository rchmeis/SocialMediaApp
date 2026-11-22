using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class InstitutionService : IInstitutionService
    {
        private readonly IInstitutionRepository _institutionRepository;
        public InstitutionService(IInstitutionRepository institutionRepository)
        {
            _institutionRepository = institutionRepository;
        }
       
        public string? ExtractDomain(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            int atIndex = email.IndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                return null; // missing '@' or nothing after '@'
            }

            return email[(atIndex + 1)..].Trim().ToLowerInvariant();
        }

        public async Task<List<Institute>> GetByDomainAsync(string domain_name, CancellationToken cancellationToken)
        {
            return await _institutionRepository.GetByDomainAsync(domain_name, cancellationToken);
        }
        public async Task<List<Course>> GetCoursesByInstituteIdAsync(int instituteId, CancellationToken cancellationToken)
        {
            return await _institutionRepository.GetCoursesByInstituteIdAsync(instituteId, cancellationToken);
        }

        public async Task<Course?> GetCourseForInstituteAsync(int courseId, int instituteId, CancellationToken cancellationToken)
        {
            return await _institutionRepository.GetCourseForInstituteAsync(courseId, instituteId, cancellationToken);
        }
       

        public async Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken)
        {
            return await _institutionRepository.ReadAllDomainsAsync(cancellationToken);
        }

       
        public Task<Institute> WriteAsync(Institute entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Institute?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Institute>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Institute> UpdateAsync(int id, Institute entity)
        {
            throw new NotImplementedException();
        }

        public Task<Institute> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }










        //NEDENSTÅENDE SLETTES NÅR ALLE REPOS HAR ÆNDRET METODER TIL TASK
        public List<Institute> ReadAll()
        {
            throw new NotImplementedException();
        }
        public Institute? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Institute Update(int id, Institute entity)
        {
            throw new NotImplementedException();
        }

        public Institute Write(Institute entity)
        {
            throw new NotImplementedException();
        }
        public Institute Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
