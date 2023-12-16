﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagementSystem.Entity
{
    public class Loan
    {
        public int LoanId { get; set; }
        public int CustomerID { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTerm { get; set; }
        public string LoanType { get; set; }
        public string LoanStatus { get; set; }

        // Default constructor
        public Loan() { }

        // Parameterized constructor
        public Loan(int loanId, int customerid, decimal principalAmount, decimal interestRate, int loanTerm, string loanType, string loanStatus)
        {
            LoanId = loanId;
            CustomerID = customerid;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            LoanTerm = loanTerm;
            LoanType = loanType;
            LoanStatus = loanStatus;
        }


        // Print all information
        public override string ToString()
        {
            return $"Loan ID: {LoanId}\nCustomer: {CustomerID}\nPrincipal Amount: {PrincipalAmount}\nInterest Rate: {InterestRate}\n" +
                   $"Loan Term: {LoanTerm}\nLoan Type: {LoanType}\nLoan Status: {LoanStatus}";
        }
    }
}
