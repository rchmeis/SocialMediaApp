using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class JobRepository : IJobRepository
    {

        public Task<StudentJob> WriteAsync(StudentJob entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<StudentJob> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudentJob>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StudentJob?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
        public Task<StudentJob> UpdateAsync(int id, StudentJob entity)
        {
            throw new NotImplementedException();
        }


        //BRUG IKKE SYNKRONE METODER - slettes når alle repos er flyttet til task

        public StudentJob Update(int id, StudentJob entity)
        {
            throw new NotImplementedException();
        }

        public StudentJob Write(StudentJob entity)
        {
            throw new NotImplementedException();
        }

        public StudentJob? ReadById(int id)
        {
            throw new NotImplementedException();
        }
        public StudentJob Delete(int id)
        {
            throw new NotImplementedException();
        }
        public List<StudentJob> ReadAll()
        {
            throw new NotImplementedException();
        }
    }
}
