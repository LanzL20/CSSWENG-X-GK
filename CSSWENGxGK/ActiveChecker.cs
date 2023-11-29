using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using System.Data.SqlClient;

public class ActiveChecker
{
    Emailer emailer = new Emailer();
    string connectionString = "Server=localhost\\SQLEXPRESS;Database=cssweng;Trusted_Connection=True;TrustServerCertificate=True;";

    public void PerformDatabaseCheck()
    {
        Console.WriteLine("Checking db");
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<int> volunteerIDs = new List<int>();
                List<string> closeExpireEmails = new List<string>();

                string selectQuery = "SELECT VolunteerID FROM T_Volunteer WHERE LastUpdateDate < @CutoffDate";

                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    
                    // 6 months
                    selectCommand.Parameters.AddWithValue("@CutoffDate", DateTime.Now.AddMonths(-6));
                    // 5 mins for now
                    // selectCommand.Parameters.AddWithValue("@CutoffDate", DateTime.Now.AddMinutes(-5));

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int volunteerID = reader.GetInt32(0);
                            volunteerIDs.Add(volunteerID);
                        }
                    }
                }

                string selectCloseExpireQuery = "SELECT Email FROM T_Volunteer WHERE LastUpdateDate >= @StartDate AND LastUpdateDate < @EndDate";
                
                using (SqlCommand selectCommand = new SqlCommand(selectCloseExpireQuery, connection))
                {
                    // Set the date range to a month before the current date
                    DateTime startDate = DateTime.Now.AddMonths(-1).Date;  // Midnight of a month before the current date
                    DateTime endDate = DateTime.Now.Date;  // Midnight of the current date
                
                    selectCommand.Parameters.AddWithValue("@StartDate", startDate);
                    selectCommand.Parameters.AddWithValue("@EndDate", endDate);
                
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string email = reader.GetString(0);
                            closeExpireEmails.Add(email);
                        }
                    }
                }

                emailer.Send_Near_Expire(closeExpireEmails);

                // Update isActive status to false
                string updateQuery = "UPDATE T_Volunteer SET IsActive = 0 WHERE VolunteerID = @VolunteerID";

                foreach (int volunteerID in volunteerIDs)
                {
                    Console.WriteLine(volunteerID);
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
