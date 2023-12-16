using LoanManagementSystem.Entity;
using LoanManagementSystem.Exceptions;
using LoanManagementSystem.util;
using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace LoanManagementSystem.DAO
{
    internal class LoanRepositoryImpl : ILoanRepository
    {
        public string connectionString;
        SqlCommand cmd = null;

        public LoanRepositoryImpl()
        {
            connectionString = DBUtil.GetConnectionString();
            cmd = new SqlCommand(connectionString);
        }

        public void ApplyLoan(Loan loan)
        {
            try
            {
                Console.WriteLine("Loan Details:");
                Console.WriteLine($"Customer ID: {loan.CustomerID}");
                Console.WriteLine($"Principal Amount: {loan.PrincipalAmount}");
                Console.WriteLine($"Interest Rate: {loan.InterestRate}");
                Console.WriteLine($"Loan Term: {loan.LoanTerm} months");
                Console.WriteLine($"Loan Type: {loan.LoanType}");
                Console.WriteLine($"Loan Status: Pending");

                // Ask for user confirmation before proceeding
                Console.Write("Do you want to proceed and apply for the loan? (Yes/No): ");
                string userResponse = Console.ReadLine();

                if (userResponse.Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Loan (CustomerId, PrincipalAmount, InterestRate, LoanTerm, LoanType, LoanStatus) VALUES (@CustomerId, @PrincipalAmount, @InterestRate, @LoanTerm, @LoanType, @LoanStatus)", connection))
                        {
                            cmd.Parameters.AddWithValue("@CustomerId", loan.CustomerID);
                            cmd.Parameters.AddWithValue("@PrincipalAmount", loan.PrincipalAmount);
                            cmd.Parameters.AddWithValue("@InterestRate", loan.InterestRate);
                            cmd.Parameters.AddWithValue("@LoanTerm", loan.LoanTerm);
                            cmd.Parameters.AddWithValue("@LoanType", loan.LoanType);
                            cmd.Parameters.AddWithValue("@LoanStatus", "Pending");

                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("Loan application successful! The status is pending.");
                    }
                }
                else
                {
                    Console.WriteLine("Loan application cancelled.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during loan application : Customer Not Found");
            }
        }


        public decimal CalculateInterest(int loanId)
        {
            decimal interest = 0;
            bool loanFound = false;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                string query = "SELECT principalAmount, interestRate, loanTerm FROM Loan WHERE loanID = @LoanID";

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@LoanID", loanId);

                    sqlConnection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        loanFound = true;

                        decimal principalAmount = Convert.ToDecimal(reader["PrincipalAmount"]);
                        decimal interestRate = Convert.ToDecimal(reader["InterestRate"]);
                        int loanTerm = Convert.ToInt32(reader["LoanTerm"]);

                        // Calculate the interest amount
                        interest = (principalAmount * interestRate * loanTerm) / 12;
                    }
                }
            }

            if (!loanFound)
            {
                Console.WriteLine("Loan not found.");
                // Handle loan not found scenario
                // You can set a default interest value, throw an exception, or return an error status based on your requirements
            }

            return interest;
        }


        public decimal CalculateInterest(int loanId, decimal principalAmount, decimal interestRate, int loanTerm)
        {
            decimal interest = 0;
            bool loanFound = false;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                string query = "SELECT LoanID FROM Loan WHERE LoanID = @LoanID";

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@LoanID", loanId);

                    sqlConnection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        loanFound = true;
                        // Calculate the interest amount using the provided parameters
                        interest = (principalAmount * interestRate * loanTerm) / 12;
                    }
                    else
                    {
                        Console.WriteLine("Loan not found.");
                    }
                }
            }

            return interest;
        }




        public string LoanStatus(int loanId)
        {
            string status = "";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT C.CreditScore FROM Loan L JOIN Customers C ON L.CustomerID = C.CustomerID WHERE L.LoanID = @LoanId", sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@LoanId", loanId);

                        sqlConnection.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            int creditScore = Convert.ToInt32(result);

                            if (creditScore > 650)
                            {
                                status = "Approved";
                            }
                            else
                            {
                                status = "Rejected";
                            }
                        }
                        else
                        {
                            status = "Loan not found";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Exception: " + ex.Message);
                status = "Error";
            }

            return status;
        }

        public decimal CalculateEMI(int loanId)
        {
            try
            {
                Loan loan = GetLoanById(loanId);

                if (loan == null)
                {
                    throw new InvalidLoanException("Loan not found");
                }

                return CalculateEMI(loan.LoanId, loan.PrincipalAmount, loan.InterestRate, loan.LoanTerm);
            }
            catch (InvalidLoanException ex)
            {
                Console.WriteLine($"Error during EMI calculation: {ex.Message}");
                return 0;
            }
        }


        public decimal CalculateEMI(int loanId, decimal principalAmount, decimal interestRate, int loanTerm)
        {
            try
            {
                decimal monthlyInterestRate = interestRate / 12 / 100;

                // Calculate EMI using the formula: [P * R * (1+R)^N] / [(1+R)^N-1]
                decimal emi = principalAmount * monthlyInterestRate *
                              (decimal)(Math.Pow(1 + (double)monthlyInterestRate, loanTerm)) /
                              (decimal)(Math.Pow(1 + (double)monthlyInterestRate, loanTerm) - 1);

                return emi;
            }
            catch (ArithmeticException ex)
            {
                Console.WriteLine($"Error during EMI calculation: {ex.Message}");
                throw;
            }
        }




        public int LoanRepayment(int loanId, decimal amount)
        {
            int noOfEmiPaid = 0;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                string query = "SELECT PrincipalAmount, InterestRate, LoanTerm FROM Loan WHERE LoanID = @LoanID";

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@LoanID", loanId);

                    sqlConnection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        decimal principalAmount = Convert.ToDecimal(reader["PrincipalAmount"]);
                        decimal interestRate = Convert.ToDecimal(reader["InterestRate"]);
                        int loanTerm = Convert.ToInt32(reader["LoanTerm"]);

                        decimal emi = CalculateEMI(loanId, principalAmount, interestRate, loanTerm);

                        if (amount >= emi)
                        {
                            noOfEmiPaid = (int)(amount / emi);
                            Console.WriteLine($"Paid {noOfEmiPaid} EMI(s) from the given amount.");
                        }
                        else
                        {
                            Console.WriteLine("Payment amount is less than the EMI. Payment rejected.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Loan not found.");
                    }
                }
            }

            return noOfEmiPaid;
        }

        public List<Loan> GetAllLoans()
        {
            List<Loan> loans = RetrieveAllLoansFromDatabase();

            // Print the details of each loan
            foreach (Loan loan in loans)
            {
                PrintLoanDetails(loan);
            }

            return loans;
        }


















        private void UpdateLoanStatus(int loanId, string status)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                string updateQuery = "UPDATE Loan SET LoanStatus = @Status WHERE LoanID = @LoanID";

                using (SqlCommand updateCmd = new SqlCommand(updateQuery, sqlConnection))
                {
                    updateCmd.Parameters.AddWithValue("@Status", status);
                    updateCmd.Parameters.AddWithValue("@LoanID", loanId);

                    sqlConnection.Open();
                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Loan status updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to update loan status.");
                    }
                }
            }
        }

        private int CalculateNoOfEMIToPay(double amount, decimal principalAmount, decimal interestRate, int loanTerm)
        {
            decimal monthlyInterestRate = interestRate / 12 / 100;

            // Calculate EMI using the formula: [P * R * (1+R)^N] / [(1+R)^N-1]
            double emi = (double)((double)principalAmount * (double)monthlyInterestRate * Math.Pow(1 + (double)monthlyInterestRate, loanTerm))
                / (Math.Pow(1 + (double)monthlyInterestRate, loanTerm) - 1);

            // Calculate the number of EMIs that can be paid from the given amount
            int noOfEmiToPay = (int)Math.Floor(amount / emi);

            return noOfEmiToPay;
        }


        public Loan GetLoanById(int loanId)
        {
            Loan loan = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Loan WHERE LoanId = @LoanId", connection))
                {
                    cmd.Parameters.AddWithValue("@LoanId", loanId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            loan = new Loan
                            {
                                LoanId = (int)reader["LoanId"],
                                PrincipalAmount = (decimal)reader["PrincipalAmount"],
                                InterestRate = (decimal)reader["InterestRate"],
                                LoanTerm = (int)reader["LoanTerm"],
                                // Populate other properties based on your table structure
                            };
                        }
                    }
                }
            }

            return loan;
        }

        private List<Loan> RetrieveAllLoansFromDatabase()
        {
            List<Loan> loans = new List<Loan>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Loan", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Loan loan = new Loan
                                {
                                    LoanId = (int)reader["LoanId"],
                                    PrincipalAmount = (decimal)reader["PrincipalAmount"],
                                    InterestRate = (decimal)reader["InterestRate"],
                                    LoanTerm = (int)reader["LoanTerm"],
                                    // Populate other properties based on your table structure
                                };

                                loans.Add(loan);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"An error occurred while retrieving loans: {ex.Message}");
                // You might want to throw the exception again or return an empty list, depending on your use case.
                throw;
            }

            return loans;
        }


        public void PrintLoanDetails(Loan loan)
        {
            // Print loan details to the console
            Console.WriteLine($"Loan ID: {loan.LoanId}");
            Console.WriteLine($"Principal Amount: {loan.PrincipalAmount}");
            Console.WriteLine($"Interest Rate: {loan.InterestRate}");
            Console.WriteLine($"Loan Term: {loan.LoanTerm}");
            Console.WriteLine("---------------------------------------");
        }
    }
}

