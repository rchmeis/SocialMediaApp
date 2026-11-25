using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System.Data.Common;

namespace SocialMediaAppLibrary.Repositories
{
    /// <summary>
    /// Repository responsible for persisting, querying, updating, and soft-deleting
    /// <see cref="Student"/> entities and their associated <c>users</c> and <c>user_interests</c> records in MySQL.
    /// </summary>
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnect _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudentRepository"/> class.
        /// </summary>
        /// <param name="dbConnect">Database connection factory used to open asynchronous MySQL connections.</param>
        public StudentRepository(IDbConnect dbConnect)
        {
            _db = dbConnect;
        }

        /// <summary>
        /// Persists a new <see cref="Student"/> within a database transaction by inserting into
        /// <c>users</c>, assigning the generated primary key to <see cref="User.Id"/>, inserting into
        /// <c>students</c>, and synchronizing <see cref="Student.Interests"/> in <c>user_interests</c>.
        /// </summary>
        /// <param name="student">The student entity to persist.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous database operation.</param>
        /// <returns>The persisted <see cref="Student"/> instance with <see cref="User.Id"/> populated.</returns>
        public async Task<Student> WriteAsync(Student student, CancellationToken cancellationToken = default)
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
                userCmd.Parameters.AddWithValue("@image_id", (object?)student.Image?.Id ?? DBNull.Value);

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

