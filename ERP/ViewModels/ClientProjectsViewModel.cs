using ERP.Models;
using System;
using System.Collections.Generic;

namespace ERP.ViewModels
{
    public class ClientProjectsViewModel
    {
        public int ClientId { get; set; }
        public string ClientTitle { get; set; } = null!;
        public string? City { get; set; }
        public DateOnly? FirstRequestDate { get; set; }  
        public List<ClientProjectViewModel> Projects { get; set; } = new List<ClientProjectViewModel>();
        public List<ClientRequisiteViewModel> Requisites { get; set; } = new List<ClientRequisiteViewModel>();
        public List<ClientDeliveryAddressViewModel> DeliveryAddresses { get; set; } = new List<ClientDeliveryAddressViewModel>();
        public List<ClientContactViewModel> Contacts { get; set; } = new List<ClientContactViewModel>();
        public List<YandexAccountViewModel> Accounts { get; set; } = new List<YandexAccountViewModel>();

    }

    public class YandexAccountViewModel
    {
        public int AccountId { get; set; }
        public bool IsCurrent { get; set; }
        public string Email { get; set; } = null!;
    }

    public class ClientProjectViewModel
    {
        public int ProjectId { get; set; }
        public bool? IsArchived { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public DateOnly? EventDate { get; set; }
        public DateOnly? Deadline { get; set; }
        public decimal? PaymentTotal { get; set; }
        public decimal? AdvanceRate { get; set; }
        public string? ImagePath { get; set; }
    }

    public class ClientRequisiteViewModel
    {
        public int RequisiteId { get; set; }
        public string FileTitle { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime? UploadedAt { get; set; }
    }

    public class ClientDeliveryAddressViewModel
    {
        public int AddressId { get; set; }
        public string DeliveryAddress { get; set; } = null!;
    }

    public class ClientContactViewModel
    {
        public int ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Passport { get; set; }
    }
}