/*public decimal CalculateInterest(int loanId)
{
    Loan loan = GetLoanById(loanId);

    if (loan == null)
    {
        throw new InvalidLoanException("Loan not found");
    }

    decimal interest = CalculateInterest(loan.LoanId,loan.PrincipalAmount,loan.InterestRate,loan.LoanTerm);
    return interest;
}
public decimal CalculateInterest(int loanId, decimal principalAmount, decimal interestRate, int loanTerm)
{
    // Convert annual interest rate to monthly rate
    decimal monthlyInterestRate = interestRate / 12 / 100;

    // Calculate interest using the formula: (Principal Amount * Monthly Interest Rate * Loan Term)
    decimal interest = principalAmount * monthlyInterestRate * loanTerm;

    return interest;
}*/

/*public void LoanRepayment(int loanId, double amount)
        {
            try
            {
                Loan loan = GetLoanById(loanId);

                if (loan == null)
                {
                    throw new InvalidLoanException("Loan not found");
                }

                // Calculate the number of EMIs that can be paid from the given amount
                int noOfEmiToPay = CalculateNoOfEMIToPay(amount, loan.PrincipalAmount, loan.InterestRate, loan.LoanTerm);

                // If the amount is less than a single EMI, reject the payment
                decimal emiAmount = CalculateEMI(loan.LoanId);

                if ((decimal)amount < emiAmount)
                {
                    Console.WriteLine("Payment rejected. Amount is less than a single EMI.");
                    return;
                }

                // Update the loan details with the paid EMIs
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update the noOfEmiPaid column or any other relevant columns based on your schema
                    using (SqlCommand cmd = new SqlCommand("UPDATE Loan SET NoOfEmiPaid = @NoOfEmiPaid WHERE LoanId = @LoanId", connection))
                    {
                        cmd.Parameters.AddWithValue("@LoanId", loanId);
                        cmd.Parameters.AddWithValue("@NoOfEmiPaid", noOfEmiToPay);

                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Payment successful. {noOfEmiToPay} EMIs paid from the amount.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during loan repayment: {ex.Message}");
                // You might want to log the exception or handle it in a way that suits your application.
                // You can also throw the exception again if necessary.
            }
        }*/


