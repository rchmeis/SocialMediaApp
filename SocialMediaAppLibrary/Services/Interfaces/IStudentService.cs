using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IStudentService : ICrud<Student, int>
    {
    }
}
