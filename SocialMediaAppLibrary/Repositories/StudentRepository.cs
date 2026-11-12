using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class StudentRepository : ICrud<Student, int>
    {

        private readonly DbConnect _dbConnect;

        public StudentRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
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
