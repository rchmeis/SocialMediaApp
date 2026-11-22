using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class JobService : IJobService
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







        //SLET NEDENSTÅENDE NÅR ALLE REPOS ER OVERGÅE TTIL TASK
        public StudentJob? ReadById(int id)
        {
            throw new NotImplementedException();
        }


        public Task<StudentJob> UpdateAsync(int id, StudentJob entity)
        {
            throw new NotImplementedException();
        }

        public StudentJob Update(int id, StudentJob entity)
        {
            throw new NotImplementedException();
        }

        public StudentJob Write(StudentJob entity)
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
