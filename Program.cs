using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Data.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ai2
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Define the relative path to appsettings.json from the base directory
            string relativePath = @"..\..\..\appsettings.json";  // Assuming appsettings.json is in the project root

            // Get the full path to appsettings.json by combining the base directory with the relative path
            string fullPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));

            // Build the configuration from appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)  // Optional: You can still use the base directory
                .AddJsonFile(fullPath, optional: false, reloadOnChange: true); // Load the appsettings.json using fullPath

            var configuration = builder.Build();

            DropTableIfExists("Wheights2", configuration);

            CreateWheights(configuration);

            WritingWheights(configuration);

            //SelectWheights( configuration);

            //DropTableIfExists("Wheights2", configuration);
            // Wait for user input before closing

            Console.WriteLine("Writing finish");
            Console.ReadLine();
        }
        static void SelectWheights(IConfigurationRoot configurationRoot)
        {

            // Get the connection string from appsettings.json
            string connectionString = configurationRoot.GetConnectionString("DefaultConnection");

            // Query to retrieve data from the 'Wheights' table
            string query = "SELECT Id, Weights FROM Wheights2";

            // Using SqlConnection to connect to the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();
                    Console.WriteLine("Connection established successfully!");

                    // Create a SqlCommand to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    // Execute the query and read the data
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Check if there is data
                        if (reader.HasRows)
                        {
                            Console.WriteLine("Id\tWeights");
                            while (reader.Read())
                            {
                                // Access the values of the columns
                                int id = reader.GetInt32(0);  // The Id column
                                var  weights = reader.GetDecimal(1);  // The Weights column
                                
                                // Output the data
                                Console.WriteLine($"{id}\t{weights}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No data found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If there's an error, print it to the console
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void CreateWheights(IConfiguration configurationRoot)
        {

            // Get the connection string from appsettings.json
            var builder = configurationRoot.GetConnectionString("DefaultConnection");

            // Query to retrieve data from the 'Wheights' table
            string query = @"CREATE TABLE[dbo].[Wheights2](
            Id INT PRIMARY KEY IDENTITY(1, 1), --Auto - incrementing Id
            Weights DECIMAL(10, 8) NOT NULL-- Weights column, explicitly defined as FLOAT
            )";



            // Using SqlConnection to connect to the database
            using (SqlConnection connection = new SqlConnection(builder))
            {
                try
                {
                    // Open the connection
                    connection.Open();
                    Console.WriteLine("Connection established successfully!");

                    // Create a SqlCommand to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    command.ExecuteNonQuery();
                    //command.BeginExecuteNonQuery();

                    connection.Close();
                }
                catch (Exception ex)
                {
                    // If there's an error, print it to the console
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

        }
        static int Wlayer = 1000;
        static int WLayerLigne = 10;
        static int WLayerColone= 5;

        public static void WritingWheights(IConfiguration configurationRoot)
        {
            // Retrieve the connection string from the configuration
            string connectionString = configurationRoot.GetConnectionString("DefaultConnection");

            
            
            double[][][] MatrixWheights = new double[Wlayer][][];

            Random random = new Random();


            for (int i = 0; i < Wlayer; i++)
            {
                MatrixWheights[i] = new double[WLayerLigne][];
                for (int j = 0; j < WLayerLigne; j++)
                {
                    MatrixWheights[i][j] = new double[WLayerColone];
                    for (int k = 0; k < WLayerColone; k++)
                    {
                        MatrixWheights[i][j][k] = new double();
                        MatrixWheights[i][j][k] = random.NextDouble() + 0.1>1.0? random.NextDouble() : random.NextDouble() + 0.1;

                    }
                }
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connection to the database established successfully!");

                for (int i = 0; i < Wlayer; i++)
                {
                    for (int j = 0; j < WLayerLigne; j++)
                    {
                        for (int k = 0; k < WLayerColone; k++)
                        {
                            try
                            {
                                string parameterName = "MatrixWeight_" + i + "_" + j + "_" + k;

                                string query = "INSERT INTO [dbo].[Wheights2] (Weights) VALUES (@"+ parameterName + ");";

                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue(parameterName, MatrixWheights[i][j][k]);
                                     int rowsAffected = command.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Handle any errors that might occur
                                Console.WriteLine($"An error occurred: {ex.Message}");
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        public static  void DropTableIfExists(string tableName, IConfiguration configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("DefaultConnection");

            // Ensure that table name is safely enclosed to prevent SQL injection
            string query = $"DROP TABLE IF EXISTS [{tableName}]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
