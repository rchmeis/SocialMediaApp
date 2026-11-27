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
 
    public Task<Interest> WriteAsync(Interest entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<Interest>> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _interestRepository.ReadAllAsync(cancellationToken);
    }

    public Task<Interest> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Interest?> ReadByIdAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Interest> UpdateAsync(int id, Interest entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
