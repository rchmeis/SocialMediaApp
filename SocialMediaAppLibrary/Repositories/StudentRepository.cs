using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly DbConnect _dbConnect;

        public StudentRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }
        public Student Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Student> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Student? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Student Update(int id, Student entity)
        {
            throw new NotImplementedException();
        }

        public Student Write(Student entity)
        {
            throw new NotImplementedException();
        }
    }
}
