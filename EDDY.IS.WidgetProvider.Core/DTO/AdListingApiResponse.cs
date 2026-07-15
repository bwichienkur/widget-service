using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.DTO
{
    public class AdListingApiResponse
    {
        public string Error { get; set; }
        public string Status { get; set; }
        public List<Institution> Institutions { get; set; }
        public object Message { get; set; }
        public string TransactionId { get; set; }
    }
    public class Address
    {
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class Program
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Institution
    {
        public int? Position { get; set; }
        public string InstitutionName { get; set; }
        public string ShortDescription { get; set; }
        public string LogoLargeImage { get; set; }
        public string LogoSmallImage { get; set; }
        public string LogoMediumImage { get; set; }
        public string CampusType { get; set; }
        public string CampusName { get; set; }
        public Address Address { get; set; }
        public string ClickThroughUrl { get; set; }
        public List<Program> Programs { get; set; }
        public decimal? Comission { get; set; }
        public string Description { get; set; }
        public bool IsLeadUrl { get; set; }
    }
}
