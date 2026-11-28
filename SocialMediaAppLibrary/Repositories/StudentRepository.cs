using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System.Data.Common;

namespace SocialMediaAppLibrary.Repositories
{
    /// <summary>
    /// Repository responsible for persisting, querying, updating, and soft-deleting
    /// <see cref="Student"/> entities and their associated <c>users</c>, <c>images</c>,
    /// <c>institutes</c>, <c>cities</c>, <c>courses</c>, <c>fields_of_study</c>,
    /// and <c>user_interests</c> records in MySQL.
    /// </summary>
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnect _db;

        private const string StudentSelectColumns = """
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
            s.course_id,

            img.image_id AS img_id,
            img.file_name,
            img.alt_text,
            -- img.image_data,                               -- Commented out as Images PageModel will handle image data retrieval and caching
            img.height,
            img.width,
            img.file_size,
            img.created_at AS img_created_at,

            inst.institute_id AS inst_id,
            inst.name AS institute_name,
            inst.domain_name AS institute_domain_name,
            inst.city_id AS inst_city_id,

            c.city_id AS city_id,
            c.name AS city_name,
            c.postal_code AS city_postal_code,

            co.course_id AS co_id,
            co.name AS course_name,
            co.field_of_study_id AS co_field_of_study_id,

            fos.field_of_study_id AS fos_id,
            fos.name AS field_of_study_name
            """;

