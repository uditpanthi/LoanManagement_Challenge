using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagementSystem.Entity
{
    public class CarLoan : Loan
    {
        public string CarModel { get; set; }
        public int CarValue { get; set; }

        // Default constructor
        public CarLoan() { }

        // Parameterized constructor
        public CarLoan(int loanId, int customer, decimal principalAmount, decimal interestRate, int loanTerm, string loanStatus, string carModel, int carValue)
            : base(loanId, customer, principalAmount, interestRate, loanTerm, "CarLoan", loanStatus)
        {
            CarModel = carModel;
            CarValue = carValue;
        }


        // Print all information
        public override string ToString()
        {
            return $"{base.ToString()}\nCar Model: {CarModel}\nCar Value: {CarValue}";
        }
    }
}
