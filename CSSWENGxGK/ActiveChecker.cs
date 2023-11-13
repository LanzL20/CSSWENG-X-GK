using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using System.Data.SqlClient;

public class ActiveChecker
{
    string connectionString = "Server=localhost\\SQLEXPRESS;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

    public void PerformDatabaseCheck()
    {

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<int> volunteerIDs = new List<int>();

                string selectQuery = "SELECT VolunteerID FROM T_Volunteer WHERE LastUpdateDate < @CutoffDate";

                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@CutoffDate", DateTime.Now.AddMonths(-6));

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int volunteerID = reader.GetInt32(0);
                            volunteerIDs.Add(volunteerID);
                        }
                    }
                }

                // Update isActive status to false
                string updateQuery = "UPDATE T_Volunteer SET IsActive = 0 WHERE VolunteerID = @VolunteerID";

                foreach (int volunteerID in volunteerIDs)
                {
                    Console.Write(volunteerID);
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@VolunteerID", volunteerID);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

}
