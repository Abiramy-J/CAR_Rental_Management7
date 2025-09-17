﻿// ViewModels/CreateUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Car_Rental_Management.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; }   // Admin / Staff
    }
}
