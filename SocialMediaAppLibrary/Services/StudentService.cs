using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Repository;
using SocialMediaAppLibrary.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{    
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public List<Interest> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Interest ReadById(int id)
        {
            throw new NotImplementedException();
        }

        Student IWrite<Student, int>.Delete(int id)
        {
            throw new NotImplementedException();
        }

        List<Student> IRead<Student, int>.ReadAll()
        {
            throw new NotImplementedException();
        }

        Student? IRead<Student, int>.ReadById(int id)
        {
            throw new NotImplementedException();
        }

        Student IWrite<Student, int>.Update(int id, Student entity)
        {
            throw new NotImplementedException();
        }

        Student IWrite<Student, int>.Write(Student entity)
        {
            throw new NotImplementedException();
        }
    }
}