                await SyncUserInterestsAsync(connection, transaction, generatedUserId, student.Interests, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    await transaction.DisposeAsync();
                }
                throw;
            }

            return student;
        }

        /// <summary>
        /// Retrieves a single active (non-deleted) <see cref="Student"/> by user identifier,
        /// including its <see cref="Student.Interests"/> list.
        /// </summary>
        /// <param name="userId">The unique user/student identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous database operation.</param>
        /// <returns>
        /// The matching <see cref="Student"/> if found and not soft-deleted; otherwise <see langword="null"/>.
        /// </returns>
        public async Task<Student?> ReadByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            const string selectStudentQuery = """
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
                WHERE u.user_id = @user_id
                  AND u.deleted_at IS NULL;
                """;

            Student? student = null;

            await using (MySqlCommand cmd = new MySqlCommand(selectStudentQuery, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", userId);

                await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                int imageIdOrdinal = reader.GetOrdinal("image_id");
                int deletedAtOrdinal = reader.GetOrdinal("deleted_at");

                student = new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("user_id")),
                    UserAuthenticationId = reader.GetInt32(reader.GetOrdinal("user_auth_id")),
                    Bio = reader.GetString(reader.GetOrdinal("bio")),
                    Image = reader.IsDBNull(imageIdOrdinal)
                        ? null
                        : new Image { Id = reader.GetInt32(imageIdOrdinal) },
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    DeletedAt = reader.IsDBNull(deletedAtOrdinal)
                        ? null
                        : reader.GetDateTime(deletedAtOrdinal),
                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                    LastName = reader.GetString(reader.GetOrdinal("last_name")),
                    IsOnboarded = reader.GetBoolean(reader.GetOrdinal("is_onboarded")),
                    Institute = new Institute { Id = reader.GetInt32(reader.GetOrdinal("institute_id")) },
                    Course = new Course { Id = reader.GetInt32(reader.GetOrdinal("course_id")) },
                    Interests = new List<string>()
                };
            }

            const string selectInterestsQuery = """
                SELECT i.name
                FROM user_interests ui
                INNER JOIN interests i ON i.interest_id = ui.interest_id
                WHERE ui.user_id = @user_id;
                """;

            await using MySqlCommand interestsCmd = new MySqlCommand(selectInterestsQuery, connection);
            interestsCmd.Parameters.AddWithValue("@user_id", userId);

            await using var interestReader = await interestsCmd.ExecuteReaderAsync(cancellationToken);
            while (await interestReader.ReadAsync(cancellationToken))
            {
                student.Interests.Add(interestReader.GetString(interestReader.GetOrdinal("name")));
            }

            return student;
        }

        /// <summary>
        /// Retrieves all active (non-deleted) <see cref="Student"/> records along with their
        /// associated <see cref="Student.Interests"/>.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the asynchronous database operation.</param>
        /// <returns>A list of all active <see cref="Student"/> entities.</returns>
        public async Task<List<Student>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            const string selectStudentsQuery = """
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
                WHERE u.deleted_at IS NULL;
                """;

            var studentsById = new Dictionary<int, Student>();

            await using (MySqlCommand cmd = new MySqlCommand(selectStudentsQuery, connection))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                int userIdOrdinal = reader.GetOrdinal("user_id");
                int userAuthIdOrdinal = reader.GetOrdinal("user_auth_id");
                int bioOrdinal = reader.GetOrdinal("bio");
                int imageIdOrdinal = reader.GetOrdinal("image_id");
                int createdAtOrdinal = reader.GetOrdinal("created_at");
                int deletedAtOrdinal = reader.GetOrdinal("deleted_at");
                int firstNameOrdinal = reader.GetOrdinal("first_name");
                int lastNameOrdinal = reader.GetOrdinal("last_name");
                int isOnboardedOrdinal = reader.GetOrdinal("is_onboarded");
                int instituteIdOrdinal = reader.GetOrdinal("institute_id");
                int courseIdOrdinal = reader.GetOrdinal("course_id");

                while (await reader.ReadAsync(cancellationToken))
                {
                    int userId = reader.GetInt32(userIdOrdinal);

                    var student = new Student
                    {
                        Id = userId,
                        UserAuthenticationId = reader.GetInt32(userAuthIdOrdinal),
                        Bio = reader.GetString(bioOrdinal),
                        Image = reader.IsDBNull(imageIdOrdinal)
                            ? null
                            : new Image { Id = reader.GetInt32(imageIdOrdinal) },
                        CreatedAt = reader.GetDateTime(createdAtOrdinal),
                        DeletedAt = reader.IsDBNull(deletedAtOrdinal)
                            ? null
                            : reader.GetDateTime(deletedAtOrdinal),
                        FirstName = reader.GetString(firstNameOrdinal),
                        LastName = reader.GetString(lastNameOrdinal),
                        IsOnboarded = reader.GetBoolean(isOnboardedOrdinal),
                        Institute = new Institute { Id = reader.GetInt32(instituteIdOrdinal) },
                        Course = new Course { Id = reader.GetInt32(courseIdOrdinal) },
                        Interests = new List<string>()
                    };

                    studentsById[userId] = student;
                }
            }

            if (studentsById.Count == 0)
            {
                return new List<Student>();
            }

            const string selectAllInterestsQuery = """
                SELECT
                    ui.user_id,
                    i.name
                FROM user_interests ui
                INNER JOIN interests i ON i.interest_id = ui.interest_id
                INNER JOIN users u ON u.user_id = ui.user_id
                WHERE u.deleted_at IS NULL;
                """;

            await using MySqlCommand interestsCmd = new MySqlCommand(selectAllInterestsQuery, connection);
            await using var interestReader = await interestsCmd.ExecuteReaderAsync(cancellationToken);

            int interestUserIdOrdinal = interestReader.GetOrdinal("user_id");
            int interestNameOrdinal = interestReader.GetOrdinal("name");

            while (await interestReader.ReadAsync(cancellationToken))
            {
                int userId = interestReader.GetInt32(interestUserIdOrdinal);
                if (studentsById.TryGetValue(userId, out Student? student))
                {
                    student.Interests.Add(interestReader.GetString(interestNameOrdinal));
                }
            }

            return studentsById.Values.ToList();
        }

        /// <summary>
        /// Updates an existing active <see cref="Student"/>, its underlying <c>users</c> row, and its
        /// <see cref="Student.Interests"/> in <c>user_interests</c> within a database transaction.
        /// </summary>
        /// <param name="id">The identifier of the student/user to update.</param>
        /// <param name="student">The updated student data.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous database operation.</param>
        /// <returns>
        /// The updated <see cref="Student"/> if found and active; otherwise <see langword="null"/>.
        /// </returns>
        public async Task<Student?> UpdateAsync(int id, Student student, CancellationToken cancellationToken = default)
        {
            MySqlTransaction? transaction = null;

            try
            {
                await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
                transaction = await connection.BeginTransactionAsync(cancellationToken);

                student.Id = id;

                const string updateUserQuery = """
                    UPDATE users
                    SET
                        user_auth_id = @user_auth_id,
                        bio = @bio,
                        image_id = @image_id
                    WHERE user_id = @user_id
                      AND deleted_at IS NULL;
                    """;

                await using MySqlCommand userCmd = new MySqlCommand(updateUserQuery, connection, transaction);
                userCmd.Parameters.AddWithValue("@user_id", id);
                userCmd.Parameters.AddWithValue("@user_auth_id", student.UserAuthenticationId);
                userCmd.Parameters.AddWithValue("@bio", student.Bio ?? string.Empty);
                userCmd.Parameters.AddWithValue("@image_id", (object?)student.Image?.Id ?? DBNull.Value);

                int affectedUsers = await userCmd.ExecuteNonQueryAsync(cancellationToken);
                if (affectedUsers == 0)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    await transaction.DisposeAsync();
                    return null;
                }

                const string updateStudentQuery = """
                    UPDATE students
                    SET
                        first_name = @first_name,
                        last_name = @last_name,
                        is_onboarded = @is_onboarded,
                        institute_id = @institute_id,
                        course_id = @course_id
                    WHERE user_id = @user_id;
                    """;

                await using MySqlCommand studentCmd = new MySqlCommand(updateStudentQuery, connection, transaction);
                studentCmd.Parameters.AddWithValue("@user_id", id);
                studentCmd.Parameters.AddWithValue("@first_name", student.FirstName);
                studentCmd.Parameters.AddWithValue("@last_name", student.LastName);
                studentCmd.Parameters.AddWithValue("@is_onboarded", student.IsOnboarded);
                studentCmd.Parameters.AddWithValue("@institute_id", student.Institute.Id);
                studentCmd.Parameters.AddWithValue("@course_id", student.Course.Id);

                await studentCmd.ExecuteNonQueryAsync(cancellationToken);

                await SyncUserInterestsAsync(connection, transaction, id, student.Interests, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();
                return student;
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    await transaction.DisposeAsync();
                }
                throw;
            }
        }

        /// <summary>
        /// Soft-deletes an active <see cref="Student"/> by setting <c>deleted_at</c> on the underlying
        /// <c>users</c> row and returns the deleted student snapshot.
        /// </summary>
        /// <param name="userId">The identifier of the student/user to soft-delete.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous database operation.</param>
        /// <returns>
        /// The soft-deleted <see cref="Student"/> with <see cref="User.DeletedAt"/> populated if found;
        /// otherwise <see langword="null"/>.
        /// </returns>
        public async Task<Student?> DeleteAsync(int userId, CancellationToken cancellationToken = default)
        {
            Student? student = await ReadByIdAsync(userId, cancellationToken);
            if (student is null)
            {
                return null;
            }

            MySqlTransaction? transaction = null;

            try
            {
                await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
                transaction = await connection.BeginTransactionAsync(cancellationToken);

                const string softDeleteUserQuery = """
                    UPDATE users
                    SET deleted_at = CURRENT_TIMESTAMP
                    WHERE user_id = @user_id
                      AND deleted_at IS NULL;
                    """;

                await using MySqlCommand cmd = new MySqlCommand(softDeleteUserQuery, connection, transaction);
                cmd.Parameters.AddWithValue("@user_id", userId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                if (rowsAffected == 0)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    await transaction.DisposeAsync();
                    return null;
                }

                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();

                student.DeletedAt = DateTime.UtcNow;
                return student;
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    await transaction.DisposeAsync();
                }
                throw;
            }
        }

        /// <summary>
        /// Replaces all <c>user_interests</c> mappings for the specified user with the provided interest names.
        /// </summary>
        /// <param name="connection">An open <see cref="MySqlConnection"/> participating in the transaction.</param>
        /// <param name="transaction">The active <see cref="MySqlTransaction"/> for the operation.</param>
        /// <param name="userId">The identifier of the user whose interests are being synchronized.</param>
        /// <param name="interests">Collection of interest names to assign to the user.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous database operation.</param>
        private static async Task SyncUserInterestsAsync(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int userId,
            IEnumerable<string>? interests,
            CancellationToken cancellationToken)
        {
            const string deleteQuery = "DELETE FROM user_interests WHERE user_id = @user_id;";
            await using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection, transaction))
            {
                deleteCmd.Parameters.AddWithValue("@user_id", userId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            var distinctInterests = (interests ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            const string insertQuery = """
                INSERT IGNORE INTO user_interests (user_id, interest_id)
                SELECT @user_id, interest_id
                FROM interests
                WHERE name = @name;
                """;

            foreach (string interestName in distinctInterests)
            {
                await using MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection, transaction);
                insertCmd.Parameters.AddWithValue("@user_id", userId);
                insertCmd.Parameters.AddWithValue("@name", interestName.Trim());
                await insertCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        // BRUG IKKE SYNKRONE METODER - slettes fra ICRUD når alle repos har skiftet til task
        public Student Delete(int id) => throw new NotImplementedException();
        public List<Student> ReadAll() => throw new NotImplementedException();
        public Student? ReadById(int id) => throw new NotImplementedException();
        public Student Update(int id, Student entity) => throw new NotImplementedException();
        public Student Write(Student entity) => throw new NotImplementedException();
    }
}