        private const string StudentJoins = """
            FROM users u
            INNER JOIN students s ON s.user_id = u.user_id
            LEFT JOIN images img ON img.image_id = u.image_id
            LEFT JOIN institutes inst ON inst.institute_id = s.institute_id
            LEFT JOIN cities c ON c.city_id = inst.city_id
            LEFT JOIN courses co ON co.course_id = s.course_id
            LEFT JOIN fields_of_study fos ON fos.field_of_study_id = co.field_of_study_id
            """;

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
        /// including its full <see cref="Image"/>, <see cref="Institute"/>, <see cref="City"/>,
        /// <see cref="Course"/>, and <see cref="Student.Interests"/> list.
        /// </summary>
        public async Task<Student?> ReadByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            string selectStudentQuery = $"""
                SELECT
                    {StudentSelectColumns}
                {StudentJoins}
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

                student = MapStudent(reader);
            }

            const string selectInterestsQuery = """
                SELECT i.name
                FROM user_interests ui
                INNER JOIN interests i ON i.interest_id = ui.interest_id
                WHERE ui.user_id = @user_id;
                """;

            await using MySqlCommand interestsCmd = new MySqlCommand(selectInterestsQuery, connection);
            interestsCmd.Parameters.AddWithValue("@user_id", userId);

            await using DbDataReader interestReader = await interestsCmd.ExecuteReaderAsync(cancellationToken);
            int nameOrdinal = interestReader.GetOrdinal("name");
            while (await interestReader.ReadAsync(cancellationToken))
            {
                if (!interestReader.IsDBNull(nameOrdinal))
                {
                    student.Interests.Add(interestReader.GetString(nameOrdinal));
                }
            }

            return student;
        }

        /// <summary>
        /// Retrieves all active (non-deleted) <see cref="Student"/> records along with their
        /// full <see cref="Image"/>, <see cref="Institute"/>, <see cref="City"/>, <see cref="Course"/>,
        /// and associated <see cref="Student.Interests"/>.
        /// </summary>
        public async Task<List<Student>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            string selectStudentsQuery = $"""
                SELECT
                    {StudentSelectColumns}
                {StudentJoins}
                WHERE u.deleted_at IS NULL;
                """;

            var studentsById = new Dictionary<int, Student>();

            await using (MySqlCommand cmd = new MySqlCommand(selectStudentsQuery, connection))
            await using (DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    Student student = MapStudent(reader);
                    studentsById[student.Id] = student;
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
            await using DbDataReader interestReader = await interestsCmd.ExecuteReaderAsync(cancellationToken);

            int interestUserIdOrdinal = interestReader.GetOrdinal("user_id");
            int interestNameOrdinal = interestReader.GetOrdinal("name");

            while (await interestReader.ReadAsync(cancellationToken))
            {
                int userId = interestReader.GetInt32(interestUserIdOrdinal);
                if (!interestReader.IsDBNull(interestNameOrdinal) &&
                    studentsById.TryGetValue(userId, out Student? student))
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

                // Hent den fuldt opdaterede student inkl. joined institute, city, course og interests
                return await ReadByIdAsync(id, cancellationToken);
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
        /// Maps a joined <c>users</c>, <c>students</c>, <c>images</c>, <c>institutes</c>,
        /// <c>cities</c>, <c>courses</c>, and <c>fields_of_study</c> row into a <see cref="Student"/> instance.
        /// </summary>
        private static Student MapStudent(DbDataReader reader)
        {
            int imageIdOrdinal = reader.GetOrdinal("image_id");
            int imgIdOrdinal = reader.GetOrdinal("img_id");
            int instIdOrdinal = reader.GetOrdinal("inst_id");
            int cityIdOrdinal = reader.GetOrdinal("city_id");
            int courseIdOrdinal = reader.GetOrdinal("co_id");
            int fosIdOrdinal = reader.GetOrdinal("fos_id");
            int bioOrdinal = reader.GetOrdinal("bio");
            int deletedAtOrdinal = reader.GetOrdinal("deleted_at");

            Image? image = null;
            if (!reader.IsDBNull(imageIdOrdinal) && !reader.IsDBNull(imgIdOrdinal))
            {
                /*int imageDataOrdinal = reader.GetOrdinal("image_data"); // Commented out as Images PageModel will handle image data retrieval and caching
                byte[] imageData = reader.IsDBNull(imageDataOrdinal)
                    ? []
                    : (byte[])reader[imageDataOrdinal];*/

                int fileNameOrdinal = reader.GetOrdinal("file_name");
                int altTextOrdinal = reader.GetOrdinal("alt_text");
                int heightOrdinal = reader.GetOrdinal("height");
                int widthOrdinal = reader.GetOrdinal("width");
                int fileSizeOrdinal = reader.GetOrdinal("file_size");
                int imgCreatedAtOrdinal = reader.GetOrdinal("img_created_at");

                image = new Image
                {
                    Id = reader.GetInt32(imgIdOrdinal),
                    FileName = reader.IsDBNull(fileNameOrdinal) ? string.Empty : reader.GetString(fileNameOrdinal),
                    AltText = reader.IsDBNull(altTextOrdinal) ? string.Empty : reader.GetString(altTextOrdinal),
                    // ImageData = imageData, // Commented out as Images PageModel will handle image data retrieval and caching
                    Height = reader.IsDBNull(heightOrdinal) ? 0 : reader.GetInt32(heightOrdinal),
                    Width = reader.IsDBNull(widthOrdinal) ? 0 : reader.GetInt32(widthOrdinal),
                    FileSize = reader.IsDBNull(fileSizeOrdinal) ? 0 : reader.GetInt32(fileSizeOrdinal),
                    CreatedAt = reader.IsDBNull(imgCreatedAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(imgCreatedAtOrdinal)
                };
            }

            City? city = null;
            if (!reader.IsDBNull(cityIdOrdinal))
            {
                int postalCodeOrdinal = reader.GetOrdinal("city_postal_code");
                city = new City
                {
                    Id = reader.GetInt32(cityIdOrdinal),
                    Name = reader.GetString(reader.GetOrdinal("city_name")),
                    PostalCode = reader.IsDBNull(postalCodeOrdinal)
                        ? string.Empty
                        : reader.GetValue(postalCodeOrdinal).ToString() ?? string.Empty
                };
            }

            Institute? institute = null;
            if (!reader.IsDBNull(instIdOrdinal))
            {
                int domainOrdinal = reader.GetOrdinal("institute_domain_name");
                institute = new Institute
                {
                    Id = reader.GetInt32(instIdOrdinal),
                    Name = reader.GetString(reader.GetOrdinal("institute_name")),
                    DomainName = reader.IsDBNull(domainOrdinal) ? null : reader.GetString(domainOrdinal),
                    City = city
                };
            }

            Course? course = null;
            if (!reader.IsDBNull(courseIdOrdinal))
            {
                int fosNameOrdinal = reader.GetOrdinal("field_of_study_name");
                course = new Course
                {
                    Id = reader.GetInt32(courseIdOrdinal),
                    Name = reader.GetString(reader.GetOrdinal("course_name")),
                    FieldOfStudy = reader.IsDBNull(fosIdOrdinal) || reader.IsDBNull(fosNameOrdinal)
                        ? null
                        : reader.GetString(fosNameOrdinal)
                };
            }

            return new Student
            {
                Id = reader.GetInt32(reader.GetOrdinal("user_id")),
                UserAuthenticationId = reader.GetInt32(reader.GetOrdinal("user_auth_id")),
                Bio = reader.IsDBNull(bioOrdinal) ? null : reader.GetString(bioOrdinal),
                Image = image,
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                DeletedAt = reader.IsDBNull(deletedAtOrdinal)
                    ? null
                    : reader.GetDateTime(deletedAtOrdinal),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                IsOnboarded = reader.GetBoolean(reader.GetOrdinal("is_onboarded")),
                Institute = institute,
                Course = course,
                Interests = new List<string>()
            };
        }

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