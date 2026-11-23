using MySql.Data.MySqlClient;
using ServiceStack;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    /// <summary>
    /// Repository responsible for asynchronous MySQL data access operations for educational institutions.
    /// </summary>
    public class InstitutionRepository : IInstitutionRepository
    {
        private readonly IDbConnect _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionRepository"/> class with the specified database connection provider.
        /// </summary>
        /// <param name="db">The database connection provider used to open asynchronous MySQL connections.</param>
        public InstitutionRepository(IDbConnect db)
        {
            _db = db;
        }
        public Task<Institute> WriteAsync(Institute entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves all distinct, non-empty institution email domain names from the database.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A list of trimmed institution domain names; returns an empty list if none are configured.
        /// </returns>
        public async Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken = default)
        {
            List<string> domains = new List<string>();

            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);           

            const string query = """
                SELECT DISTINCT domain_name
                FROM institutes
                WHERE domain_name IS NOT NULL;
                """;

            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            int domainOrdinal = reader.GetOrdinal("domain_name");

            while (await reader.ReadAsync(cancellationToken))
            {
                if (!await reader.IsDBNullAsync(domainOrdinal, cancellationToken))
                {
                    string domain = reader.GetString(domainOrdinal).Trim();
                    if (!string.IsNullOrWhiteSpace(domain))
                    {
                        domains.Add(domain);
                    }
                }
            }

            return domains;
        }

        /// <summary>
        /// Asynchronously retrieves all institutes matching the specified domain name along with their associated city details.
        /// </summary>
        /// <param name="domain_name">The email or institution domain name used to filter matching institutes.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests while connecting to and querying the database.</param>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="Institute"/> objects populated with <see cref="City"/> details for each matching record,
        /// or an empty list when <paramref name="domain_name"/> is null or empty or no matching institutes are found.
        /// </returns>
        public async Task<List<Institute>> GetByDomainAsync(string domain_name, CancellationToken cancellationToken)
        {
            if (domain_name.IsNullOrEmpty())
            {
                return new List<Institute>();
            }

            List<Institute> institutes = new List<Institute>();
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
            string query = """
                SELECT 
                    i.institute_id, 
                    i.name AS institute_name,
                    i.domain_name,                     
                    c.city_id,
                    c.name AS city_name,
                    c.postal_code                    
                FROM institutes i 
                JOIN cities c ON i.city_id = c.city_id
                WHERE i.domain_name = @domain_name
                """;
            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@domain_name", domain_name.Trim());

            await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            int instituteIdOrdinal = reader.GetOrdinal("institute_id");
            int instituteNameOrdinal = reader.GetOrdinal("institute_name");
            int domainNameOrdinal = reader.GetOrdinal("domain_name");
            int cityIdOrdinal = reader.GetOrdinal("city_id");
            int cityNameOrdinal = reader.GetOrdinal("city_name");
            int postalCodeOrdinal = reader.GetOrdinal("postal_code");

            while (await reader.ReadAsync(cancellationToken))
            {
                City city = new City
                {
                    Id = reader.GetInt32(cityIdOrdinal),
                    Name = reader.GetString(cityNameOrdinal),
                    PostalCode = reader.GetString(postalCodeOrdinal)
                };

                Institute institute = new Institute
                {
                    Id = reader.GetInt32(instituteIdOrdinal),
                    Name = reader.GetString(instituteNameOrdinal),
                    DomainName = reader.GetString(domainNameOrdinal),
                    City = city
                };

                institutes.Add(institute);
            }

            return institutes;
        }

        /// <summary>
        /// Asynchronously retrieves a course along with its field of study if it is associated with the specified institute.
        /// </summary>
        /// <param name="courseId">The unique identifier of the course to retrieve. Must be greater than zero.</param>
        /// <param name="instituteId">The unique identifier of the institute to verify association against. Must be greater than zero.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests while connecting to and querying the database.</param>
        /// <returns>
        /// A <see cref="Course"/> instance populated with the course's ID, name, and field of study if a matching record is found;
        /// otherwise, <see langword="null"/> when either identifier is invalid or no matching association exists.
        /// </returns>
        public async Task<List<Course>> GetCoursesByInstituteIdAsync(int instituteId, CancellationToken cancellationToken)
        {
            List<Course> courses = new List<Course>();
            if (instituteId <= 0)
            {
                return courses;
            }

            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
            string query = """
                SELECT
                    c.course_id,
                    c.name AS course_name,                   
                    fos.name AS field_of_study_name
                FROM courses c
                INNER JOIN institute_courses ic ON c.course_id = ic.course_id
                INNER JOIN fields_of_study fos ON c.field_of_study_id = fos.field_of_study_id
                WHERE ic.institute_id = @institute_id;
                """;
            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@institute_id", instituteId);
            await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            int courseIdOrdinal = reader.GetOrdinal("course_id");
            int courseNameOrdinal = reader.GetOrdinal("course_name");
            int fieldOfStudyNameOrdinal = reader.GetOrdinal("field_of_study_name");

            while (await reader.ReadAsync(cancellationToken))
            {
                var course = new Course
                {
                    Id = reader.GetInt32(courseIdOrdinal),
                    Name = reader.GetString(courseNameOrdinal),
                    FieldOfStudy = reader.GetString(fieldOfStudyNameOrdinal)
                };

                courses.Add(course);
            }           

            return courses;
        }

        /// <summary>
        /// Retrieves a <see cref="Course"/> by its identifier only if it is associated with the specified institute.
        /// </summary>
        /// <param name="courseId">The unique identifier of the course.</param>
        /// <param name="instituteId">The unique identifier of the institute to verify association against.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous database operations.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning the matching <see cref="Course"/>
        /// with its field of study populated, or <see langword="null"/> if the IDs are non-positive or no matching
        /// course is offered by the specified institute.
        /// </returns>
        public async Task<Course?> GetCourseForInstituteAsync(int courseId, int instituteId, CancellationToken cancellationToken)
        {
            if (courseId <= 0 || instituteId <= 0)
            {
                return null;
            }

            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            const string query = """
                SELECT
                    c.course_id,
                    c.name AS course_name,
                    fos.name AS field_of_study_name
                FROM courses c
                INNER JOIN institute_courses ic ON c.course_id = ic.course_id
                INNER JOIN fields_of_study fos ON c.field_of_study_id = fos.field_of_study_id
                WHERE c.course_id = @course_id
                AND ic.institute_id = @institute_id;
                """;

            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@course_id", courseId);
            cmd.Parameters.AddWithValue("@institute_id", instituteId);

            await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            int courseIdOrdinal = reader.GetOrdinal("course_id");
            int courseNameOrdinal = reader.GetOrdinal("course_name");
            int fieldOfStudyNameOrdinal = reader.GetOrdinal("field_of_study_name");

            if (await reader.ReadAsync(cancellationToken))
            {
                return new Course
                {
                    Id = reader.GetInt32(courseIdOrdinal),
                    Name = reader.GetString(courseNameOrdinal),
                    FieldOfStudy = reader.GetString(fieldOfStudyNameOrdinal)
                };
            }

            return null;
        }
        public Task<Institute?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Institute>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Institute> UpdateAsync(int id, Institute entity)
        {
            throw new NotImplementedException();
        }

        public Task<Institute> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }


        //BRUG IKKE NEDENSTÅENDE - FJERNES NÅR ALLE REPOS ER LFYTTET TIL TASK
        public List<Institute> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Institute? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Institute Update(int id, Institute entity)
        {
            throw new NotImplementedException();
        }

        public Institute Write(Institute entity)
        {
            throw new NotImplementedException();
        }


        public Institute Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
