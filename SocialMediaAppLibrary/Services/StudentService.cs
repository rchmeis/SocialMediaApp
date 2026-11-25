using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ZstdSharp.Unsafe;

namespace SocialMediaAppLibrary.Services
{    
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public Task<Student> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Student>> ReadAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Student?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            return _studentRepository.ReadByIdAsync(id, cancellationToken);
        }

        public Task<Student> UpdateAsync(int id, Student entity, CancellationToken cancellationToken)
        {
            return _studentRepository.UpdateAsync(id, entity, cancellationToken);
        }

        public async Task<Student> WriteAsync(Student entity, CancellationToken cancellationToken)
        {
            return await _studentRepository.WriteAsync(entity, cancellationToken);
        }




        //SLET NEDENSTÅENDE NÅR ALLE REPOS ER SKIFTET TIL TASK
        public Student Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Student Write(Student entity)
        {
            throw new NotImplementedException();
        }
        public Student Update(int id, Student entity)
        {
            throw new NotImplementedException();
        }
        public Student? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Student> ReadAll()
        {
            throw new NotImplementedException();        }

    
    }
}
