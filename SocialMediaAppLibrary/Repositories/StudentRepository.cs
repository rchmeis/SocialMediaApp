using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class StudentRepository
    {

        private readonly DbConnect _dbConnect;

        public StudentRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }


    }
}