/*public void LoanStatus(int loanId)
{
    Loan loan = GetLoanById(loanId);

    if (loan == null)
    {
        throw new InvalidLoanException("Loan not found");
    }

    // Determine loan approval or rejection based on credit score
    string statusMessage = (loan.Customer.CreditScore > 650) ? "Loan Approved" : "Loan Rejected";

    // Update loan status in the database
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // Assuming you have a LoanStatus column in your LoanTable
        using (SqlCommand cmd = new SqlCommand("UPDATE Loan SET LoanStatus = @Status WHERE LoanId = @LoanId", connection))
        {
            cmd.Parameters.AddWithValue("@LoanId", loanId);
            cmd.Parameters.AddWithValue("@Status", statusMessage);

            cmd.ExecuteNonQuery();
        }
    }

    // Display the status message
    Console.WriteLine($"Loan ID: {loanId} - {statusMessage}");
}*/

/*private decimal CalculateEMI(decimal principalAmount, decimal interestRate, int loanTerm)
{
    decimal emi = 0;

    decimal monthlyInterestRate = interestRate / 12 / 100;
    decimal numerator = principalAmount * monthlyInterestRate * (decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTerm);
    decimal denominator = (decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTerm) - 1;

    emi = numerator / denominator;

    return emi;
}*/