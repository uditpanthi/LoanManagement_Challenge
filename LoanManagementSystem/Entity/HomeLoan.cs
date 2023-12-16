using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagementSystem.Entity
{
    public class HomeLoan : Loan
    {
        public string PropertyAddress { get; set; }
        public int PropertyValue { get; set; }

        // Default constructor
        public HomeLoan() { }

        // Parameterized constructor
        public HomeLoan(int loanId, int customer, decimal principalAmount,decimal interestRate, int loanTerm, string loanStatus, string propertyAddress, int propertyValue)
            : base(loanId, customer, principalAmount, interestRate, loanTerm, "HomeLoan", loanStatus)
        {
            PropertyAddress = propertyAddress;
            PropertyValue = propertyValue;
        }


        // Print all information
        public override string ToString()
        {
            return $"{base.ToString()}\nProperty Address: {PropertyAddress}\nProperty Value: {PropertyValue}";
        }
    }
}
