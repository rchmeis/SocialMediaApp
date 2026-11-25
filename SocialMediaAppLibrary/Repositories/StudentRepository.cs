using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Transactions;

namespace SocialMediaAppLibrary.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        /// <summary>
        /// Repository responsible for persisting and retrieving <see cref="Student"/> records
        /// and their underlying <c>users</c> data in MySQL.
        /// </summary>
        private readonly IDbConnect _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudentRepository"/> class.
        /// </summary>
        /// <param name="dbConnect">Database connection factory used to open MySQL connections.</param>
        public StudentRepository(IDbConnect dbConnect)
        {
            _db = dbConnect;
        }

        /// <summary>
        /// Persists a new <see cref="Student"/> within a database transaction by inserting the base
        /// record into <c>users</c> first, assigning the generated primary key to <see cref="Student.Id"/>,
        /// and inserting the student-specific record into <c>students</c>.
        /// </summary>
        /// <param name="student">The student entity to persist, including institute and course selections.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous database operations.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning the <paramref name="student"/> instance
        /// with <see cref="Student.Id"/> populated from the newly inserted <c>users</c> row.
        /// </returns>
        public async Task<Student> WriteAsync(Student student, CancellationToken cancellationToken)
        {
            MySqlTransaction? transaction = null;

            try
            {
                await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
                transaction = await connection.BeginTransactionAsync(cancellationToken);

                const string insertUserQuery = """
                INSERT INTO users (
                    user_auth_id,
                    bio,
                    image_id
                )
                VALUES (
                    @user_auth_id,
                    @bio,
                    @image_id
                );
                """;

                await using MySqlCommand userCmd = new MySqlCommand(insertUserQuery, connection, transaction);
                userCmd.Parameters.AddWithValue("@user_auth_id", student.UserAuthenticationId);
                userCmd.Parameters.AddWithValue("@bio", student.Bio ?? string.Empty);
                userCmd.Parameters.AddWithValue(
                    "@image_id",
                    (object?)student.Image?.Id ?? DBNull.Value);

                await userCmd.ExecuteNonQueryAsync(cancellationToken);

                int generatedUserId = Convert.ToInt32(userCmd.LastInsertedId);
                student.Id = generatedUserId;

                const string insertStudentQuery = """
                INSERT INTO students (
                    user_id,
                    first_name,
                    last_name,
                    is_onboarded,
                    institute_id,
                    course_id
                )
                VALUES (
                    @user_id,
                    @first_name,
                    @last_name,
                    @is_onboarded,
                    @institute_id,
                    @course_id
                );
                """;

                await using MySqlCommand studentCmd = new MySqlCommand(insertStudentQuery, connection, transaction);
                studentCmd.Parameters.AddWithValue("@user_id", generatedUserId);
                studentCmd.Parameters.AddWithValue("@first_name", student.FirstName);
                studentCmd.Parameters.AddWithValue("@last_name", student.LastName);
                studentCmd.Parameters.AddWithValue("@is_onboarded", student.IsOnboarded);
                studentCmd.Parameters.AddWithValue("@institute_id", student.Institute.Id);
                studentCmd.Parameters.AddWithValue("@course_id", student.Course.Id);

                await studentCmd.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();
            }
            catch (Exception ex)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    await transaction.DisposeAsync();
                }
                throw; // rethrows the original exception with its stack trace intact
            }
            return student;
        }

        public async Task<Student?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {            
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            const string selectQuery = """
                                    SELECT
                                        u.user_id,
                                        u.user_auth_id,
                                        u.bio,
                                        u.image_id,
                                        u.created_at,
                                        u.deleted_at,
                                        s.first_name,
                                        s.last_name,
                                        s.is_onboarded,
                                        s.institute_id,
                                        s.course_id
                                    FROM users u
                                    INNER JOIN students s ON s.user_id = u.user_id
                                    WHERE u.user_id = @user_id;
                                    """;

            await using MySqlCommand cmd = new MySqlCommand(selectQuery, connection);
            cmd.Parameters.AddWithValue("@user_id", id);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            int idOrdinal = reader.GetOrdinal("user_id");
            int authIdOrdinal = reader.GetOrdinal("user_auth_id");
            int bioOrdinal = reader.GetOrdinal("bio");
            int imageIdOrdinal = reader.GetOrdinal("image_id");
            int createdAtOrdinal = reader.GetOrdinal("created_at");
            int deletedAtOrdinal = reader.GetOrdinal("deleted_at");
            int firstNameOrdinal = reader.GetOrdinal("first_name");
            int lastNameOrdinal = reader.GetOrdinal("last_name");
            int isOnboardedOrdinal = reader.GetOrdinal("is_onboarded");
            int instituteIdOrdinal = reader.GetOrdinal("institute_id");
            int courseIdOrdinal = reader.GetOrdinal("course_id");

            int? imageId = reader.IsDBNull(imageIdOrdinal) ? null : reader.GetInt32(imageIdOrdinal);
            DateTime? deletedAt = reader.IsDBNull(deletedAtOrdinal) ? null : reader.GetDateTime(deletedAtOrdinal);

            return new Student
            {
                Id = reader.GetInt32(idOrdinal),
                UserAuthenticationId = reader.GetInt32(authIdOrdinal),
                Bio = reader.IsDBNull(bioOrdinal) ? string.Empty : reader.GetString(bioOrdinal),
                CreatedAt = reader.GetDateTime(createdAtOrdinal),
                DeletedAt = deletedAt,
                FirstName = reader.GetString(firstNameOrdinal),
                LastName = reader.GetString(lastNameOrdinal),
                IsOnboarded = reader.GetBoolean(isOnboardedOrdinal),
                Image = imageId.HasValue ? new Image { Id = imageId.Value } : null,
                Institute = new Institute { Id = reader.GetInt32(instituteIdOrdinal) },
                Course = new Course { Id = reader.GetInt32(courseIdOrdinal) }
            };
        }
        

        public Task<List<Student>> ReadAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Student> UpdateAsync(int id, Student entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Student> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        //BRUG IKKE SYNKRONE METODER - slettes fra ICRUD når alle repos har skiftet til task
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
