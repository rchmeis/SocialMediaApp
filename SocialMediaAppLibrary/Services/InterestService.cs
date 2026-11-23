using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services;

public class InterestService : IInterestService
{
    private readonly IInterestRepository _interestRepository;
    public InterestService(IInterestRepository interestRepository)
    {
        _interestRepository = interestRepository;
    }
    public Interest Delete(int id)
    {
        throw new NotImplementedException();
    }

    public List<Interest> ReadAll()
    {
        return _interestRepository.ReadAll();
    }

    public Interest? ReadById(int id)
    {
        throw new NotImplementedException();
    }

    public Interest Update(int id, Interest entity)
    {
        throw new NotImplementedException();
    }

    public Interest Write(Interest entity)
    {
        throw new NotImplementedException();
    }

    public Task<Interest> WriteAsync(Interest entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<Interest>> ReadAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Interest> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Interest?> ReadByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Interest> UpdateAsync(int id, Interest entity)
    {
        throw new NotImplementedException();
    }
}
