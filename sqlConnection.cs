using System;
using System.Data.SqlClient;

namespace CSSWENGxGK
{
    public class DatabaseConnection
    {
        private const string ConnectionString = "Server=Server;Database=Database;User Id=Username;Password=Password;";

        public SqlConnection Connect()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                Console.WriteLine("Database connected successfully!");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to the database: " + ex.Message);
                return null;
            }
        }

        public void Disconnect(SqlConnection connection)
        {
            try
            {
                connection.Close();
                Console.WriteLine("Database disconnected successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error disconnecting to the database: " + ex.Message);
            }
        }

        public bool CreateNewVolunteer(string firstName, string lastName, string email, string mobileNumber, string password)
        {
            using (SqlConnection connection = Connect())
            {
                if (connection == null)
                {
                    return false;
                }

                string insertQuery = "INSERT INTO T_Volunteer (FirstName, LastName, Email, MobileNumber, [Password]) " +
                                    "VALUES (@FirstName, @LastName, @Email, @MobileNumber, @Password)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@MobileNumber", mobileNumber);
                    command.Parameters.AddWithValue("@Password", password);

                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Volunteer data inserted successfully!");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error inserting volunteer data: " + ex.Message);
                        return false;
                    }
                }
            }
        }
        
        public bool UpdateVolunteerInfo(string firstName, string lastName, string email, string mobileNumber, DateTime birthDate, string gender, string provCode, string townCode, string brgyCode, DateTime yearStarted)
        {
            using (SqlConnection connection = Connect())
            {
                if (connection == null)
                {
                    return false;
                }

                string insertQuery = "INSERT INTO T_Volunteer (FirstName, LastName, Email, MobileNumber, BirthDate, Gender, PROV_CODE, TOWN_CODE, BRGY_CODE, YearStarted) " +
                                    "VALUES (@FirstName, @LastName, @Email, @MobileNumber, @BirthDate, @Gender, @PROV_CODE, @TOWN_CODE, @BRGY_CODE, @YearStarted)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@MobileNumber", mobileNumber);
                    command.Parameters.AddWithValue("@BirthDate", birthDate);
                    command.Parameters.AddWithValue("@Gender", gender);
                    command.Parameters.AddWithValue("@PROV_CODE", provCode);
                    command.Parameters.AddWithValue("@TOWN_CODE", townCode);
                    command.Parameters.AddWithValue("@BRGY_CODE", brgyCode);
                    command.Parameters.AddWithValue("@YearStarted", yearStarted);

                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Volunteer data inserted successfully!");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error inserting volunteer data: " + ex.Message);
                        return false;
                    }
                }
            }
        }
        
    }
}
